import axios from "axios";
import { User } from "@/models/userModels";
import { BaseDto } from "@/models/commonModels.js";

export async function login(userName, password) {
  const response = BaseDto.fromResponse(
    await axios.post("/api/authentication/login", { userName, password }),
  );
  if (!response.success) {
    throw new Error(response.message || "Login failed");
  }

  return new User(response.data);
}

export async function logout() {
  const response = BaseDto.fromResponse(
    await axios.post("/api/authentication/logout"),
  );

  if (!response.success) {
    throw new Error(response.message || "Logout failed");
  }

  return response.data;
}
