<script setup>
import RenameFilenames from "@/components/Admin/Functions/RenameFilenames.vue";
import AdminHeader from "@/components/Admin/AdminHeader.vue";
import ChangePassword from "@/components/Admin/Functions/ChangePassword.vue";
</script>

<template>
  <AdminHeader />
  <div class=" bg-secondary-subtle">
    <div class="container container-fluid min-vh-100 pt-5">
      <h1>Administrēšanas darbības</h1>
      <hr />
      <RenameFilenames />
      <hr />
      <ChangePassword />
    </div>
  </div>
</template>
