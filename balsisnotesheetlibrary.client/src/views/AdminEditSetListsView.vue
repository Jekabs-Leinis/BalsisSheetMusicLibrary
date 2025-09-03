<script setup>
import { ref, onMounted } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import EditSetList from "@/components/Admin/EditSetLists/EditSetList.vue";
import draggable from "vuedraggable";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import DeleteSetList from "@/components/Admin/EditSetLists/DeleteSetList.vue";

// Initialize stores
const setListStore = useSetListStore();
const noteSheetStore = useNoteSheetStore();

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

const newSetListTitle = ref("");
const isCreatingSetList = ref(false);
const isCreatingLoading = ref(false);

const createSetList = async () => {
  if (!newSetListTitle.value.trim()) return;

  isCreatingLoading.value = true;
  await setListStore.createSetList(newSetListTitle.value.trim());
  isCreatingLoading.value = false;
  newSetListTitle.value = "";
  isCreatingSetList.value = false;
};

const cancelCreateSetList = () => {
  isCreatingSetList.value = false;
  newSetListTitle.value = "";
};

const handleSetListMove = ({ setListId, newIndex }) => {
  setListStore.moveSetList(setListId, newIndex);
};

const handleDraggableMove = ({ moved: {newIndex, oldIndex} }) => {
  // The draggable component will update the array position automatically,
  // but it will not update the order field in each set list item.
  setListStore.setLists.forEach((list, index) => {
    list.order = index;
  });
  setListStore.saveSetList(setListStore.setLists[newIndex]);
  setListStore.saveSetList(setListStore.setLists[oldIndex]);
};

const showDeleteModal = ref(false);
const setListToDelete = ref(null);
const handleSetListDelete = (setList) => {
  setListToDelete.value = setList;
  showDeleteModal.value = true;
};
</script>

<template>
  <AdminHeader />
  <div class="bg-secondary-subtle">
    <div class="container container-fluid py-4">
      <!-- Loading state -->
      <div v-if="isInitializing" class="row">
        <div class="col-12 text-center py-5">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Ielādē...</span>
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
          <!-- For each item we also provide moving using arrow buttons,
          as dragging might not always work on mobile devices.
          However this requires us to implement manual array order update 
          in handleSetListMove() -->
          <draggable
            v-model="setListStore.setLists"
            handle=".setlist-header"
            item-key="id"
            group="setlists"
            class="setlists-container"
            @change="handleDraggableMove"
          >
            <template #item="{ element: setList }">
              <edit-set-list
                :set-list="setList"
                :all-sheets="noteSheetStore.noteSheets"
                :is-loading="noteSheetStore.isLoading"
                :set-list-count="setListStore.setLists.length"
                @updated="setListStore.saveSetList(setList)"
                @move="handleSetListMove"
                @remove="handleSetListDelete(setList)"
              />
            </template>
          </draggable>

          <!-- Create new setlist form -->
          <div
            v-if="isCreatingSetList"
            v-loading="isCreatingLoading"
            class="card mb-3"
          >
            <div class="card-header">
              <h5 class="mb-0">Jauns dziesmu saraksts</h5>
            </div>
            <div class="card-body">
              <div class="mb-3">
                <label for="setListTitle" class="form-label"
                  >Dziesmu saraksta nosaukums</label
                >
                <input
                  type="text"
                  class="form-control"
                  id="setListTitle"
                  v-model="newSetListTitle"
                  placeholder="Ievadi dziesmu saraksta nosaukumu"
                  required
                  autofocus
                />
              </div>
              <div class="d-flex justify-content-between gap-2">
                <button
                  type="button"
                  class="btn btn-outline-secondary"
                  @click="cancelCreateSetList"
                >
                  Atcelt
                </button>
                <button
                  type="button"
                  @click="createSetList"
                  class="btn btn-primary"
                >
                  Izveidot jaunu sarakstu
                </button>
              </div>
            </div>
          </div>

          <!-- Add new setlist button -->
          <button
            v-if="!isCreatingSetList"
            class="btn btn-primary mb-4 float-end"
            @click="isCreatingSetList = true"
          >
            <i class="bi bi-plus-circle me-1" /> Pievieno jaunu sarakstu
          </button>
        </div>
      </div>
    </div>
  </div>

  <DeleteSetList
    :set-list="setListToDelete"
    v-model:show="showDeleteModal"
    @close="showDeleteModal = false"
    @confirm="setListStore.removeSetList"
  />
</template>
