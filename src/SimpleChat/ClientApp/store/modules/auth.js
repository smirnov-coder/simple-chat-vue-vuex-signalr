import { readToken, storeToken } from "@/scripts/utils";
import { MutationTypes, ActionTypes, GetterTypes } from "@/store/constants";

const state = {
    isAuthenticated: false,
    error: null,
    confirmData: null
};

const mutations = {
    // Устанавливает состояние модуля.
    [MutationTypes.SET_IS_AUTHENTICATED]: (state, isAuthenticated) => state.isAuthenticated = isAuthenticated,

    [MutationTypes.SET_CONFIRM_DATA]: (state, { sessionId, email, provider }) => {
        state.confirmData = {
            sessionId,
            email,
            provider
        };
    },

    [MutationTypes.CLEAR_AUTH_STATE]: (state) => {
        state.isAuthenticated = false;
        state.error = null;
        state.confirmData = null;
    },

    [MutationTypes.SET_ERROR]: (state, error) => state.error = error,
};

const actions = {
    // Производит авторизацию пользователя на сервере.
    [ActionTypes.AUTHENTICATE]: ({ commit, dispatch }) => {
        let accessToken = readToken();
        if (!accessToken) {
            commit(MutationTypes.CLEAR_AUTH_STATE);
            return;
        }
        let options = {
            method: "GET",
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        };
        fetch("/api/auth/check", options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error(`Получен ответ с ошибкой ${response.status} (${response.statusText}).`);
            })
            .then(result => {
                if ("type" in result) {
                    if (result.type === "auth_check") {
                        commit(MutationTypes.SET_IS_AUTHENTICATED, result.isAuthenticated);
                        dispatch(ActionTypes.CURRENT_USER, {
                            ...result.user,
                            connectionIds: []
                        });
                    } else if (result.type === "error") {
                        /// TODO: ???????????????????????????
                        dispatch(ActionTypes.SIGN_OUT);
                    } else {
                        throw new Error("Неизвестный тип результата операции.");
                    }
                } else {
                    throw new Error("Неизвестная структура результата операции.");
                }
            })
            .catch(error => console.log("[SimpleChat] Не удалось авторизовать пользователя на сервере.", error));
    },

    // Выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => commit(MutationTypes.CLEAR_AUTH_STATE),

    [ActionTypes.SIGN_IN_RESULT]: ({ commit, dispatch }, result) => {
        if ("type" in result) {
            switch (result.type) {
                case "success": {
                    storeToken(result.accessToken);
                    dispatch(ActionTypes.AUTHENTICATE);
                    break;
                };

                case "confirm_sign_in": {
                    commit(MutationTypes.SET_CONFIRM_DATA, result);
                    break;
                };

                case "email_required": {
                    commit(MutationTypes.SET_ERROR, {
                        message: result.message
                    });
                };

                case "error": {
                    commit(MutationTypes.SET_ERROR, {
                        message: result.message,
                        errors: result.errors
                    });
                };

                default: {

                }
            }
        }
    },

    [ActionTypes.ERROR_HANDLED]: ({ commit }) => commit(MutationTypes.CLEAR_AUTH_STATE),

    [ActionTypes.SIGN_IN_CANCELED]: ({ commit }) => commit(MutationTypes.CLEAR_AUTH_STATE),

    [ActionTypes.SIGN_IN_CONFIRMED]: ({ commit, dispatch }, { code, sessionId }) => {
        let options = {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: `code=${code}&sessionId=${sessionId}`
        };
        fetch("/api/auth/sign-in/confirm", options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error(`Получен ответ с ошибкой ${response.status} (${response.statusText}).`);
            })
            .then(result => {
                if ("type" in result) {
                    if (result.type === "success") {
                        storeToken(result.accessToken);
                        commit(MutationTypes.CLEAR_AUTH_STATE);
                        dispatch(ActionTypes.AUTHENTICATE);
                    } else if (result.type === "error") {
                        commit(MutationTypes.CLEAR_AUTH_STATE);
                        commit(MutationTypes.SET_ERROR, {
                            message: result.message,
                            errors: result.errors
                        });
                    } else {
                        throw new Error("Неизвестный тип результата операции.");
                    }
                } else {
                    throw new Error("Неизвестная структура результата операции.");
                }
            })
            .catch(error => console.log("[SimpleChat] Не удалось подтвердить вход на сайт через внешнего провайдера.", error));
    },
};

const getters = {
    // Показывает, что пользователь осуществил вход в приложение.
    [GetterTypes.IS_AUTHENTICATED]: state => state.isAuthenticated,

    [GetterTypes.ERROR]: state => state.error,

    [GetterTypes.CONFIRM_DATA]: state => state.confirmData,
};

export default {
    state,
    mutations,
    actions,
    getters
};
