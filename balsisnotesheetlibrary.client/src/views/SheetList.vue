<script setup></script>

<template>
  <div class="">
    <div class="my-3 d-flex justify-content-center">
      <img src="../assets/img/balsis_logo.png" alt="Balsis Logo" class="logo" />
    </div>
    <div class="section-links py-5 d-flex justify-content-center">
      <div class="row flex-grow-1">
        <div class="col-4 d-flex justify-content-center">
          <a
            href="#active-sheets"
            class="d-flex flex-column align-items-center"
          >
            <img
              src="../assets/img/aktuals.png"
              alt="Saite uz aktuālajām notīm"
            />
            <h4 class="text-white text-decoration-none text-center">
              Aktuālais
            </h4>
          </a>
        </div>
        <div class="col-4 d-flex justify-content-center">
          <a href="#lv-sheets" class="d-flex flex-column align-items-center">
            <img
              src="../assets/img/latviesi.png"
              alt="Saite uz Latviešu komponistu notīm"
            />
            <h4 class="text-white text-decoration-none text-center">
              Latviešu skaņdarbi
            </h4>
          </a>
        </div>
        <div class="col-4 d-flex justify-content-center">
          <a
            href="#foreign-sheets"
            class="d-flex flex-column align-items-center"
          >
            <img
              src="../assets/img/pasaule.png"
              alt="Saite uz ārzemju komponistu notīm"
            />
            <h4 class="text-white text-decoration-none text-center">
              Ārzemju skaņdarbi
            </h4>
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.logo {
  max-width: 600px;
  width: 100%;
  height: auto;
}

.section-links {
  background-color: #c0392b;
  & > .row {
    max-width: 1200px;
  }
}
</style>
