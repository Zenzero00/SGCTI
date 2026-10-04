import { createRouter, createWebHistory } from 'vue-router'

import LoginView from '@/views/LoginView.vue'
import PanelView from '@/views/PanelView.vue'
import TicketsView from '@/views/TicketsView.vue'
import ImpresorasView from '@/views/ImpresorasView.vue'

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
          name: 'tickets',
          component: TicketsView,
        },
        {
          path: 'impresoras',
          name: 'impresoras',
          component: ImpresorasView,
        },
      ],
    },
  ],
})

export default router
