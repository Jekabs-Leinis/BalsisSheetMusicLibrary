import { defineStore } from "pinia";
import { getCurrentUser, logout as apiLogout } from "@/api/authenticationApi.js";
import { User } from "@/models/userModels.js";
import { ref } from "vue";

export const useAuthStore = defineStore("auth", () => {
  /** @type {import('@/models/userModels').User|null} */
  const user = ref(null);
  const isAuthenticated = ref(false);
  const isLoading = ref(true);

  async function checkAuthStatus() {
    try {
      const response = await getCurrentUser();
      this.user = new User(response.data);
      this.isAuthenticated = true;
    } catch (error) {
      this.user = null;
      this.isAuthenticated = false;
      
      throw error;
    } finally {
      this.isLoading = false;
    }
  }
  
  function setUser(user) {
    this.user = user;
    this.isAuthenticated = Boolean(user);
  }

  async function logout() {
    await apiLogout();
    this.user = null;
    this.isAuthenticated = false;
  }

  return {
    user,
    isAuthenticated,
    isLoading,
    checkAuthStatus,
    setUser,
    logout,
  };
});
