import { AuthResultTypes } from "@/scripts/constants";

let
    authenticateRequestUri = window.globals.authenticateEndpoint,
    confirmSignInRequestUri = window.globals.confirmSignInEndpoint;

// Инкапсулирует доступ к API авторизации пользователей.
export class AuthApiClient {
    static authenticate({ accessToken, success, externalLoginError, error }) {
        let options = {
            method: "GET",
            headers: {
                Authorization: `Bearer ${accessToken}`
            }
        };
        return fetch(authenticateRequestUri, options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error(`Получен ответ с ошибкой ${response.status} (${response.statusText}).`);
            })
            .then(result => {
                if ("type" in result) {
                    switch (result.type) {
                        case AuthResultTypes.AUTH_CHECK: {
                            success(result);
                            break;
                        }
                        case AuthResultTypes.EXTERNAL_LOGIN_ERROR: {
                            externalLoginError(result);
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
                } else {
                    throw new Error("Неизвестная структура результата операции.");
                }
            });
    }

    // Выполняет подтверждение первого входа на сайт через внешний OAuth2-провайдер.
    static confirmSignIn({ code, sessionId, success, externalLoginError, error }) {
        let options = {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: `code=${code}&sessionId=${sessionId}`
        };
        return fetch(confirmSignInRequestUri, options)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error(`Получен ответ с ошибкой ${response.status} (${response.statusText}).`);
            })
            .then(result => {
                if ("type" in result) {
                    switch (result.type) {
                        case AuthResultTypes.SUCCESS: {
                            success(result);
                            break;
                        }
                        case AuthResultTypes.EXTERNAL_LOGIN_ERROR: {
                            externalLoginError(result);
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
                } else {
                    throw new Error("Неизвестная структура результата операции.");
                }
            });
    }
}
