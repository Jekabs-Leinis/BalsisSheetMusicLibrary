<script setup>
import { RouterView } from "vue-router";
import VBackToTop from "@/components/Common/VBackToTop.vue";
import { onMounted } from "vue";
import { useAuthStore } from "@/stores/authStore.js";

const authStore = useAuthStore();

onMounted(() => {
  authStore.checkAuthStatus().catch((error) => {
    console.error("Error checking auth status:", error);
  });
})

</script>

<template>
  <div v-if="authStore.isLoading" v-loading="authStore.isLoading" />
  <template v-else>
    <RouterView />
    <VBackToTop />
  </template>
</template>
