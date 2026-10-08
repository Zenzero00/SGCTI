import { reactive } from 'vue'

export interface OpcionesConfirmacion {
  mensaje: string
  confirmarTexto?: string
  cancelarTexto?: string
}

export const estadoConfirmacion = reactive({
  visible: false,
  mensaje: '',
  confirmarTexto: 'Confirmar',
  cancelarTexto: 'Cancelar',
})

let resolucion: ((confirmado: boolean) => void) | null = null

export function confirmar(opciones: string | OpcionesConfirmacion): Promise<boolean> {
  const opcionesNormalizadas = typeof opciones === 'string' ? { mensaje: opciones } : opciones

  if (estadoConfirmacion.visible) {
    cerrarConfirmacion(false)
  }

  estadoConfirmacion.mensaje = opcionesNormalizadas.mensaje
  estadoConfirmacion.confirmarTexto = opcionesNormalizadas.confirmarTexto ?? 'Confirmar'
  estadoConfirmacion.cancelarTexto = opcionesNormalizadas.cancelarTexto ?? 'Cancelar'
  estadoConfirmacion.visible = true

  return new Promise<boolean>((resolve) => {
    resolucion = resolve
  })
}

export function cerrarConfirmacion(confirmado: boolean) {
  if (!estadoConfirmacion.visible) {
    return
  }

  estadoConfirmacion.visible = false

  const resolver = resolucion
  resolucion = null
  resolver?.(confirmado)
}
