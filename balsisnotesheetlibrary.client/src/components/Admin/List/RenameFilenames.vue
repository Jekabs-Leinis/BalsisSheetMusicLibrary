<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import { BModal, BButton, BAlert } from "bootstrap-vue-next";
import { useToast, TYPE } from "vue-toastification";
import StatusHubService from "@/services/statusHubService";
import axios from "axios";

const showModal = ref(false);

function openModal() {
  showModal.value = true;
}

function handleCancel() {
  showModal.value = false;
}

const toast = useToast();
// For info messages we want to update the toast contents with progress
// instead of creating new toasts
const infoToastId = ref(null);

async function handleConfirm() {
  toast.clear();
  infoToastId.value = null;

  showModal.value = false;
  try {
    await axios.post("/api/NoteSheet/RenameAllFilenames");
  } catch (err) {
    const message = err?.response?.data?.error || err.message;
    //TODO: log error to somewhere

    showToastMessage(`Kļūda, nevar sākt pārsaukšanu: ${message}`, TYPE.ERROR);
  }
}

onMounted(() => StatusHubService.onStatus(handleStatus));

/**
 * The renaming process will first send a "start" status, then multiple "info" updates,
 * and finally either "complete" or "error".
 * There might be an "error" in the middle of "info", if a specific file rename fails,
 * but the overall process should continue
 */
function handleStatus(data) {
  const variantMap = {
    start: TYPE.DEFAULT,
    progress: TYPE.INFO,
    error: TYPE.ERROR,
    complete: TYPE.SUCCESS,
  };

  showToastMessage(data.message, variantMap[data.status] ?? TYPE.INFO);
}

async function showToastMessage(content, type) {
  if (type === TYPE.INFO) {
    if (!infoToastId.value) {
      infoToastId.value = toast(content, {
        timeout: false,
        type,
      });
    } else {
      toast.update(
        infoToastId.value,
        {
          content,
          type,
          timeout: false,
        },
        true, // Required, user might have closed the update toast before the next update
      );
    }
  } else {
    toast(content, {
      timeout: type === TYPE.SUCCESS ? 5000 : false,
      type,
    });
  }
}

onUnmounted(() => {
  StatusHubService.offStatus(handleStatus);
});
</script>

<template>
  <div>
    <button class="btn btn-warning" @click="openModal">
      <i class="bi bi-pencil-square me-2"></i>
      Pārsaukt failus
    </button>
    <BModal
      :model-value="showModal"
      @update:model-value="showModal = $event"
      title="Apstiprināt pārsaukšanu"
      centered
      size="md"
      @hidden="handleCancel"
    >
      <BAlert variant="warning" show>
        Šī darbība pārsauks visus failus, lai tie atbilstu shēmai
        <code>[nosaukums], [mūzikas_autors], [vārdu_autors], [gads]</code>. Šī
        darbība ir neatgriezeniska.
      </BAlert>
      <template #footer>
        <div class="w-100 d-flex justify-content-between">
          <BButton variant="secondary" @click="handleCancel"> Atcelt </BButton>
          <BButton variant="danger" @click="handleConfirm"> Pārsaukt </BButton>
        </div>
      </template>
    </BModal>
  </div>
</template>

<style scoped lang="scss"></style>
