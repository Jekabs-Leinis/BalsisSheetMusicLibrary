<script setup>
import { computed } from "vue";
import VModal from "@/components/Common/VModal.vue";
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";
import { useToast } from "vue-toastification";

const sheetMusicStore = useSheetMusicStore();
const toast = useToast();

const props = defineProps({
  /** @type import('vue').PropType<SheetMusic> */
  sheet: {
    type: Object,
    default: null,
  },
  show: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["close", "update:show", "deleted"]);

const showModal = computed({
  get: () => props.show,
  set: (value) => emit("update:show", value),
});

async function onDeleteConfirm() {
  if (!props.sheet) {
    throw new Error("Sheet is required to confirm deletion.");
  }

  try {
    await sheetMusicStore.deleteSheetMusic(props.sheet.id);

    toast.success(`Notis "${props.sheet.title}" ir veiksmīgi izdzēstas.`);
  } catch (e) {
    console.error("Error deleting sheet music:", e);
    toast.error(`Nošu dzēšana neizdevās: ${e.message}`);
  }

  emit("deleted");
  showModal.value = false;
}

function onClose() {
  showModal.value = false;
  emit("close");
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    title="Dzēst notis"
    centered
    @hidden="onClose"
  >
    <template v-if="sheet">
      <p class="text-break">Vai tiešām vēlaties dzēst notis "{{ sheet.title }}"?</p>
      <p class="text-danger">
        <strong>Šī darbība ir neatgriezeniska.</strong>
      </p>
    </template>

    <template #footer>
      <div class="w-100 d-flex justify-content-between">
        <button
          type="button"
          class="btn btn-secondary"
          @click="showModal = false"
        >
          Atcelt
        </button>
        <button type="button" class="btn btn-danger" @click="onDeleteConfirm">
          Dzēst
        </button>
      </div>
    </template>
  </VModal>
</template>
