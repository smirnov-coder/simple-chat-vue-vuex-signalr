<template>
    <v-layout fill-height
              justify-center
              align-center
              column>
        <v-card width="400px"
                hover
                color="primary">
            <v-img src="images/chat.svg"
                   height="200px"
                   contain>
            </v-img>
            <v-layout justify-center>
                <v-flex shrink
                        class="mb-3 px-3 headline font-weight-bold text-xs-center white--text"
                        align-self-end>
                    Добро пожаловать в SimpleChat!
                </v-flex>
            </v-layout>
            <v-layout>
                <sign-in-area />
            </v-layout>
        </v-card>
    </v-layout>
</template>

<script>
    import SignInArea from "@/components/SignInArea";

    export default {
        components: {
            SignInArea,
        }
    };
</script>
