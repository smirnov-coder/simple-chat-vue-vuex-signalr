<template>
    <ul class="px-3 message-list">
        <li v-for="(message, index) in messages" :key="index">
            <message-item :message="message"></message-item>
        </li>
    </ul>
</template>

<script>
    import MessageItem from "@/components/MessageItem";
    import { GetterTypes } from "@/store/constants";

    export default {
        computed: {
            messages() {
                return this.$store.getters[GetterTypes.MESSAGES];
            }
        },
        components: {
            MessageItem
        }
    }
</script>

<style>
    .message-list {
        list-style: none;
        display: flex;
        flex-direction: column;
        flex-grow: 1;
    }
</style>
