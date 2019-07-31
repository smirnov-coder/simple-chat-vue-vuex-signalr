import Vue from "vue";
import Vuex from "vuex";
import messages from "@/store/modules/messages";
import users from "@/store/modules/users";
import auth from "@/store/modules/auth";
import hub from "@/store/modules/hub";

Vue.use(Vuex);
//Vue.config.devtools = process.env.NODE_ENV === "development";

export default new Vuex.Store({
    modules: {
        messages,
        users,
        auth,
        hub
    }
});
