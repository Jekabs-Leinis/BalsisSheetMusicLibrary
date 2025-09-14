import axios from "axios";
import { SetList } from "@/models/sheetModels";

export async function getAllSetLists(withSheets = false) {
  const response = await axios.get(`/api/setList/getAll?withSheets=${withSheets}`);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to get all set lists");
  }

  return response.data.data.map((setList) => new SetList(setList));
}

export async function addSetList(setList) {
  const response = await axios.post("/api/setList/add", setList);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to add set list");
  }
  
  return response.data;
}

// Do not use this function directly, use the setListStore.saveSetList() instead
// as it handles potential errors.
export async function updateSetList(setList) {
  const response = await axios.post("/api/setList/update", setList);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to update set list");
  }
  
  return response.data;
}

export async function deleteSetList(setListId) {
  const response = await axios.delete(`/api/setList/delete/${setListId}`);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to delete set list");
  }
  
  return response.data;
}

export async function archiveSetList(setListId) {
  const response = await axios.post(`/api/setList/archive/${setListId}`);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to archive set list");
  }
  
  return response.data;
}

export async function unarchiveSetList(setListId) {
  const response = await axios.post(`/api/setList/unarchive/${setListId}`);

  if (!response.data.success) {
    throw Error(response.data.message || "Failed to unarchive set list");
  }
  
  return response.data;
}
