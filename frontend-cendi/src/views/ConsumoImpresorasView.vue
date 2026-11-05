<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js'
import { Bar, Line } from 'vue-chartjs'
import type { ChartData, ChartOptions } from 'chart.js'
import { API_BASE_URL } from '../config'

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  Filler,
  BarElement,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
)

const API_BASE = API_BASE_URL
const TOKEN_KEY = 'sgcti_token'

const PALETA_IMPRESORAS = [
  { borde: '#2563eb', fondo: 'rgba(37, 99, 235, 0.18)', barra: '#3b82f6' },
  { borde: '#059669', fondo: 'rgba(5, 150, 105, 0.18)', barra: '#10b981' },
  { borde: '#d97706', fondo: 'rgba(217, 119, 6, 0.18)', barra: '#f59e0b' },
  { borde: '#dc2626', fondo: 'rgba(220, 38, 38, 0.18)', barra: '#ef4444' },
  { borde: '#7c3aed', fondo: 'rgba(124, 58, 237, 0.18)', barra: '#8b5cf6' },
  { borde: '#0891b2', fondo: 'rgba(8, 145, 178, 0.18)', barra: '#06b6d4' },
  { borde: '#db2777', fondo: 'rgba(219, 39, 119, 0.18)', barra: '#ec4899' },
  { borde: '#4f46e5', fondo: 'rgba(79, 70, 229, 0.18)', barra: '#6366f1' },
  { borde: '#ea580c', fondo: 'rgba(234, 88, 12, 0.18)', barra: '#f97316' },
  { borde: '#65a30d', fondo: 'rgba(101, 163, 13, 0.18)', barra: '#84cc16' },
]

const router = useRouter()

const fechaInicio = ref('')
const fechaFin = ref('')

const descargandoReporte = ref(false)
const cargandoGrafica = ref(false)
const errorGrafica = ref('')
const mensaje = ref('')

interface DatoGrafica {
  impresora: string
  total: number
}

interface RegistroTabla {
  id: number
  fecha: string
  impresora: string
  contador: number
  consumo: number
  observacion: string
}

const datosGrafica = ref<DatoGrafica[]>([])
const vistaActual = ref<'grafica' | 'tabla'>('grafica')
const tipoGraficaTemporal = ref<'lineas' | 'barras'>('barras')
const impresoraFiltro = ref<string>('TODAS')
const registrosTabla = ref<RegistroTabla[]>([])
const guardandoObsId = ref<number | null>(null)
const ultimaObservacion = ref<Record<number, string>>({})
const tieneFechas = computed(() => Boolean(fechaInicio.value && fechaFin.value))

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

function fechasComoDato(fecha: Date): string {
  const anio = fecha.getFullYear()
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  const dia = String(fecha.getDate()).padStart(2, '0')
  return `${anio}-${mes}-${dia}`
}

function formatearFecha(iso: string): string {
  const partes = iso.slice(0, 10).split('-')
  if (partes.length !== 3) return iso
  return `${partes[2]}/${partes[1]}/${partes[0]}`
}

const etiquetaPeriodo = computed(() => {
  if (!fechaInicio.value || !fechaFin.value) return ''
  return `${formatearFecha(fechaInicio.value)} al ${formatearFecha(fechaFin.value)}`
})

const impresorasDisponibles = computed(() => {
  const conjunto = new Set<string>()
  for (const reg of registrosTabla.value) {
    if (reg.impresora) conjunto.add(reg.impresora)
  }
  for (const item of datosGrafica.value) {
    if (item.impresora) conjunto.add(item.impresora)
  }
  return Array.from(conjunto).sort((a, b) => a.localeCompare(b))
})

const fechasOrdenadasIso = computed(() => {
  const conjunto = new Set<string>()
  for (const reg of registrosTabla.value) {
    if (reg.fecha) conjunto.add(reg.fecha.slice(0, 10))
  }
  return Array.from(conjunto).sort()
})

const registrosTablaFiltrados = computed(() =>
  impresoraFiltro.value === 'TODAS'
    ? registrosTabla.value
    : registrosTabla.value.filter((r) => r.impresora === impresoraFiltro.value),
)

const totalPaginasPeriodo = computed(() =>
  registrosTablaFiltrados.value.reduce((acc, item) => acc + (item.consumo || 0), 0),
)

const chartEvolucionPorFechaLineData = computed<ChartData<'line'>>(() => {
  const fechasIso = fechasOrdenadasIso.value
  const etiquetasFecha = fechasIso.map((f) => formatearFecha(f))

  const impresoras =
    impresoraFiltro.value === 'TODAS'
      ? impresorasDisponibles.value
      : impresorasDisponibles.value.filter((imp) => imp === impresoraFiltro.value)

  const datasets = impresoras.map((nombreImpresora, idx) => {
    const color = PALETA_IMPRESORAS[idx % PALETA_IMPRESORAS.length]!
    const datosPorFecha = fechasIso.map((fechaIso) =>
      registrosTabla.value
        .filter((r) => r.impresora === nombreImpresora && r.fecha.slice(0, 10) === fechaIso)
        .reduce((suma, r) => suma + (r.consumo || 0), 0),
    )

    return {
      label: nombreImpresora,
      data: datosPorFecha,
      tension: 0.35,
      fill: impresoras.length === 1,
      backgroundColor: color.fondo,
      borderColor: color.borde,
      borderWidth: 2.5,
      pointBackgroundColor: '#ffffff',
      pointBorderColor: color.borde,
      pointBorderWidth: 2,
      pointRadius: 4,
      pointHoverRadius: 6,
    }
  })

  return { labels: etiquetasFecha, datasets }
})

const chartEvolucionPorFechaBarData = computed<ChartData<'bar'>>(() => {
  const fechasIso = fechasOrdenadasIso.value
  const etiquetasFecha = fechasIso.map((f) => formatearFecha(f))

  const impresoras =
    impresoraFiltro.value === 'TODAS'
      ? impresorasDisponibles.value
      : impresorasDisponibles.value.filter((imp) => imp === impresoraFiltro.value)

  const datasets = impresoras.map((nombreImpresora, idx) => {
    const color = PALETA_IMPRESORAS[idx % PALETA_IMPRESORAS.length]!
    const datosPorFecha = fechasIso.map((fechaIso) =>
      registrosTabla.value
        .filter((r) => r.impresora === nombreImpresora && r.fecha.slice(0, 10) === fechaIso)
        .reduce((suma, r) => suma + (r.consumo || 0), 0),
    )

    return {
      label: nombreImpresora,
      data: datosPorFecha,
      backgroundColor: color.barra,
      borderRadius: 5,
    }
  })

  return { labels: etiquetasFecha, datasets }
})

const chartEvolucionLineOptions = computed<ChartOptions<'line'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: { display: true, position: 'top' },
    tooltip: {
      callbacks: {
        title: (items) => `Fecha: ${items[0]?.label ?? ''}`,
        label: (ctx) => ` ${ctx.dataset.label}: ${Number(ctx.parsed.y ?? 0).toLocaleString()} páginas impresas`,
      },
    },
  },
  scales: {
    x: {
      title: { display: true, text: 'Fecha de registro (dd/mm/aaaa)' },
      ticks: { autoSkip: false, maxRotation: 45, minRotation: 0 },
      grid: { display: false },
    },
    y: {
      beginAtZero: true,
      title: { display: true, text: 'Páginas impresas' },
      grid: { color: 'rgba(17, 24, 39, 0.06)' },
    },
  },
}))

const chartEvolucionBarOptions = computed<ChartOptions<'bar'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: 'index', intersect: false },
  plugins: {
    legend: { display: true, position: 'top' },
    tooltip: {
      callbacks: {
        title: (items) => `Fecha: ${items[0]?.label ?? ''}`,
        label: (ctx) => ` ${ctx.dataset.label}: ${Number(ctx.parsed.y ?? 0).toLocaleString()} páginas impresas`,
      },
    },
  },
  scales: {
    x: {
      title: { display: true, text: 'Fecha de registro (dd/mm/aaaa)' },
      grid: { display: false },
    },
    y: {
      beginAtZero: true,
      title: { display: true, text: 'Páginas impresas' },
      grid: { color: 'rgba(17, 24, 39, 0.06)' },
    },
  },
}))

const chartTotalesPorImpresoraData = computed<ChartData<'bar'>>(() => {
  const fuente =
    datosGrafica.value.length > 0
      ? datosGrafica.value
      : impresorasDisponibles.value.map((imp) => ({
          impresora: imp,
          total: registrosTabla.value
            .filter((r) => r.impresora === imp)
            .reduce((s, r) => s + (r.consumo || 0), 0),
        }))

  return {
    labels: fuente.map((d) => `${d.impresora} (${etiquetaPeriodo.value})`),
    datasets: [
      {
        label: 'Total de páginas impresas en el periodo',
        data: fuente.map((d) => d.total),
        backgroundColor: fuente.map((_, idx) => PALETA_IMPRESORAS[idx % PALETA_IMPRESORAS.length]!.barra),
        borderRadius: 6,
      },
    ],
  }
})

const chartTotalesPorImpresoraOptions = computed<ChartOptions<'bar'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (ctx) =>
          ` Total acumulado (${etiquetaPeriodo.value}): ${Number(ctx.parsed.y ?? 0).toLocaleString()} páginas`,
      },
    },
  },
  scales: {
    x: {
      title: { display: true, text: `Impresora y Periodo Evaluado (${etiquetaPeriodo.value})` },
      grid: { display: false },
    },
    y: {
      beginAtZero: true,
      title: { display: true, text: 'Total de páginas impresas' },
      grid: { color: 'rgba(17, 24, 39, 0.06)' },
    },
  },
}))

async function descargarReporte() {
  if (!tieneFechas.value) {
    mensaje.value = 'Selecciona la fecha de inicio y la fecha de fin antes de descargar el reporte.'
    return
  }

  descargandoReporte.value = true
  mensaje.value = ''

  try {
    const { data } = await axios.get(`${API_BASE}/api/reportes/consumo-impresoras`, {
      params: { fechaInicio: fechaInicio.value, fechaFin: fechaFin.value },
      responseType: 'blob',
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    const url = window.URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = `ConsumoImpresoras_${fechaInicio.value}_a_${fechaFin.value}.pdf`
    document.body.appendChild(enlace)
    enlace.click()
    enlace.remove()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    toast.error('No se pudo descargar el reporte. Intenta nuevamente.')
  } finally {
    descargandoReporte.value = false
  }
}

async function consultarEndpointsConsumo(inicio: string, fin: string) {
  const headers = { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) }

  const [grafica, datos] = await Promise.all([
    axios.get<DatoGrafica[]>(`${API_BASE}/api/reportes/grafica-consumo`, {
      params: { fechaInicio: inicio, fechaFin: fin },
      headers,
    }),
    axios.get<RegistroTabla[]>(`${API_BASE}/api/reportes/datos-consumo`, {
      params: { fechaInicio: inicio, fechaFin: fin },
      headers,
    }),
  ])

  return { grafica: grafica.data, datos: datos.data }
}

async function cargarGrafica(esCargaInicial = false) {
  if (!tieneFechas.value) {
    errorGrafica.value = 'Selecciona la fecha de inicio y la fecha de fin.'
    return
  }

  cargandoGrafica.value = true
  errorGrafica.value = ''

  try {
    let resultado = await consultarEndpointsConsumo(fechaInicio.value, fechaFin.value)

    if (esCargaInicial && resultado.datos.length === 0) {
      const inicioAnio = `${new Date().getFullYear()}-01-01`
      const resultadoAmpliado = await consultarEndpointsConsumo(inicioAnio, fechaFin.value)

      if (resultadoAmpliado.datos.length > 0) {
        fechaInicio.value = inicioAnio
        resultado = resultadoAmpliado
      }
    }

    datosGrafica.value = resultado.grafica
    registrosTabla.value = resultado.datos.map((registro) => ({
      ...registro,
      observacion: registro.observacion ?? '',
    }))

    const observaciones: Record<number, string> = {}
    for (const registro of registrosTabla.value) {
      observaciones[registro.id] = registro.observacion
    }
    ultimaObservacion.value = observaciones
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorGrafica.value = 'No se pudieron cargar los datos. Intenta nuevamente.'
  } finally {
    cargandoGrafica.value = false
  }
}

async function guardarObservacion(registro: RegistroTabla) {
  const texto = (registro.observacion ?? '').trim()
  if (texto === (ultimaObservacion.value[registro.id] ?? '') || guardandoObsId.value !== null) {
    return
  }

  guardandoObsId.value = registro.id

  try {
    await axios.put(
      `${API_BASE}/api/reportes/consumo/${registro.id}/observacion`,
      { observacion: texto },
      { headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) } },
    )

    ultimaObservacion.value[registro.id] = texto
    registro.observacion = texto
    toast.success('Observación guardada.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) cerrarSesion()
  } finally {
    guardandoObsId.value = null
  }
}

onMounted(() => {
  const hoy = new Date()
  const inicioAnio = new Date(hoy.getFullYear(), 0, 1)

  fechaInicio.value = fechasComoDato(inicioAnio)
  fechaFin.value = fechasComoDato(hoy)

  cargarGrafica(true)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <div>
        <h1 class="vista__titulo">Consumo y Métricas</h1>
        <p class="vista__subtitulo">
          Histórico de páginas impresas por fecha, impresora y reporte general
        </p>
      </div>
      <button
        class="boton boton--primario"
        type="button"
        :disabled="descargandoReporte"
        @click="descargarReporte"
      >
        {{ descargandoReporte ? 'Generando...' : 'Descargar Reporte PDF' }}
      </button>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Periodo y filtros de consulta</h2>
        <span v-if="tieneFechas" class="periodo-badge">
          📅 Periodo activo: {{ etiquetaPeriodo }}
        </span>
      </header>
      <div class="filtros">
        <div class="campo">
          <label class="campo__etiqueta" for="fecha-inicio">Fecha Inicio</label>
          <input id="fecha-inicio" v-model="fechaInicio" class="campo__input" type="date" />
        </div>
        <div class="campo">
          <label class="campo__etiqueta" for="fecha-fin">Fecha Fin</label>
          <input id="fecha-fin" v-model="fechaFin" class="campo__input" type="date" />
        </div>
        <div class="campo">
          <label class="campo__etiqueta" for="filtro-impresora">Impresora</label>
          <select id="filtro-impresora" v-model="impresoraFiltro" class="campo__input">
            <option value="TODAS">Todas las impresoras ({{ impresorasDisponibles.length }})</option>
            <option v-for="imp in impresorasDisponibles" :key="imp" :value="imp">
              {{ imp }}
            </option>
          </select>
        </div>
        <button
          class="boton boton--secundario"
          type="button"
          :disabled="!tieneFechas || cargandoGrafica"
          @click="cargarGrafica(false)"
        >
          {{ cargandoGrafica ? 'Cargando...' : 'Generar Gráfica' }}
        </button>
      </div>
    </section>

    <section class="tarjeta">
      <header class="tarjeta__cabecera tarjeta__cabecera--fija">
        <div class="tarjeta__cabecera-texto">
          <h2 class="tarjeta__titulo">
            Consumo de páginas impresas por Fecha e Impresora ({{ etiquetaPeriodo }})
          </h2>
          <p class="tarjeta__subtitulo">
            Total en el periodo seleccionado:
            <strong>{{ totalPaginasPeriodo.toLocaleString() }} páginas impresas</strong>
            en {{ fechasOrdenadasIso.length }} fechas registradas
          </p>
        </div>

        <div class="toggle toggle--fijo">
          <button
            class="toggle__boton"
            :class="{ 'toggle__boton--activo': vistaActual === 'grafica' }"
            type="button"
            @click="vistaActual = 'grafica'"
          >
            Gráfica
          </button>
          <button
            class="toggle__boton"
            :class="{ 'toggle__boton--activo': vistaActual === 'tabla' }"
            type="button"
            @click="vistaActual = 'tabla'"
          >
            Tabla Detallada
          </button>
        </div>
      </header>

      <p v-if="errorGrafica" class="alerta">{{ errorGrafica }}</p>

      <div v-if="vistaActual === 'grafica'">
        <div v-if="cargandoGrafica" class="estado-vacio">
          <p class="estado-vacio__texto">Cargando gráficas...</p>
        </div>

        <div v-else-if="registrosTabla.length === 0 && datosGrafica.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay registros de consumo en el periodo seleccionado.</p>
        </div>

        <div v-else class="graficas-contenedor">
          <div class="bloque-grafica">
            <div class="bloque-grafica__cabecera">
              <h3 class="bloque-grafica__titulo">
                1. Evolución Diaria por Fecha de Registro, Páginas Impresas e Impresora
              </h3>
              <div class="toggle">
                <button
                  class="toggle__boton"
                  :class="{ 'toggle__boton--activo': tipoGraficaTemporal === 'barras' }"
                  type="button"
                  @click="tipoGraficaTemporal = 'barras'"
                >
                  Barras por Fecha
                </button>
                <button
                  class="toggle__boton"
                  :class="{ 'toggle__boton--activo': tipoGraficaTemporal === 'lineas' }"
                  type="button"
                  @click="tipoGraficaTemporal = 'lineas'"
                >
                  Líneas por Fecha
                </button>
              </div>
            </div>
            <div class="grafica__envoltura">
              <Bar
                v-if="tipoGraficaTemporal === 'barras'"
                :data="chartEvolucionPorFechaBarData"
                :options="chartEvolucionBarOptions"
              />
              <Line
                v-else
                :data="chartEvolucionPorFechaLineData"
                :options="chartEvolucionLineOptions"
              />
            </div>
          </div>

          <div class="bloque-grafica">
            <h3 class="bloque-grafica__titulo">
              2. Total Acumulado por Impresora ({{ etiquetaPeriodo }})
            </h3>
            <div class="grafica__envoltura grafica__envoltura--compacta">
              <Bar :data="chartTotalesPorImpresoraData" :options="chartTotalesPorImpresoraOptions" />
            </div>
          </div>
        </div>
      </div>

      <div v-else-if="vistaActual === 'tabla'">
        <div v-if="cargandoGrafica" class="estado-vacio">
          <p class="estado-vacio__texto">Cargando datos...</p>
        </div>

        <div v-else-if="registrosTablaFiltrados.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay registros de consumo en el periodo seleccionado.</p>
        </div>

        <div v-else class="tabla__envoltura">
          <table class="tabla">
            <thead>
              <tr>
                <th class="tabla__th">Fecha</th>
                <th class="tabla__th">Impresora</th>
                <th class="tabla__th">Contador</th>
                <th class="tabla__th">Páginas Impresas (Consumo)</th>
                <th class="tabla__th">Observación</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="registro in registrosTablaFiltrados" :key="registro.id" class="tabla__fila">
                <td class="tabla__td">{{ formatearFecha(registro.fecha) }}</td>
                <td class="tabla__td">{{ registro.impresora }}</td>
                <td class="tabla__td">{{ registro.contador.toLocaleString() }}</td>
                <td class="tabla__td">
                  <strong>{{ registro.consumo.toLocaleString() }}</strong> págs.
                </td>
                <td class="tabla__td">
                  <input
                    v-model="registro.observacion"
                    class="campo__input campo__input--observacion"
                    type="text"
                    :disabled="guardandoObsId === registro.id"
                    @blur="guardarObservacion(registro)"
                    @keyup.enter="guardarObservacion(registro)"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.vista__encabezado { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 20px; }
.vista__titulo { margin: 0; font-size: 20px; font-weight: 600; color: #1a1f2b; }
.vista__subtitulo { margin: 4px 0 0; font-size: 13px; color: #6b7280; }
.tarjeta { margin-bottom: 20px; background: #ffffff; border: 1px solid #e2e5ea; border-radius: 10px; box-shadow: 0 1px 3px rgba(16, 24, 40, 0.06); overflow: hidden; }
.tarjeta__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 16px 20px; border-bottom: 1px solid #eceff3; }
.tarjeta__cabecera--fija { flex-wrap: nowrap; }
.tarjeta__cabecera-texto { min-width: 0; flex: 1 1 auto; }
.tarjeta__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.tarjeta__subtitulo { margin: 4px 0 0; font-size: 12.5px; color: #475467; }
.periodo-badge { display: inline-flex; align-items: center; padding: 5px 12px; font-size: 12px; font-weight: 600; color: #1f3a68; background: #e8eef8; border-radius: 999px; white-space: nowrap; }
.boton { padding: 8px 14px; font-size: 13px; font-weight: 500; font-family: inherit; border: 1px solid transparent; border-radius: 6px; cursor: pointer; }
.boton--primario { color: #ffffff; background: #1f3a68; }
.boton--primario:hover:not(:disabled) { background: #16294a; }
.boton--secundario { color: #1f3a68; background: #ffffff; border-color: #d0d5dd; }
.boton--secundario:hover:not(:disabled) { background: #f8fafc; }
.boton:disabled { cursor: not-allowed; opacity: 0.6; }
.filtros { display: flex; align-items: flex-end; flex-wrap: wrap; gap: 14px; padding: 16px 20px; }
.filtros .campo { display: flex; flex-direction: column; gap: 4px; }
.campo__etiqueta { font-size: 12px; font-weight: 500; color: #475467; }
.campo__input { padding: 8px 12px; font-size: 13px; font-family: inherit; color: #1a1f2b; background: #ffffff; border: 1px solid #d0d5dd; border-radius: 6px; box-sizing: border-box; }
.campo__input:focus { outline: none; border-color: #1f3a68; box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12); }
.alerta { margin: 16px 20px; padding: 10px 12px; font-size: 13px; color: #b42318; background: #fef3f2; border: 1px solid #fecdca; border-radius: 6px; }
.estado-vacio { padding: 48px 20px; text-align: center; }
.estado-vacio__texto { margin: 0; font-size: 14px; color: #6b7280; }
.graficas-contenedor { display: grid; gap: 24px; padding: 20px; }
.bloque-grafica { padding: 16px; background: #fbfcfd; border: 1px solid #eceff3; border-radius: 10px; }
.bloque-grafica__cabecera { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 12px; margin-bottom: 12px; }
.bloque-grafica__titulo { margin: 0; font-size: 13.5px; font-weight: 600; color: #1f3a68; }
.grafica__envoltura { position: relative; height: 360px; width: 100%; }
.grafica__envoltura--compacta { height: 280px; }
.toggle { display: inline-flex; padding: 3px; background: #f2f4f7; border: 1px solid #e2e5ea; border-radius: 8px; }
.toggle--fijo { flex-shrink: 0; margin-left: auto; }
.toggle__boton { padding: 6px 14px; font-size: 12.5px; font-weight: 500; font-family: inherit; color: #475467; background: transparent; border: none; border-radius: 6px; cursor: pointer; white-space: nowrap; }
.toggle__boton:hover { color: #1f3a68; }
.toggle__boton--activo { color: #ffffff; background: #1f3a68; }
.toggle__boton--activo:hover { color: #ffffff; }
.tabla__envoltura { padding: 8px 20px 20px; overflow-x: auto; }
.tabla { width: 100%; border-collapse: collapse; }
.tabla__th { padding: 11px 12px; font-size: 11px; font-weight: 600; letter-spacing: 0.6px; text-align: left; text-transform: uppercase; color: #475467; background: #f9fafb; border-bottom: 1px solid #e2e5ea; }
.tabla__fila:hover { background: #f9fafb; }
.tabla__td { padding: 10px 12px; font-size: 13px; color: #344054; border-bottom: 1px solid #f2f4f7; }
.tabla__fila:last-child .tabla__td { border-bottom: none; }
.campo__input--observacion { width: 100%; min-width: 180px; }
</style>