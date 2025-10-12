<script setup>
import { onMounted, ref, watch, onBeforeUnmount } from "vue";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import DeleteSheet from "@/components/Admin/List/DeleteSheet.vue";
import EditSheet from "@/components/Admin/List/EditSheet.vue";
import CreateNewSheet from "@/components/Admin/List/CreateNewSheet.vue";
import { SortDirection } from "@/models/utilModels";
import _debounce from "lodash.debounce";
import { NoteSheet } from "@/models/sheetModels";

const noteSheetStore = useNoteSheetStore();

onMounted(async () => {
  await noteSheetStore.fetchNoteSheets();
});

const searchInput = ref("");

function handleSearch(query) {
  noteSheetStore.setSearchQuery(query);
}

const debouncedSearch = _debounce(handleSearch, 300);

watch(searchInput, (query) => debouncedSearch(query));

onBeforeUnmount(() => {
  noteSheetStore.setSearchQuery("");
});

const showDeleteModal = ref(false);
const sheetToDelete = ref(null);

const openDeleteModal = (sheet) => {
  sheetToDelete.value = sheet;
  showDeleteModal.value = true;
};

const sortField = ref("title");

const handleSort = (field) => {
  sortField.value = field;
  noteSheetStore.setSortField(field);
};

const getSortIcon = () => {
  return noteSheetStore.sortDirection === SortDirection.ASC
    ? "bi-sort-down"
    : "bi-sort-down-alt";
};

const showEditModal = ref(false);
const sheetToEdit = ref(null);

const openEditModal = (sheetId) => {
  const sheet = noteSheetStore.noteSheets.find((s) => s.id === sheetId);
  sheetToEdit.value = new NoteSheet(sheet);
  showEditModal.value = true;
};

const saveSheet = async (sheet) => {
  const index = noteSheetStore.noteSheets.findIndex((s) => s.id === sheet.id);
  if (index === -1) {
    // New sheet, new ID, add it
    noteSheetStore.noteSheets.push(sheet);
  } else {
    noteSheetStore.noteSheets[index] = sheet;
  }

  showEditModal.value = false;
  sheetToEdit.value = null;
};
</script>

<template>
  <AdminHeader />
  <div class="mt-4 mx-3">
    <div class="row mb-4">
      <div class="col-6 col-md-6 col-lg-4 col-xl-3">
        <div class="input-group">
          <span class="input-group-text">
            <i class="bi bi-search"></i>
          </span>
          <input
            type="text"
            class="form-control"
            placeholder="Meklē notis..."
            v-model="searchInput"
          />
        </div>
      </div>
      <div
        class="col-6 offset-md-0 offset-lg-2 offset-xl-3 col-md-6 d-flex justify-content-end gap-2"
      >
        <CreateNewSheet @sheet-created="saveSheet" />
      </div>
    </div>

    <!-- Sheets table -->
    <div class="table-responsive">
      <table class="table table-striped table-hover">
        <thead class="table-dark">
          <tr>
            <th @click="handleSort('title')" class="sort-header">
              Nosaukums
              <i
                v-if="sortField === 'title'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th @click="handleSort('author')" class="sort-header">
              Mūzikas autors
              <i
                v-if="sortField === 'author'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th @click="handleSort('lyricist')" class="sort-header">
              Vārdu autors
              <i
                v-if="sortField === 'lyricist'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th @click="handleSort('year')" class="sort-header">
              Gads
              <i
                v-if="sortField === 'year'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th
              @click="handleSort('isLatvian')"
              class="sort-header text-center"
            >
              Valoda
              <i
                v-if="sortField === 'isLatvian'"
                :class="['bi', getSortIcon(), 'ms-1']"
              ></i>
            </th>
            <th @click="handleSort('filename')" class="sort-header">
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
            v-for="sheet in noteSheetStore.filteredNoteSheets"
            :key="sheet.id"
          >
            <td>{{ sheet.title }}</td>
            <td>{{ sheet.author || "-" }}</td>
            <td>{{ sheet.lyricist || "-" }}</td>
            <td>{{ sheet.year || "-" }}</td>
            <td
              class="text-center"
              :title="sheet.isLatvian ? 'Latviešu' : 'Ārzemju'"
            >
              {{ sheet.isLatvian ? "🇱🇻" : "🌍" }}
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
                  class="btn btn-sm btn-primary me-1"
                  @click="openEditModal(sheet.id)"
                >
                  <i class="bi bi-pencil"></i>
                </button>
                <button
                  class="btn btn-sm btn-danger"
                  @click="openDeleteModal(sheet)"
                >
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="noteSheetStore.filteredNoteSheets.length === 0">
            <td
              colspan="7"
              class="text-center py-3"
              v-loading="noteSheetStore.isLoading"
            >
              <template v-if="!noteSheetStore.isLoading">
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
    :sheet="sheetToDelete"
    v-model:show="showDeleteModal"
    @close="sheetToDelete = null"
    @deleted="sheetToDelete = null"
  />

  <!-- Edit sheet modal -->
  <EditSheet
    :sheet="sheetToEdit"
    v-model:show="showEditModal"
    @close="sheetToEdit = null"
    @save="saveSheet"
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
