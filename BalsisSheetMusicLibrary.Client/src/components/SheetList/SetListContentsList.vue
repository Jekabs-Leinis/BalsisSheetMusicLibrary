<script setup>
const props = defineProps({
  /** @type import('vue').PropType<SheetMusic[]> */
  sheetMusicArray: { type: Array, required: true },
  /** @type import('vue').PropType<SetList[]> */
  setLists: { type: Array, required: true },
});
</script>

<template>
  <div class="list-container w-100 pt-5 pb-3">
    <div class="container">
      <h1 class="set-list-title fw-bold ps-2">Aktuālās notis</h1>
      <div class="row">
        <div
          v-for="setList in setLists"
          :key="setList.title"
          class="col-12 mt-4 text-break"
        >
          <ul>
            <li>
              <h2 class="pt-0 my-3">
                <b>{{ setList.title }}</b>
              </h2>
            </li>
            <li
              v-for="sheet in setList.getSheetMusic(props.sheetMusicArray)"
              :key="sheet.id"
            >
              <a
                :href="`/api/download/${sheet.id}/${sheet.fileName}`"
                class="text-decoration-none"
                target="_blank"
              >
                <b>{{ sheet.title }}</b></a
              >{{ sheet.getFormattedAdditionalData() }}
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.list-container {
  background-color: #dcddd8;
  font-family: "Courier New", monospace;
}

a {
  color: black;
  text-decoration: none;

  &:hover {
    text-decoration: underline !important;
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

h2 {
  text-decoration: underline;
  font-size: 18px;
  padding-top: 30px;
}

.set-list-title {
  font-size: 2em;
}
</style>
