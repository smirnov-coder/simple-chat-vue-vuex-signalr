import { MutationTypes, ActionTypes, GetterTypes, MESSAGES_MAX_LENGTH } from "@/scripts/constants";
import FakesGenerator from "@/scripts/fakes-generator";

// Представляет собой сообщение чата.
export class Message {
    constructor(author, text) {
        this.author = author;
        this.text = text;
    }
}

const fakeMessages = new FakesGenerator().getFakeMessages();

//
// State
//
const state = {
    // Коллекция сообщений чата.
    messages: []//fakeMessages
};

//
// Mutations
//
const mutations = {
    // Очищает состояние модуля.
    [MutationTypes.CLEAR_MESSAGES_STATE]: state => state.messages = [],

    // Добавляет сообщение в коллекцию сообщений.
    [MutationTypes.ADD_MESSAGE]: (state, message) => state.messages.push(message),

    // Устанавливает коллекцию сообщений.
    [MutationTypes.SET_MESSAGES]: (state, messages) => state.messages = messages
};

//
// Actions
//
const actions = {
    // Выполняет выход из приложения.
    [ActionTypes.SIGN_OUT]: ({ commit }) => commit(MutationTypes.CLEAR_MESSAGES_STATE),

    // Обработчик, срабатывающий при получении нового сообщения чата.
    [ActionTypes.RECEIVED_MESSAGE]: ({ commit, dispatch, state }, message) => {
        commit(MutationTypes.ADD_MESSAGE, message);
        if (state.messages.length > MESSAGES_MAX_LENGTH) {
            dispatch(ActionTypes.REDUCE_MESSAGES);
        }
    },

    // Урезает коллекцию сообщений для уменьшения количества узлов DOM-дерева.
    [ActionTypes.REDUCE_MESSAGES]: ({ commit, state }) => {
        let sliceStartIndex = 1;
        let messages = state.messages.slice(sliceStartIndex);
        commit(MutationTypes.SET_MESSAGES, messages);
    }
};

//
// Getters
//
const getters = {
    [GetterTypes.MESSAGES]: state => state.messages
};

export default {
    state,
    mutations,
    actions,
    getters
};
