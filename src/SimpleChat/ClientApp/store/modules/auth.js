import { AuthApiClient } from "@/scripts/api/auth-api-client";
import { OAuth2Client, OAuth2ClientOptionsBuilder } from "@/scripts/api/oauth2-client";
import { ActionTypes, GetterTypes, MutationTypes } from "@/scripts/constants";
import { ACCESS_TOKEN_STORAGE_KEY, clearToken, readToken, storeToken } from "@/scripts/utils";

//
// State
//
const state = {
    // Показывает, что пользователь аутентифицирован на сервере.
    isAuthenticated: false,

    // Ошибка работы с AuthСontroller.
    error: null,

    // Данные, необходимые для подтверждения первого входа на сайт через внешний OAuth2-провайдер.
    confirmationData: null
};

//
// Mutations
//
const mutations = {
    // Устанавливает значение флага аутентификации.
    [MutationTypes.SET_IS_AUTHENTICATED]: (state, isAuthenticated) => state.isAuthenticated = isAuthenticated,

    // Устанавливает значение данных подтверждения первого входа на сайт через внешний OAuth2-провайдер.
    [MutationTypes.SET_CONFIRMATION_DATA]: (state, { sessionId, email, provider }) => {
        state.confirmationData = {
            sessionId,
            email,
            provider
        };
    },

    // Очищает состояние модуля.
    [MutationTypes.CLEAR_AUTH_STATE]: (state) => {
        state.isAuthenticated = false;
        state.error = null;
        state.confirmationData = null;
    },

    // Устанавливает ошибку работы с AuthController.
    [MutationTypes.SET_ERROR]: (state, error) => state.error = error
};

//
// Actions
//
const actions = {
    // Выполняет аутентификацию пользователя на сервере.
    [ActionTypes.AUTHENTICATE]: ({ commit, dispatch }) => {
        let accessToken = readToken();
        if (!accessToken) {
            commit(MutationTypes.CLEAR_AUTH_STATE);
            console.log(`[SimpleChat] Отсутствует значение для ключа '${ACCESS_TOKEN_STORAGE_KEY}' в localStorage.`);
            return;
        }
        let options = {
            accessToken,
            success: result => {
                if (result.isAuthenticated) {
                    dispatch(ActionTypes.CURRENT_USER, {
                        ...result.user,
                        connectionIds: []
                    });
                    commit(MutationTypes.SET_IS_AUTHENTICATED, result.isAuthenticated);
                } else {
                    dispatch(ActionTypes.SIGN_OUT);
                }
            },
            externalLoginError: result => handleErrorResult(commit, dispatch, result),
            error: result => handleErrorResult(commit, dispatch, result)
        };
        AuthApiClient.authenticate(options)
            .catch(error => console.error("[SimpleChat] Не удалось авторизовать пользователя на сервере.", error));
    },

    // Выполняет выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => {
        clearToken();
        commit(MutationTypes.CLEAR_AUTH_STATE);
    },

    // Выполняет вход в приложение через внешний OAuth2-провайдер.
    [ActionTypes.SIGN_IN]: ({ commit, dispatch }, { uri, provider }) => {
        let optionsBuilder = new OAuth2ClientOptionsBuilder();
        let options = optionsBuilder
            .withWindow(window)
            .withRequestOrigin(window.globals.domain)
            .withRequestUri(uri)
            .withSuccessHandler(result => {
                storeToken(result.accessToken);
                dispatch(ActionTypes.AUTHENTICATE);
            })
            .withConfirmSignInHandler(result => {
                commit(MutationTypes.SET_CONFIRMATION_DATA, result);
            })
            .withEmailRequiredHandler(result => {
                commit(MutationTypes.SET_ERROR, {
                    message: result.message
                });
            })
            .withErrorHandler(result => {
                commit(MutationTypes.SET_ERROR, {
                    message: result.message,
                    errors: result.errors
                });
            })
            .build();
        let oauth2Client = new OAuth2Client(options);
        try {
            oauth2Client.signIn();
        } catch (e) {
            console.error(`[SimpleChat] Не удалось войти на сайт через внешнего провайдера '${provider}'.`, e.message);
        }
    },

    // Обработчик, вызываемый при закрытии модального окна с ошибкой.
    [ActionTypes.ERROR_HANDLED]: ({ commit }) => commit(MutationTypes.CLEAR_AUTH_STATE),

    // Обработчик, вызываемый при отмене входа на сайт.
    [ActionTypes.CANCEL_SIGN_IN]: ({ commit }) => commit(MutationTypes.CLEAR_AUTH_STATE),

    // Выполняет подтверждение первого входа на сайт через внешний OAuth2-провайдер.
    [ActionTypes.CONFIRM_SIGN_IN]: ({ commit, dispatch }, { code, sessionId }) => {
        let options = {
            code,
            sessionId,
            success: result => {
                storeToken(result.accessToken);
                commit(MutationTypes.CLEAR_AUTH_STATE);
                dispatch(ActionTypes.AUTHENTICATE);
            },
            externalLoginError: result => handleErrorResult(commit, dispatch, result),
            error: result => handleErrorResult(commit, dispatch, result)
        };
        AuthApiClient.confirmSignIn(options)
            .catch(error => console.error("[SimpleChat] Не удалось подтвердить вход на сайт через внешний " +
                "OAuth2-провайдер.", error));
    }
};

// Вспомогательная функция обработки результата с ошибкой.
function handleErrorResult(commit, dispatch, result) {
    dispatch(ActionTypes.SIGN_OUT);
    commit(MutationTypes.SET_ERROR, {
        message: result.message,
        errors: result.errors
    });
}

//
// Getters
//
const getters = {
    [GetterTypes.IS_AUTHENTICATED]: state => state.isAuthenticated,

    [GetterTypes.ERROR]: state => state.error,

    [GetterTypes.CONFIRMATION_DATA]: state => state.confirmationData
};

export default {
    state,
    mutations,
    actions,
    getters
};
