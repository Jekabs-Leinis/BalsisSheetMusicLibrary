import axios from "axios";
import { NoteSheet } from "@/models/sheetModels";

export async function getAllNoteSheets() {
  const response = await axios.get("/api/noteSheet/getAll");

  if (!response.data.success) {
    console.error("Failed to get all note sheets", response.data.error);
    
    return [];
  }

  return response.data.model.map((noteSheet) => new NoteSheet(noteSheet));
}
