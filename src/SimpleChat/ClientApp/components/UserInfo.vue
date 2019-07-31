<template>
    <v-list class="pa-1"
            two-line>
        <!-- User info -->
        <v-list-tile avatar
                     tag="div">
            <!-- Avatar -->
            <v-list-tile-avatar>
                <v-badge bottom
                         overlap
                         class="provider-badge">
                    <template v-slot:badge>
                        <v-icon color="white"
                                class="provider-badge__icon">
                            {{ badgeIcon }}
                        </v-icon>
                    </template>
                    <img :src="user.avatar" class="avatar" />
                </v-badge>
            </v-list-tile-avatar>
            <!-- Sign in info -->
            <v-list-tile-content>
                <v-list-tile-title class="d-flex">
                    {{ user.name }}
                </v-list-tile-title>
                <v-list-tile-sub-title class="sign-out-button"
                                       @click.stop="signOut">
                    Выйти
                </v-list-tile-sub-title>
            </v-list-tile-content>
            <!-- Left arrow button (collapse sidebar) -->
            <v-list-tile-action>
                <v-btn icon
                       @click.stop="raiseCollapseButtonClick">
                    <v-icon>mdi-chevron-left</v-icon>
                </v-btn>
            </v-list-tile-action>
        </v-list-tile>
    </v-list>
</template>

<script>
    import { clearToken } from "@/scripts/utils"
    import { ActionTypes, GetterTypes } from "@/store/constants";

    export default {
        props: {
            mini: {
                type: Boolean,
                required: true
            }
        },
        computed: {
            user() {
                return this.$store.getters[GetterTypes.CURRENT_USER]
            },
            badgeIcon() {
                switch (this.user.provider) {
                    case "Facebook": return "mdi-facebook";
                    case "LinkedIn": return "mdi-linkedin";
                    case "ВКонтакте": return "mdi-vk";
                    case "Одноклассники": return "mdi-odnoklassniki";
                    default: return "mdi-help";
                }
            },
            badgeBgColor() {
                switch (this.user.provider) {
                    case "Facebook": return "primary";
                    case "LinkedIn": return "primary";
                    case "ВКонтакте": return "primary";
                    case "Одноклассники": return "orange";
                    default: return "red";
                }
            }
        },
        methods: {
            raiseCollapseButtonClick() {
                this.$emit("collapseButtonClick", !this.mini);
            },
            signOut() {
                clearToken();
                this.$store.dispatch(ActionTypes.SIGN_OUT);
            }
        }
    }
</script>

<style>
    .provider-badge > .v-badge__badge {
        bottom: 2px !important;
        right: -4px !important;
    }
    .provider-badge__icon {
        font-size: 14px !important;
    }
    .sign-out-button {
        cursor: pointer;
    }
    .sign-out-button:hover {
        text-decoration: underline;
    }
    .avatar {
        max-height: 40px;
        max-width: 40px;
    }
</style>
