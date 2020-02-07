<template>
    <v-btn dark
           color="primary"
           @click.stop="signIn">
        <v-icon left light>mdi-facebook</v-icon>
        Вход через Facebook
    </v-btn>
</template>

<script>
    import { ActionTypes } from "@/store/constants";

    export default {
        methods: {
            signIn() {
                let
                    { domain } = window.globals,
                    queryString = new URLSearchParams(),
                    { clientId, redirectUri, scope, state, authorizeEndpoint } = window.globals.Facebook;

                queryString.append("client_id", clientId);
                queryString.append("redirect_uri", redirectUri);
                queryString.append("display", "popup");
                queryString.append("scope", scope);
                queryString.append("response_type", "code");
                queryString.append("state", state);

                queryString.append("auth_type", "reauthorize"); /////

                window.addEventListener("message", (event) => {
                    if (event.data.source && event.data.source === "popup") {
                        if (event.origin.startsWith(domain)) {
                            event.source.close();
                            this.$store.dispatch(ActionTypes.SIGN_IN_RESULT, event.data.result);
                        } else {
                            console.warn("[SimpleChat] Нарушение политики CORS. Получено сообщение с неизвестного домена.");
                        }
                    }
                }, true);

                // Взято из https://vk.com/dev/openapi VK.UI.popup().
                let
                    screenX = typeof window.screenX != "undefined" ? window.screenX : window.screenLeft,
                    screenY = typeof window.screenY != "undefined" ? window.screenY : window.screenTop,
                    outerWidth = typeof window.outerWidth != "undefined" ? window.outerWidth : document.body.clientWidth,
                    outerHeight = typeof window.outerHeight != "undefined" ? window.outerHeight : (document.body.clientHeight - 22),
                    width = 480,
                    height = 480,
                    left = parseInt(screenX + ((outerWidth - width) / 2), 10),
                    top = parseInt(screenY + ((outerHeight - height) / 2.5), 10);
                left = window.screen && window.screenX && screen.left && screen.left > 1000 ? 0 : left; // FF with 2 monitors fix
                let features = (
                    "width=" + width +
                    ",height=" + height +
                    ",left=" + left +
                    ",top=" + top
                );

                window.open(`${authorizeEndpoint}?${queryString.toString()}`, "facebook_popup", features);
            },
        },
    };
</script>
