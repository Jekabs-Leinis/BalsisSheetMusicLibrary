<script setup>
import { useSheetMusicStore } from "@/stores/sheetMusicStore.js";
import router from "@/router/routes";
import { ref, watch, onBeforeUnmount } from "vue";
import _debounce from "lodash.debounce";
import { useAuthStore } from "@/stores/authStore.js";
import { useToast } from "vue-toastification";

const authStore = useAuthStore();
const sheetMusicStore = useSheetMusicStore();
const toast = useToast();

const isAdmin = authStore.isAuthenticated && authStore.user.isAdmin;
const searchInput = ref("");

function onSearch(query) {
  sheetMusicStore.setSearchQuery(query);
}

const debouncedSearch = _debounce(onSearch, 300);

watch(searchInput, (query) => debouncedSearch(query));

onBeforeUnmount(() => {
  sheetMusicStore.setSearchQuery("");
});

async function attemptLogout() {
  try {
    await authStore.logout();
    router.push({ name: "Login" });
  } catch (error) {
    console.error("Logout error:", error);
    toast.error(`Kļūda izrakstoties: ${error.message}`);
  }
}
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark sticky-top">
    <div class="container-fluid d-flex flex-nowrap">
      <a class="navbar-brand" href="#">
        <img
          alt="Balsis Logo"
          src="@/static/img/balsis_logo.png"
          class="balsis-logo"
        />
        <span class="d-none d-lg-inline ps-3 fw-semibold">Nošu bibliotēka</span>
      </a>
      <form class="d-flex my-3 flex-grow-1" role="search" @submit.prevent>
        <div class="input-group search-box">
          <span class="input-group-text pe-0" id="basic-addon1">
            <i class="bi bi-search" />
          </span>
          <input
            v-model="searchInput"
            class="form-control me-2"
            type="search"
            placeholder="Meklē dziesmu"
            aria-label="Meklē dziesmu"
          />
          <i
            v-if="searchInput"
            class="bi bi-x-lg search-clear-btn"
            aria-label="Clear search"
            @click="searchInput = ''"
          />
        </div>
      </form>
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="offcanvas"
        data-bs-target="#offcanvasNavbar"
        aria-controls="offcanvasNavbar"
        aria-expanded="false"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>
      <div
        class="offcanvas offcanvas-end bg-dark text-white flex-md-grow-0"
        tabindex="-1"
        id="offcanvasNavbar"
        aria-labelledby="offcanvasNavbarLabel"
      >
        <div class="offcanvas-header">
          <h5 class="offcanvas-title" id="offcanvasNavbarLabel">
            Nošu bibliotēka
          </h5>
          <button
            type="button"
            class="btn-close btn-close-white"
            data-bs-dismiss="offcanvas"
            aria-label="Close"
          />
        </div>
        <div class="offcanvas-body">
          <ul class="navbar-nav j mb-2 mb-lg-0 d-flex h-100">
            <li class="nav-item">
              <a
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="#lv-sheets"
                >Latviešu skaņdarbi</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="#foreign-sheets"
                >Ārzemju skaņdarbi</a
              >
            </li>
            <li class="nav-item text-end" v-if="isAdmin">
              <RouterLink
                :to="{ name: 'AdminFunctions' }"
                class="nav-link text-nowrap pe-2 pe-lg-0 text-light"
                href="/admin"
              >
                Admin
              </RouterLink>
            </li>
            <li class="nav-item flex-grow-1" />
            <li class="m-0">
              <a
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="#"
                @click="attemptLogout"
                >Iziet</a
              >
            </li>
          </ul>
        </div>
      </div>
    </div>
  </nav>
</template>

<style lang="scss" scoped>
@import "bootstrap/scss/functions";
@import "bootstrap/scss/variables";
@import "bootstrap/scss/mixins";

.search-box {
  position: relative;

  span,
  input {
    background-color: transparent;
    border: 0;
    border-bottom: 1px solid white;
    border-radius: 0;
    color: white;

    &:focus {
      outline: none;
      box-shadow: none;
    }

    &::placeholder {
      color: white;
      opacity: 0.75;
    }
  }

  .search-clear-btn {
    position: absolute;
    right: 10px;
    top: 55%;
    transform: translateY(-50%);
    z-index: 10;
    color: white;
    cursor: pointer;
    font-size: 1.25rem;
    opacity: 0.75;
    padding: 0.25rem;

    &:hover {
      opacity: 1;
    }
  }
}

a {
  text-decoration: none;
}

// Overriding Bootstrap styles for offcanvas component
.offcanvas-end {
  max-width: 80%;
}

.navbar {
  background-color: var(--balsis-red);
}

.nav-link:hover {
  font-weight: bold;
}

// Ensuring nav items are properly spaced in larger viewports
@media (min-width: 992px) {
  .navbar-nav {
    .nav-item {
      margin-left: 1rem;
    }
  }
}

.balsis-logo {
  width: 80px;
}
</style>
