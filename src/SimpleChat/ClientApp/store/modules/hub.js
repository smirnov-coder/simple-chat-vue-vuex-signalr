import * as SignalR from "@aspnet/signalr";
import { readToken } from "@/scripts/utils";
import { MutationTypes, ActionTypes, HubMethodNames, HubClientMethodNames } from "@/store/constants";
import { CustomHttpClient } from "@/store/CustomHttpClient";

const state = {
    connection: null
};

const mutations = {
    // Устанавливает объект подключения к хабу.
    [MutationTypes.SET_CONNECTION]: (state, connection) => state.connection = connection,

    // Закрывает подключение к хабу.
    [MutationTypes.CLEAR_CONNECTION]: state => state.connection = null
};

const actions = {
    // Осуществляет подключение к хабу.
    [ActionTypes.CONNECT_HUB]: ({ commit, dispatch }) => {
        // Для обработки ответов 4хх от хаба используем пользовательский HTTP-клиент.
        let httpClient = new CustomHttpClient(SignalR.NullLogger.instance);
        httpClient.onErrorResponse = (statusCode) => {
            switch (statusCode) {
                // Если получили ответ 401, значит 'access_token' не валиден, выходим из приложения.
                case 401: {
                    console.error("[SimpleChat] Получен ответ 401 (Unauthorized). Возможно, 'access_token' просрочен " +
                        "или отсутствует.");
                    dispatch(ActionTypes.SIGN_OUT);
                    break;
                }
                
                // Если получили ответ 404, значит на сервере не найдено соединение, скорее всего потому, что оно было
                // принудительно закрыто сервером из-за истечения таймаута.
                case 404: {
                    console.error("[SimpleChat] Получен ответ 404 (Not Found). Возможно, сервер закрыл соединение " +
                        "из-за истечения таймаута.");
                    dispatch(ActionTypes.CONNECTION_ERROR);
                    break;
                }

                default: {
                    console.error(`[SimpleChat] Получен ответ ${statusCode}.`);
                    dispatch(ActionTypes.CONNECTION_ERROR);
                }
            }
        };

        let connection = new SignalR.HubConnectionBuilder()
            .withUrl("/api/chat", {
                accessTokenFactory: () => readToken(),
                httpClient,
            })
            .configureLogging(SignalR.LogLevel.Information)
            .build();
        // Увеличить таймаут, чтобы сервер не закрывал принудительно соединение.
        // Значение по умолчанию 30 сек. слишком мало.
        connection.serverTimeoutInMilliseconds = 60000;

        // Обработчик получения сообщения чата.
        connection.on(HubClientMethodNames.RECEIVE_MESSAGE, message => {
            //console.log(`[hub] Receive message at ${new Date().toLocaleTimeString()}`, message);
            dispatch(ActionTypes.RECEIVED_MESSAGE, message);
        });

        // Обработчик получения данных о подключении к хабу нового пользователя.
        connection.on(HubClientMethodNames.NEW_USER, user => {
            //console.log(`[hub] New user [UserId=${user.id}] at ${new Date().toLocaleTimeString()}`, user);
            dispatch(ActionTypes.NEW_USER, user);
        });

        // Обработчик получения данных о подключённых к хабу пользователей.
        connection.on(HubClientMethodNames.CONNECTED_USERS, (ownConnectionIds, users) => {
            //console.log(`[hub] Connected users at ${new Date().toLocaleTimeString()}. My ConnectionIds=${ownConnectionIds}`, users);
            dispatch(ActionTypes.CONNECTED_USERS, { ownConnectionIds, users });
        });

        // Обработчик получения данных о новом подключении к хабу существующего пользователя.
        connection.on(HubClientMethodNames.NEW_USER_CONNECTION, (userId, connectionId) => {
            //console.log(`[hub] New connection Id=${connectionId}`);
            dispatch(ActionTypes.NEW_USER_CONNECTION, { userId, connectionId });
        });

        // Обработчик получения данных об отключённом пользователе.
        connection.on(HubClientMethodNames.DISCONNECTED_USER, (userId, connectionId) => {
            //console.log(`[hub] Disconnected user [UserId=${userId}] at ${new Date().toLocaleTimeString()}`);
            dispatch(ActionTypes.DISCONNECTED_USER, { userId, connectionId });
        });

        // Обработчик для принудительного выхода пользователя из приложения, т.к. на данный момент SignalR не имеет
        // встроенной возможности принудительно закрывать определённое соединение из списка существующих подключений.
        connection.on(HubClientMethodNames.FORCE_SIGN_OUT, () => {
            //console.log(`[hub] Force sign out at ${new Date().toLocaleTimeString()}`);
            dispatch(ActionTypes.SIGN_OUT);
        });

        // Обработчик закрытия соединения к хабу.
        connection.onclose(error => {
            if (error) {
                dispatch(ActionTypes.CONNECTION_ERROR);
                console.error("[SimpleChat] Соединение аварийно закрыто из-за ошибки.", error);
            } else {
                console.log("[SimpleChat] Соединение успешно закрыто.");
            }
        });

        // Корректное завершение работы с окном чата. Роли не играет, но на всякий случай.
        window.addEventListener("beforeunload", () => {
            //console.log("[hub] window beforeUnload");
            dispatch(ActionTypes.DISCONNECT_HUB);
        });

        // Установка соединения с хабом.
        connection.start()
            .then(() => {
                commit(MutationTypes.SET_CONNECTION, connection);
                console.log("[SimpleChat] Соединение установлено.");
            })
            .catch(error => console.error("[SimpleChat] Произошла ошибка при попытке установить соединение.", error));
    },

    // Отключает пользователя от хаба.
    [ActionTypes.DISCONNECT_HUB]: ({ commit, state }) => {
        if (state.connection) {
            // На всякий случай отпишемся от всех событий хаба.
            state.connection.off(HubClientMethodNames.RECEIVE_MESSAGE);
            state.connection.off(HubClientMethodNames.NEW_USER);
            state.connection.off(HubClientMethodNames.CONNECTED_USERS);
            state.connection.off(HubClientMethodNames.DISCONNECTED_USER);
            state.connection.off(HubClientMethodNames.NEW_USER_CONNECTION);
            state.connection.off(HubClientMethodNames.FORCE_SIGN_OUT);
            state.connection.stop()
                .then(() => commit(MutationTypes.CLEAR_CONNECTION));
        }
    },

    // Отправляет сообщение в хаб.
    [ActionTypes.SEND_MESSAGE]: ({ state }, message) => {
        state.connection.invoke(HubMethodNames.SEND_MESSAGE, message)
            .catch(error => console.error("[SimpleChat] Не удалось отправить сообщение.", error));
    },

    // Выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ dispatch }) => dispatch(ActionTypes.DISCONNECT_HUB)
};

export default {
    state,
    mutations,
    actions
};
