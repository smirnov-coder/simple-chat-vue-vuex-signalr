<template>
    <v-card>
        <div>
            Чтобы начать общаться в чате, войдите на сайт с помощью аккаунта в социальной сети.
        </div>
        <v-divider />
        <facebook-button />
        <vkontakte-button />
        <linked-in-button />
        <odnoklassniki-button />

        <error-modal />
        <confirm-modal />
    </v-card>
</template>

<script>
    import VkontakteButton from "@/components/VkontakteButton";
    import FacebookButton from "@/components/FacebookButton";
    import LinkedInButton from "@/components/LinkedInButton";
    import OdnoklassnikiButton from "@/components/OdnoklassnikiButton";
    import ErrorModal from "@/components/ErrorModal";
    import ConfirmModal from "@/components/ConfirmModal";

    export default {
        components: {
            VkontakteButton,
            FacebookButton,
            LinkedInButton,
            OdnoklassnikiButton,
            ErrorModal,
            ConfirmModal,
        },
    };
</script>
