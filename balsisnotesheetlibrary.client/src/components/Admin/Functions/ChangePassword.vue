<script setup>
import VModal from "@/components/Common/VModal.vue";
import { useToast } from "vue-toastification";
import { ref, onMounted } from "vue";
import { getAllUsers, changeUserPassword } from "@/api/userApi.js";

const toast = useToast();

const showModal = ref(false);
/** @type {User[]} */
const users = ref([]);
const selectedUserName = ref("");
const newPassword = ref("");
const loading = ref(false);

onMounted(async () => {
  try {
    users.value = await getAllUsers();
  } catch (e) {
    toast.error("Neizdevās ielādēt lietotājus");
  }
});

async function onConfirm() {
  if (!selectedUserName.value || !newPassword.value) {
    toast.error("Lūdzu, izvēlieties lietotāju un ievadiet jaunu paroli");
    return;
  }
  loading.value = true;
  try {
    await changeUserPassword(selectedUserName.value, newPassword.value);
    toast.success("Parole veiksmīgi nomainīta");
    showModal.value = false;
    selectedUserName.value = "";
    newPassword.value = "";
  } catch (e) {
    toast.error(`Neizdevās nomainīt paroli: ${e.message}`);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div>
    <button class="btn btn-warning" @click="showModal = true">
      <i class="bi bi-pencil-square me-2"></i>
      Mainīt lietotāja paroli
    </button>
    <VModal
      v-model:show="showModal"
      title="Mainīt lietotāja paroli"
      centered
      @hidden="showModal = false"
    >
      <div class="mb-3">
        <label for="userSelect" class="form-label">Izvēlieties lietotāju</label>
        <select id="userSelect" class="form-select" v-model="selectedUserName">
          <option value="" disabled>Izvēlieties...</option>
          <option
            v-for="user in users"
            :key="user.userName"
            :value="user.userName"
          >
            {{ user.userName }}
          </option>
        </select>
      </div>
      <div class="mb-3">
        <label for="newPassword" class="form-label">Jaunā parole</label>
        <input
          id="newPassword"
          type="text"
          class="form-control"
          v-model="newPassword"
          autocomplete="new-password"
        />
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
          <button
            type="button"
            class="btn btn-success"
            :disabled="loading || !selectedUserName || !newPassword"
            v-loading.bg="loading"
            @click="onConfirm"
          >
            Mainīt
          </button>
        </div>
      </template>
    </VModal>
  </div>
</template>
