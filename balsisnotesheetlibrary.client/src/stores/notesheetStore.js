import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { getAllNoteSheets } from "@/api/noteSheetApi";

export const useNoteSheetStore = defineStore("notesheet", () => {
  const noteSheets = ref([]);
  const isLoading = ref(true);
  const error = ref(null);
  const searchQuery = ref("");

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

  const filteredNoteSheets = computed(() => {
    if (!searchQuery.value) return noteSheets.value;

    return noteSheets.value.filter(
      (sheet) =>
        sheet.title.toLowerCase().includes(searchQuery.value) ||
        sheet
          .getFormattedAdditionalData()
          .toLowerCase()
          .includes(searchQuery.value),
    );
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
    filteredNoteSheets,
    filteredLatvianNoteSheets,
    filteredForeignNoteSheets,
    fetchNoteSheets,
    getAvailableNoteSheets,
    setSearchQuery,
  };
});
