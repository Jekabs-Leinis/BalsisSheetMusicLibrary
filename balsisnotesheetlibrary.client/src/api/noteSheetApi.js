import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";
import { BaseDto } from "@/models/commonModels.js";

export async function getAllNoteSheets() {
  const response = BaseDto.fromResponse(
    await axios.get("/api/noteSheet/getAll"),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to get all note sheets");
  }

  return response.data.map((noteSheet) => new NoteSheet(noteSheet));
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

  const response = BaseDto.fromResponse(
    await axios.post("/api/noteSheet/add", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to create note sheet");
  }

  return new NoteSheet(response.data);
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

  const response = BaseDto.fromResponse(
    await axios.post("/api/noteSheet/update", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }),
  );

  if (!response.success) {
    throw Error(response.message || "Failed to update note sheet");
  }

  return new NoteSheet(response.data);
}
