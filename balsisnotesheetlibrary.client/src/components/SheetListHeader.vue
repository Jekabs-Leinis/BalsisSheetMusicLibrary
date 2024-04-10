<script setup>
//TODO add logout button (here?)
</script>

<template>
  <div class="my-3 d-flex justify-content-center">
    <img alt="Balsis Logo" class="logo" src="../assets/img/balsis_logo.png" />
  </div>
  <div class="section-links py-5 d-flex justify-content-center">
    <div class="row flex-grow-1">
      <div class="col-4 d-flex justify-content-center">
        <a class="d-flex flex-column align-items-center" href="#active-sheets">
          <img
            alt="Saite uz aktuālajām notīm"
            src="../assets/img/aktuals.png"
          />
          <h4 class="text-white text-decoration-none text-center">Aktuālais</h4>
        </a>
      </div>
      <div class="col-4 d-flex justify-content-center">
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
