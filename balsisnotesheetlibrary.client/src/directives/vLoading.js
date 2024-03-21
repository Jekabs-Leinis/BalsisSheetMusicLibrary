// From https://codesandbox.io/p/sandbox/nostalgic-noyce-18306n50rj?file=%2Fsrc%2FApp.vue%3A10%2C9
export default function (el, binding) {
  if (binding.value) {
    el.classList.add("state-loading");
    el.setAttribute("disabled", "disabled");
  } else {
    el.classList.remove("state-loading");
    el.removeAttribute("disabled");
  }
}