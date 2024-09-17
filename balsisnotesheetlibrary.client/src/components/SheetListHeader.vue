<script setup>
//TODO add logout button (here?)
</script>

<template>
  <div class="m-3 d-flex justify-content-center logo-container">
    <img alt="Balsis Logo" class="logo" src="../assets/img/balsis_logo.png" />
  </div>
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

.logo-container {
  position: sticky;
  top: 16px;
  z-index: -1;
}

.logo {
  max-width: 600px;
  width: 100%;
  height: auto;
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
