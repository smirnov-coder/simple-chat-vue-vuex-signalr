import faker from "faker";
import { MutationTypes, ActionTypes, GetterTypes } from "@/store/constants";

export class Message {
    constructor(author, text) {
        this.author = author;
        this.text = text;
    }
}

const fakeMessages = [];
for (var index = 0; index < 10; index++) {
    fakeMessages.push(new Message(
        faker.random.number({ min: 0, max: 4 }),
        faker.lorem.text()
    ));
}

const state = {
    messages: [],//fakeMessages,
    maxLength: 5, // Пороговое значение размера коллекции сообщений.
    sliceStartIndex: 1
};

const mutations = {
    // Очищает состояние модуля.
    [MutationTypes.CLEAR_MESSAGES_STATE]: state => state.messages = [],

    // Добавляет сообщение в коллекцию сообщений.
    [MutationTypes.ADD_MESSAGE]: (state, message) => state.messages.push(message),

    // Устанавливает коллекцию сообщений.
    [MutationTypes.SET_MESSAGES]: (state, messages) => state.messages = messages,
};

const actions = {
    // Выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => commit(MutationTypes.CLEAR_MESSAGES_STATE),

    // Добавляет сообщение в коллекцию сообщений.
    [ActionTypes.RECEIVED_MESSAGE]: ({ commit, dispatch, state }, message) => {
        commit(MutationTypes.ADD_MESSAGE, message);
        if (state.messages.length > state.maxLength) {
            dispatch(ActionTypes.REDUCE_MESSAGES);
        }
    },

    // Урезает коллекцию сообщений для уменьшения количества узлов DOM-дерева.
    [ActionTypes.REDUCE_MESSAGES]: ({ commit, state }) => {
        let messages = state.messages.slice(state.sliceStartIndex);
        commit(MutationTypes.SET_MESSAGES, messages);
    }
};

const getters = {
    // Коллекция сообщений.
    [GetterTypes.MESSAGES]: state => state.messages
};

export default {
    state,
    mutations,
    actions,
    getters
};
