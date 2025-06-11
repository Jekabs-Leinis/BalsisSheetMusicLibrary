<script setup>
import { onMounted, ref, watch, onBeforeUnmount } from "vue";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import { useRouter } from "vue-router";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import DeleteSheet from "@/components/Admin/List/DeleteSheet.vue";
import { SortDirection } from "@/models/utilModels";
import _debounce from "lodash.debounce";

const noteSheetStore = useNoteSheetStore();
const router = useRouter();

onMounted(async () => {
  await noteSheetStore.fetchNoteSheets();
});

const editSheet = (sheetId) => {
  router.push(`/admin/sheets/edit/${sheetId}`);
};

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

const deleteSheet = async (sheetId) => {
  try {
    // Replace with your actual delete API call
    // await deleteNoteSheet(sheetId);
    console.log("Dzēšot noti", sheetId);
    // Refresh the list after deletion
    await noteSheetStore.fetchNoteSheets();
  } catch (error) {
    console.error("Kļūda dzēšot noti:", error);
  }
};

const handleCloseDeleteModal = () => {
  showDeleteModal.value = false;
  sheetToDelete.value = null;
};

const sortField = ref("title");

const handleSort = (field) => {
  sortField.value = field;
  noteSheetStore.setSortField(field);
};

watch(sortField, (field) => noteSheetStore.setSortField(field), {
  immediate: true,
});

const getSortIcon = () => {
  return noteSheetStore.sortDirection === SortDirection.ASC
    ? "bi-sort-down"
    : "bi-sort-down-alt";
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
      <div class="col-6 offset-md-0 offset-lg-2 offset-xl-3 col-md-6 text-end">
        <button class="btn btn-primary">
          <i class="bi bi-plus-circle me-2" /> Pievienot jaunas notis
        </button>
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
            <th>Darbības</th>
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
                :href="`/api/download/${sheet.filename}`"
                target="_blank"
                class="text-decoration-none text-break"
              >
                {{ sheet.filename }}
              </a>
            </td>
            <td>
              <div class="btn-group">
                <button
                  class="btn btn-sm btn-primary me-1"
                  @click="editSheet(sheet.id)"
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
            <td colspan="7" class="text-center py-3">Notis nav atrastas</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>

  <!-- Delete confirmation modal -->
  <DeleteSheet 
    :sheet="sheetToDelete" 
    v-model:show="showDeleteModal"
    @close="handleCloseDeleteModal"
    @confirm="deleteSheet"
  />
</template>

<style scoped lang="scss">
.table {
  vertical-align: middle;
}

.btn-group {
  white-space: nowrap;
}

.sort-header {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;

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