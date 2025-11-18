<script setup>
import { ref, onMounted } from 'vue';

const originalUrl = ref('');
const shortenedResult = ref(null);
const errorMessage = ref(null);
const isLoading = ref(false);
const linkHistory = ref([]);
const isDarkMode = ref(false);
const showQRCode = ref(false);

const gatewayBaseUrl = 'http://localhost:5086'; 

// Particle effect cho background
const particles = ref([]);

onMounted(() => {
  const savedTheme = localStorage.getItem('theme');
  if (savedTheme === 'dark') {
    isDarkMode.value = true;
    document.documentElement.setAttribute('data-theme', 'dark');
  }
  
  const savedHistory = localStorage.getItem('linkHistory');
  if (savedHistory) {
    linkHistory.value = JSON.parse(savedHistory);
  }

  // Generate particles
  generateParticles();
});

function generateParticles() {
  particles.value = Array.from({ length: 20 }, (_, i) => ({
    id: i,
    x: Math.random() * 100,
    y: Math.random() * 100,
    size: Math.random() * 4 + 2,
    duration: Math.random() * 20 + 10,
    delay: Math.random() * 5
  }));
}

function toggleDarkMode() {
  isDarkMode.value = !isDarkMode.value;
  if (isDarkMode.value) {
    document.documentElement.setAttribute('data-theme', 'dark');
    localStorage.setItem('theme', 'dark');
  } else {
    document.documentElement.removeAttribute('data-theme');
    localStorage.setItem('theme', 'light');
  }
}

async function shortenUrl() {
  if (!originalUrl.value) {
    errorMessage.value = 'Please enter a URL.';
    return;
  }
  isLoading.value = true;
  errorMessage.value = null;
  shortenedResult.value = null;
  showQRCode.value = false;

  try {
    const tokenResponse = await fetch(`${gatewayBaseUrl}/get-token`);
    if (!tokenResponse.ok) throw new Error('Could not retrieve auth token.');
    const token = await tokenResponse.text();

    const shortenResponse = await fetch(`${gatewayBaseUrl}/api/shorten`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ originalUrl: originalUrl.value })
    });

    const responseData = await shortenResponse.json();
    if (!shortenResponse.ok) {
      throw new Error(responseData.title || 'An error occurred while shortening the URL.');
    }

    shortenedResult.value = responseData;
    saveToHistory(originalUrl.value, responseData.shortUrl);
    showQRCode.value = true;

  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}

function saveToHistory(original, short) {
  const newLink = {
    id: Date.now(),
    original: original,
    short: short,
    createdAt: new Date().toLocaleString('vi-VN')
  };
  
  linkHistory.value.unshift(newLink);
  
  if (linkHistory.value.length > 10) {
    linkHistory.value = linkHistory.value.slice(0, 10);
  }
  
  localStorage.setItem('linkHistory', JSON.stringify(linkHistory.value));
}

function deleteFromHistory(id) {
  linkHistory.value = linkHistory.value.filter(link => link.id !== id);
  localStorage.setItem('linkHistory', JSON.stringify(linkHistory.value));
}

function clearHistory() {
  if (confirm('Bạn có chắc muốn xóa tất cả lịch sử?')) {
    linkHistory.value = [];
    localStorage.removeItem('linkHistory');
  }
}

async function copyToClipboard(text) {
  try {
    await navigator.clipboard.writeText(text);
    showCopySuccess(event.target);
  } catch (err) {
    errorMessage.value = 'Failed to copy text.';
  }
}

function showCopySuccess(button) {
  const btn = button.closest('button');
  const originalHTML = btn.innerHTML;
  btn.innerHTML = `
    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <polyline points="20 6 9 17 4 12"></polyline>
    </svg>
    Copied!
  `;
  btn.classList.add('success-pulse');
  setTimeout(() => {
    btn.innerHTML = originalHTML;
    btn.classList.remove('success-pulse');
  }, 2000);
}

function truncateUrl(url, maxLength = 40) {
  if (url.length <= maxLength) return url;
  return url.substring(0, maxLength) + '...';
}

function shareToTwitter() {
  const text = `Check out this link: ${shortenedResult.value.shortUrl}`;
  window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}`, '_blank');
}

function shareToFacebook() {
  window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shortenedResult.value.shortUrl)}`, '_blank');
}

function shareToLinkedIn() {
  window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(shortenedResult.value.shortUrl)}`, '_blank');
}

function downloadQRCode() {
  const link = document.createElement('a');
  link.href = `https://api.qrserver.com/v1/create-qr-code/?size=400x400&data=${encodeURIComponent(shortenedResult.value.shortUrl)}`;
  link.download = `qrcode-${Date.now()}.png`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}
</script>

<template>
  <div class="container">
    <!-- Animated Background Particles -->
    <div class="particles-container">
      <div 
        v-for="particle in particles" 
        :key="particle.id"
        class="particle"
        :style="{
          left: particle.x + '%',
          top: particle.y + '%',
          width: particle.size + 'px',
          height: particle.size + 'px',
          animationDuration: particle.duration + 's',
          animationDelay: particle.delay + 's'
        }"
      ></div>
    </div>

    <!-- Gradient Orbs -->
    <div class="gradient-orb orb-1"></div>
    <div class="gradient-orb orb-2"></div>
    <div class="gradient-orb orb-3"></div>

    <!-- Dark Mode Toggle with animation -->
    <button class="theme-toggle glass-effect" @click="toggleDarkMode">
      <div class="toggle-icon">
        <svg v-if="!isDarkMode" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="5"></circle>
          <line x1="12" y1="1" x2="12" y2="3"></line>
          <line x1="12" y1="21" x2="12" y2="23"></line>
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
          <line x1="1" y1="12" x2="3" y2="12"></line>
          <line x1="21" y1="12" x2="23" y2="12"></line>
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
        </svg>
      </div>
    </button>

    <div class="shortener-box glass-morphism">
      <!-- Animated gradient border -->
      <div class="animated-border"></div>
      
      <div class="header-section">
        <div class="badge-new">✨ NEW</div>
        <h1 class="gradient-text">
          <span class="wave-text">URL Shortener</span>
        </h1>
        <p class="subtitle floating-text">
          Rút gọn link dài thành link ngắn, dễ chia sẻ
          <span class="subtitle-icon">🚀</span>
        </p>
      </div>
      
      <form @submit.prevent="shortenUrl" class="shorten-form">
        <div class="input-wrapper floating-input">
          <div class="input-glow"></div>
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="icon-link pulse-icon">
            <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.72"></path>
            <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.72-1.72"></path>
          </svg>
          <input 
            type="url" 
            v-model="originalUrl" 
            placeholder="Nhập URL dài cần rút gọn..." 
            required 
          />
          <div class="input-shine"></div>
        </div>

        <button type="submit" :disabled="isLoading" class="submit-button magnetic-button">
          <span class="button-bg"></span>
          <span class="button-content">
            <span v-if="!isLoading" class="button-text">
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"></path>
              </svg>
              Rút gọn ngay
            </span>
            <div v-else class="loading-container">
              <div class="spinner-modern"></div>
              <span>Đang xử lý...</span>
            </div>
          </span>
        </button>
      </form>

      <!-- Error Message with animation -->
      <transition name="shake">
        <div v-if="errorMessage" class="message error-message glass-effect">
          <div class="message-icon error-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"></circle>
              <line x1="15" y1="9" x2="9" y2="15"></line>
              <line x1="9" y1="9" x2="15" y2="15"></line>
            </svg>
          </div>
          <p>{{ errorMessage }}</p>
        </div>
      </transition>

      <!-- Success Result with stunning animation -->
      <transition name="scale-fade">
        <div v-if="shortenedResult" class="result-container glass-morphism">
          <div class="success-confetti">
            <div class="confetti" v-for="i in 20" :key="i"></div>
          </div>
          
          <div class="success-header">
            <div class="success-icon-wrapper">
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="success-icon">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                <polyline points="22 4 12 14.01 9 11.01"></polyline>
              </svg>
            </div>
            <h3 class="success-title">Thành công rồi! 🎉</h3>
          </div>
          
          <div class="result-display">
            <div class="url-box neon-border">
              <label>Link rút gọn của bạn:</label>
              <a :href="shortenedResult.shortUrl" target="_blank" class="short-url hover-lift">
                {{ shortenedResult.shortUrl }}
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"></path>
                  <polyline points="15 3 21 3 21 9"></polyline>
                  <line x1="10" y1="14" x2="21" y2="3"></line>
                </svg>
              </a>
            </div>
            
            <div class="action-buttons">
              <button @click="copyToClipboard(shortenedResult.shortUrl)" class="action-btn copy-btn hover-glow">
                <span class="btn-shine"></span>
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                  <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                </svg>
                Sao chép
              </button>
              
              <button @click="showQRCode = !showQRCode" class="action-btn qr-btn hover-glow">
                <span class="btn-shine"></span>
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <rect x="3" y="3" width="7" height="7"></rect>
                  <rect x="14" y="3" width="7" height="7"></rect>
                  <rect x="14" y="14" width="7" height="7"></rect>
                  <rect x="3" y="14" width="7" height="7"></rect>
                </svg>
                {{ showQRCode ? 'Ẩn QR' : 'Hiện QR' }}
              </button>
            </div>
          </div>

          <!-- QR Code với hiệu ứng 3D -->
          <transition name="expand-3d">
            <div v-if="showQRCode" class="qr-section glass-effect">
              <div class="qr-code qr-3d">
                <div class="qr-wrapper">
                  <img 
                    :src="`https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=${encodeURIComponent(shortenedResult.shortUrl)}`" 
                    alt="QR Code"
                  />
                  <div class="qr-scan-line"></div>
                </div>
                <div class="qr-actions">
                  <button @click="downloadQRCode" class="download-qr-btn hover-lift">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                      <polyline points="7 10 12 15 17 10"></polyline>
                      <line x1="12" y1="15" x2="12" y2="3"></line>
                    </svg>
                    Tải về QR
                  </button>
                </div>
              </div>
            </div>
          </transition>

          <!-- Share Section với hiệu ứng ripple -->
          <div class="share-section glass-effect">
            <p class="share-label">Chia sẻ trên:</p>
            <div class="share-buttons">
              <button @click="shareToTwitter" class="share-btn twitter-btn ripple-effect">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M23 3a10.9 10.9 0 0 1-3.14 1.53 4.48 4.48 0 0 0-7.86 3v1A10.66 10.66 0 0 1 3 4s-4 9 5 13a11.64 11.64 0 0 1-7 2c9 5 20 0 20-11.5a4.5 4.5 0 0 0-.08-.83A7.72 7.72 0 0 0 23 3z"></path>
                </svg>
              </button>
              <button @click="shareToFacebook" class="share-btn facebook-btn ripple-effect">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"></path>
                </svg>
              </button>
              <button @click="shareToLinkedIn" class="share-btn linkedin-btn ripple-effect">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"></path>
                  <rect x="2" y="9" width="4" height="12"></rect>
                  <circle cx="4" cy="4" r="2"></circle>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </transition>
    </div>

    <!-- Link History với card animation -->
    <transition name="slide-up-fade">
      <div v-if="linkHistory.length > 0" class="history-box glass-morphism">
        <div class="animated-border"></div>
        
        <div class="history-header">
          <h2>
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="rotate-icon">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
            Lịch sử gần đây
            <span class="history-count">{{ linkHistory.length }}</span>
          </h2>
          <button @click="clearHistory" class="clear-history-btn hover-lift">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
            </svg>
            Xóa tất cả
          </button>
        </div>
        
        <div class="history-list">
          <transition-group name="list-stagger">
            <div v-for="(link, index) in linkHistory" :key="link.id" 
                 class="history-item glass-effect hover-scale"
                 :style="{ '--item-index': index }">
              <div class="history-number">{{ index + 1 }}</div>
              <div class="history-content">
                <div class="history-urls">
                  <span class="original-url" :title="link.original">
                    {{ truncateUrl(link.original, 35) }}
                  </span>
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="arrow-icon bounce-arrow">
                    <line x1="5" y1="12" x2="19" y2="12"></line>
                    <polyline points="12 5 19 12 12 19"></polyline>
                  </svg>
                  <a :href="link.short" target="_blank" class="short-url glow-text">{{ link.short }}</a>
                </div>
                <span class="history-date">
                  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"></circle>
                    <polyline points="12 6 12 12 16 14"></polyline>
                  </svg>
                  {{ link.createdAt }}
                </span>
              </div>
              <div class="history-actions">
                <button @click="copyToClipboard(link.short)" class="icon-btn hover-rotate" title="Sao chép">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
                  </svg>
                </button>
                <button @click="deleteFromHistory(link.id)" class="icon-btn delete-btn hover-shake" title="Xóa">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
              </div>
            </div>
          </transition-group>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
/* Base transitions - giữ nguyên */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

.slide-up-enter-active {
  animation: slideUp 0.5s ease;
}
.slide-up-leave-active {
  animation: slideUp 0.5s ease reverse;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.expand-enter-active, .expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}
.expand-enter-from, .expand-leave-to {
  max-height: 0;
  opacity: 0;
}
.expand-enter-to, .expand-leave-from {
  max-height: 500px;
  opacity: 1;
}

.list-enter-active, .list-leave-active {
  transition: all 0.3s ease;
}
.list-enter-from {
  opacity: 0;
  transform: translateX(-30px);
}
.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}
.list-move {
  transition: transform 0.3s ease;
}
</style>