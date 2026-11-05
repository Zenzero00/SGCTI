<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Filler,
} from 'chart.js'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Filler)

const API_URL = 'http://localhost:5141/api/dashboard/resumen'

const TOKEN_KEY = 'sgcti_token'

interface AlertaSuministro {
  nombre: string
  tipo: string
  cantidadActual: number
  stockMinimo: number
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

const DIAS_SEMANA = [
  { corto: 'Lun', nombre: 'Lunes' },
  { corto: 'Mar', nombre: 'Martes' },
  { corto: 'Mié', nombre: 'Miércoles' },
  { corto: 'Jue', nombre: 'Jueves' },
  { corto: 'Vie', nombre: 'Viernes' },
  { corto: 'Sáb', nombre: 'Sábado' },
  { corto: 'Dom', nombre: 'Domingo' },
]

function formatearCorta(fecha: Date): string {
  const dia = String(fecha.getDate()).padStart(2, '0')
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  return `${dia}/${mes}`
}

function formatearCompleta(fecha: Date): string {
  const dia = String(fecha.getDate()).padStart(2, '0')
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  return `${dia}/${mes}/${fecha.getFullYear()}`
}

function inicioDeSemana(fecha: Date): Date {
  const inicio = new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate())
  const diaSemana = inicio.getDay()
  const desplazamiento = diaSemana === 0 ? -6 : 1 - diaSemana
  inicio.setDate(inicio.getDate() + desplazamiento)
  return inicio
}

const inicioSemana = inicioDeSemana(new Date())
const finSemana = new Date(inicioSemana)
finSemana.setDate(finSemana.getDate() + 6)

const subtituloSemana = computed(
  () => `Semana del ${formatearCompleta(inicioSemana)} al ${formatearCompleta(finSemana)}`,
)

const etiquetasSemana = DIAS_SEMANA.map((dia, indice) => {
  const fecha = new Date(inicioSemana)
  fecha.setDate(fecha.getDate() + indice)
  return `${dia.corto} ${formatearCorta(fecha)}`
})

const chartData = {
  labels: etiquetasSemana,
  datasets: [
    {
      label: 'Páginas impresas',
      data: [850, 920, 610, 780, 1050, 1420, 990],
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.2)',
      fill: true,
      tension: 0.4,
    },
  ],
}

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false,
    },
  },
  scales: {
    x: {
      grid: {
        color: 'rgba(17, 24, 39, 0.05)',
      },
    },
    y: {
      grid: {
        color: 'rgba(17, 24, 39, 0.05)',
      },
    },
  },
}

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
    <div class="banner">
      <div class="banner__textos">
        <h1 class="banner__saludo">¡Hola, Administrador!</h1>
        <p class="banner__subtitulo">
          Aquí tienes un resumen del estado actual del departamento de TI.
        </p>
      </div>
      <div class="banner__acciones">
        <p class="banner__frase">Tecnología que mantiene tu operación en movimiento</p>
        <button class="banner__boton" type="button" :disabled="cargando" @click="cargarResumen">
          {{ cargando ? 'Cargando...' : '↻ Actualizar' }}
        </button>
      </div>
    </div>

    <p v-if="error" class="alerta">{{ error }}</p>

    <div v-if="cargando" class="estado-vacio">
      <p class="estado-vacio__texto">Cargando indicadores...</p>
    </div>

    <template v-else-if="resumen">
      <div class="grid-metricas">
        <article class="metrica">
          <div class="metrica__cabecera">
            <div class="metrica__icono metrica__icono--tickets">
              <i class="ph ph-ticket"></i>
            </div>
            <span class="metrica__titulo">Tickets Abiertos</span>
          </div>
          <span class="metrica__valor">{{ resumen.totalTicketsAbiertos }}</span>
          <span class="metrica__pie">vs. semana anterior</span>
        </article>

        <article class="metrica">
          <div class="metrica__cabecera">
            <div class="metrica__icono metrica__icono--suministros">
              <i class="ph ph-package"></i>
            </div>
            <span class="metrica__titulo">Suministros Críticos</span>
          </div>
          <span class="metrica__valor">{{ resumen.suministrosCriticos }}</span>
          <span class="metrica__pie">vs. semana anterior</span>
        </article>

        <article class="metrica">
          <div class="metrica__cabecera">
            <div class="metrica__icono metrica__icono--impresoras">
              <i class="ph ph-printer"></i>
            </div>
            <span class="metrica__titulo">Impresoras Desconectadas</span>
          </div>
          <span class="metrica__valor">{{ resumen.impresorasDesconectadas }}</span>
          <span class="metrica__pie">vs. semana anterior</span>
        </article>

        <article class="metrica">
          <div class="metrica__cabecera">
            <div class="metrica__icono metrica__icono--actividades">
              <i class="ph ph-list-checks"></i>
            </div>
            <span class="metrica__titulo">Actividades Hoy</span>
          </div>
          <span class="metrica__valor">{{ resumen.actividadesHoy }}</span>
          <span class="metrica__pie">vs. semana anterior</span>
        </article>
      </div>

      <div class="grid-medio">
        <section class="tarjeta">
          <h2 class="tarjeta__titulo">Consumo de Impresoras</h2>
          <p class="tarjeta__subtitulo-grafica">{{ subtituloSemana }}</p>
          <div style="position: relative; height: 300px; width: 100%;">
            <Line :data="chartData" :options="chartOptions" />
          </div>
        </section>

        <section class="tarjeta">
          <h2 class="tarjeta__titulo">Estado de Inventario</h2>
          <div class="dona">
            <div class="dona__anillo">
              <div class="dona__centro">
                <span class="dona__valor">{{ resumen.suministrosCriticos }}</span>
                <span class="dona__etiqueta">Críticos</span>
              </div>
            </div>
          </div>
          <ul class="leyenda">
            <li class="leyenda__item">
              <span class="leyenda__punto leyenda__punto--critico"></span>
              En nivel crítico
            </li>
            <li class="leyenda__item">
              <span class="leyenda__punto leyenda__punto--normal"></span>
              Resto del inventario
            </li>
          </ul>
        </section>

        <section class="tarjeta tarjeta--alertas">
          <h2 class="tarjeta__titulo tarjeta__titulo--alerta">
            ⚠️ Alertas Predictivas ({{ resumen.alertasPredictivas.length + resumen.alertasSuministros.length }})
          </h2>

          <div class="alertas__scroll">
            <p
              v-if="resumen.alertasPredictivas.length === 0 && resumen.alertasSuministros.length === 0"
              class="tarjeta__vacio"
            >
              ✅ Todo en orden (Sin alertas críticas activas)
            </p>

            <div v-if="resumen.alertasPredictivas.length > 0" class="alertas">
              <div
                v-for="(alerta, indice) in resumen.alertasPredictivas"
                :key="indice"
                class="alertas__fila"
              >
                <div class="alertas__principal">
                  <i class="ph ph-warning-circle alertas__icono"></i>
                  <div class="alertas__textos">
                    <span class="alertas__titulo">{{ alerta }}</span>
                    <span class="alertas__subtitulo">Detección automática del sistema</span>
                  </div>
                </div>
                <span class="alertas__badge">Alta</span>
              </div>
            </div>

            <div v-if="resumen.alertasSuministros.length > 0" class="alertas">
              <div
                v-for="alerta in resumen.alertasSuministros"
                :key="alerta.nombre"
                class="alertas__fila"
              >
                <div class="alertas__principal">
                  <i class="ph ph-warning-circle alertas__icono"></i>
                  <div class="alertas__textos">
                    <span class="alertas__titulo">{{ alerta.nombre }} - {{ alerta.tipo }}</span>
                    <span class="alertas__subtitulo">
                      Stock actual: {{ alerta.cantidadActual }} · Mínimo: {{ alerta.stockMinimo }}
                    </span>
                  </div>
                </div>
                <span class="alertas__badge">Stock bajo</span>
              </div>
            </div>
          </div>
        </section>
      </div>

      <div class="grid-inferior">
        <section class="tarjeta">
          <h2 class="tarjeta__titulo">Actividad Reciente</h2>

          <p v-if="resumen.actividadReciente.length === 0" class="tarjeta__vacio">
            Sin actividad registrada todavía.
          </p>

          <ul v-else class="actividad">
            <li v-for="actividad in resumen.actividadReciente" :key="actividad.hora" class="actividad__item">
              <div class="actividad__principal">
                <i class="ph ph-check-circle actividad__icono"></i>
                <div class="actividad__textos">
                  <span class="actividad__nombre">{{ actividad.actividad }}</span>
                  <span class="actividad__analista">{{ actividad.analista }}</span>
                </div>
              </div>
              <span class="actividad__hora">{{ formatearFechaHora(actividad.hora) }}</span>
            </li>
          </ul>
        </section>

        <aside class="cerebro">
          <span class="cerebro__icono">🧠</span>
          <h2 class="cerebro__titulo">Cerebro Predictivo</h2>
          <p class="cerebro__texto">
            El sistema analiza el consumo para anticipar agotamientos.
          </p>
          <RouterLink class="cerebro__boton" to="/panel/impresoras-consumo">
            Ver análisis
          </RouterLink>
        </aside>
      </div>
    </template>
  </div>
</template>

<style scoped>
.vista { display: block; }
.banner { display: flex; align-items: center; justify-content: space-between; gap: 20px; padding: 24px; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); }
.banner__saludo { margin: 0; font-size: 26px; font-weight: 700; color: #111827; }
.banner__subtitulo { margin: 6px 0 0; font-size: 14px; color: #9ca3af; }
.banner__acciones { display: flex; align-items: center; gap: 12px; flex-shrink: 0; }
.banner__frase { margin: 0; padding: 14px 18px; max-width: 260px; font-size: 13px; font-weight: 600; line-height: 1.45; color: #ffffff; background: linear-gradient(135deg, #1e3a8a, #3b82f6); border-radius: 12px; }
.banner__boton { flex-shrink: 0; padding: 10px 18px; font-size: 13px; font-weight: 600; color: #ffffff; background: #1a2238; border: none; border-radius: 8px; cursor: pointer; transition: background 0.15s ease; }
.banner__boton:hover:not(:disabled) { background: #27314f; }
.banner__boton:disabled { cursor: not-allowed; opacity: 0.6; }
.alerta { margin: 20px 0 0; padding: 12px 14px; font-size: 13px; color: #b42318; background: #fef3f2; border: 1px solid #fecdca; border-radius: 8px; }
.estado-vacio { margin-top: 20px; padding: 48px 20px; text-align: center; background: #ffffff; border: 1px solid #e5e8ee; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); }
.estado-vacio__texto { margin: 0; font-size: 14px; color: #6b7280; }
.grid-metricas { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin-top: 20px; }
.metrica { display: flex; flex-direction: column; padding: 20px; background: #ffffff; border-radius: 12px; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.03); }
.metrica__cabecera { display: flex; align-items: center; gap: 12px; }
.metrica__icono { display: flex; align-items: center; justify-content: center; width: 48px; height: 48px; flex-shrink: 0; font-size: 24px; border-radius: 12px; }
.metrica__icono--tickets { color: #2563eb; background: #e0f2fe; }
.metrica__icono--suministros { color: #ea580c; background: #ffedd5; }
.metrica__icono--impresoras { color: #9333ea; background: #f3e8ff; }
.metrica__icono--actividades { color: #16a34a; background: #dcfce7; }
.metrica__titulo { font-size: 12.5px; font-weight: 500; color: #6b7280; }
.metrica__valor { margin-top: 15px; font-size: 2.5rem; font-weight: 700; color: #111827; line-height: 1.1; }
.metrica__pie { margin-top: 4px; font-size: 11.5px; color: #98a2b3; }
.grid-medio { display: grid; grid-template-columns: 2fr 1fr 1.5fr; gap: 20px; margin-top: 20px; align-items: stretch; }
.tarjeta { padding: 20px; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); }
.tarjeta--alertas { display: flex; flex-direction: column; height: 390px; box-sizing: border-box; }
.alertas__scroll { flex: 1; overflow-y: auto; padding-right: 6px; }
.alertas__scroll::-webkit-scrollbar { width: 6px; }
.alertas__scroll::-webkit-scrollbar-track { background: #f1f5f9; border-radius: 999px; }
.alertas__scroll::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 999px; }
.alertas__scroll::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.tarjeta__titulo { margin: 0 0 16px; font-size: 14.5px; font-weight: 600; color: #1a2238; flex-shrink: 0; }
.tarjeta__titulo--alerta { color: #d92d20; }
.tarjeta__subtitulo-grafica { margin: -8px 0 12px; font-size: 12.5px; font-weight: 500; color: #6b7280; }
.tarjeta__vacio { margin: 0; padding: 24px 0; font-size: 13px; color: #6b7280; text-align: center; }
.dona { display: flex; justify-content: center; padding: 12px 0 4px; }
.dona__anillo { display: flex; align-items: center; justify-content: center; width: 150px; height: 150px; background: conic-gradient(#f79009 0deg 90deg, #eef1f5 90deg 360deg); border-radius: 50%; }
.dona__centro { display: flex; align-items: center; flex-direction: column; justify-content: center; width: 106px; height: 106px; background: #ffffff; border-radius: 50%; }
.dona__valor { font-size: 28px; font-weight: 700; color: #1a2238; line-height: 1; }
.dona__etiqueta { margin-top: 4px; font-size: 11.5px; color: #6b7280; }
.leyenda { display: flex; flex-direction: column; gap: 8px; margin: 12px 0 0; padding: 0; list-style: none; }
.leyenda__item { display: flex; align-items: center; gap: 8px; font-size: 12.5px; color: #475467; }
.leyenda__punto { width: 9px; height: 9px; flex-shrink: 0; border-radius: 50%; }
.leyenda__punto--critico { background: #f79009; }
.leyenda__punto--normal { background: #eef1f5; border: 1px solid #d0d5dd; }
.alertas { display: flex; flex-direction: column; }
.alertas + .alertas { margin-top: 6px; }
.alertas__fila { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px 0; border-bottom: 1px solid #f3f4f6; }
.alertas__principal { display: flex; align-items: center; gap: 12px; min-width: 0; }
.alertas__icono { font-size: 22px; color: #ef4444; flex-shrink: 0; }
.alertas__textos { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.alertas__titulo { font-size: 13px; font-weight: 600; color: #111827; }
.alertas__subtitulo { font-size: 11.5px; color: #6b7280; }
.alertas__badge { flex-shrink: 0; padding: 4px 10px; font-size: 0.75rem; font-weight: 600; color: #dc2626; background-color: #fee2e2; border-radius: 999px; }
.grid-inferior { display: grid; grid-template-columns: 2fr 1fr; gap: 20px; margin-top: 20px; }
.actividad { margin: 0; padding: 0; list-style: none; }
.actividad__item { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 15px 0; border-bottom: 1px solid #f3f4f6; }
.actividad__item:last-child { border-bottom: none; padding-bottom: 0; }
.actividad__principal { display: flex; align-items: center; gap: 12px; min-width: 0; }
.actividad__icono { font-size: 24px; color: #16a34a; flex-shrink: 0; }
.actividad__textos { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.actividad__nombre { font-size: 13.5px; font-weight: 600; color: #111827; }
.actividad__analista { font-size: 12.5px; color: #6b7280; }
.actividad__hora { flex-shrink: 0; font-size: 0.85rem; color: #6b7280; font-variant-numeric: tabular-nums; }
.cerebro { display: flex; flex-direction: column; align-items: flex-start; gap: 10px; padding: 24px; background: #1a2238; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05); }
.cerebro__icono { font-size: 24px; line-height: 1; }
.cerebro__titulo { margin: 0; font-size: 16px; font-weight: 700; color: #ffffff; }
.cerebro__texto { margin: 0; font-size: 13.5px; line-height: 1.55; color: #c7cede; }
.cerebro__boton { display: inline-block; margin-top: auto; padding: 10px 18px; font-size: 13px; font-weight: 600; color: #1a2238; text-decoration: none; background: #ffffff; border-radius: 8px; transition: background 0.15s ease, transform 0.15s ease; }
.cerebro__boton:hover { background: #eef4ff; transform: translateY(-1px); }
@media (max-width: 1200px) { .grid-medio { grid-template-columns: 1fr 1fr; } .grid-inferior { grid-template-columns: 1fr; } }
@media (max-width: 1024px) { .grid-metricas { grid-template-columns: repeat(2, 1fr); } }
@media (max-width: 768px) { .grid-metricas, .grid-medio { grid-template-columns: 1fr; } }
@media (max-width: 640px) { .banner { flex-direction: column; align-items: flex-start; padding: 20px; } .banner__saludo { font-size: 21px; } .banner__acciones { flex-wrap: wrap; } }
</style>