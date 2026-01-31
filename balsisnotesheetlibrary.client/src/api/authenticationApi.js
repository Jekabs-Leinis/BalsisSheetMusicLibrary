import axios from "axios";
import { User } from "@/models/userModels";
import { ResponseError } from "@/models/errorModels.js";


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
    
    throw new ResponseError(e, "Login failed");
  }
}

export async function logout() {
  try {
    const response = await axios.post("/api/authentication/logout");

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Logout failed");
  }
}

export async function getCurrentUser() {
  try {
    const response = await axios.get("/api/authentication/getCurrentUser");
    
    return new User(response.data);
  }
  catch (e) {
    if (e.response && e.response.status === 401) {
      return null; // Not logged in
    }

    throw new ResponseError(e, "Failed to get current user");
  }
}