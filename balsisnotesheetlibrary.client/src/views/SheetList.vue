<script setup>
import SheetListHeader from "@/components/SheetListHeader.vue";
import { ref, computed } from "vue";
import { SetList } from "@/models/sheetModels";
import SetListNoteList from "@/components/SetListNoteList.vue";
import { getAllNoteSheets } from "@/api/noteSheetApi";
import NoteSheetList from "@/components/NoteSheetList.vue";

/* @type {Ref<UnwrapRef<NoteSheet[]>>} */
const noteSheets = ref([]);

getAllNoteSheets().then((sheets) => {
  console.log("sheets");
  console.log(sheets);
  noteSheets.value = sheets;
});

const latvianNoteSheets = computed(() =>
  noteSheets.value.filter((sheet) => sheet.isLatvian),
);
const foreignNoteSheets = computed(() =>
  noteSheets.value.filter((sheet) => !sheet.isLatvian),
);

/* @type {Ref<UnwrapRef<SetList[]>>} */
const setLists = ref([]);

setLists.value = [
  new SetList({ ids: [2, 3], title: "BALSU baseinā" }),
  new SetList({ ids: [2, 2, 2, 2, 2], title: "baseins Balsīs" }),
  new SetList({ ids: [3, 3], title: "Basi baseinā" }),
];
</script>

<template>
  <SheetListHeader />
  <SetListNoteList
    id="active-sheets"
    :note-sheets="noteSheets"
    :set-lists="setLists"
  />
  <div class="invert" id="lv-sheets">
    <div class="container">
      <NoteSheetList
        title="Latviešu skaņdarbi"
        :note-sheets="latvianNoteSheets"
      />
    </div>
  </div>
  <div id="foreign-sheets">
    <div class="container">
      <NoteSheetList
        title="Ārzemju skaņdarbi"
        :note-sheets="foreignNoteSheets"
      />
    </div>
  </div>
</template>

<style lang="scss" scoped>
#foreign-sheets {
  background-color: #dcddd8;
}
</style>

<style>
#lv-sheets * {
  color: white !important;
  background-color: #373737 !important;
}
</style>
