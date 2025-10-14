<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const showButton = ref(false)
let lastScrollY = window.scrollY
let fadeTimeout = null

// Handles scroll direction and button visibility
function onScroll() {
  const currentY = window.scrollY
  const direction = currentY < lastScrollY ? 'up' : 'down'

  if (direction === 'up') {
    showButton.value = true
    // Reset fade timeout on upward scroll
    if (fadeTimeout) clearTimeout(fadeTimeout)
    fadeTimeout = setTimeout(() => {
      showButton.value = false
    }, 3000)
  } else if (direction === 'down') {
    // Hide button immediately on downward scroll
    showButton.value = false
    if (fadeTimeout) clearTimeout(fadeTimeout)
  }

  lastScrollY = currentY
}

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => {
  window.addEventListener('scroll', onScroll, { passive: true })
})

onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
  if (fadeTimeout) clearTimeout(fadeTimeout)
})
</script>

<template>
  <div>
    <transition name="fade">
      <button
        v-if="showButton"
        class="btn btn-secondary position-fixed back-to-top d-flex align-items-center justify-content-center"
        @click="scrollToTop"
        aria-label="Back to top"
      >
        <i class="bi bi-arrow-up"></i>
      </button>
    </transition>
  </div>
</template>

<style scoped lang="scss">
.back-to-top {
  bottom: 1rem;
  right: 1rem;
  border-radius: 0.3rem;
  opacity: 0.65;
  min-width: 2.5rem;
  min-height: 2.5rem;
  font-size: 1rem;
  padding: 0;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  z-index: 1050;
}
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>