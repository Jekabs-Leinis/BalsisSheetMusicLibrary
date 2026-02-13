import axios from "axios";
import { ResponseError } from "@/models/errorModels.js";

export async function moveSetListItem(setListId, sheetMusicId, newOrder) {
  try {
    const response = await axios.post(`/api/setListItem/move`, {
      setListId,
      sheetMusicId,
      newOrder,
    });

    return response.data;
  } catch (e) {
    throw new ResponseError(e, "Failed to update set list item order");
  }
}
