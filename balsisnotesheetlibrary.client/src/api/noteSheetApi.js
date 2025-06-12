import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";

export async function getAllNoteSheets() {
  const response = await axios.get("/api/noteSheet/getAll");

  if (!response.data.success) {
    throw Error(response.data.error || "Failed to get all note sheets");
  }

  return response.data.model.map((noteSheet) => new NoteSheet(noteSheet));
}

export async function createNoteSheet(noteSheet, file) {
  const formData = new FormData();

  for (const key in noteSheet) {
    if (Object.prototype.hasOwnProperty.call(noteSheet, key) && noteSheet[key]) {
      formData.append(key, noteSheet[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }

  const response = await axios.post("/api/noteSheet/add", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  if (!response.data.success) {
    throw Error(response.data.error || "Failed to create note sheet");
  }

  return new NoteSheet(response.data.model);
}

export async function updateNoteSheet(noteSheet, file) {
  const formData = new FormData();

  for (const key in noteSheet) {
    if (Object.prototype.hasOwnProperty.call(noteSheet, key) && noteSheet[key]) {
      formData.append(key, noteSheet[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }

  const response = await axios.post("/api/noteSheet/update", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  if (!response.data.success) {
    throw Error(response.data.error || "Failed to update note sheet");
  }

  return new NoteSheet(response.data.model);
}
