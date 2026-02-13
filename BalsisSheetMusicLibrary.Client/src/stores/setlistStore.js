import { defineStore } from "pinia";
import { ref } from "vue";
import {
  addSetList,
  archiveSetList as archiveSetListApi,
  deleteSetList as deleteSetListApi,
  getAllArchivedSetLists,
  getAllSetLists,
  moveSetList as updateSetListOrderApi,
  restoreSetList as restoreSetListApi,
  updateSetList,
} from "@/api/setListApi";
import { moveSetListItem as moveSetListItemApi } from "@/api/setListItemApi";
import { SetList } from "@/models/sheetModels";

export const useSetListStore = defineStore("setlist", () => {
  /** @type {import('vue').Ref<import('@/models/sheetModels').SetList[]>} */
  const setLists = ref([]);
  /** @type {import('vue').Ref<import('@/models/sheetModels').SetList[]>} */
  const archivedSetLists = ref([]);
  /** @type {import('vue').Ref<boolean>} */
  const isLoading = ref(false);
  /** @type {import('vue').Ref<string|null>} */
  const error = ref(null);

  async function fetchSetLists(withSheetMusic = false) {
    isLoading.value = true;
    error.value = null;

    try {
      const lists = await getAllSetLists(withSheetMusic);
      setLists.value = lists.sort((a, b) => a.order - b.order);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchArchivedSetLists() {
    isLoading.value = true;
    error.value = null;

    try {
      const lists = await getAllArchivedSetLists();
      archivedSetLists.value = lists.sort(
        (a, b) => a.archivedAt - b.archivedAt,
      );
    } finally {
      isLoading.value = false;
    }
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
      reorderSetLists(setLists.value);
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

      throw err;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateSetListOrder(setList) {
    isLoading.value = true;
    error.value = null;

    try {
      await updateSetListOrderApi(setList.id, setList.order);
    } finally {
      isLoading.value = false;
    }
  }

  async function deleteSetList(setListId) {
    isLoading.value = true;
    error.value = null;

    try {
      await deleteSetListApi(setListId);

      setLists.value = setLists.value.filter((list) => list.id !== setListId);
      archivedSetLists.value = archivedSetLists.value.filter(
        (list) => list.id !== setListId,
      );
    } finally {
      isLoading.value = false;
    }
  }

  async function moveSetList(oldIndex, newIndex) {
    const firstSetList = setLists.value[oldIndex];
    const secondSetList = setLists.value[newIndex];

    if (!firstSetList || !secondSetList) {
      throw new Error("Invalid setlists for moving");
    }

    setLists.value[newIndex] = firstSetList;
    setLists.value[firstSetList.order] = secondSetList;

    reorderSetLists(setLists.value);

    await updateSetListOrder(firstSetList);
  }

  async function moveSetListItem(setListId, oldIndex, newIndex) {
    const setList = setLists.value.find((sl) => sl.id === setListId);
    if (!setList) {
      throw new Error("Setlist not found");
    }
    const firstItem = setList.items[oldIndex];
    const secondItem = setList.items[newIndex];

    if (!firstItem || !secondItem) {
      throw new Error("Invalid items for moving song in setlist");
    }

    setList.items[newIndex] = firstItem;
    setList.items[oldIndex] = secondItem;
    setList.reorderItems();

    await moveSetListItemApi(setListId, firstItem.sheetMusicId, newIndex);
  }

  async function archiveSetList(setListId) {
    await archiveSetListApi(setListId);
    const index = setLists.value.findIndex((sl) => sl.id === setListId);
    if (index !== -1) {
      const [archived] = setLists.value.splice(index, 1);
      archived.archivedAt = new Date().toISOString();
      archivedSetLists.value.unshift(archived);
    }
    return true;
  }

  async function restoreSetList(setListId) {
    await restoreSetListApi(setListId);
    const index = archivedSetLists.value.findIndex((sl) => sl.id === setListId);
    if (index !== -1) {
      const [unarchived] = archivedSetLists.value.splice(index, 1);
      unarchived.archivedAt = null;
      setLists.value.push(unarchived);
      reorderSetLists(setLists.value);
    }
    return true;
  }

  function reorderSetLists(setLists) {
    setLists.forEach((list, index) => {
      list.order = index;
    });
    setLists.sort((a, b) => a.order - b.order);

    return setLists;
  }

  return {
    setLists,
    archivedSetLists,
    isLoading,
    error,
    fetchSetLists,
    fetchArchivedSetLists,
    createSetList,
    saveSetList,
    deleteSetList,
    moveSetList,
    moveSetListItem,
    archiveSetList,
    restoreSetList,
  };
});
