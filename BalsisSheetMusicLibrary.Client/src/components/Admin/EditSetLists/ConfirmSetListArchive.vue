<script setup>
import { computed } from "vue";
import VModal from "@/components/Common/VModal.vue";

const props = defineProps({
  /** @type import('vue').PropType<SetList> */
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

function onArchiveConfirm() {
  if (props.setList) {
    showModal.value = false;
    emit("confirm", props.setList);
  }
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    title="Arhivēt dziesmu sarakstu"
    centered
    @hidden="emit('close')"
  >
    <template v-if="setList">
      <p class="text-break">
        Vai tiešām vēlies arhivēt dziesmu sarakstu "{{ setList.title }}"?
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
        <button type="button" class="btn btn-warning" @click="onArchiveConfirm">
          Arhivēt
        </button>
      </div>
    </template>
  </VModal>
</template>
