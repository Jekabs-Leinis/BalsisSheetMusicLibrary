<script setup>
import { computed } from "vue";
import VModal from "@/components/Common/VModal.vue";

const props = defineProps({
  sheet: {
    type: Object,
    default: null,
  },
  show: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["close", "confirm", "update:show"]);

const showModal = computed({
  get: () => props.show,
  set: (value) => emit("update:show", value)
});

function handleConfirmDelete() {
  if (props.sheet) {
    emit("confirm", props.sheet.id);
    showModal.value = false; 
  }
}

function handleClose() {
  emit("close");
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    title="Dzēst notis"
    centered
    @hidden="handleClose"
  >
    <template v-if="sheet">
      <p>Vai tiešām vēlaties dzēst notis "{{ sheet.title }}"?</p>
      <p class="text-danger">
        <strong>Šī darbība ir neatgriezeniska.</strong>
      </p>
    </template>
    
    <template #footer>
      <div class="w-100 d-flex justify-content-between">
        <button type="button" class="btn btn-secondary" @click="showModal = false">
          Atcelt
        </button>
        <button type="button" class="btn btn-danger" @click="handleConfirmDelete">
          Dzēst
        </button>
      </div>
    </template>
  </VModal>
</template>
