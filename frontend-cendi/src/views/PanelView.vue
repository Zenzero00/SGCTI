<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
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
const menuAbierto = ref(false)

const NOTIFICACIONES_KEY = 'notificaciones'

const notificaciones = ref<Notificacion[]>(
  JSON.parse(localStorage.getItem(NOTIFICACIONES_KEY) || '[]'),
)
const notificacionesAbiertas = ref(false)

const notificacionesNoLeidas = computed(() => notificaciones.value.length)

watch(
  notificaciones,
  (nuevas) => localStorage.setItem(NOTIFICACIONES_KEY, JSON.stringify(nuevas)),
  { deep: true },
)

function irADestinoNotificacion(notificacion: Notificacion) {
  notificacionesAbiertas.value = false

  const texto = `${notificacion.titulo} ${notificacion.mensaje}`
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')

  if (
    texto.includes('stock') ||
    texto.includes('suministro') ||
    texto.includes('consumible') ||
    texto.includes('inventario') ||
    (texto.includes('toner') && texto.includes('unidades'))
  ) {
    router.push('/panel/suministros')
    return
  }

  if (
    texto.includes('impresora') ||
    texto.includes('mantenimiento') ||
    texto.includes('agotamiento') ||
    texto.includes('snmp') ||
    texto.includes('latencia')
  ) {
    router.push('/panel/impresoras')
    return
  }

  router.push('/panel/tickets')
}

function eliminarNotificacion(indice: number) {
  notificaciones.value.splice(indice, 1)
}

let conexion: HubConnection | null = null

onMounted(() => {
  conexion = new HubConnectionBuilder()
    .withUrl(API_BASE_URL + '/hubs/notificaciones')
    .withAutomaticReconnect()
    .build()

  conexion.on('RecibirNotificacion', (titulo: string, mensaje: string) => {
    notificaciones.value.unshift({ titulo, mensaje })
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

const nombreUsuario = localStorage.getItem(NOMBRE_USUARIO_KEY) || 'Usuario'
const rolGuardado = localStorage.getItem(ROL_USUARIO_KEY) || ''

const rolUsuario = computed(() => {
  const limpio = rolGuardado.trim().toLowerCase()
  if (limpio === 'tecnico' || limpio === 'técnico') return 'Técnico'
  if (limpio === 'ingeniero') return 'Ingeniero'
  if (limpio === 'admin') return 'Administrador'
  return rolGuardado
})

const avatarUrl = computed(() => {
  const nombreParaAvatar = encodeURIComponent(nombreUsuario.trim() || 'SG')
  return `https://ui-avatars.com/api/?name=${nombreParaAvatar}&background=1e3a8a&color=fff`
})

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(NOMBRE_USUARIO_KEY)
  localStorage.removeItem(ROL_USUARIO_KEY)
  router.push('/')
}
</script>

<template>
  <div class="panel">
    <aside class="lateral" :class="{ 'menu-abierto': menuAbierto }">
      <div class="lateral__marca">
        <img
          src="@/assets/logo-completo.png"
          alt="SGCTI Logo"
          style="width: 100%; max-width: 260px; height: auto; display: block; margin: 0 auto; padding: 0 20px;"
        />
      </div>

      <p class="lateral__seccion">Menú</p>

      <nav class="lateral__nav" @click="menuAbierto = false">
        <RouterLink class="lateral__enlace" to="/panel" exact-active-class="lateral__enlace--activo">
          <i class="ph ph-squares-four"></i>
          Dashboard
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/tickets" active-class="lateral__enlace--activo">
          <i class="ph ph-ticket"></i>
          Mesa de Ayuda
        </RouterLink>
        <button
          class="lateral__enlace lateral__enlace--boton"
          type="button"
          @click.stop="menuImpresorasAbierto = !menuImpresorasAbierto"
        >
          <span class="lateral__grupo">
            <i class="ph ph-printer"></i>
            Impresoras
          </span>
          <span class="lateral__flecha">{{ menuImpresorasAbierto ? '▴' : '▾' }}</span>
        </button>
        <div v-show="menuImpresorasAbierto" class="submenu">
          <RouterLink
            class="lateral__enlace lateral__enlace--sub"
            to="/panel/impresoras"
            active-class="lateral__enlace--activo"
          >
            <i class="ph ph-network"></i>
            Red
          </RouterLink>
          <RouterLink
            class="lateral__enlace lateral__enlace--sub"
            to="/panel/impresoras-consumo"
            active-class="lateral__enlace--activo"
          >
            <i class="ph ph-chart-bar"></i>
            Consumo
          </RouterLink>
        </div>
        <RouterLink class="lateral__enlace" to="/panel/suministros" active-class="lateral__enlace--activo">
          <i class="ph ph-package"></i>
          Inventario/Suministros
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/bitacora" active-class="lateral__enlace--activo">
          <i class="ph ph-clipboard-text"></i>
          Bitácora
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/asistente" active-class="lateral__enlace--activo">
          <i class="ph ph-robot"></i>
          Asistente IA
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/usuarios" active-class="lateral__enlace--activo">
          <i class="ph ph-gear"></i>
          Usuarios / Configuración
        </RouterLink>
      </nav>

      <button class="lateral__logout" type="button" @click="cerrarSesion">
        <i class="ph ph-sign-out"></i>
        Cerrar Sesión
      </button>
    </aside>

    <div v-if="menuAbierto" class="lateral__fondo" @click="menuAbierto = false"></div>

    <div class="panel__principal">
      <header class="topbar">
        <div class="topbar__izquierda">
          <button
            class="topbar__menu"
            type="button"
            aria-label="Abrir menú"
            @click="menuAbierto = !menuAbierto"
          >
            <i class="ph ph-list" style="font-size: 24px"></i>
          </button>
        </div>

        <div class="topbar__derecha">
          <div class="barra__notificaciones">
            <button
              class="campana"
              :class="{ 'campana--con-alerta': notificacionesNoLeidas > 0 }"
              type="button"
              :aria-label="`Notificaciones (${notificacionesNoLeidas})`"
              @click="notificacionesAbiertas = !notificacionesAbiertas"
            >
              <i class="ph ph-bell" style="font-size: 26px; color: #4b5563;"></i>
              <span
                v-if="notificacionesNoLeidas > 0"
                style="position: absolute; top: -2px; right: -2px; background-color: #ef4444; color: white; font-size: 0.7rem; font-weight: bold; width: 18px; height: 18px; display: flex; align-items: center; justify-content: center; border-radius: 50%; border: 2px solid white;"
              >
                {{ notificacionesNoLeidas }}
              </span>
            </button>

            <div
              v-if="notificacionesAbiertas"
              class="campana__fondo"
              @click="notificacionesAbiertas = false"
            ></div>

            <div v-if="notificacionesAbiertas" class="campana__dropdown">
              <div class="campana__cabecera">
                <span class="campana__titulo">Notificaciones</span>
                <button
                  class="campana__leidas"
                  type="button"
                  :disabled="notificacionesNoLeidas === 0"
                  @click="marcarTodasLeidas"
                >
                  Marcar todas como leídas
                </button>
              </div>

              <ul v-if="notificacionesNoLeidas > 0" class="campana__lista">
                <li
                  v-for="(notificacion, indice) in notificaciones"
                  :key="indice"
                  class="campana__item"
                >
                  <div
                    class="campana__item-cuerpo"
                    @click="irADestinoNotificacion(notificacion)"
                  >
                    <strong class="campana__item-titulo">{{ notificacion.titulo }}</strong>
                    <span class="campana__item-mensaje">{{ notificacion.mensaje }}</span>
                  </div>
                  <button
                    class="campana__item-eliminar"
                    type="button"
                    aria-label="Eliminar notificación"
                    @click.stop="eliminarNotificacion(indice)"
                  >
                    <i class="ph ph-x"></i>
                  </button>
                </li>
              </ul>

              <p v-else class="campana__vacio">Sin notificaciones.</p>
            </div>
          </div>

          <div class="topbar__perfil">
            <img
              :src="avatarUrl"
              alt="Avatar"
              style="width: 38px; height: 38px; border-radius: 50%; border: 2px solid #e5e7eb; object-fit: cover;"
            />
            <div class="topbar__textos">
              <p class="topbar__nombre">{{ nombreUsuario }}</p>
              <p class="topbar__rol">{{ rolUsuario }}</p>
            </div>
          </div>
        </div>
      </header>

      <main class="panel__contenido">
        <div class="panel__vista">
          <router-view v-slot="{ Component }">
            <component :is="Component" v-if="Component" :key="$route.fullPath" />
          </router-view>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.panel { display: flex; height: 100vh; overflow: hidden; background: var(--color-fondo); }
.lateral { display: flex; flex-direction: column; width: 300px; height: 100%; flex-shrink: 0; padding: 18px 14px; overflow-y: auto; background: var(--color-lateral); }
.lateral__marca { display: flex; align-items: center; padding: 10px 0; border-bottom: 1px solid rgba(255, 255, 255, 0.08); }
.lateral__seccion { margin: 18px 8px 8px; font-size: 10.5px; font-weight: 600; letter-spacing: 1px; text-transform: uppercase; color: rgba(255, 255, 255, 0.32); }
.lateral__nav { display: flex; flex-direction: column; gap: 2px; }
.lateral__enlace { display: flex; align-items: center; gap: 10px; padding: 10px 12px; font-size: 13.5px; font-weight: 500; color: var(--color-lateral-texto); text-decoration: none; border-radius: 8px; transition: background 0.15s ease, color 0.15s ease; }
.lateral__enlace i { font-size: 17px; flex-shrink: 0; }
.lateral__grupo { display: flex; align-items: center; gap: 10px; }
.lateral__enlace:hover { color: #ffffff; background: var(--color-lateral-hover); }
.lateral__enlace--activo { color: #ffffff; background: var(--color-lateral-activo); box-shadow: inset 3px 0 0 var(--color-acento); }
.submenu { display: flex; flex-direction: column; gap: 2px; padding-left: 14px; }
.submenu .lateral__enlace--sub { padding: 8px 12px; font-size: 13px; }
.lateral__enlace--boton { display: flex; align-items: center; justify-content: space-between; width: 100%; text-align: left; background: transparent; border: none; }
.lateral__flecha { font-size: 11px; color: var(--color-lateral-texto); }
.lateral__logout { display: flex; align-items: center; justify-content: center; gap: 8px; margin-top: auto; padding: 10px 12px; font-size: 13px; font-weight: 500; color: #fda29b; background: transparent; border: 1px solid rgba(253, 162, 155, 0.35); border-radius: 8px; transition: background 0.15s ease, color 0.15s ease; }
.lateral__logout:hover { color: #ffffff; background: #7a271a; border-color: #7a271a; }
.lateral__fondo { display: none; }
.panel__principal { display: flex; flex-direction: column; flex: 1 1 auto; min-width: 0; height: 100%; background: var(--color-fondo); }
.topbar { display: flex; align-items: center; justify-content: space-between; gap: 16px; height: 64px; flex-shrink: 0; padding: 0 24px; background: var(--color-superficie); box-shadow: var(--sombra-suave); }
.topbar__izquierda { display: flex; align-items: center; gap: 12px; flex: 1 1 auto; min-width: 0; }
.topbar__menu { display: none; align-items: center; justify-content: center; width: 40px; height: 40px; flex-shrink: 0; color: var(--color-texto); background: transparent; border: 1px solid transparent; border-radius: 8px; transition: background 0.15s ease; }
.topbar__menu:hover { background: #f2f4f7; }
.topbar__derecha { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }
.barra__notificaciones { position: relative; }
.topbar__perfil { display: flex; align-items: center; gap: 10px; padding: 6px 10px; border-radius: 8px; }
.topbar__perfil:hover { background: #f8fafc; }
.topbar__textos { min-width: 0; }
.topbar__nombre { margin: 0; font-size: 13px; font-weight: 600; color: var(--color-texto); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.topbar__rol { margin: 0; font-size: 11.5px; color: var(--color-texto-suave); }
.campana { position: relative; display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; font-size: 17px; background: transparent; border: 1px solid transparent; border-radius: 8px; cursor: pointer; transition: background 0.15s ease; }
.campana:hover { background: #f2f4f7; }
.campana__fondo { position: fixed; inset: 0; z-index: 40; }
.campana__dropdown { position: absolute; top: calc(100% + 10px); right: 0; z-index: 50; width: 320px; max-width: calc(100vw - 40px); background: #ffffff; border: 1px solid #d0d5dd; border-radius: 10px; box-shadow: var(--sombra-media); overflow: hidden; }
.campana__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px 14px; border-bottom: 1px solid #eaecf0; background: #f9fafb; }
.campana__titulo { font-size: 13px; font-weight: 600; color: #101828; }
.campana__leidas { padding: 0; font-size: 12px; font-weight: 500; color: var(--color-acento); background: transparent; border: none; }
.campana__leidas:disabled { color: #98a2b3; cursor: default; }
.campana__lista { max-height: 260px; margin: 0; padding: 6px 0; list-style: none; overflow-y: auto; }
.campana__item { display: flex; align-items: flex-start; gap: 8px; padding: 10px 14px; border-bottom: 1px solid #f2f4f7; }
.campana__item:last-child { border-bottom: none; }
.campana__item-cuerpo { display: flex; flex-direction: column; gap: 3px; flex: 1 1 auto; min-width: 0; cursor: pointer; }
.campana__item-eliminar { display: flex; align-items: center; justify-content: center; width: 22px; height: 22px; flex-shrink: 0; margin-top: 2px; font-size: 14px; color: #98a2b3; background: transparent; border: none; border-radius: 50%; cursor: pointer; transition: background 0.15s ease, color 0.15s ease; }
.campana__item-eliminar:hover { color: #b42318; background: #fef3f2; }
.campana__item-titulo { font-size: 13px; color: #101828; }
.campana__item-mensaje { font-size: 12px; color: #475467; }
.campana__vacio { margin: 0; padding: 22px 14px; font-size: 13px; color: #98a2b3; text-align: center; }
.panel__contenido { flex: 1 1 auto; min-height: 0; padding: 28px; overflow-y: auto; overflow-x: hidden; }
.panel__vista { display: block; width: 100%; }
@media (max-width: 900px) {
  .lateral { width: 248px; }
  .topbar { padding: 0 16px; }
  .panel__contenido { padding: 20px; }
}
@media (max-width: 640px) {
  .lateral { width: 208px; padding: 16px 10px; }
  .lateral__marca { justify-content: center; padding-left: 0; padding-right: 0; }
  .topbar__textos { display: none; }
  .panel__contenido { padding: 16px; }
}
@media (max-width: 768px) {
  .topbar__menu { display: flex; cursor: pointer; }
  .lateral { position: fixed; top: 0; left: 0; height: 100vh; z-index: 50; transform: translateX(-100%); transition: transform 0.3s ease; box-shadow: 8px 0 24px rgba(16, 24, 40, 0.3); }
  .lateral.menu-abierto { transform: translateX(0); }
  .lateral__fondo { display: block; position: fixed; inset: 0; z-index: 40; background: rgba(16, 24, 40, 0.5); }
  .panel__principal { width: 100%; margin-left: 0; }
}
</style>