<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import { confirmar } from '../composables/useConfirm'

const API_URL = 'http://localhost:5141/api/suministros'

const TOKEN_KEY = 'sgcti_token'

const TIPOS_SUGERIDOS = ['Tóner', 'Tambor', 'Papel', 'Repuesto']

interface Suministro {
  id: number
  nombre: string
  tipo: string
  cantidadActual: number
  stockMinimo: number
  observacion: string | null
}

interface ProblemaDeValidacion {
  title?: string
  errors?: Record<string, string[]>
}

const router = useRouter()

const suministros = ref<Suministro[]>([])
const cargando = ref(true)
const error = ref('')

const searchQuery = ref('')
const filtroTipo = ref('')

const tiposDisponibles = computed(() => {
  const tipos = new Set<string>()

  for (const suministro of suministros.value) {
    if (suministro.tipo.trim()) {
      tipos.add(suministro.tipo.trim())
    }
  }

  return Array.from(tipos).sort((a, b) => a.localeCompare(b))
})

const suministrosFiltrados = computed(() => {
  const consulta = searchQuery.value.trim().toLowerCase()

  return suministros.value.filter((suministro) => {
    const coincideTipo = filtroTipo.value === '' || suministro.tipo === filtroTipo.value
    const coincideNombre = consulta === '' || suministro.nombre.toLowerCase().includes(consulta)

    return coincideTipo && coincideNombre
  })
})

const mostrarModal = ref(false)
const modoEdicion = ref(false)
const editandoId = ref<number | null>(null)
const guardando = ref(false)
const errorFormulario = ref('')

const formulario = reactive({
  nombre: '',
  tipo: '',
  cantidadActual: 0,
  stockMinimo: 0,
  observacion: '',
})

const eliminandoId = ref<number | null>(null)
const errorAccion = ref('')

function enCritico(suministro: Suministro): boolean {
  return suministro.cantidadActual <= suministro.stockMinimo
}

function mensajeDeError(e: unknown, respaldo: string): string {
  if (!axios.isAxiosError(e)) {
    return respaldo
  }

  const datos = e.response?.data as ProblemaDeValidacion | string | null | undefined

  if (typeof datos === 'string' && datos.trim()) {
    return datos
  }

  if (datos && typeof datos === 'object') {
    const detalle = Object.values(datos.errors ?? {}).flat().join(' ')

    if (detalle) {
      return detalle
    }

    if (datos.title) {
      return datos.title
    }
  }

  return respaldo
}

async function cargarSuministros() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Suministro[]>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    suministros.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    error.value = 'No se pudieron cargar los suministros. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

function abrirNuevo() {
  modoEdicion.value = false
  editandoId.value = null
  formulario.nombre = ''
  formulario.tipo = ''
  formulario.cantidadActual = 0
  formulario.stockMinimo = 0
  formulario.observacion = ''
  errorFormulario.value = ''
  mostrarModal.value = true
}

function abrirEditar(suministro: Suministro) {
  modoEdicion.value = true
  editandoId.value = suministro.id
  formulario.nombre = suministro.nombre
  formulario.tipo = suministro.tipo
  formulario.cantidadActual = suministro.cantidadActual
  formulario.stockMinimo = suministro.stockMinimo
  formulario.observacion = suministro.observacion ?? ''
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
  const nombre = formulario.nombre.trim()
  const tipo = formulario.tipo.trim()

  if (nombre.length < 2) {
    return 'El nombre es obligatorio y debe tener al menos 2 caracteres.'
  }

  if (nombre.length > 150) {
    return 'El nombre no puede exceder 150 caracteres.'
  }

  if (tipo.length < 2) {
    return 'El tipo es obligatorio y debe tener al menos 2 caracteres.'
  }

  if (tipo.length > 50) {
    return 'El tipo no puede exceder 50 caracteres.'
  }

  if (Number(formulario.cantidadActual) < 0) {
    return 'La cantidad actual no puede ser negativa.'
  }

  if (Number(formulario.stockMinimo) < 0) {
    return 'El stock mínimo no puede ser negativo.'
  }

  return ''
}

async function guardarSuministro() {
  errorFormulario.value = ''

  const fallo = validarFormulario()

  if (fallo) {
    errorFormulario.value = fallo
    return
  }

  guardando.value = true

  const payload = {
    nombre: formulario.nombre.trim(),
    tipo: formulario.tipo.trim(),
    cantidadActual: Number(formulario.cantidadActual),
    stockMinimo: Number(formulario.stockMinimo),
    observacion: formulario.observacion.trim() || null,
  }

  try {
    if (modoEdicion.value && editandoId.value !== null) {
      await axios.put(`${API_URL}/${editandoId.value}`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    } else {
      await axios.post(API_URL, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    }

    cerrarModal()
    await cargarSuministros()
    toast.success('Suministro guardado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormulario.value = mensajeDeError(e, 'No se pudo guardar el suministro. Intenta nuevamente.')
  } finally {
    guardando.value = false
  }
}

async function eliminarSuministro(suministro: Suministro) {
  errorAccion.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar el suministro "${suministro.nombre}"? Esta acción no se puede deshacer.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoId.value = suministro.id

  try {
    await axios.delete(`${API_URL}/${suministro.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarSuministros()
    toast.success('Suministro eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = mensajeDeError(e, 'No se pudo eliminar el suministro. Intenta nuevamente.')
  } finally {
    eliminandoId.value = null
  }
}

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
  cargarSuministros()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Inventario / Suministros</h1>
      <p class="vista__subtitulo">Control de existencias, stock mínimo y costos</p>
    </header>

    <section class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Suministros registrados</h2>
        <div class="tarjeta__acciones">
          <button class="boton boton--primario" type="button" @click="abrirNuevo">
            Nuevo Suministro
          </button>
          <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarSuministros">
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <div class="filtros">
        <input
          v-model="searchQuery"
          class="filtros__busqueda"
          type="search"
          placeholder="Buscar por nombre..."
          autocomplete="off"
        />
        <select v-model="filtroTipo" class="filtros__select">
          <option value="">Todos los tipos</option>
          <option v-for="tipo in tiposDisponibles" :key="tipo" :value="tipo">{{ tipo }}</option>
        </select>
        <span v-if="!cargando" class="filtros__conteo">
          {{ suministrosFiltrados.length }} de {{ suministros.length }}
        </span>
      </div>

      <p v-if="error" class="alerta">{{ error }}</p>
      <p v-if="errorAccion" class="alerta">{{ errorAccion }}</p>

      <div v-if="cargando" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando suministros...</p>
      </div>

      <div v-else-if="suministros.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay suministros registrados.</p>
      </div>

      <div v-else-if="suministrosFiltrados.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay suministros que coincidan con la búsqueda.</p>
      </div>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th class="tabla__th tabla__th--corta">Id</th>
            <th class="tabla__th">Nombre</th>
            <th class="tabla__th tabla__th--corta">Tipo</th>
            <th class="tabla__th tabla__th--corta">Cantidad Actual</th>
            <th class="tabla__th tabla__th--corta">Stock Mínimo</th>
            <th class="tabla__th tabla__th--acciones">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="suministro in suministrosFiltrados"
            :key="suministro.id"
            class="tabla__fila"
            :class="{ 'tabla__fila--critica': enCritico(suministro) }"
          >
            <td class="tabla__td tabla__td--id">{{ suministro.id }}</td>
            <td class="tabla__td tabla__td--nombre">
              {{ suministro.nombre }}
              <span v-if="suministro.observacion" class="tabla__td--observacion">
                {{ suministro.observacion }}
              </span>
            </td>
            <td class="tabla__td">
              <span class="etiqueta">{{ suministro.tipo }}</span>
            </td>
            <td class="tabla__td" :class="{ 'tabla__td--critico': enCritico(suministro) }">
              <span class="badge" :class="enCritico(suministro) ? 'badge--peligro' : 'badge--normal'">
                {{ suministro.cantidadActual }}
                <template v-if="enCritico(suministro)">· Crítico</template>
              </span>
            </td>
            <td class="tabla__td">{{ suministro.stockMinimo }}</td>
            <td class="tabla__td tabla__td--acciones">
              <button
                class="accion accion--editar"
                type="button"
                :disabled="eliminandoId !== null"
                @click="abrirEditar(suministro)"
              >
                Editar
              </button>
              <button
                class="accion accion--eliminar"
                type="button"
                :disabled="eliminandoId !== null"
                @click="eliminarSuministro(suministro)"
              >
                {{ eliminandoId === suministro.id ? 'Eliminando...' : 'Eliminar' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="mostrarModal" class="modal" @click.self="cerrarModal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-suministro-titulo">
        <form @submit.prevent="guardarSuministro">
          <header class="modal__cabecera">
            <h3 id="modal-suministro-titulo" class="modal__titulo">
              {{ modoEdicion ? 'Editar Suministro' : 'Nuevo Suministro' }}
            </h3>
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
              <label class="campo__etiqueta" for="suministro-nombre">Nombre</label>
              <input
                id="suministro-nombre"
                v-model="formulario.nombre"
                class="campo__input"
                type="text"
                placeholder="Ej. Tóner HP 26A"
                maxlength="150"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="suministro-tipo">Tipo</label>
              <input
                id="suministro-tipo"
                v-model="formulario.tipo"
                class="campo__input"
                type="text"
                list="suministro-tipos"
                placeholder="Tóner, Tambor, Papel o Repuesto"
                maxlength="50"
                :disabled="guardando"
              />
              <datalist id="suministro-tipos">
                <option v-for="tipo in TIPOS_SUGERIDOS" :key="tipo" :value="tipo"></option>
              </datalist>
            </div>

            <div class="campo campo--rejilla">
              <div>
                <label class="campo__etiqueta" for="suministro-cantidad">Cantidad Actual</label>
                <input
                  id="suministro-cantidad"
                  v-model.number="formulario.cantidadActual"
                  class="campo__input"
                  type="number"
                  min="0"
                  step="1"
                  :disabled="guardando"
                />
              </div>
              <div>
                <label class="campo__etiqueta" for="suministro-stock">Stock Mínimo</label>
                <input
                  id="suministro-stock"
                  v-model.number="formulario.stockMinimo"
                  class="campo__input"
                  type="number"
                  min="0"
                  step="1"
                  :disabled="guardando"
                />
              </div>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="suministro-observacion">Observación</label>
              <textarea
                id="suministro-observacion"
                v-model="formulario.observacion"
                class="campo__input campo__textarea"
                rows="3"
                maxlength="500"
                placeholder="Notas, ubicación o referencia del suministro (opcional)"
                :disabled="guardando"
              ></textarea>
            </div>

            <p class="nota">
              El suministro se marcará como crítico cuando la cantidad actual sea igual o menor
              al stock mínimo.
            </p>

            <p v-if="errorFormulario" class="alerta alerta--formulario">{{ errorFormulario }}</p>
          </div>

          <footer class="modal__pie">
            <button class="boton boton--secundario" type="button" :disabled="guardando" @click="cerrarModal">
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardando">
              {{ guardando ? 'Guardando...' : modoEdicion ? 'Guardar Cambios' : 'Guardar' }}
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

.filtros {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
  padding: 12px 20px;
  border-bottom: 1px solid #eceff3;
  background: #f9fafb;
}

.filtros__busqueda {
  flex: 1 1 220px;
  min-width: 0;
  padding: 8px 12px;
  font-size: 13px;
  font-family: inherit;
  color: #1a1f2b;
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 6px;
  box-sizing: border-box;
}

.filtros__busqueda:focus {
  outline: none;
  border-color: #1f3a68;
  box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12);
}

.filtros__select {
  padding: 8px 12px;
  font-size: 13px;
  font-family: inherit;
  color: #1a1f2b;
  background: #ffffff;
  border: 1px solid #d0d5dd;
  border-radius: 6px;
  cursor: pointer;
}

.filtros__conteo {
  margin-left: auto;
  font-size: 12px;
  color: #667085;
  white-space: nowrap;
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
.tabla__th--acciones {
  width: 1%;
  white-space: nowrap;
}

.tabla__fila:hover {
  background: #f9fafb;
}

.tabla__fila--critica,
.tabla__fila--critica:hover {
  background: #fef3f2;
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

.tabla__td--nombre {
  font-weight: 500;
  color: #1a1f2b;
}

.tabla__td--observacion {
  display: block;
  margin-top: 2px;
  font-size: 12px;
  font-weight: 400;
  color: #667085;
}

.tabla__td--critico {
  font-weight: 600;
  color: #b42318;
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
  font-variant-numeric: tabular-nums;
  border: 1px solid transparent;
  border-radius: 999px;
  white-space: nowrap;
}

.badge--normal {
  color: #344054;
  background: #f2f4f7;
  border-color: #e2e5ea;
}

.badge--peligro {
  color: #ffffff;
  background: #f04438;
  border-color: #f04438;
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

.accion--editar {
  color: #1f3a68;
  background: #eff8ff;
  border: 1px solid #b2ddff;
}

.accion--editar:hover:not(:disabled) {
  background: #d1e9ff;
  border-color: #84caff;
}

.accion--eliminar {
  margin-left: 8px;
  color: #b42318;
  background: #fef3f2;
  border: 1px solid #fecdca;
}

.accion--eliminar:hover:not(:disabled) {
  background: #fee4e2;
  border-color: #fda29b;
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

.campo--rejilla {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
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

.nota {
  margin: 0;
  padding: 8px 10px;
  font-size: 12px;
  color: #667085;
  background: #f9fafb;
  border: 1px solid #eceff3;
  border-radius: 6px;
}
</style>
