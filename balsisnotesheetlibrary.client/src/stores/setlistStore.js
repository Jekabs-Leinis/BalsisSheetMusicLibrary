import { defineStore } from 'pinia';
import { ref } from 'vue';
import { getAllSetLists, addSetList, updateSetList, deleteSetList } from '@/api/setListApi';
import { SetList, SetListItem } from '@/models/sheetModels';

export const useSetListStore = defineStore('setlist', () => {
  const setLists = ref([]);
  const isLoading = ref(false);
  const error = ref(null);

  // Get all setlists
  async function fetchSetLists() {
    isLoading.value = true;
    error.value = null;
    
    try {
      const lists = await getAllSetLists();
      // Sort by order property
      setLists.value = lists.sort((a, b) => a.order - b.order);
    } catch (err) {
      console.error('Error fetching setlists:', err);
      error.value = 'Failed to load setlists';
    } finally {
      isLoading.value = false;
    }
  }

  // Add a new setlist
  async function createSetList(title) {
    isLoading.value = true;
    error.value = null;
    
    try {
      // Calculate the max order and add 1 for the new setlist
      const maxOrder = setLists.value.length > 0 
        ? Math.max(...setLists.value.map(list => list.order))
        : 0;
      
      const newSetList = new SetList({
        title,
        order: maxOrder + 1,
        songs: []
      });
      
      const response = await addSetList(newSetList);
      
      if (response.success) {
        // Add the newly created setlist with server-generated ID to our state
        const createdSetList = new SetList({
          ...newSetList,
          id: response.model.id
        });
        
        setLists.value.push(createdSetList);
      } else {
        error.value = response.error || 'Failed to create setlist';
      }
    } catch (err) {
      console.error('Error creating setlist:', err);
      error.value = 'Failed to create setlist';
    } finally {
      isLoading.value = false;
    }
  }

  // Update an existing setlist
  async function saveSetList(updatedSetList) {
    isLoading.value = true;
    error.value = null;
    
    try {
      const response = await updateSetList(updatedSetList);
      
      if (response.success) {
        const index = setLists.value.findIndex(list => list.id === updatedSetList.id);
        if (index !== -1) {
          setLists.value[index] = new SetList(updatedSetList);
        }
      } else {
        error.value = response.error || 'Failed to update setlist';
      }
    } catch (err) {
      console.error('Error updating setlist:', err);
      error.value = 'Failed to update setlist';
    } finally {
      isLoading.value = false;
    }
  }

  // Remove a setlist
  async function removeSetList(setListId) {
    isLoading.value = true;
    error.value = null;
    
    try {
      const response = await deleteSetList(setListId);
      
      if (response.success) {
        setLists.value = setLists.value.filter(list => list.id !== setListId);
      } else {
        error.value = response.error || 'Failed to delete setlist';
      }
    } catch (err) {
      console.error('Error deleting setlist:', err);
      error.value = 'Failed to delete setlist';
    } finally {
      isLoading.value = false;
    }
  }

  // Add a song to a setlist
  async function addSongToSetList(setListId, noteSheetId) {
    const setList = setLists.value.find(list => list.id === setListId);
    if (!setList) return;

    // Calculate the max order and add 1 for the new item
    const maxOrder = setList.items.length > 0 
      ? Math.max(...setList.items.map(item => item.order))
      : 0;
    
    // Create a new setlist item
    const newItem = new SetListItem({
      noteSheetId: noteSheetId,
      setListId: setListId,
      order: maxOrder + 1
    });
    
    // Add to local setlist items
    setList.items.push(newItem);
    
    // Update the setlist on the server
    await saveSetList(setList);
  }

  // Remove a song from a setlist
  async function removeSongFromSetList(setListId, noteSheetId) {
    const setList = setLists.value.find(list => list.id === setListId);
    if (!setList) return;

    // Filter out the item to remove
    setList.songs = setList.songs.filter(item => item.noteSheetId !== noteSheetId);
    
    // Update the setlist on the server
    await saveSetList(setList);
  }

  // Reorder songs within a setlist
  async function reorderSongsInSetList(setListId, newItemsOrder) {
    const setList = setLists.value.find(list => list.id === setListId);
    if (!setList) return;

    // Update the order of items
    newItemsOrder.forEach((noteSheetId, index) => {
      const item = setList.items.find(item => item.noteSheetId === noteSheetId);
      if (item) item.order = index;
    });

    // Sort the items by the new order
    setList.items.sort((a, b) => a.order - b.order);
    
    // Update the setlist on the server
    await saveSetList(setList);
  }

  // Reorder setlists
  async function reorderSetLists(newOrder) {
    // Update the order of setlists
    newOrder.forEach((setListId, index) => {
      const setList = setLists.value.find(list => list.id === setListId);
      if (setList) setList.order = index;
    });

    // Sort by the new order
    setLists.value.sort((a, b) => a.order - b.order);

    // Update all setlists on the server
    for (const setList of setLists.value) {
      await saveSetList(setList);
    }
  }

  return {
    setLists,
    isLoading,
    error,
    fetchSetLists,
    createSetList,
    saveSetList,
    removeSetList,
    addSongToSetList,
    removeSongFromSetList,
    reorderSongsInSetList,
    reorderSetLists
  };
});
