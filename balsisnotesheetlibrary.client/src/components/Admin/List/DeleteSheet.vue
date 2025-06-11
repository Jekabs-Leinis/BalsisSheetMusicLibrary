<script setup>
import { computed } from "vue";
import { BModal, BButton } from "bootstrap-vue-next";

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

// Computed property for two-way binding
const modelValue = computed({
  get: () => props.show,
  set: (value) => emit("update:show", value)
});

function handleConfirmDelete() {
  if (props.sheet) {
    emit("confirm", props.sheet.id);
    modelValue.value = false; // Close modal after confirmation
  }
}

function handleClose() {
  emit("close");
}
</script>

<template>
  <BModal
    :model-value="modelValue"
    @update:model-value="modelValue = $event"
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
        <BButton variant="secondary" @click="modelValue = false">
          Atcelt
        </BButton>
        <BButton variant="danger" @click="handleConfirmDelete">
          Dzēst
        </BButton>
      </div>
    </template>
  </BModal>
</template>
