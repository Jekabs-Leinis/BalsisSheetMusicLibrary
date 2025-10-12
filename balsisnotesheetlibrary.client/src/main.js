import { createApp } from "vue";
import { createPinia } from "pinia";
import Toast, { POSITION } from "vue-toastification";

import App from "@/App.vue";
import router from "@/router/routes";
import { useUserStore } from "@/stores/userStore";
import vLoading from "@/directives/vLoading";

// JS configurations
import "@/config/js/bootstrapConfig.js";
import "@/config/js/axiosConfig";
// SCSS styles
import "@/assets/scss/main.scss";

const app = createApp(App);
const pinia = createPinia();

// Make the store available for router hook setup
app.use(pinia);

const userStore = useUserStore();

router.beforeEach((to) => {
  if (userStore.isLoggedIn || to.name === "Login") {
    return;
  }

  return { name: "Login" };
});

app.use(router);
app.use(Toast, {
  position: POSITION.TOP_RIGHT,
  timeout: 5000,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: true,
});

app.directive("loading", vLoading);

app.mount("#app");
