<template>
    <v-navigation-drawer drawer
                         :mini-variant="mini"
                         permanent>
        <user-info :mini="mini"
                   @collapseButtonClick="raiseSizeChanged($event)">
        </user-info>
        <v-divider light />
        <user-list :mini="mini"></user-list>
    </v-navigation-drawer>
</template>

<script>
    import UserList from "@/components/UserList";
    import UserInfo from "@/components/UserInfo";

    export default {
        props: {
            mini: {
                type: Boolean,
                required: true
            }
        },
        components: {
            UserList,
            UserInfo
        },
        methods: {
            raiseSizeChanged(isCollapsed) {
                this.$emit("sizeChanged", isCollapsed);
            }
        }
    };
</script>
