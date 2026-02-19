import axios from "axios";
import { SheetMusic } from "@/models/sheetModels";
import { ResponseError } from "@/models/errorModels.js";

export async function getAllSheetMusic() {
  try {
    const response = await axios.get("/api/sheetMusic/getAll");

    return response.data.map((sheetMusic) => new SheetMusic(sheetMusic));
  } catch (e) {
    throw new ResponseError(e, "Failed to get all sheet music");
  }
}

export async function createSheetMusic(sheetMusic, file) {
  const formData = new FormData();

  for (const key in sheetMusic) {
    if (
      Object.prototype.hasOwnProperty.call(sheetMusic, key) &&
      sheetMusic[key]
    ) {
      formData.append(key, sheetMusic[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }
  try {
    const response = await axios.post("/api/sheetMusic/add", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return new SheetMusic(response.data);
  } catch (e) {
    throw new ResponseError(e, "Failed to create sheet music");
  }
}

export async function updateSheetMusic(sheetMusic, file) {
  const formData = new FormData();

  for (const key in sheetMusic) {
    if (
      Object.prototype.hasOwnProperty.call(sheetMusic, key) &&
      sheetMusic[key]
    ) {
      formData.append(key, sheetMusic[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }

  try {
    const response = await axios.post("/api/sheetMusic/update", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return new SheetMusic(response.data);
  } catch (e) {
    throw new ResponseError(e, "Failed to update sheet music");
  }
}

export async function deleteSheetMusic(sheetMusicId) {
  try {
    await axios.delete(`/api/sheetMusic/delete/${sheetMusicId}`);
  } catch (e) {
    throw new ResponseError(e, "Failed to delete sheet music");
  }
}

export async function renameAllSheetMusic() {
  try {
    await axios.post("/api/sheetMusic/RenameAllFilenames");
  } catch (e) {
    throw new ResponseError(e, "Failed to rename sheet music");
  }
}
