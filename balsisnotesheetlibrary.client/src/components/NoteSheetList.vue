<script setup>
import { NoteSheet } from "@/models/sheetModels";

defineProps({
  /** @type {NoteSheet[]} */
  noteSheets: {
    type: Array,
    required: true,
  },
  title: {
    type: String,
    required: true,
  },
});
</script>

<template>
  <div class="pt-4">
    <h1 class="list-title fw-bold pb-2">{{ title }}</h1>
    <ul class="mb-0 pb-4">
      <li v-for="sheet in noteSheets" :key="sheet.id">
        <a
          :href="`/api/download/${sheet.filename}`"
          class="fw-bold"
          target="_blank"
          >{{ sheet.title }}</a
        >{{ sheet.getFormattedAdditionalData() }}
      </li>
    </ul>
  </div>
</template>

<style lang="scss" scoped>
a {
  color: black;
  text-decoration: none;
  
  &:hover {
    text-decoration: underline;
  }
}

.list-title {
  font-size: 2em;
}
</style>
