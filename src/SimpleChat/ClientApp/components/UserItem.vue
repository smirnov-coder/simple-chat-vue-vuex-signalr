<template>
    <v-list-tile class="py-1">
        <v-list-tile-avatar>
            <v-badge :color="badgeColor"
                     class="provider-badge"
                     bottom
                     overlap>
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
    import { ProviderHelper } from "@/scripts/provider-helper";

    export default {
        props: {
            user: {
                type: Object,
                required: true
            }
        },
        computed: {
            helper() {
                return new ProviderHelper(this.user.provider);
            },
            badgeIcon() {
                return this.helper.getIcon();
            },
            badgeColor() {
                return this.helper.getColor();
            }
        }
    };
</script>

<style>

</style>
