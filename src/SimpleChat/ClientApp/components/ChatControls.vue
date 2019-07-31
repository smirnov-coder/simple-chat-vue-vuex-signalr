<template>
    <v-form>
        <v-layout pa-3>
            <v-textarea ref="textArea"
                        autofocus
                        no-resize
                        label="Ваше сообщение"
                        clearable
                        rows="1"
                        counter="300"
                        maxLength="300"
                        v-model.trim="text">
                <template v-slot:append-outer>
                    <v-btn bottom
                           fab
                           dark
                           color="primary"
                           title="Отправить"
                           @click.stop="sendMessage">
                        <v-icon>mdi-send</v-icon>
                    </v-btn>
                </template>
            </v-textarea>
        </v-layout>
    </v-form>
</template>

<script>
    import { Message } from "@/store/modules/messages";
    import { ActionTypes, GetterTypes } from "@/store/constants";

    export default {
        data() {
            return {
                text: ""
            };
        },
        computed: {
            user() {
                return this.$store.getters[GetterTypes.CURRENT_USER];
            }
        },
        methods: {
            sendMessage() {
                if (this.text) {
                    let message = new Message({
                        name: this.user.name,
                        avatar: this.user.avatar
                    }, this.text);
                    //console.log("message", message);///
                    this.$store.dispatch(ActionTypes.SEND_MESSAGE, message);
                    this.text = "";
                }
                this.$refs.textArea.focus();
            }
        }
    }
</script>
