import { defineStore } from "pinia";
import { ref, computed } from "vue";
import {
  getAllNoteSheets,
  deleteNoteSheet as deleteNoteSheetApi,
  createNoteSheet as createNoteSheetApi,
  updateNoteSheet as updateNoteSheetApi,
} from "@/api/noteSheetApi";
import { SortDirection } from "@/models/utilModels";
import {
  filterAndSortNoteSheets,
  filterLatvianNoteSheets,
  filterForeignNoteSheets,
} from "@/services/noteSheetService";

export const useNoteSheetStore = defineStore("notesheet", () => {
  /** @type import('vue').Ref<NoteSheet[]> */
  const noteSheets = ref([]);
  /** @type import('vue').Ref<boolean> */
  const isLoading = ref(true);
  /** @type import('vue').Ref<string> */
  const searchQuery = ref("");
  /** @type import('vue').Ref<string> */
  const sortField = ref("title");
  /** @type import('vue').Ref<string> */
  const sortDirection = ref(SortDirection.ASC);

  async function fetchNoteSheets() {
    isLoading.value = true;

    try {
      noteSheets.value = await getAllNoteSheets();
    } finally {
      isLoading.value = false;
    }
  }

  function setSearchQuery(query) {
    searchQuery.value = query.toLowerCase().trim();
  }

  function setSortField(field, toggleIfSame = true) {
    if (sortField.value === field && toggleIfSame) {
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

  function setSortDirection(direction) {
    if (direction !== SortDirection.ASC && direction !== SortDirection.DESC) {
      throw new Error("Invalid sort direction");
    }

    sortDirection.value = direction;
  }

  const filteredNoteSheets = computed(() =>
    filterAndSortNoteSheets(
      noteSheets.value,
      searchQuery.value,
      sortField.value,
      sortDirection.value,
    ),
  );

  const filteredLatvianNoteSheets = computed(() =>
    filterLatvianNoteSheets(filteredNoteSheets.value),
  );

  const filteredForeignNoteSheets = computed(() =>
    filterForeignNoteSheets(filteredNoteSheets.value),
  );

  async function deleteNoteSheet(id) {
    await deleteNoteSheetApi(id);
    noteSheets.value = noteSheets.value.filter((sheet) => sheet.id !== id);
  }

  async function createNoteSheet(noteSheet, file) {
    const newNoteSheet = await createNoteSheetApi(noteSheet, file);
    noteSheets.value.push(newNoteSheet);

    return newNoteSheet;
  }

  async function updateNoteSheet(noteSheet, file) {
    const updatedNoteSheet = await updateNoteSheetApi(noteSheet, file);
    const index = noteSheets.value.findIndex(
      (sheet) => sheet.id === updatedNoteSheet.id,
    );
    if (index !== -1) {
      noteSheets.value[index] = updatedNoteSheet;
    } else {
      noteSheets.value.push(updatedNoteSheet);
    }

    return updatedNoteSheet;
  }

  return {
    noteSheets,
    isLoading,
    searchQuery,
    sortField,
    sortDirection,
    filteredNoteSheets,
    filteredLatvianNoteSheets,
    filteredForeignNoteSheets,
    fetchNoteSheets,
    setSearchQuery,
    setSortField,
    setSortDirection,
    deleteNoteSheet,
    createNoteSheet,
    updateNoteSheet,
  };
});
