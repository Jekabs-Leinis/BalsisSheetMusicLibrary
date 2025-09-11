import { createApp } from "vue";
import { createPinia } from "pinia";
import Toast, { POSITION } from "vue-toastification";

import App from "@/App.vue";
import router from "@/router/routes";
import { useUserStore } from "@/stores/userStore";
import vLoading from "@/directives/vLoading";
import axios from "axios";
import "@/assets/js/bootstrap";

import "@/assets/scss/main.scss";


// Redirect to login page if the user is not authenticated for XHR requests
axios.interceptors.response.use(
  (response) => response,
  (request) => {
      //TODO: remove
      console.log(request);
    if (request.response.status === 401 && request.response.headers.location.includes("login")) {
      window.location.href = "/login";
    }

    return Promise.reject(request);
  },
);

//TODO: figure out how to do this with only cookies, this is unsecure
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
