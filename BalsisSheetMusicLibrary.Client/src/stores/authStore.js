import { defineStore } from "pinia";
import {
  getCurrentUser,
  logout as apiLogout,
} from "@/api/authenticationApi.js";
import { User } from "@/models/userModels.js";
import { ref } from "vue";

export const useAuthStore = defineStore("auth", () => {
  /** @type {import('vue').Ref<User|null>} */
  const user = ref(null);
  /** @type {import('vue').Ref<boolean>} */
  const isAuthenticated = ref(false);
  /** @type {import('vue').Ref<boolean>} */
  const isLoading = ref(false);

  async function checkAuthStatus() {
    try {
      this.isLoading = true;
      const user = await getCurrentUser();

      if (user) {
        this.user = new User(user);
        this.isAuthenticated = true;
      } else {
        // Not authenticated - this is expected, not an error
        this.user = null;
        this.isAuthenticated = false;
      }
    } catch (error) {
      // Only throw on unexpected errors (network issues, etc.)
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
