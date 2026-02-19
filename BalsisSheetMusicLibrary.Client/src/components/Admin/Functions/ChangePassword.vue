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
    console.error("Failed to change password:", e);
    toast.error(`Neizdevās nomainīt paroli: ${e.message}`);
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div>
    <button class="btn btn-warning" @click="showModal = true">
      <i class="bi bi-person-fill-gear me-2"></i>
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
        <select id="userSelect" v-model="selectedUserName" class="form-select">
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
          v-model="newPassword"
          type="text"
          class="form-control"
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
            v-loading.bg="loading"
            type="button"
            class="btn btn-success"
            :disabled="loading || !selectedUserName || !newPassword"
            @click="onConfirm"
          >
            Mainīt
          </button>
        </div>
      </template>
    </VModal>
  </div>
</template>
