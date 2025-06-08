import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";

export async function getAllNoteSheets() {
  const response = await axios.get("/api/noteSheet/getAll");

  if (!response.data.success) {
    throw Error(response.data.error || "Failed to get all note sheets");
  }

  return response.data.model.map((noteSheet) => new NoteSheet(noteSheet));
}
