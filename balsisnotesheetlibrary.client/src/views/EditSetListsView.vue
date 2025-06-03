<script setup>
import { ref, onMounted } from "vue";
import draggable from "vuedraggable";
import { useSetListStore } from "@/stores/setlistStore";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import SetListItem from "@/components/EditSetLists/SetListItem.vue";

// Initialize stores
const setListStore = useSetListStore();
const noteSheetStore = useNoteSheetStore();

// Variables for creating new setlists
const newSetListTitle = ref("");
const isCreatingSetList = ref(false);

// Load data on component mount
onMounted(async () => {
  // Load setlists and notesheets in parallel
  await Promise.all([
    setListStore.fetchSetLists(),
    noteSheetStore.fetchNoteSheets(),
  ]);
});

// Handle setlist drag-and-drop reordering
const onSetListsReorder = async () => {
  const setListIds = setListStore.setLists.map((list) => list.id);
  await setListStore.reorderSetLists(setListIds);
};

// Create a new setlist
const createSetList = async () => {
  if (!newSetListTitle.value.trim()) return;

  await setListStore.createSetList(newSetListTitle.value.trim());
  newSetListTitle.value = "";
  isCreatingSetList.value = false;
};

// Remove a setlist
const removeSetList = async (setListId) => {
  if (confirm("Are you sure you want to delete this setlist?")) {
    await setListStore.removeSetList(setListId);
  }
};

// Cancel setlist creation
const cancelCreateSetList = () => {
  isCreatingSetList.value = false;
  newSetListTitle.value = "";
};
</script>

<template>
  <div class="bg-secondary-subtle">
    <div class="container container-fluid py-4">
      <div class="row">
        <div class="col-12 mb-4">
          <h1>Manage Setlists</h1>
          <p class="text-muted">
            Create and organize your setlists by adding songs and arranging
            their order
          </p>
        </div>
      </div>

      <!-- Loading state -->
      <div
        v-if="setListStore.isLoading || noteSheetStore.isLoading"
        class="row"
      >
        <div class="col-12 text-center py-5">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="mt-2">Ielādē dziesmu sarakstus...</p>
        </div>
      </div>

      <!-- Error messages -->
      <div v-if="setListStore.error || noteSheetStore.error" class="row">
        <div class="col-12">
          <div class="alert alert-danger">
            {{ setListStore.error || noteSheetStore.error }}
          </div>
        </div>
      </div>

      <!-- Setlists content -->
      <div
        v-if="!setListStore.isLoading && !noteSheetStore.isLoading"
        class="row"
      >
        <div class="col-12 col-md-10 offset-md-1 col-lg-8 offset-lg-2">
          <!-- Draggable setlists container -->
          <draggable
            v-model="setListStore.setLists"
            handle=".setlist-header"
            item-key="id"
            group="setlists"
            @change="onSetListsReorder"
            class="setlists-container"
          >
            <template #item="{ element: setList }">
              <set-list-item
                :set-list="setList"
                :all-sheets="noteSheetStore.noteSheets"
                :is-loading="noteSheetStore.isLoading"
                @remove="removeSetList(setList.id)"
                @updated="setListStore.saveSetList(setList)"
              />
            </template>
          </draggable>

          <!-- Create new setlist form -->
          <div v-if="isCreatingSetList" class="card mb-3">
            <div class="card-header">
              <h5 class="mb-0">New Setlist</h5>
            </div>
            <div class="card-body">
              <form @submit.prevent="createSetList">
                <div class="mb-3">
                  <label for="setListTitle" class="form-label"
                    >Setlist Title</label
                  >
                  <input
                    type="text"
                    class="form-control"
                    id="setListTitle"
                    v-model="newSetListTitle"
                    placeholder="Enter setlist title"
                    required
                    autofocus
                  />
                </div>
                <div class="d-flex gap-2">
                  <button type="submit" class="btn btn-primary">
                    Create Setlist
                  </button>
                  <button
                    type="button"
                    class="btn btn-outline-secondary"
                    @click="cancelCreateSetList"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            </div>
          </div>

          <!-- Add new setlist button -->
          <button
            v-if="!isCreatingSetList"
            class="btn btn-primary mb-4"
            @click="isCreatingSetList = true"
          >
            <i class="bi bi-plus-circle me-1"></i> Add New Setlist
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
