<template>
    <v-list dense>
        <v-list-tile v-if="mini">
            <v-list-tile-avatar>
                <v-badge v-if="showBadge"
                         small
                         overlap>
                    <template v-slot:badge>
                        <span>{{ users.length | shrink }}</span>
                    </template>
                    <v-icon large>mdi-account-multiple</v-icon>
                </v-badge>
                <v-icon v-else large>mdi-account-multiple</v-icon>
            </v-list-tile-avatar>
        </v-list-tile>
        <user-item v-else
                   v-for="(user, index) in users"
                   :key="index"
                   :user="user">
        </user-item>
    </v-list>
</template>

<script>
    import UserItem from "@/components/UserItem";
    import { GetterTypes } from "@/store/constants";

    export default {
        props: {
            mini: {
                type: Boolean,
                required: true
            }
        },
        components: {
            UserItem
        },
        computed: {
            users() {
                return this.$store.getters[GetterTypes.USERS];
            },
            showBadge() {
                return this.users.length > 0;
            }
        },
        filters: {
            shrink(value) {
                return parseInt(value) > 99 ? "99+" : value;
            }
        }
    };
</script>
