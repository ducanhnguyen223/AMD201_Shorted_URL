<script setup>
import { ref, onMounted, computed, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const isLoading = ref(false);
const errorMessage = ref(null);
const successMessage = ref(null);

// System Stats
const systemStats = ref({
  totalUrls: 0,
  totalUsers: 0,
  totalClicks: 0,
  todayUrls: 0
});

// All URLs (admin view)
const allUrls = ref([]);
const currentPage = ref(1);
const pageSize = ref(10);

// All Users
const allUsers = ref([]);

// Service Health
const serviceHealth = ref({
  shortenerService: { status: 'unknown', responseTime: 0 },
  userService: { status: 'unknown', responseTime: 0 },
  apiGateway: { status: 'unknown', responseTime: 0 },
  database: { status: 'unknown', responseTime: 0 }
});

// Active tab
const activeTab = ref('overview');

// User-like functionality for Admin
const newUrl = ref('');
const newCustomAlias = ref('');
const createUrlLoading = ref(false);
const createUrlError = ref('');
const createUrlSuccess = ref('');

const gatewayBaseUrl = import.meta.env.PROD
  ? 'https://api-gateway-production-e75a.up.railway.app'
  : 'http://localhost:5000';

const userInfo = computed(() => ({
  email: localStorage.getItem('userEmail'),
  fullName: localStorage.getItem('userFullName')
}));

let healthCheckInterval = null;

onMounted(async () => {
  checkAuth();
  await fetchAllData();
  // Auto refresh health every 30 seconds
  healthCheckInterval = setInterval(checkServiceHealth, 30000);
});

onUnmounted(() => {
  if (healthCheckInterval) {
    clearInterval(healthCheckInterval);
  }
});

function checkAuth() {
  const token = localStorage.getItem('authToken');
  if (!token) {
    router.push('/login');
    return;
  }
}

async function fetchAllData() {
  isLoading.value = true;
  try {
    await Promise.all([
      fetchSystemStats(),
      fetchAllUrls(),
      fetchAllUsers(),
      checkServiceHealth()
    ]);
  } catch (error) {
    errorMessage.value = 'Failed to load admin data';
  } finally {
    isLoading.value = false;
  }
}

async function fetchSystemStats() {
  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/admin/stats`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      const data = await response.json();
      systemStats.value = data;
    } else {
      // Mock data for demo
      systemStats.value = {
        totalUrls: allUrls.value.length,
        totalUsers: allUsers.value.length,
        totalClicks: allUrls.value.reduce((sum, url) => sum + (url.accessCount || 0), 0),
        todayUrls: 0
      };
    }
  } catch (error) {
    console.error('Error fetching stats:', error);
  }
}

async function fetchAllUrls() {
  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/admin/urls`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      allUrls.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching URLs:', error);
  }
}

async function fetchAllUsers() {
  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/admin/users`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      allUsers.value = await response.json();
    }
  } catch (error) {
    console.error('Error fetching users:', error);
  }
}

async function checkServiceHealth() {
  const services = [
    { key: 'apiGateway', url: `${gatewayBaseUrl}/health` },
    { key: 'shortenerService', url: `${gatewayBaseUrl}/api/health` },
    { key: 'userService', url: `${gatewayBaseUrl}/api/auth/health` }
  ];

  for (const service of services) {
    const start = Date.now();
    try {
      const response = await fetch(service.url, {
        method: 'GET',
        signal: AbortSignal.timeout(5000)
      });

      if (response.ok) {
        const data = await response.json();
        serviceHealth.value[service.key] = {
          status: data.status === 'healthy' ? 'healthy' : 'unhealthy',
          responseTime: Date.now() - start
        };

        // If this is the shortener service health, update database status
        if (service.key === 'shortenerService') {
          serviceHealth.value.database = {
            status: data.database === 'connected' ? 'healthy' : 'offline',
            responseTime: Date.now() - start
          };
        }
      } else {
        serviceHealth.value[service.key] = {
          status: 'unhealthy',
          responseTime: Date.now() - start
        };
      }
    } catch (error) {
      serviceHealth.value[service.key] = {
        status: 'offline',
        responseTime: 0
      };
    }
  }
}

async function deleteUrl(id) {
  if (!confirm('Are you sure you want to delete this URL? This action cannot be undone.')) {
    return;
  }

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/admin/urls/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      successMessage.value = 'URL deleted successfully';
      allUrls.value = allUrls.value.filter(u => u.id !== id);
      await fetchSystemStats();
    } else {
      throw new Error('Failed to delete URL');
    }
  } catch (error) {
    errorMessage.value = error.message;
  }
}

async function deleteUser(id) {
  if (!confirm('Are you sure you want to delete this user? All their URLs will also be deleted.')) {
    return;
  }

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/admin/users/${id}`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` }
    });

    if (response.ok) {
      successMessage.value = 'User deleted successfully';
      allUsers.value = allUsers.value.filter(u => u.id !== id);
      await fetchAllData();
    } else {
      throw new Error('Failed to delete user');
    }
  } catch (error) {
    errorMessage.value = error.message;
  }
}

async function createShortUrl() {
  if (!newUrl.value) {
    createUrlError.value = 'Vui lòng nhập URL';
    return;
  }

  createUrlLoading.value = true;
  createUrlError.value = '';
  createUrlSuccess.value = '';

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/shorten`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        originalUrl: newUrl.value,
        customAlias: newCustomAlias.value || null
      })
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.title || 'Có lỗi xảy ra');
    }

    createUrlSuccess.value = `Link rút gọn: ${data.shortUrl}`;
    newUrl.value = '';
    newCustomAlias.value = '';

    // Refresh URLs list
    await fetchAllUrls();
    await fetchSystemStats();

    // Auto clear success message after 5 seconds
    setTimeout(() => {
      createUrlSuccess.value = '';
    }, 5000);
  } catch (error) {
    createUrlError.value = error.message;
  } finally {
    createUrlLoading.value = false;
  }
}

function logout() {
  localStorage.clear();
  router.push('/');
}

function truncateUrl(url, maxLength = 50) {
  if (!url) return '';
  if (url.length <= maxLength) return url;
  return url.substring(0, maxLength) + '...';
}

function formatDate(dateString) {
  if (!dateString) return 'N/A';
  return new Date(dateString).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

function getStatusClass(status) {
  switch (status) {
    case 'healthy': return 'status-healthy';
    case 'unhealthy': return 'status-unhealthy';
    case 'offline': return 'status-offline';
    default: return 'status-unknown';
  }
}

const paginatedUrls = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return allUrls.value.slice(start, start + pageSize.value);
});

const totalPages = computed(() => Math.ceil(allUrls.value.length / pageSize.value));
</script>

<template>
  <div class="admin-container">
    <!-- Gradient Background -->
    <div class="gradient-orb orb-1"></div>
    <div class="gradient-orb orb-2"></div>

    <!-- Header -->
    <header class="admin-header glass-morphism">
      <div class="header-content">
        <div class="admin-info">
          <div class="admin-badge">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"></polygon>
            </svg>
            ADMIN
          </div>
          <div class="user-details">
            <h2>Admin Dashboard</h2>
            <p>{{ userInfo.email }}</p>
          </div>
        </div>
        <div class="header-actions">
          <button @click="fetchAllData" class="refresh-btn" title="Refresh">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="23 4 23 10 17 10"></polyline>
              <polyline points="1 20 1 14 7 14"></polyline>
              <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
            </svg>
          </button>
          <button @click="logout" class="logout-btn">
            <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
              <polyline points="16 17 21 12 16 7"></polyline>
              <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
            Logout
          </button>
        </div>
      </div>
    </header>

    <!-- Messages -->
    <transition name="slide-down">
      <div v-if="successMessage" class="message success-message glass-effect">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
          <polyline points="22 4 12 14.01 9 11.01"></polyline>
        </svg>
        {{ successMessage }}
      </div>
    </transition>

    <transition name="shake">
      <div v-if="errorMessage" class="message error-message glass-effect">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"></circle>
          <line x1="15" y1="9" x2="9" y2="15"></line>
          <line x1="9" y1="9" x2="15" y2="15"></line>
        </svg>
        {{ errorMessage }}
      </div>
    </transition>

    <!-- Tab Navigation -->
    <div class="tab-nav glass-morphism">
      <button
        @click="activeTab = 'overview'"
        :class="['tab-btn', { active: activeTab === 'overview' }]"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="3" y="3" width="7" height="7"></rect>
          <rect x="14" y="3" width="7" height="7"></rect>
          <rect x="14" y="14" width="7" height="7"></rect>
          <rect x="3" y="14" width="7" height="7"></rect>
        </svg>
        Overview
      </button>
      <button
        @click="activeTab = 'create'"
        :class="['tab-btn', { active: activeTab === 'create' }]"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"></line>
          <line x1="5" y1="12" x2="19" y2="12"></line>
        </svg>
        Tạo Link
      </button>
      <button
        @click="activeTab = 'urls'"
        :class="['tab-btn', { active: activeTab === 'urls' }]"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.72"></path>
          <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.72-1.72"></path>
        </svg>
        All URLs
      </button>
      <button
        @click="activeTab = 'users'"
        :class="['tab-btn', { active: activeTab === 'users' }]"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
          <circle cx="9" cy="7" r="4"></circle>
          <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
          <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
        </svg>
        Users
      </button>
      <button
        @click="activeTab = 'health'"
        :class="['tab-btn', { active: activeTab === 'health' }]"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M22 12h-4l-3 9L9 3l-3 9H2"></path>
        </svg>
        Health
      </button>
    </div>

    <div class="admin-content">
      <!-- Overview Tab -->
      <div v-if="activeTab === 'overview'" class="tab-content">
        <!-- Stats Grid -->
        <div class="stats-grid">
          <div class="stat-card glass-effect">
            <div class="stat-icon urls">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.72"></path>
                <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.72-1.72"></path>
              </svg>
            </div>
            <div class="stat-info">
              <h3>{{ systemStats.totalUrls }}</h3>
              <p>Total URLs</p>
            </div>
          </div>

          <div class="stat-card glass-effect">
            <div class="stat-icon users">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                <circle cx="9" cy="7" r="4"></circle>
              </svg>
            </div>
            <div class="stat-info">
              <h3>{{ systemStats.totalUsers }}</h3>
              <p>Total Users</p>
            </div>
          </div>

          <div class="stat-card glass-effect">
            <div class="stat-icon clicks">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                <circle cx="12" cy="12" r="3"></circle>
              </svg>
            </div>
            <div class="stat-info">
              <h3>{{ systemStats.totalClicks }}</h3>
              <p>Total Clicks</p>
            </div>
          </div>

          <div class="stat-card glass-effect">
            <div class="stat-icon today">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                <line x1="16" y1="2" x2="16" y2="6"></line>
                <line x1="8" y1="2" x2="8" y2="6"></line>
                <line x1="3" y1="10" x2="21" y2="10"></line>
              </svg>
            </div>
            <div class="stat-info">
              <h3>{{ systemStats.todayUrls }}</h3>
              <p>URLs Today</p>
            </div>
          </div>
        </div>

        <!-- Quick Health Overview -->
        <div class="quick-health glass-morphism">
          <h3>Service Status</h3>
          <div class="health-indicators">
            <div v-for="(health, name) in serviceHealth" :key="name" class="health-indicator">
              <span :class="['status-dot', getStatusClass(health.status)]"></span>
              <span class="service-name">{{ name }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Create Link Tab -->
      <div v-if="activeTab === 'create'" class="tab-content">
        <div class="section glass-morphism">
          <div class="section-header">
            <h3>Tạo Link Rút Gọn</h3>
          </div>

          <form @submit.prevent="createShortUrl" class="create-form">
            <div class="form-group">
              <label>URL gốc *</label>
              <input
                v-model="newUrl"
                type="url"
                placeholder="https://example.com/very-long-url"
                required
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label>Custom Alias (tùy chọn)</label>
              <input
                v-model="newCustomAlias"
                type="text"
                placeholder="my-custom-link"
                class="form-input"
              />
              <small class="form-hint">Để trống để tự động tạo mã ngẫu nhiên</small>
            </div>

            <div v-if="createUrlError" class="alert error">
              {{ createUrlError }}
            </div>

            <div v-if="createUrlSuccess" class="alert success">
              {{ createUrlSuccess }}
            </div>

            <button type="submit" :disabled="createUrlLoading" class="submit-button">
              {{ createUrlLoading ? 'Đang tạo...' : 'Tạo Link Rút Gọn' }}
            </button>
          </form>
        </div>
      </div>

      <!-- All URLs Tab -->
      <div v-if="activeTab === 'urls'" class="tab-content">
        <div class="section glass-morphism">
          <div class="section-header">
            <h3>All Shortened URLs ({{ allUrls.length }})</h3>
          </div>

          <div v-if="allUrls.length === 0" class="empty-state">
            <p>No URLs found</p>
          </div>

          <div v-else class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Short Code</th>
                  <th>Original URL</th>
                  <th>User ID</th>
                  <th>Clicks</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="url in paginatedUrls" :key="url.id">
                  <td>{{ url.id }}</td>
                  <td>
                    <a :href="url.shortUrl" target="_blank" class="short-code">
                      {{ url.shortCode }}
                    </a>
                  </td>
                  <td :title="url.originalUrl">{{ truncateUrl(url.originalUrl, 40) }}</td>
                  <td>{{ url.userId || 'Anonymous' }}</td>
                  <td>{{ url.accessCount }}</td>
                  <td>{{ formatDate(url.createdAt) }}</td>
                  <td>
                    <button @click="deleteUrl(url.id)" class="delete-btn" title="Delete">
                      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="3 6 5 6 21 6"></polyline>
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                      </svg>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>

            <div class="pagination">
              <button
                @click="currentPage--"
                :disabled="currentPage === 1"
                class="page-btn"
              >
                Previous
              </button>
              <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
              <button
                @click="currentPage++"
                :disabled="currentPage === totalPages"
                class="page-btn"
              >
                Next
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Users Tab -->
      <div v-if="activeTab === 'users'" class="tab-content">
        <div class="section glass-morphism">
          <div class="section-header">
            <h3>All Users ({{ allUsers.length }})</h3>
          </div>

          <div v-if="allUsers.length === 0" class="empty-state">
            <p>No users found</p>
          </div>

          <div v-else class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Email</th>
                  <th>Full Name</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in allUsers" :key="user.id">
                  <td>{{ user.id }}</td>
                  <td>{{ user.email }}</td>
                  <td>{{ user.fullName }}</td>
                  <td>{{ formatDate(user.createdAt) }}</td>
                  <td>
                    <button @click="deleteUser(user.id)" class="delete-btn" title="Delete User">
                      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="3 6 5 6 21 6"></polyline>
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                      </svg>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Health Tab -->
      <div v-if="activeTab === 'health'" class="tab-content">
        <div class="section glass-morphism">
          <div class="section-header">
            <h3>Service Health Check</h3>
            <button @click="checkServiceHealth" class="refresh-btn small">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="23 4 23 10 17 10"></polyline>
                <polyline points="1 20 1 14 7 14"></polyline>
                <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
              </svg>
              Refresh
            </button>
          </div>

          <div class="health-grid">
            <div v-for="(health, name) in serviceHealth" :key="name" class="health-card glass-effect">
              <div class="health-header">
                <span class="service-name">{{ name }}</span>
                <span :class="['status-badge', getStatusClass(health.status)]">
                  {{ health.status }}
                </span>
              </div>
              <div class="health-details">
                <span v-if="health.responseTime > 0" class="response-time">
                  {{ health.responseTime }}ms
                </span>
                <span v-else class="response-time">N/A</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-container {
  min-height: 100vh;
  padding: 2rem;
  position: relative;
  overflow-x: hidden;
}

.gradient-orb {
  position: fixed;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.3;
  animation: float 20s ease-in-out infinite;
  pointer-events: none;
}

.orb-1 {
  width: 400px;
  height: 400px;
  background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%);
  top: -10%;
  right: -10%;
}

.orb-2 {
  width: 350px;
  height: 350px;
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  bottom: -10%;
  left: -10%;
  animation-delay: -10s;
}

@keyframes float {
  0%, 100% { transform: translate(0, 0) rotate(0deg); }
  33% { transform: translate(30px, -30px) rotate(120deg); }
  66% { transform: translate(-20px, 20px) rotate(240deg); }
}

.glass-morphism {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
}

.glass-effect {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(10px);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.15);
}

/* Header */
.admin-header {
  margin-bottom: 2rem;
  padding: 1.5rem 2rem;
  position: relative;
  z-index: 10;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.admin-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.admin-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
}

.user-details h2 {
  color: white;
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
}

.user-details p {
  color: rgba(255, 255, 255, 0.6);
  margin: 0.25rem 0 0 0;
  font-size: 0.85rem;
}

.header-actions {
  display: flex;
  gap: 1rem;
}

.refresh-btn {
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.refresh-btn:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: rotate(180deg);
}

.refresh-btn.small {
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
}

.logout-btn {
  padding: 0.75rem 1.5rem;
  background: rgba(239, 68, 68, 0.2);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 12px;
  color: #fca5a5;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.logout-btn:hover {
  background: rgba(239, 68, 68, 0.3);
}

/* Messages */
.message {
  max-width: 1200px;
  margin: 0 auto 1rem;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.success-message {
  background: rgba(34, 197, 94, 0.2);
  border: 1px solid rgba(34, 197, 94, 0.3);
  color: #86efac;
}

.error-message {
  background: rgba(239, 68, 68, 0.2);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
}

/* Tab Navigation */
.tab-nav {
  max-width: 1200px;
  margin: 0 auto 2rem;
  padding: 0.5rem;
  display: flex;
  gap: 0.5rem;
  overflow-x: auto;
}

.tab-btn {
  padding: 0.75rem 1.5rem;
  background: transparent;
  border: none;
  border-radius: 12px;
  color: rgba(255, 255, 255, 0.6);
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.tab-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.tab-btn.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

/* Admin Content */
.admin-content {
  max-width: 1200px;
  margin: 0 auto;
  position: relative;
  z-index: 1;
}

.tab-content {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: transform 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-4px);
}

.stat-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-icon.urls { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
.stat-icon.users { background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); }
.stat-icon.clicks { background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); }
.stat-icon.today { background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%); }

.stat-info h3 {
  color: white;
  font-size: 1.75rem;
  margin: 0;
  font-weight: 700;
}

.stat-info p {
  color: rgba(255, 255, 255, 0.6);
  margin: 0.25rem 0 0 0;
  font-size: 0.85rem;
}

/* Quick Health */
.quick-health {
  padding: 1.5rem;
}

.quick-health h3 {
  color: white;
  margin: 0 0 1rem 0;
  font-size: 1rem;
}

.health-indicators {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.health-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.status-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.status-healthy { background: #22c55e; }
.status-unhealthy { background: #f59e0b; }
.status-offline { background: #ef4444; }
.status-unknown { background: #6b7280; }

.service-name {
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.85rem;
  text-transform: capitalize;
}

/* Section */
.section {
  padding: 2rem;
  margin-bottom: 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.section-header h3 {
  color: white;
  margin: 0;
  font-size: 1.25rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

/* Create Form */
.create-form {
  max-width: 600px;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  color: rgba(255, 255, 255, 0.9);
  font-weight: 600;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  color: white;
  font-size: 0.95rem;
  transition: all 0.2s;
}

.form-input::placeholder {
  color: rgba(255, 255, 255, 0.4);
}

.form-input:focus {
  outline: none;
  background: rgba(255, 255, 255, 0.15);
  border-color: #667eea;
}

.form-hint {
  display: block;
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.8rem;
  margin-top: 0.25rem;
}

.alert {
  padding: 0.875rem 1rem;
  border-radius: 10px;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.alert.error {
  background: rgba(239, 68, 68, 0.2);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
}

.alert.success {
  background: rgba(34, 197, 94, 0.2);
  border: 1px solid rgba(34, 197, 94, 0.3);
  color: #86efac;
}

.submit-button {
  padding: 0.875rem 2rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: transform 0.2s;
}

.submit-button:hover:not(:disabled) {
  transform: translateY(-2px);
}

.submit-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Table */
.table-container {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th,
.data-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.data-table th {
  color: rgba(255, 255, 255, 0.6);
  font-weight: 600;
  font-size: 0.85rem;
  text-transform: uppercase;
}

.data-table td {
  color: white;
  font-size: 0.9rem;
}

.short-code {
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
}

.short-code:hover {
  text-decoration: underline;
}

.delete-btn {
  padding: 0.5rem;
  background: rgba(239, 68, 68, 0.2);
  border: 1px solid rgba(239, 68, 68, 0.3);
  border-radius: 8px;
  color: #fca5a5;
  cursor: pointer;
  transition: all 0.3s ease;
}

.delete-btn:hover {
  background: rgba(239, 68, 68, 0.3);
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.page-btn {
  padding: 0.5rem 1rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
}

.page-info {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
}

/* RabbitMQ */
.connection-status {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
}

.connection-status.connected {
  background: rgba(34, 197, 94, 0.2);
  color: #86efac;
}

.connection-status.disconnected {
  background: rgba(239, 68, 68, 0.2);
  color: #fca5a5;
}

.rabbitmq-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 2rem;
}

.mq-stat {
  text-align: center;
  padding: 1.5rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 12px;
}

.mq-label {
  display: block;
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
  margin-bottom: 0.5rem;
}

.mq-value {
  display: block;
  color: white;
  font-size: 2rem;
  font-weight: 700;
}

.queue-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.queue-item {
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.queue-name {
  color: white;
  font-weight: 600;
}

.queue-stats {
  display: flex;
  gap: 1.5rem;
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
}

/* Redis Stats */
.redis-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1rem;
}

.redis-stat {
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: transform 0.3s ease;
}

.redis-stat:hover {
  transform: translateY(-4px);
}

.redis-stat-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.redis-stat-icon.memory { background: linear-gradient(135deg, #ef4444, #f97316); }
.redis-stat-icon.keys { background: linear-gradient(135deg, #667eea, #764ba2); }
.redis-stat-icon.clients { background: linear-gradient(135deg, #10b981, #34d399); }
.redis-stat-icon.uptime { background: linear-gradient(135deg, #3b82f6, #06b6d4); }
.redis-stat-icon.hit-rate { background: linear-gradient(135deg, #f59e0b, #fbbf24); }
.redis-stat-icon.ops { background: linear-gradient(135deg, #8b5cf6, #a855f7); }

.redis-stat-info {
  display: flex;
  flex-direction: column;
}

.redis-stat-value {
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
}

.redis-stat-label {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.8rem;
}

/* Health Grid */
.health-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.health-card {
  padding: 1.5rem;
}

.health-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.health-card .service-name {
  font-weight: 600;
  text-transform: capitalize;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.status-badge.status-healthy {
  background: rgba(34, 197, 94, 0.2);
  color: #86efac;
}

.status-badge.status-unhealthy {
  background: rgba(245, 158, 11, 0.2);
  color: #fcd34d;
}

.status-badge.status-offline {
  background: rgba(239, 68, 68, 0.2);
  color: #fca5a5;
}

.status-badge.status-unknown {
  background: rgba(107, 114, 128, 0.2);
  color: #d1d5db;
}

.response-time {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 3rem;
  color: rgba(255, 255, 255, 0.5);
}

/* Transitions */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

.shake-enter-active {
  animation: shake 0.5s ease;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
  20%, 40%, 60%, 80% { transform: translateX(5px); }
}

/* Responsive */
@media (max-width: 768px) {
  .admin-container {
    padding: 1rem;
  }

  .admin-header {
    padding: 1rem;
  }

  .header-content {
    flex-direction: column;
    gap: 1rem;
  }

  .admin-info {
    width: 100%;
  }

  .header-actions {
    width: 100%;
    justify-content: flex-end;
  }

  .tab-nav {
    padding: 0.5rem;
  }

  .tab-btn {
    padding: 0.5rem 1rem;
    font-size: 0.85rem;
  }

  .section {
    padding: 1rem;
  }

  .rabbitmq-stats {
    grid-template-columns: 1fr;
  }

  .stats-grid {
    grid-template-columns: 1fr 1fr;
  }
}
</style>
