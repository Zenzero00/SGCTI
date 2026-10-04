<script setup lang="ts">
import { useRouter } from 'vue-router'

const TOKEN_KEY = 'sgcti_token'

const router = useRouter()

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
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

      <nav class="lateral__nav">
        <RouterLink class="lateral__enlace" to="/panel" exact-active-class="lateral__enlace--activo">
          Mesa de Ayuda
        </RouterLink>
        <RouterLink class="lateral__enlace" to="/panel/impresoras" active-class="lateral__enlace--activo">
          Impresoras
        </RouterLink>
      </nav>

      <button class="lateral__logout" type="button" @click="cerrarSesion">
        Cerrar Sesión
      </button>
    </aside>

    <main class="panel__contenido">
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

.lateral__nav {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 18px;
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

.panel__vista {
  display: block;
  width: 100%;
  height: 100%;
  overflow: visible;
}
</style>
