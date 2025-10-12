import axios from "axios";
import { User } from "@/models/userModels";

export async function login(userName, password) {
  try {
    const response = await axios.post("/api/authentication/login", {
      userName,
      password,
    });

    return new User(response.data);
  } catch (e) {
    if (e.response && e.response.status === 401) {
      throw new Error("Nepareizi ievadīts lietotājvārds vai parole");
    }
    
    throw new Error(e.message || "Login failed");
  }
}

export async function logout() {
  try {
    const response = await axios.post("/api/authentication/logout");

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Logout failed");
  }
}
