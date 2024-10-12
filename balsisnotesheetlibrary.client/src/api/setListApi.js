import axios from "axios";
import {SetList} from "@/models/sheetModels";

export async function getAllSetLists() {
  const response = await axios.get("/api/setList/getAll");

  if (!response.data.success) {
    console.error("Failed to get all set lists", response.data.error);

    return [];
  }

  return response.data.model.map((setList) => new SetList(setList));
}