<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios, { type AxiosError } from 'axios'

const API_URL = 'http://localhost:5141/api/Tickets'

const TOKEN_KEY = 'sgcti_token'

const ESTADOS = ['Abierto', 'En Progreso', 'Resuelto'] as const
const PRIORIDADES = ['Baja', 'Media', 'Alta', 'Crítica'] as const

interface Ticket {
  id: number
  titulo: string
  descripcion?: string
  estado: number | string
  prioridad: number | string
  fechaCreacion?: string
  fechaCierre?: string | null
  slaHoras?: number
}

interface ProblemaDeValidacion {
  title?: string
  errors?: Record<string, string[]>
}

interface Fila {
  id: number
  titulo: string
  estado: string
  estadoClase: string
  prioridad: string
  prioridadClase: string
}

const router = useRouter()

const filas = ref<Fila[]>([])
const cargando = ref(true)
const error = ref('')

const formulario = reactive({
  titulo: '',
  descripcion: '',
})

const mostrarFormulario = ref(false)
const guardando = ref(false)
const errorFormulario = ref('')

const resolviendoId = ref<number | null>(null)
const errorAccion = ref('')

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

async function cargarTickets() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Ticket[]>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    filas.value = data.map((ticket) => {
      const estado = normalizar(ticket.estado, ESTADOS)
      const prioridad = normalizar(ticket.prioridad, PRIORIDADES)

      return {
        id: ticket.id,
        titulo: ticket.titulo,
        estado: estado.texto,
        estadoClase: estado.clase,
        prioridad: prioridad.texto,
        prioridadClase: prioridad.clase,
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

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

function abrirFormulario() {
  errorFormulario.value = ''
  mostrarFormulario.value = true
}

function cerrarFormulario() {
  mostrarFormulario.value = false
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
        Estado: 0,
        Prioridad: 1,
        SLAHoras: 24,
        FechaCreacion: new Date().toISOString(),
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    formulario.titulo = ''
    formulario.descripcion = ''
    mostrarFormulario.value = false

    await cargarTickets()
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

async function resolverTicket(fila: Fila) {
  errorAccion.value = ''
  resolviendoId.value = fila.id

  try {
    const { data: ticket } = await axios.get<Ticket>(`${API_URL}/${fila.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await axios.put(
      `${API_URL}/${fila.id}`,
      { ...ticket, estado: 2, fechaCierre: new Date().toISOString() },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    await cargarTickets()
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    const fallo = e as AxiosError<ProblemaDeValidacion>

    errorAccion.value =
      fallo.response?.data?.title + ' - ' + JSON.stringify(fallo.response?.data?.errors) || fallo.message
  } finally {
    resolviendoId.value = null
  }
}

onMounted(cargarTickets)
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Mesa de Ayuda</h1>
      <p class="vista__subtitulo">Control de Suministros y Gestión de los Servicios TI</p>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Tickets registrados</h2>
        <div class="tarjeta__acciones">
          <button class="boton boton--primario" type="button" @click="abrirFormulario">
            Nuevo Ticket
          </button>
          <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarTickets">
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <form v-if="mostrarFormulario" class="formulario" @submit.prevent="crearTicket">
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
          <button class="boton boton--secundario" type="button" :disabled="guardando" @click="cerrarFormulario">
            Cancelar
          </button>
        </div>
      </form>

      <p v-if="error" class="alerta">{{ error }}</p>
      <p v-if="errorAccion" class="alerta">{{ errorAccion }}</p>

      <div v-if="cargando" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando tickets...</p>
      </div>

      <div v-else-if="filas.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay tickets registrados.</p>
      </div>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th class="tabla__th tabla__th--id">Id</th>
            <th class="tabla__th">Título</th>
            <th class="tabla__th tabla__th--estado">Estado</th>
            <th class="tabla__th tabla__th--prioridad">Prioridad</th>
            <th class="tabla__th tabla__th--acciones">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="fila in filas" :key="fila.id" class="tabla__fila">
            <td class="tabla__td tabla__td--id">{{ fila.id }}</td>
            <td class="tabla__td tabla__td--titulo">{{ fila.titulo }}</td>
            <td class="tabla__td">
              <span class="etiqueta" :class="`etiqueta--${fila.estadoClase}`">{{ fila.estado }}</span>
            </td>
            <td class="tabla__td">
              <span class="etiqueta" :class="`etiqueta--${fila.prioridadClase}`">{{ fila.prioridad }}</span>
            </td>
            <td class="tabla__td tabla__td--acciones">
              <button
                v-if="fila.estadoClase !== 'resuelto'"
                class="resolver"
                type="button"
                :disabled="resolviendoId !== null"
                @click="resolverTicket(fila)"
              >
                {{ resolviendoId === fila.id ? 'Resolviendo...' : 'Resolver' }}
              </button>
              <span v-else class="tabla__sin-accion">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
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

.formulario {
  padding: 20px;
  background: #f9fafb;
  border-bottom: 1px solid #e2e5ea;
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

.campo__input--area {
  resize: vertical;
}

.formulario__acciones {
  display: flex;
  align-items: center;
  gap: 10px;
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
  margin: 0 0 16px;
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

.tabla__th--id,
.tabla__th--estado,
.tabla__th--prioridad,
.tabla__th--acciones {
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

.tabla__td--titulo {
  font-weight: 500;
  color: #1a1f2b;
}

.tabla__td--acciones {
  text-align: right;
}

.tabla__sin-accion {
  color: #98a2b3;
}

.resolver {
  padding: 5px 12px;
  font-size: 12px;
  font-weight: 500;
  font-family: inherit;
  color: #067647;
  background: #ecfdf3;
  border: 1px solid #abefc6;
  border-radius: 6px;
  cursor: pointer;
}

.resolver:hover:not(:disabled) {
  background: #d3f8e0;
  border-color: #6ce9a6;
}

.resolver:disabled {
  color: #98a2b3;
  background: #f2f4f7;
  border-color: #e2e5ea;
  cursor: not-allowed;
}

.etiqueta {
  display: inline-block;
  padding: 3px 10px;
  font-size: 12px;
  font-weight: 500;
  border: 1px solid transparent;
  border-radius: 999px;
  white-space: nowrap;
}

.etiqueta--abierto,
.etiqueta--en-progreso,
.etiqueta--resuelto,
.etiqueta--baja,
.etiqueta--media,
.etiqueta--alta,
.etiqueta--critica {
  background: #f2f4f7;
  border-color: #e2e5ea;
  color: #344054;
}

.etiqueta--en-progreso {
  background: #eff8ff;
  border-color: #b2ddff;
  color: #175cd3;
}

.etiqueta--resuelto {
  background: #ecfdf3;
  border-color: #abefc6;
  color: #067647;
}

.etiqueta--alta {
  background: #fffaeb;
  border-color: #fedf89;
  color: #b54708;
}

.etiqueta--critica {
  background: #fef3f2;
  border-color: #fecdca;
  color: #b42318;
}
</style>
