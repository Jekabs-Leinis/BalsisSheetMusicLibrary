import axios from "axios";
import { User } from "@/models/userModels.js";

export async function getAllUsers() {
  try {
    const response = await axios.get(`/api/user/getAll`);

    return response.data.map((setList) => new User(setList));
  } catch (e) {
    throw new Error(e.message || "Failed to get all users");
  }
}

export async function changeUserPassword(userName, newPassword) {
  try {
    await axios.post(`/api/authentication/changePassword`, {
      userName,
      newPassword,
    });
  } catch (e) {
    throw new Error(e.message || "Failed to change user password");
  }
}
