import { AuthResultTypes } from "@/scripts/constants";

// Инкапсулирует доступ к API внешнего OAuth2-провайдера.
export class OAuth2Client {
    constructor(options) {
        this._options = options;
        this._popupWindowSize = this._getPopupWindowSize();
        this._configureWindow();
    }

    // Настраивает родительское окно браузера для получения результата операции от popup-окна входа через внешний
    // OAuth2-провайдер.
    _configureWindow() {
        let { window, requestOrigin, success, confirmSignIn, emailRequired, error } = this._options;
        window.addEventListener("message", event => {
            if (event.data.source && event.data.source === "popup") {
                if (event.origin.startsWith(requestOrigin)) {
                    event.source.close();
                    let result = event.data.result;
                    if ("type" in result) {
                        switch (result.type) {
                            case AuthResultTypes.SUCCESS: {
                                success(result);
                                break;
                            }
                            case AuthResultTypes.CONFIRM_SIGN_IN: {
                                confirmSignIn(result);
                                break;
                            }
                            case AuthResultTypes.EMAIL_REQUIRED: {
                                emailRequired(result);
                                break;
                            }
                            case AuthResultTypes.ERROR: {
                                error(result);
                                break;
                            }
                            default: {
                                throw new Error("Неизвестный тип результата операции.");
                            }
                        }
                    }
                } else {
                    throw new Error("Нарушение политики CORS. Получено сообщение с неизвестного домена.");
                }
            }
        }, true);
    }

    // Возвращает размер и расположение popup-окна входа через внешний OAuth2-провайдер.
    _getPopupWindowSize() {
        let { window } = this._options;
        // Взято из https://vk.com/dev/openapi VK.UI.popup().
        let
            screenX = typeof window.screenX !== "undefined" ? window.screenX : window.screenLeft,
            screenY = typeof window.screenY !== "undefined" ? window.screenY : window.screenTop,
            outerWidth = typeof window.outerWidth !== "undefined" ? window.outerWidth : document.body.clientWidth,
            outerHeight = typeof window.outerHeight !== "undefined"
                ? window.outerHeight
                : document.body.clientHeight - 22,
            width = 480,
            height = 480,
            left = parseInt(screenX + ((outerWidth - width) / 2), 10),
            top = parseInt(screenY + ((outerHeight - height) / 2.5), 10);
        // FF with 2 monitors fix
        left = window.screen && window.screenX && screen.left && screen.left > 1000 ? 0 : left; 
        return `width=${width},height=${height},left=${left},top=${top}`;
    }

    // Открывает popup-окно для входа на сайт через внешний OAuth2-провайдер.
    signIn() {
        let { window, requestUri } = this._options;
        window.open(requestUri, "external_provider_popup", this._popupWindowSize);
    }
}

// Вспомогательный класс для создания объекта параметров клиента внешнего OAuth2-провайдера.
export class OAuth2ClientOptionsBuilder {
    constructor() {
        this._options = {};
    }

    withRequestOrigin(requestOrigin) {
        this._options.requestOrigin = requestOrigin;
        return this;
    }

    withWindow(window) {
        this._options.window = window;
        return this;
    }

    withRequestUri(requestUri) {
        this._options.requestUri = requestUri;
        return this;
    }

    withSuccessHandler(handler) {
        this._options.success = handler;
        return this;
    }

    withConfirmSignInHandler(handler) {
        this._options.confirmSignIn = handler;
        return this;
    }

    withEmailRequiredHandler(handler) {
        this._options.emailRequired = handler;
        return this;
    }

    withErrorHandler(handler) {
        this._options.error = handler;
        return this;
    }

    build() {
        return this._options;
    }
}
