<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { API_BASE_URL } from '../config'

interface Notificacion {
  titulo: string
  mensaje: string
}

const TOKEN_KEY = 'sgcti_token'
const NOMBRE_USUARIO_KEY = 'nombre_usuario'
const ROL_USUARIO_KEY = 'rol_usuario'

const router = useRouter()

const menuImpresorasAbierto = ref(false)

const notificaciones = ref<Notificacion[]>([])
const notificacionesAbiertas = ref(false)

const notificacionesNoLeidas = computed(() => notificaciones.value.length)

let conexion: HubConnection | null = null

onMounted(() => {
  conexion = new HubConnectionBuilder()
    .withUrl(API_BASE_URL + '/hubs/notificaciones')
    .withAutomaticReconnect()
    .build()

  conexion.on('RecibirNotificacion', (titulo: string, mensaje: string) => {
    notificaciones.value.push({ titulo, mensaje })
  })

  void conexion
    .start()
    .then(() => {
      console.log('Conectado a SignalR')
    })
    .catch((error) => {
      console.error('No se pudo conectar al hub de notificaciones.', error)
    })
})

onBeforeUnmount(() => {
  void conexion?.stop()
})

function marcarTodasLeidas() {
  notificaciones.value = []
}

const nombreUsuario = localStorage.getItem(NOMBRE_USUARIO_KEY) || ''
const rolUsuario = localStorage.getItem(ROL_USUARIO_KEY) || ''

const inicialUsuario = nombreUsuario.trim().charAt(0).toUpperCase() || 'U'

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(NOMBRE_USUARIO_KEY)
  localStorage.removeItem(ROL_USUARIO_KEY)
  router.push('/')
}
</script>

<template>
  <div class="panel">
    <aside class="lateral">
      <div class="lateral__marca">
        <span class="lateral__logo">SGCTI</span>
        <div class="lateral__textos">
          <p class="lateral__nombre">Control de Suministros</p>
          <p class="lateral__subtitulo">Servicios TI</p>
        </div>
      </div>

      <div class="lateral__usuario">
        <span class="lateral__avatar">{{ inicialUsuario }}</span>
        <div class="lateral__textos">
          <p class="lateral__nombre lateral__nombre--usuario">{{ nombreUsuario }}</p>
          <p class="lateral__subtitulo">{{ rolUsuario }}</p>
        </div>
      </div>

      <nav class="lateral__nav">
        <RouterLink class="lateral__enlace" to="/panel" exact-active-class="lateral__enlace--activo">
          Dashboard
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/tickets" active-class="lateral__enlace--activo">
          Mesa de Ayuda
        </RouterLink>
        <button
          class="lateral__enlace lateral__enlace--boton"
          type="button"
          @click="menuImpresorasAbierto = !menuImpresorasAbierto"
        >
          <span>Impresoras</span>
          <span class="lateral__flecha">{{ menuImpresorasAbierto ? '▴' : '▾' }}</span>
        </button>
        <div v-show="menuImpresorasAbierto" class="submenu">
          <RouterLink
            class="lateral__enlace lateral__enlace--sub"
            to="/panel/impresoras"
            active-class="lateral__enlace--activo"
          >
            Red o Conexión
          </RouterLink>
          <RouterLink
            class="lateral__enlace lateral__enlace--sub"
            to="/panel/impresoras-consumo"
            active-class="lateral__enlace--activo"
          >
            Consumo
          </RouterLink>
        </div>
        <RouterLink class="lateral__enlace" to="/panel/suministros" active-class="lateral__enlace--activo">
          Inventario/Suministros
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/bitacora" active-class="lateral__enlace--activo">
          Bitácora
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/usuarios" active-class="lateral__enlace--activo">
          Usuarios / Configuración
        </RouterLink>
      </nav>

      <button class="lateral__logout" type="button" @click="cerrarSesion">
        Cerrar Sesión
      </button>
    </aside>

    <main class="panel__contenido">
      <header class="panel__barra">
        <div class="barra__espacio"></div>

        <div class="barra__notificaciones">
          <button
            class="campana"
            :class="{ 'campana--con-alerta': notificacionesNoLeidas > 0 }"
            type="button"
            :aria-label="`Notificaciones (${notificacionesNoLeidas})`"
            @click="notificacionesAbiertas = !notificacionesAbiertas"
          >
            🔔
            <span v-if="notificacionesNoLeidas > 0" class="campana__contador">{{ notificacionesNoLeidas }}</span>
          </button>

          <div v-if="notificacionesAbiertas" class="campana__fondo" @click="notificacionesAbiertas = false"></div>

          <div v-if="notificacionesAbiertas" class="campana__dropdown">
            <div class="campana__cabecera">
              <span class="campana__titulo">Notificaciones</span>
              <button class="campana__leidas" type="button" :disabled="notificacionesNoLeidas === 0" @click="marcarTodasLeidas">
                Marcar todas como leídas
              </button>
            </div>

            <ul v-if="notificacionesNoLeidas > 0" class="campana__lista">
              <li v-for="(notificacion, indice) in notificaciones" :key="indice" class="campana__item">
                <strong class="campana__item-titulo">{{ notificacion.titulo }}</strong>
                <span class="campana__item-mensaje">{{ notificacion.mensaje }}</span>
              </li>
            </ul>

            <p v-else class="campana__vacio">Sin notificaciones.</p>
          </div>
        </div>
      </header>

      <div class="panel__vista">
        <router-view v-slot="{ Component }">
          <component :is="Component" v-if="Component" :key="$route.fullPath" />
        </router-view>
      </div>
    </main>
  </div>
</template>

<style scoped>
.panel {
  display: flex;
  height: 100vh;
  overflow: hidden;
  background: #f0f2f5;
}

.lateral {
  display: flex;
  flex-direction: column;
  width: 244px;
  height: 100%;
  flex-shrink: 0;
  padding: 20px 14px;
  overflow-y: auto;
  background: #101828;
}

.lateral__marca {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 6px 22px;
  border-bottom: 1px solid #1d2939;
}

.lateral__logo {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 0.5px;
  color: #101828;
  background: #ffffff;
  border-radius: 8px;
}

.lateral__textos {
  min-width: 0;
}

.lateral__nombre {
  margin: 0;
  font-size: 13px;
  font-weight: 600;
  color: #ffffff;
}

.lateral__subtitulo {
  margin: 2px 0 0;
  font-size: 11px;
  color: #98a2b3;
}

.lateral__usuario {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 14px;
  padding: 10px 12px;
  background: #1d2939;
  border-radius: 8px;
}

.lateral__avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  font-size: 14px;
  font-weight: 700;
  text-transform: uppercase;
  color: #ffffff;
  background: #1f3a68;
  border-radius: 50%;
}

.lateral__nombre--usuario {
  font-size: 12px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.lateral__nav {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 16px;
}

.lateral__enlace {
  padding: 10px 12px;
  font-size: 14px;
  font-weight: 500;
  color: #98a2b3;
  text-decoration: none;
  border-radius: 6px;
}

.lateral__enlace:hover {
  color: #ffffff;
  background: #1d2939;
}

.lateral__enlace--activo {
  color: #ffffff;
  background: #1f3a68;
}

.submenu {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-left: 18px;
}

.submenu .lateral__enlace--sub {
  padding: 8px 12px;
  font-size: 13px;
}

.lateral__enlace--boton {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  text-align: left;
  background: transparent;
  border: none;
  cursor: pointer;
}

.lateral__flecha {
  font-size: 11px;
  color: #98a2b3;
}

.lateral__logout {
  margin-top: auto;
  padding: 10px 12px;
  font-size: 13px;
  font-weight: 500;
  font-family: inherit;
  color: #fda29b;
  background: transparent;
  border: 1px solid #7a271a;
  border-radius: 6px;
  cursor: pointer;
}

.lateral__logout:hover {
  color: #ffffff;
  background: #7a271a;
}

.panel__contenido {
  flex: 1 1 auto;
  min-width: 0;
  height: 100%;
  padding: 28px;
  overflow-y: auto;
  overflow-x: hidden;
  box-sizing: border-box;
}

.panel__barra {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.barra__notificaciones {
  position: relative;
}

.campana {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  font-size: 18px;
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 50%;
  cursor: pointer;
  transition: background 0.15s ease;
}

.campana:hover {
  background: #f2f4f7;
}

.campana--con-alerta {
  border-color: #f79009;
}

.campana__contador {
  position: absolute;
  top: -4px;
  right: -4px;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 18px;
  height: 18px;
  padding: 0 4px;
  font-size: 11px;
  font-weight: 700;
  color: #ffffff;
  background: #b42318;
  border: 2px solid #ffffff;
  border-radius: 999px;
}

.campana__fondo {
  position: fixed;
  inset: 0;
  z-index: 40;
}

.campana__dropdown {
  position: absolute;
  top: calc(100% + 10px);
  right: 0;
  z-index: 50;
  width: 320px;
  max-width: calc(100vw - 40px);
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 10px;
  box-shadow: 0 12px 32px rgba(16, 24, 40, 0.16);
  overflow: hidden;
}

.campana__cabecera {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
  border-bottom: 1px solid #eaecf0;
  background: #f9fafb;
}

.campana__titulo {
  font-size: 13px;
  font-weight: 600;
  color: #101828;
}

.campana__leidas {
  padding: 0;
  font-size: 12px;
  font-weight: 500;
  font-family: inherit;
  color: #1f3a68;
  background: transparent;
  border: none;
  cursor: pointer;
}

.campana__leidas:disabled {
  color: #98a2b3;
  cursor: default;
}

.campana__lista {
  max-height: 260px;
  margin: 0;
  padding: 6px 0;
  list-style: none;
  overflow-y: auto;
}

.campana__item {
  display: flex;
  flex-direction: column;
  gap: 3px;
  padding: 10px 14px;
  border-bottom: 1px solid #f2f4f7;
}

.campana__item:last-child {
  border-bottom: none;
}

.campana__item-titulo {
  font-size: 13px;
  color: #101828;
}

.campana__item-mensaje {
  font-size: 12px;
  color: #475467;
}

.campana__vacio {
  margin: 0;
  padding: 22px 14px;
  font-size: 13px;
  color: #98a2b3;
  text-align: center;
}

.panel__vista {
  display: block;
  width: 100%;
  height: 100%;
  overflow: visible;
}
</style>
