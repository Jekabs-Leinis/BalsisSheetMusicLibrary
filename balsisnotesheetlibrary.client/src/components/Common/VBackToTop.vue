<script setup>
import { ref, onMounted, onUnmounted } from "vue";

const showButton = ref(false);
let lastScrollY = window.scrollY;
let fadeTimeout = null;

// Handles scroll direction and button visibility
function onScroll() {
  const currentY = window.scrollY;
  const direction = currentY < lastScrollY ? "up" : "down";

  if (direction === "up") {
    showButton.value = true;
    // Reset fade timeout on upward scroll
    if (fadeTimeout) clearTimeout(fadeTimeout);
    fadeTimeout = setTimeout(() => {
      showButton.value = true;
    }, 3000);
  } else if (direction === "down") {
    // Hide button immediately on downward scroll
    showButton.value = false;
    if (fadeTimeout) clearTimeout(fadeTimeout);
  }

  lastScrollY = currentY;
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: "smooth" });
}

onMounted(() => {
  window.addEventListener("scroll", onScroll, { passive: true });
});

onUnmounted(() => {
  window.removeEventListener("scroll", onScroll);
  if (fadeTimeout) clearTimeout(fadeTimeout);
});
</script>

<template>
  <div>
    <transition name="fade">
      <div v-if="showButton" class="position-fixed d-block back-to-top">
        <a
          class="btn btn-secondary d-flex justify-content-center align-items-center btn__scroll"
          @click="scrollToTop"
          aria-label="Back to top"
        >
          <i class="bi bi-arrow-up" />
        </a>
      </div>
    </transition>
  </div>
</template>

<style scoped lang="scss">
.back-to-top {
  bottom: 1rem;
  right: 1rem;
  opacity: 0.65;
  z-index: 1050;
}

.btn__scroll {
  line-height: 1 !important;
  padding: 0;
  width: 2.5rem;
  aspect-ratio: 1;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

</style>