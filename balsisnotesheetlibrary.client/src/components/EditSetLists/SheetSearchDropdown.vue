<template>
  <div class="sheet-search-dropdown">
    <div class="input-group">
      <input
        type="text"
        class="form-control"
        placeholder="Pievieno dziesmu..."
        v-model="searchQuery"
        @focus="showDropdown = true"
        @click="showDropdown = true"
      />
      <div class="input-group-append">
        <button
          class="btn btn-outline-secondary"
          type="button"
          @click="toggleDropdown"
        >
          <i class="bi bi-caret-down-fill"></i>
        </button>
      </div>
    </div>

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
            {{ sheet.title }}{{ sheet.getFormattedAdditionalData() }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from "vue";

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

// Filter items based on search query
/** @type {Array<NoteSheet>} */
const filteredSheets = computed(() => {
  if (!searchQuery.value) return props.sheets;

  const query = searchQuery.value.toLowerCase();
  return props.sheets.filter(
    (sheet) =>
      sheet.title.toLowerCase().includes(query) ||
      (sheet.author && sheet.author.toLowerCase().includes(query)),
  );
});

// Select an item from the dropdown
const selectItem = (item) => {
  emit("select", item);
  searchQuery.value = "";
  showDropdown.value = false;
};

// Toggle dropdown visibility
const toggleDropdown = () => {
  showDropdown.value = !showDropdown.value;
};

// Handle clicks outside the component to close dropdown
const handleClickOutside = (event) => {
  if (!event.target.closest(".sheet-search-dropdown")) {
    showDropdown.value = false;
  }
};

// Setup and teardown event listener for clicks outside
onMounted(() => {
  document.addEventListener("click", handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleClickOutside);
});
</script>

<style scoped lang="scss">
.sheet-search-dropdown {
  position: relative;

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
