<script setup>
import SheetListHeader from "@/components/SheetListHeader.vue";
import { ref } from "vue";
import { NoteSheet, SetList } from "@/models/sheetModels";
import SetListNoteList from "@/components/SetListNoteList.vue";

/* @type {Ref<UnwrapRef<NoteSheet[]>>} */
const noteSheets = ref([]);
/* @type {Ref<UnwrapRef<SetList[]>>} */
const setLists = ref([]);

//TODO replace test data with actual data from server
noteSheets.value = [
  new NoteSheet({
    id: 1,
    title: "Mēness starus stīgo (Dārziņš)",
    author: "Emīls Dārziņš",
    lyricist: "Aspazija (Johanna Emīlija Lizete Rozenberga)",
    year: 1912,
    downloadLink:
      "http://notis.balsis.lv/file/247/meness_starus_stigo_darzins_pko2014.pdf",
  }),
  new NoteSheet({
    id: 2,
    title: "Title 2",
    author: "Author 2",
    year: 2002,
    downloadLink: "link2",
  }),
  new NoteSheet({
    id: 3,
    title: "Title 3",
    author: "Author 3",
    year: 2003,
    downloadLink: "link3",
  }),
];

setLists.value = [
  new SetList({ ids: [1, 3], title: "BALSU baseinā" }),
  new SetList({ ids: [1, 1, 1, 1, 1], title: "baseins Balsīs" }),
  new SetList({ ids: [3, 3], title: "Basi baseinā" }),
];
</script>

<template>
  <SheetListHeader />
  <SetListNoteList :note-sheets="noteSheets" :set-lists="setLists" />
  <div class="lv-chart">
    <div class="container"></div>
  </div>
</template>

<style lang="scss" scoped>
.lv-chart {
  background-color: #dcddd8;
  width: 100%;
}
</style>
<style>
a {
  color: black;
  text-decoration: none;
}
</style>
