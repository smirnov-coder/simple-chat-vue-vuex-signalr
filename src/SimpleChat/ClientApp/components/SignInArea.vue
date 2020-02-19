<template>
    <v-flex class="sign-in-area pa-4">
        <div class="mb-3 text-xs-center font-weight-medium">
            Чтобы начать общаться, войдите на сайт с помощью аккаунта в социальной сети.
        </div>
        <v-layout justify-center
                  column>
            <sign-in-button :provider="facebook"></sign-in-button>
            <sign-in-button :provider="vkontakte"></sign-in-button>
            <sign-in-button :provider="odnoklassniki"></sign-in-button>
        </v-layout>
    </v-flex>
</template>

<script>
    import SignInButton from "@/components/SignInButton";
    import { Provider } from "@/scripts/provider-helper";

    export default {
        components: {
            SignInButton
        },
        data() {
            return {
                facebook: Provider.FACEBOOK,
                vkontakte: Provider.VKONTAKTE,
                odnoklassniki: Provider.ODNOKLASSNIKI
            }
        }
    };
</script>

<style>
    .sign-in-area {
        width: 400px;
        background-color: white;
        flex-grow: 0 !important;
    }
</style>
