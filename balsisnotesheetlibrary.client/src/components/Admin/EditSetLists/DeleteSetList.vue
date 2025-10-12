<script setup>
import { computed } from "vue";
import VModal from "@/components/Common/VModal.vue";

const props = defineProps({
  setList: {
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

function onConfirmDelete() {
  if (props.setList) {
    emit("confirm", props.setList.id);
    showModal.value = false;
  }
}

function onClose() {
  emit("close");
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    title="Dzēst nošu sarakstu"
    centered
    @hidden="onClose"
  >
    <template v-if="setList">
      <p>Vai tiešām vēlaties dzēst nošu sarakstu "{{ setList.title }}"?</p>
      <p class="text-danger">
        <strong>Šī darbība ir neatgriezeniska.</strong>
      </p>
    </template>

    <template #footer>
      <div class="w-100 d-flex justify-content-between">
        <button type="button" class="btn btn-secondary" @click="showModal = false">
          Atcelt
        </button>
        <button type="button" class="btn btn-danger" @click="onConfirmDelete">
          Dzēst
        </button>
      </div>
    </template>
  </VModal>
</template>
