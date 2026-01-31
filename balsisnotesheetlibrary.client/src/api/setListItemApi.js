import axios from "axios";
import { ResponseError } from "@/models/errorModels.js";

export async function moveSetListItem(setListId, noteSheetId, newOrder) {
  try {
    const response = await axios.post(`/api/setListItem/move`, {
      setListId,
      noteSheetId,
      newOrder,
    });

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to update set list item order");
  }
}