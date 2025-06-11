import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { getAllNoteSheets } from "@/api/noteSheetApi";
import { SortDirection } from "@/models/utilModels";

export const useNoteSheetStore = defineStore("notesheet", () => {
  const noteSheets = ref([]);
  const isLoading = ref(true);
  const error = ref(null);
  const searchQuery = ref("");
  const sortField = ref("title");
  const sortDirection = ref(SortDirection.DESC);

  // Latvian language collator for proper diacritic sorting
  const latvianCollator = new Intl.Collator("lv-LV");

  async function fetchNoteSheets() {
    isLoading.value = true;
    error.value = null;

    try {
      noteSheets.value = await getAllNoteSheets();
    } catch (err) {
      console.error("Error fetching note sheets:", err);
      error.value = "Failed to load note sheets";
    } finally {
      isLoading.value = false;
    }
  }

  const getAvailableNoteSheets = (setList) => {
    if (!setList) return noteSheets.value;

    const setListNoteSheetIds = new Set(
      setList.items.map((item) => item.noteSheetId),
    );

    return noteSheets.value.filter(
      (sheet) => !setListNoteSheetIds.has(sheet.id),
    );
  };

  function setSearchQuery(query) {
    searchQuery.value = query.toLowerCase().trim();
  }

  function setSortField(field) {
    if (sortField.value === field) {
      // Toggle sort direction if clicking the same field
      sortDirection.value =
        sortDirection.value === SortDirection.ASC
          ? SortDirection.DESC
          : SortDirection.ASC;
    } else {
      // Set new field and default to ascending
      sortField.value = field;
      sortDirection.value = SortDirection.ASC;
    }
  }

  const filteredNoteSheets = computed(() => {
    let filtered = [];
    let query = searchQuery.value.trim().toLowerCase();

    // First filter by search query
    if (!query) {
      // Copy to prevert sorting
      filtered = [...noteSheets.value];
    } else {
      filtered = noteSheets.value.filter(
        (sheet) =>
          sheet.title.toLowerCase().includes(query) ||
          sheet
            .getFormattedAdditionalData()
            .toLowerCase()
            .includes(query),
      );
    }

    // Then sort the filtered results
    return filtered.sort((a, b) => {
      let valA = a[sortField.value];
      let valB = b[sortField.value];

      const isEmptyA = valA === null || valA === undefined || valA === "";
      const isEmptyB = valB === null || valB === undefined || valB === "";

      // Always place empty values at the bottom regardless of sort direction
      if (isEmptyA && !isEmptyB) return 1;
      if (!isEmptyA && isEmptyB) return -1;
      if (isEmptyA && isEmptyB) return 0;

      // Both values are non-empty, proceed with normal comparison
      if (typeof valA === "string" && typeof valB === "string") {
        // Use Latvian collator for string comparison to properly handle diacritics
        const comparisonResult = latvianCollator.compare(valA, valB);
        return sortDirection.value === SortDirection.ASC
          ? comparisonResult
          : -comparisonResult;
      }

      // For non-string values, use standard comparison
      if (sortDirection.value === SortDirection.ASC) {
        return valA < valB ? -1 : valA > valB ? 1 : 0;
      } else {
        return valA > valB ? -1 : valA < valB ? 1 : 0;
      }
    });
  });

  const filteredLatvianNoteSheets = computed(() =>
    filteredNoteSheets.value.filter((sheet) => sheet.isLatvian),
  );

  const filteredForeignNoteSheets = computed(() =>
    filteredNoteSheets.value.filter((sheet) => !sheet.isLatvian),
  );

  return {
    noteSheets,
    isLoading,
    error,
    searchQuery,
    sortField,
    sortDirection,
    filteredNoteSheets,
    filteredLatvianNoteSheets,
    filteredForeignNoteSheets,
    fetchNoteSheets,
    getAvailableNoteSheets,
    setSearchQuery,
    setSortField,
  };
});
