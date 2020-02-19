<template>
    <chat-layout>
        <template v-slot:header>
            <chat-header :mini="mini"
                         @menuButtonClick="mini = $event">
            </chat-header>
        </template>
        <template v-slot:aside>
            <side-bar :mini="mini"
                      @sizeChanged="mini = $event">
            </side-bar>
        </template>
        <template v-slot:top-content>
            <message-list />
        </template>
        <template v-slot:bottom-content>
            <chat-controls />
        </template>
        <template v-slot:footer>
            <chat-footer />
        </template>
    </chat-layout>
</template>

<script>
    import ChatLayout from "@/components/ChatLayout";
    import SideBar from "@/components/Sidebar";
    import ChatHeader from "@/components/ChatHeader";
    import MessageList from "@/components/MessageList";
    import ChatControls from "@/components/ChatControls";
    import ChatFooter from "@/components/ChatFooter";
    import { ActionTypes } from "@/scripts/constants";

    export default {
        components: {
            SideBar,
            ChatHeader,
            MessageList,
            ChatControls,
            ChatFooter,
            ChatLayout
        },
        data() {
            return {
                mini: false,
            }
        },
        created() {
            this.$store.dispatch(ActionTypes.CONNECT_HUB);
        },
    };
</script>
