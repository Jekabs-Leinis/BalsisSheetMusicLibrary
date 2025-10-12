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
  const noteSheets = ref([]);
  const isLoading = ref(true);
  const error = ref(null);
  const searchQuery = ref("");
  const sortField = ref("title");
  const sortDirection = ref(SortDirection.ASC);

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
    try {
      await deleteNoteSheetApi(id);
      noteSheets.value = noteSheets.value.filter((sheet) => sheet.id !== id);
    } catch (err) {
      console.error("Error deleting note sheet:", err);
      error.value = "Failed to delete note sheet";
    }
  }
  
  async function createNoteSheet(noteSheet, file) {
    try {
      const newNoteSheet = await createNoteSheetApi(noteSheet, file);
      noteSheets.value.push(newNoteSheet);
      
      return newNoteSheet;
    } catch (err) {
      console.error("Error creating note sheet:", err);
      error.value = "Failed to create note sheet";
    }
  }
  
  async function updateNoteSheet(noteSheet, file) {
    try {
      const updatedNoteSheet = await updateNoteSheetApi(noteSheet, file);
      const index = noteSheets.value.findIndex(sheet => sheet.id === updatedNoteSheet.id);
      if (index !== -1) {
        noteSheets.value[index] = updatedNoteSheet;
      } else {
        noteSheets.value.push(updatedNoteSheet);
      }
      
      return updatedNoteSheet;
    }
    catch (err) {
      console.error("Error updating note sheet:", err);
      error.value = "Failed to update note sheet";
    }
  }

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
    setSearchQuery,
    setSortField,
    setSortDirection,
    deleteNoteSheet,
    createNoteSheet,
    updateNoteSheet,
  };
});
