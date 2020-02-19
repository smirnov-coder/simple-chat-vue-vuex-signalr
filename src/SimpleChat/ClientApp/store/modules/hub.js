import { MutationTypes, ActionTypes, HubClientMethodNames } from "@/scripts/constants";
import { ChatHubClient, ChatHubClientOptionsBuilder } from "@/scripts/api/chat-hub-client";

// Вспомогательная функция создания предварительно настроенного клиента чат-хаба.
function createChatHubClient(dispatch) {
    let optionsBuilder = new ChatHubClientOptionsBuilder();
    let options = optionsBuilder
        // Обработчик открытия соединения с хабом.
        .withStartHandler(() => console.log("[SimpleChat] Соединение установлено."))
        // Обработчик закрытия соединения к хабу. Соединение может быть закрыто из-за ошибки.
        .withCloseHandler(error => {
            if (error) {
                dispatch(ActionTypes.CONNECTION_ERROR);
                console.error("[SimpleChat] Соединение аварийно закрыто из-за ошибки.", error);
            } else {
                console.log("[SimpleChat] Соединение успешно закрыто.");
            }
        })
        // Если получили ответ 401, значит 'access_token' не валиден, выходим из приложения.
        .withUnauthorizedHandler(() => {
            console.error("[SimpleChat] Получен ответ 401 (Unauthorized). Возможно, 'access_token' просрочен " +
                "или отсутствует.");
            dispatch(ActionTypes.SIGN_OUT);
        })
        // Если получили ответ 404, значит на сервере не найдено соединение, скорее всего потому, что оно было
        // принудительно закрыто сервером из-за истечения таймаута.
        .withNotFoundHandler(() => {
            console.error("[SimpleChat] Получен ответ 404 (Not Found). Возможно, сервер закрыл соединение из-за " +
                "истечения таймаута.");
            dispatch(ActionTypes.CONNECTION_ERROR);
        })
        // Другие коды http-ответов не обрабатываем.
        .withUnknownErrorHandler(() => {
            console.error(`[SimpleChat] Получен ответ ${statusCode}.`);
            dispatch(ActionTypes.CONNECTION_ERROR);
        })
        // Обработчик получения сообщения чата.
        .withChatHubMethodHandler(HubClientMethodNames.RECEIVE_MESSAGE, message => {
            dispatch(ActionTypes.RECEIVED_MESSAGE, message);
        })
        // Обработчик получения данных о подключении к хабу нового пользователя.
        .withChatHubMethodHandler(HubClientMethodNames.NEW_USER, user => {
            dispatch(ActionTypes.NEW_USER, user);
        })
        // Обработчик получения данных о подключённых к хабу пользователей.
        .withChatHubMethodHandler(HubClientMethodNames.CONNECTED_USERS, (ownConnectionIds, users) => {
            dispatch(ActionTypes.CONNECTED_USERS, { ownConnectionIds, users });
        })
        // Обработчик получения данных о новом подключении к хабу существующего пользователя.
        .withChatHubMethodHandler(HubClientMethodNames.NEW_USER_CONNECTION, (userId, connectionId) => {
            dispatch(ActionTypes.NEW_USER_CONNECTION, { userId, connectionId });
        })
        // Обработчик получения данных об отключённом пользователе.
        .withChatHubMethodHandler(HubClientMethodNames.DISCONNECTED_USER, (userId, connectionId) => {
            dispatch(ActionTypes.DISCONNECTED_USER, { userId, connectionId });
        })
        // Обработчик для принудительного выхода пользователя из приложения, т.к. на данный момент SignalR не имеет
        // встроенной возможности принудительно закрывать определённое соединение из списка существующих подключений.
        .withChatHubMethodHandler(HubClientMethodNames.FORCE_SIGN_OUT, () => {
            dispatch(ActionTypes.SIGN_OUT);
        })
        .build();
    return new ChatHubClient(options);
}

//
// State
//
const state = {
    // Клиент чат-хаба.
    client: null
};

//
// Mutations
//
const mutations = {
    // Устанавливает клиент чат-хаба.
    [MutationTypes.SET_CLIENT]: (state, client) => state.client = client
};

//
// Actions
//
const actions = {
    // Осуществляет подключение к хабу.
    [ActionTypes.CONNECT_HUB]: ({ commit, dispatch, state }) => {
        if (!state.client) {
            let client = createChatHubClient(dispatch);
            commit(MutationTypes.SET_CLIENT, client);
        }
        state.client.connect()
            .catch(error => console.error("[SimpleChat] Произошла ошибка при попытке подключения к хабу.", error));
    },

    // Отключает пользователя от хаба.
    [ActionTypes.DISCONNECT_HUB]: ({ state }) => {
        if (state.client) {
            state.client.disconnect()
                .catch(error => console.error("[SimpleChat] Что-то пошло не так при отключении от хаба.", error));
        }
    },

    // Отправляет сообщение в чат.
    [ActionTypes.SEND_MESSAGE]: ({ state }, message) => {
        state.client.sendMessage(message)
            .catch(error => console.error("[SimpleChat] Не удалось отправить сообщение.", error));
    },

    // Выполняет выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ dispatch }) => dispatch(ActionTypes.DISCONNECT_HUB)
};

export default {
    state,
    mutations,
    actions
};
