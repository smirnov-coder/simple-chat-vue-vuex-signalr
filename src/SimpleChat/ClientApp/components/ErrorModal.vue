<template>
    <v-dialog v-model="show"
              max-width="500"
              persistent>
        <v-card>
            <v-card-title>
                <v-icon color="red" left>mdi-emoticon-sad-outline</v-icon>
                Ошибка
            </v-card-title>
            <v-divider />
            <v-card-text v-html="message"></v-card-text>
            <v-divider />
            <v-card-actions>
                <v-spacer />
                <v-btn dark
                       color="primary"
                       @click.stop="close">
                    Закрыть
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script>
    import { ActionTypes, GetterTypes } from "@/store/constants";

    export default {
        data() {
            return {
                show: false
            };
        },
        methods: {
            close() {
                this.show = false;
                this.$store.dispatch(ActionTypes.ERROR_HANDLED);
            },
            changeTextSelection(value) {
                return value.replace(new RegExp("'(.*?)'"), "<b>$1</b>");
            }
        },
        computed: {
            error() {
                return this.$store.getters[GetterTypes.ERROR];
            },
            message() {
                return this.error ? this.changeTextSelection(this.error.message) : "";
            }
        },
        watch: {
            error(value) {
                if (value) {
                    this.show = true;
                } else {
                    this.show = false;
                }
            }
        },
    };
</script>
