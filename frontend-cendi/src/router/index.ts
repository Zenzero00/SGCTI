import { createRouter, createWebHistory } from 'vue-router'

import LoginView from '@/views/LoginView.vue'
import PanelView from '@/views/PanelView.vue'
import DashboardView from '@/views/DashboardView.vue'
import TicketsView from '@/views/TicketsView.vue'
import ImpresorasView from '@/views/ImpresorasView.vue'
import ConsumoImpresorasView from '@/views/ConsumoImpresorasView.vue'
import SuministrosView from '@/views/SuministrosView.vue'
import BitacoraView from '@/views/BitacoraView.vue'
import UsuariosView from '@/views/UsuariosView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/panel',
      name: 'panel',
      component: PanelView,
      children: [
        {
          path: '',
          name: 'dashboard',
          component: DashboardView,
        },
        {
          path: 'tickets',
          name: 'tickets',
          component: TicketsView,
        },
        {
          path: 'impresoras',
          name: 'impresoras',
          component: ImpresorasView,
        },
        {
          path: 'impresoras-consumo',
          name: 'consumo-impresoras',
          component: ConsumoImpresorasView,
        },
        {
          path: 'suministros',
          name: 'suministros',
          component: SuministrosView,
        },
        {
          path: 'bitacora',
          name: 'bitacora',
          component: BitacoraView,
        },
        {
          path: 'usuarios',
          name: 'usuarios',
          component: UsuariosView,
        },
      ],
    },
  ],
})

export default router
