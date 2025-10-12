<script setup>
import { computed, ref } from "vue";
import { useSetListStore } from "@/stores/setlistStore";
import { SetList } from "@/models/sheetModels.js";

const props = defineProps({
  /** @type {SetList} */
  setList: {
    type: Object,
    required: true,
  },
  setListCount: {
    type: Number,
    required: true,
  },
});

const emit = defineEmits(["archive", "delete"]);

const setListStore = useSetListStore();
const isEditing = ref(false);
const editedTitle = ref("");
const isLoading = ref(false);
const error = ref("");

const isValid = computed(() => {
  return editedTitle.value.trim() !== "" && editedTitle.value.length <= 200;
});

const startEditing = () => {
  editedTitle.value = props.setList.title;
  isEditing.value = true;
};

const cancelEditing = () => {
  isEditing.value = false;
  error.value = "";
};

const saveTitle = async () => {
  if (!isValid.value) return;

  isLoading.value = true;
  error.value = "";

  try {
    await setListStore.saveSetList(
      new SetList({
        ...props.setList,
        title: editedTitle.value.trim(),
      }),
    );
    isEditing.value = false;
  } catch (err) {
    console.error("Failed to update set list title:", err);
    error.value = "Failed to update title. Please try again.";
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="editable-title d-flex align-items-center w-100">
    <div v-if="!isEditing" class="d-flex align-items-center flex-grow-1">
      <h5 class="mb-0 me-2">{{ setList.title }}</h5>
      <button
        :disabled="isLoading"
        aria-label="Edit title"
        class="btn btn-action btn-sm ms-1"
        title="Rediģēt"
        @click="startEditing"
      >
        <i class="bi bi-pencil-square"></i>
      </button>
    </div>

    <div v-else class="edit-mode d-flex align-items-center flex-grow-1 gap-2">
      <div class="flex-grow-1 position-relative">
        <div class="input-group">
          <input
            v-model="editedTitle"
            :disabled="isLoading"
            aria-label="Edit set list title"
            class="form-control"
            type="text"
            @keyup.enter="isValid && saveTitle()"
            @keyup.esc="cancelEditing"
          />
          <button
            :disabled="isLoading"
            aria-label="Cancel editing"
            class="btn btn-action btn-sm btn-secondary"
            title="Atcelt rediģēšanu"
            type="button"
            @click="cancelEditing"
          >
            <i class="bi bi-x-lg"></i>
          </button>
          <button
            :disabled="!isValid || isLoading"
            aria-label="Save changes"
            class="btn btn-action btn-sm btn-primary"
            title="Saglabāt izmaiņas"
            type="button"
            @click="saveTitle"
          >
            <i class="bi bi-check-lg"></i>
          </button>
        </div>
        <div v-if="error" class="invalid-feedback d-block">
          {{ error }}
        </div>
      </div>
    </div>
    <div class="flex-grow-1" />
    <div class="action-buttons d-flex align-items-center gap-1 ms-2">
      <button
        v-if="setList.order > 0"
        :class="[
          setList.order < setListCount - 1 ? 'pe-0' : 'no-down-arrow-padding',
        ]"
        class="btn btn-action btn-sm"
        @click="setListStore.moveSetList(setList.order, setList.order - 1)"
      >
        <i class="bi bi-arrow-up movement-arrows" />
      </button>
      <button
        v-if="setList.order < setListCount - 1"
        class="btn btn-action btn-sm"
        @click="setListStore.moveSetList(setList.order, setList.order + 1)"
      >
        <i class="bi bi-arrow-down movement-arrows" />
      </button>
      <button
        class="btn btn-action btn-sm btn-outline-secondary"
        title="Arhivēt nošu sarakstu"
        @click.stop="emit('archive', setList)"
      >
        <i class="bi bi-archive" />
      </button>
      <button
        class="btn btn-action btn-sm btn-outline-danger"
        title="Dzēst nošu sarakstu"
        @click.stop="emit('delete', setList)"
      >
        <i class="bi bi-trash" />
      </button>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.action-buttons .btn {
  min-width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  padding: 0 0.5rem;
}

.btn-action {
  min-width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  padding: 0 0.5rem;
}

.movement-arrows {
  opacity: 0.75;

  &:hover {
    opacity: 1;
  }
}

.no-down-arrow-padding {
  padding-right: 3.1rem !important;
}
</style>
