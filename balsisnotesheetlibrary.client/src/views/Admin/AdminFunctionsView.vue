<script setup>
import RenameFilenames from "@/components/Admin/Functions/RenameFilenames.vue";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
</script>

<template>
  <AdminHeader />
  <div class=" bg-secondary-subtle">
    <div class="container container-fluid min-vh-100 pt-5">
      <h1>Administrēšanas darbības</h1>
      <hr />
      <RenameFilenames />
    </div>
  </div>
</template>
