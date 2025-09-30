<script setup>
import { computed, toRefs } from "vue";
import SheetSearchDropdown from "./SheetSearchDropdown.vue";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import { VueDraggable } from "vue-draggable-plus";
import { moveSheetInSetList } from "@/services/setListServices";
import { SetListItem } from "@/models/sheetModels";
import { getAvailableNoteSheets } from "@/services/noteSheetService.js";

const props = defineProps({
  /** @type {SetList} */
  setList: {
    type: Object,
    required: true,
  },
  setListCount: {
    type: Number,
    required: true,
  },
  isLoading: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["remove", "updated", "archive"]);

const { setList } = toRefs(props);

const noteSheetStore = useNoteSheetStore();

// Create a computed prop that's reactive to changes made to the items
/** @type {SetListItem[]} */
const setListItems = computed({
  get: () => setList.value.items,
  set: (value) => {
    // setListItems can be modified by draggable or moving
    setList.value.items = value;
    setList.value.reorderItems();
    emit("updated", setList.value);
  },
});

const availableSheets = computed(() => {
  return getAvailableNoteSheets(noteSheetStore.noteSheets.value, setList.value);
});

const addSheet = async (sheet) => {
  const item = new SetListItem({
    noteSheetId: sheet.id,
    setListId: setList.value.id,
    order: setListItems.value.length, // Place at the end of set
    noteSheet: sheet,
  });

  // Have to use explicit assignment to trigger setter
  setListItems.value = [...setListItems.value, item];
};

const removeSheet = async (noteSheetId) => {
  setListItems.value = setListItems.value.filter(
    (item) => item.noteSheetId !== noteSheetId,
  );
};

const moveSheet = async (noteSheetId, order) => {
  setListItems.value = moveSheetInSetList(setList.value, noteSheetId, order);
};

const moveSetList = (setListId, newIndex) => {
  emit("move", { setListId, newIndex });
};
</script>

<template>
  <div class="setlist-item card mb-3">
    <div
      class="card-header d-flex justify-content-between align-items-center setlist-header"
    >
      <h5 class="mb-0 flex-grow-1">{{ setList.title }}</h5>
      <button
        v-if="setList.order > 0"
        class="btn btn-sm fs-6"
        :class="[
          setList.order < setListCount - 1 ? 'pe-0' : 'no-down-arrow-padding',
        ]"
        @click="moveSetList(setList.id, setList.order - 1)"
      >
        <i class="bi bi-arrow-up movement-arrows" />
      </button>
      <button
        v-if="setList.order < setListCount - 1"
        class="btn btn-sm fs-6"
        @click="moveSetList(setList.id, setList.order + 1)"
      >
        <i class="bi bi-arrow-down movement-arrows" />
      </button>
      <div class="d-flex gap-2">
        <button 
          class="btn btn-sm btn-outline-secondary" 
          @click.stop="$emit('archive')"
          title="Archive setlist"
        >
          <i class="bi bi-archive" />
        </button>
        <button 
          class="btn btn-sm btn-outline-danger" 
          @click.stop="$emit('remove')"
          title="Delete setlist"
        >
          <i class="bi bi-trash" />
        </button>
      </div>
    </div>
    <div class="card-body">
      <VueDraggable
        v-model="setListItems"
        :group="`sheets-${setList.id}`"
        item-key="noteSheetId"
        handle=".sheet-drag-handle"
        class="list-group sheets-list mb-3"
      >
        <div
          v-for="item in setListItems"
          :key="`${setList.id}-${item.noteSheetId}`"
          class="list-group-item d-flex justify-content-between align-items-center"
        >
          <div class="d-flex align-items-center flex-grow-1">
            <span class="sheet-drag-handle me-2">
              <i class="bi bi-grip-vertical text-muted"></i>
            </span>
            <span>
              {{ item.order + 1 }}.
              {{ item.noteSheet?.getFormattedTitle() || "Nosaukums nav pieejams" }}
            </span>
          </div>
          <button
            v-if="item.order > 0"
            class="btn btn-sm fs-6"
            :class="[
              item.order < setListItems.length - 1
                ? 'pe-0'
                : 'no-down-arrow-padding',
            ]"
            @click="moveSheet(item.noteSheetId, item.order - 1)"
          >
            <i class="bi bi-arrow-up movement-arrows" />
          </button>
          <button
            v-if="item.order < setListItems.length - 1"
            class="btn btn-sm fs-6"
            @click="moveSheet(item.noteSheetId, item.order + 1)"
          >
            <i class="bi bi-arrow-down movement-arrows" />
          </button>
          <button
            class="btn btn-sm btn-close"
            @click="removeSheet(item.noteSheetId)"
          />
        </div>
      </VueDraggable>

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
