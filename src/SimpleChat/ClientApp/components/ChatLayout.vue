<template>
    <v-card height="100%">
        <v-layout fill-height column>
            <v-flex shrink>
                <slot name="header"></slot>
            </v-flex>
            <v-layout grow>
                <v-flex shrink>
                    <slot name="aside"></slot>
                </v-flex>
                <v-layout column>
                    <v-flex ref="container" grow class="chat-layout__top-content pt-2">
                        <slot name="top-content"></slot>
                    </v-flex>
                    <v-divider light />
                    <v-flex shrink>
                        <slot name="bottom-content"></slot>
                    </v-flex>
                </v-layout>
            </v-layout>
            <v-flex shrink>
                <slot name="footer"></slot>
            </v-flex>
        </v-layout>
    </v-card>
</template>

<script>
    import { MutationTypes } from "@/scripts/constants";

    export default {
        data() {
            return {
                unsubscribe: null
            };
        },
        created() {
            this.unsubscribe = this.$store.subscribe(mutation => {
                if (mutation.type === MutationTypes.ADD_MESSAGE) {
                    this.$vuetify.goTo(9999, {
                        container: this.$refs.container
                    });
                }
            })
        },
        beforeDestroy() {
            if (this.unsubscribe) {
                this.unsubscribe();
            }
        }
    }
</script>

<style>
    .chat-layout__top-content {
        height: 1px;
        overflow-y: auto;
    }
</style>
