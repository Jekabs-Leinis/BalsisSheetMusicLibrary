<script setup>
import { ref, computed, onMounted } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import ArchiveSetList from "@/components/Admin/Archive/ArchiveSetList.vue";
import ArchiveSearchBar from "@/components/Admin/Archive/ArchiveSearchBar.vue";
import ConfirmSetListDelete from "@/components/Admin/EditSetLists/ConfirmSetListDelete.vue";
import ConfirmSetListRestore from "@/components/Admin/Archive/ConfirmSetListRestore.vue";
import { useToast } from "vue-toastification";
import { searchCollection } from "@/services/collectionSearchService.js";

const setListStore = useSetListStore();
const toast = useToast();
const searchQuery = ref("");
const expandedSetLists = ref(new Set());
const isInitializing = ref(true);

onMounted(async () => {
  try {
    await setListStore.fetchArchivedSetLists();
  } catch (error) {
    console.error("Error loading archived setlists:", error);
    toast.error(`Error loading archived setlists: ${error.message}`);
  } finally {
    isInitializing.value = false;
  }
});

// Expand state is stored in this component to allow expanding search results
const toggleExpand = (setListId) => {
  if (expandedSetLists.value.has(setListId)) {
    expandedSetLists.value.delete(setListId);
  } else {
    expandedSetLists.value.add(setListId);
  }
};

const filteredSetLists = computed(() => {
  expandedSetLists.value.clear();

  if (!searchQuery.value) {
    return setListStore.archivedSetLists;
  }

  const query = searchQuery.value.toLowerCase().trim();
  const filteredSetLists = setListStore.archivedSetLists.filter((setList) => {
    if (!query) return true;

    // Check if set list title matches
    if (searchCollection([setList], query, (item) => item.title).length > 0) {
      return true;
    }

    // Check if any sheet music title in the set list matches
    return (
      searchCollection(setList.items, query, (item) =>
        item.sheetMusic?.getFormattedTitle(),
      ).length > 0
    );
  });

  filteredSetLists.forEach((setList) => {
    expandedSetLists.value.add(setList.id);
  });

  return filteredSetLists;
});

const onSearch = (query) => {
  searchQuery.value = query;
};

const setListToRestore = ref(null);
const showRestoreModal = ref(false);

const onRestore = (setList) => {
  setListToRestore.value = setList;
  showRestoreModal.value = true;
};
const onRestoreConfirm = (setList) => {
  setListStore
    .restoreSetList(setList.id)
    .then(() =>
      toast.success(`Dziesmu saraksts "${setList.title}" ir atjaunots`),
    )
    .catch((error) => {
      console.error("Error restoring setlist:", error);
      toast.error(`Kļūda atjaunojot dziesmu sarakstu: ${error.message}`);
    });
  setListToRestore.value = null;
};

const setListToDelete = ref(null);
const showDeleteModal = ref(false);
const onDelete = (setList) => {
  setListToDelete.value = setList;
  showDeleteModal.value = true;
};

const onDeleteConfirm = (setList) => {
  setListStore
    .deleteSetList(setList.id)
    .then(() =>
      toast.success(`Dziemsu saraksts "${setList.title}" ir izdzēsts`),
    )
    .catch((error) => {
      console.error("Error deleting setlist:", error);
      toast.error(`Kļūda dzēšot dziesmu sarakstu: ${error.message}`);
    });
  setListToDelete.value = null;
};
</script>

<template>
  <div class="bg-secondary-subtle min-vh-100">
    <AdminHeader />
    <div class="container py-4">
      <div class="row justify-content-center">
        <div class="col-12 col-lg-10 col-xl-8">
          <ArchiveSearchBar class="mb-4" @search="onSearch" />

          <div v-if="isInitializing" class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Ielādē...</span>
            </div>
            <p class="mt-2">Ielādē arhivētos dziesmu sarakstus...</p>
          </div>

          <div
            v-else-if="filteredSetLists.length === 0"
            class="text-center py-5"
          >
            <div class="alert alert-info">
              <i class="bi bi-archive me-2"></i>
              <span v-if="searchQuery"
                >Pēc meklēšanas kritērijiem, nav atrasts atbilstošs dziesmu
                saraksts</span
              >
              <span v-else>Nav arhivētu dziesmu sarakstu</span>
            </div>
          </div>

          <div v-else>
            <ArchiveSetList
              v-for="setList in setListStore.archivedSetLists"
              v-show="filteredSetLists.includes(setList)"
              :key="setList.id"
              :set-list="setList"
              :is-expanded="expandedSetLists.has(setList.id)"
              class="mb-3"
              @toggle-expand="toggleExpand"
              @restore="onRestore"
              @remove="onDelete"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <ConfirmSetListRestore
    v-model:show="showRestoreModal"
    :set-list="setListToRestore"
    @close="setListToRestore = null"
    @confirm="onRestoreConfirm"
  />
  <ConfirmSetListDelete
    v-model:show="showDeleteModal"
    :set-list="setListToDelete"
    @close="setListToDelete = null"
    @confirm="onDeleteConfirm"
  />
</template>

<style scoped>
.alert {
  max-width: 500px;
  margin: 0 auto;
}
</style>
