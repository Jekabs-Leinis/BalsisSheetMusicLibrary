<script setup>
/** @type {import("vue").DefineProps<{
 noteSheets: NoteSheet[],
 setLists: SetList[],
 }>} */
const props = defineProps({
  noteSheets: Array,
  setLists: Array,
});
</script>

<template>
  <div class="list-container w-100 pt-5 pb-3">
    <div class="container">
      <h1 class="set-list-title fw-bold ps-2">Aktuālās notis</h1>
      <div class="row">
        <div
          v-for="setList in props.setLists"
          :key="setList.title"
          class="col-12"
        >
          <ul>
            <li>
              <h2 class="fw-bold pt-0">{{ setList.title }}</h2>
            </li>
            <li
              v-for="sheet in setList.getNoteSheets(props.noteSheets)"
              :key="sheet.id"
            >
              <a :href="`/api/download/${sheet.id}/${sheet.filename}`" class="text-decoration-none" target="_blank">
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
