export function moveSheetInSetList(setList, noteSheetId, newIndex) {
  const firstItem = setList.items.find(
    (item) => item.noteSheetId === noteSheetId,
  );
  const secondItem = setList.items[newIndex];

  if (!firstItem || !secondItem) {
    throw new Error("Invalid items for moving song in setlist");
  }
  
  setList.items[newIndex] = firstItem;
  setList.items[firstItem.order] = secondItem;
  setList.reorderItems();

  return setList.items;
}