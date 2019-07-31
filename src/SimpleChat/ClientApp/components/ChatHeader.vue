<template>
    <v-toolbar dark
               color="primary"
               clipped-left
               prominent
               flat>
        <v-toolbar-side-icon v-if="mini" @click.stop="raiseMenuButtonClick" />
        <v-spacer />
        <v-toolbar-title>Simple Chat</v-toolbar-title>
    </v-toolbar>
</template>

<script>
    export default {
        props: {
            mini: {
                type: Boolean,
                required: true
            }
        },
        methods: {
            raiseMenuButtonClick() {
                this.$emit("menuButtonClick", !this.mini);
            },
        }
    }
</script>
