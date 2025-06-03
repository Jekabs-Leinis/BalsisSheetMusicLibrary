<template>
  <div class="setlist-item card mb-3">
    <div class="card-header d-flex justify-content-between align-items-center setlist-header">
      <h5 class="mb-0">{{ setList.title }}</h5>
      <div>
        <button class="btn btn-outline-danger btn-sm" @click="$emit('remove')">
          <i class="bi bi-trash"></i>
        </button>
      </div>
    </div>
    <div class="card-body">
      <draggable 
        v-model="sheetList" 
        group="sheets"
        item-key="id"
        handle=".sheet-drag-handle"
        @change="onSongsReorder"
        class="list-group sheets-list mb-3"
      >
        <template #item="{ element : noteSheet, index }">
          <div class="list-group-item d-flex justify-content-between align-items-center">
            <div class="d-flex align-items-center">
              <span class="sheet-drag-handle me-2">
                <i class="bi bi-grip-vertical text-muted"></i>
              </span>
              <span>{{ index + 1 }}. {{ getSongName(noteSheet.noteSheetId) }}</span>
            </div>
            <button class="btn btn-sm btn-outline-danger" @click="removeSong(noteSheet.noteSheetId)">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
        </template>
      </draggable>

      <div class="add-sheet-wrapper">
        <sheet-search-dropdown 
          :sheets="availableSheets" 
          :isLoading="isLoading"
          @select="addSheet" 
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, toRefs } from 'vue';
import draggable from 'vuedraggable';
import SheetSearchDropdown from './SheetSearchDropdown.vue';
import { useSetListStore } from '@/stores/setlistStore';
import { useNoteSheetStore } from '@/stores/notesheetStore';

const props = defineProps({
  /** @type {SetList} */
  setList: {
    type: Object,
    required: true
  },
  allSheets: {
    type: Array,
    required: true
  },
  isLoading: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['remove', 'updated']);

const { setList, allSheets } = toRefs(props);

const setListStore = useSetListStore();
const noteSheetStore = useNoteSheetStore();

// Create a computed prop that's reactive to changes made to the items
const sheetList = computed({
  get: () => [...setList.value.items].sort((a, b) => a.order - b.order),
  set: (value) => {
    // When the sheetList is modified by draggable, update the setList
    setList.value.items = value;
    emit('updated', setList.value);
  }
});

// Get available songs that aren't already in the setlist
const availableSheets = computed(() => {
  return noteSheetStore.getAvailableNoteSheets(setList.value);
});

// Get the name of a song by its ID
const getSongName = (noteSheetId) => {
  const sheet = allSheets.value.find(sheet => sheet.id === noteSheetId);
  return sheet ? sheet.title : 'Unknown Song';
};

// Add a sheet to the setlist
const addSheet = async (sheet) => {
  await setListStore.addSongToSetList(setList.value.id, sheet.id);
  emit('updated', setList.value);
};

// Remove a song from the setlist
const removeSong = async (noteSheetId) => {
  await setListStore.removeSongFromSetList(setList.value.id, noteSheetId);
  emit('updated', setList.value);
};

// Handle reordering of songs
const onSongsReorder = async () => {
  const noteSheetIds = sheetList.value.map(item => item.noteSheetId);
  await setListStore.reorderSongsInSetList(setList.value.id, noteSheetIds);
  emit('updated', setList.value);
};
</script>

<style scoped lang="scss">
@import 'bootstrap-icons/font/bootstrap-icons.css';

.setlist-item {
  transition: background-color 0.2s ease;
  
  &:hover {
    box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
  }
}

.setlist-header {
  cursor: move;
}

.sheets-list {
  min-height: 50px;
}

.sheet-drag-handle {
  cursor: grab;
  
  &:active {
    cursor: grabbing;
  }
}

.add-sheet-wrapper {
  margin-top: 10px;
}
</style>
