<script setup>
import { useRoute } from "vue-router";
const route = useRoute();
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark sticky-top">
    <div class="container-fluid d-flex flex-nowrap">
      <a class="navbar-brand" href="/">
        <img
          alt="Balsis Logo"
          src="@/static/img/balsis_logo.png"
          class="balsis-logo"
        />
        <span class="d-none d-lg-inline ps-3 fw-semibold">Administrācija</span>
      </a>
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
        id="offcanvasNavbar"
        class="offcanvas offcanvas-end bg-dark text-white flex-md-grow-0"
        tabindex="-1"
        aria-labelledby="offcanvasNavbarLabel"
      >
        <div class="offcanvas-header">
          <h5 id="offcanvasNavbarLabel" class="offcanvas-title">
            Administrācija
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
              <RouterLink
                :to="{ name: 'AdminFunctions' }"
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="/admin"
                :class="{
                  'text-decoration-underline': route.path === '/admin',
                }"
              >
                Funkcijas
              </RouterLink>
            </li>
            <li class="nav-item">
              <RouterLink
                :to="{ name: 'AdminSheetList' }"
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="/admin/sheets"
                :class="{
                  'text-decoration-underline': route.path === '/admin/sheets',
                }"
              >
                Notis
              </RouterLink>
            </li>
            <li class="nav-item">
              <RouterLink
                :to="{ name: 'EditSetLists' }"
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="/admin/edit-set-lists"
                :class="{
                  'text-decoration-underline':
                    route.path === '/admin/edit-set-lists',
                }"
              >
                Dziesmu saraksti
              </RouterLink>
            </li>
            <li class="nav-item">
              <RouterLink
                :to="{ name: 'Archive' }"
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="/admin/archive"
                :class="{
                  'text-decoration-underline': route.path === '/admin/archive',
                }"
              >
                Arhīvs
              </RouterLink>
            </li>
            <li class="nav-item flex-grow-1 ms-0" />
            <li class="nav-item">
              <RouterLink
                :to="{ name: 'SheetListView' }"
                class="nav-link text-nowrap text-end pe-2 pe-lg-0 text-light"
                href="/"
                >Uz vietni</RouterLink
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
  background-color: var(--color-bluegray);
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
