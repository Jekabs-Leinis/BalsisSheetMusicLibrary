const formatStringForSearch = (str) =>
  str
    .trim()
    .toLowerCase()
    .normalize("NFD") // Split accented characters into their base character and diacritical marks (e.g., "ā" becomes "a\u0304")
    .replace(/[\u0300-\u036f]/g, ""); // Remove diacritical marks

export function searchCollection(collection, searchTerm, attributeSelector) {
  if (!searchTerm) {
    return collection;
  }

  const query = formatStringForSearch(searchTerm).replace(
    /[.*+?^${}()|[\]\\]/g,
    "\\$&",
  ); // Escape regex special characters

  if (!query) return collection;

  console.log(query);

  // Fuzzy search - "Lan she" will match "Landscape with Shepherds"
  const expression = new RegExp(query.replace(/\s+/g, ".*"));

  console.log(expression);

  return collection.filter((sheet) =>
    formatStringForSearch(attributeSelector(sheet)).match(expression),
  );
}
