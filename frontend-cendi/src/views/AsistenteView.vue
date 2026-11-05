<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'

const API_BASE = ''
const TOKEN_KEY = 'sgcti_token'
axios.defaults.baseURL = 'http://localhost:5141'

interface Mensaje {
  autor: 'usuario' | 'ia'
  texto: string
}

const HISTORIAL_KEY = 'sgcti_chat_history_v2'

const PREGUNTAS_RAPIDAS = [
  '📱 ¿Cómo vinculo un empleado en el Bot de Telegram?',
  '🤖 ¿Cuál es el comando para registrar un teléfono desde Telegram?',
  '🔔 ¿Cómo funcionan los códigos y las alertas sonoras de tickets?',
  '📦 ¿Qué suministros están en stock crítico?',
] as const

const MENSAJE_BIENVENIDA: Mensaje = {
  autor: 'ia',
  texto:
    'Hola, soy el Asistente Inteligente TI integrado en SGCTI. ¿En qué te puedo ayudar?\n\n' +
    '¿Para qué sirvo? Te ayudo a:\n' +
    '1) Verificar en tiempo real si aún quedan artículos, tóneres o repuestos en el inventario y cuáles están en nivel crítico.\n' +
    '2) Consultar el estado y niveles de las impresoras en red.\n' +
    '3) Leer e interpretar los manuales técnicos en PDF para resolver fallas o códigos de error (usando el botón "Cargar Manual").\n' +
    '4) Guiar paso a paso a los técnicos de TI sobre cómo configurar y vincular teléfonos de empleados en el Bot de Telegram (desde la web o con el comando /registrar) y cómo usar las alertas de la Mesa de Ayuda.',
}

const route = useRoute()
const equipo = route.query.equipo
const impresoraIdQuery = route.query.id
const impresoraId = impresoraIdQuery ? Number(impresoraIdQuery) : 0

const mensajes = ref<Mensaje[]>([])

onMounted(() => {
  try {
    const guardado = localStorage.getItem(HISTORIAL_KEY)
    if (guardado) {
      const parseado = JSON.parse(guardado) as Mensaje[]
      if (Array.isArray(parseado) && parseado.length > 0) {
        mensajes.value = parseado
        nextTick(() => {
          desplazarAlFinal()
        })
        return
      }
    }
  } catch {
    // Ignorar errores de lectura de localStorage
  }

  mensajes.value = [{ ...MENSAJE_BIENVENIDA }]
  guardarHistorial()
})

function guardarHistorial() {
  try {
    localStorage.setItem(HISTORIAL_KEY, JSON.stringify(mensajes.value))
  } catch {
    // Ignorar errores de almacenamiento
  }
}

function reiniciarConversacion() {
  mensajes.value = [{ ...MENSAJE_BIENVENIDA }]
  guardarHistorial()
  nextTick(() => {
    desplazarAlFinal()
  })
}

function enviarPreguntaRapida(pregunta: string) {
  if (cargando.value) return
  texto.value = pregunta
  enviar()
}

const texto = ref(
  typeof equipo === 'string'
    ? `Por favor, analiza las posibles fallas del equipo modelo: ${equipo}`
    : '',
)

const cargando = ref(false)

const mensajesEl = ref<HTMLElement | null>(null)
const entradaEl = ref<HTMLTextAreaElement | null>(null)
const archivoInputEl = ref<HTMLInputElement | null>(null)
const impresoraSeleccionada = ref<number | null>(
  Number.isInteger(impresoraId) && impresoraId > 0 ? impresoraId : null,
)

function ajustarAltura() {
  const el = entradaEl.value
  if (!el) return
  el.style.height = 'auto'
  el.style.height = `${Math.min(el.scrollHeight, 140)}px`
}

function desplazarAlFinal() {
  const contenedor = mensajesEl.value
  if (contenedor) {
    contenedor.scrollTop = contenedor.scrollHeight
  }
}

function mensajeDeError(e: unknown): string {
  if (axios.isAxiosError(e)) {
    if (!e.response) {
      return 'No se pudo conectar con el servidor.'
    }

    const cuerpo = e.response.data
    if (typeof cuerpo === 'string' && cuerpo.trim().length > 0) {
      return cuerpo
    }

    if (typeof cuerpo === 'object' && cuerpo !== null && 'mensaje' in cuerpo) {
      const mensaje = (cuerpo as { mensaje?: string }).mensaje
      if (mensaje && mensaje.trim().length > 0) {
        return mensaje
      }
    }

    return 'La IA no pudo procesar la consulta.'
  }

  return 'Ocurrió un error inesperado.'
}

function abrirSelectorArchivo() {
  archivoInputEl.value?.click()
}

async function subirManual() {
  const archivo = archivoInputEl.value?.files?.[0]
  if (!archivo || cargando.value) return

  if (archivo.type !== 'application/pdf' && !archivo.name.toLowerCase().endsWith('.pdf')) {
    mensajes.value.push({ autor: 'ia', texto: 'El archivo debe ser un PDF.' })
    if (archivoInputEl.value) archivoInputEl.value.value = ''
    return
  }

  mensajes.value.push({ autor: 'usuario', texto: `[Manual PDF] ${archivo.name}` })
  cargando.value = true

  await nextTick()
  ajustarAltura()
  desplazarAlFinal()

  try {
    const formData = new FormData()
    formData.append('archivo', archivo)
    if (impresoraSeleccionada.value) {
      formData.append('impresoraId', impresoraSeleccionada.value.toString())
    }

    const { data } = await axios.post(
      '/api/Asistente/manuales',
      formData,
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    const mensaje =
      (data as { mensaje?: string })?.mensaje ||
      'Manual cargado correctamente. Ya puede ser consultado desde el chat.'
    mensajes.value.push({ autor: 'ia', texto: mensaje })
    guardarHistorial()
  } catch (e) {
    mensajes.value.push({ autor: 'ia', texto: mensajeDeError(e) })
    guardarHistorial()
  } finally {
    cargando.value = false
    if (archivoInputEl.value) archivoInputEl.value.value = ''
    await nextTick()
    desplazarAlFinal()
  }
}

async function enviar() {
  const contenido = texto.value.trim()
  if (!contenido || cargando.value) return

  mensajes.value.push({ autor: 'usuario', texto: contenido })
  guardarHistorial()
  texto.value = ''
  cargando.value = true

  await nextTick()
  ajustarAltura()
  desplazarAlFinal()

  try {
    const endpoint =
      Number.isInteger(impresoraSeleccionada.value) &&
      impresoraSeleccionada.value &&
      impresoraSeleccionada.value > 0
        ? `${API_BASE}/api/impresoras/${impresoraSeleccionada.value}/chat`
        : '/api/Asistente/chat'

    const historialParaEnviar = mensajes.value
      .filter((m) => m.texto && m.texto.trim().length > 0)
      .map((m) => ({ autor: m.autor, texto: m.texto }))

    const { data } = await axios.post<{ respuesta: string }>(
      endpoint,
      { pregunta: contenido, historial: historialParaEnviar },
      {
        headers: {
          Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
        },
      },
    )

    mensajes.value.push({ autor: 'ia', texto: data.respuesta })
    guardarHistorial()
  } catch (e) {
    mensajes.value.push({ autor: 'ia', texto: mensajeDeError(e) })
    guardarHistorial()
  } finally {
    cargando.value = false
    await nextTick()
    desplazarAlFinal()
  }
}
</script>

<template>
  <div class="chat-vista">
    <div class="chat">
      <header class="chat__cabecera">
        <div class="chat__cabecera-info">
          <h1 class="chat__titulo">Asistente Inteligente TI</h1>
          <p class="chat__subtitulo">
            Consulta sobre inventario, impresoras, manuales, Bot de Telegram y servicios de TI
          </p>
        </div>
        <div class="chat__cabecera-acciones">
          <label class="chat__selector">
            <span class="chat__selector-label">Vincular a impresora</span>
            <select v-model="impresoraSeleccionada" :disabled="cargando">
              <option :value="null">Consulta general</option>
              <option v-if="Number.isInteger(impresoraId) && impresoraId > 0" :value="impresoraId">
                Impresora desde ficha (#{{ impresoraId }})
              </option>
            </select>
          </label>
          <button
            class="chat__limpiar"
            type="button"
            :disabled="cargando"
            @click="reiniciarConversacion"
          >
            Limpiar Chat
          </button>
        </div>
      </header>

      <div ref="mensajesEl" class="chat__mensajes">
        <div
          v-for="(mensaje, indice) in mensajes"
          :key="indice"
          class="burbuja"
          :class="mensaje.autor === 'usuario' ? 'burbuja--usuario' : 'burbuja--ia'"
        >
          <p class="burbuja__texto">{{ mensaje.texto }}</p>
        </div>

        <div v-if="cargando" class="burbuja-ia burbuja-cargando">
          El asistente está escribiendo...
        </div>
      </div>

      <div class="chat__sugerencias">
        <button
          v-for="sugerencia in PREGUNTAS_RAPIDAS"
          :key="sugerencia"
          type="button"
          class="chat__sugerencia-chip"
          :disabled="cargando"
          @click="enviarPreguntaRapida(sugerencia)"
        >
          {{ sugerencia }}
        </button>
      </div>

      <form class="chat__entrada" @submit.prevent="enviar">
        <input
          ref="archivoInputEl"
          type="file"
          accept="application/pdf,.pdf"
          class="chat__archivo"
          @change="subirManual"
        />
        <button
          class="chat__adjuntar"
          type="button"
          :disabled="cargando"
          aria-label="Cargar manual PDF"
          @click="abrirSelectorArchivo"
        >
          <i class="ph ph-paperclip"></i>
          <span>Cargar Manual</span>
        </button>
        <textarea
          ref="entradaEl"
          v-model="texto"
          class="chat__campo"
          rows="1"
          placeholder="Pregunta sobre Telegram, inventario, impresoras o manuales..."
          :disabled="cargando"
          @input="ajustarAltura"
          @keydown.enter.exact.prevent="enviar"
        ></textarea>
        <button
          class="chat__enviar"
          type="submit"
          :disabled="!texto.trim() || cargando"
          aria-label="Enviar consulta"
        >
          <i class="ph ph-paper-plane-right"></i>
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.chat-vista {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: calc(100vh - 80px);
  overflow: hidden;
  padding: 2rem;
  background-color: #e2e8f0;
  border-radius: 16px;
}

.chat {
  display: flex;
  flex: 1;
  flex-direction: column;
  width: 100%;
  max-width: 850px;
  margin: 0 auto;
  overflow: hidden;
  background-color: #ffffff;
  border: 1px solid #cbd5e1;
  border-radius: 16px;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
}

.chat__cabecera {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  padding: 16px 24px;
  border-bottom: 1px solid #eef2f7;
}

.chat__cabecera-info {
  display: flex;
  flex-direction: column;
}

.chat__cabecera-acciones {
  display: flex;
  align-items: flex-end;
  gap: 10px;
}

.chat__titulo {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #1a1f2b;
}

.chat__subtitulo {
  margin: 4px 0 0;
  font-size: 13px;
  color: #6b7280;
}

.chat__selector {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 12px;
  color: #475569;
}

.chat__selector select {
  padding: 6px 10px;
  border-radius: 8px;
  border: 1px solid #cbd5e1;
  font-size: 13px;
}

.chat__selector-label {
  font-weight: 600;
}

.chat__limpiar {
  padding: 7px 12px;
  font-size: 12.5px;
  font-weight: 600;
  font-family: inherit;
  color: #475569;
  background: #f8fafc;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  cursor: pointer;
}

.chat__limpiar:hover:not(:disabled) {
  color: #1e293b;
  background: #f1f5f9;
}

.chat__mensajes {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 12px;
  padding: 20px;
  overflow-y: auto;
  background-color: #f8fafc;
  border-top: 1px solid #e2e8f0;
  border-bottom: 1px solid #e2e8f0;
}

.burbuja {
  display: flex;
  max-width: 78%;
}

.burbuja--usuario {
  align-self: flex-end;
}

.burbuja--ia {
  align-self: flex-start;
}

.burbuja__texto {
  margin: 0;
  padding: 10px 14px;
  font-size: 14px;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-word;
  border-radius: 14px;
}

.burbuja--usuario .burbuja__texto {
  color: #ffffff;
  background: #1d4ed8;
  border-bottom-right-radius: 4px;
}

.burbuja--ia .burbuja__texto {
  color: #1a1f2b;
  background-color: #ffffff;
  border: 1px solid #e2e8f0;
  border-bottom-left-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.burbuja-cargando {
  align-self: flex-start;
  padding: 10px 14px;
  font-size: 14px;
  font-style: italic;
  color: #6b7280;
  background-color: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  border-bottom-left-radius: 4px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.chat__sugerencias {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  padding: 10px 16px 4px;
  background-color: #ffffff;
}

.chat__sugerencia-chip {
  padding: 6px 12px;
  font-size: 12px;
  font-weight: 500;
  font-family: inherit;
  color: #1f3a68;
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: 999px;
  cursor: pointer;
  transition: background-color 0.15s;
}

.chat__sugerencia-chip:hover:not(:disabled) {
  background: #dbeafe;
}

.chat__sugerencia-chip:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.chat__entrada {
  display: flex;
  align-items: flex-end;
  gap: 10px;
  padding: 12px 16px 16px;
  background-color: #ffffff;
  flex-wrap: wrap;
}

.chat__archivo {
  display: none;
}

.chat__adjuntar {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 14px;
  font-size: 13px;
  font-weight: 600;
  color: #1e293b;
  background: #f8fafc;
  border: 1px solid #cbd5e1;
  border-radius: 999px;
  cursor: pointer;
  transition:
    background-color 0.2s,
    border-color 0.2s;
}

.chat__adjuntar:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.chat__adjuntar:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.chat__campo {
  flex: 1 1 200px;
  max-height: 140px;
  padding: 12px 20px;
  font-size: 14px;
  font-family: inherit;
  line-height: 1.4;
  color: #0f172a;
  background-color: #f8fafc;
  border: 1px solid #cbd5e1;
  border-radius: 24px;
  resize: none;
  box-sizing: border-box;
  transition:
    border-color 0.2s,
    box-shadow 0.2s;
}

.chat__campo:focus {
  outline: none;
  border-color: #1d4ed8;
  box-shadow: 0 0 0 3px rgba(29, 78, 216, 0.15);
}

.chat__enviar {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  font-size: 20px;
  color: #ffffff;
  background: #1d4ed8;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  transition:
    background-color 0.2s,
    transform 0.1s;
}

.chat__enviar:hover:not(:disabled) {
  background: #1e40af;
}

.chat__enviar:active:not(:disabled) {
  transform: scale(0.95);
}

.chat__enviar:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}
</style>