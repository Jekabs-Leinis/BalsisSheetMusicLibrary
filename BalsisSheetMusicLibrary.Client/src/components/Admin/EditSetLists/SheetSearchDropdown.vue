<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { searchCollection } from "@/services/collectionSearchService.js";

// noinspection JSValidateTypes
const props = defineProps({
  /** @type import('vue').PropType<SheetMusic[]> */
  sheets: {
    type: Array,
    required: true,
  },
});

const emit = defineEmits(["select"]);

const searchQuery = ref("");
const showDropdown = ref(false);

/** @type {SheetMusic[]} */
const filteredSheets = computed(() =>
  searchCollection(props.sheets, searchQuery.value, (sheet) =>
    sheet.getFormattedTitle(),
  ),
);

const selectItem = (item) => {
  emit("select", item);
  searchQuery.value = "";
  showDropdown.value = false;
};

const onClickOutside = (event) => {
  if (!event.target.closest(".sheet-search-dropdown")) {
    showDropdown.value = false;
  }
};

onMounted(() => {
  document.addEventListener("click", onClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", onClickOutside);
});
</script>

<template>
  <div class="sheet-search-dropdown">
    <input
      v-model="searchQuery"
      class="form-control search-input"
      placeholder="Pievieno dziesmu..."
      type="text"
      @click="showDropdown = true"
      @focus="showDropdown = true"
    />
    <div v-if="showDropdown" class="dropdown-menu show w-100">
      <div class="px-3 py-2">
        <div v-if="filteredSheets.length === 0" class="text-center py-2">
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

<style lang="scss" scoped>
.sheet-search-dropdown {
  position: relative;

  .search-input {
    --bs-form-select-bg-img: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='%23343a40' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='m2 5 6 6 6-6'/%3e%3c/svg%3e");
    background-image:
      var(--bs-form-select-bg-img), var(--bs-form-select-bg-icon, none);
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
