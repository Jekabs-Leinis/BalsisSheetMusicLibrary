<script setup>
import SheetListHeader from "@/components/SheetList/SheetListHeader.vue";
import { onMounted } from "vue";
import SetListContentsList from "@/components/SheetList/SetListContentsList.vue";
import SheetMusicList from "@/components/SheetList/SheetMusicList.vue";
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";
import { SortDirection } from "@/models/utilModels";
import { useSetListStore } from "@/stores/setlistStore.js";
import { useToast } from "vue-toastification";

const sheetMusicStore = useSheetMusicStore();
const setListStore = useSetListStore();
const toast = useToast();

onMounted(async () => {
  await Promise.all([
    sheetMusicStore.fetchSheetMusic(),
    setListStore.fetchSetLists(),
  ]).catch((error) => {
    console.error("Error loading sheets or set lists:", error);
    toast.error(`Kļūda ielādējot datus: ${error.message}`);
  });

  // Reset sorting to default as user might navigate here from admin view with different sorting
  sheetMusicStore.setSortField("title", false);
  sheetMusicStore.setSortDirection(SortDirection.ASC);
});
</script>

<template>
  <SheetListHeader />
  <SetListContentsList
    id="active-sheets"
    v-loading="setListStore.isLoading"
    :sheet-music="sheetMusicStore.filteredSheetMusic"
    :set-lists="setListStore.setLists"
  />
  <div id="lv-sheets">
    <div class="container" v-loading="sheetMusicStore.isLoading">
      <SheetMusicList
        title="Latviešu skaņdarbi"
        :sheet-music="sheetMusicStore.filteredLatvianSheetMusic"
      />
    </div>
  </div>
  <div id="foreign-sheets">
    <div class="container" v-loading="sheetMusicStore.isLoading">
      <SheetMusicList
        title="Ārzemju skaņdarbi"
        :sheet-music="sheetMusicStore.filteredForeignSheetMusic"
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
