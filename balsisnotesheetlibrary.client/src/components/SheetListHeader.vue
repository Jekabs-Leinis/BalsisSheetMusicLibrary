<script setup>
import { useUserStore } from "@/stores/userStore";
import { logout } from "@/services/authenticationService";

const userStore = useUserStore();

const isAdmin = userStore.isLoggedIn && userStore.currentUser.isAdmin;
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container-fluid">
      <a class="navbar-brand" href="#">
        <img
          alt="Balsis Logo"
          src="../assets/img/balsis_logo.png"
          style="width: 100px"
        />
        <span class="d-none d-sm-inline ps-3">Nošu bibliotēka</span>
      </a>
      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
        aria-controls="navbarNav"
        aria-expanded="false"
        aria-label="Toggle navigation bar"
      >
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarNav">
        <form class="d-flex my-3 me-auto" role="search">
          <div class="input-group search-box">
            <span class="input-group-text" id="basic-addon1">
              <i class="bi bi-search" />
            </span>
            <input
              class="form-control me-2"
              type="search"
              placeholder="Meklē dziesmu"
              aria-label="Meklē dziesmu"
            />
          </div>
        </form>
        <ul class="navbar-nav">
          <li class="nav-item" v-if="isAdmin">
            <a class="nav-link" href="#">Admin</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" href="#" @click="logout">Iziet</a>
          </li>
        </ul>
      </div>
    </div>
  </nav>
  <div class="section-links py-md-5 py-sm-3 py-2 d-flex justify-content-center">
    <div class="row flex-grow-1">
      <div class="col-4 d-flex justify-content-center">
        <!--suppress HtmlUnknownAnchorTarget -->
        <a class="d-flex flex-column align-items-center" href="#active-sheets">
          <img
            alt="Saite uz aktuālajām notīm"
            src="../assets/img/aktuals.png"
          />
          <h4 class="text-white text-decoration-none text-center">Aktuālais</h4>
        </a>
      </div>
      <div class="col-4 d-flex justify-content-center">
        <!--suppress HtmlUnknownAnchorTarget -->
        <a class="d-flex flex-column align-items-center" href="#lv-sheets">
          <img
            alt="Saite uz Latviešu komponistu notīm"
            src="../assets/img/latviesi.png"
          />
          <h4 class="text-white text-decoration-none text-center">
            Latviešu skaņdarbi
          </h4>
        </a>
      </div>
      <div class="col-4 d-flex justify-content-center">
        <!--suppress HtmlUnknownAnchorTarget -->
        <a class="d-flex flex-column align-items-center" href="#foreign-sheets">
          <img
            alt="Saite uz ārzemju komponistu notīm"
            src="../assets/img/pasaule.png"
          />
          <h4 class="text-white text-decoration-none text-center">
            Ārzemju skaņdarbi
          </h4>
        </a>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
@import "bootstrap/scss/functions";
@import "bootstrap/scss/variables";
@import "bootstrap/scss/mixins";
@import "bootstrap-icons/font/bootstrap-icons.css";

.search-box {
  width: 300px;

  //noinspection SassScssUnresolvedMixin
  @include media-breakpoint-down(lg) {
    width: 100%;
  }

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
      opacity: 0.65;
    }
  }
}

//noinspection SassScssUnresolvedMixin
@include media-breakpoint-down(md) {
  img {
    max-width: 150px;
  }

  .row {
    --bs-gutter-x: -0.75rem;
  }

  h4 {
    font-size: 1rem;
  }
}

//noinspection SassScssUnresolvedMixin
@include media-breakpoint-down(sm) {
  img {
    max-width: 100px;
  }
}

a {
  text-decoration: none;
}

.section-links {
  background-color: #c0392b;

  & > .row {
    max-width: 1200px;
  }
}
</style>
