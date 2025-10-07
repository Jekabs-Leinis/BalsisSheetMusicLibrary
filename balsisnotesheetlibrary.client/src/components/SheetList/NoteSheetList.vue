<script setup>
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
  <div class="pt-4 row">
    <div class="col-12">
      <h1 class="list-title fw-bold pb-2">{{ title }}</h1>
      <ul class="mb-0 pb-4">
        <li v-for="sheet in noteSheets" :key="sheet.id">
          <a
            :href="`/api/download/${sheet.id}/${sheet.fileName}`"
            class="fw-bold"
            target="_blank"
            >{{ sheet.title }}</a
          >{{ sheet.getFormattedAdditionalData() }}
        </li>
      </ul>
    </div>
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

ul {
  list-style-type: none;
  line-height: 160%;
  margin-left: -43px;
}

.row {
  --bs-gutter-x: -1rem;
}

.list-title {
  font-size: 2em;
}
</style>
