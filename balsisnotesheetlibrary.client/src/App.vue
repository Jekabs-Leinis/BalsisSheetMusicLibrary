<script setup>
import { RouterView } from "vue-router";
import VBackToTop from "@/components/Common/VBackToTop.vue";
import { useAuthStore } from "@/stores/authStore.js";

const authStore = useAuthStore();
</script>

<template>
  <div
    v-if="authStore.isLoading"
    v-loading="authStore.isLoading"
    class="loading-fullscreen"
  ></div>
  <template v-else>
    <RouterView />
    <VBackToTop />
  </template>
</template>

<style scoped>
.loading-fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}
</style>
