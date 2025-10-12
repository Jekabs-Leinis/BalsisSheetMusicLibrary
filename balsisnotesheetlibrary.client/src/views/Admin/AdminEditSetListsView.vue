<script setup>
import { onMounted, ref } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import EditSetList from "@/components/Admin/EditSetLists/EditSetList.vue";
import { VueDraggable } from "vue-draggable-plus";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import DeleteSetList from "@/components/Admin/EditSetLists/DeleteSetList.vue";

// Initialize stores
const setListStore = useSetListStore();
const noteSheetStore = useNoteSheetStore();

const isInitializing = ref(true);

onMounted(async () => {
  await Promise.all([
    setListStore.fetchSetLists(true),
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

const onDraggableListMove = ({ oldIndex, newIndex }) => {
  setListStore.moveSetList(oldIndex, newIndex);
};

const showDeleteModal = ref(false);
const setListToDelete = ref(null);
</script>

<template>
  <div class="bg-secondary-subtle min-vh-100">
    <AdminHeader />
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
          as dragging might not always work on mobile devices. -->
          <VueDraggable
            v-model="setListStore.setLists"
            class="setlists-container"
            group="setlists"
            handle=".setlist-header"
            item-key="id"
            @sort="onDraggableListMove"
          >
            <EditSetList
              v-for="setList in setListStore.setLists"
              :key="setList.id"
              :is-loading="setListStore.isLoading"
              :set-list="setList"
              :set-list-count="setListStore.setLists.length"
              class="mb-3"
            />
          </VueDraggable>

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
                <label class="form-label" for="setListTitle"
                  >Dziesmu saraksta nosaukums</label
                >
                <input
                  id="setListTitle"
                  v-model="newSetListTitle"
                  autofocus
                  class="form-control"
                  placeholder="Ievadi dziesmu saraksta nosaukumu"
                  required
                  type="text"
                />
              </div>
              <div class="d-flex justify-content-between gap-2">
                <button
                  class="btn btn-outline-secondary"
                  type="button"
                  @click="cancelCreateSetList"
                >
                  Atcelt
                </button>
                <button
                  class="btn btn-primary"
                  type="button"
                  @click="createSetList"
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
    v-model:show="showDeleteModal"
    :set-list="setListToDelete"
    @close="showDeleteModal = false"
    @confirm="setListStore.removeSetList"
  />
</template>
