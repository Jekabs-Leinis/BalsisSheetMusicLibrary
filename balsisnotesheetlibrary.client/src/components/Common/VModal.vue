<script setup>
import {
  ref,
  watch,
  onMounted,
  onBeforeUnmount,
  nextTick,
  computed,
} from "vue";
import Modal from 'bootstrap/js/dist/modal'

const props = defineProps({
  show: { type: Boolean, required: true },
  title: { type: String, default: "" },
  centered: { type: Boolean, default: false },
  size: { type: String, default: "" }, // 'sm', 'md', 'lg', 'xl'
});

const emit = defineEmits([
  "update:show",
  "hide",
  "hidden",
  "hidePrevented",
  "show",
  "shown",
]);

const modalRef = ref(null);
let bsModal = null;

function setupBootstrapEvents() {
  if (!modalRef.value) return;
  const el = modalRef.value;
  el.addEventListener("hide.bs.modal", () => emit("hide"));
  el.addEventListener("hidden.bs.modal", () => {
    emit("hidden");
    emit("update:show", false);
  });
  el.addEventListener("hidePrevented.bs.modal", () => emit("hidePrevented"));
  el.addEventListener("show.bs.modal", () => emit("show"));
  el.addEventListener("shown.bs.modal", () => emit("shown"));
}

onMounted(() => {
  nextTick(() => {
    // @ts-ignore
    bsModal = new Modal(modalRef.value, {
      backdrop: true,
      keyboard: true,
    });
    setupBootstrapEvents();
    if (props.show) bsModal.show();
  });
});

onBeforeUnmount(() => {
  if (bsModal) {
    bsModal.hide();
    bsModal.dispose();
    bsModal = null;
  }
});

watch(
  () => props.show,
  (val) => {
    if (!bsModal) return;
    if (val) bsModal.show();
    else bsModal.hide();
  },
);

const sizeClass = computed(() => {
  switch (props.size) {
    case "sm":
      return "modal-sm";
    case "md":
      return ""; // Default size, no class needed
    case "lg":
      return "modal-lg";
    case "xl":
      return "modal-xl";
    default:
      return "";
  }
});
</script>

<template>
  <div ref="modalRef" class="modal fade" tabindex="-1" aria-hidden="true">
    <div
      :class="[
        'modal-dialog',
        { 'modal-dialog-centered': centered },
        sizeClass,
      ]"
    >
      <div class="modal-content">
        <div v-if="title" class="modal-header">
          <h5 class="modal-title">{{ title }}</h5>
          <button
            type="button"
            class="btn-close"
            data-bs-dismiss="modal"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body">
          <slot />
        </div>
        <div v-if="$slots.footer" class="modal-footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss"></style>
