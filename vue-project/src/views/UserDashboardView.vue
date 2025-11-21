<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const userLinks = ref([]);
const isLoading = ref(false);
const errorMessage = ref(null);
const successMessage = ref(null);
const isCreating = ref(false);

// Create custom link form
const newUrl = ref('');
const customAlias = ref('');

// Edit modal
const showEditModal = ref(false);
const editingLink = ref(null);
const editUrl = ref('');

const gatewayBaseUrl = import.meta.env.PROD
  ? 'https://api-gateway-production-e75a.up.railway.app'
  : 'http://localhost:5000';

const userInfo = computed(() => ({
  email: localStorage.getItem('userEmail'),
  fullName: localStorage.getItem('userFullName')
}));

onMounted(async () => {
  await checkAuth();
  await fetchMyLinks();
});

function checkAuth() {
  const token = localStorage.getItem('authToken');
  if (!token) {
    router.push('/login');
    return;
  }
}

async function fetchMyLinks() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/urls`, {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      if (response.status === 401) {
        localStorage.clear();
        router.push('/login');
        return;
      }
      throw new Error('Failed to fetch links');
    }

    const data = await response.json();
    userLinks.value = data;
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}

async function createCustomLink() {
  if (!newUrl.value) {
    errorMessage.value = 'Please enter a URL';
    return;
  }

  isCreating.value = true;
  errorMessage.value = null;
  successMessage.value = null;

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
        customAlias: customAlias.value || null
      })
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'Failed to create short link');
    }

    successMessage.value = 'Link created successfully!';
    newUrl.value = '';
    customAlias.value = '';
    await fetchMyLinks();
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isCreating.value = false;
  }
}

function openEditModal(link) {
  editingLink.value = link;
  editUrl.value = link.originalUrl;
  showEditModal.value = true;
}

function closeEditModal() {
  showEditModal.value = false;
  editingLink.value = null;
  editUrl.value = '';
}

async function updateLink() {
  if (!editUrl.value) {
    errorMessage.value = 'URL cannot be empty';
    return;
  }

  isLoading.value = true;
  errorMessage.value = null;

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/urls/${editingLink.value.id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        originalUrl: editUrl.value
      })
    });

    if (!response.ok) {
      throw new Error('Failed to update link');
    }

    successMessage.value = 'Link updated successfully!';
    closeEditModal();
    await fetchMyLinks();
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}

async function deleteLink(id) {
  if (!confirm('Are you sure you want to delete this link?')) {
    return;
  }

  isLoading.value = true;
  errorMessage.value = null;

  try {
    const token = localStorage.getItem('authToken');
    const response = await fetch(`${gatewayBaseUrl}/api/urls/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      throw new Error('Failed to delete link');
    }

    successMessage.value = 'Link deleted successfully!';
    await fetchMyLinks();
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}

async function copyToClipboard(text) {
  try {
    await navigator.clipboard.writeText(text);
    successMessage.value = 'Link copied to clipboard!';
    setTimeout(() => {
      successMessage.value = null;
    }, 2000);
  } catch (err) {
    errorMessage.value = 'Failed to copy link';
  }
}

function logout() {
  localStorage.clear();
  router.push('/');
}

function truncateUrl(url, maxLength = 50) {
  if (url.length <= maxLength) return url;
  return url.substring(0, maxLength) + '...';
}

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}
</script>

<template>
  <div class="dashboard-container">
    <!-- Gradient Background -->
    <div class="gradient-orb orb-1"></div>
    <div class="gradient-orb orb-2"></div>
    <div class="gradient-orb orb-3"></div>

    <!-- Header -->
    <header class="dashboard-header glass-morphism">
      <div class="header-content">
        <div class="user-info">
          <div class="user-avatar">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
              <circle cx="12" cy="7" r="4"></circle>
            </svg>
          </div>
          <div class="user-details">
            <h2>{{ userInfo.fullName }}</h2>
            <p>{{ userInfo.email }}</p>
          </div>
        </div>
        <button @click="logout" class="logout-btn hover-lift">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
            <polyline points="16 17 21 12 16 7"></polyline>
            <line x1="21" y1="12" x2="9" y2="12"></line>
          </svg>
          Logout
        </button>
      </div>
    </header>

    <div class="dashboard-content">
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

      <!-- Create Custom Link Section -->
      <div class="create-section glass-morphism">
        <div class="animated-border"></div>
        <h2 class="section-title">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="16"></line>
            <line x1="8" y1="12" x2="16" y2="12"></line>
          </svg>
          Create Custom Short Link
        </h2>

        <form @submit.prevent="createCustomLink" class="create-form">
          <div class="form-row">
            <div class="form-group flex-2">
              <label>Original URL</label>
              <div class="input-wrapper">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.72"></path>
                  <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.72-1.72"></path>
                </svg>
                <input
                  type="url"
                  v-model="newUrl"
                  placeholder="https://example.com/very-long-url"
                  required
                />
              </div>
            </div>

            <div class="form-group flex-1">
              <label>Custom Alias (Optional)</label>
              <div class="input-wrapper">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M4 7h16M4 12h16M4 17h7"></path>
                </svg>
                <input
                  type="text"
                  v-model="customAlias"
                  placeholder="my-custom-link"
                  pattern="[a-zA-Z0-9-_]+"
                  title="Only letters, numbers, hyphens and underscores"
                />
              </div>
            </div>
          </div>

          <button type="submit" :disabled="isCreating" class="create-btn magnetic-button">
            <span class="button-bg"></span>
            <span class="button-content">
              <span v-if="!isCreating">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"></path>
                </svg>
                Create Link
              </span>
              <div v-else class="loading-container">
                <div class="spinner"></div>
                <span>Creating...</span>
              </div>
            </span>
          </button>
        </form>
      </div>

      <!-- My Links Section -->
      <div class="links-section glass-morphism">
        <div class="animated-border"></div>
        <div class="section-header">
          <h2 class="section-title">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.72"></path>
              <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.72-1.72"></path>
            </svg>
            My Short Links
            <span class="link-count">{{ userLinks.length }}</span>
          </h2>
          <button @click="fetchMyLinks" class="refresh-btn hover-rotate" title="Refresh">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="23 4 23 10 17 10"></polyline>
              <polyline points="1 20 1 14 7 14"></polyline>
              <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"></path>
            </svg>
          </button>
        </div>

        <div v-if="isLoading && userLinks.length === 0" class="loading-state">
          <div class="spinner-large"></div>
          <p>Loading your links...</p>
        </div>

        <div v-else-if="userLinks.length === 0" class="empty-state">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="16" x2="12" y2="12"></line>
            <line x1="12" y1="8" x2="12.01" y2="8"></line>
          </svg>
          <h3>No links yet</h3>
          <p>Create your first custom short link above!</p>
        </div>

        <div v-else class="links-grid">
          <transition-group name="list-stagger">
            <div
              v-for="(link, index) in userLinks"
              :key="link.id"
              class="link-card glass-effect hover-lift"
              :style="{ '--item-index': index }"
            >
              <div class="link-header">
                <div class="link-badge" v-if="link.customAlias">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"></polygon>
                  </svg>
                  Custom
                </div>
                <div class="link-stats">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                    <circle cx="12" cy="12" r="3"></circle>
                  </svg>
                  {{ link.accessCount }} views
                </div>
              </div>

              <div class="link-urls">
                <div class="url-row">
                  <label>Short URL:</label>
                  <a :href="link.shortUrl" target="_blank" class="short-url">
                    {{ link.shortUrl }}
                  </a>
                </div>
                <div class="url-row">
                  <label>Original:</label>
                  <span class="original-url" :title="link.originalUrl">
                    {{ truncateUrl(link.originalUrl) }}
                  </span>
                </div>
              </div>

              <div class="link-footer">
                <span class="link-date">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"></circle>
                    <polyline points="12 6 12 12 16 14"></polyline>
                  </svg>
                  {{ formatDate(link.createdAt) }}
                </span>
                <div class="link-actions">
                  <button @click="copyToClipboard(link.shortUrl)" class="action-btn copy-btn" title="Copy">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                    </svg>
                  </button>
                  <button @click="openEditModal(link)" class="action-btn edit-btn" title="Edit">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                      <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                    </svg>
                  </button>
                  <button @click="deleteLink(link.id)" class="action-btn delete-btn" title="Delete">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <polyline points="3 6 5 6 21 6"></polyline>
                      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </transition-group>
        </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <transition name="modal">
      <div v-if="showEditModal" class="modal-overlay" @click.self="closeEditModal">
        <div class="modal-content glass-morphism">
          <div class="animated-border"></div>
          <div class="modal-header">
            <h3>Edit Link</h3>
            <button @click="closeEditModal" class="close-btn">
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="18" y1="6" x2="6" y2="18"></line>
                <line x1="6" y1="6" x2="18" y2="18"></line>
              </svg>
            </button>
          </div>

          <form @submit.prevent="updateLink" class="modal-form">
            <div class="form-group">
              <label>Original URL</label>
              <input
                type="url"
                v-model="editUrl"
                placeholder="https://example.com"
                required
              />
            </div>

            <div class="modal-actions">
              <button type="button" @click="closeEditModal" class="cancel-btn">
                Cancel
              </button>
              <button type="submit" class="save-btn">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"></path>
                  <polyline points="17 21 17 13 7 13 7 21"></polyline>
                  <polyline points="7 3 7 8 15 8"></polyline>
                </svg>
                Save Changes
              </button>
            </div>
          </form>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.dashboard-container {
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
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  top: -10%;
  left: -10%;
}

.orb-2 {
  width: 350px;
  height: 350px;
  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
  top: 50%;
  right: -10%;
  animation-delay: -7s;
}

.orb-3 {
  width: 300px;
  height: 300px;
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  bottom: -10%;
  left: 30%;
  animation-delay: -14s;
}

@keyframes float {
  0%, 100% { transform: translate(0, 0) rotate(0deg); }
  33% { transform: translate(30px, -30px) rotate(120deg); }
  66% { transform: translate(-20px, 20px) rotate(240deg); }
}

.glass-morphism {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  position: relative;
}

.glass-effect {
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.15);
}

.animated-border {
  position: absolute;
  inset: -2px;
  background: linear-gradient(45deg, #667eea, #764ba2, #f093fb, #f5576c);
  background-size: 300% 300%;
  border-radius: 24px;
  z-index: -1;
  opacity: 0.5;
  animation: gradientMove 6s ease infinite;
}

@keyframes gradientMove {
  0%, 100% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
}

/* Header */
.dashboard-header {
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

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
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
  font-size: 0.9rem;
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
  transform: translateY(-2px);
}

/* Content */
.dashboard-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  position: relative;
  z-index: 1;
}

/* Messages */
.message {
  padding: 1rem 1.5rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 500;
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

/* Create Section */
.create-section {
  padding: 2rem;
  position: relative;
}

.section-title {
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.create-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.flex-1 {
  flex: 1;
}

.flex-2 {
  flex: 2;
}

.form-group label {
  color: rgba(255, 255, 255, 0.9);
  font-weight: 600;
  font-size: 0.9rem;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-wrapper svg {
  position: absolute;
  left: 1rem;
  color: rgba(255, 255, 255, 0.5);
}

.input-wrapper input {
  width: 100%;
  padding: 0.875rem 1rem 0.875rem 3rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  color: white;
  font-size: 0.95rem;
  transition: all 0.3s ease;
}

.input-wrapper input::placeholder {
  color: rgba(255, 255, 255, 0.4);
}

.input-wrapper input:focus {
  outline: none;
  background: rgba(255, 255, 255, 0.15);
  border-color: #667eea;
  box-shadow: 0 0 20px rgba(102, 126, 234, 0.3);
}

.magnetic-button {
  position: relative;
  padding: 1rem 2rem;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  overflow: hidden;
  transition: all 0.3s ease;
}

.button-bg {
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  transition: transform 0.3s ease;
}

.button-content {
  position: relative;
  color: white;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.create-btn:hover:not(:disabled) .button-bg {
  transform: scale(1.05);
}

.create-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
}

.create-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Links Section */
.links-section {
  padding: 2rem;
  position: relative;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.link-count {
  background: rgba(102, 126, 234, 0.3);
  color: #667eea;
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 600;
}

.refresh-btn {
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.refresh-btn:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: rotate(180deg);
}

/* Loading & Empty States */
.loading-state,
.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: rgba(255, 255, 255, 0.6);
}

.spinner-large {
  width: 48px;
  height: 48px;
  border: 4px solid rgba(255, 255, 255, 0.2);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.empty-state svg {
  margin-bottom: 1rem;
  color: rgba(255, 255, 255, 0.3);
}

.empty-state h3 {
  color: white;
  font-size: 1.25rem;
  margin-bottom: 0.5rem;
}

/* Links Grid */
.links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.link-card {
  padding: 1.5rem;
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.link-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.2);
}

.link-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.link-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 0.375rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
}

.link-stats {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
}

.link-urls {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.url-row {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.url-row label {
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.short-url {
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
  word-break: break-all;
}

.short-url:hover {
  text-decoration: underline;
}

.original-url {
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.9rem;
  word-break: break-all;
}

.link-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.link-date {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.8rem;
}

.link-actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  padding: 0.5rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 8px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-btn:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateY(-2px);
}

.copy-btn:hover {
  border-color: #667eea;
  color: #667eea;
}

.edit-btn:hover {
  border-color: #f093fb;
  color: #f093fb;
}

.delete-btn:hover {
  border-color: #f5576c;
  color: #f5576c;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(10px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 2rem;
}

.modal-content {
  width: 100%;
  max-width: 500px;
  padding: 2rem;
  position: relative;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.modal-header h3 {
  color: white;
  font-size: 1.5rem;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.6);
  cursor: pointer;
  padding: 0.5rem;
  transition: all 0.3s ease;
}

.close-btn:hover {
  color: white;
  transform: rotate(90deg);
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.modal-form input {
  width: 100%;
  padding: 0.875rem 1rem;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  color: white;
  font-size: 0.95rem;
}

.modal-form input:focus {
  outline: none;
  background: rgba(255, 255, 255, 0.15);
  border-color: #667eea;
}

.modal-actions {
  display: flex;
  gap: 1rem;
}

.cancel-btn,
.save-btn {
  flex: 1;
  padding: 0.875rem;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.cancel-btn {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.cancel-btn:hover {
  background: rgba(255, 255, 255, 0.15);
}

.save-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.save-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

/* Utility Classes */
.hover-lift {
  transition: transform 0.3s ease;
}

.hover-lift:hover {
  transform: translateY(-2px);
}

.hover-rotate {
  transition: transform 0.3s ease;
}

.spinner,
.loading-container {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

/* Transitions */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}

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

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-content,
.modal-leave-to .modal-content {
  transform: scale(0.9);
}

.list-stagger-enter-active {
  transition: all 0.4s ease;
  transition-delay: calc(var(--item-index) * 0.05s);
}

.list-stagger-leave-active {
  transition: all 0.3s ease;
}

.list-stagger-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.list-stagger-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

/* Responsive */
@media (max-width: 768px) {
  .dashboard-container {
    padding: 1rem;
  }

  .dashboard-header {
    padding: 1rem;
  }

  .header-content {
    flex-direction: column;
    gap: 1rem;
  }

  .user-info {
    width: 100%;
  }

  .logout-btn {
    width: 100%;
    justify-content: center;
  }

  .create-section,
  .links-section {
    padding: 1.5rem;
  }

  .form-row {
    flex-direction: column;
  }

  .links-grid {
    grid-template-columns: 1fr;
  }

  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
}
</style>
