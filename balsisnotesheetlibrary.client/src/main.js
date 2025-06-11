import { createApp } from "vue";
import { createPinia } from "pinia";
import { createBootstrap } from 'bootstrap-vue-next'

import App from "@/App.vue";
import router from "@/router/routes";
import { useUserStore } from "@/stores/userStore";
import vLoading from "@/directives/vLoading";
import axios from "axios";

// Mandatory import to initialize bs components
// eslint-disable-next-line no-unused-vars
import * as bootstrap from "bootstrap";

import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue-next/dist/bootstrap-vue-next.css'
import "./assets/main.css";


// Redirect to login page if the user is not authenticated for XHR requests
axios.interceptors.response.use(
  (response) => response,
  (request) => {
      console.log(request);
    if (request.response.status === 401 && request.response.headers.location.includes("login")) {
      window.location.href = "/login";
    }

    return Promise.reject(request);
  },
);

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



const app = createApp(App);
const pinia = createPinia();

app.use(pinia);

const userStore = useUserStore();

router.beforeEach((to) => {
  if (userStore.isLoggedIn || to.name === "Login") {
    return;
  }

  return { name: "Login" };
});

app.use(router);
app.use(createBootstrap());

app.directive("loading", vLoading);

app.mount("#app");


