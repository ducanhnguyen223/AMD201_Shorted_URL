<script setup>
import { RouterLink, RouterView, useRouter } from 'vue-router'
import { ref, onMounted, watch } from 'vue'

const router = useRouter()
const isLoggedIn = ref(false)

onMounted(() => {
  checkAuth()
})

// Watch route changes to update auth state
watch(() => router.currentRoute.value.path, () => {
  checkAuth()
})

function checkAuth() {
  isLoggedIn.value = !!localStorage.getItem('authToken')
}

function logout() {
  localStorage.clear()
  isLoggedIn.value = false
  router.push('/')
}
</script>

<template>
  <div class="app-container">
    <main class="app-main">
      <RouterView @vue:mounted="checkAuth" @vue:updated="checkAuth" />
    </main>
  </div>
</template>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  min-height: 100vh;
  color: #333;
}

.app-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

/* Main */
.app-main {
  flex: 1;
  padding: 0;
}
</style>