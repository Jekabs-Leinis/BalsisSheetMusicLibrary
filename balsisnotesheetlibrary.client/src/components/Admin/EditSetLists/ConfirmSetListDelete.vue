<script setup>
import { computed } from "vue";
import VModal from "@/components/Common/VModal.vue";

const props = defineProps({
  /** @type {SetList} */
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
  set: (value) => emit("update:show", value),
});

function onDeleteConfirm() {
  if (props.setList) {
    showModal.value = false;
    emit("confirm", props.setList);
  }
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    title="Dzēst dziesmu sarakstu"
    centered
    @hidden="emit('close')"
  >
    <template v-if="setList">
      <p>Vai tiešām vēlaties dzēst dziesmu sarakstu "{{ setList.title }}"?</p>
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
