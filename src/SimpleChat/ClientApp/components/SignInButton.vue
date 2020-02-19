<template>
    <v-btn dark
           :color="buttonColor"
           @click.stop="signIn">
        <v-icon left light>{{ icon }}</v-icon>
        Вход через {{ provider }}
    </v-btn>
</template>

<script>
    import { ActionTypes } from "@/scripts/constants";
    import { ProviderHelper } from "@/scripts/provider-helper";

    export default {
        props: {
            provider: {
                type: String,
                required: true
            }
        },
        data() {
            return {
                helper: new ProviderHelper(this.provider)
            }
        },
        computed: {
            buttonColor() {
                return this.helper.getColor();
            },
            icon() {
                return this.helper.getIcon();
            },
            uri() {
                return this.helper.getPopupUri()
            }
        },
        methods: {
            signIn() {
                let
                    provider = this.provider,
                    uri = this.uri;
                this.$store.dispatch(ActionTypes.SIGN_IN, { uri, provider });
            }
        }
    };
</script>
