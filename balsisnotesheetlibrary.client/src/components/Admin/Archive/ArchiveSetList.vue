<script setup>
defineProps({
  /** @type {SetList} */
  setList: {
    type: Object,
    required: true,
  },
  isExpanded: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["toggleExpand", "restore", "remove"]);
const formatDate = (dateString) => {
  if (!dateString) return "N/A";
  const date = new Date(dateString);
  return date.toLocaleDateString("lv-LV", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
};
</script>

<template>
  <div class="setlist-item card mb-3">
    <div
      class="card-header d-flex justify-content-between align-items-center"
      @click="emit('toggleExpand', setList.id)"
    >
      <i
        class="bi"
        :class="[isExpanded ? 'bi-chevron-up' : 'bi-chevron-down']"
      />
      <div class="d-flex align-items-center flex-grow-1">
        <h5 class="mb-0 mx-3">{{ setList.title }}</h5>
        <span class="badge text-bg-secondary">
          Arhivēts: {{ formatDate(setList.archivedAt) }}
        </span>
        <div class="flex-grow-1" />
        <button
          class="btn btn-icon btn-action btn-sm btn-outline-secondary me-1"
          title="Atjaunot dziesmu sarakstu"
          @click.stop="emit('restore', setList)"
        >
          <i class="bi bi-arrow-counterclockwise" />
        </button>
        <button
          class="btn btn-icon btn-action btn-sm btn-outline-danger"
          title="Dzēst dziesmu sarakstu"
          @click.stop="emit('remove', setList)"
        >
          <i class="bi bi-trash" />
        </button>
      </div>
    </div>
    <div v-if="isExpanded" class="card-body">
      <div v-if="setList.items.length > 0">
        <table class="table table-hover">
          <thead>
            <tr>
              <th>#</th>
              <th>Nosaukums</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in setList.items" :key="item.noteSheetId">
              <td>{{ item.order + 1 }}</td>
              <td>{{ item.noteSheet?.getFormattedTitle() || "N/A" }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="text-muted">Šajā dziesmu sarakstā nav dziesmu</div>
    </div>
  </div>
</template>

<style scoped>
.card-header {
  transition: background-color 0.2s ease-in-out;
  cursor: pointer;
}

.card-header:hover {
  /* Matches bootstrap's table row hover color */
  background-color: rgba(0, 0, 0, 0.075);
}
</style>
