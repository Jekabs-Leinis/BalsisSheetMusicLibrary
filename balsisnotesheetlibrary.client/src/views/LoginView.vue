<script setup>
import { login } from "@/services/authenticationService";
import { ref } from "vue";

let email = ref(""),
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

  let response = await login(email.value, password.value);

  loading.value = false;
  showError.value = !response.success;
  errorMessage.value = response.error;
}
</script>

<template>
  <div class="login-bg">
    <div
      class="d-flex justify-content-center flex-column align-items-baseline login-form mx-auto h-100"
    >
      <div class="info mb-3">Please log in to access this page.</div>
      <h1 class="fw-normal">Nošu arhīvs</h1>
      <input
        style="display: none"
        id="csrf_token"
        name="csrf_token"
        type="hidden"
        value="1706834653.65##84a26a04af581aaf35c490c040cbee4e5cb328a7"
      />
      <label for="email" class="mt-3">Email Address</label>
      <input
        class="form-control form-control-sm"
        id="email"
        name="email"
        type="text"
        value=""
        v-model="email"
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
