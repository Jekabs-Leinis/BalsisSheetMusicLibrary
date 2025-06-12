<script setup>
import { ref, onMounted } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import EditSetList from "@/components/Admin/EditSetLists/EditSetList.vue";
import draggable from "vuedraggable";
import AdminHeader from "@/components/Admin/AdminHeader.vue";

// Initialize stores
const setListStore = useSetListStore();
const noteSheetStore = useNoteSheetStore();

const newSetListTitle = ref("");
const isCreatingSetList = ref(false);
const isInitializing = ref(true);

onMounted(async () => {
  await Promise.all([
    setListStore.fetchSetLists(),
    noteSheetStore.fetchNoteSheets(),
  ])
    .then(() => {
      isInitializing.value = false;
    })
    .catch((error) => {
      console.error("Error initializing data:", error);
    });
});

const createSetList = async () => {
  if (!newSetListTitle.value.trim()) return;

  await setListStore.createSetList(newSetListTitle.value.trim());
  newSetListTitle.value = "";
  isCreatingSetList.value = false;
};

const removeSetList = async (setListId) => {
  if (confirm("Are you sure you want to delete this setlist?")) {
    await setListStore.removeSetList(setListId);
  }
};

const cancelCreateSetList = () => {
  isCreatingSetList.value = false;
  newSetListTitle.value = "";
};

const handleSetListMove = ({setListId, newIndex}) => {
  setListStore.moveSetList(setListId, newIndex);
};
</script>

<template>
  <AdminHeader />
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
      <div v-if="isInitializing" class="row">
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
      <div v-if="!isInitializing" class="row">
        <div class="col-12 col-md-10 offset-md-1 col-lg-8 offset-lg-2">
          <!-- Draggable setlists container -->
          <draggable
            v-model="setListStore.setLists"
            handle=".setlist-header"
            item-key="id"
            group="setlists"
            class="setlists-container"
          >
            <template #item="{ element: setList }">
              <edit-set-list
                :set-list="setList"
                :all-sheets="noteSheetStore.noteSheets"
                :is-loading="noteSheetStore.isLoading"
                :set-list-count="setListStore.setLists.length"
                @remove="removeSetList(setList.id)"
                @updated="setListStore.saveSetList(setList)"
                @move="handleSetListMove"
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
