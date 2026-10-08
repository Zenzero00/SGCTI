# Sistema de Control de Suministros y Gestión de los Servicios TI (SGCTI)

## Documento de Estado — Versión Final y Completada

**Proyecto:** Sistema de Control de Suministros y Gestión de los Servicios TI
**Institución:** CENDI
**Fecha de corte:** Octubre 2026
**Estado global:** ✅ Fase de desarrollo completada al 100% — Listo para despliegue

---

## 1. Resumen del Proyecto

El **SGCTI** es un *Modelo de Sinergia Digital* completamente funcional que centraliza la operación integral del departamento de Tecnología de la Información en un único entorno de trabajo unificado.

El sistema sustituye el manejo disperso de hojas de cálculo, registros manuales y herramientas aisladas por una plataforma web moderna donde conviven, de forma coherente y en tiempo real:

- La **mesa de ayuda** de incidencias del departamento.
- El **monitoreo de red** de las impresoras y equipos mediante SNMP.
- El **control de inventario** de suministros y consumibles.
- La **bitácora de actividades** y auditoría del trabajo técnico.
- Un **cerebro predictivo** que anticipa agotamientos de tóner y mantenimientos, notificando de forma proactiva a los usuarios.

La sinergia entre módulos es el eje del producto: el consumo detectado por SNMP alimenta el análisis predictivo, éste alimenta las alertas del Dashboard, y las alertas llegan al usuario mediante notificaciones push —todo dentro de la misma sesión, sin cambiar de herramienta—.

El resultado es una plataforma lista para operar como sistema oficial de gestión del departamento de TI.

---

## 2. Arquitectura y Tecnologías

El sistema se organiza en una arquitectura **cliente-servidor de tres capas** con comunicación en tiempo real.

### Backend

| Componente | Tecnología |
| --- | --- |
| API Web | **ASP.NET Core (.NET 10) Web API** con controladores REST y autenticación **JWT Bearer** |
| ORM / Persistencia | **Entity Framework Core** con proveedor **Npgsql** |
| Base de datos | **PostgreSQL** (base `SgctiDB`) |
| Tiempo real | **SignalR** (hub `/hubs/notificaciones`) |
| Procesos en segundo plano | **BackgroundServices** (`ServicioMonitoreoRed`, `ServicioAlertasAutomaticas`) |
| Reportes y extras | QuestPDF (reportes), Lextm.SharpSnmpLib (SNMP v2), Gemini API (asistente IA), Swagger (documentación) |

Estructura de proyectos:

- `src/Sgcti.Api` — API, controladores, hubs, servicios en segundo plano.
- `src/Sgcti.Core` — entidades, DTOs, contratos y servicios de negocio (incluido el análisis predictivo).
- `src/Sgcti.Infrastructure` — acceso a datos, `SgctiDbContext` y migraciones de EF Core.

### Frontend

| Componente | Tecnología |
| --- | --- |
| Framework | **Vue 3** (Composition API con `<script setup>`) + TypeScript |
| Build | **Vite** |
| HTTP | **Axios** (interceptores de sesión, token Bearer y manejo de 401) |
| Notificaciones | **vue3-toastify** (toasts globales con cierre automático) |
| Routing | vue-router (historial HTML5, rutas anidadas bajo `/panel`) |
| Estilos | CSS propio con metodología BEM en bloques `scoped` (sin frameworks CSS externos) |
| Gráficas | Chart.js + vue-chartjs (consumo de impresoras) |

### Comunicación en tiempo real

```
┌──────────────┐   REST (Axios + JWT)   ┌──────────────────────────┐
│              │ ─────────────────────▶ │                          │
│   Frontend   │                        │   API .NET (SGCTI)       │
│   Vue 3      │ ◀────────────────────  │   Controladores + EF Core│
│              │   SignalR (push)       │   BackgroundServices     │
└──────────────┘                        └────────────┬─────────────┘
                                                     │
                                            ┌────────▼────────┐
                                            │   PostgreSQL    │
                                            └─────────────────┘
```

---

## 3. Módulos Principales

El sistema se compone de seis módulos operativos, todos accesibles desde el panel lateral con control de sesión activo.

### 3.1 Dashboard (Centro de Mando)

Vista de entrada al panel que ofrece **KPIs en tiempo real** del departamento: tickets abiertos, suministros en nivel crítico, impresoras desconectadas y actividades del día. Bajo los indicadores, dos paneles complementarios muestran las **alertas predictivas** (tóner y mantenimientos próximos a vencer) y la **actividad reciente** registrada por el equipo.

### 3.2 Mesa de Ayuda (Tickets)

Gestión completa de incidencias y solicitudes con **prioridades** (Baja, Media, Alta, Crítica) y **estados** (Abierto, En Progreso, Resuelto), SLA por ticket y cierre con fecha automática mediante el botón "Resolver". El módulo incluye alta rápida de tickets y **notificaciones push** vía SignalR para mantener al equipo informado sin recargar la página.

### 3.3 Impresoras y SNMP

Monitoreo continuo en red de todas las impresoras del parque:

- **Conexión directa por OIDs** (Printer MIB) mediante SNMP v2 para obtener modelo, estado de conexión, latencia y **nivel de tóner** en porcentaje.
- Tabla de equipos con barra de nivel de tóner y luz de estado (En Línea / Inestable / Desconectado).
- Modal de detalle con información del equipo, **registro de mantenimientos aislado** (preventivo/correctivo, con técnico, fecha y descripción), carga de manuales en PDF y asistente IA que resuelve dudas sobre el manual del equipo.
- Generación de reporte de consumo en PDF y reporte gráfico de consumo por rango de fechas.

### 3.4 Inventario y Suministros

Control del stock de suministros (tóneres, tambores, papeles y repuestos) con **stock mínimo** y detección visual de niveles críticos. Permite altas, ediciones y bajas de productos, además del **registro de consumo histórico** a través de movimientos de entrada y salida que actualizan el inventario de forma automática y trazable (cantidad, fecha y observación).

### 3.5 Bitácora de Actividades

Auditoría del trabajo diario de los técnicos: cada actividad se registra con analista, descripción, equipo intervenido, horario, etiqueta (Rutina, Urgente, Pendiente de Compra) y estado. Incluye dos capacidades distintivas:

- **Recorrido Activo con cronómetro:** inicia una actividad raíz con temporizador en vivo (HH:MM:SS) que se detiene y consigna el tiempo total al finalizar.
- **Anidación de subtareas:** mientras el recorrido está activo, cada nueva actividad registrada se anota automáticamente como subtarea dentro de la descripción del recorrido, conservando la trazabilidad completa de la jornada.

### 3.6 Configuración de Sistema

Módulo administrativo con dos pestañas:

- **Usuarios:** gestión de cuentas con **roles protegidos (Admin / Técnico)**, activación y desactivación, edición de credenciales y eliminación con confirmación segura. El usuario administrador principal está protegido contra eliminación o desactivación.
- **Equipos (SNMP):** alta y configuración de los equipos a monitorear, definiendo su **IP** y **Comunidad SNMP** (credencial de acceso), que son los parámetros utilizados por el servicio de monitoreo.

---

## 4. Características Avanzadas (El "Cerebro" del Sistema)

Más allá del CRUD operativo, el SGCTI incorpora una capa de inteligencia que transforma los datos históricos en decisiones anticipadas.

### 4.1 Análisis Predictivo

El servicio `ServicioAnalisisPredictivo` implementa un **modelo matemático** que, a partir del historial de consumo registrado de cada impresora, calcula:

- El **promedio de páginas impresas por día** (ventana de 30 días o desde la fecha de instalación).
- Los **días estimados y la fecha exacta de agotamiento de tóner**, aplicando el rendimiento del consumible.
- Los **días estimados y la fecha del próximo mantenimiento**, en función de las páginas acumuladas desde la última intervención.
- El indicador de **alerta de stock crítico**, cruzando el consumo con el stock disponible y los días de antelación para pedido.

Las predicciones se recalculan automáticamente cada vez que una impresora es actualizada, de modo que el Dashboard y las alertas reflejan siempre el escenario más reciente.

### 4.2 Proactividad (BackgroundService y SignalR)

El sistema no espera a que el usuario consulte: **actúa proactivamente**.

- **`ServicioAlertasAutomaticas` (BackgroundService):** servicio en segundo plano que evalúa las predicciones de **todas las impresoras cada 24 horas** y determina cuáles superan el umbral de alerta (tóner o mantenimiento a punto de agotarse/vencer).
- **SignalR en tiempo real:** cuando se detecta una condición crítica, el servidor envía la alerta de inmediato a todos los usuarios conectados a través del hub `/hubs/notificaciones`.
- **Campanita interactiva:** en la barra superior del panel, la campana de notificaciones recibe los avisos en vivo, muestra un contador de pendientes y despliega el historial con título y mensaje, permitiendo marcarlas como leídas —sin recargar la página—.

Complementariamente, el `ServicioMonitoreoRed` (BackgroundService) consulta periódicamente cada impresora por SNMP y persiste sus métricas, garantizando que los datos que alimentan al cerebro predictivo estén siempre actualizados.

---

## 5. Estado Actual

**El proyecto se encuentra al 100% de su fase de desarrollo.**

| Aspecto | Estado |
| --- | --- |
| Backend (.NET API + EF Core + PostgreSQL) | ✅ Completo y compilando sin errores |
| Módulos funcionales (Dashboard, Tickets, Impresoras, Inventario, Bitácora, Configuración) | ✅ Operativos |
| Análisis predictivo y alertas automáticas (24 h) | ✅ Implementado |
| Notificaciones en tiempo real (SignalR + campanita) | ✅ Implementado |
| Autenticación JWT y roles Admin/Técnico | ✅ Implementado |
| UX/UI | ✅ Pulida |
| Build de producción (type-check + Vite) | ✅ Pasando |
| Preparado para despliegue y presentación | ✅ Sí |

### Pulido de UX/UI

- **Notificaciones Toast** (vue3-toastify) en toda la interfaz: sustituyen los diálogos nativos del navegador con avisos de éxito y error no intrusivos con cierre automático.
- **Modales de confirmación propios** para las acciones destructivas (eliminar usuarios, equipos o suministros), diseñados con la misma identidad visual que el resto del sistema, con soporte de tecla Escape y cierre por clic exterior.
- **Tablas responsivas** con desplazamiento horizontal (`overflow-x: auto`) en Dashboard, Tickets y Bitácora, para que el diseño se mantenga íntegro en pantallas pequeñas.
- **Estilos coherentes** en toda la aplicación: sistema de diseño propio con clases BEM, tipografía, paleta institucional y componentes reutilizables (botones, tarjetas, etiquetas, badges y modales).

### Conclusión

El SGCTI entrega su versión final como un sistema **completo, estable y listo para su despliegue y presentación**, con toda la funcionalidad planificada desarrollada, probada y con la experiencia de usuario finalizada.

---

*Documento generado como parte de la entrega final del Sistema de Control de Suministros y Gestión de los Servicios TI.*
