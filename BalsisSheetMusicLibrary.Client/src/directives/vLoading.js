// Modified from https://codesandbox.io/p/sandbox/nostalgic-noyce-18306n50rj?file=%2Fsrc%2FApp.vue%3A10%2C9
export default {
  mounted(el, binding) {
    updateLoadingState(el, binding);
  },
  updated(el, binding) {
    updateLoadingState(el, binding);
  },
  unmounted(el) {
    updateLoadingState(el, { value: false });
  }
};

function updateLoadingState(el, binding) {
  if (binding.value) {
    el.classList.add("state-loading");
    if (binding.modifiers.bg) {
      el.classList.add("state-loading-bg");
    }
    el.setAttribute("disabled", "disabled");
  } else {
    el.classList.remove("state-loading", "state-loading-bg");
    el.removeAttribute("disabled");
  }
}
