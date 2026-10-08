<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  Title,
  Tooltip,
} from 'chart.js'
import { Bar } from 'vue-chartjs'
import type { ChartData, ChartOptions } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

const API_BASE = 'http://localhost:5141'

const TOKEN_KEY = 'sgcti_token'

const router = useRouter()

const fechaInicio = ref('')
const fechaFin = ref('')

const descargandoReporte = ref(false)

const archivoExcel = ref<File | null>(null)
const inputExcel = ref<HTMLInputElement | null>(null)
const subiendoExcel = ref(false)

const cargandoGrafica = ref(false)
const errorGrafica = ref('')

const mensaje = ref('')

interface DatoGrafica {
  impresora: string
  total: number
}

interface ResultadoImportacion {
  registrosImportados: number
  impresorasCreadas: number
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

async function descargarReporte() {
  if (!tieneFechas.value) {
    mensaje.value = 'Selecciona la fecha de inicio y la fecha de fin antes de descargar el reporte.'
    return
  }

  descargandoReporte.value = true
  mensaje.value = ''

  try {
    const { data } = await axios.get(`${API_BASE}/api/reportes/consumo-impresoras`, {
      params: {
        fechaInicio: fechaInicio.value,
        fechaFin: fechaFin.value,
      },
      responseType: 'blob',
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    const url = window.URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = 'ConsumoImpresorasCendi.pdf'
    document.body.appendChild(enlace)
    enlace.click()
    enlace.remove()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    console.error('No se pudo descargar el reporte:', e)
    toast.error('No se pudo descargar el reporte. Intenta nuevamente.')
  } finally {
    descargandoReporte.value = false
  }
}

function seleccionarExcel(event: Event) {
  const objetivo = event.target as HTMLInputElement
  archivoExcel.value = objetivo.files?.[0] ?? null
}

async function importarExcel() {
  if (!archivoExcel.value) {
    return
  }

  subiendoExcel.value = true
  mensaje.value = ''

  try {
    const formData = new FormData()
    formData.append('archivo', archivoExcel.value)

    const { data } = await axios.post<ResultadoImportacion>(
      `${API_BASE}/api/impresoras/importar-excel`,
      formData,
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    mensaje.value = `Importación completada: ${data.registrosImportados} registros guardados y ${data.impresorasCreadas} impresoras nuevas.`

    if (inputExcel.value) {
      inputExcel.value.value = ''
    }

    archivoExcel.value = null
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    if (axios.isAxiosError(e) && typeof e.response?.data === 'string') {
      mensaje.value = e.response.data
    } else {
      console.error('No se pudo importar el Excel:', e)
      mensaje.value = 'No se pudo importar el archivo. Intenta nuevamente.'
    }
  } finally {
    subiendoExcel.value = false
  }
}

async function cargarGrafica() {
  if (!tieneFechas.value) {
    errorGrafica.value = 'Selecciona la fecha de inicio y la fecha de fin.'
    return
  }

  cargandoGrafica.value = true
  errorGrafica.value = ''

  try {
    const headers = {
      Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
    }

    const [grafica, datos] = await Promise.all([
      axios.get<DatoGrafica[]>(`${API_BASE}/api/reportes/grafica-consumo`, {
        params: {
          fechaInicio: fechaInicio.value,
          fechaFin: fechaFin.value,
        },
        headers,
      }),
      axios.get<RegistroTabla[]>(`${API_BASE}/api/reportes/datos-consumo`, {
        params: {
          fechaInicio: fechaInicio.value,
          fechaFin: fechaFin.value,
        },
        headers,
      }),
    ])

    datosGrafica.value = grafica.data
    registrosTabla.value = datos.data.map((registro) => ({
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

function formatearFecha(iso: string): string {
  const partes = iso.slice(0, 10).split('-')

  if (partes.length !== 3) {
    return iso
  }

  return `${partes[2]}/${partes[1]}/${partes[0]}`
}

async function guardarObservacion(registro: RegistroTabla) {
  const texto = (registro.observacion ?? '').trim()

  if (texto === (ultimaObservacion.value[registro.id] ?? '')) {
    return
  }

  if (guardandoObsId.value !== null) {
    return
  }

  guardandoObsId.value = registro.id

  try {
    await axios.put(
      `${API_BASE}/api/reportes/consumo/${registro.id}/observacion`,
      { observacion: texto },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    ultimaObservacion.value[registro.id] = texto
    registro.observacion = texto
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    console.error('No se pudo guardar la observación:', e)
  } finally {
    guardandoObsId.value = null
  }
}

const chartData = computed<ChartData<'bar'>>(() => ({
  labels: datosGrafica.value.map((dato) => dato.impresora),
datasets: [
      {
        label: 'Páginas impresas',
        data: datosGrafica.value.map((dato) => dato.total),
      backgroundColor: '#1f3a68',
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed<ChartOptions<'bar'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false,
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      title: {
        display: true,
        text: 'Páginas impresas',
      },
    },
  },
}))

onMounted(() => {
  const hoy = new Date()
  const primeroDelMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1)

  fechaInicio.value = fechasComoDato(primeroDelMes)
  fechaFin.value = fechasComoDato(hoy)

  cargarGrafica()
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <div>
        <h1 class="vista__titulo">Consumo y Métricas CENDI</h1>
        <p class="vista__subtitulo">Histórico de consumo por impresora y reporte general del CENDI</p>
      </div>
      <button
        class="boton boton--primario"
        type="button"
        :disabled="descargandoReporte"
        @click="descargarReporte"
      >
        {{ descargandoReporte ? 'Generando...' : 'Descargar Reporte CENDI' }}
      </button>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Periodo de consulta</h2>
      </header>
      <div class="filtros">
        <div class="campo">
          <label class="campo__etiqueta" for="fecha-inicio">Fecha Inicio</label>
          <input
            id="fecha-inicio"
            v-model="fechaInicio"
            class="campo__input"
            type="date"
          />
        </div>
        <div class="campo">
          <label class="campo__etiqueta" for="fecha-fin">Fecha Fin</label>
          <input
            id="fecha-fin"
            v-model="fechaFin"
            class="campo__input"
            type="date"
          />
        </div>
        <button
          class="boton boton--secundario"
          type="button"
          :disabled="!tieneFechas || cargandoGrafica"
          @click="cargarGrafica"
        >
          {{ cargandoGrafica ? 'Cargando...' : 'Generar Gráfica' }}
        </button>
      </div>
    </section>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Comparativa de consumo por impresora</h2>
        <div class="toggle">
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
            Tabla
          </button>
        </div>
      </header>

      <p v-if="errorGrafica" class="alerta">{{ errorGrafica }}</p>

      <div v-if="vistaActual === 'grafica'">
        <div v-if="cargandoGrafica" class="estado-vacio">
          <p class="estado-vacio__texto">Cargando gráfica...</p>
        </div>

        <div v-else-if="datosGrafica.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay registros de consumo en el periodo seleccionado.</p>
        </div>

        <div v-else class="grafica__envoltura">
          <Bar :data="chartData" :options="chartOptions" />
        </div>
      </div>

      <div v-else-if="vistaActual === 'tabla'">
        <div v-if="cargandoGrafica" class="estado-vacio">
          <p class="estado-vacio__texto">Cargando datos...</p>
        </div>

        <div v-else-if="registrosTabla.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay registros de consumo en el periodo seleccionado.</p>
        </div>

        <div v-else class="tabla__envoltura">
          <table class="tabla">
            <thead>
              <tr>
                <th class="tabla__th">Fecha</th>
                <th class="tabla__th">Impresora</th>
                <th class="tabla__th">Contador</th>
                <th class="tabla__th">Consumo</th>
                <th class="tabla__th">Observación</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="registro in registrosTabla"
                :key="registro.id"
                class="tabla__fila"
              >
                <td class="tabla__td">{{ formatearFecha(registro.fecha) }}</td>
                <td class="tabla__td">{{ registro.impresora }}</td>
                <td class="tabla__td">{{ registro.contador.toLocaleString() }}</td>
                <td class="tabla__td">{{ registro.consumo.toLocaleString() }}</td>
                <td class="tabla__td">
                  <input
                    class="campo__input campo__input--observacion"
                    type="text"
                    v-model="registro.observacion"
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

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Importar Excel de Gerencia</h2>
      </header>
      <div class="importar">
        <input
          ref="inputExcel"
          class="importar__archivo"
          type="file"
          accept=".xlsx,.xls"
          :disabled="subiendoExcel"
          @change="seleccionarExcel"
        />
        <button
          class="boton boton--primario"
          type="button"
          :disabled="!archivoExcel || subiendoExcel"
          @click="importarExcel"
        >
          {{ subiendoExcel ? 'Importando...' : 'Importar Excel' }}
        </button>
      </div>
      <p v-if="mensaje" class="nota">{{ mensaje }}</p>
    </section>
  </div>
</template>

<style scoped>
.vista__encabezado {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
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
  margin-bottom: 20px;
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

.boton--primario {
  color: #ffffff;
  background: #1f3a68;
}

.boton--primario:hover:not(:disabled) {
  background: #16294a;
}

.boton--secundario {
  color: #1f3a68;
  background: #ffffff;
  border-color: #d0d5dd;
}

.boton--secundario:hover:not(:disabled) {
  background: #f8fafc;
}

.boton:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

.filtros {
  display: flex;
  align-items: flex-end;
  flex-wrap: wrap;
  gap: 14px;
  padding: 16px 20px;
}

.filtros .campo {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.campo__etiqueta {
  font-size: 12px;
  font-weight: 500;
  color: #475467;
}

.campo__input {
  padding: 8px 12px;
  font-size: 13px;
  font-family: inherit;
  color: #1a1f2b;
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 6px;
  box-sizing: border-box;
}

.campo__input:focus {
  outline: none;
  border-color: #1f3a68;
  box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12);
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

.grafica__envoltura {
  position: relative;
  height: 360px;
  padding: 20px;
  box-sizing: border-box;
}

.importar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 14px;
  padding: 16px 20px;
}

.importar__archivo {
  font-size: 13px;
  font-family: inherit;
}

.nota {
  margin: 0 20px 16px;
  padding: 8px 10px;
  font-size: 12px;
  color: #067647;
  background: #ecfdf3;
  border: 1px solid #abefc6;
  border-radius: 6px;
}

.toggle {
  display: inline-flex;
  padding: 3px;
  background: #f2f4f7;
  border: 1px solid #e2e5ea;
  border-radius: 8px;
}

.toggle__boton {
  padding: 6px 16px;
  font-size: 13px;
  font-weight: 500;
  font-family: inherit;
  color: #475467;
  background: transparent;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.toggle__boton:hover {
  color: #1f3a68;
}

.toggle__boton--activo {
  color: #ffffff;
  background: #1f3a68;
}

.toggle__boton--activo:hover {
  color: #ffffff;
}

.tabla__envoltura {
  padding: 8px 20px 20px;
  overflow-x: auto;
}

.tabla {
  width: 100%;
  border-collapse: collapse;
}

.tabla__th {
  padding: 11px 12px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.6px;
  text-align: left;
  text-transform: uppercase;
  color: #475467;
  background: #f9fafb;
  border-bottom: 1px solid #e2e5ea;
}

.tabla__fila:hover {
  background: #f9fafb;
}

.tabla__td {
  padding: 10px 12px;
  font-size: 13px;
  color: #344054;
  border-bottom: 1px solid #f2f4f7;
}

.tabla__fila:last-child .tabla__td {
  border-bottom: none;
}

.campo__input--observacion {
  width: 100%;
  min-width: 180px;
}
</style>