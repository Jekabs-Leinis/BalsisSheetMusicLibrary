import { defineStore } from "pinia";
import { ref, computed } from "vue";
import {
  getAllSetLists,
  addSetList,
  updateSetList,
  deleteSetList,
  archiveSetList as archiveSetListApi,
  unarchiveSetList as unarchiveSetListApi,
  updateSetListOrder as updateSetListOrderApi,
} from "@/api/setListApi";
import { SetList } from "@/models/sheetModels";

export const useSetListStore = defineStore("setlist", () => {
  const setLists = ref([]);
  const archivedSetLists = computed(() =>
    setLists.value
      .filter((list) => list.archivedAt)
      .sort((a, b) => b.archivedAt - a.archivedAt),
  );
  const isLoading = ref(false);
  const error = ref(null);

  async function fetchSetLists(withSheets = false, withArchived = false) {
    isLoading.value = true;
    error.value = null;

    try {
      const lists = await getAllSetLists(withSheets, withArchived);
      setLists.value = lists.sort((a, b) => a.order - b.order);
    } catch (err) {
      console.error("Error fetching setlists:", err);
      error.value = "Failed to load setlists";
    } finally {
      isLoading.value = false;
    }
  }

  function reorderLists() {
    setLists.value.forEach((list, index) => {
      list.order = index;
    });
    setLists.value.sort((a, b) => a.order - b.order);
  }

  async function createSetList(title) {
    isLoading.value = true;
    error.value = null;

    try {
      const currentMaxOrder =
        setLists.value.length > 0
          ? Math.max(...setLists.value.map((list) => list.order))
          : 0;

      const newSetList = new SetList({
        title,
        order: currentMaxOrder + 1,
        songs: [],
      });

      const createdSetList = await addSetList(newSetList);

      setLists.value.push(createdSetList);
    } catch (err) {
      console.error("Error creating setlist:", err);
      error.value = err.message || "Failed to create setlist";
    } finally {
      isLoading.value = false;
    }
  }

  async function saveSetList(updatedSetList) {
    isLoading.value = true;
    error.value = null;

    const index = setLists.value.findIndex(
      (list) => list.id === updatedSetList.id,
    );
    const oldSetList = setLists.value[index];

    try {
      setLists.value[index] = new SetList(updatedSetList);

      await updateSetList(updatedSetList);
    } catch (err) {
      // Revert on save fail
      setLists.value[index] = oldSetList;
      console.error("Error updating setlist:", err);
      error.value = "Failed to update setlist";
    } finally {
      isLoading.value = false;
    }
  }

  async function updateSetListOrder(setList) {
    isLoading.value = true;
    error.value = null;

    try {
      await updateSetListOrderApi(setList);
    } catch (err) {
      console.error("Error updating setlist order:", err);
      error.value = "Failed to update setlist order";
    } finally {
      isLoading.value = false;
    }
  }

  async function removeSetList(setListId) {
    isLoading.value = true;
    error.value = null;

    try {
      await deleteSetList(setListId);

      setLists.value = setLists.value.filter((list) => list.id !== setListId);
    } catch (err) {
      console.error("Error deleting setlist:", err);
      error.value = "Failed to delete setlist";
    } finally {
      isLoading.value = false;
    }
  }

  async function moveSetList(setListId, newIndex) {
    const firstSetList = setLists.value.find((list) => list.id === setListId);
    const secondSetList = setLists.value[newIndex];

    if (!firstSetList || !secondSetList) {
      throw new Error("Invalid setlists for moving");
    }

    setLists.value[newIndex] = firstSetList;
    setLists.value[firstSetList.order] = secondSetList;

    reorderLists();

    try {
      await updateSetListOrder(firstSetList);
    } catch (err) {
      console.error("Error updating setlist order:", err);
      error.value = "Failed to update setlist order";
    }
  }

  async function archiveSetList(setListId) {
    try {
      await archiveSetListApi(setListId);
      const index = setLists.value.findIndex((sl) => sl.id === setListId);
      if (index !== -1) {
        const [archived] = setLists.value.splice(index, 1);
        archived.archivedAt = new Date().toISOString();
        archivedSetLists.value.unshift(archived);
      }
      return true;
    } catch (err) {
      console.error("Error archiving setlist:", err);
      error.value = "Failed to archive setlist";
      throw err;
    }
  }

  async function unarchiveSetList(setListId) {
    try {
      await unarchiveSetListApi(setListId);
      const index = archivedSetLists.value.findIndex(
        (sl) => sl.id === setListId,
      );
      if (index !== -1) {
        const [unarchived] = archivedSetLists.value.splice(index, 1);
        unarchived.archivedAt = null;
        setLists.value.push(unarchived);
        reorderLists();
      }
      return true;
    } catch (err) {
      console.error("Error unarchiving setlist:", err);
      error.value = "Failed to unarchive setlist";
      throw err;
    }
  }

  return {
    setLists,
    archivedSetLists,
    isLoading,
    error,
    fetchSetLists,
    createSetList,
    saveSetList,
    removeSetList,
    moveSetList,
    reorderLists,
    archiveSetList,
    unarchiveSetList,
  };
});
