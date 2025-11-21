<script setup>
import { RouterLink, RouterView } from 'vue-router'
import { ref, onMounted } from 'vue'

const isLoggedIn = ref(false)

onMounted(() => {
  checkAuth()
  window.addEventListener('storage', checkAuth)
})

function checkAuth() {
  isLoggedIn.value = !!localStorage.getItem('authToken')
}
</script>

<template>
  <header>
    <div class="wrapper">
      <nav>
        <RouterLink to="/">Home</RouterLink>
        <RouterLink v-if="isLoggedIn" to="/dashboard">Dashboard</RouterLink>
        <RouterLink v-if="!isLoggedIn" to="/login">Login</RouterLink>
        <RouterLink v-if="!isLoggedIn" to="/register">Register</RouterLink>
      </nav>
    </div>
  </header>

  <main>
    <RouterView @vnode-mounted="checkAuth" />
  </main>
</template>