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
      throw new Error(data.title || 'Có lỗi xảy ra khi rút gọn URL');
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
    alert('Đã sao chép link!');
  } catch (err) {
    errorMessage.value = 'Không thể sao chép link';
  }
}
</script>

<template>
  <div class="home-container">
    <div class="home-content">
      <!-- Hero Section -->
      <div class="hero">
        <h1 class="title">Rút Gọn URL Của Bạn</h1>
        <p class="subtitle">Chuyển đổi link dài thành link ngắn gọn, dễ chia sẻ</p>
      </div>

      <!-- Shortener Form -->
      <div class="shortener-card">
        <form @submit.prevent="shortenUrl" class="shortener-form">
          <div class="input-group">
            <input
              type="url"
              v-model="originalUrl"
              placeholder="Nhập URL dài của bạn... (vd: https://example.com)"
              class="url-input"
              required
            />
            <button
              type="submit"
              :disabled="isLoading"
              class="shorten-btn"
            >
              {{ isLoading ? 'Đang xử lý...' : 'Rút gọn' }}
            </button>
          </div>
        </form>

        <!-- Error Message -->
        <div v-if="errorMessage" class="message error">
          ❌ {{ errorMessage }}
        </div>

        <!-- Result -->
        <div v-if="shortenedUrl" class="result">
          <div class="result-header">
            <span class="success-icon">✅</span>
            <span>Link đã được rút gọn!</span>
          </div>
          <div class="result-url">
            <a :href="shortenedUrl" target="_blank" class="short-url">
              {{ shortenedUrl }}
            </a>
            <button @click="copyToClipboard" class="copy-btn">
              📋 Sao chép
            </button>
          </div>
        </div>
      </div>

      <!-- Features -->
      <div class="features">
        <div class="feature-card">
          <div class="feature-icon">🚀</div>
          <h3>Nhanh Chóng</h3>
          <p>Rút gọn URL chỉ trong vài giây</p>
        </div>
        <div class="feature-card">
          <div class="feature-icon">🔒</div>
          <h3>An Toàn</h3>
          <p>Bảo mật và đáng tin cậy</p>
        </div>
        <div class="feature-card">
          <div class="feature-icon">📊</div>
          <h3>Dashboard</h3>
          <p>Quản lý links của bạn</p>
        </div>
      </div>

      <!-- CTA -->
      <div class="cta">
        <p>Muốn quản lý và theo dõi links của bạn?</p>
        <router-link to="/register" class="cta-btn">
          Đăng ký miễn phí →
        </router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-container {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.home-content {
  display: flex;
  flex-direction: column;
  gap: 3rem;
}

/* Hero */
.hero {
  text-align: center;
  color: white;
}

.title {
  font-size: 3rem;
  font-weight: 800;
  margin-bottom: 1rem;
  text-shadow: 0 2px 20px rgba(0, 0, 0, 0.2);
}

.subtitle {
  font-size: 1.25rem;
  opacity: 0.95;
}

/* Shortener Card */
.shortener-card {
  background: white;
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.shortener-form {
  margin-bottom: 1.5rem;
}

.input-group {
  display: flex;
  gap: 1rem;
}

.url-input {
  flex: 1;
  padding: 1rem 1.5rem;
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.url-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.shorten-btn {
  padding: 1rem 2.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.shorten-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

.shorten-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Messages */
.message {
  padding: 1rem;
  border-radius: 12px;
  margin-bottom: 1rem;
  font-weight: 500;
}

.error {
  background: #fee;
  color: #c00;
  border: 1px solid #fcc;
}

/* Result */
.result {
  background: #f0fdf4;
  border: 2px solid #86efac;
  border-radius: 12px;
  padding: 1.5rem;
}

.result-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  font-weight: 600;
  color: #16a34a;
}

.success-icon {
  font-size: 1.5rem;
}

.result-url {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.short-url {
  flex: 1;
  padding: 0.75rem 1rem;
  background: white;
  border: 1px solid #86efac;
  border-radius: 8px;
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
  word-break: break-all;
}

.short-url:hover {
  background: #fafafa;
}

.copy-btn {
  padding: 0.75rem 1.5rem;
  background: #16a34a;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.3s ease;
}

.copy-btn:hover {
  background: #15803d;
  transform: translateY(-2px);
}

/* Features */
.features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.feature-card {
  background: rgba(255, 255, 255, 0.95);
  border-radius: 16px;
  padding: 2rem;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-5px);
}

.feature-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.feature-card h3 {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  color: #333;
}

.feature-card p {
  color: #666;
  font-size: 0.95rem;
}

/* CTA */
.cta {
  text-align: center;
  background: rgba(255, 255, 255, 0.95);
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.cta p {
  font-size: 1.125rem;
  color: #333;
  margin-bottom: 1rem;
}

.cta-btn {
  display: inline-block;
  padding: 1rem 2.5rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  text-decoration: none;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1.125rem;
  transition: all 0.3s ease;
}

.cta-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

/* Responsive */
@media (max-width: 768px) {
  .title {
    font-size: 2rem;
  }

  .subtitle {
    font-size: 1rem;
  }

  .shortener-card {
    padding: 1.5rem;
  }

  .input-group {
    flex-direction: column;
  }

  .result-url {
    flex-direction: column;
  }

  .features {
    grid-template-columns: 1fr;
  }
}
</style>
