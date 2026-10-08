<script setup lang="ts">
import { onBeforeUnmount, onMounted, onUnmounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'

const API_URL = 'http://localhost:5141/api/bitacora'

const TOKEN_KEY = 'sgcti_token'

const ETIQUETAS_DISPONIBLES = ['Rutina', 'Urgente', 'Pendiente de Compra']

interface BitacoraActividad {
  id: number
  analistaId: string
  descripcionActividad: string
  equipoIntervenido: string | null
  horaInicio: string
  horaFin: string | null
  etiqueta: string
  estado: string
}

const router = useRouter()

const actividades = ref<BitacoraActividad[]>([])
const cargando = ref(true)
const error = ref('')

const mostrarModal = ref(false)
const guardando = ref(false)
const errorFormulario = ref('')

const completandoId = ref<number | null>(null)
const errorAccion = ref('')

const RECORRIDO_ANALISTA = 'Técnico'
const RECORRIDO_DESCRIPCION = 'Recorrido de instalaciones'
const RECORRIDO_ETIQUETA = 'Rutina'

const recorridoActivo = ref(false)
const recorridoId = ref<number | null>(null)
const tiempoTranscurrido = ref('00:00:00')
const intervaloRecorrido = ref<number | null>(null)
const marcaInicioRecorrido = ref(0)

const formulario = reactive({
  analistaId: '',
  descripcionActividad: '',
  equipoIntervenido: '',
  etiqueta: 'Rutina',
})

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

async function cargarActividades() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<BitacoraActividad[]>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    actividades.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    error.value = 'No se pudieron cargar las actividades. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

function abrirNuevo() {
  formulario.analistaId = ''
  formulario.descripcionActividad = ''
  formulario.equipoIntervenido = ''
  formulario.etiqueta = 'Rutina'
  errorFormulario.value = ''
  mostrarModal.value = true
}

function cerrarModal() {
  mostrarModal.value = false
  errorFormulario.value = ''
}

function alPulsarEscape(evento: KeyboardEvent) {
  if (evento.key === 'Escape' && mostrarModal.value && !guardando.value) {
    cerrarModal()
  }
}

function validarFormulario(): string {
  const analista = formulario.analistaId.trim()
  const descripcion = formulario.descripcionActividad.trim()

  if (analista.length < 2) {
    return 'El analista es obligatorio y debe tener al menos 2 caracteres.'
  }

  if (descripcion.length < 2) {
    return 'La descripción de la actividad es obligatoria.'
  }

  return ''
}

async function guardarActividad() {
  errorFormulario.value = ''

  const fallo = validarFormulario()

  if (fallo) {
    errorFormulario.value = fallo
    return
  }

  guardando.value = true

  const descripcion = formulario.descripcionActividad.trim()

  const payload = {
    analistaId: formulario.analistaId.trim(),
    descripcionActividad: descripcion,
    equipoIntervenido: formulario.equipoIntervenido.trim() || null,
    etiqueta: formulario.etiqueta,
  }

  try {
    await axios.post(API_URL, payload, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    if (recorridoActivo.value) {
      void registrarSubtareaEnRecorrido(descripcion)
    }

    cerrarModal()
    await cargarActividades()
    toast.success('Actividad registrada correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormulario.value = 'No se pudo registrar la actividad. Intenta nuevamente.'
  } finally {
    guardando.value = false
  }
}

async function marcarCompletada(actividad: BitacoraActividad) {
  errorAccion.value = ''

  if (actividad.estado === 'Completado') {
    return
  }

  completandoId.value = actividad.id

  try {
    await axios.put(
      `${API_URL}/${actividad.id}`,
      {
        estado: 'Completado',
        horaFin: new Date().toISOString(),
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    await cargarActividades()
    toast.success('Actividad marcada como completada.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = 'No se pudo completar la actividad. Intenta nuevamente.'
  } finally {
    completandoId.value = null
  }
}

function formatearFechaHora(iso: string | null): string {
  if (!iso) {
    return '—'
  }

  const fecha = new Date(iso)

  if (Number.isNaN(fecha.getTime())) {
    return '—'
  }

  const dia = String(fecha.getDate()).padStart(2, '0')
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  const hora = String(fecha.getHours()).padStart(2, '0')
  const minutos = String(fecha.getMinutes()).padStart(2, '0')

  return `${dia}/${mes}/${fecha.getFullYear()} ${hora}:${minutos}`
}

function actualizarTiempoRecorrido() {
  if (marcaInicioRecorrido.value === 0) {
    return
  }

  const segundosTotales = Math.floor((Date.now() - marcaInicioRecorrido.value) / 1000)

  const horas = String(Math.floor(segundosTotales / 3600)).padStart(2, '0')
  const minutos = String(Math.floor((segundosTotales % 3600) / 60)).padStart(2, '0')
  const segundos = String(segundosTotales % 60).padStart(2, '0')

  tiempoTranscurrido.value = `${horas}:${minutos}:${segundos}`
}

function detenerCronometro() {
  if (intervaloRecorrido.value !== null) {
    window.clearInterval(intervaloRecorrido.value)
    intervaloRecorrido.value = null
  }
}

async function iniciarRecorrido() {
  errorAccion.value = ''

  try {
    const { data } = await axios.post<BitacoraActividad>(
      API_URL,
      {
        analistaId: RECORRIDO_ANALISTA,
        descripcionActividad: RECORRIDO_DESCRIPCION,
        equipoIntervenido: null,
        etiqueta: RECORRIDO_ETIQUETA,
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    recorridoId.value = data.id
    recorridoActivo.value = true
    marcaInicioRecorrido.value = Date.now()
    tiempoTranscurrido.value = '00:00:00'

    detenerCronometro()
    intervaloRecorrido.value = window.setInterval(actualizarTiempoRecorrido, 1000)
    actualizarTiempoRecorrido()
    toast.success('Recorrido iniciado.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = 'No se pudo iniciar el recorrido. Intenta nuevamente.'
  }
}

async function finalizarRecorrido() {
  const id = recorridoId.value

  if (id === null) {
    return
  }

  detenerCronometro()
  completandoId.value = id
  const tiempoFinal = tiempoTranscurrido.value

  try {
    const { data } = await axios.get<BitacoraActividad>(`${API_URL}/${id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    const descripcionFinal = `${data.descripcionActividad}\n[Finalizado. Tiempo: ${tiempoFinal}]`

    await axios.put(
      `${API_URL}/${id}`,
      {
        estado: 'Completado',
        horaFin: new Date().toISOString(),
        descripcionActividad: descripcionFinal,
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    toast.success(`Recorrido finalizado. Tiempo total: ${tiempoFinal}.`)
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = 'No se pudo finalizar el recorrido. Intenta nuevamente.'
  } finally {
    recorridoActivo.value = false
    recorridoId.value = null
    tiempoTranscurrido.value = '00:00:00'
    marcaInicioRecorrido.value = 0
    completandoId.value = null
    await cargarActividades()
  }
}

async function registrarSubtareaEnRecorrido(descripcionSubtarea: string) {
  const id = recorridoId.value

  if (id === null) {
    return
  }

  try {
    const { data } = await axios.get<BitacoraActividad>(`${API_URL}/${id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    const descripcionActualizada = `${data.descripcionActividad}\n- Subtarea: ${descripcionSubtarea}`

    await axios.put(
      `${API_URL}/${id}`,
      {
        estado: data.estado,
        descripcionActividad: descripcionActualizada,
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    console.error('No se pudo registrar la subtarea en el recorrido:', e)
  }
}

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
  cargarActividades()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})

onUnmounted(() => {
  detenerCronometro()
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Bitácora de Actividades</h1>
      <p class="vista__subtitulo">Auditoría y tareas diarias de los técnicos</p>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Actividades registradas</h2>
        <div class="tarjeta__acciones">
          <button
            v-if="!recorridoActivo"
            class="boton boton--recorrido"
            type="button"
            @click="iniciarRecorrido"
          >
            Iniciar Recorrido
          </button>
          <button class="boton boton--primario" type="button" @click="abrirNuevo">
            Nueva Actividad
          </button>
          <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarActividades">
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <div v-if="recorridoActivo" class="recorrido">
        <span class="recorrido__tiempo">
          ⏱️ Recorrido en progreso: {{ tiempoTranscurrido }}
        </span>
        <button
          class="boton boton--recorrido-finalizar"
          type="button"
          :disabled="completandoId !== null"
          @click="finalizarRecorrido"
        >
          {{ completandoId === recorridoId ? 'Finalizando...' : 'Finalizar Recorrido' }}
        </button>
      </div>

      <p v-if="error" class="alerta">{{ error }}</p>
      <p v-if="errorAccion" class="alerta">{{ errorAccion }}</p>

      <div v-if="cargando" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando actividades...</p>
      </div>

      <div v-else-if="actividades.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay actividades registradas.</p>
      </div>

      <div v-else class="tabla__envoltura">
        <table class="tabla">
          <thead>
            <tr>
              <th class="tabla__th">Analista</th>
              <th class="tabla__th">Actividad</th>
              <th class="tabla__th">Equipo</th>
              <th class="tabla__th">Inicio</th>
              <th class="tabla__th">Fin</th>
              <th class="tabla__th tabla__th--corta">Etiqueta</th>
              <th class="tabla__th tabla__th--corta">Estado</th>
              <th class="tabla__th tabla__th--acciones">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="actividad in actividades" :key="actividad.id" class="tabla__fila">
              <td class="tabla__td">{{ actividad.analistaId }}</td>
              <td class="tabla__td tabla__td--actividad">{{ actividad.descripcionActividad }}</td>
              <td class="tabla__td">{{ actividad.equipoIntervenido || '—' }}</td>
              <td class="tabla__td">{{ formatearFechaHora(actividad.horaInicio) }}</td>
              <td class="tabla__td">{{ formatearFechaHora(actividad.horaFin) }}</td>
              <td class="tabla__td">
                <span class="etiqueta">{{ actividad.etiqueta }}</span>
              </td>
              <td class="tabla__td">
                <span
                  class="badge"
                  :class="actividad.estado === 'Completado' ? 'badge--completado' : 'badge--pendiente'"
                >
                  {{ actividad.estado }}
                </span>
              </td>
              <td class="tabla__td tabla__td--acciones">
                <button
                  v-if="actividad.estado === 'Pendiente'"
                  class="accion accion--completar"
                  type="button"
                  :disabled="completandoId !== null"
                  @click="marcarCompletada(actividad)"
                >
                  {{ completandoId === actividad.id ? 'Completando...' : 'Marcar Completada' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div v-if="mostrarModal" class="modal" @click.self="cerrarModal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-bitacora-titulo">
        <form @submit.prevent="guardarActividad">
          <header class="modal__cabecera">
            <h3 id="modal-bitacora-titulo" class="modal__titulo">Nueva Actividad</h3>
            <button
              class="modal__cerrar"
              type="button"
              aria-label="Cerrar"
              :disabled="guardando"
              @click="cerrarModal"
            >
              &times;
            </button>
          </header>

          <div class="modal__cuerpo">
            <div class="campo">
              <label class="campo__etiqueta" for="bitacora-analista">Analista</label>
              <input
                id="bitacora-analista"
                v-model="formulario.analistaId"
                class="campo__input"
                type="text"
                placeholder="Nombre del técnico"
                maxlength="50"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="bitacora-actividad">Actividad</label>
              <textarea
                id="bitacora-actividad"
                v-model="formulario.descripcionActividad"
                class="campo__input campo__textarea"
                rows="3"
                maxlength="2000"
                placeholder="Describe la tarea realizada"
                :disabled="guardando"
              ></textarea>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="bitacora-equipo">Equipo Intervenido</label>
              <input
                id="bitacora-equipo"
                v-model="formulario.equipoIntervenido"
                class="campo__input"
                type="text"
                placeholder="Equipo o impresora (opcional)"
                maxlength="150"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="bitacora-etiqueta">Etiqueta</label>
              <select
                id="bitacora-etiqueta"
                v-model="formulario.etiqueta"
                class="campo__input"
                :disabled="guardando"
              >
                <option v-for="etiqueta in ETIQUETAS_DISPONIBLES" :key="etiqueta" :value="etiqueta">
                  {{ etiqueta }}
                </option>
              </select>
            </div>

            <p v-if="errorFormulario" class="alerta alerta--formulario">{{ errorFormulario }}</p>
          </div>

          <footer class="modal__pie">
            <button class="boton boton--secundario" type="button" :disabled="guardando" @click="cerrarModal">
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardando">
              {{ guardando ? 'Guardando...' : 'Registrar' }}
            </button>
          </footer>
        </form>
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

.tarjeta__acciones {
  display: flex;
  align-items: center;
  gap: 10px;
}

.tabla__envoltura {
  padding: 8px 20px 20px;
  overflow-x: auto;
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

.boton:disabled {
  cursor: not-allowed;
  opacity: 0.6;
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

.boton--recorrido {
  color: #1f3a68;
  background: #eff8ff;
  border-color: #b2ddff;
}

.boton--recorrido:hover:not(:disabled) {
  background: #d1e9ff;
  border-color: #84caff;
}

.boton--recorrido-finalizar {
  color: #ffffff;
  background: #b42318;
  border-color: #b42318;
}

.boton--recorrido-finalizar:hover:not(:disabled) {
  background: #912018;
}

.recorrido {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 20px;
  color: #ffffff;
  background: #1f3a68;
  border-bottom: 1px solid #eceff3;
}

.recorrido__tiempo {
  font-size: 15px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
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

.alerta--formulario {
  margin: 0;
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

.tabla__th--corta,
.tabla__th--acciones {
  width: 1%;
  white-space: nowrap;
}

.tabla__fila:hover {
  background: #f9fafb;
}

.tabla__td {
  padding: 12px 12px;
  font-size: 13px;
  color: #344054;
  border-bottom: 1px solid #f2f4f7;
}

.tabla__fila:last-child .tabla__td {
  border-bottom: none;
}

.tabla__td--actividad {
  font-weight: 500;
  color: #1a1f2b;
}

.tabla__td--acciones {
  text-align: right;
  white-space: nowrap;
}

.etiqueta {
  display: inline-block;
  padding: 3px 10px;
  font-size: 12px;
  font-weight: 500;
  color: #344054;
  background: #f2f4f7;
  border: 1px solid #e2e5ea;
  border-radius: 999px;
  white-space: nowrap;
}

.badge {
  display: inline-block;
  padding: 3px 10px;
  font-size: 12px;
  font-weight: 600;
  border: 1px solid transparent;
  border-radius: 999px;
  white-space: nowrap;
}

.badge--pendiente {
  color: #b54708;
  background: #fffaeb;
  border-color: #fec84b;
}

.badge--completado {
  color: #067647;
  background: #ecfdf3;
  border-color: #abefc6;
}

.accion {
  padding: 5px 12px;
  font-size: 12px;
  font-weight: 500;
  font-family: inherit;
  border-radius: 6px;
  cursor: pointer;
}

.accion:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.accion--completar {
  color: #067647;
  background: #ecfdf3;
  border: 1px solid #abefc6;
}

.accion--completar:hover:not(:disabled) {
  background: #d1fadf;
  border-color: #6ce9a6;
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
  max-width: 460px;
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

.modal__cerrar:hover:not(:disabled) {
  color: #1a1f2b;
  background: #f2f4f7;
}

.modal__cuerpo {
  padding: 18px 20px;
}

.modal__pie {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 14px 20px;
  background: #f9fafb;
  border-top: 1px solid #eceff3;
}

.campo {
  margin-bottom: 16px;
}

.campo__etiqueta {
  display: block;
  margin-bottom: 6px;
  font-size: 13px;
  font-weight: 500;
  color: #344054;
}

.campo__input {
  width: 100%;
  padding: 9px 12px;
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
  border-color: #1f3a68;
  box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12);
}

.campo__input:disabled {
  background: #f2f4f7;
  cursor: not-allowed;
}

.campo__textarea {
  resize: vertical;
  min-height: 74px;
}
</style>