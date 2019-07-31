<template>
    <v-card height="100%">
        <v-layout fill-height column>
            <slot name="header"></slot>
            <v-layout>
                <v-flex shrink>
                    <slot name="aside"></slot>
                </v-flex>
                <v-layout column>
                    <v-flex ref="container" grow class="pt-2 chat-layout__top-content">
                        <slot name="top-content"></slot>
                    </v-flex>
                    <v-divider light />
                    <v-flex shrink>
                        <slot name="bottom-content"></slot>
                    </v-flex>
                </v-layout>
            </v-layout>
            <slot name="footer"></slot>
        </v-layout>
    </v-card>
</template>

<script>
    import { MutationTypes } from "@/store/constants";

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
