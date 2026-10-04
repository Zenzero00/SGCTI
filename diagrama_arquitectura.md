# Diagrama de Arquitectura de Datos - SGCTI

Sistema de Control de Suministros y Gestión de los Servicios TI.

**Fuente:** `src/Sgcti.Infrastructure/Persistence/SgctiDbContext.cs` (configuración en
`OnModelCreating`) y las entidades de `src/Sgcti.Core/Entities/`.

**Sintaxis:** `erDiagram` de Mermaid.js, validada con el parser de Mermaid v12
(6 entidades, 3 relaciones, 60 atributos).

> Los comentarios `%%` no están soportados dentro de `erDiagram` en Mermaid: rompen el
> parseo. Por eso las notas van en las etiquetas de atributos y en las secciones de este
> documento, no como comentarios del diagrama.

## Diagrama Entidad-Relación

```mermaid
erDiagram

    IMPRESORAS {
        int Id PK "PK autoincremental"
        varchar45 Ip UK "unico, requerido, index unico"
        varchar100 Modelo "requerido"
        varchar100 Fabricante "requerido"
        varchar60 Departamento "requerido, indexado, default Facturacion"
        datetime FechaInstalacion "requerido"
        varchar20 EstadoPing "enum EstadoPing, requerido, default Desconectado"
        varchar500 ComputadorasVinculadas "nullable, texto libre"
        int ContadorTotalPaginas "paginas acumuladas"
        int ContadorMonocromo "solo blanco y negro"
        int ContadorColor "solo color"
        int NivelTonnerNegro "default 0, indexado"
        int NivelTonnerCian "nullable"
        int NivelTonnerMagenta "nullable"
        int NivelTonnerAmarillo "nullable"
        numeric102 PromedioPaginasDiario "numeric 10,2"
        numeric102 PromedioTonnerDiario "numeric 10,2"
        datetime UltimoMantenimiento "nullable"
        int PaginasDesdeUltimoMantenimiento "acumulado desde el ultimo servicio"
        int DiasEstimadosAgotamientoTonner "indexado, calculo predictivo"
        datetime FechaEstimadaAgotamientoTonner "nullable, calculo predictivo"
        int DiasEstimadosMantenimiento "indexado, calculo predictivo"
        datetime FechaEstimadaMantenimiento "nullable, calculo predictivo"
        int ConsumibleTonerId FK "nullable, indexado, ON DELETE SET NULL"
        bool AlertaStockCritico "default false, indexado"
    }

    CONSUMIBLES {
        int Id PK "PK autoincremental"
        varchar50 Tipo "requerido"
        varchar100 ModeloCompatible "requerido"
        int StockActual "cantidad en almacen"
        int StockMinimo "default 1, umbral de reposicion"
        int CantidadActual "default 0, cantidad en uso"
        int DiasAntelacionPedido "default 0, lead time de compra"
        decimal182 CostoUnitario "decimal 18,2"
    }

    HISTORIALCONSUMO {
        int Id PK "PK autoincremental"
        int ImpresoraId FK "requerido, ON DELETE CASCADE"
        int ConsumibleId FK "requerido, ON DELETE RESTRICT"
        datetime FechaCambio "indexado"
        int PaginasImpresas "paginas del periodo"
        decimal182 Costo "decimal 18,2, costo del periodo"
    }

    TICKETS {
        int Id PK "PK autoincremental"
        varchar200 Titulo "requerido"
        varchar2000 Descripcion "requerido"
        varchar20 Estado "enum EstadoTicket, default Abierto, indexado"
        varchar20 Prioridad "enum PrioridadTicket, default Media"
        datetime FechaCreacion "requerido, indexado"
        datetime FechaCierre "nullable"
        int SLAHoras "compromiso de resolucion"
    }

    BITACORAACTIVIDADES {
        int Id PK "PK autoincremental"
        int AnalistaId "indexado, sin FK"
        varchar2000 DescripcionActividad "requerido"
        varchar150 EquipoIntervenido "nullable"
        datetime HoraInicio "requerido"
        datetime HoraFin "requerido"
        varchar30 Etiqueta "default Rutina, values Urgente, Pendiente de Compra"
        varchar15 Estado "enum EstadoActividad, default Pendiente, indexado"
    }

    METRICASRED {
        int Id PK "PK autoincremental"
        varchar45 DispositivoIP "requerido, indexado, sin FK a Impresoras"
        numeric102 LatenciaMS "numeric 10,2"
        int PaquetesPerdidos "paquetes no recibidos en el sondeo"
        datetime FechaHora "indexado, muestra temporal"
    }

    IMPRESORAS ||--o{ HISTORIALCONSUMO : "1 Impresora genera N registros, ON DELETE CASCADE"
    CONSUMIBLES ||--o{ HISTORIALCONSUMO : "1 Consumible aparece en N registros, ON DELETE RESTRICT"
    CONSUMIBLES o|--o{ IMPRESORAS : "1 Consumible de toner para 0..N Impresoras, ON DELETE SET NULL"
```

## Relationships

| # | Origen | Cardinalidad | Destino | FK | Comportamiento al borrar |
|---|--------|--------------|---------|----|--------------------------|
| 1 | `IMPRESORAS` | 1 → N (obligatoria) | `HISTORIALCONSUMO` | `ImpresoraId` | `Cascade` |
| 2 | `CONSUMIBLES` | 1 → N (obligatoria) | `HISTORIALCONSUMO` | `ConsumibleId` | `Restrict` |
| 3 | `CONSUMIBLES` | 0..1 → N (opcional) | `IMPRESORAS` | `ConsumibleTonerId` | `SetNull` |

`HISTORIALCONSUMO` es una entidad asociativa que resuelve la relación muchos-a-muchos
entre `Impresoras` y `Consumibles`, guardando además los datos del periodo
(`PaginasImpresas`, `Costo`, `FechaCambio`).

## Enumeraciones

| Tipo | Valores | Se persiste como |
|------|---------|------------------|
| `EstadoPing` | `EnLinea`, `Inestable`, `Desconectado` | `string` (max 20) |
| `EstadoTicket` | `Abierto`, `EnProgreso`, `Resuelto` | `string` (max 20) |
| `PrioridadTicket` | `Baja`, `Media`, `Alta`, `Critica` | `string` (max 20) |
| `EstadoActividad` | `Pendiente`, `Completado` | `string` (max 15) |

Todas se configuran con `HasConversion<string>()`, por lo que las consultas que filtren
o ordenen por estos campos deben comparar contra el texto, no contra el enum.

## Hallazgos

Solo 3 de las 6 entidades tienen llaves foráneas reales. El resto del modelo es
plano y no participa en relaciones.

- **Tablas huérfanas:** `Tickets`, `BitacoraActividades` y `MetricasRed` no tienen
  ninguna relación configurada en `OnModelCreating`.
- **`BitacoraActividad.AnalistaId`** está indexado (`SgctiDbContext.cs:101`) pero no
  existe una entidad `Usuario`/`Analista` ni una propiedad de navegación. Es un enlace
  lógico a un modelo de usuarios que aún no está implementado.
- **`MetricaRed.DispositivoIP`** se indexa del mismo modo, pero nunca referencia
  `Impresoras.Ip`, pese a que ambas columnas se describen como la misma dirección de
  red y tienen el mismo largo (`varchar(45)`).
- **Relación unidireccional:** `Impresora.ConsumibleToner` se declara con `WithMany()`
  sin colección inversa (`SgctiDbContext.cs:49`), así que el lado `Consumible` no puede
  navegar hacia sus impresoras. Para consultas inversas hay que ir por `ConsumibleTonerId`.
- **Columnas potencialmente duplicadas:** `Consumible` declara `StockActual` y
  `CantidadActual` con significados solapados; solo `StockMinimo` recibe valor por
  defecto, y ninguna de las dos se valida en el modelo.
- **Desviación respecto a la especificación:** `spec/spec.md:78` describe
  `Tickets (ID, UsuarioSolicitante, Departamento, Categoria, Descripcion, Prioridad,
  Estado, FechaApertura, FechaCierre)`. Las columnas `UsuarioSolicitante`, `Departamento`,
  `Categoria` y `FechaApertura` **no existen** en la entidad real, que usa
  `FechaCreacion` y `Titulo`. Este diagrama refleja el código, que es la fuente de verdad.

## Índices

| Tabla | Columna(s) | Único |
|-------|-----------|-------|
| `Impresoras` | `Ip` | Sí |
| `Impresoras` | `Departamento`, `NivelTonnerNegro`, `DiasEstimadosAgotamientoTonner`, `DiasEstimadosMantenimiento`, `ConsumibleTonerId`, `AlertaStockCritico` | No |
| `HistorialConsumo` | `FechaCambio` | No |
| `Tickets` | `FechaCreacion`, `Estado` | No |
| `BitacoraActividades` | `Estado`, `AnalistaId` | No |
| `MetricasRed` | `FechaHora`, `DispositivoIP` | No |
