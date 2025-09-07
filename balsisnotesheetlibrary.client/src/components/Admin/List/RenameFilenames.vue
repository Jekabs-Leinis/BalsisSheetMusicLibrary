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
async function handleConfirm() {
  toast.clear();
  toastId.value = null;
  lastType.value = null;
  
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

const toastId = ref(null);
const lastType = ref(null);

async function showToastMessage(content, type) {
  // If type changes, create a new toast as type updates are not supported
  if (toastId.value && lastType.value !== type) {
    await toast.clear();
    toastId.value = null;
  }
  if (!toastId.value) {
    toastId.value = toast(content, {
      timeout: type === TYPE.SUCCESS ? 5000 : false,
      type,
    });
    lastType.value = type;
  } else {
    toast.update(
      toastId.value,
      {
        content,
        type,
        timeout: type === TYPE.SUCCESS ? 5000 : false,
      },
      true,
    );
    lastType.value = type;
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
