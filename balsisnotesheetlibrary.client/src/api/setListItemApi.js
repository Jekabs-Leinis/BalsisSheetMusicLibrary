import axios from "axios";

export async function moveSetListItem(setListId, noteSheetId, newOrder) {
  try {
    const response = await axios.post(`/api/setListItem/move`, {
      setListId,
      noteSheetId,
      newOrder,
    });

    return response.data;
  } catch (e) {
    throw new Error(e.response?.data || e.message || "Failed to update set list item order");
  }
}