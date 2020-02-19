// Вспомогательный класс для работы с стилями внешнего OAuth2-провайдера..
export class ProviderHelper {
    constructor(provider) {
        this.provider = provider;
    }

    getIcon() {
        switch (this.provider) {
            case Provider.FACEBOOK: return "mdi-facebook";
            case Provider.VKONTAKTE: return "mdi-vk";
            case Provider.ODNOKLASSNIKI: return "mdi-odnoklassniki";
            default: return "mdi-help";
        }
    }

    getColor() {
        switch (this.provider) {
            case Provider.FACEBOOK: return "#3b5998";
            case Provider.VKONTAKTE: return "#45668e";
            case Provider.ODNOKLASSNIKI: return "#ed812b";
            default: return "red";
        }
    }

    getPopupUri() {
        switch (this.provider) {
            case Provider.FACEBOOK: return this._getFacebookUri();
            case Provider.VKONTAKTE: return this._getVKontakteUri();
            case Provider.ODNOKLASSNIKI: return this._getOdnoklassnikiUri();
            default: return null;
        }
    }

    _getFacebookUri() {
        let
            queryString = new URLSearchParams(),
            { clientId, redirectUri, scope, state, authorizeEndpoint } = window.globals.Facebook;

        queryString.append("client_id", clientId);
        queryString.append("redirect_uri", redirectUri);
        queryString.append("display", "popup");
        queryString.append("scope", scope);
        queryString.append("response_type", "code");
        queryString.append("state", state);
        queryString.append("auth_type", "reauthorize");

        return `${authorizeEndpoint}?${queryString.toString()}`;
    }

    _getVKontakteUri() {
        let
            queryString = new URLSearchParams(),
            { clientId, redirectUri, scope, state, authorizeEndpoint, apiVersion } = window.globals.VKontakte;

        queryString.append("client_id", clientId);
        queryString.append("redirect_uri", redirectUri);
        queryString.append("display", "popup");
        queryString.append("scope", scope);
        queryString.append("response_type", "code");
        queryString.append("state", state);
        queryString.append("v", apiVersion);

        return `${authorizeEndpoint}?${queryString.toString()}`;
    }

    _getOdnoklassnikiUri() {
        let
            queryString = new URLSearchParams(),
            { clientId, redirectUri, scope, state, authorizeEndpoint } = window.globals.Odnoklassniki;

        queryString.append("client_id", clientId);
        queryString.append("redirect_uri", redirectUri);
        queryString.append("scope", scope);
        queryString.append("response_type", "code");
        queryString.append("state", state);

        return `${authorizeEndpoint}?${queryString.toString()}`;
    }
}

export const Provider = {
    FACEBOOK: "Facebook",
    VKONTAKTE: "ВКонтакте",
    ODNOKLASSNIKI: "Одноклассники"
};
