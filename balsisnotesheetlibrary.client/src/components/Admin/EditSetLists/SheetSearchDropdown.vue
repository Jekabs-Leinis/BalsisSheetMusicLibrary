<script setup>
import {
  ref,
  computed,
  onMounted,
  onBeforeUnmount,
} from "vue";

const props = defineProps({
  /** @type {Array<NoteSheet>} */
  sheets: {
    type: Array,
    required: true,
  },
  isLoading: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["select"]);

const searchQuery = ref("");
const showDropdown = ref(false);

/** @type {Array<NoteSheet>} */
const filteredSheets = computed(() => {
  if (!searchQuery.value.trim()) return props.sheets;

  // Fuzzy search
  const query = new RegExp(
    searchQuery.value.toLowerCase().replace(/\s+/g, ".*"),
  );
  return props.sheets.filter((sheet) =>
    sheet.getFormattedTitle().toLowerCase().match(query),
  );
});

const selectItem = (item) => {
  emit("select", item);
  searchQuery.value = "";
  showDropdown.value = false;
};

const handleClickOutside = (event) => {
  if (!event.target.closest(".sheet-search-dropdown")) {
    showDropdown.value = false;
  }
};

onMounted(() => {
  document.addEventListener("click", handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleClickOutside);
});
</script>

<template>
  <div class="sheet-search-dropdown">
    <input
      type="text"
      class="form-control search-input"
      placeholder="Pievieno dziesmu..."
      v-model="searchQuery"
      @focus="showDropdown = true"
      @click="showDropdown = true"
    />
    <div v-if="showDropdown" class="dropdown-menu show w-100">
      <div class="px-3 py-2">
        <div v-if="isLoading" class="text-center py-2">
          <div class="spinner-border spinner-border-sm" role="status">
            <span class="visually-hidden">Ielādē...</span>
          </div>
          <span class="ms-2">Ielādē dziesmas...</span>
        </div>
        <div v-else-if="filteredSheets.length === 0" class="text-center py-2">
          Nav atrastas dziesmas, kas atbilst meklēšanas kritērijiem.
        </div>
        <div v-else class="list-group search-results">
          <button
            v-for="sheet in filteredSheets"
            :key="sheet.id"
            class="list-group-item list-group-item-action"
            @click="selectItem(sheet)"
          >
            {{ sheet.getFormattedTitle() }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.sheet-search-dropdown {
  position: relative;

  .search-input {
    --bs-form-select-bg-img: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='%23343a40' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='m2 5 6 6 6-6'/%3e%3c/svg%3e");
    background-image: var(--bs-form-select-bg-img),
      var(--bs-form-select-bg-icon, none);
    background-repeat: no-repeat;
    background-position: right 0.75rem center;
    background-size: 16px 12px;
  }

  .dropdown-menu {
    max-height: 300px;
    overflow-y: auto;
  }

  .search-results {
    max-height: 250px;
    overflow-y: auto;
  }
}
</style>
