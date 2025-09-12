<script setup>

import RenameFilenames from "@/components/Admin/Functions/RenameFilenames.vue";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
</script>

<template>
  <AdminHeader />
  <div class="container mt-5">
    <h1>Administrēšanas darbības</h1>
    <hr/>
    <RenameFilenames />
  </div>
</template>

<style scoped lang="scss">

</style>