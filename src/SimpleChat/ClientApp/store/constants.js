export const MutationTypes = mapKeyToValue({
    SET_CONNECTION: null,
    CLEAR_CONNECTION: null,
    SET_IS_AUTHENTICATED: null,
    CLEAR_MESSAGES_STATE: null,
    ADD_MESSAGE: null,
    SET_MESSAGES: null,
    SET_CURRENT_USER: null,
    CLEAR_USERS_STATE: null,
    ADD_USERS: null,
    REMOVE_USER: null,
    CLEAR_USERS: null,
    ADD_CURRENT_USER_CONNECTION: null,
    ADD_USER_CONNECTION: null,
    REMOVE_CURRENT_USER_CONNECTION: null,
    REMOVE_USER_CONNECTION: null,
    SET_CONFIRM_DATA: null,
    CLEAR_AUTH_STATE: null,
    SET_ERROR: null,
});

export const ActionTypes = mapKeyToValue({
    CONNECT_HUB: null,
    RECEIVED_MESSAGE: null,
    NEW_USER: null,
    CONNECTED_USERS: null,
    DISCONNECTED_USER: null,
    CONNECTION_ERROR: null,
    DISCONNECT_HUB: null,
    SEND_MESSAGE: null,
    AUTHENTICATE: null,
    CURRENT_USER: null,
    SIGN_OUT: null,
    REDUCE_MESSAGES: null,
    NEW_USER_CONNECTION: null,
    SIGN_IN_RESULT: null,
    SIGN_IN_CANCELED: null,
    SIGN_IN_CONFIRMED: null,
    ERROR_HANDLED: null,
});

export const GetterTypes = mapKeyToValue({
    IS_AUTHENTICATED: null,
    MESSAGES: null,
    USERS: null,
    CURRENT_USER: null,
    ERROR: null,
    CONFIRM_DATA: null,
});

export const HubMethodNames = {
    SEND_MESSAGE: "SendMessage",
};

export const HubClientMethodNames = {
    RECEIVE_MESSAGE: "ReceiveMessage",
    NEW_USER: "NewUser",
    CONNECTED_USERS: "ConnectedUsers",
    DISCONNECTED_USER: "DisconnectedUser",
    NEW_USER_CONNECTION: "NewUserConnection",
    FORCE_SIGN_OUT: "ForceSignOut",
};

function mapKeyToValue(obj) {
    let result = {};
    Object.keys(obj).map(key => {
        result[key] = key;
    });
    return result;
}
