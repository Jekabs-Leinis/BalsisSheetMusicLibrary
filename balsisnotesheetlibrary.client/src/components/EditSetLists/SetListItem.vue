<script setup>
import { computed, toRefs } from "vue";
import SheetSearchDropdown from "./SheetSearchDropdown.vue";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import { VueDraggableNext } from "vue-draggable-next";
import { moveSheetInSetList } from "@/services/setListServices";
import { SetListItem } from "@/models/sheetModels";

const props = defineProps({
  /** @type {SetList} */
  setList: {
    type: Object,
    required: true,
  },
  allSheets: {
    type: Array,
    required: true,
  },
  isLoading: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["remove", "updated"]);

const { setList, allSheets } = toRefs(props);

const noteSheetStore = useNoteSheetStore();

// Create a computed prop that's reactive to changes made to the items
/** @type {SetListItem[]} */
const itemsList = computed({
  get: () => setList.value.items,
  set: (value) => {
    // itemsList can be modified by draggable or moving
    setList.value.items = value;
    setList.value.reorderItems();
    emit("updated", setList.value);
  },
});

const availableSheets = computed(() => {
  return noteSheetStore.getAvailableNoteSheets(setList.value);
});

const getSongName = (noteSheetId) => {
  const sheet = allSheets.value.find((sheet) => sheet.id === noteSheetId);
  return sheet ? sheet.title : "Unknown Song";
};

const addSheet = async (sheet) => {
  const item = new SetListItem({
    noteSheetId: sheet.id,
    setListId: setList.value.id,
    order: itemsList.value.length, // Place at the end of set
  });

  // Have to use explicit assignment to trigger setter
  itemsList.value = [...itemsList.value, item];
};

const removeSheet = async (noteSheetId) => {
  itemsList.value = itemsList.value.filter(
    (item) => item.noteSheetId !== noteSheetId,
  );
};

const moveSheet = async (noteSheetId, order) => {
  itemsList.value = moveSheetInSetList(setList.value, noteSheetId, order);
};
</script>

<template>
  <div class="setlist-item card mb-3">
    <div
      class="card-header d-flex justify-content-between align-items-center setlist-header"
    >
      <h5 class="mb-0">{{ setList.title }}</h5>
      <div>
        <button class="btn btn-sm" @click="$emit('remove')">
          <i class="bi bi-trash" />
        </button>
      </div>
    </div>
    <div class="card-body">
      <vue-draggable-next
        v-model="itemsList"
        :group="`sheets-${setList.id}`"
        item-key="id"
        handle=".sheet-drag-handle"
        class="list-group sheets-list mb-3"
      >
        <transition-group>
          <div
            v-for="(noteSheet, index) in itemsList"
            :key="`${noteSheet.setListId}_${noteSheet.noteSheetId}`"
            class="list-group-item d-flex justify-content-between align-items-center"
          >
            <div class="d-flex align-items-center flex-grow-1">
              <span class="sheet-drag-handle me-2">
                <i class="bi bi-grip-vertical text-muted"></i>
              </span>
              <span>
                {{ index + 1 }}. {{ getSongName(noteSheet.noteSheetId) }}
              </span>
            </div>
            <button
              v-if="index > 0"
              class="btn btn-sm fs-6"
              :class="[
                index < itemsList.length - 1 ? 'pe-0' : 'no-down-arrow-padding',
              ]"
              @click="moveSheet(noteSheet.noteSheetId, index - 1)"
            >
              <i class="bi bi-arrow-up movement-arrows" />
            </button>
            <button
              v-if="index < itemsList.length - 1"
              class="btn btn-sm fs-6"
              @click="moveSheet(noteSheet.noteSheetId, index + 1)"
            >
              <i class="bi bi-arrow-down movement-arrows" />
            </button>
            <button
              class="btn btn-sm btn-close"
              @click="removeSheet(noteSheet.noteSheetId)"
            />
          </div>
        </transition-group>
      </vue-draggable-next>

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

<style scoped lang="scss">
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

.movement-arrows {
  opacity: 0.75;
  &:hover {
    opacity: 1;
  }
}

.no-down-arrow-padding {
  padding-right: 2.1rem;
}
</style>
