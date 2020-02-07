<template>
    <v-list-tile class="py-1">
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
                <img :src="user.avatar" class="avatar">
            </v-badge>
        </v-list-tile-avatar>
        <v-list-tile-content>
            <v-list-tile-title class="text-truncate">
                {{ user.name }}
            </v-list-tile-title>
        </v-list-tile-content>
    </v-list-tile>
</template>

<script>
    export default {
        props: {
            user: {
                type: Object,
                required: true
            }
        },
        computed: {
            badgeIcon() {
                switch (this.user.provider) {
                    case "Facebook": return "mdi-facebook";
                    case "ВКонтакте": return "mdi-vk";
                    case "Одноклассники": return "mdi-odnoklassniki";
                    default: return "mdi-help";
                }
            },
            badgeBgColor() {
                switch (this.user.provider) {
                    case "Facebook": return "primary";
                    case "ВКонтакте": return "primary";
                    case "Одноклассники": return "orange";
                    default: return "red";
                }
            }
        }
    };
</script>

<style>

</style>
