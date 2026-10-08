<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue'
import { cerrarConfirmacion, estadoConfirmacion } from '../composables/useConfirm'

function alPulsarEscape(evento: KeyboardEvent) {
  if (evento.key === 'Escape' && estadoConfirmacion.visible) {
    cerrarConfirmacion(false)
  }
}

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})
</script>

<template>
  <div
    v-if="estadoConfirmacion.visible"
    class="confirmacion"
    @click.self="cerrarConfirmacion(false)"
  >
    <div
      class="confirmacion__panel"
      role="alertdialog"
      aria-modal="true"
      aria-labelledby="confirmacion-titulo"
    >
      <div class="confirmacion__cuerpo">
        <span class="confirmacion__icono" aria-hidden="true">⚠️</span>
        <p id="confirmacion-titulo" class="confirmacion__mensaje">
          {{ estadoConfirmacion.mensaje }}
        </p>
      </div>

      <footer class="confirmacion__pie">
        <button class="boton boton--secundario" type="button" @click="cerrarConfirmacion(false)">
          {{ estadoConfirmacion.cancelarTexto }}
        </button>
        <button class="boton boton--peligro" type="button" @click="cerrarConfirmacion(true)">
          {{ estadoConfirmacion.confirmarTexto }}
        </button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.confirmacion {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background: rgba(16, 24, 40, 0.55);
}

.confirmacion__panel {
  width: 100%;
  max-width: 420px;
  background: #ffffff;
  border-radius: 10px;
  box-shadow: 0 20px 40px rgba(16, 24, 40, 0.22);
  overflow: hidden;
}

.confirmacion__cuerpo {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 20px;
}

.confirmacion__icono {
  flex-shrink: 0;
  font-size: 18px;
  line-height: 1.4;
}

.confirmacion__mensaje {
  margin: 0;
  font-size: 14px;
  line-height: 1.5;
  color: #1a1f2b;
}

.confirmacion__pie {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 14px 20px;
  background: #f9fafb;
  border-top: 1px solid #eceff3;
}

.boton {
  padding: 8px 14px;
  font-size: 13px;
  font-weight: 500;
  font-family: inherit;
  border: 1px solid transparent;
  border-radius: 6px;
  cursor: pointer;
}

.boton--secundario {
  color: #1f3a68;
  background: #ffffff;
  border-color: #d0d5dd;
}

.boton--secundario:hover {
  background: #f8fafc;
}

.boton--peligro {
  color: #ffffff;
  background: #b42318;
  border-color: #b42318;
}

.boton--peligro:hover {
  background: #912018;
}
</style>
