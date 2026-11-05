<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import { API_BASE_URL } from '../config'

const API_BASE = API_BASE_URL
const API_URL = `${API_BASE}/api/Impresoras`
const TOKEN_KEY = 'sgcti_token'

const ESTADOS_PING = ['En Línea', 'Inestable', 'Desconectado'] as const

interface Impresora {
  id: number
  modelo: string
  ip: string
  departamento?: string
  fabricante?: string
  nivelTonnerNegro: number
  estadoPing: number | string
  latenciaMs?: number
  contadorTotalPaginas?: number
  promedioPaginasDiario?: number
  paginasDesdeUltimoMantenimiento?: number
  diasEstimadosAgotamientoTonner?: number
  diasEstimadosMantenimiento?: number
  ultimoMantenimiento?: string | null
  fechaInstalacion?: string
}

interface Fila {
  id: number
  modelo: string
  departamento: string
  ip: string
  nivel: number
  nivelClase: string
  estadoPingClase: string
  latencia: number
  latenciaClase: string
  fabricante: string
  contadorTotalPaginas: number
  promedioPaginasDiario: number
  paginasDesdeUltimoMantenimiento: number
  diasEstimadosAgotamientoTonner: number
  diasEstimadosMantenimiento: number
  ultimoMantenimiento: string
  fechaInstalacion: string
}

interface Mantenimiento {
  id: number
  impresoraId: number
  fecha: string
  tipo: string
  descripcion: string
  realizadoPor: string
}

const router = useRouter()

const filas = ref<Fila[]>([])
const cargando = ref(true)
const error = ref('')

const detalle = ref<Fila | null>(null)
const pestañaActiva = ref<'info' | 'mantenimientos'>('info')

const mantenimientos = ref<Mantenimiento[]>([])
const cargandoMantenimientos = ref(false)
const guardandoMantenimiento = ref(false)
const errorMantenimientos = ref('')

const formularioMantenimiento = ref<{ tipo: string; realizadoPor: string; descripcion: string }>({
  tipo: 'Preventivo',
  realizadoPor: localStorage.getItem('nombre_usuario') || '',
  descripcion: '',
})

const buscandoManuales = ref(false)
const archivoManual = ref<File | null>(null)
const subiendoManual = ref(false)
const inputArchivoRef = ref<HTMLInputElement | null>(null)

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
  if (nivel <= 15) return 'bajo'
  if (nivel <= 40) return 'medio'
  return 'alto'
}

function claseDiasPrediccion(dias: number): string {
  if (dias <= 5) return 'critico'
  if (dias <= 14) return 'preventivo'
  return 'optimo'
}

function claseDeLatencia(latencia: number, desconectado: boolean): string {
  if (desconectado || latencia > 150) return 'rojo'
  if (latencia < 50) return 'verde'
  return 'amarillo'
}

function formatearFecha(iso: string | null | undefined): string {
  if (!iso) return 'Sin registro'
  const fecha = new Date(iso)
  if (Number.isNaN(fecha.getTime())) return iso
  return fecha.toLocaleDateString('es-MX', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

function abrirDetalle(fila: Fila) {
  detalle.value = fila
  pestañaActiva.value = 'info'
  mantenimientos.value = []
  errorMantenimientos.value = ''
  formularioMantenimiento.value = {
    tipo: 'Preventivo',
    realizadoPor: localStorage.getItem('nombre_usuario') || 'Ingeniero TI',
    descripcion: '',
  }
  archivoManual.value = null
}

function cerrarDetalle() {
  detalle.value = null
  archivoManual.value = null
}

function alPulsarEscape(evento: KeyboardEvent) {
  if (evento.key === 'Escape') cerrarDetalle()
}

async function cargarImpresoras() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Impresora[]>(API_URL, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    filas.value = data.map((impresora) => {
      const estadoPing = normalizar(impresora.estadoPing, ESTADOS_PING)
      const nivel = Math.max(0, Math.min(100, impresora.nivelTonnerNegro ?? 0))
      const latencia = Math.max(0, impresora.latenciaMs ?? 0)
      const desconectado = estadoPing.clase === 'desconectado'

      return {
        id: impresora.id,
        modelo: impresora.modelo,
        departamento: impresora.departamento || 'General',
        ip: impresora.ip,
        nivel,
        nivelClase: claseDeNivel(nivel),
        estadoPingClase: estadoPing.clase,
        latencia,
        latenciaClase: claseDeLatencia(latencia, desconectado),
        fabricante: impresora.fabricante || 'Canon',
        contadorTotalPaginas: impresora.contadorTotalPaginas ?? 0,
        promedioPaginasDiario: impresora.promedioPaginasDiario ?? 0,
        paginasDesdeUltimoMantenimiento: impresora.paginasDesdeUltimoMantenimiento ?? 0,
        diasEstimadosAgotamientoTonner: impresora.diasEstimadosAgotamientoTonner ?? 0,
        diasEstimadosMantenimiento: impresora.diasEstimadosMantenimiento ?? 0,
        ultimoMantenimiento: formatearFecha(impresora.ultimoMantenimiento),
        fechaInstalacion: formatearFecha(impresora.fechaInstalacion),
      }
    })

    if (detalle.value) {
      const actualizada = filas.value.find((f) => f.id === detalle.value?.id)
      if (actualizada) detalle.value = actualizada
    }
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

async function buscarManuales(id: number) {
  buscandoManuales.value = true

  try {
    const { data } = await axios.get<string>(`${API_BASE}/api/impresoras/${id}/buscar-manuales`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })
    if (data) window.open(data, '_blank')
  } catch (e) {
    console.error('No se pudo obtener la URL de búsqueda de manuales:', e)
  } finally {
    buscandoManuales.value = false
  }
}

function abrirSelectorArchivo() {
  inputArchivoRef.value?.click()
}

async function seleccionarArchivo(event: Event) {
  const input = event.target as HTMLInputElement
  archivoManual.value = input.files?.[0] ?? null
  input.value = ''

  if (detalle.value) {
    await subirManual(detalle.value.id)
  }
}

async function subirManual(id: number) {
  if (!archivoManual.value) return
  subiendoManual.value = true

  try {
    const formData = new FormData()
    formData.append('archivo', archivoManual.value)

    await axios.post(`${API_BASE}/api/impresoras/${id}/subir-manual`, formData, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })

    toast.success('Manual subido correctamente.')
    archivoManual.value = null
  } catch (e) {
    console.error('No se pudo subir el manual:', e)
    toast.error('No se pudo subir el manual. Intenta nuevamente.')
  } finally {
    subiendoManual.value = false
  }
}

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

function cambiarPestaña(pestaña: 'info' | 'mantenimientos') {
  pestañaActiva.value = pestaña
  if (pestaña === 'mantenimientos' && mantenimientos.value.length === 0 && detalle.value) {
    cargarMantenimientos(detalle.value.id)
  }
}

async function cargarMantenimientos(id: number) {
  cargandoMantenimientos.value = true
  errorMantenimientos.value = ''

  try {
    const { data } = await axios.get<Mantenimiento[]>(`${API_BASE}/api/impresoras/${id}/mantenimientos`, {
      headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) },
    })
    mantenimientos.value = data
  } catch (e) {
    console.error('No se pudieron cargar los mantenimientos:', e)
    errorMantenimientos.value = 'No se pudieron cargar los mantenimientos.'
  } finally {
    cargandoMantenimientos.value = false
  }
}

async function guardarMantenimiento(id: number) {
  if (!formularioMantenimiento.value.descripcion.trim() || !formularioMantenimiento.value.realizadoPor.trim()) {
    return
  }

  guardandoMantenimiento.value = true

  try {
    await axios.post(
      `${API_BASE}/api/impresoras/${id}/mantenimientos`,
      formularioMantenimiento.value,
      { headers: { Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY) } },
    )

    formularioMantenimiento.value = {
      tipo: 'Preventivo',
      realizadoPor: localStorage.getItem('nombre_usuario') || 'Ingeniero TI',
      descripcion: '',
    }

    await Promise.all([cargarMantenimientos(id), cargarImpresoras()])
    toast.success('Mantenimiento registrado y predicción actualizada.')
  } catch (e) {
    console.error('No se pudo guardar el mantenimiento:', e)
    toast.error('No se pudo guardar el mantenimiento. Intenta nuevamente.')
  } finally {
    guardandoMantenimiento.value = false
  }
}

let intervaloRed: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
  cargarImpresoras()
  intervaloRed = setInterval(cargarImpresoras, 10000)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
  if (intervaloRed) clearInterval(intervaloRed)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <div>
        <h1 class="vista__titulo">Impresoras en Red y Análisis Predictivo</h1>
        <p class="vista__subtitulo">
          Monitoreo SNMP en tiempo real, predicción de agotamiento de tóner y mantenimiento preventivo
        </p>
      </div>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Equipos monitoreados ({{ filas.length }})</h2>
        <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarImpresoras">
          {{ cargando ? 'Cargando...' : 'Actualizar Red' }}
        </button>
      </header>

      <p v-if="error" class="alerta">{{ error }}</p>

      <div v-if="cargando && filas.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">Consultando impresoras por SNMP...</p>
      </div>

      <div v-else-if="filas.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay impresoras registradas.</p>
      </div>

      <div v-else class="tabla__envoltura">
        <table class="tabla">
          <thead>
            <tr>
              <th class="tabla__th">Modelo / Equipo</th>
              <th class="tabla__th">Departamento</th>
              <th class="tabla__th">Dirección IP</th>
              <th class="tabla__th tabla__th--corta">Estado SNMP</th>
              <th class="tabla__th tabla__th--toner">Nivel Tóner Negro</th>
              <th class="tabla__th tabla__th--corta">Agotamiento Tóner</th>
              <th class="tabla__th tabla__th--corta">Próx. Mantenimiento</th>
              <th class="tabla__th tabla__th--corta">Latencia</th>
              <th class="tabla__th tabla__th--corta">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="fila in filas" :key="fila.id" class="tabla__fila">
              <td class="tabla__td tabla__td--modelo">{{ fila.modelo }}</td>
              <td class="tabla__td">{{ fila.departamento }}</td>
              <td class="tabla__td tabla__td--ip">{{ fila.ip }}</td>
              <td class="tabla__td">
                <span class="badge" :class="`badge--${fila.estadoPingClase}`">
                  {{
                    fila.estadoPingClase === 'en-linea'
                      ? 'En línea'
                      : fila.estadoPingClase === 'inestable'
                        ? 'Inestable'
                        : 'Desconectada'
                  }}
                </span>
              </td>
              <td class="tabla__td">
                <div v-if="fila.estadoPingClase === 'en-linea'" class="toner">
                  <span class="toner__barra">
                    <span
                      class="toner__relleno"
                      :class="`toner__relleno--${fila.nivelClase}`"
                      :style="{ width: `${fila.nivel}%` }"
                    ></span>
                  </span>
                  <span class="toner__valor" :class="`toner__valor--${fila.nivelClase}`">
                    {{ fila.nivel }}%
                  </span>
                </div>
                <span v-else class="latencia__valor latencia__valor--rojo">
                  0% (Sin lectura SNMP)
                </span>
              </td>
              <td class="tabla__td">
                <span
                  v-if="fila.estadoPingClase === 'en-linea' && fila.diasEstimadosAgotamientoTonner > 0"
                  class="badge-prediccion"
                  :class="`badge-prediccion--${claseDiasPrediccion(fila.diasEstimadosAgotamientoTonner)}`"
                >
                  ~{{ fila.diasEstimadosAgotamientoTonner }} días
                </span>
                <span v-else class="latencia__valor">
                  —
                </span>
              </td>
              <td class="tabla__td">
                <span
                  v-if="fila.estadoPingClase === 'en-linea' && fila.diasEstimadosMantenimiento > 0"
                  class="badge-prediccion"
                  :class="`badge-prediccion--${claseDiasPrediccion(fila.diasEstimadosMantenimiento)}`"
                >
                  ~{{ fila.diasEstimadosMantenimiento }} días
                </span>
                <span v-else class="latencia__valor">
                  —
                </span>
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
                <button class="detalles" type="button" @click="abrirDetalle(fila)">
                  Ver Detalles
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div v-if="detalle" class="modal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-titulo">
        <header class="modal__cabecera">
          <h3 id="modal-titulo" class="modal__titulo">{{ detalle.modelo }} ({{ detalle.ip }})</h3>
          <button class="modal__cerrar" type="button" aria-label="Cerrar" @click="cerrarDetalle">
            &times;
          </button>
        </header>

        <div class="modal__pestanas" role="tablist">
          <button
            class="modal__pestana"
            type="button"
            :class="{ 'modal__pestana--activa': pestañaActiva === 'info' }"
            @click="cambiarPestaña('info')"
          >
            Diagnóstico y Predicción
          </button>
          <button
            class="modal__pestana"
            type="button"
            :class="{ 'modal__pestana--activa': pestañaActiva === 'mantenimientos' }"
            @click="cambiarPestaña('mantenimientos')"
          >
            Mantenimientos
          </button>
        </div>

        <div v-if="pestañaActiva === 'info'">
          <dl class="modal__lista">
            <div class="modal__dato">
              <dt class="modal__etiqueta">Fabricante / Área</dt>
              <dd class="modal__valor">{{ detalle.fabricante }} · {{ detalle.departamento }}</dd>
            </div>
            <div class="modal__dato">
              <dt class="modal__etiqueta">Contador Total de Páginas</dt>
              <dd class="modal__valor modal__valor--numero">
                {{ detalle.contadorTotalPaginas.toLocaleString('es-MX') }} págs.
              </dd>
            </div>
            <div class="modal__dato">
              <dt class="modal__etiqueta">Promedio de Uso Diario</dt>
              <dd class="modal__valor modal__valor--numero">
                {{ detalle.promedioPaginasDiario.toLocaleString('es-MX') }} págs. / día
              </dd>
            </div>
            <div class="modal__dato">
              <dt class="modal__etiqueta">Predicción Agotamiento de Tóner ({{ detalle.nivel }}%)</dt>
              <dd class="modal__valor modal__valor--numero">
                <template v-if="detalle.estadoPingClase === 'en-linea' && detalle.diasEstimadosAgotamientoTonner > 0">
                  En ~{{ detalle.diasEstimadosAgotamientoTonner }} días
                </template>
                <template v-else>
                  Sin lectura SNMP (Desconectada)
                </template>
              </dd>
            </div>
            <div class="modal__dato">
              <dt class="modal__etiqueta">Páginas desde Último Mantenimiento</dt>
              <dd class="modal__valor modal__valor--numero">
                {{ detalle.paginasDesdeUltimoMantenimiento.toLocaleString('es-MX') }} / 15,000 págs.
              </dd>
            </div>
            <div class="modal__dato">
              <dt class="modal__etiqueta">Predicción Próximo Mantenimiento</dt>
              <dd class="modal__valor modal__valor--numero">
                <template v-if="detalle.estadoPingClase === 'en-linea' && detalle.diasEstimadosMantenimiento > 0">
                  En ~{{ detalle.diasEstimadosMantenimiento }} días (Último: {{ detalle.ultimoMantenimiento }})
                </template>
                <template v-else>
                  Sin lectura SNMP (Último: {{ detalle.ultimoMantenimiento }})
                </template>
              </dd>
            </div>
          </dl>

          <div
            style="
              padding: 12px 20px 8px;
              display: flex;
              flex-wrap: wrap;
              gap: 10px;
              align-items: center;
            "
          >
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="buscandoManuales"
              @click="buscarManuales(detalle.id)"
            >
              {{ buscandoManuales ? 'Buscando...' : 'Buscar Manuales en la Web' }}
            </button>

            <input
              ref="inputArchivoRef"
              type="file"
              accept=".pdf"
              style="display: none"
              @change="seleccionarArchivo"
            />

            <button
              class="boton boton--primario"
              type="button"
              :disabled="subiendoManual"
              @click="abrirSelectorArchivo"
            >
              {{ subiendoManual ? 'Subiendo...' : 'Subir Manual PDF' }}
            </button>

            <button
              class="boton boton--primario"
              type="button"
              @click="
                router.push({
                  path: '/panel/asistente',
                  query: { equipo: detalle.modelo, id: detalle.id },
                })
              "
            >
              Consultar IA sobre este equipo
            </button>
          </div>
        </div>

        <div v-else style="padding: 0 20px 8px">
          <h4 class="mantenimiento__titulo">Registrar mantenimiento (Reinicia contador de desgaste)</h4>

          <div class="mantenimiento__formulario">
            <select v-model="formularioMantenimiento.tipo" class="campo campo--tipo">
              <option value="Preventivo">Preventivo</option>
              <option value="Correctivo">Correctivo</option>
            </select>

            <input
              v-model="formularioMantenimiento.realizadoPor"
              class="campo"
              type="text"
              placeholder="Ingeniero o Técnico responsable"
            />

            <textarea
              v-model="formularioMantenimiento.descripcion"
              class="campo campo--area"
              placeholder="Ej. Limpieza de rodillos, cambio de kit fusor y revisión general..."
            ></textarea>

            <button
              class="boton boton--primario"
              type="button"
              :disabled="
                guardandoMantenimiento ||
                !formularioMantenimiento.realizadoPor.trim() ||
                !formularioMantenimiento.descripcion.trim()
              "
              @click="guardarMantenimiento(detalle.id)"
            >
              {{ guardandoMantenimiento ? 'Guardando...' : 'Guardar Mantenimiento' }}
            </button>
          </div>

          <p v-if="errorMantenimientos" class="alerta">{{ errorMantenimientos }}</p>

          <div v-if="cargandoMantenimientos" class="estado-vacio">
            <p class="estado-vacio__texto">Cargando mantenimientos...</p>
          </div>

          <div v-else-if="mantenimientos.length === 0" class="estado-vacio">
            <p class="estado-vacio__texto">No hay mantenimientos registrados.</p>
          </div>

          <table v-else class="tabla mantenimiento__tabla">
            <thead>
              <tr>
                <th class="tabla__th">Fecha</th>
                <th class="tabla__th">Tipo</th>
                <th class="tabla__th">Responsable</th>
                <th class="tabla__th">Descripción</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="mantenimiento in mantenimientos" :key="mantenimiento.id" class="tabla__fila">
                <td class="tabla__td">{{ formatearFecha(mantenimiento.fecha) }}</td>
                <td class="tabla__td">
                  <span class="tipo" :class="`tipo--${claseDe(mantenimiento.tipo)}`">
                    {{ mantenimiento.tipo }}
                  </span>
                </td>
                <td class="tabla__td">{{ mantenimiento.realizadoPor }}</td>
                <td class="tabla__td">{{ mantenimiento.descripcion }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <footer class="modal__pie">
          <button class="boton boton--primario" type="button" @click="cerrarDetalle">Cerrar</button>
        </footer>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vista__encabezado { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 20px; }
.vista__titulo { margin: 0; font-size: 20px; font-weight: 600; color: #1a1f2b; }
.vista__subtitulo { margin: 4px 0 0; font-size: 13px; color: #6b7280; }
.tarjeta { background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); padding: 24px; margin-top: 20px; }
.tarjeta__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 20px; }
.tarjeta__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.boton { padding: 8px 14px; font-size: 13px; font-weight: 500; font-family: inherit; border: 1px solid transparent; border-radius: 6px; cursor: pointer; }
.boton--secundario { color: #1f3a68; background: #ffffff; border-color: #d0d5dd; }
.boton--primario { color: #ffffff; background: #1f3a68; }
.boton--primario:hover:not(:disabled) { background: #16294a; }
.boton--secundario:hover:not(:disabled) { background: #f8fafc; }
.boton:disabled { cursor: not-allowed; opacity: 0.6; }
.alerta { margin: 16px 0 0; padding: 10px 12px; font-size: 13px; color: #b42318; background: #fef3f2; border: 1px solid #fecdca; border-radius: 6px; }
.estado-vacio { padding: 48px 20px; text-align: center; }
.estado-vacio__texto { margin: 0; font-size: 14px; color: #6b7280; }
.tabla__envoltura { overflow-x: auto; }
.tabla { width: 100%; border-collapse: collapse; }
.tabla__th { padding: 12px 8px; font-size: 0.75rem; font-weight: 600; letter-spacing: 0.05em; text-align: left; text-transform: uppercase; color: #6b7280; background: transparent; border-bottom: 1px solid #e5e7eb; }
.tabla__th--corta, .tabla__th--toner { width: 1%; white-space: nowrap; }
.tabla__fila:hover { background: #f9fafb; }
.tabla__td { padding: 14px 8px; font-size: 13.5px; text-align: left; color: #344054; border-bottom: 1px solid #f3f4f6; }
.tabla__fila:last-child .tabla__td { border-bottom: none; }
.tabla__td--modelo { font-weight: 600; color: #1a1f2b; }
.tabla__td--ip { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 13px; color: #475467; }
.tabla__td--acciones { text-align: right; white-space: nowrap; }
.detalles { padding: 6px 12px; font-size: 12.5px; font-weight: 600; font-family: inherit; color: #2563eb; background: transparent; border: none; border-radius: 6px; cursor: pointer; }
.detalles:hover { background: #eff6ff; }
.badge { display: inline-block; padding: 4px 12px; font-size: 0.8rem; font-weight: 600; border-radius: 999px; white-space: nowrap; }
.badge--en-linea { color: #16a34a; background: #dcfce7; }
.badge--inestable { color: #d97706; background: #fef3c7; }
.badge--desconectado, .badge--desconectada { color: #dc2626; background: #fee2e2; }
.badge-prediccion { display: inline-block; padding: 3px 10px; font-size: 12px; font-weight: 600; border-radius: 999px; white-space: nowrap; }
.badge-prediccion--critico { color: #b42318; background: #fee4e2; }
.badge-prediccion--preventivo { color: #b54708; background: #fef0c7; }
.badge-prediccion--optimo { color: #067647; background: #d1fadf; }
.latencia { display: inline-flex; align-items: center; gap: 8px; white-space: nowrap; }
.latencia__luz { width: 9px; height: 9px; border-radius: 50%; flex-shrink: 0; }
.latencia__luz--verde { background: #12b76a; box-shadow: 0 0 0 3px rgba(18, 183, 106, 0.18); }
.latencia__luz--amarillo { background: #f79009; box-shadow: 0 0 0 3px rgba(247, 144, 9, 0.18); }
.latencia__luz--rojo { background: #f04438; box-shadow: 0 0 0 3px rgba(240, 68, 56, 0.18); }
.latencia__valor { font-size: 13px; font-weight: 500; font-variant-numeric: tabular-nums; color: #344054; }
.latencia__valor--verde { color: #067647; }
.latencia__valor--amarillo { color: #b54708; }
.latencia__valor--rojo { color: #b42318; }
.toner { display: flex; align-items: center; gap: 10px; }
.toner__barra { display: block; width: 90px; height: 6px; background: #eef1f5; border-radius: 999px; overflow: hidden; }
.toner__relleno { display: block; height: 100%; border-radius: 999px; }
.toner__relleno--alto { background: #12b76a; }
.toner__relleno--medio { background: #f79009; }
.toner__relleno--bajo { background: #f04438; }
.toner__valor { font-size: 12px; font-weight: 600; font-variant-numeric: tabular-nums; color: #475467; }
.toner__valor--bajo { color: #b42318; }
.modal { position: fixed; inset: 0; z-index: 50; display: flex; align-items: center; justify-content: center; padding: 24px; background: rgba(16, 24, 40, 0.55); }
.modal__panel { width: 100%; max-width: 640px; max-height: calc(100vh - 48px); overflow-y: auto; background: #ffffff; border-radius: 10px; box-shadow: 0 20px 40px rgba(16, 24, 40, 0.22); }
.modal__pestanas { display: flex; gap: 4px; padding: 12px 16px 0; border-bottom: 1px solid #eceff3; }
.modal__pestana { padding: 9px 14px; font-size: 13px; font-weight: 500; font-family: inherit; color: #667085; background: transparent; border: none; border-bottom: 2px solid transparent; cursor: pointer; }
.modal__pestana:hover { color: #1f3a68; }
.modal__pestana--activa { color: #1f3a68; border-bottom-color: #1f3a68; }
.mantenimiento__titulo { margin: 16px 0 8px; font-size: 14px; font-weight: 600; color: #1a1f2b; }
.mantenimiento__formulario { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.campo { width: 100%; box-sizing: border-box; padding: 8px 10px; font-size: 13px; font-family: inherit; color: #344054; background: #ffffff; border: 1px solid #d0d5dd; border-radius: 6px; }
.campo--tipo { grid-column: 1 / 2; }
.campo--area { grid-column: 1 / -1; min-height: 72px; resize: vertical; }
.mantenimiento__formulario .boton { justify-self: start; align-self: end; }
.mantenimiento__tabla { margin-top: 14px; }
.tipo { display: inline-block; padding: 3px 10px; font-size: 12px; font-weight: 600; border-radius: 999px; }
.tipo--preventivo { color: #067647; background: #ecfdf3; }
.tipo--correctivo { color: #b42318; background: #fef3f2; }
.modal__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 16px 20px; border-bottom: 1px solid #eceff3; }
.modal__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.modal__cerrar { padding: 0; width: 28px; height: 28px; font-size: 20px; line-height: 1; font-family: inherit; color: #667085; background: transparent; border: none; border-radius: 6px; cursor: pointer; }
.modal__cerrar:hover { color: #1a1f2b; background: #f2f4f7; }
.modal__lista { margin: 0; padding: 4px 20px; }
.modal__dato { display: flex; align-items: baseline; justify-content: space-between; gap: 16px; padding: 11px 0; border-bottom: 1px solid #f2f4f7; }
.modal__dato:last-child { border-bottom: none; }
.modal__etiqueta { font-size: 13px; color: #667085; }
.modal__valor { margin: 0; font-size: 14px; font-weight: 600; color: #1a1f2b; text-align: right; }
.modal__valor--numero { font-variant-numeric: tabular-nums; }
.modal__pie { display: flex; justify-content: flex-end; padding: 14px 20px; background: #f9fafb; border-top: 1px solid #eceff3; }
</style>