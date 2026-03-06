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
      class="card-header"
      @click="emit('toggleExpand', setList.id)"
    >
      <div class="d-flex flex-column flex-md-row align-items-start align-items-md-center gap-2 w-100">
        <div class="d-flex align-items-center flex-grow-1 flex-md-grow-0 w-100 w-md-auto">
          <i
            class="bi flex-shrink-0"
            :class="[isExpanded ? 'bi-chevron-up' : 'bi-chevron-down']"
          />
          <h5 class="mb-0 mx-3 text-break flex-grow-1">{{ setList.title }}</h5>
        </div>

        <div class="d-flex align-items-center gap-2 ms-0 ms-md-auto w-100 w-md-auto">
          <span class="badge text-bg-secondary flex-shrink-0 me-auto me-md-0">
            Arhivēts: {{ formatDate(setList.archivedAt) }}
          </span>

          <div class="d-flex gap-1 flex-shrink-0 ms-auto">
            <button
              class="btn btn-action btn-sm btn-outline-secondary"
              title="Atjaunot dziesmu sarakstu"
              @click.stop="emit('restore', setList)"
            >
              <i class="bi bi-box-arrow-up bi-no-text" />
            </button>
            <button
              class="btn btn-action btn-sm btn-outline-danger"
              title="Dzēst dziesmu sarakstu"
              @click.stop="emit('remove', setList)"
            >
              <i class="bi bi-trash bi-no-text" />
            </button>
          </div>
        </div>
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
          <tr v-for="item in setList.items" :key="item.sheetMusicId">
            <td>{{ item.order + 1 }}</td>
            <td class="text-break">{{ item.sheetMusic?.getFormattedTitle() || "N/A" }}</td>
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

.w-md-auto {
  @media (min-width: 768px) {
    width: auto !important;
  }
}

.btn-action {
  min-width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  padding: 0 0.5rem;
}
</style>