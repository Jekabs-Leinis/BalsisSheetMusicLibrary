<script setup>
import { ref, watch, onMounted } from "vue";
import Modal from "bootstrap/js/dist/modal";

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

const emit = defineEmits(["close", "confirm"]);

const showModal = ref(false);
const modalRef = ref(null);
const modal = ref(null);

onMounted(() => {
  modal.value = new Modal(modalRef.value);
});

// Watch the show prop to control the modal visibility
watch(
  () => props.show,
  (newVal) => {
    if (newVal) {
      modal.value.show();
    } else {
      modal.value.hide();
    }
  },
);

watch(showModal, (newVal) => {
  if (newVal) {
    modal.value.show();
  } else {
    modal.value.hide();
    emit("close");
  }
});

function handleConfirmDelete() {
  emit("confirm", props.sheet.id);
  showModal.value = false;
}

function handleCancel() {
  showModal.value = false;
}
</script>

<template>
  <div
    ref="modalRef"
    class="modal fade"
    :class="{ 'show d-block': showModal }"
    tabindex="-1"
    role="dialog"
    aria-labelledby="deleteModalLabel"
    :aria-hidden="!showModal"
    @hide.bs.modal="handleCancel"
  >
    <div class="modal-dialog modal-dialog-centered" role="document">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="deleteModalLabel">Dzēst notis</h5>
          <button
            type="button"
            class="btn-close"
            aria-label="Close"
            @click="handleCancel"
          ></button>
        </div>
        <div class="modal-body" v-if="sheet">
          <p>Vai tiešām vēlaties dzēst notis "{{ sheet.title }}"?</p>
          <p class="text-danger">
            <strong>Šī darbība ir neatgriezeniska.</strong>
          </p>
        </div>
        <div class="modal-footer justify-content-between">
          <button type="button" class="btn btn-secondary" @click="handleCancel">
            Atcelt
          </button>
          <button
            type="button"
            class="btn btn-danger"
            @click="handleConfirmDelete"
          >
            Dzēst
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.modal {
  background-color: rgba(0, 0, 0, 0.5);
}

.modal.show {
  animation: fadeIn 0.3s;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>