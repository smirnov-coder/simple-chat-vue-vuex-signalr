<template>
    <v-card height="100%">
        <v-layout fill-height wrap>
            <v-flex xs6>
                left content
            </v-flex>
            <v-flex xs6>
                <sign-in-area />
            </v-flex>
        </v-layout>
    </v-card>
</template>
<script>
    import SignInArea from "@/components/SignInArea";

    export default {
        components: {
            SignInArea,
        }
    };
</script>
