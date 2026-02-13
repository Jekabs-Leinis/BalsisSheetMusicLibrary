<script setup>
import { computed, ref, watch } from "vue";
import VModal from "@/components/Common/VModal.vue";
import { SheetMusic } from "@/models/sheetModels";
import { useToast } from "vue-toastification";
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";

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

const emit = defineEmits(["close", "save", "update:show"]);

const toast = useToast();
const sheetMusicStore = useSheetMusicStore();

const showModal = computed({
  get: () => props.show,
  set: (value) => emit("update:show", value),
});

const formData = ref(new SheetMusic());
const selectedFile = ref(null);
const fileError = ref("");

const isValid = ref(true);
const validationMessage = ref("");

const isCreateMode = computed(() => !formData.value.id);

watch(
  () => props.sheet,
  (newSheet) => {
    if (newSheet) {
      formData.value = new SheetMusic(newSheet);
    } else {
      formData.value = new SheetMusic();
    }
    selectedFile.value = null;
    fileError.value = "";
  },
  { immediate: true, deep: true },
);

function onFileChanged(event) {
  const file = event.target?.files?.[0];
  if (file) {
    if (file.type !== "application/pdf") {
      fileError.value = "Tikai PDF faili ir atbalstīti.";
      selectedFile.value = null;
      return;
    }

    fileError.value = "";
    selectedFile.value = file;
  }
}

function clearFileSelection() {
  selectedFile.value = null;
  fileError.value = "";
}

function validateForm() {
  isValid.value = true;
  validationMessage.value = "";
  let errors = [];

  if (!formData.value.title.trim()) {
    errors.push("Nosaukums ir obligāts.");
  }

  if (formData.value.year !== null && formData.value.year !== "") {
    const yearNum = Number(formData.value.year);
    if (isNaN(yearNum)) {
      errors.push("Gadam jābūt skaitlim.");
    } else if (yearNum <= 0) {
      errors.push("Gadam jābūt lielākam par 0.");
    } else {
      formData.value.year = yearNum;
    }
  } else {
    formData.value.year = null;
  }

  if (isCreateMode.value && !selectedFile.value) {
    errors.push("Nepieciešams pievienot failu.");
  }
  
  if (formData.value.title.length >= 200) {
    errors.push("Nosaukums nedrīkst pārsniegt 200 rakstzīmes.");
  }
  
  if (formData.value.author.length >= 200) {
    errors.push("Mūzikas autors nedrīkst pārsniegt 200 rakstzīmes.");
  }
  
  if (formData.value.lyricist.length >= 200) {
    errors.push("Vārdu autors nedrīkst pārsniegt 200 rakstzīmes.");
  }
  
  validationMessage.value = errors.join("\n");
  isValid.value = errors.length === 0;
  
  return errors.length === 0;
}

const isSaving = ref(false);
async function onSave() {
  if (!validateForm()) {
    return;
  }

  let savedSheet;

  isSaving.value = true;
  
  try {
    if (isCreateMode.value) {
      savedSheet = await sheetMusicStore.createSheetMusic(formData.value, selectedFile.value);
    } else {
      savedSheet = await sheetMusicStore.updateSheetMusic(formData.value, selectedFile.value);
    }
    toast.success(`Notis "${savedSheet.title}" ir veiksmīgi saglabātas.`);
  } catch (e) {
    console.error("Error saving sheet music:", e);
    toast.error(`Notu saglabāšana neizdevās: ${e.message}`);
    
    return;
  } finally {
    isSaving.value = false;
  }
  
  showModal.value = false;
  emit("save", savedSheet);

  clearFileSelection();
}

function onClose() {
  isValid.value = true;
  emit("close");
}

function onInputBlur(field) {
  if (field === "title" && !formData.value.title.trim()) {
    isValid.value = false;
    validationMessage.value = "Nosaukums ir obligāts.";
  } else {
    isValid.value = true;
    validationMessage.value = "";
  }
}
</script>

<template>
  <VModal
    v-model:show="showModal"
    :title="isCreateMode ? 'Pievienot notis' : 'Rediģēt notis'"
    centered
    size="lg"
    @hidden="onClose"
  >
    <form @submit.prevent="onSave">
      <div v-if="!isValid" class="alert alert-danger" role="alert">
        {{ validationMessage }}
      </div>

      <div class="mb-3">
        <label for="title-input" class="form-label">Nosaukums *</label>
        <input
          id="title-input"
          v-model="formData.title"
          type="text"
          class="form-control"
          placeholder="Ievadiet dziesmas nosaukumu"
          required
          @blur="onInputBlur('title')"
          :class="{ 'is-invalid': !formData.title.trim() }"
        />
      </div>

      <div class="mb-3 pt-2">
        <label for="author-input" class="form-label">Mūzikas autors</label>
        <input
          id="author-input"
          v-model="formData.author"
          type="text"
          class="form-control"
          placeholder="Ievadiet mūzikas autoru"
        />
      </div>

      <div class="mb-3 pt-2">
        <label for="lyricist-input" class="form-label">Vārdu autors</label>
        <input
          id="lyricist-input"
          v-model="formData.lyricist"
          type="text"
          class="form-control"
          placeholder="Ievadiet vārdu autoru"
        />
      </div>

      <div class="mb-3 pt-2">
        <label for="year-input" class="form-label">Gads</label>
        <input
          id="year-input"
          v-model="formData.year"
          type="text"
          inputmode="numeric"
          class="form-control"
          placeholder="Ievadiet gadu"
        />
      </div>

      <div class="form-check py-2 mb-3">
        <input
          class="form-check-input"
          type="checkbox"
          id="latvian-checkbox"
          v-model="formData.isLatvian"
        />
        <label class="form-check-label" for="latvian-checkbox">
          Latviešu dziesma
        </label>
      </div>

      <div class="mb-3 pt-2">
        <label for="file-input" class="form-label">
          PDF fails{{ isCreateMode ? " *" : "" }}
        </label>
        <div
          v-if="formData.fileName && !selectedFile"
          class="d-flex align-items-center mb-2"
        >
          <i class="bi bi-file-earmark-pdf text-danger fs-4 me-2"></i>
          <span class="text-break">{{ formData.fileName }}</span>
          <a
            :href="`/api/download/${formData.id}/${formData.fileName}`"
            target="_blank"
            class="ms-2 text-decoration-none"
          >
            <i class="bi bi-download"></i>
          </a>
        </div>

        <div v-if="selectedFile" class="d-flex align-items-center mb-2">
          <i class="bi bi-file-earmark-pdf text-danger fs-4 me-2"></i>
          <span class="text-break">{{ selectedFile.name }}</span>
          <button
            type="button"
            class="btn btn-sm text-danger"
            @click="clearFileSelection"
          >
            <i class="bi bi-x-circle"></i>
          </button>
        </div>

        <div class="input-group">
          <input
            type="file"
            class="form-control"
            id="file-input"
            accept="application/pdf"
            @change="onFileChanged"
            :class="{ 'is-invalid': fileError }"
          />
        </div>

        <div v-if="fileError" class="invalid-feedback d-block">
          {{ fileError }}
        </div>
      </div>
    </form>

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
          v-loading.bg="isSaving"
          class="btn btn-primary"
          @click="onSave"
        >
          Saglabāt
        </button>
      </div>
    </template>
  </VModal>
</template>

<style scoped lang="scss">
.modal-body {
  max-height: 70vh;
  overflow-y: auto;
}

.alert {
  white-space: pre-line;
}
</style>
