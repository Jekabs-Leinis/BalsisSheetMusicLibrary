import { createApp } from "vue";
import { createPinia } from "pinia";
import Toast, { POSITION } from "vue-toastification";

import App from "@/App.vue";
import router from "@/router/routes";
import { useAuthStore } from "@/stores/authStore.js";
import vLoading from "@/directives/vLoading";

// JS configurations
import "@/config/js/bootstrapConfig.js";
import "@/config/js/axiosConfig";
// SCSS styles
import "@/assets/scss/main.scss";

const app = createApp(App);
const pinia = createPinia();

// Make auth store available for router hook setup
app.use(pinia);

const authStore = useAuthStore();
router.beforeEach(async (to) => {
  if (to.name !== "Login" && !authStore.isAuthenticated) {
    await authStore.checkAuthStatus();

    if (!authStore.isAuthenticated) {
      return { name: "Login" };
    }
  }

  if (to.path.startsWith('/admin') && !authStore.user?.isAdmin) {
    return { name: "SheetListView" };
  }
});

router.afterEach(() => {
  // Bootstrap offcanvas component doesn't restore body scroll on route change,
  // so we have to do it manually, otherwise the page remains unscrollable
  if (document.body.style.overflow === "hidden") {
    document.body.style.overflow = "";
  }
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
