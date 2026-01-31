import axios from "axios";
import { SetList } from "@/models/sheetModels";
import { ResponseError } from "@/models/errorModels.js";

export async function getAllSetLists(withNoteSheets = false) {
  try {
    const response = await axios.get(`/api/setList/getAll`, {
      params: { withNoteSheets },
    });

    return response.data.map((setList) => new SetList(setList));
  } catch (e) {
    throw new ResponseError(e, "Failed to get all set lists");
  }
}

export async function getAllArchivedSetLists() {
  try {
    const response = await axios.get(`/api/setList/getAllArchived`);

    return response.data.map((setList) => new SetList(setList));
  } catch (e) {
    throw new ResponseError(e, "Failed to get all set lists");
  }
}

export async function addSetList(setList) {
  try {
    const response = await axios.post("/api/setList/add", setList);

    return new SetList(response.data);
  } catch (e) {
    throw new ResponseError(e, "Failed to add set list");
  }
}

// Do not use this function directly, use the setListStore.saveSetList() instead
export async function updateSetList(setList) {
  try {
    const response = await axios.post("/api/setList/update", setList);

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to update set list");
  }
}

export async function moveSetList(id, newOrder) {
  try {
    const response = await axios.post("/api/setList/move", { id, newOrder });

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to update set list order");
  }
}

export async function deleteSetList(setListId) {
  try {
    const response = await axios.delete(`/api/setList/delete/${setListId}`);

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to delete set list");
  }
}

export async function archiveSetList(setListId) {
  try {
    const response = await axios.post(`/api/setList/archive/${setListId}`);

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to archive set list");
  }
}

export async function restoreSetList(setListId) {
  try {
    const response = await axios.post(`/api/setList/restore/${setListId}`);

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to restore set list");
  }
}