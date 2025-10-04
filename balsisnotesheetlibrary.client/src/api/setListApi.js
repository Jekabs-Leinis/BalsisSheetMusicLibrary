import axios from "axios";
import { SetList } from "@/models/sheetModels";

export async function getAllSetLists(withNoteSheets = false) {
  try {
    const response = await axios.get(`/api/setList/getAll`, { params: { withNoteSheets } });

    return response.data.map((setList) => new SetList(setList));
  } catch (e) {
    throw new Error(e.message || "Failed to get all set lists");
  }
}

export async function getAllArchivedSetLists() {
  try {
    const response = await axios.get(`/api/setList/getAllArchived`);

    return response.data.map((setList) => new SetList(setList));
  } catch (e) {
    throw new Error(e.message || "Failed to get all set lists");
  }
}

export async function addSetList(setList) {
  try {
    const response = await axios.post("/api/setList/add", setList);

    return new SetList(response.data);
  } catch (e) {
    throw new Error(e.message || "Failed to add set list");
  }
}

// Do not use this function directly, use the setListStore.saveSetList() instead
export async function updateSetList(setList) {
  try {
    const response = await axios.post("/api/setList/update", setList);

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Failed to update set list");
  }
}

export async function updateSetListOrder(setList) {
  try {
    const response = await axios.post("/api/setList/updateOrder", setList);

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Failed to update set list order");
  }
}

export async function deleteSetList(setListId) {
  try {
    const response = await axios.delete(`/api/setList/delete/${setListId}`);

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Failed to delete set list");
  }
}

export async function archiveSetList(setListId) {
  try {
    const response = await axios.post(`/api/setList/archive/${setListId}`);

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Failed to archive set list");
  }
}

export async function unarchiveSetList(setListId) {
  try {
    const response = await axios.post(`/api/setList/unarchive/${setListId}`);

    return response.data;
  } catch (e) {
    throw new Error(e.message || "Failed to unarchive set list");
  }
}
