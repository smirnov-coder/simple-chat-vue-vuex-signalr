<template>
    <v-app>
        <v-container>
            <chat-app v-if="isAuthenticated" />
            <welcome-screen v-else />
        </v-container>
    </v-app>
</template>

<script>
    import WelcomeScreen from "@/components/Welcome";
    import ChatApp from "@/components/Chat";
    import { ActionTypes, GetterTypes } from "@/store/constants";

    export default {
        components: {
            WelcomeScreen,
            ChatApp
        },
        computed: {
            isAuthenticated() {
                return this.$store.getters[GetterTypes.IS_AUTHENTICATED];
            }
        },
        created() {
            this.$store.dispatch(ActionTypes.AUTHENTICATE);
            window.store = this.$store;///////
        }
    };
</script>
