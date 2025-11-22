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
    <header class="app-header">
      <div class="container">
        <div class="header-content">
          <RouterLink to="/" class="logo">
            <span class="logo-icon">🔗</span>
            <span class="logo-text">URL Shortener</span>
          </RouterLink>

          <nav class="nav">
            <RouterLink to="/" class="nav-link">Home</RouterLink>
            <RouterLink v-if="isLoggedIn" to="/dashboard" class="nav-link">Dashboard</RouterLink>
            <RouterLink v-if="!isLoggedIn" to="/login" class="nav-link">Login</RouterLink>
            <RouterLink v-if="!isLoggedIn" to="/register" class="nav-link">Register</RouterLink>
            <button v-if="isLoggedIn" @click="logout" class="nav-link logout-btn">Logout</button>
          </nav>
        </div>
      </div>
    </header>

    <main class="app-main">
      <RouterView @vue:mounted="checkAuth" @vue:updated="checkAuth" />
    </main>

    <footer class="app-footer">
      <div class="container">
        <p>&copy; 2025 URL Shortener. All rights reserved.</p>
      </div>
    </footer>
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

/* Header */
.app-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  text-decoration: none;
  color: #667eea;
  font-weight: 700;
  font-size: 1.25rem;
}

.logo-icon {
  font-size: 1.5rem;
}

.nav {
  display: flex;
  gap: 1.5rem;
  align-items: center;
}

.nav-link {
  text-decoration: none;
  color: #555;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  transition: all 0.3s ease;
  border: none;
  background: none;
  cursor: pointer;
  font-size: 1rem;
}

.nav-link:hover {
  background: rgba(102, 126, 234, 0.1);
  color: #667eea;
}

.nav-link.router-link-active {
  background: #667eea;
  color: white;
}

.logout-btn {
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
}

.logout-btn:hover {
  background: rgba(239, 68, 68, 0.2);
}

/* Main */
.app-main {
  flex: 1;
  padding: 2rem 0;
}

/* Footer */
.app-footer {
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  padding: 1.5rem 0;
  text-align: center;
  color: #666;
  font-size: 0.9rem;
}

/* Responsive */
@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    gap: 1rem;
  }

  .nav {
    gap: 0.75rem;
    flex-wrap: wrap;
    justify-content: center;
  }

  .nav-link {
    padding: 0.5rem 0.75rem;
    font-size: 0.9rem;
  }
}
</style>