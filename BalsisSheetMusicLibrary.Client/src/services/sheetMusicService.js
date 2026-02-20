import { SortDirection } from "@/models/utilModels";

// Latvian language collator for proper diacritic sorting
const latvianCollator = new Intl.Collator("lv-LV");

export function filterAndSortSheetMusic(
  sheetMusicArray,
  query,
  sortField,
  sortDirection,
) {
  let filtered;
  const search = query.trim().toLowerCase();

  if (!search) {
    filtered = [...sheetMusicArray];
  } else {
    filtered = sheetMusicArray.filter((sheet) =>
      sheet.getFormattedTitle().toLowerCase().includes(search),
    );
  }

  return filtered.sort((a, b) => {
    let valA = a[sortField];
    let valB = b[sortField];

    // We want to sort empty values to the end
    const isEmptyA = valA === null || valA === undefined || valA === "";
    const isEmptyB = valB === null || valB === undefined || valB === "";

    if (isEmptyA && !isEmptyB) return 1;
    if (!isEmptyA && isEmptyB) return -1;
    if (isEmptyA && isEmptyB) return 0;

    if (typeof valA === "string" && typeof valB === "string") {
      const comparisonResult = latvianCollator.compare(valA, valB);
      return sortDirection === SortDirection.ASC
        ? comparisonResult
        : -comparisonResult;
    }

    if (sortDirection === SortDirection.ASC) {
      return valA < valB ? -1 : valA > valB ? 1 : 0;
    } else {
      return valA > valB ? -1 : valA < valB ? 1 : 0;
    }
  });
}

export function filterLatvianSheetMusic(sheetMusicArray) {
  return sheetMusicArray.filter((sheet) => sheet.isLatvian);
}

export function filterForeignSheetMusic(sheetMusicArray) {
  return sheetMusicArray.filter((sheet) => !sheet.isLatvian);
}

export function getSheetsNotInList(sheetMusicArray, setList) {
  if (!setList || !setList.items.length) {
    return sheetMusicArray;
  }

  const setListSheetMusicIds = new Set(
    setList.items.map((item) => item.sheetMusicId),
  );

  return sheetMusicArray.filter((sheet) => !setListSheetMusicIds.has(sheet.id));
}
