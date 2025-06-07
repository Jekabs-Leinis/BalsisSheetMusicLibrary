<script setup>
import { login } from "@/services/authenticationService";
import { ref } from "vue";
import router from "@/router/routes";

let userName = ref(""),
  password = ref(""),
  showError = ref(false),
  errorMessage = ref(""),
  loading = ref(false);

async function attemptLogin() {
  if (loading.value) {
    return;
  }

  loading.value = true;
  showError.value = false;

  let response = await login(userName.value, password.value);

  loading.value = false;
  showError.value = !response.success;
  errorMessage.value = response.error;
  
  if (response.success) {
    router.push({ name: "SheetList" });
  }
}
</script>

<template>
  <div class="login-bg">
    <div
      class="d-flex justify-content-center flex-column align-items-baseline login-form mx-auto h-100"
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
      />

      <div class="info mt-3" v-if="showError">{{ errorMessage }}</div>

      <div class="w-100">
        <button
          v-loading="loading"
          class="mt-3"
          id="submit"
          name="submit"
          value="Login"
          @click="attemptLogin"
        >
          Submit
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-form {
  max-width: 300px;
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
  font-family: Courier New;
  color: black;
  background: white;
  text-decoration: none;
  border: none;
  padding: 8px 12px;
  float: right;
}
</style>
