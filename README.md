# 🖥️ SGCTI — Sistema de Control de Suministros y Gestión de los Servicios TI

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Vue 3](https://img.shields.io/badge/Vue.js-3.x-4FC08D?style=for-the-badge&logo=vuedotjs&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-007ACC?style=for-the-badge&logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Plataforma web integral desarrollada bajo una arquitectura cliente-servidor en tres capas para automatizar el monitoreo en red del parque de impresoras (mediante los protocolos **SNMPv2c** e **ICMP**), la proyección predictiva del agotamiento de consumibles, el control de inventario de insumos tecnológicos y la gestión centralizada de soporte técnico (Mesa de Ayuda omnicanal con **Bot de Telegram** y **Asistente de Inteligencia Artificial**) en el **Centro de Distribución (CENDI)** de la empresa **Star Gas, C.A. (Grupo Empresarial RG)**.

---

## 📋 Características y Módulos Principales

### 1. 📊 Dashboard Ejecutivo y Alertas en Tiempo Real
* Visualización centralizada de indicadores clave (KPIs): tickets abiertos, suministros en estado crítico, impresoras desconectadas y actividades técnicas del día.
* Gráficas dinámicas de consumo semanal de páginas (`Chart.js`) y estado proporcional del inventario.
* Panel de **Alertas Predictivas** que notifica proactivamente qué equipos agotarán su tóner o requerirán mantenimiento preventivo en un umbral menor o igual a $14\text{ días}$.
* Notificaciones *push* instantáneas en la barra superior mediante **WebSockets (SignalR)** con alertas visuales y sonoras sin necesidad de recargar la página.

### 2. 🖨️ Monitoreo de Impresoras en Red (SNMPv2c) y Motor Predictivo
* **Escaneo Asíncrono en Paralelo (`ServicioMonitoreoRed`):** Proceso en segundo plano (`BackgroundService`) que consulta periódicamente los agentes SNMP de las impresoras de la red corporativa sin bloquear el hilo principal.
* **Compatibilidad Dual (Equipos Físicos Reales + Simulador CENDI):**
  * Consulta directa por el puerto estándar `UDP 161` para impresoras físicas reales en la red LAN/WAN (ej. `192.168.x.x`).
  * Integración con el emulador de red del CENDI (`emulador_snmp_cendi.py`) para simular en tiempo real las 13 impresoras multifuncionales (Canon imageRUNNER, Canon i-SENSYS y Ricoh Aficio) en los puertos `UDP 16101–16113`.
* **Identificación Inteligente:** Separa el **Nombre del Equipo (visible en el sistema)** del **Modelo Técnico**, evitando que la respuesta del OID `sysDescr` (`1.3.6.1.2.1.1.1.0`) sobrescriba el alias asignado por el administrador, mientras conserva el modelo técnico para la búsqueda automatizada de manuales.
* **Algoritmo de Análisis Predictivo:** Calcula el promedio diario de impresión ($P_{\text{diario}}$) a partir del historial de contadores y estima:
  * **Días restantes de tóner:**
    $$D_{\text{tóner}} = \text{round}\left(\frac{(N_{\text{tóner}} / 100) \times R_{\text{cartucho}}}{P_{\text{diario}}}\right)$$
  * **Días para próximo mantenimiento preventivo:**
    $$D_{\text{mant}} = \text{round}\left(\frac{\max(0, U_{\text{mant}} - C_{\text{desde\_mant}})}{P_{\text{diario}}}\right)$$
* **Gestión de Mantenimientos y Manuales:** Registro histórico de mantenimientos preventivos/correctivos con reinicio inmediato de contadores de desgaste, carga de manuales en PDF y buscador web automático de documentación técnica.

### 3. 🎫 Mesa de Ayuda (Help Desk) y Omnicanalidad vía Bot de Telegram
* Recepción automática de incidencias reportadas por los empleados desde sus dispositivos móviles a través del **Bot de Telegram (`ServicioTelegramBot`)**.
* Vinculación automática del número telefónico del empleado con su departamento y asignación inteligente de códigos correlativos (ej. `FAC-001`, `PCP-002`) y niveles de prioridad (`Baja`, `Media`, `Alta`, `Crítica`).
* Gestión del ciclo de vida del ticket (`Abierto`, `En Progreso`, `Resuelto`), historial de soluciones cerradas y estadísticas de incidencias por departamento y categoría.

### 4. 📦 Gestión de Inventario, Suministros y Consumo Histórico
* Administración de existencias de consumibles (tóneres, tambores, kits de mantenimiento, repuestos) con alertas visuales automáticas cuando el stock actual iguala o desciende del stock mínimo ($S_{\text{actual}} \le S_{\text{mín}}$).
* **Importador Masivo de Excel (`ClosedXML`):** Procesamiento automatizado de libros históricos de consumo diario de páginas y contadores por impresora.
* **Exportación de Reportes (`QuestPDF`):** Generación de reportes ejecutivos y comparativas de consumo por rango de fechas en formato descargable.

### 5. ⏱️ Bitácora de Actividades Técnicas
* Herramienta de auditoría interna para registrar las labores diarias e intervenciones del personal de soporte.
* Función interactiva **"Iniciar Recorrido"** con cronómetro en tiempo real y registro de subtareas para medir con exactitud la duración de las inspecciones en el CENDI.

### 6. 🤖 Asistente Inteligente TI (IA Generativa)
* Chatbot técnico conectado a la API de **Google Gemini**, capaz de responder consultas en lenguaje natural sobre el estado en vivo del inventario, niveles actuales de tóner de las impresoras y procedimientos de soporte.
* Extracción y lectura contextual de los **manuales técnicos en PDF** vinculados a cada impresora para diagnosticar códigos de error y guiar paso a paso al técnico.

### 7. 🔐 Seguridad, Roles y Catálogos
* Autenticación basada en **JSON Web Tokens (JWT)** y encriptación de contraseñas mediante **BCrypt**.
* Control de acceso basado en roles jerárquicos: **Administrador**, **Ingeniero** y **Técnico**.
* Administración centralizada del directorio de empleados de Telegram, equipos SNMP, departamentos (con prioridad automática por defecto) y categorías de fallas.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnologías e Implementación |
| :--- | :--- |
| **Backend (API & Servicios)** | C#, ASP.NET Core (`.NET 10`), Entity Framework Core, SignalR (WebSockets), `Lextm.SharpSnmpLib` (SNMPv2c), `ClosedXML`, `QuestPDF`, Google Gemini API, JWT Bearer Auth. |
| **Frontend (Cliente SPA)** | Vue 3 (Composition API `<script setup>`), TypeScript, Vite, Vue Router, Axios, Chart.js (`vue-chartjs`), Phosphor Icons, `vue3-toastify`, CSS3 bajo metodología **BEM**. |
| **Base de Datos** | PostgreSQL 16 (Modelo relacional en 3FN bajo estándar transaccional ACID). |
| **Infraestructura y Red** | Docker, Docker Compose, Nginx, Python 3.12 (Emulador multi-agente SNMPv2c UDP). |

---

## 📂 Estructura del Proyecto

```text
SGCTI/
├── db-backup/
│   └── 01_datos_cendi.sql          # Respaldo inicial de la BD con histórico del CENDI
├── frontend-cendi/                 # Capa de Presentación (Vue 3 + TypeScript + Vite)
│   ├── src/
│   │   ├── assets/                 # Estilos globales y recursos gráficos
│   │   ├── components/             # Componentes reutilizables y modales
│   │   ├── composables/            # Lógica reactiva compartida (confirmaciones, etc.)
│   │   ├── router/                 # Definición de rutas protegidas por JWT
│   │   ├── services/               # Clientes HTTP y servicios de catálogos
│   │   ├── views/                  # Vistas principales (Dashboard, Impresoras, MesaAyuda, etc.)
│   │   ├── App.vue
│   │   ├── config.ts
│   │   └── main.ts
│   ├── Dockerfile
│   └── package.json
├── src/                            # Solución Backend (.NET 10)
│   ├── Sgcti.Api/                  # Capa de Presentación API y BackgroundServices
│   │   ├── Controllers/            # Endpoints RESTful (Impresoras, Tickets, Dashboard, etc.)
│   │   ├── Hubs/                   # Concentradores SignalR (Notificaciones en tiempo real)
│   │   ├── Services/               # ServicioMonitoreoRed (SNMP), ServicioTelegramBot, IA
│   │   ├── Uploads/Manuales/       # Almacenamiento local de manuales técnicos en PDF
│   │   └── Program.cs
│   ├── Sgcti.Core/                 # Capa de Dominio (Entidades, DTOs, Contratos y Servicios)
│   └── Sgcti.Infrastructure/       # Capa de Persistencia (SgctiDbContext, Migraciones EF Core)
├── docker-compose.yml              # Orquestación de contenedores (PostgreSQL + API + Frontend)
├── emulador_snmp_cendi.py          # Simulador interactivo SNMPv2c de las 13 impresoras del CENDI
└── README.md
```

---

## 🚀 Requisitos Previos

1. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** instalado y en ejecución.
2. **[Python 3.12+](https://www.python.org/downloads/)** (64-bit) instalado en el sistema anfitrión para ejecutar el simulador interactivo de impresoras SNMP *(no requiere instalar paquetes externos con `pip`)*.

---

## ⚙️ Instalación y Puesta en Marcha

### Paso 1: Levantar los contenedores con Docker Compose
Abre una terminal en la carpeta raíz del proyecto y ejecuta:

```powershell
docker compose up --build -d
```

Esto construirá e iniciará automáticamente los 3 servicios:
* **Base de Datos (`sgcti-db`):** PostgreSQL 16 en el puerto `5432` (carga automáticamente los datos iniciales desde `db-backup/01_datos_cendi.sql` cuando el volumen se crea por primera vez).
* **Backend API (`sgcti-api`):** Servidor ASP.NET Core 10 disponible en `http://localhost:5141`.
* **Frontend Web (`sgcti-frontend`):** Interfaz web disponible en `http://localhost:8080` (o `http://localhost:5173` en modo desarrollo).

### Paso 2: Iniciar el Emulador SNMP de las 13 Impresoras del CENDI
Para simular la respuesta en red de las 13 impresoras multifuncionales del CENDI (o probar el encendido/apagado individual de equipos en vivo), abre una terminal en la raíz del proyecto y ejecuta:

```powershell
python emulador_snmp_cendi.py
```

Desde la consola interactiva del emulador puedes:
* Presionar **`1` al `13`** y `Enter` para conectar o desconectar en vivo cualquiera de las 13 impresoras del CENDI.
* Presionar **`t`** y `Enter` para encender o apagar todas las impresoras simultáneamente.
* En menos de **3 segundos**, el módulo de **Impresoras en Red** y el **Dashboard** reflejarán el cambio de estado, la latencia en milisegundos, el porcentaje de tóner negro y las proyecciones de días restantes.

> **Nota sobre Impresoras Físicas Reales:** Si agregas una impresora física real desde **Usuarios / Configuración $\rightarrow$ Equipos (SNMP) $\rightarrow$ Nuevo Equipo** indicando su dirección IP real en la red local (ej. `192.168.100.57`), el sistema la consultará directamente por el puerto estándar `UDP 161` sin pasar por el simulador.

---

## 💾 Comandos Útiles de Base de Datos (PostgreSQL)

* **Generar un respaldo actualizado de la base de datos hacia `db-backup/01_datos_cendi.sql` (PowerShell):**
  ```powershell
  New-Item -ItemType Directory -Force -Path "db-backup" | Out-Null; $env:PGPASSWORD="postgres"; $pgDump = Get-ChildItem "C:\Program Files\PostgreSQL\*\bin\pg_dump.exe" | Select-Object -Last 1 -ExpandProperty FullName; & $pgDump -h localhost -p 5432 -U postgres -d SgctiDB --clean --if-exists --no-owner --no-privileges -f "db-backup/01_datos_cendi.sql"
  ```

* **Reiniciar los contenedores desde cero recargando el respaldo SQL:**
  ```powershell
  docker compose down -v
  docker compose up --build -d
  ```

---

## 👨‍💻 Autoría y Créditos Académicos

Proyecto del Informe de Pasantías presentado como requisito para optar al título de **Ingeniero de Computación**:

* **Autor:** Ing. Francisco Geraldo Unda Solórzano
* **Tutora Académica:** Ing. Rosa Virginia Ortega
* **Institución:** Universidad José Antonio Páez (UJAP) — Facultad de Ingeniería, Escuela de Ingeniería de Computación.
* **Línea de Investigación:** Tecnologías Digitales e Inteligencia Computacional.
* **Organización:** Star Gas, C.A. — Centro de Distribución (CENDI), Los Guayos, Estado Carabobo, Venezuela (2026).