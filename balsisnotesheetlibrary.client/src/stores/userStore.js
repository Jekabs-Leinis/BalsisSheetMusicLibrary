import { ref, computed } from "vue";
import { defineStore } from "pinia";
import { User } from "@/models/userModels";

export const useUserStore = defineStore("user", () => {
  const localUser = localStorage.getItem("currentUser");

  const currentUser = ref(new User(localUser ? JSON.parse(localUser) : {}));
  const isLoggedIn = computed(() => Boolean(currentUser.value?.id));

  const setUser = (user) => {
    currentUser.value = user;

    localStorage.setItem("currentUser", JSON.stringify(currentUser.value));
  };

  const logout = () => {
    currentUser.value = new User();
    localStorage.removeItem("currentUser");
  };

  return { currentUser, isLoggedIn, setUser, logout };
});
