<script setup>
import { computed, toRefs } from "vue";
import SheetSearchDropdown from "@/components/Admin/EditSetLists/SheetSearchDropdown.vue";
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";
import { VueDraggable } from "vue-draggable-plus";
import { SetListItem } from "@/models/sheetModels";
import { getSheetsNotInList } from "@/services/sheetMusicService.js";
import EditSetListTitle from "@/components/Admin/EditSetLists/EditSetListTitle.vue";
import { useSetListStore } from "@/stores/setlistStore.js";
import { useToast } from "vue-toastification";

const props = defineProps({
  /** @type import('vue').PropType<SetList> */
  setList: {
    type: Object,
    required: true,
  },
  setListCount: {
    type: Number,
    required: true,
  },
});

const emit = defineEmits(["archive", "delete"]);

const { setList } = toRefs(props);

const sheetMusicStore = useSheetMusicStore();
const setListStore = useSetListStore();
const toast = useToast();

const availableSheets = computed(() => {
  return getSheetsNotInList(sheetMusicStore.sheetMusicArray, setList.value);
});

const addSheet = async (sheet) => {
  const item = new SetListItem({
    sheetMusicId: sheet.id,
    setListId: setList.value.id,
    order: setList.value.items.length, // Place at the end of set
    sheetMusic: sheet,
  });

  setList.value.items.push(item);
  await setListStore.saveSetList(setList.value).catch((error) => {
    console.error("Failed to add sheet to set list:", error);
    toast.error(`Neizdevās pievienot dziesmu sarakstam: ${error.message}`);
  });
};

const removeItem = async (sheetMusicId) => {
  setList.value.items = setList.value.items.filter(
    (item) => item.sheetMusicId !== sheetMusicId,
  );

  await setListStore.saveSetList(setList.value).catch((error) => {
    console.error("Failed to remove sheet from set list:", error);
    toast.error(`Neizdevās noņemt dziesmu no saraksta: ${error.message}`);
  });
};

const onDraggableItemMove = ({ newIndex }) => {
  // VueDraggable has already swapped the items in setList.value.items
  // However we still have to update the order field and call api with error handling
  // To avoid creating a separate store action for this,
  // we can just call moveSetListItem() with a bogus in-place move
  // This is ok, because on server side we re-order items
  // based on the first item, and it's target order
  setListStore
    .moveSetListItem(setList.value.id, newIndex, newIndex)
    .catch((error) => {
      console.error("Failed to reorder set list items:", error);
      toast.error(`Neizdevās pārkārtot dziesmu sarakstu: ${error.message}`);
    });
};

const moveItem = (item, newOrder) => {
  setListStore
    .moveSetListItem(setList.value.id, item.order, newOrder)
    .catch((error) => {
      console.error("Failed to move item:", error);
      toast.error(`Neizdevās pārvietot dziesmu: ${error.message}`);
    });
};
</script>

<template>
  <div class="setlist-item card mb-3">
    <div
      class="card-header d-flex justify-content-between align-items-center"
    >
      <EditSetListTitle
        :set-list="setList"
        :set-list-count="setListCount"
        @archive="emit('archive', $event)"
        @delete="emit('delete', $event)"
      />
    </div>
    <div class="card-body">
      <VueDraggable
        v-model="setList.items"
        :group="`sheets-${setList.id}`"
        class="list-group sheets-list mb-3"
        handle=".sheet-drag-handle"
        item-key="sheetMusicId"
        @sort="onDraggableItemMove"
      >
        <div
          v-for="item in setList.items"
          :key="`${setList.id}-${item.sheetMusicId}`"
          class="list-group-item d-flex justify-content-between align-items-center"
        >
          <div class="d-flex text-break align-items-center flex-grow-1">
            <span class="sheet-drag-handle me-2">
              <i class="bi bi-grip-vertical text-muted" />
            </span>
            <span>
              {{ item.order + 1 }}.
              {{
                item.sheetMusic?.getFormattedTitle() || "Nosaukums nav pieejams"
              }}
            </span>
          </div>
          <button
            v-if="item.order > 0"
            :class="[
              item.order < setList.items.length - 1
                ? 'pe-0'
                : 'no-down-arrow-padding',
            ]"
            class="btn btn-sm fs-6"
            @click="moveItem(item, item.order - 1)"
          >
            <i class="bi bi-arrow-up movement-arrows" />
          </button>
          <button
            v-if="item.order < setList.items.length - 1"
            class="btn btn-sm fs-6"
            @click="moveItem(item, item.order + 1)"
          >
            <i class="bi bi-arrow-down movement-arrows" />
          </button>
          <button
            class="btn btn-sm btn-close"
            @click="removeItem(item.sheetMusicId)"
          />
        </div>
      </VueDraggable>

      <div class="add-sheet-wrapper">
        <SheetSearchDropdown :sheets="availableSheets" @select="addSheet" />
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.setlist-item {
  transition: background-color 0.2s ease;

  &:hover {
    box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
  }
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
