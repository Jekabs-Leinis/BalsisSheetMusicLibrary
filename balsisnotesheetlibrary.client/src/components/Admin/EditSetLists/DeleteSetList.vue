<script setup>
import { computed, defineProps, defineEmits } from "vue";
import { BModal, BButton } from "bootstrap-vue-next";

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

function handleConfirmDelete() {
  if (props.setList) {
    emit("confirm", props.setList.id);
    showModal.value = false;
  }
}

function handleClose() {
  emit("close");
}
</script>

<template>
  <BModal
    :model-value="showModal"
    @update:model-value="showModal = $event"
    title="Dzēst nošu sarakstu"
    centered
    @hidden="handleClose"
  >
    <template v-if="setList">
      <p>Vai tiešām vēlaties dzēst nošu sarakstu "{{ setList.title }}"?</p>
      <p class="text-danger">
        <strong>Šī darbība ir neatgriezeniska.</strong>
      </p>
    </template>

    <template #footer>
      <div class="w-100 d-flex justify-content-between">
        <BButton variant="secondary" @click="showModal = false">
          Atcelt
        </BButton>
        <BButton variant="danger" @click="handleConfirmDelete">
          Dzēst
        </BButton>
      </div>
    </template>
  </BModal>
</template>
