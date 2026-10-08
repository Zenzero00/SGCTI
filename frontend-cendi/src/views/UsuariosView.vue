<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import { confirmar } from '../composables/useConfirm'

const API_URL = 'http://localhost:5141/api/impresoras'

const TOKEN_KEY = 'sgcti_token'

const ROLES = ['Admin', 'Tecnico'] as const

interface Usuario {
  id: number
  nombreCompleto: string
  username: string
  rol: string
  activo: boolean
}

interface Equipo {
  id: number
  modelo: string
  ip: string
  departamento: string
  comunidadSnmp: string
}

interface ProblemaDeValidacion {
  title?: string
  errors?: Record<string, string[]>
}

const router = useRouter()

const pestanaActiva = ref<'usuarios' | 'equipos'>('usuarios')

const usuarios = ref<Usuario[]>([])
const cargando = ref(true)
const error = ref('')

const searchQuery = ref('')

const usuariosFiltrados = computed(() => {
  const consulta = searchQuery.value.trim().toLowerCase()

  if (consulta === '') {
    return usuarios.value
  }

  return usuarios.value.filter(
    (usuario) =>
      usuario.nombreCompleto.toLowerCase().includes(consulta) ||
      usuario.username.toLowerCase().includes(consulta) ||
      usuario.rol.toLowerCase().includes(consulta),
  )
})

const mostrarModal = ref(false)
const modoEdicion = ref(false)
const editandoId = ref<number | null>(null)
const guardando = ref(false)
const errorFormulario = ref('')

const formulario = reactive({
  nombreCompleto: '',
  username: '',
  password: '',
  rol: 'Tecnico',
  activo: true,
})

const eliminandoId = ref<number | null>(null)
const errorAccion = ref('')

const equipos = ref<Equipo[]>([])
const cargandoEquipos = ref(false)
const errorEquipos = ref('')

const mostrarModalEquipo = ref(false)
const modoEdicionEquipo = ref(false)
const editandoEquipoId = ref<number | null>(null)
const guardandoEquipo = ref(false)
const errorFormularioEquipo = ref('')

const formularioEquipo = reactive({
  modelo: '',
  ip: '',
  departamento: '',
  comunidadSnmp: 'public',
})

const eliminandoEquipoId = ref<number | null>(null)
const errorAccionEquipo = ref('')

function cambiarPestana(pestana: 'usuarios' | 'equipos') {
  pestanaActiva.value = pestana

  if (pestana === 'equipos' && equipos.value.length === 0) {
    cargarEquipos()
  }
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

async function cargarUsuarios() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<Usuario[]>('http://localhost:5141/api/usuarios', {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    usuarios.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    error.value = 'No se pudieron cargar los usuarios. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem('nombre_usuario')
  localStorage.removeItem('rol_usuario')
  router.push('/')
}

function abrirNuevo() {
  modoEdicion.value = false
  editandoId.value = null
  formulario.nombreCompleto = ''
  formulario.username = ''
  formulario.password = ''
  formulario.rol = 'Tecnico'
  formulario.activo = true
  errorFormulario.value = ''
  mostrarModal.value = true
}

function abrirEditar(usuario: Usuario) {
  modoEdicion.value = true
  editandoId.value = usuario.id
  formulario.nombreCompleto = usuario.nombreCompleto
  formulario.username = usuario.username
  formulario.password = ''
  formulario.rol = usuario.rol
  formulario.activo = usuario.activo
  errorFormulario.value = ''
  mostrarModal.value = true
}

function cerrarModal() {
  mostrarModal.value = false
  errorFormulario.value = ''
}

function alPulsarEscape(evento: KeyboardEvent) {
  if (evento.key === 'Escape') {
    if (mostrarModal.value && !guardando.value) {
      cerrarModal()
    } else if (mostrarModalEquipo.value && !guardandoEquipo.value) {
      cerrarModalEquipo()
    }
  }
}

function validarFormulario(): string {
  const nombre = formulario.nombreCompleto.trim()
  const username = formulario.username.trim()

  if (nombre.length < 2 || nombre.length > 150) {
    return 'El nombre es obligatorio y debe tener entre 2 y 150 caracteres.'
  }

  if (username.length < 3 || username.length > 50) {
    return 'El usuario es obligatorio y debe tener entre 3 y 50 caracteres.'
  }

  if (!modoEdicion.value && formulario.password.length < 4) {
    return 'La contraseña es obligatoria y debe tener al menos 4 caracteres.'
  }

  if (modoEdicion.value && formulario.password && formulario.password.length < 4) {
    return 'La contraseña debe tener al menos 4 caracteres.'
  }

  if (!ROLES.includes(formulario.rol as (typeof ROLES)[number])) {
    return 'El rol debe ser Admin o Tecnico.'
  }

  return ''
}

async function guardarUsuario() {
  errorFormulario.value = ''

  const fallo = validarFormulario()

  if (fallo) {
    errorFormulario.value = fallo
    return
  }

  guardando.value = true

  const payload = {
    nombreCompleto: formulario.nombreCompleto.trim(),
    username: formulario.username.trim(),
    password: formulario.password || null,
    rol: formulario.rol,
    activo: formulario.activo,
  }

  try {
    if (modoEdicion.value && editandoId.value !== null) {
      await axios.put(`http://localhost:5141/api/usuarios/${editandoId.value}`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    } else {
      await axios.post('http://localhost:5141/api/usuarios', payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    }

    cerrarModal()
    await cargarUsuarios()
    toast.success(modoEdicion.value ? 'Usuario actualizado correctamente.' : 'Usuario creado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormulario.value = mensajeDeError(e, 'No se pudo guardar el usuario. Intenta nuevamente.')
  } finally {
    guardando.value = false
  }
}

async function cambiarEstado(usuario: Usuario) {
  errorAccion.value = ''

  const accion = usuario.activo ? 'desactivar' : 'activar'

  const confirmado = await confirmar({
    mensaje: `¿${accion === 'desactivar' ? 'Desactivar' : 'Activar'} al usuario "${usuario.nombreCompleto}"?`,
    confirmarTexto: accion === 'desactivar' ? 'Desactivar' : 'Activar',
  })

  if (!confirmado) {
    return
  }

  try {
    await axios.put(
      `http://localhost:5141/api/usuarios/${usuario.id}`,
      {
        nombreCompleto: usuario.nombreCompleto,
        username: usuario.username,
        password: null,
        rol: usuario.rol,
        activo: !usuario.activo,
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    await cargarUsuarios()
    toast.success(accion === 'desactivar' ? 'Usuario desactivado correctamente.' : 'Usuario activado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = mensajeDeError(e, 'No se pudo cambiar el estado del usuario.')
  }
}

async function eliminarUsuario(usuario: Usuario) {
  errorAccion.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar al usuario "${usuario.nombreCompleto}"? Esta acción no se puede deshacer.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoId.value = usuario.id

  try {
    await axios.delete(`http://localhost:5141/api/usuarios/${usuario.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarUsuarios()
    toast.success('Usuario eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccion.value = mensajeDeError(e, 'No se pudo eliminar el usuario. Intenta nuevamente.')
  } finally {
    eliminandoId.value = null
  }
}

function abrirNuevoEquipo() {
  modoEdicionEquipo.value = false
  editandoEquipoId.value = null
  formularioEquipo.modelo = ''
  formularioEquipo.ip = ''
  formularioEquipo.departamento = ''
  formularioEquipo.comunidadSnmp = 'public'
  errorFormularioEquipo.value = ''
  mostrarModalEquipo.value = true
}

function abrirEditarEquipo(equipo: Equipo) {
  modoEdicionEquipo.value = true
  editandoEquipoId.value = equipo.id
  formularioEquipo.modelo = equipo.modelo
  formularioEquipo.ip = equipo.ip
  formularioEquipo.departamento = equipo.departamento
  formularioEquipo.comunidadSnmp = equipo.comunidadSnmp || 'public'
  errorFormularioEquipo.value = ''
  mostrarModalEquipo.value = true
}

function cerrarModalEquipo() {
  mostrarModalEquipo.value = false
  errorFormularioEquipo.value = ''
}

function validarFormularioEquipo(): string {
  const modelo = formularioEquipo.modelo.trim()
  const ip = formularioEquipo.ip.trim()
  const departamento = formularioEquipo.departamento.trim()

  if (modelo.length < 2 || modelo.length > 100) {
    return 'El modelo es obligatorio y debe tener entre 2 y 100 caracteres.'
  }

  if (ip.length === 0 || ip.length > 45) {
    return 'La IP es obligatoria y no puede exceder 45 caracteres.'
  }

  if (departamento.length < 2 || departamento.length > 60) {
    return 'El departamento es obligatorio y debe tener entre 2 y 60 caracteres.'
  }

  if (formularioEquipo.comunidadSnmp.trim().length > 100) {
    return 'La comunidad SNMP no puede exceder 100 caracteres.'
  }

  return ''
}

async function cargarEquipos() {
  cargandoEquipos.value = true
  errorEquipos.value = ''

  try {
    const { data } = await axios.get<Equipo[]>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    equipos.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorEquipos.value = 'No se pudieron cargar los equipos. Intenta nuevamente.'
  } finally {
    cargandoEquipos.value = false
  }
}

async function guardarEquipo() {
  errorFormularioEquipo.value = ''

  const fallo = validarFormularioEquipo()

  if (fallo) {
    errorFormularioEquipo.value = fallo
    return
  }

  guardandoEquipo.value = true

  const payload = {
    modelo: formularioEquipo.modelo.trim(),
    ip: formularioEquipo.ip.trim(),
    departamento: formularioEquipo.departamento.trim(),
    comunidadSnmp: formularioEquipo.comunidadSnmp.trim() || 'public',
  }

  try {
    if (modoEdicionEquipo.value && editandoEquipoId.value !== null) {
      await axios.put(`${API_URL}/${editandoEquipoId.value}`, payload, {
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

    cerrarModalEquipo()
    await cargarEquipos()
    toast.success(modoEdicionEquipo.value ? 'Equipo actualizado correctamente.' : 'Equipo creado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormularioEquipo.value = mensajeDeError(e, 'No se pudo guardar el equipo. Intenta nuevamente.')
  } finally {
    guardandoEquipo.value = false
  }
}

async function eliminarEquipo(equipo: Equipo) {
  errorAccionEquipo.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar el equipo "${equipo.modelo}" (${equipo.ip})? Esta acción no se puede deshacer.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoEquipoId.value = equipo.id

  try {
    await axios.delete(`${API_URL}/${equipo.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarEquipos()
    toast.success('Equipo eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccionEquipo.value = mensajeDeError(e, 'No se pudo eliminar el equipo. Intenta nuevamente.')
  } finally {
    eliminandoEquipoId.value = null
  }
}

onMounted(() => {
  window.addEventListener('keydown', alPulsarEscape)
  cargarUsuarios()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Configuración del Sistema</h1>
      <p class="vista__subtitulo">Gestión de usuarios y equipos de impresión (SNMP)</p>
    </header>

    <div class="pestanas" role="tablist">
      <button
        class="pestana"
        type="button"
        :class="{ 'pestana--activa': pestanaActiva === 'usuarios' }"
        @click="cambiarPestana('usuarios')"
      >
        Usuarios
      </button>
      <button
        class="pestana"
        type="button"
        :class="{ 'pestana--activa': pestanaActiva === 'equipos' }"
        @click="cambiarPestana('equipos')"
      >
        Equipos (SNMP)
      </button>
    </div>

    <section v-if="pestanaActiva === 'usuarios'" class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Usuarios registrados</h2>
        <div class="tarjeta__acciones">
          <button class="boton boton--primario" type="button" @click="abrirNuevo">
            Nuevo Usuario
          </button>
          <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarUsuarios">
            {{ cargando ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <div class="filtros">
        <input
          v-model="searchQuery"
          class="filtros__busqueda"
          type="search"
          placeholder="Buscar por nombre, usuario o rol..."
          autocomplete="off"
        />
        <span v-if="!cargando" class="filtros__conteo">
          {{ usuariosFiltrados.length }} de {{ usuarios.length }}
        </span>
      </div>

      <p v-if="error" class="alerta">{{ error }}</p>
      <p v-if="errorAccion" class="alerta">{{ errorAccion }}</p>

      <div v-if="cargando" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando usuarios...</p>
      </div>

      <div v-else-if="usuarios.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay usuarios registrados.</p>
      </div>

      <div v-else-if="usuariosFiltrados.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay usuarios que coincidan con la búsqueda.</p>
      </div>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th class="tabla__th">Nombre</th>
            <th class="tabla__th">Usuario</th>
            <th class="tabla__th tabla__th--corta">Rol</th>
            <th class="tabla__th tabla__th--corta">Estado</th>
            <th class="tabla__th tabla__th--acciones">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="usuario in usuariosFiltrados" :key="usuario.id" class="tabla__fila">
            <td class="tabla__td tabla__td--nombre">{{ usuario.nombreCompleto }}</td>
            <td class="tabla__td">{{ usuario.username }}</td>
            <td class="tabla__td">
              <span class="etiqueta" :class="usuario.rol === 'Admin' ? 'etiqueta--admin' : ''">
                {{ usuario.rol }}
              </span>
            </td>
            <td class="tabla__td">
              <span class="badge" :class="usuario.activo ? 'badge--activo' : 'badge--inactivo'">
                {{ usuario.activo ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td class="tabla__td tabla__td--acciones">
              <button
                class="accion accion--editar"
                type="button"
                :disabled="eliminandoId !== null"
                @click="abrirEditar(usuario)"
              >
                Editar
              </button>
              <button
                class="accion"
                :class="usuario.activo ? 'accion--desactivar' : 'accion--activar'"
                type="button"
                :disabled="eliminandoId !== null || usuario.username === 'admin'"
                @click="cambiarEstado(usuario)"
              >
                {{ usuario.activo ? 'Desactivar' : 'Activar' }}
              </button>
              <button
                class="accion accion--eliminar"
                type="button"
                :disabled="eliminandoId !== null || usuario.username === 'admin'"
                @click="eliminarUsuario(usuario)"
              >
                {{ eliminandoId === usuario.id ? 'Eliminando...' : 'Eliminar' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <section v-else class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Equipos registrados (SNMP)</h2>
        <div class="tarjeta__acciones">
          <button class="boton boton--primario" type="button" @click="abrirNuevoEquipo">
            Nuevo Equipo
          </button>
          <button
            class="boton boton--secundario"
            type="button"
            :disabled="cargandoEquipos"
            @click="cargarEquipos"
          >
            {{ cargandoEquipos ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <p v-if="errorEquipos" class="alerta">{{ errorEquipos }}</p>
      <p v-if="errorAccionEquipo" class="alerta">{{ errorAccionEquipo }}</p>

      <div v-if="cargandoEquipos" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando equipos...</p>
      </div>

      <div v-else-if="equipos.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay equipos registrados. Usa 'Nuevo Equipo' para agregar uno.</p>
      </div>

      <table v-else class="tabla">
        <thead>
          <tr>
            <th class="tabla__th tabla__th--corta">ID</th>
            <th class="tabla__th">Modelo</th>
            <th class="tabla__th">IP</th>
            <th class="tabla__th">Departamento</th>
            <th class="tabla__th tabla__th--acciones">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="equipo in equipos" :key="equipo.id" class="tabla__fila">
            <td class="tabla__td tabla__td--id">{{ equipo.id }}</td>
            <td class="tabla__td tabla__td--nombre">{{ equipo.modelo }}</td>
            <td class="tabla__td tabla__td--ip">{{ equipo.ip }}</td>
            <td class="tabla__td">{{ equipo.departamento }}</td>
            <td class="tabla__td tabla__td--acciones">
              <button
                class="accion accion--editar"
                type="button"
                :disabled="eliminandoEquipoId !== null"
                @click="abrirEditarEquipo(equipo)"
              >
                Editar
              </button>
              <button
                class="accion accion--eliminar"
                type="button"
                :disabled="eliminandoEquipoId !== null"
                @click="eliminarEquipo(equipo)"
              >
                {{ eliminandoEquipoId === equipo.id ? 'Eliminando...' : 'Eliminar' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <div v-if="mostrarModal" class="modal" @click.self="cerrarModal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-usuario-titulo">
        <form @submit.prevent="guardarUsuario">
          <header class="modal__cabecera">
            <h3 id="modal-usuario-titulo" class="modal__titulo">
              {{ modoEdicion ? 'Editar Usuario' : 'Nuevo Usuario' }}
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
              <label class="campo__etiqueta" for="usuario-nombre">Nombre Completo</label>
              <input
                id="usuario-nombre"
                v-model="formulario.nombreCompleto"
                class="campo__input"
                type="text"
                placeholder="Ej. Juan Pérez"
                maxlength="150"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="usuario-username">Nombre de Usuario</label>
              <input
                id="usuario-username"
                v-model="formulario.username"
                class="campo__input"
                type="text"
                placeholder="Ej. jperez"
                maxlength="50"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="usuario-password">
                Contraseña {{ modoEdicion ? '(dejar vacía para no cambiarla)' : '' }}
              </label>
              <input
                id="usuario-password"
                v-model="formulario.password"
                class="campo__input"
                type="password"
                placeholder="••••••••"
                maxlength="200"
                autocomplete="new-password"
                :disabled="guardando"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="usuario-rol">Rol</label>
              <select id="usuario-rol" v-model="formulario.rol" class="campo__input" :disabled="guardando">
                <option v-for="rol in ROLES" :key="rol" :value="rol">{{ rol }}</option>
              </select>
            </div>

            <label class="interruptor">
              <input v-model="formulario.activo" type="checkbox" :disabled="guardando" />
              <span>Cuenta activa</span>
            </label>

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

    <div v-if="mostrarModalEquipo" class="modal" @click.self="cerrarModalEquipo">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-equipo-titulo">
        <form @submit.prevent="guardarEquipo">
          <header class="modal__cabecera">
            <h3 id="modal-equipo-titulo" class="modal__titulo">
              {{ modoEdicionEquipo ? 'Editar Equipo' : 'Nuevo Equipo' }}
            </h3>
            <button
              class="modal__cerrar"
              type="button"
              aria-label="Cerrar"
              :disabled="guardandoEquipo"
              @click="cerrarModalEquipo"
            >
              &times;
            </button>
          </header>

          <div class="modal__cuerpo">
            <div class="campo">
              <label class="campo__etiqueta" for="equipo-modelo">Modelo</label>
              <input
                id="equipo-modelo"
                v-model="formularioEquipo.modelo"
                class="campo__input"
                type="text"
                placeholder="Ej. HP LaserJet Pro M404"
                maxlength="100"
                :disabled="guardandoEquipo"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-ip">IP</label>
              <input
                id="equipo-ip"
                v-model="formularioEquipo.ip"
                class="campo__input"
                type="text"
                placeholder="Ej. 192.168.1.50"
                maxlength="45"
                :disabled="guardandoEquipo"
              />
              <p v-if="modoEdicionEquipo && formularioEquipo.ip.startsWith('importado-')" class="nota">
                Esta es una IP temporal del importador. Reemplázala por la IP real del equipo.
              </p>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-departamento">Departamento</label>
              <input
                id="equipo-departamento"
                v-model="formularioEquipo.departamento"
                class="campo__input"
                type="text"
                placeholder="Ej. Facturación"
                maxlength="60"
                :disabled="guardandoEquipo"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-comunidad">Comunidad SNMP (Contraseña)</label>
              <input
                id="equipo-comunidad"
                v-model="formularioEquipo.comunidadSnmp"
                class="campo__input"
                type="text"
                placeholder="Ej. public"
                maxlength="100"
                :disabled="guardandoEquipo"
              />
            </div>

            <p v-if="errorFormularioEquipo" class="alerta alerta--formulario">{{ errorFormularioEquipo }}</p>
          </div>

          <footer class="modal__pie">
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="guardandoEquipo"
              @click="cerrarModalEquipo"
            >
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardandoEquipo">
              {{ guardandoEquipo ? 'Guardando...' : modoEdicionEquipo ? 'Guardar Cambios' : 'Guardar' }}
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

.pestanas {
  display: flex;
  gap: 4px;
  margin-bottom: 16px;
  padding: 5px;
  background: #e6e9ee;
  border-radius: 8px;
}

.pestana {
  flex: 1 1 0;
  padding: 9px 14px;
  font-size: 13px;
  font-weight: 500;
  font-family: inherit;
  color: #667085;
  background: transparent;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.pestana:hover {
  color: #1a1f2b;
}

.pestana--activa {
  color: #1f3a68;
  background: #ffffff;
  box-shadow: 0 1px 3px rgba(16, 24, 40, 0.12);
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

.tabla__td--ip {
  font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
  font-size: 13px;
  color: #475467;
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

.etiqueta--admin {
  color: #1f3a68;
  background: #eff8ff;
  border-color: #b2ddff;
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

.badge--activo {
  color: #067647;
  background: #ecfdf3;
  border-color: #a6f4c5;
}

.badge--inactivo {
  color: #b42318;
  background: #fef3f2;
  border-color: #fecdca;
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

.accion--activar {
  margin-left: 8px;
  color: #067647;
  background: #ecfdf3;
  border: 1px solid #a6f4c5;
}

.accion--activar:hover:not(:disabled) {
  background: #d1fadf;
  border-color: #6ce9a6;
}

.accion--desactivar {
  margin-left: 8px;
  color: #b54708;
  background: #fffaeb;
  border: 1px solid #fec84b;
}

.accion--desactivar:hover:not(:disabled) {
  background: #fef0c7;
  border-color: #f79009;
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
  max-height: calc(100vh - 48px);
  overflow-y: auto;
  background: #ffffff;
  border-radius: 10px;
  box-shadow: 0 20px 40px rgba(16, 24, 40, 0.22);
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

.nota {
  margin-top: 6px;
  padding: 8px 10px;
  font-size: 12px;
  color: #b54708;
  background: #fffaeb;
  border: 1px solid #fec84b;
  border-radius: 6px;
}

.interruptor {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 500;
  color: #344054;
  cursor: pointer;
}
</style>