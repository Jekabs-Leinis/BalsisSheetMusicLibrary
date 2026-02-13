<script setup>
import { ref, watch } from "vue";
import _debounce from "lodash.debounce";

const emit = defineEmits(["search"]);

const searchQuery = ref("");

watch(
  searchQuery,
  _debounce((query) => emit("search", query.trim().toLowerCase())),
  300,
);
</script>

<template>
  <div class="mb-4">
    <div class="input-group">
      <span class="input-group-text">
        <i class="bi bi-search"></i>
      </span>
      <input
        v-model="searchQuery"
        type="text"
        class="form-control"
        placeholder="Meklē dziesmu sarakstus un notis..."
        aria-label="Search setlists and songs"
      />
    </div>
  </div>
</template>

<style scoped>
.input-group-text {
  background-color: #f8f9fa;
}
</style>
