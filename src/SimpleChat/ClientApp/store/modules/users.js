import faker from "faker";
import { MutationTypes, ActionTypes, GetterTypes } from "@/store/constants";

const fakeUsers = [];
for (let index = 0; index < 5; index++) {
    fakeUsers.push({
        id: index.toString(),
        name: faker.name.findName(),
        avatar: faker.image.avatar(),
        provider: "Facebook",
        connectionIds: []
    });
}

const state = {
    users: [],//fakeUsers,
    currentUser: {
        id: "",
        name: "",
        avatar: "",
        provider: "",
        connectionIds: []
    }
};

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
        //console.log(`[users] Add users [${users.length}] at ${new Date().toLocaleTimeString()}`, users);//
        state.users = state.users.concat(users);
    },

    // Удаляет онлайн-пользователя.
    [MutationTypes.REMOVE_USER]: (state, userId) => {
        //console.log(`[users] Remove online user [UserId=${userId}] at ${new Date().toLocaleTimeString()}`);//
        state.users = state.users.filter(user => user.id !== userId);
    },

    [MutationTypes.CLEAR_USERS]: (state) => {
        //console.log(`[users] Clear online users at at ${new Date().toLocaleTimeString()}`, state);//
        state.users = [];
    },

    [MutationTypes.ADD_CURRENT_USER_CONNECTION]: (state, connectionId) => {
        //console.log(`[users] Add current user connection [Id=${connectionId}]`);//
        let user = { ...state.currentUser };
        user.connectionIds = user.connectionIds.concat(connectionId);
        state.currentUser = user;
    },

    [MutationTypes.ADD_USER_CONNECTION]: (state, { userId, connectionId }) => {
        //console.log(`[users] Add user [Id=${userId}] connection [Id=${connectionId}]`);//
        let users = [...state.users];/////
        let user = users.find(item => item.id === userId);
        user.connectionIds = user.connectionIds.concat(connectionId);
        state.users = users;
    },

    [MutationTypes.REMOVE_CURRENT_USER_CONNECTION]: (state, connectionId) => {
        //console.log(`[users] Remove current user connection [Id=${connectionId}]`);//
        let user = { ...state.currentUser };
        user.connectionIds = user.connectionIds.filter(item => item !== connectionId);
        state.currentUser = user;
    },

    [MutationTypes.REMOVE_USER_CONNECTION]: (state, { userId, connectionId }) => {
        //console.log(`[users] Remove connection [Id=${connectionId}]`);//
        let users = [...state.users];//////
        let user = users.find(item => item.id === userId);
        user.connectionIds = user.connectionIds.filter(item => item !== connectionId);
        state.users = users;
    },
};

const actions = {
    // Выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => commit(MutationTypes.CLEAR_USERS_STATE),

    // Новый подключённый к хабу пользователь.
    [ActionTypes.NEW_USER]: ({ commit }, user) => commit(MutationTypes.ADD_USERS, user),

    // Подключённые к хабу пользователи.
    [ActionTypes.CONNECTED_USERS]: ({ commit, state }, { ownConnectionIds, users }) => {
        //console.log(`[users] Connected users at ${new Date().toLocaleTimeString()}. My UserId=${userId}`, users);
        let user = { ...state.currentUser };
        user.connectionIds = ownConnectionIds;
        commit(MutationTypes.SET_CURRENT_USER, user);
        commit(MutationTypes.ADD_USERS, users);
    },

    [ActionTypes.NEW_USER_CONNECTION]: ({ commit, state }, { userId, connectionId }) => {
        if (state.currentUser.id === userId) {
            commit(MutationTypes.ADD_CURRENT_USER_CONNECTION, connectionId);
        } else {
            commit(MutationTypes.ADD_USER_CONNECTION, { userId, connectionId });
        }
    },

    // Отключение пользователя от хаба.
    [ActionTypes.DISCONNECTED_USER]: ({ commit, state }, { userId, connectionId }) => {
        //console.log(`[users] Disconnected user [UserId=${userId}] connection [Id=${connectionId}]`);//
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

    // Закрытие подключения к хабу из-за ошибки.
    [ActionTypes.CONNECTION_ERROR]: ({ commit }) => {
        commit(MutationTypes.CLEAR_USERS);
    },
};

const getters = {
    // Коллекция онлайн-пользователей.
    [GetterTypes.USERS]: state => state.users,

    // Текущий пользователь.
    [GetterTypes.CURRENT_USER]: state => state.currentUser
};

export default {
    state,
    mutations,
    actions,
    getters
};
