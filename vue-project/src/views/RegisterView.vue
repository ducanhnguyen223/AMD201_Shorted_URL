<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const email = ref('');
const password = ref('');
const fullName = ref('');
const errorMessage = ref(null);
const isLoading = ref(false);

const gatewayBaseUrl = import.meta.env.PROD
  ? 'https://api-gateway-production-e75a.up.railway.app'
  : 'http://localhost:5000';

async function handleRegister() {
  if (!email.value || !password.value || !fullName.value) {
    errorMessage.value = 'Vui lòng điền đầy đủ thông tin';
    return;
  }

  if (password.value.length < 6) {
    errorMessage.value = 'Mật khẩu phải có ít nhất 6 ký tự';
    return;
  }

  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await fetch(`${gatewayBaseUrl}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: email.value,
        password: password.value,
        fullName: fullName.value
      })
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'Đăng ký thất bại');
    }

    // Save token and user info
    localStorage.setItem('authToken', data.token);
    localStorage.setItem('userEmail', data.email);
    localStorage.setItem('userFullName', data.fullName);

    // Save user role (default is User for new registrations)
    localStorage.setItem('userRole', 'User');

    // Redirect to dashboard
    router.push('/dashboard');
  } catch (error) {
    errorMessage.value = error.message;
  } finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <div class="register-page">
    <div class="container">
      <!-- Top Navigation -->
      <div class="top-nav">
        <router-link to="/" class="nav-btn">← Trang chủ</router-link>
      </div>

      <div class="register-card">
        <div class="header">
          <h1>Đăng Ký</h1>
          <p>Tạo tài khoản miễn phí</p>
        </div>

        <form @submit.prevent="handleRegister">
          <div class="form-group">
            <label>Họ và tên</label>
            <input
              type="text"
              v-model="fullName"
              placeholder="Nguyễn Văn A"
              required
            />
          </div>

          <div class="form-group">
            <label>Email</label>
            <input
              type="email"
              v-model="email"
              placeholder="your@email.com"
              required
            />
          </div>

          <div class="form-group">
            <label>Mật khẩu</label>
            <input
              type="password"
              v-model="password"
              placeholder="Ít nhất 6 ký tự"
              required
            />
          </div>

          <div v-if="errorMessage" class="alert error">
            {{ errorMessage }}
          </div>

          <button type="submit" :disabled="isLoading" class="submit-btn">
            {{ isLoading ? '⏳ Đang xử lý...' : 'Đăng ký' }}
          </button>
        </form>

        <div class="footer">
          <p>Đã có tài khoản? <router-link to="/login">Đăng nhập ngay</router-link></p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.register-page {
  min-height: 100vh;
  padding: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.container {
  max-width: 450px;
  width: 100%;
  margin: 0 auto;
}

/* Top Navigation */
.top-nav {
  margin-bottom: 1rem;
}

.nav-btn {
  display: inline-block;
  padding: 0.5rem 1rem;
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

.register-card {
  background: white;
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.header {
  text-align: center;
  margin-bottom: 2rem;
}

.header h1 {
  font-size: 2rem;
  font-weight: 700;
  color: #333;
  margin-bottom: 0.5rem;
}

.header p {
  color: #666;
  font-size: 0.95rem;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 600;
  font-size: 0.9rem;
}

.form-group input {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.95rem;
  transition: border-color 0.2s;
}

.form-group input:focus {
  outline: none;
  border-color: #667eea;
}

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

.submit-btn {
  width: 100%;
  padding: 0.875rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: transform 0.2s;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.footer {
  margin-top: 1.5rem;
  text-align: center;
  color: #666;
  font-size: 0.9rem;
}

.footer a {
  color: #667eea;
  text-decoration: none;
  font-weight: 600;
}

.footer a:hover {
  text-decoration: underline;
}

@media (max-width: 768px) {
  .register-card {
    padding: 1.5rem;
  }

  .header h1 {
    font-size: 1.75rem;
  }
}
</style>
