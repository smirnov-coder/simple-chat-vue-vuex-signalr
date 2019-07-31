<template>
    <v-card class="mb-2 pa-2">
        <v-layout>
            <v-flex shrink>
                <v-avatar size="36" tile>
                    <v-img :src="message.author.avatar" :alt="message.author.name" />
                </v-avatar>
            </v-flex>
            <v-flex ml-2>
                <strong>{{ message.author.name }}: </strong>
                <span class="message-item__text">{{ message.text }}</span>
            </v-flex>
        </v-layout>
    </v-card>
</template>

<script>
    export default {
        props: {
            message: {
                type: Object,
                required: true
            }
        },
    }
</script>

<style>
    .message-item__text {
        word-break: break-word;
        white-space: pre-line;
    }
</style>
