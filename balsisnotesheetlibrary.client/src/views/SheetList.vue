<script setup>
import SheetListHeader from "@/components/SheetList/SheetListHeader.vue";
import { ref, computed } from "vue";
import SetListNoteList from "@/components/SheetList/SetListNoteList.vue";
import { getAllNoteSheets } from "@/api/noteSheetApi";
import NoteSheetList from "@/components/SheetList/NoteSheetList.vue";
import { getAllSetLists } from "@/api/setListApi";

/* @type {Ref<UnwrapRef<NoteSheet[]>>} */
const noteSheets = ref([]);

getAllNoteSheets().then((sheets) => {
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

getAllSetLists().then((lists) => {
  setLists.value = lists;
});
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
  background-color: var(--color-light-darker);
}

#lv-sheets {
  &, :deep(a) {
    color: var(--color-light) !important;
    background-color: var(--text-color) !important;
  }
}

#lv-sheets, #foreign-sheets {
  scroll-margin-top: 80px
}
</style>

