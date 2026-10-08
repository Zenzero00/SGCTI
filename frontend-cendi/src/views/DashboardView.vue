<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const API_URL = 'http://localhost:5141/api/dashboard/resumen'

const TOKEN_KEY = 'sgcti_token'

interface AlertaSuministro {
  nombre: string
  cantidadActual: number
}

interface ActividadReciente {
  analista: string
  actividad: string
  hora: string
}

interface ResumenDashboard {
  totalTicketsAbiertos: number
  suministrosCriticos: number
  impresorasDesconectadas: number
  actividadesHoy: number
  alertasSuministros: AlertaSuministro[]
  alertasPredictivas: string[]
  actividadReciente: ActividadReciente[]
}

const router = useRouter()

const cargando = ref(true)
const error = ref('')
const resumen = ref<ResumenDashboard | null>(null)

function cerrarSesion() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/')
}

async function cargarResumen() {
  cargando.value = true
  error.value = ''

  try {
    const { data } = await axios.get<ResumenDashboard>(API_URL, {
      headers: {
        Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
      },
    })

    resumen.value = data
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 401) {
      cerrarSesion()
      return
    }

    error.value = 'No se pudo cargar el dashboard. Intenta nuevamente.'
  } finally {
    cargando.value = false
  }
}

function formatearFechaHora(iso: string): string {
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

onMounted(cargarResumen)
</script>

<template>
  <div class="vista">
    <header class="vista__encabezado">
      <div>
        <h1 class="vista__titulo">Centro de Mando</h1>
        <p class="vista__subtitulo">Resumen general de los módulos del sistema</p>
      </div>
      <button class="boton boton--secundario" type="button" :disabled="cargando" @click="cargarResumen">
        {{ cargando ? 'Cargando...' : 'Actualizar' }}
      </button>
    </header>

    <p v-if="error" class="alerta">{{ error }}</p>

    <div v-if="cargando" class="estado-vacio">
      <p class="estado-vacio__texto">Cargando indicadores...</p>
    </div>

    <template v-else-if="resumen">
      <section class="tarjetas">
        <div class="tarjeta">
          <span class="tarjeta__numero">{{ resumen.totalTicketsAbiertos }}</span>
          <span class="tarjeta__titulo">Tickets Abiertos</span>
        </div>
        <div class="tarjeta">
          <span class="tarjeta__numero">{{ resumen.suministrosCriticos }}</span>
          <span class="tarjeta__titulo">Suministros Críticos</span>
        </div>
        <div class="tarjeta">
          <span class="tarjeta__numero">{{ resumen.impresorasDesconectadas }}</span>
          <span class="tarjeta__titulo">Impresoras Desconectadas</span>
        </div>
        <div class="tarjeta">
          <span class="tarjeta__numero">{{ resumen.actividadesHoy }}</span>
          <span class="tarjeta__titulo">Actividades Hoy</span>
        </div>
      </section>

      <section class="paneles">
        <div class="panel panel--alerta">
          <header class="panel__cabecera">
            <h2 class="panel__titulo">Atención Requerida</h2>
          </header>
          <p
            v-if="resumen.alertasSuministros.length === 0 && resumen.alertasPredictivas.length === 0"
            class="panel__vacio"
          >
            ✅ Todo en orden
          </p>

          <ul v-if="resumen.alertasSuministros.length > 0" class="lista">
            <li v-for="alerta in resumen.alertasSuministros" :key="alerta.nombre" class="lista__item lista__item--alerta">
              <span class="lista__nombre">{{ alerta.nombre }}</span>
              <span class="lista__dato">Stock actual: {{ alerta.cantidadActual }}</span>
            </li>
          </ul>

          <ul v-if="resumen.alertasPredictivas.length > 0" class="lista">
            <li
              v-for="(alerta, indice) in resumen.alertasPredictivas"
              :key="indice"
              class="lista__item lista__item--predictiva"
            >
              <span class="lista__mensaje">{{ alerta }}</span>
            </li>
          </ul>
        </div>

        <div class="panel">
          <header class="panel__cabecera">
            <h2 class="panel__titulo">Actividad Reciente</h2>
          </header>
          <p v-if="resumen.actividadReciente.length === 0" class="panel__vacio">
            Sin actividad registrada todavía.
          </p>
          <ul v-else class="lista">
            <li v-for="actividad in resumen.actividadReciente" :key="actividad.hora" class="lista__item">
              <div class="lista__actividad">
                <span class="lista__nombre">{{ actividad.actividad }}</span>
                <span class="lista__dato">{{ actividad.analista }}</span>
              </div>
              <span class="lista__hora">{{ formatearFechaHora(actividad.hora) }}</span>
            </li>
          </ul>
        </div>
      </section>
    </template>
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

.boton--secundario {
  color: #1f3a68;
  background: #ffffff;
  border-color: #d0d5dd;
}

.boton--secundario:hover:not(:disabled) {
  background: #f8fafc;
}

.alerta {
  margin: 16px 0;
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

.tarjetas {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 20px;
  overflow-x: auto;
}

.tarjeta {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 20px;
  background: #ffffff;
  border: 1px solid #e2e5ea;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(16, 24, 40, 0.06);
}

.tarjeta__numero {
  font-size: 32px;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  color: #1a1f2b;
}

.tarjeta__titulo {
  font-size: 13px;
  font-weight: 500;
  color: #667085;
}

.paneles {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  align-items: start;
}

.panel {
  background: #ffffff;
  border: 1px solid #e2e5ea;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(16, 24, 40, 0.06);
  overflow: hidden;
}

.panel--alerta {
  border-color: #fecdca;
}

.panel__cabecera {
  padding: 14px 20px;
  border-bottom: 1px solid #eceff3;
}

.panel--alerta .panel__cabecera {
  background: #fef3f2;
}

.panel__titulo {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: #1a1f2b;
}

.panel__vacio {
  margin: 0;
  padding: 28px 20px;
  font-size: 14px;
  color: #667085;
}

.lista {
  margin: 0;
  padding: 8px 0;
  list-style: none;
}

.lista__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 20px;
  border-bottom: 1px solid #f2f4f7;
}

.lista__item:last-child {
  border-bottom: none;
}

.lista__item--alerta {
  color: #b42318;
  background: #fffbfa;
}

.lista__item--predictiva {
  color: #b54708;
  background: #fffaeb;
}

.lista__mensaje {
  font-size: 13px;
  font-weight: 500;
  color: #b54708;
}

.lista__nombre {
  font-size: 14px;
  font-weight: 500;
  color: #1a1f2b;
}

.lista__item--alerta .lista__nombre {
  color: #b42318;
}

.lista__dato {
  font-size: 13px;
  color: #667085;
}

.lista__item--alerta .lista__dato {
  font-weight: 600;
}

.lista__actividad {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.lista__hora {
  flex-shrink: 0;
  font-size: 12px;
  color: #667085;
  font-variant-numeric: tabular-nums;
}
</style>