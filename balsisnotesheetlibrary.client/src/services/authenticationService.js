import { useUserStore } from "@/stores/userStore";
import { User } from "@/models/userModels";
import axios from "axios";

export async function login(userName, password) {
  const userStore = useUserStore();
  
  const response = await axios.post("/api/authentication/login", { userName, password });

  if (response.data.success) {
    userStore.setUser(new User(response.data.model));
  }

  return response.data;
}

export async function logout() {
  const userStore = useUserStore();

  const response = await axios.post("/api/authentication/logout");

  if (response.data.success) {
    userStore.logout();
    
    window.location.href = "/login";
  }

  return response.data;
}
