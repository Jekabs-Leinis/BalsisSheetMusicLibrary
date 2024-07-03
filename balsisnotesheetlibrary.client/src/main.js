import "./assets/main.css";

// Mandatory import to initialize bs components
// eslint-disable-next-line no-unused-vars
import * as bootstrap from "bootstrap";

import axios from "axios";

function getCsrfToken() {
  return document.cookie
    .split("; ")
    .find((row) => row.startsWith("CSRF-TOKEN="))
    ?.split("=")[1];
}

function setAxiosCsrfToken() {
  axios.defaults.headers.common["X-CSRF-TOKEN"] = getCsrfToken();
}

// If the token is found, set it in the axios headers
if (getCsrfToken()) {
  setAxiosCsrfToken();
} else {
  axios
    .get("/api/csrf/getToken")
    .then(() => {
      // Token is set in the cookie, we can now set it in the axios headers
      setAxiosCsrfToken();
    })
    .catch((e) => {
      console.error(e);
      console.error("Could not get CSRF token.");
    });
}

import { createApp } from "vue";
import { createPinia } from "pinia";

import router from "@/router/routes";

import { useUserStore } from "@/stores/userStore";

import vLoading from "@/directives/vLoading";

import App from "@/App.vue";

const app = createApp(App);

app.use(createPinia());

const userStore = useUserStore();

router.beforeEach((to) => {
  if (userStore.isLoggedIn || to.name === "login") {
    return;
  }

  return { name: "Login" };
});

app.use(router);

app.directive("loading", vLoading);

app.mount("#app");
