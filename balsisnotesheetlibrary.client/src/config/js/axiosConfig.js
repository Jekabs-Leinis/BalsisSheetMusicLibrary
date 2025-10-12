import axios from "axios";

axios.defaults.withCredentials = true;

// Initial request to get the anti-forgery token cookie
axios.get("api/antiforgery/token");

// Redirect to login page if 401 response is received and not already on the login page
axios.interceptors.response.use(
  (response) => response,
  (request) => {
    if (
      request.response.status === 401 &&
      window.location.pathname !== "/login" &&
      request.response.headers?.location?.includes("login")
    ) {
      window.location.href = "/login";
    }

    return Promise.reject(request);
  },
);
