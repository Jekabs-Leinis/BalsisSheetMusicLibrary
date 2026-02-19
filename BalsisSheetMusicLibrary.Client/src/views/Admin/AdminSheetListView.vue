<script setup>
import { onMounted, ref, watch, onBeforeUnmount } from "vue";
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import DeleteSheet from "@/components/Admin/List/DeleteSheet.vue";
import EditSheet from "@/components/Admin/List/EditSheet.vue";
import CreateNewSheet from "@/components/Admin/List/CreateNewSheet.vue";
import { SortDirection } from "@/models/utilModels";
import _debounce from "lodash.debounce";
import { SheetMusic } from "@/models/sheetModels";
import { useToast } from "vue-toastification";

const sheetMusicStore = useSheetMusicStore();
const toast = useToast();

onMounted(async () => {
  await sheetMusicStore.fetchSheetMusic().catch(error =>{
    console.error("Failed to fetch sheet music:", error);
    toast.error(`Neizdevās ielādēt notis: ${error.message}`);
  });
});

const searchInput = ref("");

function onSearch(query) {
  sheetMusicStore.setSearchQuery(query);
}

const debouncedSearch = _debounce(onSearch, 300);

watch(searchInput, (query) => debouncedSearch(query));

onBeforeUnmount(() => {
  sheetMusicStore.setSearchQuery("");
});

const showDeleteModal = ref(false);
const sheetToDelete = ref(null);

const openDeleteModal = (sheet) => {
  sheetToDelete.value = sheet;
  showDeleteModal.value = true;
};

const sortField = ref("title");

const onSort = (field) => {
  sortField.value = field;
  sheetMusicStore.setSortField(field);
};

const getSortIcon = () => {
  return sheetMusicStore.sortDirection === SortDirection.ASC
    ? "bi-sort-down"
    : "bi-sort-down-alt";
};

const showEditModal = ref(false);
const sheetToEdit = ref(null);

const openEditModal = (sheetId) => {
  const sheet = sheetMusicStore.sheetMusic.find((s) => s.id === sheetId);
  sheetToEdit.value = new SheetMusic(sheet);
  showEditModal.value = true;
};
</script>

<template>
  <AdminHeader />
  <div class="pt-4 px-3 bg-secondary-subtle ">
    <div class="row mb-4">
      <div class="col-6 col-md-6 col-lg-4 col-xl-3">
        <div class="input-group">
          <span class="input-group-text">
            <i class="bi bi-search"></i>
          </span>
          <input
            v-model="searchInput"
            type="text"
            class="form-control"
            placeholder="Meklē notis..."
          />
        </div>
      </div>
      <div
        class="col-6 offset-md-0 offset-lg-2 offset-xl-3 col-md-6 d-flex justify-content-end gap-2"
      >
        <CreateNewSheet @sheet-created="sheetToEdit = null" />
      </div>
    </div>

    <!-- Sheets table -->
    <div class="table-responsive">
      <table class="table table-striped table-hover">
        <thead class="table-dark">
          <tr>
            <th class="sort-header" @click="onSort('title')">
              Nosaukums
              <i
                v-if="sortField === 'title'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th class="sort-header" @click="onSort('author')">
              Mūzikas autors
              <i
                v-if="sortField === 'author'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th class="sort-header" @click="onSort('lyricist')">
              Vārdu autors
              <i
                v-if="sortField === 'lyricist'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th class="sort-header" @click="onSort('year')">
              Gads
              <i
                v-if="sortField === 'year'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th
              class="sort-header text-center"
              @click="onSort('isLatvian')"
            >
              Valoda
              <i
                v-if="sortField === 'isLatvian'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th class="sort-header" @click="onSort('fileName')">
              Faila nosaukums
              <i
                v-if="sortField === 'filename'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th class="fw-normal">Darbības</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="sheet in sheetMusicStore.filteredSheetMusic"
            :key="sheet.id"
            class="text-break"
          >
            <td>{{ sheet.title }}</td>
            <td>{{ sheet.author || "-" }}</td>
            <td>{{ sheet.lyricist || "-" }}</td>
            <td>{{ sheet.year || "-" }}</td>
            <td
              class="text-center"
              :title="sheet.isLatvian ? 'Latviešu' : 'Ārzemju'"
            >
<!-- Flag: Latvia Emoji or Globe Showing Europe-Africa Emoji -->
              {{ sheet.isLatvian ? "&#127473;&#127483;" : "&#127757;" }}
            </td>
            <td>
              <a
                :href="`/api/download/${sheet.id}/${sheet.fileName}`"
                target="_blank"
                class="text-decoration-none text-break"
              >
                {{ sheet.fileName }}
              </a>
            </td>
            <td>
              <div class="btn-group">
                <button
                  class="btn btn-icon btn-sm btn-primary me-1"
                  @click="openEditModal(sheet.id)"
                >
                  <i class="bi bi-pencil"></i>
                </button>
                <button
                  class="btn btn-icon btn-sm btn-danger"
                  @click="openDeleteModal(sheet)"
                >
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="sheetMusicStore.filteredSheetMusic.length === 0">
            <td
              v-loading="sheetMusicStore.isLoading"
              colspan="7"
              class="text-center py-3"
            >
              <template v-if="!sheetMusicStore.isLoading">
                Notis nav atrastas
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- Delete confirmation modal -->
  <DeleteSheet
    v-model:show="showDeleteModal"
    :sheet="sheetToDelete"
    @close="sheetToDelete = null"
    @deleted="sheetToDelete = null"
  />

  <!-- Edit sheet modal -->
  <EditSheet
    v-model:show="showEditModal"
    :sheet="sheetToEdit"
    @close="sheetToEdit = null"
    @save="sheetToEdit = null"
  />
</template>

<style scoped lang="scss">
.table {
  vertical-align: middle;

  th {
    --bs-table-bg: var(--color-bluegray);
  }
}

.btn-group {
  white-space: nowrap;
}

.sort-header {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
  font-weight: normal;

  &:hover {
    background-color: var(--bs-table-hover-bg);
  }
}
</style>

<style>
body {
  background-color: #f8f9fa;
}
</style>
