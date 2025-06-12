<script setup>
import { computed, ref, watch, defineProps, defineEmits } from "vue";
import {
  BModal,
  BButton,
  BForm,
  BFormGroup,
  BFormInput,
  BFormCheckbox,
  BAlert,
} from "bootstrap-vue-next";
import { NoteSheet } from "@/models/sheetModels";
import { updateNoteSheet, createNoteSheet } from "@/api/noteSheetApi";

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

const showModal = computed({
  get: () => props.show,
  set: (value) => emit("update:show", value),
});

const formData = ref(new NoteSheet());
const selectedFile = ref(null);
const fileError = ref("");

const isValid = ref(true);
const validationMessage = ref("");

const isCreateMode = computed(() => !formData.value.id);

watch(
  () => props.sheet,
  (newSheet) => {
    if (newSheet) {
      formData.value = new NoteSheet(newSheet);
    } else {
      formData.value = new NoteSheet();
    }
    selectedFile.value = null;
    fileError.value = "";
  },
  { immediate: true, deep: true },
);

function handleFileChange(event) {
  const file = event.target?.files?.[0];
  if (file) {
    if (file.type !== 'application/pdf') {
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

  if (!formData.value.title.trim()) {
    isValid.value = false;
    validationMessage.value = "Nosaukums ir obligāts.";
    return false;
  }

  if (formData.value.year !== null && formData.value.year !== "") {
    const yearNum = Number(formData.value.year);
    if (isNaN(yearNum)) {
      isValid.value = false;
      validationMessage.value = "Gadam jābūt skaitlim.";
      return false;
    }
    if (yearNum <= 0) {
      isValid.value = false;
      validationMessage.value = "Gadam jābūt lielākam par 0.";
      return false;
    }
    
    formData.value.year = yearNum;
  } else {
    formData.value.year = null;
  }
  
  if (isCreateMode.value && !selectedFile.value) {
    isValid.value = false;
    validationMessage.value = "Nepieciešams pievienot failu.";
    return false;
  }

  return true;
}

const loading = ref(false);
async function handleSave() {
  if (!validateForm()) {
    return;
  }
    
  let savedSheet;
  
  loading.value = true;
  if (isCreateMode.value) {
    savedSheet = await createNoteSheet(formData.value, selectedFile.value);
  } else {
    savedSheet = await updateNoteSheet(formData.value, selectedFile.value);
  }
  loading.value = false;
  showModal.value = false;
  emit("save", savedSheet);
  
  clearFileSelection();
}

function handleClose() {
  isValid.value = true;
  
  emit("close");
}

function handleInputBlur(field) {
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
  <BModal
    :model-value="showModal"
    @update:model-value="showModal = $event"
    :title="isCreateMode ? 'Pievienot notis' : 'Rediģēt notis'"
    centered
    size="lg"
    @hidden="handleClose"
    :content-class="{ 'state-loading': loading }"
  >
    <BForm @submit.prevent="handleSave">
      <BAlert v-if="!isValid" variant="danger" show>
        {{ validationMessage }}
      </BAlert>

      <BFormGroup label="Nosaukums *" label-for="title-input">
        <BFormInput
          id="title-input"
          v-model="formData.title"
          placeholder="Ievadiet dziesmas nosaukumu"
          required
          @blur="handleInputBlur('title')"
          :state="formData.title.trim() ? null : false"
        />
      </BFormGroup>

      <BFormGroup class="pt-2" label="Mūzikas autors" label-for="author-input">
        <BFormInput
          id="author-input"
          v-model="formData.author"
          placeholder="Ievadiet mūzikas autoru"
        />
      </BFormGroup>

      <BFormGroup class="pt-2" label="Vārdu autors" label-for="lyricist-input">
        <BFormInput
          id="lyricist-input"
          v-model="formData.lyricist"
          placeholder="Ievadiet vārdu autoru"
        />
      </BFormGroup>

      <BFormGroup class="pt-2" label="Gads" label-for="year-input">
        <BFormInput
          id="year-input"
          v-model="formData.year"
          type="text"
          inputmode="numeric"
          placeholder="Ievadiet gadu"
        />
      </BFormGroup>

      <BFormGroup class="py-2">
        <BFormCheckbox v-model="formData.isLatvian" id="latvian-checkbox">
          Latviešu dziesma
        </BFormCheckbox>
      </BFormGroup>

      <BFormGroup 
        class="pt-2" 
        :label="`PDF fails${isCreateMode ? ' *' : ''}`"
        label-for="file-input"
      >
        <div v-if="formData.filename && !selectedFile" class="d-flex align-items-center mb-2">
          <i class="bi bi-file-earmark-pdf text-danger fs-4 me-2"></i>
          <span>{{ formData.filename }}</span>
          <a :href="`/api/download/${formData.id}/${formData.filename}`" target="_blank" class="ms-2 text-decoration-none">
            <i class="bi bi-download"></i>
          </a>
        </div>

        <div v-if="selectedFile" class="d-flex align-items-center mb-2">
          <i class="bi bi-file-earmark-pdf text-danger fs-4 me-2"></i>
          <span>{{ selectedFile.name }}</span>
          <button type="button" class="btn btn-sm text-danger" @click="clearFileSelection">
            <i class="bi bi-x-circle"></i>
          </button>
        </div>

        <div class="input-group">
          <input
            type="file"
            class="form-control"
            id="file-input"
            accept="application/pdf"
            @change="handleFileChange"
            :class="{ 'is-invalid': fileError }"
          />
        </div>
        
        <div v-if="fileError" class="invalid-feedback d-block">
          {{ fileError }}
        </div>
      </BFormGroup>
    </BForm>

    <template #footer>
      <div class="w-100 d-flex justify-content-between">
        <BButton variant="secondary" @click="showModal = false">
          Atcelt
        </BButton>
        <BButton variant="primary" @click="handleSave">
          Saglabāt
        </BButton>
      </div>
    </template>
  </BModal>
</template>

<style scoped lang="scss">
.modal-body {
  max-height: 70vh;
  overflow-y: auto;
}
</style>
