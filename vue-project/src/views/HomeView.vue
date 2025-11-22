<script setup>
import { ref } from 'vue';

const originalUrl = ref('');
const shortenedUrl = ref('');
const errorMessage = ref('');
const isLoading = ref(false);

const gatewayBaseUrl = import.meta.env.PROD
  ? 'https://api-gateway-production-e75a.up.railway.app'
  : 'http://localhost:5000';

async function shortenUrl() {
  if (!originalUrl.value) {
    errorMessage.value = 'Vui lòng nhập URL';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  shortenedUrl.value = '';

  try {
    const response = await fetch(`${gatewayBaseUrl}/api/shorten`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ originalUrl: originalUrl.value })
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.title || 'Có lỗi xảy ra');
    }

    shortenedUrl.value = data.shortUrl;
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}

async function copyToClipboard() {
  try {
    await navigator.clipboard.writeText(shortenedUrl.value);
    alert('Đã sao chép!');
  } catch (err) {
    errorMessage.value = 'Không thể sao chép';
  }
}
</script>

<template>
  <div class="home-page">
    <div class="container">
      <!-- Simple Navigation -->
      <div class="top-nav">
        <router-link to="/login" class="nav-btn">Đăng nhập</router-link>
        <router-link to="/register" class="nav-btn">Đăng ký</router-link>
      </div>

      <!-- Hero -->
      <div class="hero">
        <h1>🔗 Rút Gọn URL</h1>
        <p>Chuyển link dài thành link ngắn gọn, dễ chia sẻ</p>
      </div>

      <!-- Main Form -->
      <div class="main-card">
        <form @submit.prevent="shortenUrl">
          <div class="input-group">
            <input
              type="url"
              v-model="originalUrl"
              placeholder="Nhập URL của bạn (vd: https://example.com)"
              required
            />
            <button type="submit" :disabled="isLoading">
              {{ isLoading ? '⏳ Đang xử lý...' : 'Rút gọn' }}
            </button>
          </div>
        </form>

        <!-- Error -->
        <div v-if="errorMessage" class="alert error">
          {{ errorMessage }}
        </div>

        <!-- Result -->
        <div v-if="shortenedUrl" class="result">
          <p class="result-label">✅ Link rút gọn:</p>
          <div class="result-box">
            <a :href="shortenedUrl" target="_blank">{{ shortenedUrl }}</a>
            <button @click="copyToClipboard" class="copy-btn">📋</button>
          </div>
        </div>
      </div>

      <!-- Features -->
      <div class="features">
        <div class="feature">
          <span class="icon">🚀</span>
          <p>Nhanh chóng</p>
        </div>
        <div class="feature">
          <span class="icon">🔒</span>
          <p>An toàn</p>
        </div>
        <div class="feature">
          <span class="icon">📊</span>
          <p>Quản lý link</p>
        </div>
      </div>

      <!-- CTA -->
      <div class="cta">
        <p>Muốn quản lý và theo dõi links?</p>
        <router-link to="/register" class="btn-primary">Đăng ký miễn phí →</router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-page {
  min-height: 100vh;
  padding: 1rem;
}

.container {
  max-width: 700px;
  margin: 0 auto;
}

/* Top Navigation */
.top-nav {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.nav-btn {
  padding: 0.5rem 1.25rem;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  text-decoration: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  transition: background 0.2s;
}

.nav-btn:hover {
  background: rgba(255, 255, 255, 0.3);
}

/* Hero */
.hero {
  text-align: center;
  color: white;
  margin-bottom: 2.5rem;
}

.hero h1 {
  font-size: 2.5rem;
  font-weight: 800;
  margin-bottom: 0.75rem;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
}

.hero p {
  font-size: 1.1rem;
  opacity: 0.95;
}

/* Main Card */
.main-card {
  background: white;
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.input-group {
  display: flex;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.input-group input {
  flex: 1;
  padding: 0.875rem 1rem;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.95rem;
  transition: border-color 0.2s;
}

.input-group input:focus {
  outline: none;
  border-color: #667eea;
}

.input-group button {
  padding: 0.875rem 2rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s;
  white-space: nowrap;
}

.input-group button:hover:not(:disabled) {
  transform: translateY(-2px);
}

.input-group button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Alert */
.alert {
  padding: 0.875rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.alert.error {
  background: #fee;
  color: #c00;
  border: 1px solid #fcc;
}

/* Result */
.result {
  margin-top: 1.5rem;
}

.result-label {
  font-weight: 600;
  color: #16a34a;
  margin-bottom: 0.5rem;
}

.result-box {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  padding: 0.875rem;
  background: #f0fdf4;
  border: 2px solid #86efac;
  border-radius: 8px;
}

.result-box a {
  flex: 1;
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
  word-break: break-all;
}

.result-box a:hover {
  text-decoration: underline;
}

.copy-btn {
  padding: 0.5rem 0.75rem;
  background: #16a34a;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1.125rem;
  transition: background 0.2s;
}

.copy-btn:hover {
  background: #15803d;
}

/* Features */
.features {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 2rem;
}

.feature {
  background: rgba(255, 255, 255, 0.95);
  padding: 1.5rem;
  border-radius: 12px;
  text-align: center;
  transition: transform 0.2s;
}

.feature:hover {
  transform: translateY(-4px);
}

.feature .icon {
  font-size: 2.5rem;
  display: block;
  margin-bottom: 0.5rem;
}

.feature p {
  color: #333;
  font-weight: 600;
  margin: 0;
}

/* CTA */
.cta {
  text-align: center;
  color: white;
  padding: 1.5rem;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  backdrop-filter: blur(10px);
}

.cta p {
  margin-bottom: 1rem;
  font-size: 1.05rem;
}

.btn-primary {
  display: inline-block;
  padding: 0.875rem 2rem;
  background: white;
  color: #667eea;
  text-decoration: none;
  border-radius: 10px;
  font-weight: 700;
  transition: transform 0.2s;
}

.btn-primary:hover {
  transform: translateY(-2px);
}

/* Responsive */
@media (max-width: 768px) {
  .hero h1 {
    font-size: 2rem;
  }

  .main-card {
    padding: 1.5rem;
  }

  .input-group {
    flex-direction: column;
  }

  .features {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .feature {
    padding: 1rem;
  }
}
</style>
