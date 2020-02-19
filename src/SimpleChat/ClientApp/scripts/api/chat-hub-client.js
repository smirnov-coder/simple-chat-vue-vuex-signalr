import * as SignalR from "@aspnet/signalr";
import { CustomHttpClient } from "@/scripts/api/custom-http-client";
import { readToken } from "@/scripts/utils";
import { HubMethodNames, HubClientMethodNames } from "@/scripts/constants";

// Инкапсулирует доступ к API чата.
export class ChatHubClient {
    constructor(options) {
        this._options = options;
        this._httpClient = this._configureHttpClient();
    }

    // Настраивает HttpClient для выполнения запросов к API чат-хаба.
    _configureHttpClient() {
        let { onUnauthorized, onNotFound, onUnknownError } = this._options;
        let httpClient = new CustomHttpClient(SignalR.NullLogger.instance);
        httpClient.onErrorResponse = (statusCode) => {
            switch (statusCode) {
                case 401: {
                    onUnauthorized();
                    break;
                }
                case 404: {
                    onNotFound();
                    break;
                }
                default: {
                    onUnknownError();
                    break;
                }
            }
        };
        return httpClient;
    }

    // Создаёт новое подключение к чат-хабу.
    _createConnection() {
        let httpClient = this._httpClient;
        let connection = new SignalR.HubConnectionBuilder()
            .withUrl("/api/chat", {
                accessTokenFactory: () => readToken(),
                httpClient
            })
            .configureLogging(SignalR.LogLevel.Information)
            .build();

        // Увеличить таймаут, чтобы сервер не закрывал принудительно соединение. Значение по умолчанию 30 сек. слишком
        // мало.
        connection.serverTimeoutInMilliseconds = 60000;

        // Добавить обработчики для всех пользовательских событий хаба.
        let { onClose, ...rest } = this._options;
        Object.values(HubClientMethodNames).forEach(methodName => {
            let handler = rest[methodName];
            if (handler)
                connection.on(methodName, handler);
        });

        // Обработчик закрытия соединения к хабу.
        connection.onclose(onClose);

        return connection;
    }

    // Устанавливает соединение с чат-хабом.
    connect() {
        this._connection = this._createConnection();
        return this._connection.start()
            .then(this._options.onStart);
    }

    // Разрывает существующее соединение с чат-хабом.
    disconnect() {
        if (this._connection) {
            // На всякий случай отписаться от всех событий хаба.
            Object.values(HubClientMethodNames).forEach(methodName => {
                this._connection.off(methodName);
            });
            return this._connection.stop()
                .then(() => this._connection = null);
        }
    }

    // Отправляет в чат новое сообщение.
    sendMessage(message) {
        if (!this._connection) {
            throw new Error("Соединение с хабом не установлено.");
        }
        return this._connection.invoke(HubMethodNames.SEND_MESSAGE, message);
    }
}

// Вспомогательный класс для создания объекта параметров клиента чат-хаба.
export class ChatHubClientOptionsBuilder {
    constructor() {
        this._options = {};
    }

    withStartHandler(handler) {
        this._options.onStart = handler;
        return this;
    }

    withCloseHandler(handler) {
        this._options.onClose = handler;
        return this;
    }

    withUnauthorizedHandler(handler) {
        this._options.onUnauthorized = handler;
        return this;
    }

    withNotFoundHandler(handler) {
        this._options.onNotFound = handler;
        return this;
    }

    withUnknownErrorHandler(handler) {
        this._options.onUnknownError = handler;
        return this;
    }

    withChatHubMethodHandler(methodName, handler) {
        this._options[methodName] = handler;
        return this;
    }

    build() {
        return this._options;
    }
}
