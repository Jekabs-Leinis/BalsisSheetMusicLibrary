<script setup>
import { login } from "@/api/authenticationApi";
import { ref } from "vue";
import router from "@/router/routes";
import { useUserStore } from "@/stores/userStore";

let userName = ref("");
let password = ref("");
let errorMessage = ref("");
let loading = ref(false);
let userStore = useUserStore();

async function attemptLogin() {
  if (loading.value) {
    return;
  }

  loading.value = true;
  errorMessage.value = "";

  try {
    let user = await login(userName.value, password.value);

    userStore.setUser(user);

    router.push({ name: "SheetListView" });
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="login-bg flex-column h-100">
    <div
      class="d-flex justify-content-center flex-column align-items-baseline login-form mx-auto"
    >
      <div class="info mb-3">Please log in to access this page.</div>
      <h1 class="fw-normal">Nošu arhīvs</h1>
      <label for="userName" class="mt-3">Username</label>
      <input
        class="form-control form-control-sm"
        id="userName"
        name="userName"
        type="text"
        value=""
        v-model="userName"
      />

      <label for="password" class="mt-3">Password</label>
      <input
        class="form-control form-control-sm"
        id="password"
        name="password"
        type="password"
        value=""
        v-model="password"
        @keydown.enter="attemptLogin"
      />

      <div class="info mt-3 text-white" v-if="errorMessage">
        {{ errorMessage }}
      </div>

      <div class="w-100">
        <button
          v-loading.bg="loading"
          class="mt-3"
          id="submit"
          name="submit"
          value="Login"
          @click="attemptLogin"
          @keydown.enter="attemptLogin"
        >
          Submit
        </button>
      </div>
    </div>
    <div class="flex-grow-1" />
  </div>
</template>

<style scoped>
.login-form {
  max-width: 300px;
  height: 100%;
}

.login-bg {
  background-color: #c0392b;
  color: white;
  position: fixed;
  width: 100%;
  height: 100%;
}

.info {
  list-style: none;
  margin-left: -40px;
  color: black;
}

#submit {
  font-family:
    Courier New,
    sans-serif;
  color: black;
  background: white;
  text-decoration: none;
  border: none;
  padding: 8px 12px;
  float: right;
}

/** Mobile styles to handle on-screen keyboard reducing viewport height */
@media (max-width: 767px) {
  /* Make the background container allow scrolling if the viewport is reduced by the keyboard */
  .login-bg {
    position: absolute; 
    display: flex; 
    align-items: flex-start; 
    overflow: auto;
    -webkit-overflow-scrolling: touch; /* smooth scrolling for iOS Safari */
    padding-top: 5vh;
    height: calc(100vh - 5vh);
  }
  
  .login-form {
    position: relative;
    margin: 0 auto;
    max-width: 320px;
    width: calc(100% - 32px);
    height: auto;
    max-height: 100vh;
    overflow-y: auto;
    -webkit-overflow-scrolling: touch;
  }
  
  .login-form input,
  .login-form button {
    scroll-margin-top: 10vh;
  }
  
  .login-form .info {
    margin-left: 0;
  }
}
</style>