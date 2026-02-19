<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import VModal from "@/components/Common/VModal.vue";
import { useToast, TYPE } from "vue-toastification";
import StatusHubService from "@/services/statusHubService";
import { renameAllSheetMusic } from "@/api/sheetMusicApi.js";

const showModal = ref(false);

const toast = useToast();
// For info messages we want to update the toast contents with progress
// instead of creating new toasts
const infoToastId = ref(null);

async function onConfirm() {
  toast.clear();
  infoToastId.value = null;

  showModal.value = false;
  try {
    await renameAllSheetMusic();
  } catch (err) {
    console.log(err);
    
    showToastMessage(`Kļūda, nevar sākt pārsaukšanu: ${err.message}`, TYPE.ERROR);
  }
}

onMounted(() => StatusHubService.onStatus(handleStatus));

/**
 * The renaming process will first send a "start" status, then multiple "info" updates,
 * and finally either "complete" or "error".
 * There might be an "error" in the middle of "info", if a specific file rename fails,
 * but the rename process in that case should continue
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

function showToastMessage(content, type) {
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
    toast(content, { type });
  }
}

onUnmounted(() => {
  StatusHubService.offStatus(handleStatus);
});
</script>

<template>
  <div>
    <button class="btn btn-warning" @click="showModal = true">
      <i class="bi bi-pencil-square me-2"></i>
      Pārsaukt failus
    </button>
    <VModal
      v-model:show="showModal"
      title="Apstiprināt pārsaukšanu"
      centered
      @hidden="showModal = false"
    >
      <div class="alert alert-warning" role="alert">
        Šī darbība pārsauks visus failus, lai tie atbilstu shēmai
        <code>[nosaukums], [mūzikas_autors], [vārdu_autors], [gads]</code>. Šī
        darbība ir neatgriezeniska.
      </div>
      <template #footer>
        <div class="w-100 d-flex justify-content-between">
          <button
            type="button"
            class="btn btn-secondary"
            @click="showModal = false"
          >
            Atcelt
          </button>
          <button type="button" class="btn btn-danger" @click="onConfirm">
            Pārsaukt
          </button>
        </div>
      </template>
    </VModal>
  </div>
</template>
