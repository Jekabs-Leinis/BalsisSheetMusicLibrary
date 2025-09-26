import axios from "axios";
import { SetList } from "@/models/sheetModels";
import { BaseDto } from "@/models/commonModels.js";

export async function getAllSetLists(withSheets = false, withArchived = false) {
  const response = BaseDto.fromResponse(
    await axios.get(`/api/setList/getAll`, {
      params: {
        withSheets,
        withArchived,
      },
    }),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to get all set lists");
  }

  return response.data.map((setList) => new SetList(setList));
}

export async function addSetList(setList) {
  const response = BaseDto.fromResponse(
    await axios.post("/api/setList/add", setList),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to add set list");
  }

  return new SetList(response.data);
}

// Do not use this function directly, use the setListStore.saveSetList() instead
export async function updateSetList(setList) {
  const response = BaseDto.fromResponse(
    await axios.post("/api/setList/update", setList),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to update set list");
  }

  return response.data;
}

export async function updateSetListOrder(setList) {
  const response = BaseDto.fromResponse(
    await axios.post("/api/setList/updateOrder", setList),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to update set list order");
  }

  return response.data;
}

export async function deleteSetList(setListId) {
  const response = BaseDto.fromResponse(
    await axios.delete(`/api/setList/delete/${setListId}`),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to delete set list");
  }

  return response.data;
}

export async function archiveSetList(setListId) {
  const response = BaseDto.fromResponse(
    await axios.post(`/api/setList/archive/${setListId}`),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to archive set list");
  }

  return response.data;
}

export async function unarchiveSetList(setListId) {
  const response = BaseDto.fromResponse(
    await axios.post(`/api/setList/unarchive/${setListId}`),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to unarchive set list");
  }

  return response.data;
}
