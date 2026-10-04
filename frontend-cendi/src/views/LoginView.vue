<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const API_URL = 'http://localhost:5141/api/Auth/login'

const TOKEN_KEY = 'sgcti_token'

interface LoginResponse {
  token: string
  expira: string
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
  <div class="login">
    <form class="login__card" @submit.prevent="iniciarSesion">
      <h1 class="login__titulo">Sistema de Control de Suministros</h1>
      <p class="login__subtitulo">Gestión de los Servicios TI</p>

      <div class="campo">
        <label class="campo__etiqueta" for="usuario">Usuario</label>
        <input
          id="usuario"
          v-model="form.usuario"
          class="campo__input"
          type="text"
          autocomplete="username"
          placeholder="admin"
          :disabled="cargando"
        />
      </div>

      <div class="campo">
        <label class="campo__etiqueta" for="password">Contraseña</label>
        <input
          id="password"
          v-model="form.password"
          class="campo__input"
          type="password"
          autocomplete="current-password"
          placeholder="••••••••"
          :disabled="cargando"
        />
      </div>

      <p v-if="error" class="login__error">{{ error }}</p>

      <button class="login__boton" type="submit" :disabled="cargando">
        {{ cargando ? 'Ingresando...' : 'Ingresar' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.login {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 24px;
  background: #f0f2f5;
}

.login__card {
  width: 100%;
  max-width: 360px;
  padding: 32px;
  background: #ffffff;
  border: 1px solid #e2e5ea;
  border-radius: 10px;
  box-shadow: 0 4px 16px rgba(16, 24, 40, 0.06);
}

.login__titulo {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1f2b;
  text-align: center;
}

.login__subtitulo {
  margin: 6px 0 28px;
  font-size: 14px;
  color: #6b7280;
  text-align: center;
}

.campo {
  margin-bottom: 18px;
}

.campo__etiqueta {
  display: block;
  margin-bottom: 6px;
  font-size: 13px;
  font-weight: 500;
  color: #374151;
}

.campo__input {
  width: 100%;
  padding: 10px 12px;
  font-size: 14px;
  font-family: inherit;
  color: #1a1f2b;
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 6px;
  box-sizing: border-box;
}

.campo__input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

.campo__input:disabled {
  background: #f3f4f6;
  cursor: not-allowed;
}

.login__error {
  margin: 0 0 16px;
  padding: 10px 12px;
  font-size: 13px;
  color: #b42318;
  background: #fef3f2;
  border: 1px solid #fecdca;
  border-radius: 6px;
}

.login__boton {
  width: 100%;
  padding: 11px 16px;
  font-size: 15px;
  font-weight: 500;
  font-family: inherit;
  color: #ffffff;
  background: #2563eb;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.login__boton:hover:not(:disabled) {
  background: #1d4ed8;
}

.login__boton:disabled {
  background: #93b4f5;
  cursor: not-allowed;
}
</style>
