# ESPECIFICACIÓN TÉCNICA MAESTRA (SPEC.MD)
## Sistema de Control de Suministros y Gestión de Servicios TI

---

### 1. Visión General y Alcance
* **Nombre del Sistema:** Sistema de Control de Suministros y Gestión de los Servicios TI (SCG-TI)[cite: 6].
* **Entorno Operativo:** Centro de Distribución (CENDI) de Star Gas, C.A. (Los Guayos, Edo. Carabobo)[cite: 6].
* **Marco Teórico:** ITIL 4 (Gestión de Incidentes y Activos) y Teoría General de Sistemas[cite: 6].
* **Propósito:** Automatizar el monitoreo del parque de 10 impresoras multifuncionales, el inventario de consumibles, la mesa de ayuda con tickets de usuario, la bitácora técnica y la disponibilidad de red en la WAN/VPN corporativa[cite: 6].

---

### 2. Reglas de Negocio Estrictas (Fuente de la Verdad)
1. **Terminología de Operaciones TI:** Queda estrictamente prohibido usar la palabra *"Tareas"* en la interfaz y en el código. El módulo debe denominarse **"Registros Operativos"**, **"Bitácora de Soporte"** o **"Actividades TI"**.
2. **Ambiente Confinado:** El software opera exclusivamente en la intranet corporativa (LAN/WAN/VPN) de Star Gas, C.A., sin exponer puertos hacia la red pública de Internet[cite: 6].
3. **Rol del Sistema:** El software no administra colas de impresión de Windows ni sustituye el spooler de red; actúa como un gestor de telemetría, auditoría, mesa de ayuda e inventario[cite: 6].

---

### 3. Stack Tecnológico Base
* **Backend y Lógica:** C# bajo el ecosistema .NET[cite: 6].
* **Base de Datos:** Base de datos relacional normalizada en PostgreSQL[cite: 6].
* **Protocolos de Red:** ICMP (Internet Control Message Protocol) para sondeos de eco (Ping)[cite: 6].
* **Motor de Documentos:** Generador nativo de reportes en PDF (ej. QuestPDF o iTextSharp).
* **Módulo Cognitivo (RAG):** Indexación de texto para búsqueda semántica sobre los manuales técnicos de las impresoras.

---

### 4. Arquitectura de Módulos Funcionales

#### Módulo 1: Control de Consumibles y Parque de Impresión
* **Gestión de Impresoras:** Registro de dirección IP, modelo, fabricante, departamento asignado (Facturación, PCP, Tracking, Ventas, RRHH) y computadoras vinculadas[cite: 6].
* **Control de Tóneres y Repuestos:**
  * Registro de existencias en almacén físico de TI (tóneres negros/color, tambores, rodillos, fusores)[cite: 6].
  * Registro de recargas, fechas de cambio y costo unitario de adquisición.
  * Estimación del rendimiento: cálculo de páginas impresas generadas por cartucho.
  * Alertas predictivas configurables: avisos visuales automáticos cuando el stock baje del umbral mínimo de seguridad[cite: 6].
* **Costos y Proyecciones:** Cálculo del costo total operativo mensual y anual por impresora y por departamento.

#### Módulo 2: Mesa de Ayuda (Help Desk y Tickets de Usuario)
* **Portal de Usuario:** Interfaz liviana para que los empleados reporten averías o solicitudes de consumibles directamente desde sus puestos de trabajo[cite: 6].
* **Bandeja de Entrada TI:** Centralización de solicitudes con categorización por criticidad (Baja, Media, Alta, Crítica) y área afectada[cite: 6].
* **Ciclo de Vida del Ticket:** Estados definidos (`Abierto`, `En Proceso`, `En Espera de Repuesto`, `Resuelto`, `Cerrado`).
* **Métricas de SLA:** Cronómetro de tiempo de respuesta inicial y tiempo de resolución por ticket[cite: 6].

#### Módulo 3: Bitácora de Actividades y Jornada TI (No usar la palabra "Tareas")
* **Jornada Laboral:** Marcaje de inicio y cierre de turno para el analista de soporte técnico.
* **Registro de Actividades:**
  * Formulario rápido para asentar intervenciones técnicas realizadas (mantenimiento preventivo, soporte a usuario, cableado, reemplazo de hardware).
  * Campos obligatorios: Hora de inicio, hora de fin, descripción técnica, equipo intervenido y estado (`Pendiente`, `Completado`).
* **Etiquetado y Recordatorios:** Asignación de etiquetas operativas dinámicas (`Urgente`, `Rutina`, `Pendiente de Compra`) con alertas en el panel para actividades pendientes.

#### Módulo 4: Monitoreo de Disponibilidad de Red (ICMP Ping)
* **Sondeo Proactivo:** Rutina en segundo plano que envía paquetes ICMP Ping periódicos a las IPs de las impresoras y nodos críticos en la WAN/VPN[cite: 6].
* **Métricas de Red:** Medición de latencia en milisegundos y porcentaje de pérdida de paquetes[cite: 6].
* **Panel de Estado:** Cuadrícula visual con indicadores de estado (`En Línea [Verde]`, `Inestable [Amarillo]`, `Desconectado [Rojo]`) con notificaciones de caída de enlace[cite: 6].

#### Módulo 5: Asistente IA y Base de Conocimiento (RAG Técnico)
* **Repositorio de Instructivos:** Almacenamiento local de manuales en PDF y guías de servicio por modelo de impresora.
* **Búsqueda Asistida:** Chatbot técnico interno que permite consultar fallas o códigos de error y devuelve pasos de solución extraídos de los manuales.
* **Descarga Automática:** Capacidad del sistema para asociar la documentación oficial en PDF al dar de alta un nuevo modelo de equipo en la red.

#### Módulo 6: Estadísticas, Gráficos y Reportería PDF
* **Panel Analítico:** Gráficos dinámicos con filtros cruzados por rango temporal (Día, Semana, Mes, Año), departamento e impresora individual.
* **Comparativas Mensuales:** Cálculo diferencial de consumos, gastos e incidencias entre meses consecutivos.
* **Exportación Formal en PDF:**
  * Reporte mensual de consumo y proyección de costos de impresión.
  * Informe de cumplimiento de tickets de soporte y rendimiento técnico[cite: 6].
  * Hoja de vida técnica e historial de intervenciones por equipo.

---

### 5. Esquema de Entidades Relacionales Clave
* `Impresoras` (ID, IP, Modelo, Departamento, FechaInstalacion, EstadoPing)
* `Consumibles` (ID, Tipo, ModeloCompatible, StockActual, StockMinimo, CostoUnitario)
* `HistorialConsumo` (ID, ImpresoraID, ConsumibleID, FechaCambio, PaginasImpresas, Costo)
* `Tickets` (ID, UsuarioSolicitante, Departamento, Categoria, Descripcion, Prioridad, Estado, FechaApertura, FechaCierre)
* `BitacoraActividades` (ID, AnalistaID, DescripcionActividad, HoraInicio, HoraFin, Etiqueta, Estado)
* `MetricasRed` (ID, DispositivoIP, LatenciaMS, PaquetesPerdidos, FechaHora)