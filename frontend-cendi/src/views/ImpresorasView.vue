<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const API_URL = 'http://localhost:5141/api/Impresoras'

const TOKEN_KEY = 'sgcti_token'

const ESTADOS_PING = ['En Línea', 'Inestable', 'Desconectado'] as const

interface Impresora {
  id: number
  modelo: string
  ip: string
  fabricante?: string
  nivelTonnerNegro: number
  estadoPing: number | string
  latenciaMs?: number
  contadorTotalPaginas?: number
  diasEstimadosAgotamientoTonner?: number
  fechaInstalacion?: string
}

interface Fila {
  id: number
  modelo: string
  ip: string
  nivel: number
  nivelClase: string
  estadoPingClase: string
  latencia: number
  latenciaClase: string
  fabricante: string
  contadorTotalPaginas: number
  diasEstimadosAgotamientoTonner: number
  fechaInstalacion: string
}

const router = useRouter()

const filas = ref<Fila[]>([])
const cargando = ref(true)
const error = ref('')

const detalle = ref<Fila | null>(null)

function claseDe(texto: string): string {
  return texto
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/\s+/g, '-')
}

function normalizar(valor: number | string, etiquetas: readonly string[]): { texto: string; clase: string } {
  if (typeof valor === 'number') {
    const texto = etiquetas[valor] ?? 'Desconocido'
    return { texto, clase: claseDe(texto) }
  }

  const plano = valor.replace(/([a-z])([A-Z])/g, '$1 $2')
  const encontrado = etiquetas.find((e) => e.toLowerCase() === plano.toLowerCase())
  const texto = encontrado ?? valor

  return { texto, clase: claseDe(texto) }
}

function claseDeNivel(nivel: number): string {
  if (nivel <= 15) {
    return 'bajo'
  }

  if (nivel <= 40) {
    return 'medio'
  }

  return 'alto'
}

function claseDeLatencia(latencia: number, desconectado: boolean): string {
  if (desconectado || latencia > 150) {
    return 'rojo'
  }

  if (latencia < 50) {
    return 'verde'
  }

  return 'amarillo'
}

function formatearFecha(iso: string | undefined): string {
  if (!iso) {
    return '—'
  }

  const fecha = new Date(iso)

  if (Number.isNaN(fecha.getTime())) {
    return iso
  }

  return fecha.toLocaleDateString('es-MX', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

function abrirDetalle(fila: Fila) {
  detalle.value = fila
}

function cerrarDetalle() {
  detalle.value = null
}

function alPulsarEscape(evento: KeyboardEvent) {
  if (evento.key === 'Escape') {
    cerrarDetalle()
  }
}

async function cargarImpresoras() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Impresora[]>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    filas.value = data.map((impresora) => {
      const estadoPing = normalizar(impresora.estadoPing, ESTADOS_PING)
      const nivel = Math.max(0, Math.min(100, impresora.nivelTonnerNegro ?? 0))
      const latencia = Math.max(0, impresora.latenciaMs ?? 0)
      const desconectado = estadoPing.clase === 'desconectado'

      return {
        id: impresora.id,
        modelo: impresora.modelo,
        ip: impresora.ip,
        nivel,
        nivelClase: claseDeNivel(nivel),
        estadoPingClase: estadoPing.clase,
        latencia,
        latenciaClase: claseDeLatencia(latencia, desconectado),
        fabricante: impresora.fabricante || '—',
        contadorTotalPaginas: impresora.contadorTotalPaginas ?? 0,
        diasEstimadosAgotamientoTonner: impresora.diasEstimadosAgotamientoTonner ?? 0,
        fechaInstalacion: formatearFecha(impresora.fechaInstalacion),
      }
    })
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    error.value = 'No se pudieron cargar las impresoras. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
  cargarImpresoras()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Impresoras</h1>
      <p class="vista__subtitulo">Control de equipos y consumibles de impresión</p>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Equipos registrados</h2>
        <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarImpresoras">
          {{ cargando ? 'Cargando...' : 'Actualizar' }}
        </button>
      </header>

      <p v-if="error" class="alerta">{{ error }}</p>

      <div v-if="cargando" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando impresoras...</p>
      </div>

      <div v-else-if="filas.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay impresoras registradas.</p>
      </div>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th class="tabla__th tabla__th--corta">ID</th>
            <th class="tabla__th">Modelo</th>
            <th class="tabla__th">IP</th>
            <th class="tabla__th tabla__th--toner">Nivel Tóner Negro</th>
            <th class="tabla__th tabla__th--corta">Latencia</th>
            <th class="tabla__th tabla__th--corta">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="fila in filas" :key="fila.id" class="tabla__fila">
            <td class="tabla__td tabla__td--id">{{ fila.id }}</td>
            <td class="tabla__td tabla__td--modelo">{{ fila.modelo }}</td>
            <td class="tabla__td tabla__td--ip">{{ fila.ip }}</td>
            <td class="tabla__td">
              <div class="toner">
                <span class="toner__barra">
                  <span class="toner__relleno" :class="`toner__relleno--${fila.nivelClase}`" :style="{ width: `${fila.nivel}%` }"></span>
                </span>
                <span class="toner__valor" :class="`toner__valor--${fila.nivelClase}`">{{ fila.nivel }}%</span>
              </div>
            </td>
            <td class="tabla__td">
              <span class="latencia">
                <span class="latencia__luz" :class="`latencia__luz--${fila.latenciaClase}`"></span>
                <span class="latencia__valor" :class="`latencia__valor--${fila.latenciaClase}`">
                  {{ fila.estadoPingClase === 'desconectado' ? 'Sin conexión' : `${fila.latencia} ms` }}
                </span>
              </span>
            </td>
            <td class="tabla__td tabla__td--acciones">
              <button class="detalles" type="button" @click="abrirDetalle(fila)">Ver Detalles</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="detalle" class="modal" @click.self="cerrarDetalle">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-titulo">
        <header class="modal__cabecera">
          <h3 id="modal-titulo" class="modal__titulo">{{ detalle.modelo }}</h3>
          <button class="modal__cerrar" type="button" aria-label="Cerrar" @click="cerrarDetalle">
            &times;
          </button>
        </header>

        <dl class="modal__lista">
          <div class="modal__dato">
            <dt class="modal__etiqueta">Fabricante</dt>
            <dd class="modal__valor">{{ detalle.fabricante }}</dd>
          </div>
          <div class="modal__dato">
            <dt class="modal__etiqueta">Contador Total de Páginas</dt>
            <dd class="modal__valor modal__valor--numero">
              {{ detalle.contadorTotalPaginas.toLocaleString('es-MX') }}
            </dd>
          </div>
          <div class="modal__dato">
            <dt class="modal__etiqueta">Días Estimados Agotamiento</dt>
            <dd class="modal__valor modal__valor--numero">{{ detalle.diasEstimadosAgotamientoTonner }}</dd>
          </div>
          <div class="modal__dato">
            <dt class="modal__etiqueta">Fecha Instalación</dt>
            <dd class="modal__valor">{{ detalle.fechaInstalacion }}</dd>
          </div>
        </dl>

        <footer class="modal__pie">
          <button class="boton boton--primario" type="button" @click="cerrarDetalle">Cerrar</button>
        </footer>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vista__encabezado {
  margin-bottom: 20px;
}

.vista__titulo {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1f2b;
}

.vista__subtitulo {
  margin: 4px 0 0;
  font-size: 13px;
  color: #6b7280;
}

.tarjeta {
  background: #ffffff;
  border: 1px solid #e2e5ea;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(16, 24, 40, 0.06);
  overflow: hidden;
}

.tarjeta__cabecera {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #eceff3;
}

.tarjeta__titulo {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1a1f2b;
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

.boton--primario {
  color: #ffffff;
  background: #1f3a68;
}

.boton--primario:hover:not(:disabled) {
  background: #16294a;
}

.boton--secundario:hover:not(:disabled) {
  background: #f8fafc;
}

.boton:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.alerta {
  margin: 16px 20px;
  padding: 10px 12px;
  font-size: 13px;
  color: #b42318;
  background: #fef3f2;
  border: 1px solid #fecdca;
  border-radius: 6px;
}

.estado-vacio {
  padding: 48px 20px;
  text-align: center;
}

.estado-vacio__texto {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
}

.tabla {
  width: 100%;
  border-collapse: collapse;
}

.tabla__th {
  padding: 11px 20px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.6px;
  text-align: left;
  text-transform: uppercase;
  color: #475467;
  background: #f9fafb;
  border-bottom: 1px solid #e2e5ea;
}

.tabla__th--corta,
.tabla__th--toner {
  width: 1%;
  white-space: nowrap;
}

.tabla__fila:hover {
  background: #f9fafb;
}

.tabla__td {
  padding: 13px 20px;
  font-size: 14px;
  color: #344054;
  border-bottom: 1px solid #f2f4f7;
}

.tabla__fila:last-child .tabla__td {
  border-bottom: none;
}

.tabla__td--id {
  font-variant-numeric: tabular-nums;
  color: #6b7280;
}

.tabla__td--modelo {
  font-weight: 500;
  color: #1a1f2b;
}

.tabla__td--ip {
  font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
  font-size: 13px;
  color: #475467;
}

.tabla__td--acciones {
  text-align: right;
}

.detalles {
  padding: 5px 12px;
  font-size: 12px;
  font-weight: 500;
  font-family: inherit;
  color: #1f3a68;
  background: #eff8ff;
  border: 1px solid #b2ddff;
  border-radius: 6px;
  cursor: pointer;
}

.detalles:hover {
  background: #d1e9ff;
  border-color: #84caff;
}

.latencia {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.latencia__luz {
  width: 9px;
  height: 9px;
  border-radius: 50%;
  flex-shrink: 0;
}

.latencia__luz--verde {
  background: #12b76a;
  box-shadow: 0 0 0 3px rgba(18, 183, 106, 0.18);
}

.latencia__luz--amarillo {
  background: #f79009;
  box-shadow: 0 0 0 3px rgba(247, 144, 9, 0.18);
}

.latencia__luz--rojo {
  background: #f04438;
  box-shadow: 0 0 0 3px rgba(240, 68, 56, 0.18);
}

.latencia__valor {
  font-size: 13px;
  font-weight: 500;
  font-variant-numeric: tabular-nums;
  color: #344054;
}

.latencia__valor--verde {
  color: #067647;
}

.latencia__valor--amarillo {
  color: #b54708;
}

.latencia__valor--rojo {
  color: #b42318;
}

.toner {
  display: flex;
  align-items: center;
  gap: 10px;
}

.toner__barra {
  display: block;
  width: 90px;
  height: 6px;
  background: #eef1f5;
  border-radius: 999px;
  overflow: hidden;
}

.toner__relleno {
  display: block;
  height: 100%;
  border-radius: 999px;
}

.toner__relleno--alto {
  background: #12b76a;
}

.toner__relleno--medio {
  background: #f79009;
}

.toner__relleno--bajo {
  background: #f04438;
}

.toner__valor {
  font-size: 12px;
  font-weight: 500;
  font-variant-numeric: tabular-nums;
  color: #475467;
}

.toner__valor--bajo {
  color: #b42318;
}

.modal {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background: rgba(16, 24, 40, 0.55);
}

.modal__panel {
  width: 100%;
  max-width: 440px;
  background: #ffffff;
  border-radius: 10px;
  box-shadow: 0 20px 40px rgba(16, 24, 40, 0.22);
  overflow: hidden;
}

.modal__cabecera {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 16px 20px;
  border-bottom: 1px solid #eceff3;
}

.modal__titulo {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1a1f2b;
}

.modal__cerrar {
  padding: 0;
  width: 28px;
  height: 28px;
  font-size: 20px;
  line-height: 1;
  font-family: inherit;
  color: #667085;
  background: transparent;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.modal__cerrar:hover {
  color: #1a1f2b;
  background: #f2f4f7;
}

.modal__lista {
  margin: 0;
  padding: 4px 20px;
}

.modal__dato {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 16px;
  padding: 13px 0;
  border-bottom: 1px solid #f2f4f7;
}

.modal__dato:last-child {
  border-bottom: none;
}

.modal__etiqueta {
  font-size: 13px;
  color: #667085;
}

.modal__valor {
  margin: 0;
  font-size: 14px;
  font-weight: 500;
  color: #1a1f2b;
  text-align: right;
}

.modal__valor--numero {
  font-variant-numeric: tabular-nums;
}

.modal__pie {
  display: flex;
  justify-content: flex-end;
  padding: 14px 20px;
  background: #f9fafb;
  border-top: 1px solid #eceff3;
}
</style>
