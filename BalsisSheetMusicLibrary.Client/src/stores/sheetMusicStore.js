import { defineStore } from "pinia";
import { ref, computed } from "vue";
import {
  getAllSheetMusic,
  deleteSheetMusic as deleteSheetMusicApi,
  createSheetMusic as createSheetMusicApi,
  updateSheetMusic as updateSheetMusicApi,
} from "@/api/sheetMusicApi";
import { SortDirection } from "@/models/utilModels";
import {
  filterAndSortSheetMusic,
  filterLatvianSheetMusic,
  filterForeignSheetMusic,
} from "@/services/sheetMusicService";

export const useSheetMusicStore = defineStore("sheetMusic", () => {
  /** @type import('vue').Ref<SheetMusic[]> */
  const sheetMusic = ref([]);
  /** @type import('vue').Ref<boolean> */
  const isLoading = ref(true);
  /** @type import('vue').Ref<string> */
  const searchQuery = ref("");
  /** @type import('vue').Ref<string> */
  const sortField = ref("title");
  /** @type import('vue').Ref<string> */
  const sortDirection = ref(SortDirection.ASC);

  async function fetchSheetMusic() {
    isLoading.value = true;

    try {
      sheetMusic.value = await getAllSheetMusic();
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

  const filteredSheetMusic = computed(() =>
    filterAndSortSheetMusic(
      sheetMusic.value,
      searchQuery.value,
      sortField.value,
      sortDirection.value,
    ),
  );

  const filteredLatvianSheetMusic = computed(() =>
    filterLatvianSheetMusic(filteredSheetMusic.value),
  );

  const filteredForeignSheetMusic = computed(() =>
    filterForeignSheetMusic(filteredSheetMusic.value),
  );

  async function deleteSheetMusic(id) {
    await deleteSheetMusicApi(id);
    sheetMusic.value = sheetMusic.value.filter((sheet) => sheet.id !== id);
  }

  async function createSheetMusic(sheetMusic, file) {
    const newSheetMusic = await createSheetMusicApi(sheetMusic, file);
    sheetMusic.value.push(newSheetMusic);

    return newSheetMusic;
  }

  async function updateSheetMusic(sheetMusic, file) {
    const updatedSheetMusic = await updateSheetMusicApi(sheetMusic, file);
    const index = sheetMusic.value.findIndex(
      (sheet) => sheet.id === updatedSheetMusic.id,
    );
    if (index !== -1) {
      sheetMusic.value[index] = updatedSheetMusic;
    } else {
      sheetMusic.value.push(updatedSheetMusic);
    }

    return updatedSheetMusic;
  }

  return {
    sheetMusic,
    isLoading,
    searchQuery,
    sortField,
    sortDirection,
    filteredSheetMusic,
    filteredLatvianSheetMusic,
    filteredForeignSheetMusic,
    fetchSheetMusic,
    setSearchQuery,
    setSortField,
    setSortDirection,
    deleteSheetMusic,
    createSheetMusic,
    updateSheetMusic,
  };
});
