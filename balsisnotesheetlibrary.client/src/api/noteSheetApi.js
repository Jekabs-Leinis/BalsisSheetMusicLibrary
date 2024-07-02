import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";

export async function getAllNoteSheets() {
  const response = await axios.get("/api/noteSheet/getAll");

  if (!response.data.success) {
    //TODO: handle error
    return [];
  }

  return response.data.model.map((noteSheet) => new NoteSheet(noteSheet));
}
