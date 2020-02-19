<template>
    <v-app>
        <v-container align-center justify-center>
            <chat-app v-if="isAuthenticated" />
            <welcome-screen v-else />
            <error-modal />
            <confirm-modal />
        </v-container>
    </v-app>
</template>

<script>
    import WelcomeScreen from "@/components/Welcome";
    import ChatApp from "@/components/Chat";
    import ErrorModal from "@/components/ErrorModal";
    import ConfirmModal from "@/components/ConfirmModal";
    import { ActionTypes, GetterTypes } from "@/scripts/constants";

    export default {
        components: {
            WelcomeScreen,
            ChatApp,
            ErrorModal,
            ConfirmModal
        },
        computed: {
            isAuthenticated() {
                return this.$store.getters[GetterTypes.IS_AUTHENTICATED];
            }
        },
        created() {
            this.$store.dispatch(ActionTypes.AUTHENTICATE);
        }
    };
</script>
