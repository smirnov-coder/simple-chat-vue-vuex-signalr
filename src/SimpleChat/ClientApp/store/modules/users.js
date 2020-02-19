import { MutationTypes, ActionTypes, GetterTypes } from "@/scripts/constants";
import FakesGenerator from "@/scripts/fakes-generator";

const fakeUsers = new FakesGenerator().getFakeUsers();

//
// State
//
const state = {
    // Коллекция пользователей, подключённых к чат-хабу.
    users: [],//fakeUsers,

    // Текущий пользователь.
    currentUser: {
        id: "",
        name: "",
        avatar: "",
        provider: "",
        connectionIds: []
    }
};

//
// Mutations
//
const mutations = {
    // Устанавливает текущего пользователя.
    [MutationTypes.SET_CURRENT_USER]: (state, user) => state.currentUser = user,

    // Очищает состояние модуля.
    [MutationTypes.CLEAR_USERS_STATE]: state => {
        state.currentUser = {
            id: "",
            name: "",
            avatar: "",
            provider: "",
            connectionIds: []
        };
        state.users = [];
    },

    // Добавляет пользователя(-ей) в коллекцию онлайн-пользователей.
    [MutationTypes.ADD_USERS]: (state, users) => {
        state.users = state.users.concat(users);
    },

    // Удаляет онлайн-пользователя.
    [MutationTypes.REMOVE_USER]: (state, userId) => {
        state.users = state.users.filter(user => user.id !== userId);
    },

    // Очищает коллекцию онлайн-пользователей.
    [MutationTypes.CLEAR_USERS]: (state) => {
        state.users = [];
    },

    // Добавляет новое подключение в коллекцию подключений текущего пользователя.
    [MutationTypes.ADD_CURRENT_USER_CONNECTION]: (state, connectionId) => {
        let user = { ...state.currentUser };
        user.connectionIds = user.connectionIds.concat(connectionId);
        state.currentUser = user;
    },

    // Добавляет новое подключение в коллекцию подключений одного из онлайн-пользователей.
    [MutationTypes.ADD_USER_CONNECTION]: (state, { userId, connectionId }) => {
        let users = [...state.users];
        let user = users.find(item => item.id === userId);
        user.connectionIds = user.connectionIds.concat(connectionId);
        state.users = users;
    },

    // Удаляет подключение из коллекции покдлючений текущего пользователя.
    [MutationTypes.REMOVE_CURRENT_USER_CONNECTION]: (state, connectionId) => {
        let user = { ...state.currentUser };
        user.connectionIds = user.connectionIds.filter(item => item !== connectionId);
        state.currentUser = user;
    },

    // Удаляет подключение из коллекции подключений одного из пользователей.
    [MutationTypes.REMOVE_USER_CONNECTION]: (state, { userId, connectionId }) => {
        let users = [...state.users];
        let user = users.find(item => item.id === userId);
        user.connectionIds = user.connectionIds.filter(item => item !== connectionId);
        state.users = users;
    }
};

//
// Actions
//
const actions = {
    // Выполняет выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => commit(MutationTypes.CLEAR_USERS_STATE),

    // Обработчик, вызываевый при подключении нового пользователя к чат-хабу.
    [ActionTypes.NEW_USER]: ({ commit }, user) => commit(MutationTypes.ADD_USERS, user),

    // Обработчик, вызываемый при получении информации о всех подключённых к чат-хабу пользователей.
    [ActionTypes.CONNECTED_USERS]: ({ commit, state }, { ownConnectionIds, users }) => {
        let user = { ...state.currentUser };
        user.connectionIds = ownConnectionIds;
        commit(MutationTypes.SET_CURRENT_USER, user);
        commit(MutationTypes.ADD_USERS, users);
    },

    // Добавляет новое подключение в коллекцию подключений пользователя.
    [ActionTypes.NEW_USER_CONNECTION]: ({ commit, state }, { userId, connectionId }) => {
        if (state.currentUser.id === userId) {
            commit(MutationTypes.ADD_CURRENT_USER_CONNECTION, connectionId);
        } else {
            commit(MutationTypes.ADD_USER_CONNECTION, { userId, connectionId });
        }
    },

    // Обработчик, вызывваемый при отключении пользователя от чат-хаба.
    [ActionTypes.DISCONNECTED_USER]: ({ commit, state }, { userId, connectionId }) => {
        if (state.currentUser.id === userId) {
            commit(MutationTypes.REMOVE_CURRENT_USER_CONNECTION, connectionId);
        } else {
            commit(MutationTypes.REMOVE_USER_CONNECTION, { userId, connectionId });
            let user = state.users.find(item => item.id === userId);
            if (user.connectionIds.length === 0) {
                commit(MutationTypes.REMOVE_USER, userId);
            }
        }
    },

    // Текущий пользователь приложения.
    [ActionTypes.CURRENT_USER]: ({ commit }, user) => commit(MutationTypes.SET_CURRENT_USER, user),

    // Обработчик, вызываемый при возникновении ошибки подключения к чат-хабу.
    [ActionTypes.CONNECTION_ERROR]: ({ commit }) => {
        commit(MutationTypes.CLEAR_USERS);
    }
};

//
// Getters
//
const getters = {
    [GetterTypes.USERS]: state => state.users,

    [GetterTypes.CURRENT_USER]: state => state.currentUser
};

export default {
    state,
    mutations,
    actions,
    getters
};
