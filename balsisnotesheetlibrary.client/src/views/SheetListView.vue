<script setup>
import SheetListHeader from "@/components/SheetList/SheetListHeader.vue";
import { onMounted } from "vue";
import SetListNoteList from "@/components/SheetList/SetListNoteList.vue";
import NoteSheetList from "@/components/SheetList/NoteSheetList.vue";
import { useNoteSheetStore } from "@/stores/notesheetStore";
import { SortDirection } from "@/models/utilModels";
import { useSetListStore } from "@/stores/setlistStore.js";

const noteSheetStore = useNoteSheetStore();
const setListStore = useSetListStore();

onMounted(async () => {
  await Promise.all([
    noteSheetStore.fetchNoteSheets(),
    setListStore.fetchSetLists(),
  ]);

  // Reset sorting to default as user might navigate here from admin view with different sorting
  noteSheetStore.setSortField("title", false);
  noteSheetStore.setSortDirection(SortDirection.ASC);
});
</script>

<template>
  <SheetListHeader />
  <SetListNoteList
    id="active-sheets"
    v-loading="setListStore.isLoading"
    :note-sheets="noteSheetStore.filteredNoteSheets"
    :set-lists="setListStore.setLists"
  />
  <div class="invert" id="lv-sheets">
    <div class="container" v-loading="noteSheetStore.isLoading">
      <NoteSheetList
        title="Latviešu skaņdarbi"
        :note-sheets="noteSheetStore.filteredLatvianNoteSheets"
      />
    </div>
  </div>
  <div id="foreign-sheets">
    <div class="container" v-loading="noteSheetStore.isLoading">
      <NoteSheetList
        title="Ārzemju skaņdarbi"
        :note-sheets="noteSheetStore.filteredForeignNoteSheets"
      />
    </div>
  </div>
</template>

<style lang="scss" scoped>
#foreign-sheets {
  background-color: var(--color-light-darker);
}

#lv-sheets {
  &,
  :deep(a) {
    color: var(--color-light) !important;
    background-color: var(--color-dark) !important;
  }
}

#lv-sheets,
#foreign-sheets {
  scroll-margin-top: 80px;
  font-family: "Courier New", monospace;
}
</style>

<style>
body {
  background-color: var(--color-dark);
}
</style>
