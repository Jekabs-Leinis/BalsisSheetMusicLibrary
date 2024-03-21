import { useUserStore } from "@/stores/userStore";
import { User } from "@/models/userModels";
import axios from "axios";

export async function login(email, password) {
  const userStore = useUserStore();

  const response = await axios.post("/Api/Authentication/Login", { email, password });

  if (response.data.success) {
    userStore.setUser(new User(response.data.model));
  }

  return response.data;
}
