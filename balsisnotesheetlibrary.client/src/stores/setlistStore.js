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
  
  async function saveSetList(updatedSetList) {
    isLoading.value = true;
    error.value = null;
    
    try {
      const index = setLists.value.findIndex(list => list.id === updatedSetList.id);
      const oldSetList = setLists.value[index];

      setLists.value[index] = new SetList(updatedSetList);
      
      const response = await updateSetList(updatedSetList);
      
      if (!response.success) {
        error.value = response.error || 'Failed to update setlist';
        setLists.value[index] = oldSetList;
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
  
  async function reorderSetLists(newOrder) {
    const modifiedLists = [];
    
    // Update the order of setlists
    newOrder.forEach((setListId, index) => {
      const setList = setLists.value.find(list => list.id === setListId);
      if (setList && setList.order !== index) {
        setList.order = index;
        modifiedLists.push(setList);
      }
    });

    // Sort by the new order
    setLists.value.sort((a, b) => a.order - b.order);

    // Update modified setlists on the server
    for (const setList of modifiedLists) {
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
    reorderSetLists
  };
});
