<template>
    <v-dialog v-model="show"
              max-width="500"
              persistent>
        <v-card>
            <v-card-title>
                <v-icon left color="orange">mdi-alert-circle-outline</v-icon>
                Внимание
            </v-card-title>
            <v-divider />
            <v-card-text>
                Похоже, Вы впервые входите на наш сайт через <b>{{ provider }}</b>.
                На Ваш адрес электронной почты <b>{{ email }}</b> было отправлено письмо с кодом подтверждения.
                Пожалуйста, введите код подтверждения и нажмите <b>Продолжить</b>.
                <v-text-field v-model="code"
                              label="Код подтверждения"
                              autofocus
                              class="mt-3">
                </v-text-field>
            </v-card-text>
            <v-divider />
            <v-card-actions>
                <v-spacer />
                <v-btn @click.stop="cancel">
                    Отмена
                </v-btn>
                <v-btn dark
                       color="primary"
                       @click.stop="continueSignIn">
                    Продолжить
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script>
    import { ActionTypes, GetterTypes } from "@/scripts/constants";

    export default {
        data() {
            return {
                show: false,
                code: ""
            };
        },
        methods: {
            cancel() {
                this.show = false;
                this.code = false;
                this.$store.dispatch(ActionTypes.CANCEL_SIGN_IN);
            },
            continueSignIn() {
                this.show = false;
                this.$store.dispatch(ActionTypes.CONFIRM_SIGN_IN, {
                    code: this.code,
                    sessionId: this.confirmationData.sessionId
                });
                this.code = "";
            }
        },
        computed: {
            confirmationData() {
                return this.$store.getters[GetterTypes.CONFIRMATION_DATA];
            },
            email() {
                return this.confirmationData ? this.confirmationData.email : "";
            },
            provider() {
                return this.confirmationData ? this.confirmationData.provider : "";
            }
        },
        watch: {
            confirmationData(value) {
                if (value) {
                    this.show = true;
                } else {
                    this.show = false;
                }
            }
        }
    };
</script>
