import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';
import { NoteSheet } from '@/models/sheetModels';

export const useNoteSheetStore = defineStore('notesheet', () => {
  const noteSheets = ref([]);
  const isLoading = ref(true);
  const error = ref(null);

  // Get all note sheets
  async function fetchNoteSheets() {
    isLoading.value = true;
    error.value = null;
    
    try {
      const response = await axios.get('/api/noteSheet/getAll');
      
      if (response.data.success) {
        noteSheets.value = response.data.model.map(sheet => new NoteSheet(sheet));
      } else {
        console.error('Failed to get note sheets:', response.data.error);
        error.value = response.data.error || 'Failed to load note sheets';
      }
    } catch (err) {
      console.error('Error fetching note sheets:', err);
      error.value = 'Failed to load note sheets';
    } finally {
      isLoading.value = false;
    }
  }

  // Get available note sheets (not in a specific set list)
  const getAvailableNoteSheets = (setList) => {
    if (!setList) return noteSheets.value;
    
    // Create a Set of ids that are already in the setList for quick lookup
    const setListNoteSheetIds = new Set(setList.items.map(item => item.noteSheetId));
    
    // Return note sheets not in the set list
    return noteSheets.value.filter(sheet => !setListNoteSheetIds.has(sheet.id));
  };

  return {
    noteSheets,
    isLoading,
    error,
    fetchNoteSheets,
    getAvailableNoteSheets
  };
});
