import axios from "axios";
import { User } from "@/models/userModels";

export async function login(userName, password) {
  const response = await axios.post("/api/authentication/login", { userName, password });
  if (!response.data.success) {
    throw new Error(response.data.error || "Login failed");
  }

  return new User(response.data.model);
}

export async function logout() {
  const response = await axios.post("/api/authentication/logout");

  if (!response.data.success) {
    throw new Error(response.data.error || "Logout failed");
  }

  return response.data;
}