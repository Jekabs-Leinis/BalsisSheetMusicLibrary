<script setup>
import { ref } from 'vue';
import EditSheet from './EditSheet.vue';
import { NoteSheet } from "@/models/sheetModels";

const showModal = ref(false);
const emptySheet = ref(new NoteSheet());

function openModal() {
  showModal.value = true;
}

function handleSaved(sheet) {
  showModal.value = false;
  emit("sheet-created", sheet);
  emptySheet.value = new NoteSheet(); 
}

const emit = defineEmits(["sheet-created"]);
</script>

<template>
  <div>
    <button class="btn btn-primary" @click="openModal">
      <i class="bi bi-plus-circle me-2"></i>
      Pievienot notis
    </button>
    
    <EditSheet
      v-model:show="showModal"
      :sheet="emptySheet"
      @save="handleSaved"
    />
  </div>
</template>
