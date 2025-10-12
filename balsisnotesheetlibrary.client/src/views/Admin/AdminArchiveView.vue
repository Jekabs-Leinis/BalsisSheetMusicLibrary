<script setup>
import { ref, computed, onMounted } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import ArchiveSetList from "@/components/Admin/Archive/ArchiveSetList.vue";
import ArchiveSearchBar from "@/components/Admin/Archive/ArchiveSearchBar.vue";

const setListStore = useSetListStore();
const searchQuery = ref("");
const expandedSetLists = ref(new Set());
const isInitializing = ref(true);

onMounted(async () => {
  try {
    await setListStore.fetchArchivedSetLists();
  } catch (error) {
    console.error("Error loading archived setlists:", error);
  } finally {
    isInitializing.value = false;
  }
});

// Expand state is stored in parent component to allow expanding search results
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
  const fuzzyQuery = new RegExp(query.replace(/\s+/g, ".*"));
  const filteredSetLists = setListStore.archivedSetLists.filter((setList) => {
    if (!query) return true;

    // Check if title matches
    if (setList.title.toLowerCase().includes(query)) return true;

    // Fuzzy search for song data
    return setList.items.some((item) =>
      item.noteSheet?.getFormattedTitle().toLowerCase().match(fuzzyQuery),
    );
  });

  filteredSetLists.forEach((setList) => {
    expandedSetLists.value.add(setList.id);
  });

  return filteredSetLists;
});

const handleSearch = (query) => {
  searchQuery.value = query;
};
</script>

<template>
  <div class="bg-secondary-subtle min-vh-100">
    <AdminHeader />
    <div class="container py-4">
      <div class="row justify-content-center">
        <div class="col-12 col-lg-10 col-xl-8">
          <ArchiveSearchBar @search="handleSearch" class="mb-4" />

          <div v-if="isInitializing" class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Ielādē...</span>
            </div>
            <p class="mt-2">Ielādē arhivētos nošu sarakstus...</p>
          </div>

          <div
            v-else-if="filteredSetLists.length === 0"
            class="text-center py-5"
          >
            <div class="alert alert-info">
              <i class="bi bi-archive me-2"></i>
              <span v-if="searchQuery"
                >Pēc meklēšanas kritērijiem, nav atrasts atbilstošs nošu
                saraksts</span
              >
              <span v-else>Nav arhivētu nošu sarakstu</span>
            </div>
          </div>

          <div v-else>
            <ArchiveSetList
              v-for="setList in setListStore.archivedSetLists"
              v-show="filteredSetLists.includes(setList)"
              :key="setList.id"
              :set-list="setList"
              :is-expanded="expandedSetLists.has(setList.id)"
              @toggle-expand="toggleExpand"
              class="mb-3"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.alert {
  max-width: 500px;
  margin: 0 auto;
}
</style>
