<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { jsPDF } from 'jspdf'
import autoTable from 'jspdf-autotable'
import { Bar, Doughnut } from 'vue-chartjs'
import {
  Chart as ChartJS,
  ArcElement,
  BarElement,
  CategoryScale,
  LinearScale,
  Legend,
  Title,
  Tooltip,
} from 'chart.js'
import { toast } from 'vue3-toastify'
import { confirmar } from '../composables/useConfirm'
import { API_BASE_URL } from '../config'
import {
  obtenerCategorias,
  obtenerDepartamentos,
  prioridadNumerica,
  type CategoriaTicket,
  type Departamento,
} from '../services/catalogos'

ChartJS.register(ArcElement, BarElement, CategoryScale, LinearScale, Legend, Title, Tooltip)

const API_URL = API_BASE_URL + '/api/Tickets'
const TOKEN_KEY = 'sgcti_token'
const MUTE_GLOBAL_KEY = 'sgcti_tickets_mute_global'
const MUTE_TICKETS_KEY = 'sgcti_tickets_silenciados'

const ESTADOS = ['Abierto', 'En Progreso', 'Resuelto'] as const
const PRIORIDADES = ['Baja', 'Media', 'Alta', 'Crítica'] as const
const SUMINISTROS_SUGERIDOS = ['Ninguno', 'Mouse Óptico USB', 'Cable de poder', 'Tóner HP 85A'] as const

const INTERVALOS_ALERTA_MS: Record<string, number> = {
  critica: 1 * 60 * 1000,
  alta: 3 * 60 * 1000,
  media: 10 * 60 * 1000,
  baja: 30 * 60 * 1000,
}

const PESO_PRIORIDAD: Record<string, number> = {
  baja: 1,
  media: 2,
  alta: 3,
  critica: 4,
}

type ColumnaOrden = 'codigoNomenclatura' | 'titulo' | 'solicitante' | 'departamento' | 'categoria' | 'estado' | 'prioridad'

interface Ticket {
  id: number
  titulo: string
  descripcion?: string
  estado: number | string
  prioridad: number | string
  fechaCreacion?: string
  fechaCierre?: string | null
  slaHoras?: number
  solicitante?: string | null
  telegramChatId?: string | null
  codigoNomenclatura?: string | null
  departamento?: string | null
  categoria?: string | null
  equipoAfectado?: string | null
  solucionAplicada?: string | null
  suministrosUtilizados?: string | null
  fechaResolucion?: string | null
}

interface Fila {
  id: number
  codigoNomenclatura: string
  titulo: string
  solicitante: string
  estado: string
  estadoClase: string
  prioridad: string
  prioridadClase: string
  descripcion: string
  departamento: string
  categoria: string
  equipoAfectado: string
  solucionAplicada: string
  suministrosUtilizados: string
  fechaCreacion: string
  fechaCierre: string | null
  fechaResolucion: string | null
}

interface EmpleadoDirectorio {
  id: number
  nombreCompleto: string
  cargo?: string | null
  departamento?: string | null
  prioridadPersonalizada?: string | null
}

interface ConteoEstadistica {
  etiqueta: string
  total: number
}

interface EquipoRecurrente {
  equipo: string
  total: number
  resueltos: number
  abiertos: number
  ultimaIncidencia: string | null
  soluciones: string[]
}

interface EstadisticasTickets {
  fechaInicio: string | null
  fechaFin: string | null
  resumen: { total: number; resueltos: number; abiertos: number; enProgreso: number }
  porDepartamento: ConteoEstadistica[]
  porCategoria: ConteoEstadistica[]
  equiposRecurrentes: EquipoRecurrente[]
}

const router = useRouter()

const filas = ref<Fila[]>([])
const cargando = ref(true)
const error = ref('')

const departamentos = ref<Departamento[]>([])
const categorias = ref<CategoriaTicket[]>([])
const directorioEmpleados = ref<EmpleadoDirectorio[]>([])

const pestanaActiva = ref<'activos' | 'historico' | 'estadisticas'>('activos')
const busquedaActivos = ref('')
const busquedaHistorico = ref('')

const columnaOrden = ref<ColumnaOrden | null>(null)
const direccionOrden = ref<'asc' | 'desc'>('asc')

function ordenarPor(columna: ColumnaOrden) {
  if (columnaOrden.value === columna) {
    direccionOrden.value = direccionOrden.value === 'asc' ? 'desc' : 'asc'
  } else {
    columnaOrden.value = columna
    direccionOrden.value = 'asc'
  }
}

function iconoOrden(columna: ColumnaOrden): string {
  if (columnaOrden.value !== columna) return '↕'
  return direccionOrden.value === 'asc' ? '↑' : '↓'
}

const sonidoSilenciadoGlobal = ref(localStorage.getItem(MUTE_GLOBAL_KEY) === 'true')
const ticketsSilenciados = ref<number[]>(cargarTicketsSilenciados())
const ultimaAlertaPorTicket = reactive<Record<number, number>>({})
const idsConocidos = new Set<number>()

function cargarTicketsSilenciados(): number[] {
  try {
    const guardado = localStorage.getItem(MUTE_TICKETS_KEY)
    return guardado ? (JSON.parse(guardado) as number[]) : []
  } catch {
    return []
  }
}

function guardarTicketsSilenciados() {
  localStorage.setItem(MUTE_TICKETS_KEY, JSON.stringify(ticketsSilenciados.value))
}

function alternarSilencioGlobal() {
  sonidoSilenciadoGlobal.value = !sonidoSilenciadoGlobal.value
  localStorage.setItem(MUTE_GLOBAL_KEY, String(sonidoSilenciadoGlobal.value))
  if (!sonidoSilenciadoGlobal.value) {
    reproducirSonidoAlerta('normal')
    toast.info('Alertas sonoras activadas.')
  } else {
    toast.info('Alertas sonoras silenciadas.')
  }
}

function estaTicketSilenciado(id: number): boolean {
  return ticketsSilenciados.value.includes(id)
}

function alternarSilencioTicket(fila: Fila) {
  const idx = ticketsSilenciados.value.indexOf(fila.id)
  if (idx >= 0) {
    ticketsSilenciados.value.splice(idx, 1)
    ultimaAlertaPorTicket[fila.id] = Date.now()
    toast.info(`Recordatorios reactivados para ${fila.codigoNomenclatura || '#' + fila.id}.`)
  } else {
    ticketsSilenciados.value.push(fila.id)
    toast.info(`Notificaciones silenciadas para ${fila.codigoNomenclatura || '#' + fila.id} (en atención).`)
  }
  guardarTicketsSilenciados()
}

function reproducirSonidoAlerta(tipo: 'critica' | 'normal' = 'normal') {
  if (sonidoSilenciadoGlobal.value) return

  try {
    const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext
    const ctx = new AudioCtx()
    const ahora = ctx.currentTime

    const tocarTono = (frecuencia: number, inicio: number, duracion: number) => {
      const osc = ctx.createOscillator()
      const gain = ctx.createGain()
      osc.type = tipo === 'critica' ? 'sawtooth' : 'sine'
      osc.frequency.setValueAtTime(frecuencia, inicio)
      gain.gain.setValueAtTime(0.18, inicio)
      gain.gain.exponentialRampToValueAtTime(0.001, inicio + duracion)
      osc.connect(gain)
      gain.connect(ctx.destination)
      osc.start(inicio)
      osc.stop(inicio + duracion)
    }

    if (tipo === 'critica') {
      tocarTono(880, ahora, 0.18)
      tocarTono(1046.5, ahora + 0.22, 0.18)
      tocarTono(880, ahora + 0.44, 0.25)
    } else {
      tocarTono(587.33, ahora, 0.16)
      tocarTono(880, ahora + 0.18, 0.24)
    }
  } catch {
    // Bloqueado si no hay interacción previa
  }
}

async function registrarAlertaEnCampanita(fila: Fila) {
  try {
    await axios.post(`${API_URL}/${fila.id}/recordatorio`, null, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })
  } catch {
    // Si el endpoint no está activo, continúa sin interrumpir
  }
}

function verificarRecordatoriosPendientes() {
  if (sonidoSilenciadoGlobal.value) return

  const ahora = Date.now()

  for (const fila of ticketsActivos.value) {
    if (estaTicketSilenciado(fila.id) || fila.estadoClase === 'en-progreso') {
      continue
    }

    const intervaloMs = INTERVALOS_ALERTA_MS[fila.prioridadClase] ?? INTERVALOS_ALERTA_MS.media!
    const baseTiempo =
      ultimaAlertaPorTicket[fila.id] ??
      (fila.fechaCreacion ? new Date(fila.fechaCreacion).getTime() : ahora)

    if (ahora - baseTiempo >= intervaloMs) {
      ultimaAlertaPorTicket[fila.id] = ahora
      const esUrgente = fila.prioridadClase === 'critica' || fila.prioridadClase === 'alta'
      reproducirSonidoAlerta(esUrgente ? 'critica' : 'normal')
      registrarAlertaEnCampanita(fila)

      const minutosEspera = Math.max(
        1,
        Math.round((ahora - (fila.fechaCreacion ? new Date(fila.fechaCreacion).getTime() : ahora)) / 60000),
      )

      toast.warning(
        `⏰ Recordatorio (${fila.prioridad}): Ticket ${fila.codigoNomenclatura || '#' + fila.id} de ${fila.solicitante || 'Empleado'} lleva ${minutosEspera} min esperando atención.`,
        { autoClose: 7000 },
      )
    }
  }
}

const formulario = reactive({
  titulo: '',
  solicitante: '',
  descripcion: '',
  departamento: '',
  categoria: '',
  prioridad: 1,
})

const mostrarModalNuevo = ref(false)
const guardando = ref(false)
const errorFormulario = ref('')

const mostrarModalDetalle = ref(false)
const ticketDetalle = ref<Ticket | null>(null)
const prioridadDetalle = ref(1)
const estadoDetalle = ref(0)
const departamentoDetalle = ref('')
const categoriaDetalle = ref('')
const guardandoDetalle = ref(false)
const errorDetalle = ref('')

const mostrarModalResolucion = ref(false)
const ticketResolucion = ref<Ticket | null>(null)
const guardandoResolucion = ref(false)
const errorResolucion = ref('')
const resolucionForm = reactive({
  equipoAfectado: '',
  solucionAplicada: '',
  suministrosUtilizados: 'Ninguno',
})

const resolviendoId = ref<number | null>(null)
const eliminandoTicketId = ref<number | null>(null)
const errorAccion = ref('')

const desdeEstadisticas = ref('')
const hastaEstadisticas = ref('')
const estadisticas = ref<EstadisticasTickets | null>(null)
const cargandoEstadisticas = ref(false)
const errorEstadisticas = ref('')
const descargandoReporte = ref(false)
const formatoReporte = ref<'excel' | 'pdf' | null>(null)
const graficaDepartamentoRef = ref<HTMLElement | null>(null)
const graficaCategoriaRef = ref<HTMLElement | null>(null)

const coloresGrafica = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#ec4899', '#84cc16']

const chartDepartamentoData = computed(() => ({
  labels: estadisticas.value?.porDepartamento.map((item) => item.etiqueta) ?? [],
  datasets: [
    {
      label: 'Tickets',
      data: estadisticas.value?.porDepartamento.map((item) => item.total) ?? [],
      backgroundColor: '#3b82f6',
      borderRadius: 6,
    },
  ],
}))

const chartCategoriaData = computed(() => ({
  labels: estadisticas.value?.porCategoria.map((item) => item.etiqueta) ?? [],
  datasets: [
    {
      data: estadisticas.value?.porCategoria.map((item) => item.total) ?? [],
      backgroundColor: coloresGrafica,
      borderWidth: 0,
    },
  ],
}))

const chartDepartamentoOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false } },
    y: { beginAtZero: true, ticks: { precision: 0 } },
  },
}

const chartCategoriaOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { position: 'bottom' as const } },
}

const ticketsActivos = computed(() => filas.value.filter((fila) => fila.estadoClase !== 'resuelto'))

const ticketsActivosFiltrados = computed(() => {
  const consulta = busquedaActivos.value.trim().toLowerCase()

  const filtrados = !consulta
    ? [...ticketsActivos.value]
    : ticketsActivos.value.filter((fila) =>
        [
          fila.codigoNomenclatura,
          fila.titulo,
          fila.solicitante,
          fila.departamento,
          fila.categoria,
          fila.estado,
          fila.prioridad,
          fila.descripcion,
        ].some((valor) => valor.toLowerCase().includes(consulta)),
      )

  if (!columnaOrden.value) {
    return filtrados
  }

  const col = columnaOrden.value
  const factor = direccionOrden.value === 'asc' ? 1 : -1

  return filtrados.sort((a, b) => {
    if (col === 'prioridad') {
      const pesoA = PESO_PRIORIDAD[a.prioridadClase] ?? 0
      const pesoB = PESO_PRIORIDAD[b.prioridadClase] ?? 0
      return (pesoA - pesoB) * factor
    }

    const valA = (a[col] || '').toString()
    const valB = (b[col] || '').toString()
    return valA.localeCompare(valB, 'es', { numeric: true, sensitivity: 'base' }) * factor
  })
})

const ticketsCerrados = computed(() => filas.value.filter((fila) => fila.estadoClase === 'resuelto'))

const historicoFiltrado = computed(() => {
  const consulta = busquedaHistorico.value.trim().toLowerCase()

  if (!consulta) return ticketsCerrados.value

  return ticketsCerrados.value.filter((fila) =>
    [
      fila.codigoNomenclatura,
      fila.titulo,
      fila.solicitante,
      fila.descripcion,
      fila.departamento,
      fila.categoria,
      fila.equipoAfectado,
      fila.solucionAplicada,
      fila.suministrosUtilizados,
    ].some((valor) => valor.toLowerCase().includes(consulta)),
  )
})

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

function formatearFecha(valor?: string | null): string {
  if (!valor) return '—'
  const fecha = new Date(valor)
  return Number.isNaN(fecha.getTime()) ? '—' : fecha.toLocaleString()
}

function mapearTickets(data: Ticket[]): Fila[] {
  return data.map((ticket) => {
    const estado = normalizar(ticket.estado, ESTADOS)
    const prioridad = normalizar(ticket.prioridad, PRIORIDADES)

    return {
      id: ticket.id,
      codigoNomenclatura: ticket.codigoNomenclatura ?? '',
      titulo: ticket.titulo,
      solicitante: ticket.solicitante ?? '',
      estado: estado.texto,
      estadoClase: estado.clase,
      prioridad: prioridad.texto,
      prioridadClase: prioridad.clase,
      descripcion: ticket.descripcion ?? '',
      departamento: ticket.departamento ?? '',
      categoria: ticket.categoria ?? '',
      equipoAfectado: ticket.equipoAfectado ?? '',
      solucionAplicada: ticket.solucionAplicada ?? '',
      suministrosUtilizados: ticket.suministrosUtilizados ?? '',
      fechaCreacion: ticket.fechaCreacion ?? '',
      fechaCierre: ticket.fechaCierre ?? null,
      fechaResolucion: ticket.fechaResolucion ?? null,
    }
  })
}

function alCambiarDepartamento() {
  const seleccionado = departamentos.value.find((d) => d.nombre === formulario.departamento)
  if (seleccionado) {
    formulario.prioridad = prioridadNumerica(seleccionado.prioridadPorDefecto)
  }
}

function alCambiarSolicitante() {
  const nombre = formulario.solicitante.trim()
  if (!nombre) return

  const empleado = directorioEmpleados.value.find(
    (registrado) => registrado.nombreCompleto.toLowerCase() === nombre.toLowerCase(),
  )
  if (!empleado) return

  if (empleado.departamento) {
    formulario.departamento = empleado.departamento
  }

  if (empleado.prioridadPersonalizada) {
    formulario.prioridad = prioridadNumerica(empleado.prioridadPersonalizada)
    return
  }

  const departamentoEmpleado = departamentos.value.find((d) => d.nombre === empleado.departamento)
  if (departamentoEmpleado) {
    formulario.prioridad = prioridadNumerica(departamentoEmpleado.prioridadPorDefecto)
  }
}

async function cargarDirectorio() {
  try {
    const { data } = await axios.get<EmpleadoDirectorio[]>(API_BASE_URL + '/api/usuarios/directorio', {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })
    directorioEmpleados.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) cerrarSesion()
  }
}

async function cargarCatalogos() {
  try {
    const [listaDepartamentos, listaCategorias] = await Promise.all([
      obtenerDepartamentos(),
      obtenerCategorias(),
    ])
    departamentos.value = listaDepartamentos
    categorias.value = listaCategorias
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    toast.error('No se pudieron cargar los departamentos y categorías.')
  }
}

function abrirPestana(pestana: 'activos' | 'historico' | 'estadisticas') {
  pestanaActiva.value = pestana
  if (pestana === 'estadisticas' && !estadisticas.value) {
    cargarEstadisticas()
  }
}

async function cargarEstadisticas() {
  cargandoEstadisticas.value = true
  errorEstadisticas.value = ''

  try {
    const { data } = await axios.get<EstadisticasTickets>(`${API_URL}/estadisticas`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
      params: {
        desde: desdeEstadisticas.value || undefined,
        hasta: hastaEstadisticas.value || undefined,
      },
    })
    estadisticas.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorEstadisticas.value = 'No se pudieron cargar las estadísticas. Intenta nuevamente.'
  } finally {
    cargandoEstadisticas.value = false
  }
}

function fechaParaArchivo(): string {
  return new Date().toISOString().slice(0, 10)
}

function periodoReporte(): string {
  const desde = desdeEstadisticas.value
  const hasta = hastaEstadisticas.value
  if (!desde && !hasta) return 'Todos los registros'
  return `${desde || 'Inicio'} a ${hasta || 'Hoy'}`
}

function ticketsDelPeriodo(): Fila[] {
  return filas.value.filter((fila) => {
    if (!fila.fechaCreacion) return true
    const fecha = fila.fechaCreacion.slice(0, 10)
    if (desdeEstadisticas.value && fecha < desdeEstadisticas.value) return false
    if (hastaEstadisticas.value && fecha > hastaEstadisticas.value) return false
    return true
  })
}

function escaparCsv(valor: string | number | null | undefined): string {
  const texto = valor === null || valor === undefined ? '' : String(valor)
  return `"${texto.replace(/"/g, '""')}"`
}

function filaCsv(valores: (string | number | null | undefined)[]): string {
  return valores.map(escaparCsv).join(';')
}

function descargarBlob(blob: Blob, nombre: string) {
  const enlace = document.createElement('a')
  const url = URL.createObjectURL(blob)
  enlace.href = url
  enlace.download = nombre
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()
  URL.revokeObjectURL(url)
}

function descargarExcel() {
  errorEstadisticas.value = ''
  if (!estadisticas.value) {
    errorEstadisticas.value = 'Primero consulta las estadísticas para generar el reporte.'
    return
  }

  descargandoReporte.value = true
  formatoReporte.value = 'excel'

  try {
    const datos = estadisticas.value
    const lineas: string[] = []
    const agregar = (...valores: (string | number | null | undefined)[]) => lineas.push(filaCsv(valores))
    const vacia = () => lineas.push('')

    agregar('REPORTE DE ESTADÍSTICAS E INCIDENCIAS')
    agregar('Generado', new Date().toLocaleString('es-MX'))
    agregar('Periodo', periodoReporte())
    vacia()
    agregar('RESUMEN GENERAL')
    agregar('Total de tickets', 'Resueltos', 'Abiertos', 'En progreso')
    agregar(datos.resumen.total, datos.resumen.resueltos, datos.resumen.abiertos, datos.resumen.enProgreso)
    vacia()
    agregar('INCIDENCIAS POR DEPARTAMENTO / ÁREA')
    agregar('Departamento / Área', 'Tickets')
    datos.porDepartamento.forEach((item) => agregar(item.etiqueta, item.total))
    vacia()
    agregar('INCIDENCIAS POR CATEGORÍA')
    agregar('Categoría', 'Tickets')
    datos.porCategoria.forEach((item) => agregar(item.etiqueta, item.total))
    vacia()
    agregar('EQUIPOS CON MAYOR RECURRENCIA DE FALLAS')
    agregar('Equipo / Periférico', 'Incidencias', 'Resueltas', 'Abiertas', 'Última incidencia', 'Soluciones')
    datos.equiposRecurrentes.forEach((item) =>
      agregar(item.equipo, item.total, item.resueltos, item.abiertos, formatearFecha(item.ultimaIncidencia), item.soluciones.join(' | ')),
    )
    vacia()
    agregar('LISTADO DE TICKETS')
    agregar('Código', 'Título', 'Solicitante', 'Departamento', 'Categoría', 'Prioridad', 'Estado', 'Fecha de creación', 'Fecha de resolución', 'Equipo afectado')
    ticketsDelPeriodo().forEach((fila) =>
      agregar(
        fila.codigoNomenclatura,
        fila.titulo,
        fila.solicitante,
        fila.departamento,
        fila.categoria,
        fila.prioridad,
        fila.estado,
        formatearFecha(fila.fechaCreacion),
        formatearFecha(fila.fechaResolucion),
        fila.equipoAfectado,
      ),
    )

    const contenido = '\uFEFF' + lineas.join('\r\n')
    descargarBlob(new Blob([contenido], { type: 'text/csv;charset=utf-8;' }), `Reporte_Incidencias_${fechaParaArchivo()}.csv`)
    toast.success('Reporte Excel (CSV) generado correctamente.')
  } catch {
    errorEstadisticas.value = 'No se pudo generar el reporte Excel. Intenta nuevamente.'
  } finally {
    descargandoReporte.value = false
    formatoReporte.value = null
  }
}

function capturarGrafica(elemento: HTMLElement | null): string | null {
  const canvas = elemento?.querySelector('canvas')
  if (!canvas || canvas.width === 0 || canvas.height === 0) return null

  const copia = document.createElement('canvas')
  copia.width = canvas.width
  copia.height = canvas.height
  const contexto = copia.getContext('2d')
  if (!contexto) return canvas.toDataURL('image/png')

  contexto.fillStyle = '#ffffff'
  contexto.fillRect(0, 0, copia.width, copia.height)
  contexto.drawImage(canvas, 0, 0)
  return copia.toDataURL('image/png')
}

function obtenerFinalY(doc: jsPDF, respaldo: number): number {
  const info = (doc as unknown as { lastAutoTable?: { finalY?: number } }).lastAutoTable
  return info?.finalY ?? respaldo
}

function descargarPdf() {
  errorEstadisticas.value = ''
  if (!estadisticas.value) {
    errorEstadisticas.value = 'Primero consulta las estadísticas para generar el reporte.'
    return
  }

  descargandoReporte.value = true
  formatoReporte.value = 'pdf'

  try {
    const datos = estadisticas.value
    const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' })
    const anchoPagina = doc.internal.pageSize.getWidth()
    const altoPagina = doc.internal.pageSize.getHeight()
    const margen = 12
    const anchoUtil = anchoPagina - margen * 2
    let y = 0

    const nuevaPagina = () => {
      doc.addPage('a4', 'landscape')
      y = margen
    }

    const asegurar = (altura: number) => {
      if (y + altura > altoPagina - margen - 6) nuevaPagina()
    }

    const tituloSeccion = (texto: string) => {
      asegurar(12)
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(11)
      doc.setTextColor(31, 58, 104)
      doc.text(texto, margen, y)
      y += 5
    }

    const agregarTabla = (titulo: string, cabecera: string[], cuerpo: (string | number)[][]) => {
      tituloSeccion(titulo)
      autoTable(doc, {
        startY: y,
        head: [cabecera],
        body: cuerpo.length > 0 ? cuerpo : [['Sin datos en el periodo seleccionado']],
        theme: 'grid',
        margin: { left: margen, right: margen, bottom: margen + 6 },
        styles: { font: 'helvetica', fontSize: 8, cellPadding: 2, textColor: [51, 65, 85], overflow: 'linebreak' },
        headStyles: { fillColor: [31, 58, 104], textColor: [255, 255, 255], fontStyle: 'bold' },
        alternateRowStyles: { fillColor: [248, 250, 252] },
      })
      y = obtenerFinalY(doc, y) + 8
    }

    doc.setFillColor(31, 58, 104)
    doc.rect(0, 0, anchoPagina, 22, 'F')
    doc.setTextColor(255, 255, 255)
    doc.setFont('helvetica', 'bold')
    doc.setFontSize(16)
    doc.text('Reporte de Estadísticas e Incidencias', margen, 10)
    doc.setFont('helvetica', 'normal')
    doc.setFontSize(9)
    doc.text(`Generado: ${new Date().toLocaleString('es-MX')}`, margen, 16)
    doc.text(`Periodo: ${periodoReporte()}`, anchoPagina - margen, 16, { align: 'right' })
    y = 30

    const tarjetas = [
      { valor: datos.resumen.total, etiqueta: 'Total de tickets', color: [59, 130, 246] },
      { valor: datos.resumen.resueltos, etiqueta: 'Resueltos', color: [16, 185, 129] },
      { valor: datos.resumen.abiertos, etiqueta: 'Abiertos', color: [239, 68, 68] },
      { valor: datos.resumen.enProgreso, etiqueta: 'En progreso', color: [245, 158, 11] },
    ]
    const separacion = 4
    const anchoTarjeta = (anchoUtil - separacion * (tarjetas.length - 1)) / tarjetas.length

    tarjetas.forEach((tarjeta, indice) => {
      const x = margen + indice * (anchoTarjeta + separacion)
      doc.setFillColor(248, 250, 252)
      doc.setDrawColor(226, 232, 240)
      doc.roundedRect(x, y, anchoTarjeta, 20, 2, 2, 'FD')
      doc.setFillColor(tarjeta.color[0]!, tarjeta.color[1]!, tarjeta.color[2]!)
      doc.roundedRect(x, y, 2.5, 20, 1, 1, 'F')
      doc.setTextColor(31, 58, 104)
      doc.setFont('helvetica', 'bold')
      doc.setFontSize(15)
      doc.text(String(tarjeta.valor), x + 6, y + 9)
      doc.setTextColor(100, 116, 139)
      doc.setFont('helvetica', 'normal')
      doc.setFontSize(8)
      doc.text(tarjeta.etiqueta, x + 6, y + 15)
    })
    y += 28

    const imagenes = [
      { titulo: 'Demanda por Departamento / Área de Servicio', imagen: capturarGrafica(graficaDepartamentoRef.value) },
      { titulo: 'Incidencias por Categoría', imagen: capturarGrafica(graficaCategoriaRef.value) },
    ].filter((grafica) => grafica.imagen !== null)

    if (imagenes.length > 0) {
      const altoMaximo = 60
      asegurar(altoMaximo + 12)
      const yInicio = y
      const anchoGrafica = (anchoUtil - 6 * (imagenes.length - 1)) / imagenes.length

      imagenes.forEach((grafica, indice) => {
        const x = margen + indice * (anchoGrafica + 6)
        doc.setFont('helvetica', 'bold')
        doc.setFontSize(9)
        doc.setTextColor(31, 58, 104)
        doc.text(grafica.titulo, x, yInicio)

        const imagen = grafica.imagen as string
        const propiedades = doc.getImageProperties(imagen)
        const altura = Math.min(altoMaximo, (anchoGrafica * propiedades.height) / propiedades.width)
        doc.addImage(imagen, 'PNG', x, yInicio + 3, anchoGrafica, altura)
      })
      y = yInicio + altoMaximo + 12
    }

    agregarTabla('Incidencias por Departamento / Área de Servicio', ['Departamento / Área', 'Tickets'], datos.porDepartamento.map((item) => [item.etiqueta, item.total]))
    agregarTabla('Incidencias por Categoría', ['Categoría', 'Tickets'], datos.porCategoria.map((item) => [item.etiqueta, item.total]))
    agregarTabla(
      'Equipos con Mayor Recurrencia de Fallas',
      ['Equipo / Periférico', 'Incidencias', 'Resueltas', 'Abiertas', 'Última incidencia', 'Soluciones'],
      datos.equiposRecurrentes.map((item) => [
        item.equipo,
        item.total,
        item.resueltos,
        item.abiertos,
        formatearFecha(item.ultimaIncidencia),
        item.soluciones.length > 0 ? item.soluciones.join(' | ') : 'Sin registrar',
      ]),
    )
    agregarTabla(
      'Listado de Tickets',
      ['Código', 'Título', 'Solicitante', 'Departamento', 'Categoría', 'Prioridad', 'Estado', 'Fecha'],
      ticketsDelPeriodo().map((fila) => [
        fila.codigoNomenclatura || '—',
        fila.titulo,
        fila.solicitante || '—',
        fila.departamento || '—',
        fila.categoria || '—',
        fila.prioridad,
        fila.estado,
        formatearFecha(fila.fechaCreacion),
      ]),
    )

    const totalPaginas = doc.getNumberOfPages()
    for (let pagina = 1; pagina <= totalPaginas; pagina++) {
      doc.setPage(pagina)
      doc.setFont('helvetica', 'normal')
      doc.setFontSize(7)
      doc.setTextColor(148, 163, 184)
      doc.text('SGCTI · Sistema de Control de Suministros y Gestión de los Servicios TI', margen, altoPagina - 4)
      doc.text(`Página ${pagina} de ${totalPaginas}`, anchoPagina - margen, altoPagina - 4, { align: 'right' })
    }

    doc.save(`Reporte_Incidencias_${fechaParaArchivo()}.pdf`)
    toast.success('Reporte PDF generado correctamente.')
  } catch {
    errorEstadisticas.value = 'No se pudo generar el reporte PDF. Intenta nuevamente.'
  } finally {
    descargandoReporte.value = false
    formatoReporte.value = null
  }
}

async function cargarTickets() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Ticket[]>(API_URL, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    const mapeados = mapearTickets(data)
    filas.value = mapeados

    const ahora = Date.now()
    mapeados.forEach((f) => {
      idsConocidos.add(f.id)
      if (!ultimaAlertaPorTicket[f.id]) {
        ultimaAlertaPorTicket[f.id] = ahora
      }
    })
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    error.value = 'No se pudieron cargar los tickets. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

async function eliminarTicket(fila: Fila) {
  errorAccion.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar el ticket "${fila.codigoNomenclatura || '#' + fila.id} - ${fila.titulo}"? Esta acción eliminará el mensaje permanentemente.`,
    confirmarTexto: 'Eliminar Ticket',
  })
  if (!confirmado) return

  eliminandoTicketId.value = fila.id

  try {
    await axios.delete(`${API_URL}/${fila.id}`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    filas.value = filas.value.filter((item) => item.id !== fila.id)
    idsConocidos.delete(fila.id)
    delete ultimaAlertaPorTicket[fila.id]

    if (estadisticas.value) cargarEstadisticas()
    toast.success('Ticket eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorAccion.value = 'No se pudo eliminar el ticket. Intenta nuevamente.'
  } finally {
    eliminandoTicketId.value = null
  }
}

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

function abrirModalNuevo() {
  formulario.titulo = ''
  formulario.solicitante = ''
  formulario.descripcion = ''
  formulario.departamento = ''
  formulario.categoria = ''
  formulario.prioridad = 1
  errorFormulario.value = ''
  mostrarModalNuevo.value = true

  if (departamentos.value.length === 0 || categorias.value.length === 0) cargarCatalogos()
  if (directorioEmpleados.value.length === 0) cargarDirectorio()
}

function cerrarModalNuevo() {
  mostrarModalNuevo.value = false
  errorFormulario.value = ''
}

async function crearTicket() {
  errorFormulario.value = ''
  if (!formulario.titulo.trim()) {
    errorFormulario.value = 'El título es obligatorio.'
    return
  }

  guardando.value = true

  try {
    await axios.post(
      API_URL,
      {
        Titulo: formulario.titulo.trim(),
        Descripcion: formulario.descripcion.trim(),
        Solicitante: formulario.solicitante.trim() || null,
        Estado: 0,
        Prioridad: formulario.prioridad,
        Departamento: formulario.departamento || null,
        Categoria: formulario.categoria || null,
        SLAHoras: 24,
        FechaCreacion: new Date().toISOString(),
      },
      { headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) } },
    )

    formulario.titulo = ''
    formulario.descripcion = ''
    mostrarModalNuevo.value = false

    await cargarTickets()
    toast.success('Ticket creado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorFormulario.value = 'No se pudo guardar el ticket. Intenta nuevamente.'
  } finally {
    guardando.value = false
  }
}

async function abrirModalResolucion(fila: Fila) {
  errorAccion.value = ''
  errorResolucion.value = ''
  resolviendoId.value = fila.id

  try {
    const { data } = await axios.get<Ticket>(`${API_URL}/${fila.id}`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    ticketResolucion.value = data
    resolucionForm.equipoAfectado = data.equipoAfectado ?? ''
    resolucionForm.solucionAplicada = ''
    resolucionForm.suministrosUtilizados = data.suministrosUtilizados || 'Ninguno'
    mostrarModalResolucion.value = true
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorAccion.value = 'No se pudo abrir el formulario de resolución.'
  } finally {
    resolviendoId.value = null
  }
}

function cerrarModalResolucion() {
  mostrarModalResolucion.value = false
  ticketResolucion.value = null
  errorResolucion.value = ''
}

async function confirmarResolucion() {
  if (!ticketResolucion.value) return
  errorResolucion.value = ''

  if (!resolucionForm.solucionAplicada.trim()) {
    errorResolucion.value = 'La solución aplicada es obligatoria.'
    return
  }

  guardandoResolucion.value = true
  const ahora = new Date().toISOString()

  try {
    await axios.put(
      `${API_URL}/${ticketResolucion.value.id}`,
      {
        ...ticketResolucion.value,
        estado: 2,
        fechaCierre: ticketResolucion.value.fechaCierre ?? ahora,
        fechaResolucion: ahora,
        equipoAfectado: resolucionForm.equipoAfectado.trim() || null,
        solucionAplicada: resolucionForm.solucionAplicada.trim(),
        suministrosUtilizados: resolucionForm.suministrosUtilizados.trim() || 'Ninguno',
      },
      { headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) } },
    )

    toast.success('Ticket resuelto y registrado en el histórico.')
    cerrarModalResolucion()
    await cargarTickets()
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorResolucion.value = 'No se pudo registrar la solución. Intenta nuevamente.'
  } finally {
    guardandoResolucion.value = false
  }
}

async function abrirDetalles(fila: Fila) {
  errorDetalle.value = ''

  try {
    const { data } = await axios.get<Ticket>(`${API_URL}/${fila.id}`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    ticketDetalle.value = data
    departamentoDetalle.value = data.departamento ?? ''
    categoriaDetalle.value = data.categoria ?? ''

    estadoDetalle.value =
      typeof data.estado === 'number'
        ? data.estado
        : Math.max(0, ESTADOS.findIndex((e) => e.toLowerCase() === String(data.estado).toLowerCase()))

    prioridadDetalle.value =
      typeof data.prioridad === 'number'
        ? data.prioridad
        : Math.max(0, PRIORIDADES.findIndex((p) => p.toLowerCase() === String(data.prioridad).toLowerCase()))

    mostrarModalDetalle.value = true
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorAccion.value = 'No se pudo cargar el detalle del ticket.'
  }
}

function cerrarDetalles() {
  mostrarModalDetalle.value = false
  ticketDetalle.value = null
  errorDetalle.value = ''
}

async function guardarDetalle() {
  if (!ticketDetalle.value) return
  guardandoDetalle.value = true
  errorDetalle.value = ''

  try {
    await axios.put(
      `${API_URL}/${ticketDetalle.value.id}`,
      {
        ...ticketDetalle.value,
        estado: estadoDetalle.value,
        departamento: departamentoDetalle.value || null,
        categoria: categoriaDetalle.value || null,
        prioridad: prioridadDetalle.value,
      },
      { headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) } },
    )

    toast.success('Ticket actualizado correctamente.')
    cerrarDetalles()
    await cargarTickets()
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }
    errorDetalle.value = 'No se pudieron guardar los cambios. Intenta nuevamente.'
  } finally {
    guardandoDetalle.value = false
  }
}

let intervaloActualizacion: ReturnType<typeof setInterval> | null = null

async function sincronizarTicketsEnSegundoPlano() {
  try {
    const { data } = await axios.get<Ticket[]>(API_URL, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    const nuevosMapeados = mapearTickets(data)
    const ahora = Date.now()
    let llegoTicketNuevo = false
    let esUrgenteNuevo = false

    for (const fila of nuevosMapeados) {
      if (!idsConocidos.has(fila.id)) {
        idsConocidos.add(fila.id)
        ultimaAlertaPorTicket[fila.id] = ahora
        if (fila.estadoClase !== 'resuelto') {
          llegoTicketNuevo = true
          if (fila.prioridadClase === 'critica' || fila.prioridadClase === 'alta') {
            esUrgenteNuevo = true
          }
        }
      }
    }

    filas.value = nuevosMapeados

    if (llegoTicketNuevo) {
      reproducirSonidoAlerta(esUrgenteNuevo ? 'critica' : 'normal')
    }

    verificarRecordatoriosPendientes()
  } catch {
    // Silencioso en segundo plano
  }
}

onMounted(() => {
  cargarTickets()
  cargarCatalogos()
  cargarDirectorio()
  intervaloActualizacion = setInterval(sincronizarTicketsEnSegundoPlano, 3000)
})

onBeforeUnmount(() => {
  if (intervaloActualizacion) clearInterval(intervaloActualizacion)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Mesa de Ayuda</h1>
      <p class="vista__subtitulo">Control de Suministros y Gestión de los Servicios TI</p>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <div>
          <h2 class="tarjeta__titulo">Tickets registrados</h2>
          <p class="tarjeta__leyenda-alertas">
            Alertas automáticas: Crítica (1 min) · Alta (3 min) · Media (10 min) · Baja (30 min)
          </p>
        </div>
        <div class="tarjeta__acciones">
          <button
            class="boton"
            :class="sonidoSilenciadoGlobal ? 'boton--silenciado' : 'boton--sonido'"
            type="button"
            @click="alternarSilencioGlobal"
          >
            {{ sonidoSilenciadoGlobal ? '🔕 Alertas Silenciadas' : '🔔 Sonido Activo' }}
          </button>
          <button class="boton boton--primario" type="button" @click="abrirModalNuevo">
            Nuevo Ticket
          </button>
          <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarTickets">
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <p v-if="error" class="alerta">{{ error }}</p>
      <p v-if="errorAccion" class="alerta">{{ errorAccion }}</p>

      <div class="pestanas">
        <button
          class="pestanas__boton"
          :class="{ 'pestanas__boton--activa': pestanaActiva === 'activos' }"
          type="button"
          @click="abrirPestana('activos')"
        >
          Tickets Activos (Cola de Atención)
          <span class="pestanas__contador">{{ ticketsActivos.length }}</span>
        </button>
        <button
          class="pestanas__boton"
          :class="{ 'pestanas__boton--activa': pestanaActiva === 'historico' }"
          type="button"
          @click="abrirPestana('historico')"
        >
          Histórico de Soluciones (Cerrados)
          <span class="pestanas__contador">{{ ticketsCerrados.length }}</span>
        </button>
        <button
          class="pestanas__boton"
          :class="{ 'pestanas__boton--activa': pestanaActiva === 'estadisticas' }"
          type="button"
          @click="abrirPestana('estadisticas')"
        >
          Estadísticas e Incidencias
        </button>
      </div>

      <div v-if="cargando && pestanaActiva !== 'estadisticas'" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando tickets...</p>
      </div>

      <template v-else-if="pestanaActiva === 'activos'">
        <div class="activos__barra-busqueda">
          <input
            v-model="busquedaActivos"
            class="campo__input"
            type="search"
            placeholder="Buscar en tickets activos por código, título, solicitante, departamento, categoría o prioridad..."
            autocomplete="off"
          />
          <span class="activos__conteo">
            {{ ticketsActivosFiltrados.length }} de {{ ticketsActivos.length }}
          </span>
        </div>

        <div v-if="ticketsActivos.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay tickets activos en la cola de atención.</p>
        </div>

        <div v-else-if="ticketsActivosFiltrados.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay tickets activos que coincidan con la búsqueda.</p>
        </div>

        <div v-else class="tabla__envoltura">
          <table class="tabla">
            <thead>
              <tr>
                <th class="tabla__th tabla__th--codigo">
                  <button class="th-sort" type="button" @click="ordenarPor('codigoNomenclatura')">
                    Código <span class="th-sort__icono">{{ iconoOrden('codigoNomenclatura') }}</span>
                  </button>
                </th>
                <th class="tabla__th">
                  <button class="th-sort" type="button" @click="ordenarPor('titulo')">
                    Título <span class="th-sort__icono">{{ iconoOrden('titulo') }}</span>
                  </button>
                </th>
                <th class="tabla__th">
                  <button class="th-sort" type="button" @click="ordenarPor('solicitante')">
                    Solicitante <span class="th-sort__icono">{{ iconoOrden('solicitante') }}</span>
                  </button>
                </th>
                <th class="tabla__th">
                  <button class="th-sort" type="button" @click="ordenarPor('departamento')">
                    Departamento <span class="th-sort__icono">{{ iconoOrden('departamento') }}</span>
                  </button>
                </th>
                <th class="tabla__th">
                  <button class="th-sort" type="button" @click="ordenarPor('categoria')">
                    Categoría <span class="th-sort__icono">{{ iconoOrden('categoria') }}</span>
                  </button>
                </th>
                <th class="tabla__th tabla__th--estado">
                  <button class="th-sort" type="button" @click="ordenarPor('estado')">
                    Estado <span class="th-sort__icono">{{ iconoOrden('estado') }}</span>
                  </button>
                </th>
                <th class="tabla__th tabla__th--prioridad">
                  <button class="th-sort" type="button" @click="ordenarPor('prioridad')">
                    Prioridad <span class="th-sort__icono">{{ iconoOrden('prioridad') }}</span>
                  </button>
                </th>
                <th class="tabla__th tabla__th--acciones">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="fila in ticketsActivosFiltrados" :key="fila.id" class="tabla__fila">
                <td class="tabla__td">
                  <span v-if="fila.codigoNomenclatura" class="badge badge--codigo">
                    {{ fila.codigoNomenclatura }}
                  </span>
                  <span v-else>#{{ fila.id }}</span>
                </td>
                <td class="tabla__td tabla__td--titulo">{{ fila.titulo }}</td>
                <td class="tabla__td">{{ fila.solicitante || '—' }}</td>
                <td class="tabla__td">{{ fila.departamento || '—' }}</td>
                <td class="tabla__td">{{ fila.categoria || '—' }}</td>
                <td class="tabla__td">
                  <span class="badge" :class="`badge--${fila.estadoClase}`">{{ fila.estado }}</span>
                </td>
                <td class="tabla__td">
                  <span class="badge" :class="`badge--${fila.prioridadClase}`">{{ fila.prioridad }}</span>
                </td>
                <td class="tabla__td tabla__td--acciones">
                  <button
                    class="silenciar-tarea"
                    :class="{ 'silenciar-tarea--activo': estaTicketSilenciado(fila.id) }"
                    type="button"
                    :title="
                      estaTicketSilenciado(fila.id)
                        ? 'Reactivar recordatorios de este ticket'
                        : 'Silenciar recordatorios mientras trabajas en este ticket'
                    "
                    @click="alternarSilencioTicket(fila)"
                  >
                    {{ estaTicketSilenciado(fila.id) ? '🔕 Silenciado' : '🔔 Alerta' }}
                  </button>
                  <button class="detalles" type="button" @click="abrirDetalles(fila)">
                    Detalles
                  </button>
                  <button
                    class="resolver"
                    type="button"
                    :disabled="resolviendoId !== null || guardandoResolucion"
                    @click="abrirModalResolucion(fila)"
                  >
                    {{ resolviendoId === fila.id ? 'Cargando...' : 'Resolver' }}
                  </button>
                  <button
                    class="eliminar-ticket"
                    type="button"
                    :disabled="eliminandoTicketId === fila.id"
                    @click="eliminarTicket(fila)"
                  >
                    {{ eliminandoTicketId === fila.id ? 'Borrando...' : 'Eliminar' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>

      <template v-else-if="pestanaActiva === 'historico'">
        <div class="historico__busqueda">
          <input
            v-model="busquedaHistorico"
            class="campo__input"
            type="search"
            placeholder="Buscar por código, solicitante, problema, departamento, categoría, equipo, solución o suministros..."
            autocomplete="off"
          />
        </div>

        <div v-if="ticketsCerrados.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay tickets cerrados en el histórico.</p>
        </div>

        <div v-else-if="historicoFiltrado.length === 0" class="estado-vacio">
          <p class="estado-vacio__texto">No hay coincidencias con la búsqueda.</p>
        </div>

        <div v-else class="historico__lista">
          <article v-for="fila in historicoFiltrado" :key="fila.id" class="historico__tarjeta">
            <header class="historico__cabecera">
              <div>
                <div class="historico__titulo-linea">
                  <span v-if="fila.codigoNomenclatura" class="badge badge--codigo">
                    {{ fila.codigoNomenclatura }}
                  </span>
                  <h3 class="historico__titulo">{{ fila.titulo }}</h3>
                </div>
                <p class="historico__meta">
                  Apertura: {{ formatearFecha(fila.fechaCreacion) }} · Cierre:
                  {{ formatearFecha(fila.fechaCierre) }}
                </p>
              </div>
              <div class="historico__acciones-cabecera">
                <span class="badge badge--resuelto">Resuelto</span>
                <button
                  class="eliminar-ticket"
                  type="button"
                  :disabled="eliminandoTicketId === fila.id"
                  @click="eliminarTicket(fila)"
                >
                  {{ eliminandoTicketId === fila.id ? 'Borrando...' : 'Eliminar' }}
                </button>
              </div>
            </header>

            <div class="historico__grid">
              <div class="historico__campo">
                <span class="historico__etiqueta">Solicitante</span>
                <span>{{ fila.solicitante || '—' }}</span>
              </div>
              <div class="historico__campo">
                <span class="historico__etiqueta">Departamento</span>
                <span>{{ fila.departamento || '—' }}</span>
              </div>
              <div class="historico__campo">
                <span class="historico__etiqueta">Categoría</span>
                <span>{{ fila.categoria || '—' }}</span>
              </div>
              <div class="historico__campo">
                <span class="historico__etiqueta">Equipo afectado</span>
                <span>{{ fila.equipoAfectado || '—' }}</span>
              </div>
            </div>

            <div class="historico__bloque">
              <span class="historico__etiqueta">Problema original</span>
              <p>{{ fila.descripcion || 'Sin descripción.' }}</p>
            </div>

            <div class="historico__bloque">
              <span class="historico__etiqueta">Solución aplicada</span>
              <p>{{ fila.solucionAplicada || 'Sin registrar.' }}</p>
            </div>

            <div class="historico__bloque">
              <span class="historico__etiqueta">Suministros utilizados</span>
              <p>{{ fila.suministrosUtilizados || 'Ninguno' }}</p>
            </div>
          </article>
        </div>
      </template>

      <template v-else>
        <div class="estadisticas__filtros">
          <label class="estadisticas__campo">
            <span>Desde</span>
            <input v-model="desdeEstadisticas" class="campo__input" type="date" />
          </label>
          <label class="estadisticas__campo">
            <span>Hasta</span>
            <input v-model="hastaEstadisticas" class="campo__input" type="date" />
          </label>
          <button
            class="boton boton--primario"
            type="button"
            :disabled="cargandoEstadisticas"
            @click="cargarEstadisticas"
          >
            {{ cargandoEstadisticas ? 'Consultando...' : 'Aplicar Filtro' }}
          </button>
          <div class="estadisticas__descargas">
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="descargandoReporte"
              @click="descargarExcel"
            >
              {{ formatoReporte === 'excel' ? 'Generando...' : 'Descargar Excel (.csv)' }}
            </button>
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="descargandoReporte"
              @click="descargarPdf"
            >
              {{ formatoReporte === 'pdf' ? 'Generando...' : 'Descargar PDF (Gráficas + Datos)' }}
            </button>
          </div>
        </div>

        <p v-if="errorEstadisticas" class="alerta">{{ errorEstadisticas }}</p>

        <div v-if="cargandoEstadisticas" class="estado-vacio">
          <p class="estado-vacio__texto">Calculando estadísticas...</p>
        </div>

        <template v-else-if="estadisticas">
          <div class="estadisticas__resumen">
            <article class="estadisticas__tarjeta">
              <span class="estadisticas__valor">{{ estadisticas.resumen.total }}</span>
              <span class="estadisticas__etiqueta">Total de tickets</span>
            </article>
            <article class="estadisticas__tarjeta estadisticas__tarjeta--resuelto">
              <span class="estadisticas__valor">{{ estadisticas.resumen.resueltos }}</span>
              <span class="estadisticas__etiqueta">Resueltos</span>
            </article>
            <article class="estadisticas__tarjeta estadisticas__tarjeta--abierto">
              <span class="estadisticas__valor">{{ estadisticas.resumen.abiertos }}</span>
              <span class="estadisticas__etiqueta">Abiertos</span>
            </article>
            <article class="estadisticas__tarjeta estadisticas__tarjeta--progreso">
              <span class="estadisticas__valor">{{ estadisticas.resumen.enProgreso }}</span>
              <span class="estadisticas__etiqueta">En progreso</span>
            </article>
          </div>

          <div class="estadisticas__graficas">
            <article class="estadisticas__grafica">
              <h3 class="estadisticas__titulo">Demanda por Departamento / Área de Servicio</h3>
              <div
                v-if="estadisticas.porDepartamento.length"
                ref="graficaDepartamentoRef"
                class="estadisticas__lienzo"
              >
                <Bar :data="chartDepartamentoData" :options="chartDepartamentoOptions" />
              </div>
              <p v-else class="estado-vacio__texto">No hay datos para el periodo seleccionado.</p>
            </article>

            <article class="estadisticas__grafica">
              <h3 class="estadisticas__titulo">Incidencias por Categoría</h3>
              <div
                v-if="estadisticas.porCategoria.length"
                ref="graficaCategoriaRef"
                class="estadisticas__lienzo"
              >
                <Doughnut :data="chartCategoriaData" :options="chartCategoriaOptions" />
              </div>
              <p v-else class="estado-vacio__texto">No hay datos para el periodo seleccionado.</p>
            </article>
          </div>

          <article class="estadisticas__recurrencia">
            <h3 class="estadisticas__titulo">Equipos con Mayor Recurrencia de Fallas</h3>

            <div v-if="estadisticas.equiposRecurrentes.length === 0" class="estado-vacio">
              <p class="estado-vacio__texto">Aún no se registran equipos afectados en los tickets.</p>
            </div>

            <div v-else class="tabla__envoltura">
              <table class="tabla">
                <thead>
                  <tr>
                    <th class="tabla__th">Equipo / Periférico</th>
                    <th class="tabla__th tabla__th--corta">Incidencias</th>
                    <th class="tabla__th tabla__th--corta">Resueltas</th>
                    <th class="tabla__th tabla__th--corta">Abiertas</th>
                    <th class="tabla__th">Última incidencia</th>
                    <th class="tabla__th">Cómo se resolvieron</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="equipo in estadisticas.equiposRecurrentes"
                    :key="equipo.equipo"
                    class="tabla__fila"
                  >
                    <td class="tabla__td tabla__td--nombre">{{ equipo.equipo }}</td>
                    <td class="tabla__td tabla__td--id">{{ equipo.total }}</td>
                    <td class="tabla__td tabla__td--id">{{ equipo.resueltos }}</td>
                    <td class="tabla__td tabla__td--id">{{ equipo.abiertos }}</td>
                    <td class="tabla__td">{{ formatearFecha(equipo.ultimaIncidencia) }}</td>
                    <td class="tabla__td">
                      <span v-if="equipo.soluciones.length === 0">Sin registrar</span>
                      <ul v-else class="estadisticas__soluciones">
                        <li v-for="solucion in equipo.soluciones" :key="solucion">{{ solucion }}</li>
                      </ul>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </article>
        </template>

        <div v-else class="estado-vacio">
          <p class="estado-vacio__texto">Aplica un filtro para consultar las estadísticas.</p>
        </div>
      </template>
    </section>

    <div v-if="mostrarModalNuevo" class="modal">
      <div class="modal__tarjeta">
        <h3 class="modal__titulo">Nuevo Ticket</h3>

        <form class="formulario" @submit.prevent="crearTicket">
          <div class="campo">
            <label class="campo__etiqueta" for="titulo">Título</label>
            <input
              id="titulo"
              v-model="formulario.titulo"
              class="campo__input"
              type="text"
              placeholder="Resumen del problema"
              maxlength="200"
              :disabled="guardando"
            />
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="solicitante">Solicitante (Persona auxiliada)</label>
            <input
              id="solicitante"
              v-model="formulario.solicitante"
              class="campo__input"
              type="text"
              list="solicitantes-sugeridos"
              placeholder="Selecciona un empleado o escribe el nombre manualmente"
              maxlength="150"
              :disabled="guardando"
              @change="alCambiarSolicitante"
            />
            <datalist id="solicitantes-sugeridos">
              <option
                v-for="empleado in directorioEmpleados"
                :key="empleado.id"
                :value="empleado.nombreCompleto"
              >
                {{ empleado.cargo || 'Empleado' }} · {{ empleado.departamento || 'Sin departamento' }}
              </option>
            </datalist>
            <p class="campo__ayuda">
              Al elegir un empleado registrado se completan su departamento y prioridad.
            </p>
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="departamento">Departamento</label>
            <select
              id="departamento"
              v-model="formulario.departamento"
              class="campo__input"
              :disabled="guardando"
              @change="alCambiarDepartamento"
            >
              <option value="">Selecciona un departamento</option>
              <option
                v-for="departamento in departamentos"
                :key="departamento.id"
                :value="departamento.nombre"
              >
                {{ departamento.nombre }} ({{ departamento.sigla }}) · Prioridad
                {{ departamento.prioridadPorDefecto }}
              </option>
            </select>
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="categoria">Categoría</label>
            <select
              id="categoria"
              v-model="formulario.categoria"
              class="campo__input"
              :disabled="guardando"
            >
              <option value="">Selecciona una categoría</option>
              <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.nombre">
                {{ categoria.nombre }}
              </option>
            </select>
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="prioridad">Prioridad</label>
            <select
              id="prioridad"
              v-model.number="formulario.prioridad"
              class="campo__input"
              :disabled="guardando"
            >
              <option :value="0">Baja</option>
              <option :value="1">Media</option>
              <option :value="2">Alta</option>
              <option :value="3">Crítica</option>
            </select>
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="descripcion">Descripción</label>
            <textarea
              id="descripcion"
              v-model="formulario.descripcion"
              class="campo__input campo__input--area"
              rows="3"
              placeholder="Detalle del incidente o solicitud"
              :disabled="guardando"
            ></textarea>
          </div>

          <p v-if="errorFormulario" class="alerta alerta--formulario">{{ errorFormulario }}</p>

          <div class="formulario__acciones">
            <button class="boton boton--primario" type="submit" :disabled="guardando">
              {{ guardando ? 'Guardando...' : 'Guardar' }}
            </button>
            <button class="boton boton--secundario" type="button" :disabled="guardando" @click="cerrarModalNuevo">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="mostrarModalResolucion && ticketResolucion" class="modal">
      <div class="modal__tarjeta">
        <h3 class="modal__titulo">Registrar Solución Técnica (Cierre de Ticket)</h3>

        <p class="detalle__titulo-ticket">
          {{ ticketResolucion.codigoNomenclatura || '#' + ticketResolucion.id }}
          {{ ticketResolucion.titulo }}
        </p>

        <p v-if="ticketResolucion.solicitante" class="detalle__meta-item">
          <strong>Solicitante:</strong> {{ ticketResolucion.solicitante }}
        </p>

        <form class="formulario" @submit.prevent="confirmarResolucion">
          <div class="campo">
            <label class="campo__etiqueta" for="equipo-afectado">Equipo Afectado</label>
            <input
              id="equipo-afectado"
              v-model="resolucionForm.equipoAfectado"
              class="campo__input"
              type="text"
              maxlength="150"
              placeholder="Ej. PC-005 / Monitor Dell / Impresora Facturación"
              :disabled="guardandoResolucion"
            />
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="solucion-aplicada">
              Solución Aplicada <span class="campo__obligatorio">*</span>
            </label>
            <textarea
              id="solucion-aplicada"
              v-model="resolucionForm.solucionAplicada"
              class="campo__input campo__input--area"
              rows="4"
              maxlength="2000"
              placeholder="Describe paso a paso cómo se resolvió la falla"
              :disabled="guardandoResolucion"
            ></textarea>
          </div>

          <div class="campo">
            <label class="campo__etiqueta" for="suministros-utilizados">Suministros Utilizados</label>
            <input
              id="suministros-utilizados"
              v-model="resolucionForm.suministrosUtilizados"
              class="campo__input"
              type="text"
              list="suministros-sugeridos"
              maxlength="1000"
              placeholder="Ej. Ninguno, Mouse Óptico USB, Cable de poder, Tóner HP 85A"
              :disabled="guardandoResolucion"
            />
            <datalist id="suministros-sugeridos">
              <option v-for="suministro in SUMINISTROS_SUGERIDOS" :key="suministro" :value="suministro">
                {{ suministro }}
              </option>
            </datalist>
          </div>

          <p v-if="errorResolucion" class="alerta alerta--formulario">{{ errorResolucion }}</p>

          <div class="formulario__acciones">
            <button class="boton boton--primario" type="submit" :disabled="guardandoResolucion">
              {{ guardandoResolucion ? 'Guardando...' : 'Guardar y Resolver' }}
            </button>
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="guardandoResolucion"
              @click="cerrarModalResolucion"
            >
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="mostrarModalDetalle && ticketDetalle" class="modal">
      <div class="modal__tarjeta">
        <h3 class="modal__titulo">
          Ticket {{ ticketDetalle.codigoNomenclatura || '#' + ticketDetalle.id }}
        </h3>

        <p class="detalle__titulo-ticket">{{ ticketDetalle.titulo }}</p>

        <div class="detalle__badges">
          <span class="badge" :class="`badge--${normalizar(ticketDetalle.estado, ESTADOS).clase}`">
            {{ normalizar(ticketDetalle.estado, ESTADOS).texto }}
          </span>
          <span class="badge" :class="`badge--${normalizar(ticketDetalle.prioridad, PRIORIDADES).clase}`">
            {{ normalizar(ticketDetalle.prioridad, PRIORIDADES).texto }}
          </span>
        </div>

        <div class="detalle__meta">
          <p v-if="ticketDetalle.solicitante" class="detalle__meta-item">
            <strong>Solicitante:</strong> {{ ticketDetalle.solicitante }}
          </p>
          <p v-if="ticketDetalle.fechaCreacion" class="detalle__meta-item">
            <strong>Creado:</strong> {{ new Date(ticketDetalle.fechaCreacion).toLocaleString() }}
          </p>
          <p v-if="ticketDetalle.fechaCierre" class="detalle__meta-item">
            <strong>Cerrado:</strong> {{ new Date(ticketDetalle.fechaCierre).toLocaleString() }}
          </p>
          <p v-if="ticketDetalle.slaHoras" class="detalle__meta-item">
            <strong>SLA:</strong> {{ ticketDetalle.slaHoras }} horas
          </p>
        </div>

        <div class="detalle__descripcion-envoltura">
          <p class="detalle__etiqueta">Descripción</p>
          <div class="detalle__descripcion">{{ ticketDetalle.descripcion || 'Sin descripción.' }}</div>
        </div>

        <div class="campo">
          <label class="campo__etiqueta" for="estado-detalle">Estado de Atención</label>
          <select id="estado-detalle" v-model.number="estadoDetalle" class="campo__input" :disabled="guardandoDetalle">
            <option :value="0">Abierto (Con recordatorios activos)</option>
            <option :value="1">En Progreso (Técnico trabajando · Silencia alertas)</option>
          </select>
        </div>

        <div class="campo">
          <label class="campo__etiqueta" for="departamento-detalle">Departamento</label>
          <select id="departamento-detalle" v-model="departamentoDetalle" class="campo__input" :disabled="guardandoDetalle">
            <option value="">Selecciona un departamento</option>
            <option v-for="departamento in departamentos" :key="departamento.id" :value="departamento.nombre">
              {{ departamento.nombre }} ({{ departamento.sigla }})
            </option>
          </select>
        </div>

        <div class="campo">
          <label class="campo__etiqueta" for="categoria-detalle">Categoría</label>
          <select id="categoria-detalle" v-model="categoriaDetalle" class="campo__input" :disabled="guardandoDetalle">
            <option value="">Selecciona una categoría</option>
            <option v-for="categoria in categorias" :key="categoria.id" :value="categoria.nombre">
              {{ categoria.nombre }}
            </option>
          </select>
        </div>

        <div class="campo">
          <label class="campo__etiqueta" for="prioridad-detalle">Prioridad</label>
          <select id="prioridad-detalle" v-model.number="prioridadDetalle" class="campo__input" :disabled="guardandoDetalle">
            <option :value="0">Baja</option>
            <option :value="1">Media</option>
            <option :value="2">Alta</option>
            <option :value="3">Crítica</option>
          </select>
        </div>

        <p v-if="errorDetalle" class="alerta">{{ errorDetalle }}</p>

        <div class="formulario__acciones">
          <button class="boton boton--primario" type="button" :disabled="guardandoDetalle" @click="guardarDetalle">
            {{ guardandoDetalle ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
          <button class="boton boton--secundario" type="button" :disabled="guardandoDetalle" @click="cerrarDetalles">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vista__encabezado { margin-bottom: 20px; }
.vista__titulo { margin: 0; font-size: 20px; font-weight: 600; color: #1a1f2b; }
.vista__subtitulo { margin: 4px 0 0; font-size: 13px; color: #6b7280; }
.tarjeta { background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); padding: 24px; margin-top: 20px; }
.tarjeta__cabecera { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 16px; margin-bottom: 20px; }
.tarjeta__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.tarjeta__leyenda-alertas { margin: 4px 0 0; font-size: 12px; color: #6b7280; }
.tarjeta__acciones { display: flex; align-items: center; flex-wrap: wrap; gap: 10px; }
.boton { padding: 8px 14px; font-size: 13px; font-weight: 500; font-family: inherit; border: 1px solid transparent; border-radius: 6px; cursor: pointer; }
.boton:disabled { cursor: not-allowed; opacity: 0.6; }
.boton--primario { color: #ffffff; background: #1f3a68; }
.boton--primario:hover:not(:disabled) { background: #16294a; }
.boton--secundario { color: #1f3a68; background: #ffffff; border-color: #d0d5dd; }
.boton--secundario:hover:not(:disabled) { background: #f8fafc; }
.boton--sonido { color: #067647; background: #ecfdf3; border-color: #a6f4c5; }
.boton--silenciado { color: #b54708; background: #fffaeb; border-color: #fec84b; }
.formulario { padding: 20px; margin-bottom: 4px; background: #f9fafb; border: 1px solid #eceff3; border-radius: 8px; }
.campo { margin-bottom: 16px; }
.campo__etiqueta { display: block; margin-bottom: 6px; font-size: 13px; font-weight: 500; color: #344054; }
.campo__input { width: 100%; padding: 9px 12px; font-size: 14px; font-family: inherit; color: #1a1f2b; background: #ffffff; border: 1px solid #d0d5dd; border-radius: 6px; box-sizing: border-box; }
.campo__input:focus { outline: none; border-color: #1f3a68; box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12); }
.campo__input:disabled { background: #f2f4f7; cursor: not-allowed; }
.campo__input--area { resize: vertical; }
.campo__ayuda { margin: 6px 0 0; font-size: 12px; color: #6b7280; }
.formulario__acciones { display: flex; align-items: center; gap: 10px; }
.alerta { margin: 16px 0 0; padding: 10px 12px; font-size: 13px; color: #b42318; background: #fef3f2; border: 1px solid #fecdca; border-radius: 6px; }
.alerta--formulario { margin: 0 0 16px; }
.estado-vacio { padding: 48px 20px; text-align: center; }
.estado-vacio__texto { margin: 0; font-size: 14px; color: #6b7280; }
.activos__barra-busqueda { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
.activos__conteo { font-size: 12px; color: #667085; white-space: nowrap; }
.tabla__envoltura { overflow-x: auto; }
.tabla { width: 100%; border-collapse: collapse; }
.tabla__th { padding: 12px 8px; font-size: 0.75rem; font-weight: 600; letter-spacing: 0.05em; text-align: left; text-transform: uppercase; color: #6b7280; background: transparent; border-bottom: 1px solid #e5e7eb; }
.th-sort { display: inline-flex; align-items: center; gap: 4px; padding: 0; font: inherit; text-transform: inherit; letter-spacing: inherit; color: inherit; background: transparent; border: none; cursor: pointer; user-select: none; }
.th-sort:hover { color: #1f3a68; }
.th-sort__icono { font-size: 11px; color: #98a2b3; }
.tabla__th--id, .tabla__th--codigo, .tabla__th--estado, .tabla__th--prioridad { width: 1%; white-space: nowrap; }
.tabla__th--acciones { width: 1%; white-space: nowrap; text-align: right; }
.tabla__fila:hover { background: #f9fafb; }
.tabla__td { padding: 16px 8px; font-size: 14px; text-align: left; color: #344054; border-bottom: 1px solid #f3f4f6; }
.tabla__fila:last-child .tabla__td { border-bottom: none; }
.tabla__td--id { font-variant-numeric: tabular-nums; color: #6b7280; }
.tabla__td--titulo { font-weight: 500; color: #1a1f2b; }
.tabla__td--acciones { text-align: right; white-space: nowrap; }
.silenciar-tarea { padding: 5px 10px; margin-right: 4px; font-size: 12px; font-weight: 600; font-family: inherit; color: #475467; background: #f2f4f7; border: 1px solid #d0d5dd; border-radius: 6px; cursor: pointer; }
.silenciar-tarea--activo { color: #b54708; background: #fffaeb; border-color: #fec84b; }
.detalles { padding: 6px 10px; font-size: 12.5px; font-weight: 600; font-family: inherit; color: #475467; background: transparent; border: none; border-radius: 6px; cursor: pointer; }
.detalles:hover { background: #f2f4f7; }
.resolver { padding: 6px 10px; font-size: 12.5px; font-weight: 600; font-family: inherit; color: #2563eb; background: transparent; border: none; border-radius: 6px; cursor: pointer; }
.resolver:hover:not(:disabled) { background: #eff6ff; }
.resolver:disabled { color: #98a2b3; cursor: not-allowed; }
.eliminar-ticket { padding: 6px 10px; margin-left: 4px; font-size: 12.5px; font-weight: 600; font-family: inherit; color: #dc2626; background: #fee2e2; border: none; border-radius: 6px; cursor: pointer; }
.eliminar-ticket:hover:not(:disabled) { background: #fecaca; }
.eliminar-ticket:disabled { opacity: 0.6; cursor: not-allowed; }
.badge { display: inline-block; padding: 4px 12px; font-size: 0.8rem; font-weight: 600; border-radius: 999px; white-space: nowrap; }
.badge--alta, .badge--critica { color: #dc2626; background: #fee2e2; }
.badge--codigo { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; color: #1f3a68; background: #e8eef8; letter-spacing: 0.03em; }
.badge--media { color: #d97706; background: #fef3c7; }
.badge--baja { color: #059669; background: #d1fae5; }
.badge--abierto, .badge--en-progreso { color: #2563eb; background: #dbeafe; }
.badge--resuelto, .badge--completado { color: #16a34a; background: #dcfce7; }
.modal { position: fixed; inset: 0; z-index: 50; display: flex; align-items: center; justify-content: center; padding: 20px; background: rgba(0, 0, 0, 0.5); }
.modal__tarjeta { width: 100%; max-width: 520px; max-height: 90vh; padding: 24px; overflow-y: auto; background: #ffffff; border-radius: 12px; box-shadow: 0 20px 40px rgba(16, 24, 40, 0.25); }
.modal__titulo { margin: 0 0 20px; font-size: 17px; font-weight: 600; color: #1a1f2b; }
.modal .formulario { padding: 0; margin: 0; background: transparent; border: none; }
.detalle__titulo-ticket { margin: 0 0 12px; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.detalle__badges { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 16px; }
.detalle__meta { margin-bottom: 16px; }
.detalle__meta-item { margin: 0 0 4px; font-size: 13px; color: #475467; }
.detalle__descripcion-envoltura { margin-bottom: 16px; }
.detalle__etiqueta { margin: 0 0 6px; font-size: 13px; font-weight: 500; color: #344054; }
.detalle__descripcion { max-height: 220px; padding: 12px 14px; font-size: 14px; line-height: 1.5; color: #344054; white-space: pre-wrap; overflow-wrap: anywhere; background: #f9fafb; border: 1px solid #eceff3; border-radius: 8px; overflow-y: auto; }
.modal .formulario__acciones { margin-top: 20px; }
.pestanas { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 18px; border-bottom: 1px solid #eceff3; }
.pestanas__boton { display: inline-flex; align-items: center; gap: 8px; padding: 10px 14px; margin-bottom: -1px; font-size: 13px; font-weight: 500; color: #667085; background: transparent; border: none; border-bottom: 2px solid transparent; cursor: pointer; }
.pestanas__boton:hover { color: #1a1f2b; }
.pestanas__boton--activa { color: #2563eb; border-bottom-color: #2563eb; }
.pestanas__contador { display: inline-flex; align-items: center; justify-content: center; min-width: 22px; height: 20px; padding: 0 6px; font-size: 11px; font-weight: 600; color: #475467; background: #f2f4f7; border-radius: 999px; }
.pestanas__boton--activa .pestanas__contador { color: #1d4ed8; background: #dbeafe; }
.historico__busqueda { margin-bottom: 16px; }
.historico__lista { display: grid; gap: 14px; }
.historico__tarjeta { padding: 18px; background: #fbfcfd; border: 1px solid #eceff3; border-radius: 10px; }
.historico__cabecera { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; margin-bottom: 14px; }
.historico__acciones-cabecera { display: flex; align-items: center; gap: 8px; }
.historico__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.historico__titulo-linea { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; }
.historico__meta { margin: 4px 0 0; font-size: 12px; color: #6b7280; }
.historico__grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; margin-bottom: 14px; }
.historico__campo { display: flex; flex-direction: column; gap: 4px; font-size: 13px; color: #344054; }
.historico__campo > span:last-child { overflow-wrap: anywhere; }
.historico__bloque { margin-top: 12px; }
.historico__bloque p { margin: 4px 0 0; font-size: 13px; line-height: 1.5; color: #344054; white-space: pre-wrap; overflow-wrap: anywhere; }
.historico__etiqueta { font-size: 11px; font-weight: 600; letter-spacing: 0.04em; text-transform: uppercase; color: #6b7280; }
.campo__obligatorio { color: #dc2626; }
.estadisticas__filtros { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px; margin-bottom: 18px; }
.estadisticas__campo { display: flex; flex-direction: column; gap: 6px; min-width: 160px; font-size: 12px; font-weight: 500; color: #475467; }
.estadisticas__descargas { display: flex; flex-wrap: wrap; gap: 10px; }
.estadisticas__resumen { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; margin-bottom: 18px; }
.estadisticas__tarjeta { display: flex; flex-direction: column; gap: 4px; padding: 16px; background: #f8fafc; border: 1px solid #eceff3; border-left: 4px solid #3b82f6; border-radius: 10px; }
.estadisticas__tarjeta--resuelto { border-left-color: #10b981; }
.estadisticas__tarjeta--abierto { border-left-color: #f59e0b; }
.estadisticas__tarjeta--progreso { border-left-color: #8b5cf6; }
.estadisticas__valor { font-size: 26px; font-weight: 700; color: #1a1f2b; }
.estadisticas__etiqueta { font-size: 12px; font-weight: 500; color: #6b7280; }
.estadisticas__graficas { display: grid; grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr); gap: 16px; margin-bottom: 18px; }
.estadisticas__grafica, .estadisticas__recurrencia { padding: 18px; background: #ffffff; border: 1px solid #eceff3; border-radius: 10px; }
.estadisticas__titulo { margin: 0 0 14px; font-size: 14px; font-weight: 600; color: #1a1f2b; }
.estadisticas__lienzo { position: relative; height: 280px; }
.estadisticas__soluciones { margin: 0; padding-left: 16px; font-size: 12px; line-height: 1.5; color: #475467; }
.estadisticas__soluciones li + li { margin-top: 4px; }
@media (max-width: 1024px) {
  .estadisticas__graficas { grid-template-columns: 1fr; }
  .historico__grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 768px) {
  .historico__grid { grid-template-columns: 1fr; }
  .historico__cabecera { flex-direction: column; }
  .estadisticas__resumen { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>