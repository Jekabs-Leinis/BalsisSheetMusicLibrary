import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";

export async function getAllNoteSheets() {
  try {
    const response = await axios.get("/api/noteSheet/getAll");

    return response.data.map((noteSheet) => new NoteSheet(noteSheet));
  } catch (e) {
    throw new Error(e.message || "Failed to get all note sheets");
  }
}

export async function createNoteSheet(noteSheet, file) {
  const formData = new FormData();

  for (const key in noteSheet) {
    if (
      Object.prototype.hasOwnProperty.call(noteSheet, key) &&
      noteSheet[key]
    ) {
      formData.append(key, noteSheet[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }
  try {
    const response = await axios.post("/api/noteSheet/add", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return new NoteSheet(response.data);
  } catch (e) {
    throw new Error(e.message || "Failed to create note sheet");
  }
}

export async function updateNoteSheet(noteSheet, file) {
  const formData = new FormData();

  for (const key in noteSheet) {
    if (
      Object.prototype.hasOwnProperty.call(noteSheet, key) &&
      noteSheet[key]
    ) {
      formData.append(key, noteSheet[key]);
    }
  }

  if (file) {
    formData.append("file", file);
  }

  try {
    const response = await axios.post("/api/noteSheet/update", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return new NoteSheet(response.data);
  } catch (e) {
    throw new Error(e.message || "Failed to update note sheet");
  }
}

export async function deleteNoteSheet(noteSheetId) {
  try {
    await axios.delete(`/api/noteSheet/delete/${noteSheetId}`);
  } catch (e) {
    throw new Error(e.message || "Failed to delete note sheet");
  }
}
