import Vue from "vue";
import Vuetify from "vuetify";
import App from "@/components/App";
import store from "@/store";
import "vuetify/dist/vuetify.css";
import "@mdi/font/css/materialdesignicons.css";

Vue.use(Vuetify, {
    iconfont: "mdi"
});
Vue.config.productionTip = false;

new Vue({
    el: "#app",
    store,
    render: h => h(App)
});
