import axios from 'axios'
import { API_BASE_URL } from '../config'

const TOKEN_KEY = 'sgcti_token'

export interface Departamento {
  id: number
  nombre: string
  sigla: string
  prioridadPorDefecto: string
}

export interface CategoriaTicket {
  id: number
  nombre: string
}

function cabeceras() {
  return {
    Authorization: 'Bearer ' + localStorage.getItem(TOKEN_KEY),
  }
}

export async function obtenerDepartamentos(): Promise<Departamento[]> {
  const { data } = await axios.get<Departamento[]>(`${API_BASE_URL}/api/departamentos`, {
    headers: cabeceras(),
  })

  return data
}

export async function obtenerCategorias(): Promise<CategoriaTicket[]> {
  const { data } = await axios.get<CategoriaTicket[]>(`${API_BASE_URL}/api/categorias`, {
    headers: cabeceras(),
  })

  return data
}

export function prioridadNumerica(prioridad: string): number {
  switch (prioridad.toLowerCase()) {
    case 'baja':
      return 0
    case 'alta':
      return 2
    case 'critica':
    case 'crítica':
      return 3
    default:
      return 1
  }
}
