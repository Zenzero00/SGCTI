<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { toast } from 'vue3-toastify'
import { confirmar } from '../composables/useConfirm'
import { API_BASE_URL } from '../config'
import { obtenerCategorias, obtenerDepartamentos, type CategoriaTicket, type Departamento } from '../services/catalogos'

const API_BASE = API_BASE_URL
const API_URL = `${API_BASE}/api/impresoras`

const TOKEN_KEY = 'sgcti_token'

const ROLES = ['Admin', 'Ingeniero', 'Técnico'] as const
const PRIORIDADES_DEPARTAMENTO = ['Baja', 'Media', 'Alta'] as const
const PRIORIDADES_PERSONALIZADAS = ['Baja', 'Media', 'Alta', 'Crítica'] as const

interface Usuario {
  id: number
  nombreCompleto: string
  username: string
  rol: string
  activo: boolean
}

interface Empleado {
  id: number
  nombreCompleto: string
  activo: boolean
  cargo?: string | null
  departamento?: string | null
  prioridadPersonalizada?: string | null
  telefono?: string | null
  telegramChatId?: string | null
  vinculadoTelegram?: boolean
  telegramUsuarioOId?: string | null
}

interface Equipo {
  id: number
  modelo: string
  fabricante?: string
  ip: string
  departamento: string
  comunidadSnmp: string
}

interface ProblemaDeValidacion {
  title?: string
  errors?: Record<string, string[]>
}

const router = useRouter()

function obtenerRolSesionActual(): string {
  const rolGuardado = localStorage.getItem('rol_usuario')
  if (rolGuardado) {
    return rolGuardado
  }

  const token = localStorage.getItem(TOKEN_KEY)
  if (!token) {
    return ''
  }

  try {
    const partes = token.split('.')
    if (partes.length < 2) {
      return ''
    }
    const payload = JSON.parse(atob(partes[1]!.replace(/-/g, '+').replace(/_/g, '/')))
    const claimRol =
      payload.role ||
      payload.rol ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      ''
    return Array.isArray(claimRol) ? (claimRol[0] ?? '') : claimRol
  } catch {
    return ''
  }
}

const esAdminActual = computed(() => obtenerRolSesionActual().toLowerCase() === 'admin')
const rolesDisponibles = computed(() =>
  esAdminActual.value ? ROLES : (['Ingeniero', 'Técnico'] as const),
)

function formatearRolVisual(rol: string): string {
  if (rol.toLowerCase() === 'tecnico' || rol.toLowerCase() === 'técnico') {
    return 'Técnico'
  }
  if (rol.toLowerCase() === 'ingeniero') {
    return 'Ingeniero'
  }
  return rol
}

function claseBadgeRol(rol: string): string {
  const norm = formatearRolVisual(rol)
  if (norm === 'Admin') return 'badge--admin'
  if (norm === 'Ingeniero') return 'badge--ingeniero'
  return 'badge--tecnico'
}

function claseBadgePrioridad(prioridad?: string | null): string {
  if (!prioridad) return ''
  const norm = prioridad
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
  return `badge--prioridad-${norm}`
}

function puedeModificarUsuario(usuario: Usuario): boolean {
  if ((usuario.rol === 'Admin' || usuario.username === 'admin') && !esAdminActual.value) {
    return false
  }
  return true
}

const pestanaActiva = ref<'usuarios' | 'empleados' | 'equipos' | 'catalogos'>('usuarios')

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
      formatearRolVisual(usuario.rol).toLowerCase().includes(consulta),
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
  rol: 'Técnico',
  activo: true,
})

const eliminandoId = ref<number | null>(null)
const errorAccion = ref('')

const empleados = ref<Empleado[]>([])
const cargandoEmpleados = ref(false)
const errorEmpleados = ref('')
const busquedaEmpleados = ref('')

const empleadosFiltrados = computed(() => {
  const consulta = busquedaEmpleados.value.trim().toLowerCase()

  if (consulta === '') {
    return empleados.value
  }

  return empleados.value.filter(
    (empleado) =>
      empleado.nombreCompleto.toLowerCase().includes(consulta) ||
      (empleado.cargo ?? '').toLowerCase().includes(consulta) ||
      (empleado.departamento ?? '').toLowerCase().includes(consulta) ||
      (empleado.telefono ?? empleado.telegramUsuarioOId ?? '').toLowerCase().includes(consulta),
  )
})

const mostrarModalEmpleado = ref(false)
const modoEdicionEmpleado = ref(false)
const editandoEmpleadoId = ref<number | null>(null)
const guardandoEmpleado = ref(false)
const errorFormularioEmpleado = ref('')
const eliminandoEmpleadoId = ref<number | null>(null)
const errorAccionEmpleado = ref('')

const formularioEmpleado = reactive({
  nombreCompleto: '',
  cargo: '',
  departamento: '',
  prioridadPersonalizada: '',
  telegramUsuarioOId: '',
  activo: true,
})

const equipos = ref<Equipo[]>([])
const cargandoEquipos = ref(false)
const errorEquipos = ref('')

const mostrarModalEquipo = ref(false)
const modoEdicionEquipo = ref(false)
const editandoEquipoId = ref<number | null>(null)
const guardandoEquipo = ref(false)
const errorFormularioEquipo = ref('')

const formularioEquipo = reactive({
  nombreVisible: '',
  modeloTecnico: '',
  ip: '',
  departamento: '',
  comunidadSnmp: 'public',
})

const eliminandoEquipoId = ref<number | null>(null)
const errorAccionEquipo = ref('')

const departamentos = ref<Departamento[]>([])
const categorias = ref<CategoriaTicket[]>([])
const cargandoCatalogos = ref(false)
const errorCatalogos = ref('')
const errorAccionCatalogo = ref('')

const mostrarModalDepartamento = ref(false)
const modoEdicionDepartamento = ref(false)
const editandoDepartamentoId = ref<number | null>(null)
const guardandoDepartamento = ref(false)
const errorFormularioDepartamento = ref('')
const eliminandoDepartamentoId = ref<number | null>(null)

const formularioDepartamento = reactive({
  nombre: '',
  sigla: '',
  prioridadPorDefecto: 'Media',
})

const mostrarModalCategoria = ref(false)
const modoEdicionCategoria = ref(false)
const editandoCategoriaId = ref<number | null>(null)
const guardandoCategoria = ref(false)
const errorFormularioCategoria = ref('')
const eliminandoCategoriaId = ref<number | null>(null)

const formularioCategoria = reactive({
  nombre: '',
})

function cambiarPestana(pestana: 'usuarios' | 'empleados' | 'equipos' | 'catalogos') {
  pestanaActiva.value = pestana

  if (pestana === 'empleados' && empleados.value.length === 0) {
    cargarEmpleados()
  }

  if (pestana === 'empleados' && departamentos.value.length === 0) {
    cargarDepartamentos()
  }

  if (pestana === 'equipos' && equipos.value.length === 0) {
    cargarEquipos()
  }

  if (pestana === 'equipos' && departamentos.value.length === 0) {
    cargarDepartamentos()
  }

  if (pestana === 'catalogos' && (departamentos.value.length === 0 || categorias.value.length === 0)) {
    cargarCatalogos()
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
    const { data } = await axios.get<Usuario[]>(`${API_BASE}/api/usuarios`, {
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

    if (axios.isAxiosError(e) && e.response?.status === 403) {
      error.value = 'No tienes permisos para consultar los usuarios del sistema.'
      return
    }

    if (axios.isAxiosError(e) && !e.response) {
      error.value = 'No se pudo conectar con el servidor. Verifica que el backend esté en ejecución.'
    } else {
      error.value = 'No se pudieron cargar los usuarios. Intenta nuevamente.'
    }
  } finally {
    cargando.value = false
  }
}

async function cargarEmpleados() {
  cargandoEmpleados.value = true
  errorEmpleados.value = ''

  try {
    const { data } = await axios.get<Empleado[]>(`${API_BASE}/api/usuarios/empleados`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    empleados.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    if (axios.isAxiosError(e) && e.response?.status === 403) {
      errorEmpleados.value = 'No tienes permisos para consultar el directorio de empleados.'
      return
    }

    if (axios.isAxiosError(e) && !e.response) {
      errorEmpleados.value = 'No se pudo conectar con el servidor. Verifica que el backend esté en ejecución.'
    } else {
      errorEmpleados.value = 'No se pudieron cargar los empleados. Intenta nuevamente.'
    }
  } finally {
    cargandoEmpleados.value = false
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
  formulario.rol = 'Técnico'
  formulario.activo = true
  errorFormulario.value = ''
  mostrarModal.value = true
}

function abrirEditar(usuario: Usuario) {
  if (!puedeModificarUsuario(usuario)) {
    toast.warning('Los ingenieros y técnicos no pueden editar las credenciales del Administrador.')
    return
  }

  modoEdicion.value = true
  editandoId.value = usuario.id
  formulario.nombreCompleto = usuario.nombreCompleto
  formulario.username = usuario.username
  formulario.password = ''
  formulario.rol = formatearRolVisual(usuario.rol)
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
    } else if (mostrarModalEmpleado.value && !guardandoEmpleado.value) {
      cerrarModalEmpleado()
    } else if (mostrarModalEquipo.value && !guardandoEquipo.value) {
      cerrarModalEquipo()
    } else if (mostrarModalDepartamento.value && !guardandoDepartamento.value) {
      cerrarModalDepartamento()
    } else if (mostrarModalCategoria.value && !guardandoCategoria.value) {
      cerrarModalCategoria()
    }
  }
}

function abrirNuevoEmpleado() {
  modoEdicionEmpleado.value = false
  editandoEmpleadoId.value = null
  formularioEmpleado.nombreCompleto = ''
  formularioEmpleado.cargo = ''
  formularioEmpleado.departamento = ''
  formularioEmpleado.prioridadPersonalizada = ''
  formularioEmpleado.telegramUsuarioOId = ''
  formularioEmpleado.activo = true
  errorFormularioEmpleado.value = ''
  mostrarModalEmpleado.value = true

  if (departamentos.value.length === 0) {
    cargarDepartamentos()
  }
}

function abrirEditarEmpleado(empleado: Empleado) {
  modoEdicionEmpleado.value = true
  editandoEmpleadoId.value = empleado.id
  formularioEmpleado.nombreCompleto = empleado.nombreCompleto
  formularioEmpleado.cargo = empleado.cargo ?? ''
  formularioEmpleado.departamento = empleado.departamento ?? ''
  formularioEmpleado.prioridadPersonalizada =
    empleado.prioridadPersonalizada === 'Critica'
      ? 'Crítica'
      : (empleado.prioridadPersonalizada ?? '')
  formularioEmpleado.telegramUsuarioOId = empleado.telefono ?? empleado.telegramUsuarioOId ?? ''
  formularioEmpleado.activo = empleado.activo
  errorFormularioEmpleado.value = ''
  mostrarModalEmpleado.value = true

  if (departamentos.value.length === 0) {
    cargarDepartamentos()
  }
}

function cerrarModalEmpleado() {
  mostrarModalEmpleado.value = false
  errorFormularioEmpleado.value = ''
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

  if (formulario.password && formulario.password.length < 4) {
    return 'La contraseña debe tener al menos 4 caracteres.'
  }

  if (!ROLES.includes(formulario.rol as (typeof ROLES)[number])) {
    return 'El rol debe ser Admin, Ingeniero o Técnico.'
  }

  return ''
}

function validarFormularioEmpleado(): string {
  const nombre = formularioEmpleado.nombreCompleto.trim()

  if (nombre.length < 2 || nombre.length > 150) {
    return 'El nombre es obligatorio y debe tener entre 2 y 150 caracteres.'
  }

  if (formularioEmpleado.cargo.trim().length > 100) {
    return 'El cargo no puede exceder 100 caracteres.'
  }

  if (formularioEmpleado.telegramUsuarioOId.trim().length > 40) {
    return 'El número de teléfono no puede exceder 40 caracteres.'
  }

  if (
    formularioEmpleado.prioridadPersonalizada &&
    !PRIORIDADES_PERSONALIZADAS.includes(
      formularioEmpleado.prioridadPersonalizada as (typeof PRIORIDADES_PERSONALIZADAS)[number],
    )
  ) {
    return 'Selecciona una prioridad válida.'
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
      await axios.put(`${API_BASE}/api/usuarios/${editandoId.value}`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    } else {
      await axios.post(`${API_BASE}/api/usuarios`, payload, {
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

async function guardarEmpleado() {
  errorFormularioEmpleado.value = ''

  const fallo = validarFormularioEmpleado()

  if (fallo) {
    errorFormularioEmpleado.value = fallo
    return
  }

  guardandoEmpleado.value = true

  const payload = {
    nombreCompleto: formularioEmpleado.nombreCompleto.trim(),
    cargo: formularioEmpleado.cargo.trim() || null,
    departamento: formularioEmpleado.departamento || null,
    prioridadPersonalizada: formularioEmpleado.prioridadPersonalizada || null,
    telefono: formularioEmpleado.telegramUsuarioOId.trim() || null,
    telegramUsuarioOId: formularioEmpleado.telegramUsuarioOId.trim() || null,
    activo: formularioEmpleado.activo,
  }

  try {
    if (modoEdicionEmpleado.value && editandoEmpleadoId.value !== null) {
      await axios.put(`${API_BASE}/api/usuarios/empleados/${editandoEmpleadoId.value}`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    } else {
      await axios.post(`${API_BASE}/api/usuarios/empleados`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    }

    cerrarModalEmpleado()
    await cargarEmpleados()
    toast.success(
      modoEdicionEmpleado.value ? 'Empleado actualizado correctamente.' : 'Empleado registrado correctamente.',
    )
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormularioEmpleado.value = mensajeDeError(
      e,
      'No se pudo guardar el empleado. Intenta nuevamente.',
    )
  } finally {
    guardandoEmpleado.value = false
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
      `${API_BASE}/api/usuarios/${usuario.id}`,
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
    await axios.delete(`${API_BASE}/api/usuarios/${usuario.id}`, {
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

async function cambiarEstadoEmpleado(empleado: Empleado) {
  errorAccionEmpleado.value = ''

  const accion = empleado.activo ? 'desactivar' : 'activar'

  const confirmado = await confirmar({
    mensaje: `¿${accion === 'desactivar' ? 'Desactivar' : 'Activar'} al empleado "${empleado.nombreCompleto}"?`,
    confirmarTexto: accion === 'desactivar' ? 'Desactivar' : 'Activar',
  })

  if (!confirmado) {
    return
  }

  try {
    await axios.put(
      `${API_BASE}/api/usuarios/empleados/${empleado.id}`,
      {
        nombreCompleto: empleado.nombreCompleto,
        activo: !empleado.activo,
        cargo: empleado.cargo ?? null,
        departamento: empleado.departamento ?? null,
        prioridadPersonalizada: empleado.prioridadPersonalizada ?? null,
        telefono: empleado.telefono ?? empleado.telegramUsuarioOId ?? null,
        telegramUsuarioOId: empleado.telefono ?? empleado.telegramUsuarioOId ?? null,
      },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    await cargarEmpleados()
    toast.success(
      accion === 'desactivar' ? 'Empleado desactivado correctamente.' : 'Empleado activado correctamente.',
    )
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccionEmpleado.value = mensajeDeError(e, 'No se pudo cambiar el estado del empleado.')
  }
}

async function eliminarEmpleado(empleado: Empleado) {
  errorAccionEmpleado.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar al empleado "${empleado.nombreCompleto}" del directorio? Esta acción no se puede deshacer.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoEmpleadoId.value = empleado.id

  try {
    await axios.delete(`${API_BASE}/api/usuarios/empleados/${empleado.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarEmpleados()
    toast.success('Empleado eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccionEmpleado.value = mensajeDeError(e, 'No se pudo eliminar el empleado. Intenta nuevamente.')
  } finally {
    eliminandoEmpleadoId.value = null
  }
}

async function cargarDepartamentos() {
  try {
    departamentos.value = await obtenerDepartamentos()
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorCatalogos.value = 'No se pudieron cargar los departamentos.'
  }
}

async function cargarCategorias() {
  try {
    categorias.value = await obtenerCategorias()
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorCatalogos.value = 'No se pudieron cargar las categorías.'
  }
}

async function cargarCatalogos() {
  cargandoCatalogos.value = true
  errorCatalogos.value = ''

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

    errorCatalogos.value = 'No se pudieron cargar los departamentos y categorías.'
  } finally {
    cargandoCatalogos.value = false
  }
}

function abrirNuevoDepartamento() {
  modoEdicionDepartamento.value = false
  editandoDepartamentoId.value = null
  formularioDepartamento.nombre = ''
  formularioDepartamento.sigla = ''
  formularioDepartamento.prioridadPorDefecto = 'Media'
  errorFormularioDepartamento.value = ''
  mostrarModalDepartamento.value = true
}

function abrirEditarDepartamento(departamento: Departamento) {
  modoEdicionDepartamento.value = true
  editandoDepartamentoId.value = departamento.id
  formularioDepartamento.nombre = departamento.nombre
  formularioDepartamento.sigla = departamento.sigla
  formularioDepartamento.prioridadPorDefecto = departamento.prioridadPorDefecto
  errorFormularioDepartamento.value = ''
  mostrarModalDepartamento.value = true
}

function cerrarModalDepartamento() {
  mostrarModalDepartamento.value = false
  errorFormularioDepartamento.value = ''
}

function validarDepartamento(): string {
  const nombre = formularioDepartamento.nombre.trim()
  const sigla = formularioDepartamento.sigla.trim()

  if (nombre.length < 2 || nombre.length > 100) {
    return 'El nombre es obligatorio y debe tener entre 2 y 100 caracteres.'
  }

  if (sigla.length < 2 || sigla.length > 10) {
    return 'La sigla es obligatoria y debe tener entre 2 y 10 caracteres.'
  }

  if (!PRIORIDADES_DEPARTAMENTO.includes(formularioDepartamento.prioridadPorDefecto as (typeof PRIORIDADES_DEPARTAMENTO)[number])) {
    return 'Selecciona una prioridad válida.'
  }

  return ''
}

async function guardarDepartamento() {
  errorFormularioDepartamento.value = ''

  const fallo = validarDepartamento()

  if (fallo) {
    errorFormularioDepartamento.value = fallo
    return
  }

  guardandoDepartamento.value = true

  const payload = {
    nombre: formularioDepartamento.nombre.trim(),
    sigla: formularioDepartamento.sigla.trim().toUpperCase(),
    prioridadPorDefecto: formularioDepartamento.prioridadPorDefecto,
  }

  try {
    if (modoEdicionDepartamento.value && editandoDepartamentoId.value !== null) {
      await axios.put(`${API_BASE}/api/departamentos/${editandoDepartamentoId.value}`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    } else {
      await axios.post(`${API_BASE}/api/departamentos`, payload, {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      })
    }

    cerrarModalDepartamento()
    await cargarDepartamentos()
    toast.success(
      modoEdicionDepartamento.value
        ? 'Departamento actualizado correctamente.'
        : 'Departamento creado correctamente.',
    )
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormularioDepartamento.value = mensajeDeError(
      e,
      'No se pudo guardar el departamento. Intenta nuevamente.',
    )
  } finally {
    guardandoDepartamento.value = false
  }
}

async function eliminarDepartamento(departamento: Departamento) {
  errorAccionCatalogo.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar el departamento "${departamento.nombre}"? Los tickets existentes conservarán su registro.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoDepartamentoId.value = departamento.id

  try {
    await axios.delete(`${API_BASE}/api/departamentos/${departamento.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarDepartamentos()
    toast.success('Departamento eliminado correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccionCatalogo.value = mensajeDeError(
      e,
      'No se pudo eliminar el departamento. Intenta nuevamente.',
    )
  } finally {
    eliminandoDepartamentoId.value = null
  }
}

function abrirNuevaCategoria() {
  modoEdicionCategoria.value = false
  editandoCategoriaId.value = null
  formularioCategoria.nombre = ''
  errorFormularioCategoria.value = ''
  mostrarModalCategoria.value = true
}

function abrirEditarCategoria(categoria: CategoriaTicket) {
  modoEdicionCategoria.value = true
  editandoCategoriaId.value = categoria.id
  formularioCategoria.nombre = categoria.nombre
  errorFormularioCategoria.value = ''
  mostrarModalCategoria.value = true
}

function cerrarModalCategoria() {
  mostrarModalCategoria.value = false
  errorFormularioCategoria.value = ''
}

async function guardarCategoria() {
  errorFormularioCategoria.value = ''
  const nombre = formularioCategoria.nombre.trim()

  if (nombre.length < 2 || nombre.length > 100) {
    errorFormularioCategoria.value = 'El nombre es obligatorio y debe tener entre 2 y 100 caracteres.'
    return
  }

  guardandoCategoria.value = true

  try {
    if (modoEdicionCategoria.value && editandoCategoriaId.value !== null) {
      await axios.put(
        `${API_BASE}/api/categorias/${editandoCategoriaId.value}`,
        { nombre },
        {
          headers: {
            Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
          },
        },
      )
    } else {
      await axios.post(
        `${API_BASE}/api/categorias`,
        { nombre },
        {
          headers: {
            Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
          },
        },
      )
    }

    cerrarModalCategoria()
    await cargarCategorias()
    toast.success(
      modoEdicionCategoria.value ? 'Categoría actualizada correctamente.' : 'Categoría creada correctamente.',
    )
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorFormularioCategoria.value = mensajeDeError(
      e,
      'No se pudo guardar la categoría. Intenta nuevamente.',
    )
  } finally {
    guardandoCategoria.value = false
  }
}

async function eliminarCategoria(categoria: CategoriaTicket) {
  errorAccionCatalogo.value = ''

  const confirmado = await confirmar({
    mensaje: `¿Eliminar la categoría "${categoria.nombre}"? Los tickets existentes conservarán su registro.`,
    confirmarTexto: 'Eliminar',
  })

  if (!confirmado) {
    return
  }

  eliminandoCategoriaId.value = categoria.id

  try {
    await axios.delete(`${API_BASE}/api/categorias/${categoria.id}`, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    await cargarCategorias()
    toast.success('Categoría eliminada correctamente.')
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    errorAccionCatalogo.value = mensajeDeError(
      e,
      'No se pudo eliminar la categoría. Intenta nuevamente.',
    )
  } finally {
    eliminandoCategoriaId.value = null
  }
}

function abrirNuevoEquipo() {
  modoEdicionEquipo.value = false
  editandoEquipoId.value = null
  formularioEquipo.nombreVisible = ''
  formularioEquipo.modeloTecnico = ''
  formularioEquipo.ip = ''
  formularioEquipo.departamento = ''
  formularioEquipo.comunidadSnmp = 'public'
  errorFormularioEquipo.value = ''
  mostrarModalEquipo.value = true

  if (departamentos.value.length === 0) {
    cargarDepartamentos()
  }
}

function abrirEditarEquipo(equipo: Equipo) {
  modoEdicionEquipo.value = true
  editandoEquipoId.value = equipo.id
  formularioEquipo.nombreVisible = equipo.modelo
  formularioEquipo.modeloTecnico = equipo.fabricante ?? ''
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
  const nombre = formularioEquipo.nombreVisible.trim()
  const ip = formularioEquipo.ip.trim()
  const departamento = formularioEquipo.departamento.trim()

  if (nombre.length < 2 || nombre.length > 70) {
    return 'El nombre del equipo es obligatorio y debe tener entre 2 y 70 caracteres.'
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

  const nombre = formularioEquipo.nombreVisible.trim()
  const modeloTec = formularioEquipo.modeloTecnico.trim()
  const modeloCombinado = modeloTec ? `${nombre} || ${modeloTec}` : nombre

  const payload = {
    modelo: modeloCombinado,
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
  cargarDepartamentos()
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', alPulsarEscape)
})
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <h1 class="vista__titulo">Configuración del Sistema</h1>
      <p class="vista__subtitulo">
        Gestión de cuentas del sistema, directorio de empleados de Telegram, equipos de impresión
        (SNMP), departamentos y categorías
      </p>
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
        :class="{ 'pestana--activa': pestanaActiva === 'empleados' }"
        @click="cambiarPestana('empleados')"
      >
        Empleados (Telegram)
      </button>
      <button
        class="pestana"
        type="button"
        :class="{ 'pestana--activa': pestanaActiva === 'equipos' }"
        @click="cambiarPestana('equipos')"
      >
        Equipos (SNMP)
      </button>
      <button
        class="pestana"
        type="button"
        :class="{ 'pestana--activa': pestanaActiva === 'catalogos' }"
        @click="cambiarPestana('catalogos')"
      >
        Departamentos y Categorías
      </button>
    </div>

    <section v-if="pestanaActiva === 'usuarios'" class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Usuarios del sistema (Admin, Ingeniero y Técnico)</h2>
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

      <div v-else class="tabla__envoltura">
        <table class="tabla">
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
                <span class="badge" :class="claseBadgeRol(usuario.rol)">
                  {{ formatearRolVisual(usuario.rol) }}
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
                  :disabled="eliminandoId !== null || !puedeModificarUsuario(usuario)"
                  :title="!puedeModificarUsuario(usuario) ? 'No tienes permisos para editar al Administrador' : ''"
                  @click="abrirEditar(usuario)"
                >
                  Editar
                </button>
                <button
                  class="accion"
                  :class="usuario.activo ? 'accion--desactivar' : 'accion--activar'"
                  type="button"
                  :disabled="eliminandoId !== null || usuario.username === 'admin' || !puedeModificarUsuario(usuario)"
                  @click="cambiarEstado(usuario)"
                >
                  {{ usuario.activo ? 'Desactivar' : 'Activar' }}
                </button>
                <button
                  class="accion accion--eliminar"
                  type="button"
                  :disabled="eliminandoId !== null || usuario.username === 'admin' || !puedeModificarUsuario(usuario)"
                  @click="eliminarUsuario(usuario)"
                >
                  {{ eliminandoId === usuario.id ? 'Eliminando...' : 'Eliminar' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section v-else-if="pestanaActiva === 'empleados'" class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Empleados (directorio de Telegram)</h2>
        <div class="tarjeta__acciones">
          <button class="boton boton--primario" type="button" @click="abrirNuevoEmpleado">
            Nuevo Empleado
          </button>
          <button
            class="boton boton--secundario"
            type="button"
            :disabled="cargandoEmpleados"
            @click="cargarEmpleados"
          >
            {{ cargandoEmpleados ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <p class="tarjeta__nota">
        Los empleados de esta sección reportan incidencias por Telegram y no tienen acceso al
        sistema web.
      </p>

      <div class="filtros">
        <input
          v-model="busquedaEmpleados"
          class="filtros__busqueda"
          type="search"
          placeholder="Buscar por nombre, cargo, departamento o teléfono..."
          autocomplete="off"
        />
        <span v-if="!cargandoEmpleados" class="filtros__conteo">
          {{ empleadosFiltrados.length }} de {{ empleados.length }}
        </span>
      </div>

      <p v-if="errorEmpleados" class="alerta">{{ errorEmpleados }}</p>
      <p v-if="errorAccionEmpleado" class="alerta">{{ errorAccionEmpleado }}</p>

      <div v-if="cargandoEmpleados" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando empleados...</p>
      </div>

      <div v-else-if="empleados.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay empleados registrados en el directorio.</p>
      </div>

      <div v-else-if="empleadosFiltrados.length === 0" class="estado-vacio">
        <p class="estado-vacio__texto">No hay empleados que coincidan con la búsqueda.</p>
      </div>

      <div v-else class="tabla__envoltura">
        <table class="tabla">
          <thead>
            <tr>
              <th class="tabla__th">Nombre</th>
              <th class="tabla__th">Cargo</th>
              <th class="tabla__th">Departamento</th>
              <th class="tabla__th tabla__th--corta">Prioridad</th>
              <th class="tabla__th">Teléfono</th>
              <th class="tabla__th tabla__th--corta">Bot Telegram</th>
              <th class="tabla__th tabla__th--corta">Estado</th>
              <th class="tabla__th tabla__th--acciones">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="empleado in empleadosFiltrados" :key="empleado.id" class="tabla__fila">
              <td class="tabla__td tabla__td--nombre">{{ empleado.nombreCompleto }}</td>
              <td class="tabla__td">{{ empleado.cargo || '—' }}</td>
              <td class="tabla__td">{{ empleado.departamento || '—' }}</td>
              <td class="tabla__td">
                <span
                  v-if="empleado.prioridadPersonalizada"
                  class="badge"
                  :class="claseBadgePrioridad(empleado.prioridadPersonalizada)"
                >
                  {{ empleado.prioridadPersonalizada === 'Critica' ? 'Crítica' : empleado.prioridadPersonalizada }}
                </span>
                <span v-else>—</span>
              </td>
              <td class="tabla__td">{{ empleado.telefono || empleado.telegramUsuarioOId || '—' }}</td>
              <td class="tabla__td">
                <span class="badge" :class="empleado.vinculadoTelegram ? 'badge--activo' : 'badge--inactivo'">
                  {{ empleado.vinculadoTelegram ? 'Vinculado ✓' : 'Pendiente' }}
                </span>
              </td>
              <td class="tabla__td">
                <span class="badge" :class="empleado.activo ? 'badge--activo' : 'badge--inactivo'">
                  {{ empleado.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </td>
              <td class="tabla__td tabla__td--acciones">
                <button
                  class="accion accion--editar"
                  type="button"
                  :disabled="eliminandoEmpleadoId !== null"
                  @click="abrirEditarEmpleado(empleado)"
                >
                  Editar
                </button>
                <button
                  class="accion"
                  :class="empleado.activo ? 'accion--desactivar' : 'accion--activar'"
                  type="button"
                  :disabled="eliminandoEmpleadoId !== null"
                  @click="cambiarEstadoEmpleado(empleado)"
                >
                  {{ empleado.activo ? 'Desactivar' : 'Activar' }}
                </button>
                <button
                  class="accion accion--eliminar"
                  type="button"
                  :disabled="eliminandoEmpleadoId !== null"
                  @click="eliminarEmpleado(empleado)"
                >
                  {{ eliminandoEmpleadoId === empleado.id ? 'Eliminando...' : 'Eliminar' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <section v-else-if="pestanaActiva === 'equipos'" class="tarjeta">
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

      <div v-else class="tabla__envoltura">
        <table class="tabla">
          <thead>
            <tr>
              <th class="tabla__th tabla__th--corta">ID</th>
              <th class="tabla__th">Nombre en el Sistema</th>
              <th class="tabla__th">Modelo Técnico / Marca</th>
              <th class="tabla__th">Dirección IP</th>
              <th class="tabla__th">Departamento</th>
              <th class="tabla__th tabla__th--acciones">Acciones</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="equipo in equipos" :key="equipo.id" class="tabla__fila">
              <td class="tabla__td tabla__td--id">{{ equipo.id }}</td>
              <td class="tabla__td tabla__td--nombre">{{ equipo.modelo }}</td>
              <td class="tabla__td">{{ equipo.fabricante || '—' }}</td>
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
      </div>
    </section>

    <section v-else class="tarjeta">
      <header class="tarjeta__cabecera">
        <h2 class="tarjeta__titulo">Departamentos y Categorías</h2>
        <div class="tarjeta__acciones">
          <button
            class="boton boton--secundario"
            type="button"
            :disabled="cargandoCatalogos"
            @click="cargarCatalogos"
          >
            {{ cargandoCatalogos ? 'Cargando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <p v-if="errorCatalogos" class="alerta">{{ errorCatalogos }}</p>
      <p v-if="errorAccionCatalogo" class="alerta">{{ errorAccionCatalogo }}</p>

      <div v-if="cargandoCatalogos" class="estado-vacio">
        <p class="estado-vacio__texto">Cargando departamentos y categorías...</p>
      </div>

      <div v-else class="catalogos">
        <article class="catalogos__bloque">
          <header class="catalogos__cabecera">
            <div>
              <h3 class="catalogos__titulo">Departamentos</h3>
              <p class="catalogos__nota">
                La prioridad configurada se asigna automáticamente al crear tickets.
              </p>
            </div>
            <button class="boton boton--primario" type="button" @click="abrirNuevoDepartamento">
              Nuevo Departamento
            </button>
          </header>

          <div v-if="departamentos.length === 0" class="estado-vacio">
            <p class="estado-vacio__texto">No hay departamentos configurados.</p>
          </div>

          <table v-else class="tabla">
            <thead>
              <tr>
                <th class="tabla__th">Nombre</th>
                <th class="tabla__th tabla__th--corta">Sigla</th>
                <th class="tabla__th tabla__th--corta">Prioridad</th>
                <th class="tabla__th tabla__th--acciones">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="departamento in departamentos" :key="departamento.id" class="tabla__fila">
                <td class="tabla__td tabla__td--nombre">{{ departamento.nombre }}</td>
                <td class="tabla__td">{{ departamento.sigla }}</td>
                <td class="tabla__td">
                  <span
                    class="badge"
                    :class="claseBadgePrioridad(departamento.prioridadPorDefecto)"
                  >
                    {{ departamento.prioridadPorDefecto }}
                  </span>
                </td>
                <td class="tabla__td tabla__td--acciones">
                  <button
                    class="accion accion--editar"
                    type="button"
                    :disabled="eliminandoDepartamentoId !== null"
                    @click="abrirEditarDepartamento(departamento)"
                  >
                    Editar
                  </button>
                  <button
                    class="accion accion--eliminar"
                    type="button"
                    :disabled="eliminandoDepartamentoId !== null"
                    @click="eliminarDepartamento(departamento)"
                  >
                    {{ eliminandoDepartamentoId === departamento.id ? 'Eliminando...' : 'Eliminar' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </article>

        <article class="catalogos__bloque">
          <header class="catalogos__cabecera">
            <div>
              <h3 class="catalogos__titulo">Categorías</h3>
              <p class="catalogos__nota">Clasifican las incidencias para las estadísticas de la Mesa de Ayuda.</p>
            </div>
            <button class="boton boton--primario" type="button" @click="abrirNuevaCategoria">
              Nueva Categoría
            </button>
          </header>

          <div v-if="categorias.length === 0" class="estado-vacio">
            <p class="estado-vacio__texto">No hay categorías configuradas.</p>
          </div>

          <table v-else class="tabla">
            <thead>
              <tr>
                <th class="tabla__th">Nombre</th>
                <th class="tabla__th tabla__th--acciones">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="categoria in categorias" :key="categoria.id" class="tabla__fila">
                <td class="tabla__td tabla__td--nombre">{{ categoria.nombre }}</td>
                <td class="tabla__td tabla__td--acciones">
                  <button
                    class="accion accion--editar"
                    type="button"
                    :disabled="eliminandoCategoriaId !== null"
                    @click="abrirEditarCategoria(categoria)"
                  >
                    Editar
                  </button>
                  <button
                    class="accion accion--eliminar"
                    type="button"
                    :disabled="eliminandoCategoriaId !== null"
                    @click="eliminarCategoria(categoria)"
                  >
                    {{ eliminandoCategoriaId === categoria.id ? 'Eliminando...' : 'Eliminar' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </article>
      </div>
    </section>

    <div v-if="mostrarModal" class="modal">
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
                Contraseña
                {{ modoEdicion ? '(dejar vacía para no cambiarla)' : '' }}
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
                <option v-for="rol in rolesDisponibles" :key="rol" :value="rol">{{ rol }}</option>
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

    <div v-if="mostrarModalEmpleado" class="modal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-empleado-titulo">
        <form @submit.prevent="guardarEmpleado">
          <header class="modal__cabecera">
            <h3 id="modal-empleado-titulo" class="modal__titulo">
              {{ modoEdicionEmpleado ? 'Editar Empleado' : 'Nuevo Empleado' }}
            </h3>
            <button
              class="modal__cerrar"
              type="button"
              aria-label="Cerrar"
              :disabled="guardandoEmpleado"
              @click="cerrarModalEmpleado"
            >
              &times;
            </button>
          </header>

          <div class="modal__cuerpo">
            <div class="campo">
              <label class="campo__etiqueta" for="empleado-nombre">Nombre Completo</label>
              <input
                id="empleado-nombre"
                v-model="formularioEmpleado.nombreCompleto"
                class="campo__input"
                type="text"
                placeholder="Ej. Juan Pérez"
                maxlength="150"
                :disabled="guardandoEmpleado"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="empleado-cargo">Cargo</label>
              <input
                id="empleado-cargo"
                v-model="formularioEmpleado.cargo"
                class="campo__input"
                type="text"
                placeholder="Ej. Analista de Facturación"
                maxlength="100"
                :disabled="guardandoEmpleado"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="empleado-departamento">Departamento</label>
              <select
                id="empleado-departamento"
                v-model="formularioEmpleado.departamento"
                class="campo__input"
                :disabled="guardandoEmpleado"
              >
                <option value="">Sin departamento asignado</option>
                <option
                  v-if="
                    formularioEmpleado.departamento &&
                    !departamentos.some((d) => d.nombre === formularioEmpleado.departamento)
                  "
                  :value="formularioEmpleado.departamento"
                >
                  {{ formularioEmpleado.departamento }} (no configurado)
                </option>
                <option v-for="departamento in departamentos" :key="departamento.id" :value="departamento.nombre">
                  {{ departamento.nombre }} ({{ departamento.sigla }})
                </option>
              </select>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="empleado-prioridad">
                Prioridad Personalizada (opcional)
              </label>
              <select
                id="empleado-prioridad"
                v-model="formularioEmpleado.prioridadPersonalizada"
                class="campo__input"
                :disabled="guardandoEmpleado"
              >
                <option value="">Usar prioridad del departamento</option>
                <option
                  v-for="prioridad in PRIORIDADES_PERSONALIZADAS"
                  :key="prioridad"
                  :value="prioridad"
                >
                  {{ prioridad }}
                </option>
              </select>
              <p class="nota">
                Si se deja vacío, se usa la prioridad por defecto del departamento.
              </p>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="empleado-telegram">
                Número de Teléfono (Telegram)
              </label>
              <input
                id="empleado-telegram"
                v-model="formularioEmpleado.telegramUsuarioOId"
                class="campo__input"
                type="tel"
                placeholder="Ej. 0412-1234567"
                maxlength="40"
                :disabled="guardandoEmpleado"
              />
              <p class="nota">
                Registra el número de teléfono del empleado. Al iniciar el bot en su teléfono y tocar
                "Verificar mi número de teléfono", quedará enlazado automáticamente con su departamento y prioridad.
              </p>
            </div>

            <label class="interruptor">
              <input v-model="formularioEmpleado.activo" type="checkbox" :disabled="guardandoEmpleado" />
              <span>Empleado activo</span>
            </label>

            <p v-if="errorFormularioEmpleado" class="alerta alerta--formulario">
              {{ errorFormularioEmpleado }}
            </p>
          </div>

          <footer class="modal__pie">
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="guardandoEmpleado"
              @click="cerrarModalEmpleado"
            >
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardandoEmpleado">
              {{ guardandoEmpleado ? 'Guardando...' : modoEdicionEmpleado ? 'Guardar Cambios' : 'Guardar' }}
            </button>
          </footer>
        </form>
      </div>
    </div>

    <div v-if="mostrarModalEquipo" class="modal">
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
              <label class="campo__etiqueta" for="equipo-nombre">
                Nombre del Equipo (Visible en el sistema)
              </label>
              <input
                id="equipo-nombre"
                v-model="formularioEquipo.nombreVisible"
                class="campo__input"
                type="text"
                placeholder="Ej. Impresora Personal / Facturación Caja 1"
                maxlength="70"
                :disabled="guardandoEquipo"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-modelo">
                Modelo Técnico de la Impresora (Para buscar manuales)
              </label>
              <input
                id="equipo-modelo"
                v-model="formularioEquipo.modeloTecnico"
                class="campo__input"
                type="text"
                placeholder="Ej. HP Smart Tank 215 / Canon MF455dw"
                maxlength="70"
                :disabled="guardandoEquipo"
              />
              <p class="nota">
                El sistema conservará siempre el Nombre que elegiste arriba y usará este Modelo
                Técnico para buscar los manuales PDF.
              </p>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-ip">IP</label>
              <input
                id="equipo-ip"
                v-model="formularioEquipo.ip"
                class="campo__input"
                type="text"
                placeholder="Ej. 192.168.100.57"
                maxlength="45"
                :disabled="guardandoEquipo"
              />
              <p v-if="modoEdicionEquipo && formularioEquipo.ip.startsWith('importado-')" class="nota">
                Esta es una IP temporal del importador. Reemplázala por la IP real del equipo.
              </p>
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="equipo-departamento">Departamento</label>
              <select
                id="equipo-departamento"
                v-model="formularioEquipo.departamento"
                class="campo__input"
                :disabled="guardandoEquipo"
              >
                <option value="">Selecciona un departamento</option>
                <option
                  v-if="
                    formularioEquipo.departamento &&
                    !departamentos.some((d) => d.nombre === formularioEquipo.departamento)
                  "
                  :value="formularioEquipo.departamento"
                >
                  {{ formularioEquipo.departamento }} (no configurado)
                </option>
                <option v-for="departamento in departamentos" :key="departamento.id" :value="departamento.nombre">
                  {{ departamento.nombre }} ({{ departamento.sigla }})
                </option>
              </select>
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

    <div v-if="mostrarModalDepartamento" class="modal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-departamento-titulo">
        <form @submit.prevent="guardarDepartamento">
          <header class="modal__cabecera">
            <h3 id="modal-departamento-titulo" class="modal__titulo">
              {{ modoEdicionDepartamento ? 'Editar Departamento' : 'Nuevo Departamento' }}
            </h3>
            <button
              class="modal__cerrar"
              type="button"
              aria-label="Cerrar"
              :disabled="guardandoDepartamento"
              @click="cerrarModalDepartamento"
            >
              &times;
            </button>
          </header>

          <div class="modal__cuerpo">
            <div class="campo">
              <label class="campo__etiqueta" for="departamento-nombre">Nombre</label>
              <input
                id="departamento-nombre"
                v-model="formularioDepartamento.nombre"
                class="campo__input"
                type="text"
                placeholder="Ej. Recursos Humanos"
                maxlength="100"
                :disabled="guardandoDepartamento"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="departamento-sigla">Sigla</label>
              <input
                id="departamento-sigla"
                v-model="formularioDepartamento.sigla"
                class="campo__input"
                type="text"
                placeholder="Ej. RH"
                maxlength="10"
                :disabled="guardandoDepartamento"
              />
            </div>

            <div class="campo">
              <label class="campo__etiqueta" for="departamento-prioridad">Prioridad automática</label>
              <select
                id="departamento-prioridad"
                v-model="formularioDepartamento.prioridadPorDefecto"
                class="campo__input"
                :disabled="guardandoDepartamento"
              >
                <option v-for="prioridad in PRIORIDADES_DEPARTAMENTO" :key="prioridad" :value="prioridad">
                  {{ prioridad }}
                </option>
              </select>
            </div>

            <p v-if="errorFormularioDepartamento" class="alerta alerta--formulario">
              {{ errorFormularioDepartamento }}
            </p>
          </div>

          <footer class="modal__pie">
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="guardandoDepartamento"
              @click="cerrarModalDepartamento"
            >
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardandoDepartamento">
              {{
                guardandoDepartamento
                  ? 'Guardando...'
                  : modoEdicionDepartamento
                    ? 'Guardar Cambios'
                    : 'Guardar'
              }}
            </button>
          </footer>
        </form>
      </div>
    </div>

    <div v-if="mostrarModalCategoria" class="modal">
      <div class="modal__panel" role="dialog" aria-modal="true" aria-labelledby="modal-categoria-titulo">
        <form @submit.prevent="guardarCategoria">
          <header class="modal__cabecera">
            <h3 id="modal-categoria-titulo" class="modal__titulo">
              {{ modoEdicionCategoria ? 'Editar Categoría' : 'Nueva Categoría' }}
            </h3>
            <button
              class="modal__cerrar"
              type="button"
              aria-label="Cerrar"
              :disabled="guardandoCategoria"
              @click="cerrarModalCategoria"
            >
              &times;
            </button>
          </header>

          <div class="modal__cuerpo">
            <div class="campo">
              <label class="campo__etiqueta" for="categoria-nombre">Nombre</label>
              <input
                id="categoria-nombre"
                v-model="formularioCategoria.nombre"
                class="campo__input"
                type="text"
                placeholder="Ej. Telefonía"
                maxlength="100"
                :disabled="guardandoCategoria"
              />
            </div>

            <p v-if="errorFormularioCategoria" class="alerta alerta--formulario">
              {{ errorFormularioCategoria }}
            </p>
          </div>

          <footer class="modal__pie">
            <button
              class="boton boton--secundario"
              type="button"
              :disabled="guardandoCategoria"
              @click="cerrarModalCategoria"
            >
              Cancelar
            </button>
            <button class="boton boton--primario" type="submit" :disabled="guardandoCategoria">
              {{ guardandoCategoria ? 'Guardando...' : modoEdicionCategoria ? 'Guardar Cambios' : 'Guardar' }}
            </button>
          </footer>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.vista__encabezado { margin-bottom: 20px; }
.vista__titulo { margin: 0; font-size: 20px; font-weight: 600; color: #1a1f2b; }
.vista__subtitulo { margin: 4px 0 0; font-size: 13px; color: #6b7280; }
.pestanas { display: flex; flex-wrap: wrap; gap: 4px; margin-bottom: 16px; padding: 5px; background: #e6e9ee; border-radius: 8px; }
.pestana { flex: 1 1 140px; padding: 9px 14px; font-size: 13px; font-weight: 500; font-family: inherit; color: #667085; background: transparent; border: none; border-radius: 6px; cursor: pointer; white-space: nowrap; }
.pestana:hover { color: #1a1f2b; }
.pestana--activa { color: #1f3a68; background: #ffffff; box-shadow: 0 1px 3px rgba(16, 24, 40, 0.12); }
.tarjeta { background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); padding: 24px; margin-top: 20px; }
.tarjeta__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 20px; }
.tarjeta__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.tarjeta__nota { margin: 0 0 16px; font-size: 13px; color: #667085; }
.tarjeta__acciones { display: flex; align-items: center; gap: 10px; }
.filtros { display: flex; align-items: center; gap: 10px; margin-bottom: 16px; }
.filtros__busqueda { flex: 1 1 220px; min-width: 0; padding: 8px 12px; font-size: 13px; font-family: inherit; color: #1a1f2b; background: #ffffff; border: 1px solid #d0d5dd; border-radius: 6px; box-sizing: border-box; }
.filtros__busqueda:focus { outline: none; border-color: #1f3a68; box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12); }
.filtros__conteo { margin-left: auto; font-size: 12px; color: #667085; white-space: nowrap; }
.boton { padding: 8px 14px; font-size: 13px; font-weight: 500; font-family: inherit; border: 1px solid transparent; border-radius: 6px; cursor: pointer; }
.boton:disabled { cursor: not-allowed; opacity: 0.6; }
.boton--primario { color: #ffffff; background: #1f3a68; }
.boton--primario:hover:not(:disabled) { background: #16294a; }
.boton--secundario { color: #1f3a68; background: #ffffff; border-color: #d0d5dd; }
.boton--secundario:hover:not(:disabled) { background: #f8fafc; }
.alerta { margin: 16px 0 0; padding: 10px 12px; font-size: 13px; color: #b42318; background: #fef3f2; border: 1px solid #fecdca; border-radius: 6px; }
.alerta--formulario { margin: 0; }
.estado-vacio { padding: 48px 20px; text-align: center; }
.estado-vacio__texto { margin: 0; font-size: 14px; color: #6b7280; }
.tabla__envoltura { width: 100%; overflow-x: auto; }
.tabla { width: 100%; min-width: 900px; border-collapse: collapse; }
.tabla__th { padding: 12px 8px; font-size: 0.75rem; font-weight: 600; letter-spacing: 0.05em; text-align: left; text-transform: uppercase; color: #6b7280; background: transparent; border-bottom: 1px solid #e5e7eb; }
.tabla__th--corta, .tabla__th--acciones { width: 1%; white-space: nowrap; }
.tabla__fila:hover { background: #f9fafb; }
.tabla__td { padding: 16px 8px; font-size: 14px; text-align: left; color: #344054; border-bottom: 1px solid #f3f4f6; }
.tabla__fila:last-child .tabla__td { border-bottom: none; }
.tabla__td--id { font-variant-numeric: tabular-nums; color: #6b7280; }
.tabla__td--nombre { font-weight: 500; color: #1a1f2b; }
.tabla__td--ip { font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 13px; color: #475467; }
.tabla__td--acciones { text-align: right; white-space: nowrap; }
.badge { display: inline-block; padding: 4px 12px; font-size: 0.8rem; font-weight: 600; border-radius: 999px; white-space: nowrap; }
.badge--admin { color: #2563eb; background: #dbeafe; }
.badge--ingeniero { color: #7c3aed; background: #ede9fe; }
.badge--tecnico { color: #475467; background: #f2f4f7; }
.badge--activo { color: #16a34a; background: #dcfce7; }
.badge--inactivo { color: #6b7280; background: #f3f4f6; }
.accion { padding: 5px 12px; font-size: 12px; font-weight: 500; font-family: inherit; border-radius: 6px; cursor: pointer; }
.accion:disabled { opacity: 0.6; cursor: not-allowed; }
.accion--editar { color: #2563eb; background: #dbeafe; padding: 6px 12px; border-radius: 6px; border: none; font-weight: 600; cursor: pointer; }
.accion--editar:hover:not(:disabled) { background: #bfdbfe; border: none; }
.accion--activar { margin-left: 8px; color: #067647; background: #ecfdf3; border: 1px solid #a6f4c5; }
.accion--activar:hover:not(:disabled) { background: #d1fadf; border-color: #6ce9a6; }
.accion--desactivar { margin-left: 8px; color: #b54708; background: #fffaeb; border: 1px solid #fec84b; }
.accion--desactivar:hover:not(:disabled) { background: #fef0c7; border-color: #f79009; }
.accion--eliminar { margin-left: 8px; color: #dc2626; background: #fee2e2; padding: 6px 12px; border-radius: 6px; border: none; font-weight: 600; cursor: pointer; }
.accion--eliminar:hover:not(:disabled) { background: #fecaca; border: none; }
.modal { position: fixed; inset: 0; z-index: 50; display: flex; align-items: center; justify-content: center; padding: 24px; background: rgba(16, 24, 40, 0.55); }
.modal__panel { width: 100%; max-width: 460px; max-height: calc(100vh - 48px); overflow-y: auto; background: #ffffff; border-radius: 10px; box-shadow: 0 20px 40px rgba(16, 24, 40, 0.22); }
.modal__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 16px 20px; border-bottom: 1px solid #eceff3; }
.modal__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.modal__cerrar { padding: 0; width: 28px; height: 28px; font-size: 20px; line-height: 1; font-family: inherit; color: #667085; background: transparent; border: none; border-radius: 6px; cursor: pointer; }
.modal__cerrar:hover:not(:disabled) { color: #1a1f2b; background: #f2f4f7; }
.modal__cuerpo { padding: 18px 20px; }
.modal__pie { display: flex; justify-content: flex-end; gap: 10px; padding: 14px 20px; background: #f9fafb; border-top: 1px solid #eceff3; }
.campo { margin-bottom: 16px; }
.campo__etiqueta { display: block; margin-bottom: 6px; font-size: 13px; font-weight: 500; color: #344054; }
.campo__input { width: 100%; padding: 9px 12px; font-size: 14px; font-family: inherit; color: #1a1f2b; background: #ffffff; border: 1px solid #d0d5dd; border-radius: 6px; box-sizing: border-box; }
.campo__input:focus { outline: none; border-color: #1f3a68; box-shadow: 0 0 0 3px rgba(31, 58, 104, 0.12); }
.campo__input:disabled { background: #f2f4f7; cursor: not-allowed; }
.nota { margin-top: 6px; padding: 8px 10px; font-size: 12px; color: #b54708; background: #fffaeb; border: 1px solid #fec84b; border-radius: 6px; }
.interruptor { display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 500; color: #344054; cursor: pointer; }
.badge--prioridad-alta, .badge--prioridad-critica { color: #b42318; background: #fee4e2; }
.badge--prioridad-media { color: #b54708; background: #fef0c7; }
.badge--prioridad-baja { color: #067647; background: #d1fadf; }
.catalogos { display: grid; gap: 20px; }
.catalogos__bloque { padding: 18px; background: #fbfcfd; border: 1px solid #eceff3; border-radius: 10px; }
.catalogos__cabecera { display: flex; flex-wrap: wrap; align-items: flex-start; justify-content: space-between; gap: 12px; margin-bottom: 14px; }
.catalogos__titulo { margin: 0; font-size: 15px; font-weight: 600; color: #1a1f2b; }
.catalogos__nota { margin: 4px 0 0; font-size: 12px; color: #6b7280; }
</style>