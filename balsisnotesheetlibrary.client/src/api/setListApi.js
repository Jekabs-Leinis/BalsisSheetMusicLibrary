import axios from "axios";
import { SetList } from "@/models/sheetModels";

export async function getAllSetLists() {
  const response = await axios.get("/api/setList/getAll");

  if (!response.data.success) {
    console.error("Failed to get all set lists", response.data.error);
    return [];
  }

  return response.data.model.map((setList) => new SetList(setList));
}

export async function addSetList(setList) {
  const response = await axios.post("/api/setList/add", setList);
  return response.data;
}

export async function updateSetList(setList) {
  const response = await axios.post("/api/setList/update", setList);
  return response.data;
}

export async function deleteSetList(setListId) {
  const response = await axios.delete(`/api/setList/delete/${setListId}`);
  return response.data;
}

