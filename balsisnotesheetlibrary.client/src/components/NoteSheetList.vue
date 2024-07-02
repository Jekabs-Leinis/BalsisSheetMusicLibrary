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
  <div>
    <h1>{{ title }}</h1>
    <ul>
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

<style scoped>
a:hover {
  text-decoration: underline !important;
}
</style>
