<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const API_URL = 'http://localhost:5141/api/Auth/login'

const TOKEN_KEY = 'sgcti_token'
const NOMBRE_USUARIO_KEY = 'nombre_usuario'
const ROL_USUARIO_KEY = 'rol_usuario'

interface LoginResponse {
  token: string
  expira: string
  nombreCompleto: string
  rol: string
}

const router = useRouter()

const form = reactive({
  usuario: '',
  password: '',
})

const error = ref('')
const cargando = ref(false)

async function iniciarSesion() {
  error.value = ''

  if (!form.usuario || !form.password) {
    error.value = 'Usuario y contraseña son obligatorios.'
    return
  }

  cargando.value = true

  try {
    const { data } = await axios.post<LoginResponse>(API_URL, {
      usuario: form.usuario,
      password: form.password,
    })

    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(NOMBRE_USUARIO_KEY, data.nombreCompleto)
    localStorage.setItem(ROL_USUARIO_KEY, data.rol)
    await router.push('/panel')
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}

function mensajeDeError(e: unknown): string {
  if (axios.isAxiosError(e)) {
    if (!e.response) {
      return 'No se pudo conectar con el servidor.'
    }

    if (e.response.status === 401) {
      return 'Usuario o contraseña incorrectos.'
    }

    const cuerpo = e.response.data
    if (typeof cuerpo === 'string' && cuerpo.length > 0) {
      return cuerpo
    }

    return 'Ocurrió un error al iniciar sesión.'
  }

  return 'Ocurrió un error inesperado.'
}
</script>

<template>
  <div class="login-container">
    <!-- Panel Izquierdo: Emblema -->
    <div class="brand-panel">
      <div class="logo-wrapper">
        <img src="@/assets/images/login-bg.png" alt="Emblema SGCTI" class="brand-logo" />
      </div>
      <div class="brand-footer">
        <p>Plataforma corporativa de gestión</p>
      </div>
    </div>

    <!-- Panel Derecho: Formulario -->
    <div class="form-panel">
      <div class="login-card">
        <div class="card-header">
          <div class="card-brand">
            <img src="/logo-icono.png" alt="Icono" class="card-icon" />
            <span class="card-brand__nombre">SGCTI</span>
          </div>
          <h2>Iniciar sesión</h2>
          <p>Bienvenido. Ingresa tus credenciales para continuar.</p>
        </div>

        <form @submit.prevent="iniciarSesion" class="login-form">
          <div class="input-group">
            <label for="usuario">Usuario</label>
            <div class="input-wrapper">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="20"
                height="20"
                fill="#64748b"
                viewBox="0 0 256 256"
                class="input-icon"
              >
                <path
                  d="M230.92,212c-15.23-26.33-38.7-45.21-66.09-54.16a72,72,0,1,0-73.66,0C63.78,166.78,40.31,185.66,25.08,212a8,8,0,1,0,13.85,8c18.84-32.56,52.14-52,89.07-52s70.23,19.44,89.07,52a8,8,0,1,0,13.85-8ZM72,96a56,56,0,1,1,56,56A56.06,56.06,0,0,1,72,96Z"
                ></path>
              </svg>
              <input
                type="text"
                id="usuario"
                v-model="form.usuario"
                autocomplete="username"
                :disabled="cargando"
              />
            </div>
          </div>

          <div class="input-group">
            <label for="contrasena">Contraseña</label>
            <div class="input-wrapper">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="20"
                height="20"
                fill="#64748b"
                viewBox="0 0 256 256"
                class="input-icon"
              >
                <path
                  d="M208,80H176V56a48,48,0,0,0-96,0V80H48A16,16,0,0,0,32,96V208a16,16,0,0,0,16,16H208a16,16,0,0,0,16-16V96A16,16,0,0,0,208,80ZM96,56a32,32,0,0,1,64,0V80H96ZM208,208H48V96H208V208Zm-68-56a12,12,0,1,1-12-12A12,12,0,0,1,140,152Z"
                ></path>
              </svg>
              <input
                type="password"
                id="contrasena"
                v-model="form.password"
                autocomplete="current-password"
                :disabled="cargando"
              />
            </div>
          </div>

          <p v-if="error" class="login-error">{{ error }}</p>

          <button type="submit" class="btn-submit" :disabled="cargando">
            {{ cargando ? 'Ingresando...' : 'Ingresar' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  min-height: 100vh;
  background-color: #f8fafc;
}

.brand-panel {
  flex: 0 0 40%;
  max-width: 40%;
  background-color: #04193c; /* Azul muy oscuro para fundirse con la imagen */
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
}

.logo-wrapper {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.brand-logo {
  width: 100%;
  max-height: 65vh;
  object-fit: contain;
}

.brand-footer p {
  position: absolute;
  bottom: 30px;
  left: 40px;
  color: rgba(255, 255, 255, 0.6);
  font-size: 14px;
  margin: 0;
}

.form-panel {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.login-card {
  background: white;
  width: 100%;
  max-width: 440px;
  padding: 3rem;
  border-radius: 12px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
}

.card-header {
  text-align: center;
  margin-bottom: 2rem;
}

.card-brand {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.card-icon {
  width: 48px;
}

.card-brand__nombre {
  color: #1d4ed8;
  font-size: 24px;
  font-weight: bold;
}

.card-header h2 {
  color: #1e293b;
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0 0 0.5rem 0;
}

.card-header p {
  color: #64748b;
  font-size: 0.875rem;
  margin: 0;
}

.input-group {
  margin-bottom: 1.5rem;
}

.input-group label {
  display: block;
  color: #334155;
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1rem;
  pointer-events: none;
}

.input-wrapper input {
  width: 100%;
  padding: 12px 12px 12px 44px;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 1rem;
  color: #0f172a;
  background-color: white;
  transition: 0.3s;
}

.input-wrapper input:focus {
  outline: none;
  border-color: #1d4ed8;
  box-shadow: 0 0 0 3px rgba(29, 78, 216, 0.2);
}

.input-wrapper input:disabled {
  background-color: #f1f5f9;
  cursor: not-allowed;
}

.login-error {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  font-size: 0.875rem;
  color: #b42318;
  background: #fef3f2;
  border: 1px solid #fecdca;
  border-radius: 8px;
}

.btn-submit {
  width: 100%;
  padding: 12px;
  background-color: #1d4ed8;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: bold;
  cursor: pointer;
  transition:
    background-color 0.2s,
    transform 0.1s;
  margin-top: 1rem;
}

.btn-submit:hover:not(:disabled) {
  background-color: #1e40af;
}

.btn-submit:active:not(:disabled) {
  transform: translateY(1px);
}

.btn-submit:disabled {
  background-color: #93c5fd;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .brand-panel {
    display: none;
  }

  .login-card {
    padding: 2rem;
  }
}
</style>
