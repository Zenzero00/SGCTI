--
-- PostgreSQL database dump
--

-- Dumped from database version 16.2
-- Dumped by pg_dump version 16.2

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."RegistrosConsumoDiario" DROP CONSTRAINT IF EXISTS "FK_RegistrosConsumoDiario_Impresoras_ImpresoraId";
ALTER TABLE IF EXISTS ONLY public."MantenimientosImpresora" DROP CONSTRAINT IF EXISTS "FK_MantenimientosImpresora_Impresoras_ImpresoraId";
ALTER TABLE IF EXISTS ONLY public."Impresoras" DROP CONSTRAINT IF EXISTS "FK_Impresoras_Consumibles_ConsumibleTonerId";
ALTER TABLE IF EXISTS ONLY public."HistorialConsumo" DROP CONSTRAINT IF EXISTS "FK_HistorialConsumo_Impresoras_ImpresoraId";
ALTER TABLE IF EXISTS ONLY public."HistorialConsumo" DROP CONSTRAINT IF EXISTS "FK_HistorialConsumo_Consumibles_ConsumibleId";
DROP INDEX IF EXISTS public."IX_Usuarios_Username";
DROP INDEX IF EXISTS public."IX_Usuarios_TelegramUsuarioOId";
DROP INDEX IF EXISTS public."IX_Usuarios_Departamento";
DROP INDEX IF EXISTS public."IX_Tickets_TelegramChatId";
DROP INDEX IF EXISTS public."IX_Tickets_Solicitante";
DROP INDEX IF EXISTS public."IX_Tickets_FechaCreacion";
DROP INDEX IF EXISTS public."IX_Tickets_Estado";
DROP INDEX IF EXISTS public."IX_Tickets_Departamento";
DROP INDEX IF EXISTS public."IX_Tickets_CodigoNomenclatura";
DROP INDEX IF EXISTS public."IX_Tickets_Categoria";
DROP INDEX IF EXISTS public."IX_Suministros_Tipo";
DROP INDEX IF EXISTS public."IX_Suministros_Nombre";
DROP INDEX IF EXISTS public."IX_RegistrosConsumoDiario_ImpresoraId";
DROP INDEX IF EXISTS public."IX_RegistrosConsumoDiario_Fecha";
DROP INDEX IF EXISTS public."IX_MetricasRed_FechaHora";
DROP INDEX IF EXISTS public."IX_MetricasRed_DispositivoIP";
DROP INDEX IF EXISTS public."IX_MantenimientosImpresora_ImpresoraId";
DROP INDEX IF EXISTS public."IX_MantenimientosImpresora_Fecha";
DROP INDEX IF EXISTS public."IX_Impresoras_NivelTonnerNegro";
DROP INDEX IF EXISTS public."IX_Impresoras_Ip";
DROP INDEX IF EXISTS public."IX_Impresoras_DiasEstimadosMantenimiento";
DROP INDEX IF EXISTS public."IX_Impresoras_DiasEstimadosAgotamientoTonner";
DROP INDEX IF EXISTS public."IX_Impresoras_Departamento";
DROP INDEX IF EXISTS public."IX_Impresoras_ConsumibleTonerId";
DROP INDEX IF EXISTS public."IX_Impresoras_AlertaStockCritico";
DROP INDEX IF EXISTS public."IX_HistorialConsumo_ImpresoraId";
DROP INDEX IF EXISTS public."IX_HistorialConsumo_FechaCambio";
DROP INDEX IF EXISTS public."IX_HistorialConsumo_ConsumibleId";
DROP INDEX IF EXISTS public."IX_Departamentos_Sigla";
DROP INDEX IF EXISTS public."IX_Departamentos_Nombre";
DROP INDEX IF EXISTS public."IX_CategoriasTicket_Nombre";
DROP INDEX IF EXISTS public."IX_BitacoraActividades_Estado";
DROP INDEX IF EXISTS public."IX_BitacoraActividades_AnalistaId";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."Usuarios" DROP CONSTRAINT IF EXISTS "PK_Usuarios";
ALTER TABLE IF EXISTS ONLY public."Tickets" DROP CONSTRAINT IF EXISTS "PK_Tickets";
ALTER TABLE IF EXISTS ONLY public."Suministros" DROP CONSTRAINT IF EXISTS "PK_Suministros";
ALTER TABLE IF EXISTS ONLY public."RegistrosConsumoDiario" DROP CONSTRAINT IF EXISTS "PK_RegistrosConsumoDiario";
ALTER TABLE IF EXISTS ONLY public."MetricasRed" DROP CONSTRAINT IF EXISTS "PK_MetricasRed";
ALTER TABLE IF EXISTS ONLY public."MantenimientosImpresora" DROP CONSTRAINT IF EXISTS "PK_MantenimientosImpresora";
ALTER TABLE IF EXISTS ONLY public."Impresoras" DROP CONSTRAINT IF EXISTS "PK_Impresoras";
ALTER TABLE IF EXISTS ONLY public."HistorialConsumo" DROP CONSTRAINT IF EXISTS "PK_HistorialConsumo";
ALTER TABLE IF EXISTS ONLY public."Departamentos" DROP CONSTRAINT IF EXISTS "PK_Departamentos";
ALTER TABLE IF EXISTS ONLY public."Consumibles" DROP CONSTRAINT IF EXISTS "PK_Consumibles";
ALTER TABLE IF EXISTS ONLY public."CategoriasTicket" DROP CONSTRAINT IF EXISTS "PK_CategoriasTicket";
ALTER TABLE IF EXISTS ONLY public."BitacoraActividades" DROP CONSTRAINT IF EXISTS "PK_BitacoraActividades";
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."Usuarios";
DROP TABLE IF EXISTS public."Tickets";
DROP TABLE IF EXISTS public."Suministros";
DROP TABLE IF EXISTS public."RegistrosConsumoDiario";
DROP TABLE IF EXISTS public."MetricasRed";
DROP TABLE IF EXISTS public."MantenimientosImpresora";
DROP TABLE IF EXISTS public."Impresoras";
DROP TABLE IF EXISTS public."HistorialConsumo";
DROP TABLE IF EXISTS public."Departamentos";
DROP TABLE IF EXISTS public."Consumibles";
DROP TABLE IF EXISTS public."CategoriasTicket";
DROP TABLE IF EXISTS public."BitacoraActividades";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: BitacoraActividades; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."BitacoraActividades" (
    "Id" integer NOT NULL,
    "AnalistaId" character varying(50) NOT NULL,
    "DescripcionActividad" character varying(2000) NOT NULL,
    "EquipoIntervenido" character varying(150),
    "HoraInicio" timestamp with time zone NOT NULL,
    "HoraFin" timestamp with time zone,
    "Etiqueta" character varying(30) NOT NULL,
    "Estado" character varying(15) NOT NULL
);


--
-- Name: BitacoraActividades_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."BitacoraActividades" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."BitacoraActividades_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: CategoriasTicket; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."CategoriasTicket" (
    "Id" integer NOT NULL,
    "Nombre" character varying(100) NOT NULL
);


--
-- Name: CategoriasTicket_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."CategoriasTicket" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."CategoriasTicket_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Consumibles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Consumibles" (
    "Id" integer NOT NULL,
    "Tipo" character varying(50) NOT NULL,
    "ModeloCompatible" character varying(100) NOT NULL,
    "StockActual" integer NOT NULL,
    "StockMinimo" integer DEFAULT 1 NOT NULL,
    "CostoUnitario" numeric(18,2) NOT NULL,
    "CantidadActual" integer DEFAULT 0 NOT NULL,
    "DiasAntelacionPedido" integer DEFAULT 0 NOT NULL
);


--
-- Name: Consumibles_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Consumibles" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Consumibles_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Departamentos; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Departamentos" (
    "Id" integer NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Sigla" character varying(10) NOT NULL,
    "PrioridadPorDefecto" character varying(20) NOT NULL
);


--
-- Name: Departamentos_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Departamentos" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Departamentos_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: HistorialConsumo; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."HistorialConsumo" (
    "Id" integer NOT NULL,
    "ImpresoraId" integer NOT NULL,
    "ConsumibleId" integer NOT NULL,
    "FechaCambio" timestamp with time zone NOT NULL,
    "PaginasImpresas" integer NOT NULL,
    "Costo" numeric(18,2) NOT NULL
);


--
-- Name: HistorialConsumo_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."HistorialConsumo" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."HistorialConsumo_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Impresoras; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Impresoras" (
    "Id" integer NOT NULL,
    "Ip" character varying(45) NOT NULL,
    "Modelo" character varying(100) NOT NULL,
    "Fabricante" character varying(100) NOT NULL,
    "Departamento" character varying(60) NOT NULL,
    "FechaInstalacion" timestamp with time zone NOT NULL,
    "EstadoPing" character varying(20) NOT NULL,
    "ComputadorasVinculadas" character varying(500),
    "ContadorColor" integer DEFAULT 0 NOT NULL,
    "ContadorMonocromo" integer DEFAULT 0 NOT NULL,
    "ContadorTotalPaginas" integer DEFAULT 0 NOT NULL,
    "DiasEstimadosAgotamientoTonner" integer DEFAULT 0 NOT NULL,
    "DiasEstimadosMantenimiento" integer DEFAULT 0 NOT NULL,
    "FechaEstimadaAgotamientoTonner" timestamp with time zone,
    "FechaEstimadaMantenimiento" timestamp with time zone,
    "NivelTonnerAmarillo" integer,
    "NivelTonnerCian" integer,
    "NivelTonnerMagenta" integer,
    "NivelTonnerNegro" integer DEFAULT 0 NOT NULL,
    "PaginasDesdeUltimoMantenimiento" integer DEFAULT 0 NOT NULL,
    "PromedioPaginasDiario" double precision DEFAULT 0.0 NOT NULL,
    "PromedioTonnerDiario" double precision DEFAULT 0.0 NOT NULL,
    "UltimoMantenimiento" timestamp with time zone,
    "AlertaStockCritico" boolean DEFAULT false NOT NULL,
    "ConsumibleTonerId" integer,
    "ComunidadSnmp" text DEFAULT ''::text NOT NULL,
    "LatenciaMs" integer DEFAULT 0 NOT NULL,
    "RutaManual" character varying(500)
);


--
-- Name: Impresoras_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Impresoras" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Impresoras_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: MantenimientosImpresora; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MantenimientosImpresora" (
    "Id" integer NOT NULL,
    "ImpresoraId" integer NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Descripcion" character varying(2000) NOT NULL,
    "RealizadoPor" character varying(100) NOT NULL
);


--
-- Name: MantenimientosImpresora_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."MantenimientosImpresora" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MantenimientosImpresora_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: MetricasRed; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."MetricasRed" (
    "Id" integer NOT NULL,
    "DispositivoIP" character varying(45) NOT NULL,
    "LatenciaMS" double precision NOT NULL,
    "PaquetesPerdidos" integer NOT NULL,
    "FechaHora" timestamp with time zone NOT NULL
);


--
-- Name: MetricasRed_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."MetricasRed" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."MetricasRed_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: RegistrosConsumoDiario; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."RegistrosConsumoDiario" (
    "Id" integer NOT NULL,
    "ImpresoraId" integer NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Contador" integer NOT NULL,
    "Consumo" integer NOT NULL,
    "Observacion" character varying(500)
);


--
-- Name: RegistrosConsumoDiario_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."RegistrosConsumoDiario" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RegistrosConsumoDiario_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Suministros; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Suministros" (
    "Id" integer NOT NULL,
    "Nombre" character varying(150) NOT NULL,
    "Tipo" character varying(50) NOT NULL,
    "CantidadActual" integer DEFAULT 0 NOT NULL,
    "StockMinimo" integer DEFAULT 0 NOT NULL,
    "Observacion" character varying(500)
);


--
-- Name: Suministros_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Suministros" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Suministros_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Tickets; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Tickets" (
    "Id" integer NOT NULL,
    "Descripcion" character varying(2000) NOT NULL,
    "Prioridad" character varying(20) NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "FechaCreacion" timestamp with time zone NOT NULL,
    "FechaCierre" timestamp with time zone,
    "SLAHoras" integer DEFAULT 0 NOT NULL,
    "Titulo" character varying(200) DEFAULT ''::character varying NOT NULL,
    "Departamento" character varying(100),
    "Categoria" character varying(100),
    "EquipoAfectado" character varying(150),
    "SolucionAplicada" character varying(2000),
    "SuministrosUtilizados" character varying(1000),
    "FechaResolucion" timestamp with time zone,
    "Solicitante" character varying(150),
    "TelegramChatId" character varying(100),
    "CodigoNomenclatura" character varying(30)
);


--
-- Name: Tickets_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Tickets" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Tickets_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Usuarios; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Usuarios" (
    "Id" integer NOT NULL,
    "NombreCompleto" character varying(150) NOT NULL,
    "Username" character varying(50) NOT NULL,
    "PasswordHash" character varying(200) NOT NULL,
    "Rol" character varying(20) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "Cargo" character varying(100),
    "Departamento" character varying(100),
    "PrioridadPersonalizada" character varying(20),
    "TelegramUsuarioOId" character varying(100)
);


--
-- Name: Usuarios_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public."Usuarios" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Usuarios_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Data for Name: BitacoraActividades; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."BitacoraActividades" ("Id", "AnalistaId", "DescripcionActividad", "EquipoIntervenido", "HoraInicio", "HoraFin", "Etiqueta", "Estado") FROM stdin;
1	Paseo diario	Actividad de rutina	\N	2026-10-07 04:18:50.246961-04	2026-10-07 04:19:02.642-04	Rutina	Completado
2	Segundo paseo diario	segundo paseo	\N	2026-10-07 04:19:37.556631-04	2026-10-07 04:32:19.058-04	Rutina	Completado
4	Francisco Unda	Limpieza de equipos	Computadora de Asistente RRHH	2026-10-07 04:34:17.308527-04	2026-10-07 04:35:27.978-04	Urgente	Completado
3	Técnico	Recorrido de instalaciones\n- Subtarea: Limpieza de equipos\n[Finalizado. Tiempo: 00:03:09]	\N	2026-10-07 04:32:21.604982-04	2026-10-07 04:35:31.396-04	Rutina	Completado
5	Técnico	Recorrido de instalaciones\n[Finalizado. Tiempo: 00:00:05]	\N	2026-10-08 04:36:41.259572-04	2026-10-08 04:36:46.433-04	Rutina	Completado
6	Técnico	Recorrido de instalaciones\n[Finalizado. Tiempo: 00:00:07]	\N	2026-10-28 17:28:27.379061-04	2026-10-28 17:28:35.119-04	Rutina	Completado
7	Técnico	Recorrido de instalaciones\n[Finalizado. Tiempo: 00:00:10]	\N	2026-10-31 11:43:20.549966-04	2026-10-31 11:43:31.012-04	Rutina	Completado
\.


--
-- Data for Name: CategoriasTicket; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."CategoriasTicket" ("Id", "Nombre") FROM stdin;
1	Hardware
2	Software
3	Impresoras
4	Periféricos
5	Red
\.


--
-- Data for Name: Consumibles; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Consumibles" ("Id", "Tipo", "ModeloCompatible", "StockActual", "StockMinimo", "CostoUnitario", "CantidadActual", "DiasAntelacionPedido") FROM stdin;
1	Tóner		0	2	0.00	1	15
\.


--
-- Data for Name: Departamentos; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Departamentos" ("Id", "Nombre", "Sigla", "PrioridadPorDefecto") FROM stdin;
1	Gerencia	GER	Alta
2	Facturación	FAC	Media
3	Almacén	ALM	Media
4	Administración	ADM	Media
5	Operaciones	OPE	Media
6	TI	TI	Media
13	PCP	PCP	Media
\.


--
-- Data for Name: HistorialConsumo; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."HistorialConsumo" ("Id", "ImpresoraId", "ConsumibleId", "FechaCambio", "PaginasImpresas", "Costo") FROM stdin;
\.


--
-- Data for Name: Impresoras; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Impresoras" ("Id", "Ip", "Modelo", "Fabricante", "Departamento", "FechaInstalacion", "EstadoPing", "ComputadorasVinculadas", "ContadorColor", "ContadorMonocromo", "ContadorTotalPaginas", "DiasEstimadosAgotamientoTonner", "DiasEstimadosMantenimiento", "FechaEstimadaAgotamientoTonner", "FechaEstimadaMantenimiento", "NivelTonnerAmarillo", "NivelTonnerCian", "NivelTonnerMagenta", "NivelTonnerNegro", "PaginasDesdeUltimoMantenimiento", "PromedioPaginasDiario", "PromedioTonnerDiario", "UltimoMantenimiento", "AlertaStockCritico", "ConsumibleTonerId", "ComunidadSnmp", "LatenciaMs", "RutaManual") FROM stdin;
5	127.0.0.15	MF455 Traco	Desconocido	Facturación	-infinity	EnLinea	\N	0	0	86087	9	5	2026-11-10 01:54:49.755568-04	2026-11-06 01:54:49.755568-04	\N	\N	\N	64	13649	247.1	0	\N	f	\N	public	11	\N
6	127.0.0.16	MF264 Rrhh	Desconocido	Facturación	-infinity	EnLinea	\N	0	0	31874	31	149	2026-12-02 01:54:49.756281-04	2027-03-30 01:54:49.756282-04	\N	\N	\N	78	1800	88.5	0	\N	f	\N	public	13	\N
7	127.0.0.17	RICOH MP3350 Cendi	Desconocido	Facturación	-infinity	EnLinea	\N	0	0	591501	72	363	2027-01-12 01:54:49.75705-04	2027-10-30 01:54:49.757892-04	\N	\N	\N	85	0	41.3	0	2026-10-07 05:27:36.535276-04	f	\N	public	15	\N
8	127.0.0.18	RICOH Cendi	Desconocido	Facturación	2026-10-06 20:00:00-04	EnLinea	\N	0	0	591610	42	288	2026-12-13 01:54:49.760229-04	2027-08-16 01:54:49.760231-04	\N	\N	\N	53	2256	44.2	0	\N	f	\N	public	3	\N
9	127.0.0.19	MF264 Overland	Desconocido	Facturación	2026-10-06 20:00:00-04	EnLinea	\N	0	0	69155	9	188	2026-11-10 01:54:49.761494-04	2027-05-08 01:54:49.761495-04	\N	\N	\N	19	1805	70.3	0	\N	f	\N	public	5	\N
10	127.0.0.20	MF455 Principal CENDI	Desconocido	Facturación	2026-10-06 20:00:00-04	EnLinea	\N	0	0	58468	16	58	2026-11-17 01:54:49.76376-04	2026-12-29 01:54:49.763762-04	\N	\N	\N	91	3244	201.9	0	\N	f	\N	public	7	\N
11	127.0.0.21	MF264 Comercial OV-JD	Desconocido	Facturación	2026-10-06 20:00:00-04	EnLinea	\N	0	0	34285	86	429	2027-01-26 01:54:49.765019-04	2028-01-04 01:54:49.76502-04	\N	\N	\N	72	2431	29.3	0	\N	f	\N	public	9	\N
12	127.0.0.22	MF264 Comercial	Desconocido	Facturación	2026-10-06 20:00:00-04	EnLinea	\N	0	0	34616	86	880	2027-01-26 01:54:49.765998-04	2029-03-30 01:54:49.765998-04	\N	\N	\N	36	1800	14.6	0	\N	f	\N	public	11	\N
13	192.168.100.57	HP ETHERNET MULTI-ENVIRONMENT	Desconocido	Personal del Ingeniero	2026-10-28 04:58:00.182787-04	EnLinea	\N	0	0	0	88	309	2027-01-28 01:54:51.779133-04	2027-09-06 01:54:51.779135-04	\N	\N	\N	88	4200	35	0	\N	f	\N	public	3001	Uploads/Manuales/manual_impresora_13.pdf
14	127.0.0.24	Facturacion IR1643 Total	Canon	Facturación	2026-10-30 20:00:00-04	EnLinea	\N	0	0	261888	1	22	2026-11-02 01:54:51.780471-04	2026-11-23 01:54:51.780472-04	\N	\N	\N	9	1800	610.7	0	\N	t	\N	public	15	\N
15	127.0.0.25	Facturacion IR1643 Stargas	Canon	Facturación	2026-10-30 20:00:00-04	EnLinea	\N	0	0	247246	1	2	2026-11-02 01:54:51.781444-04	2026-11-03 01:54:51.781445-04	\N	\N	\N	14	13988	625.9	0	\N	t	\N	public	3	\N
16	127.0.0.26	MF264 PCP	Canon	PCP	2026-10-30 20:00:00-04	EnLinea	\N	0	0	192748	11	62	2026-11-12 01:54:51.782403-04	2027-01-02 01:54:51.782404-04	\N	\N	\N	28	9555	88.3	0	\N	f	\N	public	5	\N
17	127.0.0.27	IR1643 RRHH	Canon	RRHH	2026-10-30 20:00:00-04	EnLinea	\N	0	0	17897	10	85	2026-11-11 01:54:51.783251-04	2027-01-25 01:54:51.783252-04	\N	\N	\N	42	2433	147.7	0	\N	f	\N	public	7	\N
1	192.168.1.50			Facturación	-infinity	EnLinea	\N	0	0	119250	9	309	2026-11-10 01:54:49.752516-04	2027-09-06 01:54:49.752518-04	\N	\N	\N	9	4200	35	0	\N	t	1	public	3002	\N
2	127.0.0.1	Brother NC-8800w, Firmware Ver.N  ,MID 8C5-K3JFID 2		Facturación	-infinity	EnLinea	\N	0	0	100	14	309	2026-11-15 01:54:49.75339-04	2027-09-06 01:54:49.753391-04	\N	\N	\N	14	4200	35	0	\N	t	\N	public	2	Uploads/Manuales/manual_impresora_2.pdf
3	127.0.0.13	MF264 Stargas	Desconocido	Facturación	-infinity	EnLinea	\N	0	0	147064	10	19	2026-11-11 01:54:49.754152-04	2026-11-20 01:54:49.754153-04	\N	\N	\N	28	13128	97	0	\N	f	\N	public	7	\N
4	127.0.0.14	MF455 Overland	Desconocido	Facturación	-infinity	EnLinea	\N	0	0	50831	49	440	2026-12-20 01:54:49.75486-04	2028-01-15 01:54:49.754861-04	\N	\N	\N	42	1800	30	0	\N	f	\N	public	9	\N
\.


--
-- Data for Name: MantenimientosImpresora; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MantenimientosImpresora" ("Id", "ImpresoraId", "Fecha", "Tipo", "Descripcion", "RealizadoPor") FROM stdin;
1	7	2026-10-07 05:27:36.535276-04	Preventivo	General	Francisco unda
\.


--
-- Data for Name: MetricasRed; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."MetricasRed" ("Id", "DispositivoIP", "LatenciaMS", "PaquetesPerdidos", "FechaHora") FROM stdin;
\.


--
-- Data for Name: RegistrosConsumoDiario; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."RegistrosConsumoDiario" ("Id", "ImpresoraId", "Fecha", "Contador", "Consumo", "Observacion") FROM stdin;
1	3	2024-12-01 20:00:00-04	117353	0	\N
2	3	2024-12-02 20:00:00-04	117447	94	\N
3	3	2024-12-03 20:00:00-04	117566	119	\N
4	3	2024-12-04 20:00:00-04	117669	103	\N
5	3	2024-12-05 20:00:00-04	117849	180	\N
6	3	2024-12-08 20:00:00-04	117935	86	\N
7	3	2024-12-09 20:00:00-04	118031	96	\N
8	3	2024-12-10 20:00:00-04	118169	138	\N
9	3	2024-12-11 20:00:00-04	118224	55	\N
10	3	2024-12-12 20:00:00-04	118390	166	\N
11	3	2024-12-15 20:00:00-04	118542	152	\N
12	3	2024-12-16 20:00:00-04	118702	160	\N
13	3	2024-12-17 20:00:00-04	118887	185	\N
14	3	2024-12-18 20:00:00-04	118965	78	\N
15	3	2024-12-19 20:00:00-04	119036	71	\N
16	3	2024-12-22 20:00:00-04	119142	106	\N
17	3	2024-12-25 20:00:00-04	119245	103	\N
18	3	2024-12-26 20:00:00-04	119410	165	\N
19	3	2024-12-29 20:00:00-04	119554	144	\N
20	4	2024-12-01 20:00:00-04	1477	0	\N
21	4	2024-12-02 20:00:00-04	1508	31	\N
22	4	2024-12-03 20:00:00-04	1527	19	\N
23	4	2024-12-04 20:00:00-04	1537	10	\N
24	4	2024-12-05 20:00:00-04	1603	66	\N
25	4	2024-12-08 20:00:00-04	1648	45	\N
26	4	2024-12-09 20:00:00-04	1676	28	\N
27	4	2024-12-10 20:00:00-04	1685	9	\N
28	4	2024-12-11 20:00:00-04	1693	8	\N
29	4	2024-12-12 20:00:00-04	1703	10	\N
30	4	2024-12-15 20:00:00-04	1724	21	\N
31	4	2024-12-16 20:00:00-04	1747	23	\N
32	4	2024-12-17 20:00:00-04	1768	21	\N
33	4	2024-12-18 20:00:00-04	1784	16	\N
34	4	2024-12-19 20:00:00-04	1794	10	\N
35	4	2024-12-22 20:00:00-04	1824	30	\N
36	4	2024-12-25 20:00:00-04	1828	4	\N
37	4	2024-12-26 20:00:00-04	1833	5	\N
38	4	2024-12-29 20:00:00-04	1839	6	\N
39	5	2024-12-01 20:00:00-04	8516	0	\N
40	5	2024-12-02 20:00:00-04	8780	264	\N
41	5	2024-12-03 20:00:00-04	9101	321	\N
42	5	2024-12-04 20:00:00-04	9275	174	\N
43	5	2024-12-05 20:00:00-04	9571	296	\N
44	5	2024-12-08 20:00:00-04	9736	165	\N
45	5	2024-12-09 20:00:00-04	10110	374	\N
46	5	2024-12-10 20:00:00-04	10416	306	\N
47	5	2024-12-11 20:00:00-04	10576	160	\N
48	5	2024-12-12 20:00:00-04	10736	160	\N
49	5	2024-12-15 20:00:00-04	11011	275	\N
50	5	2024-12-16 20:00:00-04	11146	135	\N
51	5	2024-12-17 20:00:00-04	11378	232	\N
52	5	2024-12-18 20:00:00-04	11583	205	\N
53	5	2024-12-19 20:00:00-04	11818	235	\N
54	5	2024-12-22 20:00:00-04	12070	252	\N
55	5	2024-12-25 20:00:00-04	12350	280	\N
56	5	2024-12-26 20:00:00-04	12433	83	\N
57	5	2024-12-29 20:00:00-04	12626	193	\N
58	6	2024-12-01 20:00:00-04	15138	0	\N
59	6	2024-12-02 20:00:00-04	15169	31	\N
60	6	2024-12-03 20:00:00-04	15203	34	\N
61	6	2024-12-04 20:00:00-04	15381	178	\N
62	6	2024-12-05 20:00:00-04	15414	33	\N
63	6	2024-12-08 20:00:00-04	15430	16	\N
64	6	2024-12-09 20:00:00-04	15454	24	\N
65	6	2024-12-10 20:00:00-04	15504	50	\N
66	6	2024-12-11 20:00:00-04	15533	29	\N
67	6	2024-12-12 20:00:00-04	15818	285	\N
68	6	2024-12-15 20:00:00-04	15847	29	\N
69	6	2024-12-16 20:00:00-04	15870	23	\N
70	6	2024-12-17 20:00:00-04	15908	38	\N
71	6	2024-12-18 20:00:00-04	15935	27	\N
72	6	2024-12-19 20:00:00-04	15946	11	\N
73	6	2024-12-22 20:00:00-04	15956	10	\N
74	6	2024-12-25 20:00:00-04	15990	34	\N
75	6	2024-12-26 20:00:00-04	16168	178	\N
76	6	2024-12-29 20:00:00-04	16199	31	\N
77	7	2024-12-01 20:00:00-04	590799	0	\N
78	7	2024-12-02 20:00:00-04	590867	68	\N
79	7	2024-12-03 20:00:00-04	590878	11	\N
80	7	2024-12-04 20:00:00-04	590912	34	\N
81	7	2024-12-05 20:00:00-04	590946	34	\N
82	7	2024-12-08 20:00:00-04	590966	20	\N
83	7	2024-12-09 20:00:00-04	590987	21	\N
84	7	2024-12-10 20:00:00-04	591028	41	\N
85	7	2024-12-11 20:00:00-04	591058	30	\N
86	7	2024-12-12 20:00:00-04	591098	40	\N
87	7	2024-12-15 20:00:00-04	591131	33	\N
88	7	2024-12-16 20:00:00-04	591155	24	\N
89	7	2024-12-17 20:00:00-04	591238	83	\N
90	7	2024-12-18 20:00:00-04	591238	0	\N
91	7	2024-12-19 20:00:00-04	591359	121	\N
92	7	2024-12-22 20:00:00-04	591397	38	\N
93	7	2024-12-25 20:00:00-04	591415	18	\N
94	7	2024-12-26 20:00:00-04	591485	70	\N
95	7	2024-12-29 20:00:00-04	591501	16	\N
96	3	2024-01-01 20:00:00-04	119601	0	\N
97	3	2024-01-02 20:00:00-04	119629	28	\N
98	3	2024-01-05 20:00:00-04	119733	104	\N
99	3	2024-01-06 20:00:00-04	119791	58	\N
100	3	2024-01-07 20:00:00-04	119891	100	\N
101	3	2024-01-08 20:00:00-04	119967	76	\N
102	3	2024-01-09 20:00:00-04	119967	0	\N
103	3	2024-01-12 20:00:00-04	119985	18	\N
104	3	2024-01-13 20:00:00-04	120104	119	\N
105	3	2024-01-14 20:00:00-04	120228	124	\N
106	3	2024-01-15 20:00:00-04	120321	93	\N
107	3	2024-01-16 20:00:00-04	120456	135	\N
108	3	2024-01-19 20:00:00-04	120602	146	\N
109	3	2024-01-20 20:00:00-04	120716	114	\N
110	3	2024-01-21 20:00:00-04	120910	194	\N
111	3	2024-01-22 20:00:00-04	121050	140	\N
112	3	2024-01-23 20:00:00-04	121199	149	\N
113	3	2024-01-26 20:00:00-04	121457	258	\N
114	3	2024-01-27 20:00:00-04	121650	193	\N
115	3	2024-01-28 20:00:00-04	121775	125	\N
116	3	2024-01-29 20:00:00-04	121830	55	\N
117	3	2024-01-30 20:00:00-04	121925	95	\N
118	4	2024-01-01 20:00:00-04	1835	0	\N
119	4	2024-01-02 20:00:00-04	1846	11	\N
120	4	2024-01-05 20:00:00-04	2007	161	\N
121	4	2024-01-06 20:00:00-04	2041	34	\N
122	4	2024-01-07 20:00:00-04	2072	31	\N
123	4	2024-01-08 20:00:00-04	2091	19	\N
124	4	2024-01-09 20:00:00-04	2091	0	\N
125	4	2024-01-12 20:00:00-04	2091	0	\N
126	4	2024-01-13 20:00:00-04	50457	0	\N
127	4	2024-01-14 20:00:00-04	50463	6	\N
128	4	2024-01-15 20:00:00-04	50485	22	\N
129	4	2024-01-16 20:00:00-04	50588	103	\N
130	4	2024-01-19 20:00:00-04	50602	14	\N
131	4	2024-01-20 20:00:00-04	50619	17	\N
132	4	2024-01-21 20:00:00-04	50656	37	\N
133	4	2024-01-22 20:00:00-04	50688	32	\N
134	4	2024-01-23 20:00:00-04	50705	17	\N
135	4	2024-01-26 20:00:00-04	50754	49	\N
136	4	2024-01-27 20:00:00-04	50780	26	\N
137	4	2024-01-28 20:00:00-04	50787	7	\N
138	4	2024-01-29 20:00:00-04	50806	19	\N
139	4	2024-01-30 20:00:00-04	50831	25	\N
140	5	2024-01-01 20:00:00-04	12626	0	\N
141	5	2024-01-02 20:00:00-04	12748	122	\N
142	5	2024-01-05 20:00:00-04	13116	368	\N
143	5	2024-01-06 20:00:00-04	13301	185	\N
144	5	2024-01-07 20:00:00-04	13395	94	\N
145	5	2024-01-08 20:00:00-04	13656	261	\N
146	5	2024-01-09 20:00:00-04	13656	0	\N
147	5	2024-01-12 20:00:00-04	13710	54	\N
148	5	2024-01-13 20:00:00-04	13996	458	\N
149	5	2024-01-14 20:00:00-04	14168	172	\N
150	5	2024-01-15 20:00:00-04	14324	156	\N
151	5	2024-01-16 20:00:00-04	14620	296	\N
152	5	2024-01-19 20:00:00-04	14912	292	\N
153	5	2024-01-20 20:00:00-04	15059	147	\N
154	5	2024-01-21 20:00:00-04	15363	304	\N
155	5	2024-01-22 20:00:00-04	15616	253	\N
156	5	2024-01-23 20:00:00-04	15896	280	\N
157	5	2024-01-26 20:00:00-04	16360	464	\N
158	5	2024-01-27 20:00:00-04	16568	208	\N
159	5	2024-01-28 20:00:00-04	16729	161	\N
160	5	2024-01-29 20:00:00-04	16983	254	\N
161	5	2024-01-30 20:00:00-04	17191	208	\N
162	6	2024-01-01 20:00:00-04	16199	0	\N
163	6	2024-01-02 20:00:00-04	16208	9	\N
164	6	2024-01-05 20:00:00-04	16273	65	\N
165	6	2024-01-06 20:00:00-04	16292	19	\N
166	6	2024-01-07 20:00:00-04	16298	6	\N
167	6	2024-01-08 20:00:00-04	16306	8	\N
168	6	2024-01-09 20:00:00-04	16306	0	\N
169	6	2024-01-12 20:00:00-04	16322	16	\N
170	6	2024-01-13 20:00:00-04	16518	211	\N
171	6	2024-01-14 20:00:00-04	16533	15	\N
172	6	2024-01-15 20:00:00-04	16672	139	\N
173	6	2024-01-16 20:00:00-04	16796	124	\N
174	6	2024-01-19 20:00:00-04	16913	117	\N
175	6	2024-01-20 20:00:00-04	16933	20	\N
176	6	2024-01-21 20:00:00-04	16976	43	\N
177	6	2024-01-22 20:00:00-04	16998	22	\N
178	6	2024-01-23 20:00:00-04	17030	32	\N
179	6	2024-01-26 20:00:00-04	17097	67	\N
180	6	2024-01-27 20:00:00-04	17226	129	\N
181	6	2024-01-28 20:00:00-04	17236	10	\N
182	6	2024-01-29 20:00:00-04	17377	141	\N
183	6	2024-01-30 20:00:00-04	17426	49	\N
184	8	2024-01-01 20:00:00-04	591501	0	\N
185	8	2024-01-02 20:00:00-04	591501	0	\N
186	8	2024-01-05 20:00:00-04	591504	3	\N
187	8	2024-01-06 20:00:00-04	591523	19	\N
188	8	2024-01-07 20:00:00-04	591527	4	\N
189	8	2024-01-08 20:00:00-04	591545	18	\N
190	8	2024-01-09 20:00:00-04	591545	0	\N
191	8	2024-01-12 20:00:00-04	591550	5	\N
192	8	2024-01-13 20:00:00-04	591598	48	\N
193	8	2024-01-14 20:00:00-04	591610	0	\N
194	8	2024-01-15 20:00:00-04	2123	0	\N
195	8	2024-01-16 20:00:00-04	2168	45	\N
196	8	2024-01-19 20:00:00-04	2220	52	\N
197	8	2024-01-20 20:00:00-04	2230	10	\N
198	8	2024-01-21 20:00:00-04	2266	36	\N
199	8	2024-01-22 20:00:00-04	2282	16	\N
200	8	2024-01-23 20:00:00-04	2336	54	\N
201	8	2024-01-26 20:00:00-04	2539	203	\N
202	8	2024-01-27 20:00:00-04	2600	61	\N
203	8	2024-01-28 20:00:00-04	2664	64	\N
204	8	2024-01-29 20:00:00-04	2794	130	\N
205	8	2024-01-30 20:00:00-04	2803	9	\N
206	3	2025-02-02 20:00:00-04	122002	0	\N
207	3	2025-02-03 20:00:00-04	122096	94	\N
208	3	2025-02-04 20:00:00-04	122173	77	\N
209	3	2025-02-05 20:00:00-04	122251	78	\N
210	3	2025-02-06 20:00:00-04	122319	68	\N
211	3	2025-02-09 20:00:00-04	122399	80	\N
212	3	2025-02-10 20:00:00-04	122507	108	\N
213	3	2025-02-11 20:00:00-04	122601	94	\N
214	3	2025-02-12 20:00:00-04	122787	186	\N
215	3	2025-02-13 20:00:00-04	122801	14	\N
216	3	2025-02-16 20:00:00-04	122830	29	\N
217	3	2025-02-17 20:00:00-04	122946	116	\N
218	3	2025-02-18 20:00:00-04	123071	125	\N
219	3	2025-02-19 20:00:00-04	123147	76	\N
220	3	2025-02-20 20:00:00-04	123201	54	\N
221	3	2025-02-23 20:00:00-04	123327	126	\N
222	3	2025-02-24 20:00:00-04	123478	151	\N
223	3	2025-02-25 20:00:00-04	123573	95	\N
224	3	2025-02-26 20:00:00-04	123666	93	\N
225	3	2025-02-27 20:00:00-04	123790	124	\N
226	9	2025-02-02 20:00:00-04	50846	0	\N
227	9	2025-02-03 20:00:00-04	50882	36	\N
228	9	2025-02-04 20:00:00-04	50922	40	\N
229	9	2025-02-05 20:00:00-04	50957	35	\N
230	9	2025-02-06 20:00:00-04	50971	14	\N
231	9	2025-02-09 20:00:00-04	51005	34	\N
232	9	2025-02-10 20:00:00-04	51029	24	\N
233	9	2025-02-11 20:00:00-04	51078	49	\N
234	9	2025-02-12 20:00:00-04	51115	37	\N
235	9	2025-02-13 20:00:00-04	51149	34	\N
236	9	2025-02-16 20:00:00-04	51175	26	\N
237	9	2025-02-17 20:00:00-04	51201	26	\N
238	9	2025-02-18 20:00:00-04	51256	55	\N
239	9	2025-02-19 20:00:00-04	51304	48	\N
240	9	2025-02-20 20:00:00-04	51350	46	\N
241	9	2025-02-23 20:00:00-04	51384	34	\N
242	9	2025-02-24 20:00:00-04	51412	28	\N
243	9	2025-02-25 20:00:00-04	51495	83	\N
244	9	2025-02-26 20:00:00-04	51534	39	\N
245	9	2025-02-27 20:00:00-04	51555	21	\N
246	5	2025-02-02 20:00:00-04	17601	0	\N
247	5	2025-02-03 20:00:00-04	17837	236	\N
248	5	2025-02-04 20:00:00-04	17992	155	\N
249	5	2025-02-05 20:00:00-04	18173	181	\N
250	5	2025-02-06 20:00:00-04	18373	200	\N
251	5	2025-02-09 20:00:00-04	18718	345	\N
252	5	2025-02-10 20:00:00-04	18879	161	\N
253	5	2025-02-11 20:00:00-04	19086	207	\N
254	5	2025-02-12 20:00:00-04	19478	392	\N
255	5	2025-02-13 20:00:00-04	19677	199	\N
256	5	2025-02-16 20:00:00-04	19814	137	\N
257	5	2025-02-17 20:00:00-04	20148	334	\N
258	5	2025-02-18 20:00:00-04	20369	221	\N
259	5	2025-02-19 20:00:00-04	20618	249	\N
260	5	2025-02-20 20:00:00-04	20702	84	\N
261	5	2025-02-23 20:00:00-04	21485	783	\N
262	5	2025-02-24 20:00:00-04	21787	302	\N
263	5	2025-02-25 20:00:00-04	22188	401	\N
264	5	2025-02-26 20:00:00-04	22390	202	\N
265	5	2025-02-27 20:00:00-04	22573	183	\N
266	6	2025-02-02 20:00:00-04	17454	0	\N
267	6	2025-02-03 20:00:00-04	17510	56	\N
268	6	2025-02-04 20:00:00-04	17544	34	\N
269	6	2025-02-05 20:00:00-04	17546	2	\N
270	6	2025-02-06 20:00:00-04	17576	30	\N
271	6	2025-02-09 20:00:00-04	17589	13	\N
272	6	2025-02-10 20:00:00-04	17733	144	\N
273	6	2025-02-11 20:00:00-04	18000	267	\N
274	6	2025-02-12 20:00:00-04	18298	298	\N
275	6	2025-02-13 20:00:00-04	18300	2	\N
276	6	2025-02-16 20:00:00-04	18301	1	\N
277	6	2025-02-17 20:00:00-04	18310	9	\N
278	6	2025-02-18 20:00:00-04	18312	2	\N
279	6	2025-02-19 20:00:00-04	18321	9	\N
280	6	2025-02-20 20:00:00-04	18343	22	\N
281	6	2025-02-23 20:00:00-04	18417	74	\N
282	6	2025-02-24 20:00:00-04	18498	81	\N
283	6	2025-02-25 20:00:00-04	18538	40	\N
284	6	2025-02-26 20:00:00-04	18734	196	\N
285	6	2025-02-27 20:00:00-04	18831	97	\N
286	10	2025-02-02 20:00:00-04	2839	0	\N
287	10	2025-02-03 20:00:00-04	2894	55	\N
288	10	2025-02-04 20:00:00-04	2981	87	\N
289	10	2025-02-05 20:00:00-04	3107	126	\N
290	10	2025-02-06 20:00:00-04	3331	224	\N
291	10	2025-02-09 20:00:00-04	3512	181	\N
292	10	2025-02-10 20:00:00-04	3748	236	\N
293	10	2025-02-11 20:00:00-04	3925	177	\N
294	10	2025-02-12 20:00:00-04	4225	300	\N
295	10	2025-02-13 20:00:00-04	4376	151	\N
296	10	2025-02-16 20:00:00-04	4431	55	\N
297	10	2025-02-17 20:00:00-04	4522	91	\N
298	10	2025-02-18 20:00:00-04	4698	176	\N
299	10	2025-02-19 20:00:00-04	4978	280	\N
300	10	2025-02-20 20:00:00-04	4986	8	\N
301	10	2025-02-23 20:00:00-04	5065	79	\N
302	10	2025-02-24 20:00:00-04	5151	86	\N
303	10	2025-02-25 20:00:00-04	5238	87	\N
304	10	2025-02-26 20:00:00-04	5420	182	\N
305	10	2025-02-27 20:00:00-04	5531	111	\N
306	3	2025-03-04 20:00:00-04	123926	0	\N
307	3	2025-03-05 20:00:00-04	123977	51	\N
308	3	2025-03-06 20:00:00-04	124121	144	\N
309	3	2025-03-09 20:00:00-04	124328	207	\N
310	3	2025-03-10 20:00:00-04	124486	158	\N
311	3	2025-03-11 20:00:00-04	124661	175	\N
312	3	2025-03-12 20:00:00-04	124724	63	\N
313	3	2025-03-13 20:00:00-04	124791	67	\N
314	3	2025-03-16 20:00:00-04	124910	119	\N
315	3	2025-03-17 20:00:00-04	125044	134	\N
316	3	2025-03-18 20:00:00-04	125189	145	\N
317	3	2025-03-19 20:00:00-04	125297	108	\N
318	3	2025-03-20 20:00:00-04	125418	121	\N
319	3	2025-03-23 20:00:00-04	125517	99	\N
320	3	2025-03-24 20:00:00-04	125599	82	\N
321	3	2025-03-25 20:00:00-04	125709	110	\N
322	3	2025-03-26 20:00:00-04	125879	170	\N
323	3	2025-03-27 20:00:00-04	125926	47	\N
324	3	2025-03-30 20:00:00-04	126009	83	\N
325	9	2025-03-04 20:00:00-04	51586	0	\N
326	9	2025-03-05 20:00:00-04	51617	31	\N
327	9	2025-03-06 20:00:00-04	51682	65	\N
328	9	2025-03-09 20:00:00-04	51721	39	\N
329	9	2025-03-10 20:00:00-04	51797	76	\N
330	9	2025-03-11 20:00:00-04	51817	20	\N
331	9	2025-03-12 20:00:00-04	51839	22	\N
332	9	2025-03-13 20:00:00-04	51853	14	\N
333	9	2025-03-16 20:00:00-04	51899	46	\N
334	9	2025-03-17 20:00:00-04	51950	51	\N
335	9	2025-03-18 20:00:00-04	52014	64	\N
336	9	2025-03-19 20:00:00-04	52167	153	\N
337	9	2025-03-20 20:00:00-04	52255	88	\N
338	9	2025-03-23 20:00:00-04	52399	144	\N
339	9	2025-03-24 20:00:00-04	52515	116	\N
340	9	2025-03-25 20:00:00-04	52548	33	\N
341	9	2025-03-26 20:00:00-04	52689	141	\N
342	9	2025-03-27 20:00:00-04	52737	48	\N
343	9	2025-03-30 20:00:00-04	52780	43	\N
344	5	2025-03-04 20:00:00-04	22928	0	\N
345	5	2025-03-05 20:00:00-04	23153	225	\N
346	5	2025-03-06 20:00:00-04	23388	235	\N
347	5	2025-03-09 20:00:00-04	23579	191	\N
348	5	2025-03-10 20:00:00-04	23991	412	\N
349	5	2025-03-11 20:00:00-04	24228	237	\N
350	5	2025-03-12 20:00:00-04	24512	284	\N
351	5	2025-03-13 20:00:00-04	24809	297	\N
352	5	2025-03-16 20:00:00-04	25120	311	\N
353	5	2025-03-17 20:00:00-04	25469	349	\N
354	5	2025-03-18 20:00:00-04	25679	210	\N
355	5	2025-03-19 20:00:00-04	25797	118	\N
356	5	2025-03-20 20:00:00-04	26077	280	\N
357	5	2025-03-23 20:00:00-04	26498	421	\N
358	5	2025-03-24 20:00:00-04	26706	208	\N
359	5	2025-03-25 20:00:00-04	26866	160	\N
360	5	2025-03-26 20:00:00-04	27001	135	\N
361	5	2025-03-27 20:00:00-04	27153	152	\N
362	5	2025-03-30 20:00:00-04	27340	187	\N
363	6	2025-03-04 20:00:00-04	18938	0	\N
364	6	2025-03-05 20:00:00-04	18990	52	\N
365	6	2025-03-06 20:00:00-04	19003	13	\N
366	6	2025-03-09 20:00:00-04	19056	53	\N
367	6	2025-03-10 20:00:00-04	19124	68	\N
368	6	2025-03-11 20:00:00-04	19272	148	\N
369	6	2025-03-12 20:00:00-04	19391	119	\N
370	6	2025-03-13 20:00:00-04	19508	117	\N
371	6	2025-03-16 20:00:00-04	19632	124	\N
372	6	2025-03-17 20:00:00-04	19766	134	\N
373	6	2025-03-18 20:00:00-04	19810	44	\N
374	6	2025-03-19 20:00:00-04	19920	110	\N
375	6	2025-03-20 20:00:00-04	19985	65	\N
376	6	2025-03-23 20:00:00-04	20111	126	\N
377	6	2025-03-24 20:00:00-04	20219	108	\N
378	6	2025-03-25 20:00:00-04	20291	72	\N
379	6	2025-03-26 20:00:00-04	20397	106	\N
380	6	2025-03-27 20:00:00-04	20445	48	\N
381	6	2025-03-30 20:00:00-04	20840	395	\N
382	10	2025-03-04 20:00:00-04	5677	0	\N
383	10	2025-03-05 20:00:00-04	5930	253	\N
384	10	2025-03-06 20:00:00-04	6061	131	\N
385	10	2025-03-09 20:00:00-04	6308	247	\N
386	10	2025-03-10 20:00:00-04	6492	184	\N
387	10	2025-03-11 20:00:00-04	6725	233	\N
388	10	2025-03-12 20:00:00-04	6897	172	\N
389	10	2025-03-13 20:00:00-04	7086	189	\N
390	10	2025-03-16 20:00:00-04	7297	211	\N
391	10	2025-03-17 20:00:00-04	7451	154	\N
392	10	2025-03-18 20:00:00-04	7612	161	\N
393	10	2025-03-19 20:00:00-04	7812	200	\N
394	10	2025-03-20 20:00:00-04	7906	94	\N
395	10	2025-03-23 20:00:00-04	8201	295	\N
396	10	2025-03-24 20:00:00-04	8313	112	\N
397	10	2025-03-25 20:00:00-04	8460	147	\N
398	10	2025-03-26 20:00:00-04	8615	155	\N
399	10	2025-03-27 20:00:00-04	8791	176	\N
400	10	2025-03-30 20:00:00-04	8918	127	\N
401	3	2025-03-31 20:00:00-04	126185	0	\N
402	3	2025-04-01 20:00:00-04	126309	124	\N
403	3	2025-04-02 20:00:00-04	126443	134	\N
404	3	2025-04-03 20:00:00-04	126533	90	\N
405	3	2025-04-06 20:00:00-04	126630	97	\N
406	3	2025-04-07 20:00:00-04	126742	112	\N
407	3	2025-04-08 20:00:00-04	126851	109	\N
408	3	2025-04-09 20:00:00-04	126921	70	\N
409	3	2025-04-10 20:00:00-04	127061	140	\N
410	3	2025-04-13 20:00:00-04	127200	139	\N
411	3	2025-04-14 20:00:00-04	127305	105	\N
412	3	2025-04-15 20:00:00-04	127383	78	\N
413	3	2025-04-20 20:00:00-04	127538	155	\N
414	3	2025-04-21 20:00:00-04	127621	83	\N
415	3	2025-04-22 20:00:00-04	127806	185	\N
416	3	2025-04-23 20:00:00-04	127996	190	\N
417	3	2025-04-24 20:00:00-04	128003	7	\N
418	3	2025-04-27 20:00:00-04	128105	102	\N
419	3	2025-04-28 20:00:00-04	128189	84	\N
420	3	2025-04-29 20:00:00-04	128222	33	\N
421	9	2025-03-31 20:00:00-04	52810	0	\N
422	9	2025-04-01 20:00:00-04	52832	22	\N
423	9	2025-04-02 20:00:00-04	52861	29	\N
424	9	2025-04-03 20:00:00-04	52942	81	\N
425	9	2025-04-06 20:00:00-04	52975	33	\N
426	9	2025-04-07 20:00:00-04	53001	26	\N
427	9	2025-04-08 20:00:00-04	53039	38	\N
428	9	2025-04-09 20:00:00-04	53098	59	\N
429	9	2025-04-10 20:00:00-04	53126	28	\N
430	9	2025-04-13 20:00:00-04	53152	26	\N
431	9	2025-04-14 20:00:00-04	53199	47	\N
432	9	2025-04-15 20:00:00-04	53226	27	\N
433	9	2025-04-20 20:00:00-04	53240	14	\N
434	9	2025-04-21 20:00:00-04	53257	17	\N
435	9	2025-04-22 20:00:00-04	53283	26	\N
436	9	2025-04-23 20:00:00-04	53314	31	\N
437	9	2025-04-24 20:00:00-04	53356	42	\N
438	9	2025-04-27 20:00:00-04	53377	21	\N
439	9	2025-04-28 20:00:00-04	53417	40	\N
440	9	2025-04-29 20:00:00-04	53516	99	\N
441	5	2025-03-31 20:00:00-04	27561	0	\N
442	5	2025-04-01 20:00:00-04	27827	266	\N
443	5	2025-04-02 20:00:00-04	27963	136	\N
444	5	2025-04-03 20:00:00-04	28090	127	\N
445	5	2025-04-06 20:00:00-04	28262	172	\N
446	5	2025-04-07 20:00:00-04	28563	301	\N
447	5	2025-04-08 20:00:00-04	28831	268	\N
448	5	2025-04-09 20:00:00-04	29127	296	\N
449	5	2025-04-10 20:00:00-04	29378	251	\N
450	5	2025-04-13 20:00:00-04	29648	270	\N
451	5	2025-04-14 20:00:00-04	29723	75	\N
452	5	2025-04-15 20:00:00-04	29877	154	\N
453	5	2025-04-20 20:00:00-04	30067	190	\N
454	5	2025-04-21 20:00:00-04	30321	254	\N
455	5	2025-04-22 20:00:00-04	30507	186	\N
456	5	2025-04-23 20:00:00-04	30699	192	\N
457	5	2025-04-24 20:00:00-04	30862	163	\N
458	5	2025-04-27 20:00:00-04	31168	306	\N
459	5	2025-04-28 20:00:00-04	31354	186	\N
460	5	2025-04-29 20:00:00-04	31503	149	\N
461	6	2025-03-31 20:00:00-04	20999	0	\N
462	6	2025-04-01 20:00:00-04	21150	151	\N
463	6	2025-04-02 20:00:00-04	21234	84	\N
464	6	2025-04-03 20:00:00-04	21242	8	\N
465	6	2025-04-06 20:00:00-04	21301	59	\N
466	6	2025-04-07 20:00:00-04	21399	98	\N
467	6	2025-04-08 20:00:00-04	21443	44	\N
468	6	2025-04-09 20:00:00-04	21486	43	\N
469	6	2025-04-10 20:00:00-04	21515	29	\N
470	6	2025-04-13 20:00:00-04	21620	105	\N
471	6	2025-04-14 20:00:00-04	21705	85	\N
472	6	2025-04-15 20:00:00-04	21762	57	\N
473	6	2025-04-20 20:00:00-04	22002	240	\N
474	6	2025-04-21 20:00:00-04	22465	463	\N
475	6	2025-04-22 20:00:00-04	22899	434	\N
476	6	2025-04-23 20:00:00-04	23217	318	\N
477	6	2025-04-24 20:00:00-04	23850	633	\N
478	6	2025-04-27 20:00:00-04	24002	152	\N
479	6	2025-04-28 20:00:00-04	24098	96	\N
480	6	2025-04-29 20:00:00-04	24158	60	\N
481	10	2025-03-31 20:00:00-04	9105	0	\N
482	10	2025-04-01 20:00:00-04	9322	217	\N
483	10	2025-04-02 20:00:00-04	9433	111	\N
484	10	2025-04-03 20:00:00-04	9588	155	\N
485	10	2025-04-06 20:00:00-04	9768	180	\N
486	10	2025-04-07 20:00:00-04	9965	197	\N
487	10	2025-04-08 20:00:00-04	10236	271	\N
488	10	2025-04-09 20:00:00-04	10454	218	\N
489	10	2025-04-10 20:00:00-04	10618	164	\N
490	10	2025-04-13 20:00:00-04	10959	341	\N
491	10	2025-04-14 20:00:00-04	11117	158	\N
492	10	2025-04-15 20:00:00-04	11330	213	\N
493	10	2025-04-20 20:00:00-04	11427	97	\N
494	10	2025-04-21 20:00:00-04	11599	172	\N
495	10	2025-04-22 20:00:00-04	11734	135	\N
496	10	2025-04-23 20:00:00-04	11987	253	\N
497	10	2025-04-24 20:00:00-04	12080	93	\N
498	10	2025-04-27 20:00:00-04	12243	163	\N
499	10	2025-04-28 20:00:00-04	12397	154	\N
500	10	2025-04-29 20:00:00-04	12570	173	\N
501	3	2025-05-01 20:00:00-04	128321	0	\N
502	3	2025-05-04 20:00:00-04	128443	122	\N
503	3	2025-05-05 20:00:00-04	128512	69	\N
504	3	2025-05-06 20:00:00-04	128646	134	\N
505	3	2025-05-07 20:00:00-04	128712	66	\N
506	3	2025-05-08 20:00:00-04	128842	130	\N
507	3	2025-05-11 20:00:00-04	128962	120	\N
508	3	2025-05-12 20:00:00-04	129035	73	\N
509	3	2025-05-13 20:00:00-04	129181	146	\N
510	3	2025-05-14 20:00:00-04	129276	95	\N
511	3	2025-05-15 20:00:00-04	129411	135	\N
512	3	2025-05-18 20:00:00-04	129464	53	\N
513	3	2025-05-19 20:00:00-04	129499	35	\N
514	3	2025-05-20 20:00:00-04	129553	54	\N
515	3	2025-05-21 20:00:00-04	129658	105	\N
516	3	2025-05-22 20:00:00-04	129746	88	\N
517	3	2025-05-25 20:00:00-04	129804	58	\N
518	3	2025-05-26 20:00:00-04	129947	143	\N
519	3	2025-05-27 20:00:00-04	130050	103	\N
520	3	2025-05-28 20:00:00-04	130127	77	\N
521	3	2025-05-29 20:00:00-04	130235	108	\N
522	9	2025-05-01 20:00:00-04	53542	0	\N
523	9	2025-05-04 20:00:00-04	53553	11	\N
524	9	2025-05-05 20:00:00-04	53587	34	\N
525	9	2025-05-06 20:00:00-04	53614	27	\N
526	9	2025-05-07 20:00:00-04	53632	18	\N
527	9	2025-05-08 20:00:00-04	53662	30	\N
528	9	2025-05-11 20:00:00-04	53670	8	\N
529	9	2025-05-12 20:00:00-04	53699	29	\N
530	9	2025-05-13 20:00:00-04	53720	21	\N
531	9	2025-05-14 20:00:00-04	53751	31	\N
532	9	2025-05-15 20:00:00-04	53778	27	\N
533	9	2025-05-18 20:00:00-04	53809	31	\N
534	9	2025-05-19 20:00:00-04	53846	37	\N
535	9	2025-05-20 20:00:00-04	53872	26	\N
536	9	2025-05-21 20:00:00-04	53902	30	\N
537	9	2025-05-22 20:00:00-04	53958	56	\N
538	9	2025-05-25 20:00:00-04	53967	9	\N
539	9	2025-05-26 20:00:00-04	54001	34	\N
540	9	2025-05-27 20:00:00-04	54029	28	\N
541	9	2025-05-28 20:00:00-04	54102	73	\N
542	9	2025-05-29 20:00:00-04	54167	65	\N
543	5	2025-05-01 20:00:00-04	31699	0	\N
544	5	2025-05-04 20:00:00-04	31898	199	\N
545	5	2025-05-05 20:00:00-04	31997	99	\N
546	5	2025-05-06 20:00:00-04	32146	149	\N
547	5	2025-05-07 20:00:00-04	32237	91	\N
548	5	2025-05-08 20:00:00-04	32448	211	\N
549	5	2025-05-11 20:00:00-04	32739	291	\N
550	5	2025-05-12 20:00:00-04	32987	248	\N
551	5	2025-05-13 20:00:00-04	33077	90	\N
552	5	2025-05-14 20:00:00-04	33237	160	\N
553	5	2025-05-15 20:00:00-04	33448	211	\N
554	5	2025-05-18 20:00:00-04	33557	109	\N
555	5	2025-05-19 20:00:00-04	33789	232	\N
556	5	2025-05-20 20:00:00-04	34043	254	\N
557	5	2025-05-21 20:00:00-04	34110	67	\N
558	5	2025-05-22 20:00:00-04	34189	79	\N
559	5	2025-05-25 20:00:00-04	34284	95	\N
560	5	2025-05-26 20:00:00-04	34512	228	\N
561	5	2025-05-27 20:00:00-04	34781	269	\N
562	5	2025-05-28 20:00:00-04	35001	220	\N
563	5	2025-05-29 20:00:00-04	35274	273	\N
564	6	2025-05-01 20:00:00-04	24224	0	\N
565	6	2025-05-04 20:00:00-04	24274	50	\N
566	6	2025-05-05 20:00:00-04	24401	127	\N
567	6	2025-05-06 20:00:00-04	24520	119	\N
568	6	2025-05-07 20:00:00-04	24547	27	\N
569	6	2025-05-08 20:00:00-04	24568	21	\N
570	6	2025-05-11 20:00:00-04	24619	51	\N
571	6	2025-05-12 20:00:00-04	24677	58	\N
572	6	2025-05-13 20:00:00-04	24701	24	\N
573	6	2025-05-14 20:00:00-04	25010	309	\N
574	6	2025-05-15 20:00:00-04	25210	200	\N
575	6	2025-05-18 20:00:00-04	25323	113	\N
576	6	2025-05-19 20:00:00-04	25496	173	\N
577	6	2025-05-20 20:00:00-04	25765	269	\N
578	6	2025-05-21 20:00:00-04	25806	41	\N
579	6	2025-05-22 20:00:00-04	25886	80	\N
580	6	2025-05-25 20:00:00-04	25888	2	\N
581	6	2025-05-26 20:00:00-04	25968	80	\N
582	6	2025-05-27 20:00:00-04	26013	45	\N
583	6	2025-05-28 20:00:00-04	26187	174	\N
584	6	2025-05-29 20:00:00-04	26262	75	\N
585	10	2025-05-01 20:00:00-04	12785	0	\N
586	10	2025-05-04 20:00:00-04	12999	214	\N
587	10	2025-05-05 20:00:00-04	13127	128	\N
588	10	2025-05-06 20:00:00-04	13322	195	\N
589	10	2025-05-07 20:00:00-04	13517	195	\N
590	10	2025-05-08 20:00:00-04	13687	170	\N
591	10	2025-05-11 20:00:00-04	13867	180	\N
592	10	2025-05-12 20:00:00-04	14105	238	\N
593	10	2025-05-13 20:00:00-04	14191	86	\N
594	10	2025-05-14 20:00:00-04	14301	110	\N
595	10	2025-05-15 20:00:00-04	14565	264	\N
596	10	2025-05-18 20:00:00-04	14670	105	\N
597	10	2025-05-19 20:00:00-04	14879	209	\N
598	10	2025-05-20 20:00:00-04	15052	173	\N
599	10	2025-05-21 20:00:00-04	15222	170	\N
600	10	2025-05-22 20:00:00-04	15420	198	\N
601	10	2025-05-25 20:00:00-04	15476	56	\N
602	10	2025-05-26 20:00:00-04	15724	248	\N
603	10	2025-05-27 20:00:00-04	15992	268	\N
604	10	2025-05-28 20:00:00-04	16213	221	\N
605	10	2025-05-29 20:00:00-04	16553	340	\N
606	3	2025-06-01 20:00:00-04	130342	0	\N
607	3	2025-06-02 20:00:00-04	130498	156	\N
608	3	2025-06-03 20:00:00-04	130688	190	\N
609	3	2025-06-04 20:00:00-04	130802	114	\N
610	3	2025-06-05 20:00:00-04	130992	190	\N
611	3	2025-06-08 20:00:00-04	131169	177	\N
612	3	2025-06-09 20:00:00-04	131275	106	\N
613	3	2025-06-10 20:00:00-04	131378	103	\N
614	3	2025-06-11 20:00:00-04	131421	43	\N
615	3	2025-06-12 20:00:00-04	131572	151	\N
616	3	2025-06-15 20:00:00-04	131654	82	\N
617	3	2025-06-16 20:00:00-04	131796	142	\N
618	3	2025-06-17 20:00:00-04	131949	153	\N
619	3	2025-06-18 20:00:00-04	132014	65	\N
620	3	2025-06-19 20:00:00-04	132116	102	\N
621	3	2025-06-22 20:00:00-04	132342	226	\N
622	3	2025-06-24 20:00:00-04	132395	53	\N
623	3	2025-06-25 20:00:00-04	132441	46	\N
624	3	2025-06-26 20:00:00-04	132590	149	\N
625	3	2025-06-29 20:00:00-04	132723	133	\N
626	9	2025-06-01 20:00:00-04	54264	0	\N
627	9	2025-06-02 20:00:00-04	54301	37	\N
628	9	2025-06-03 20:00:00-04	54341	40	\N
629	9	2025-06-04 20:00:00-04	54389	48	\N
630	9	2025-06-05 20:00:00-04	54431	42	\N
631	9	2025-06-08 20:00:00-04	54513	82	\N
632	9	2025-06-09 20:00:00-04	54540	27	\N
633	9	2025-06-10 20:00:00-04	54562	22	\N
634	9	2025-06-11 20:00:00-04	54581	19	\N
635	9	2025-06-12 20:00:00-04	54690	109	\N
636	9	2025-06-15 20:00:00-04	54699	9	\N
637	9	2025-06-16 20:00:00-04	54725	26	\N
638	9	2025-06-17 20:00:00-04	54796	71	\N
639	9	2025-06-18 20:00:00-04	54824	28	\N
640	9	2025-06-19 20:00:00-04	54842	18	\N
641	9	2025-06-22 20:00:00-04	54884	42	\N
642	9	2025-06-24 20:00:00-04	54908	24	\N
643	9	2025-06-25 20:00:00-04	54976	68	\N
644	9	2025-06-26 20:00:00-04	55000	24	\N
645	9	2025-06-29 20:00:00-04	55045	45	\N
646	5	2025-06-01 20:00:00-04	35495	0	\N
647	5	2025-06-02 20:00:00-04	35794	299	\N
648	5	2025-06-03 20:00:00-04	36053	259	\N
649	5	2025-06-04 20:00:00-04	36378	325	\N
650	5	2025-06-05 20:00:00-04	36602	224	\N
651	5	2025-06-08 20:00:00-04	37180	578	\N
652	5	2025-06-09 20:00:00-04	37378	198	\N
653	5	2025-06-10 20:00:00-04	37569	191	\N
654	5	2025-06-11 20:00:00-04	37774	205	\N
655	5	2025-06-12 20:00:00-04	37871	97	\N
656	5	2025-06-15 20:00:00-04	38100	229	\N
657	5	2025-06-16 20:00:00-04	38356	256	\N
658	5	2025-06-17 20:00:00-04	38500	144	\N
659	5	2025-06-18 20:00:00-04	38698	198	\N
660	5	2025-06-19 20:00:00-04	38814	116	\N
661	5	2025-06-22 20:00:00-04	39297	483	\N
662	5	2025-06-24 20:00:00-04	39349	52	\N
663	5	2025-06-25 20:00:00-04	39569	220	\N
664	5	2025-06-26 20:00:00-04	39800	231	\N
665	5	2025-06-29 20:00:00-04	40182	382	\N
666	6	2025-06-01 20:00:00-04	26287	0	\N
667	6	2025-06-02 20:00:00-04	26377	90	\N
668	6	2025-06-03 20:00:00-04	26480	103	\N
669	6	2025-06-04 20:00:00-04	26512	32	\N
670	6	2025-06-05 20:00:00-04	26601	89	\N
671	6	2025-06-08 20:00:00-04	26681	80	\N
672	6	2025-06-09 20:00:00-04	26712	31	\N
673	6	2025-06-10 20:00:00-04	26799	87	\N
674	6	2025-06-11 20:00:00-04	26868	69	\N
675	6	2025-06-12 20:00:00-04	26960	92	\N
676	6	2025-06-15 20:00:00-04	26979	19	\N
677	6	2025-06-16 20:00:00-04	27064	85	\N
678	6	2025-06-17 20:00:00-04	27120	56	\N
679	6	2025-06-18 20:00:00-04	27137	17	\N
680	6	2025-06-19 20:00:00-04	27155	18	\N
681	6	2025-06-22 20:00:00-04	27212	57	\N
682	6	2025-06-24 20:00:00-04	27248	36	\N
683	6	2025-06-25 20:00:00-04	27311	63	\N
684	6	2025-06-26 20:00:00-04	27436	125	\N
685	6	2025-06-29 20:00:00-04	27555	119	\N
686	10	2025-06-01 20:00:00-04	16697	0	\N
687	10	2025-06-02 20:00:00-04	16875	178	\N
688	10	2025-06-03 20:00:00-04	17089	214	\N
689	10	2025-06-04 20:00:00-04	17119	30	\N
690	10	2025-06-05 20:00:00-04	17396	277	\N
691	10	2025-06-08 20:00:00-04	17840	444	\N
692	10	2025-06-09 20:00:00-04	17996	156	\N
693	10	2025-06-10 20:00:00-04	18356	360	\N
694	10	2025-06-11 20:00:00-04	18521	165	\N
695	10	2025-06-12 20:00:00-04	18747	226	\N
696	10	2025-06-15 20:00:00-04	18907	160	\N
697	10	2025-06-16 20:00:00-04	19265	358	\N
698	10	2025-06-17 20:00:00-04	19546	281	\N
699	10	2025-06-18 20:00:00-04	19622	76	\N
700	10	2025-06-19 20:00:00-04	19783	161	\N
701	10	2025-06-22 20:00:00-04	20147	364	\N
702	10	2025-06-24 20:00:00-04	20487	340	\N
703	10	2025-06-25 20:00:00-04	20756	269	\N
704	10	2025-06-26 20:00:00-04	21085	329	\N
705	10	2025-06-29 20:00:00-04	21357	272	\N
706	3	2025-07-31 20:00:00-04	135350	0	\N
707	3	2025-08-03 20:00:00-04	135528	178	\N
708	3	2025-08-04 20:00:00-04	135652	124	\N
709	3	2025-08-05 20:00:00-04	135708	56	CAMBIO TONER Y DRUM
710	3	2025-08-06 20:00:00-04	135809	101	\N
711	3	2025-08-07 20:00:00-04	135852	43	\N
712	3	2025-08-10 20:00:00-04	135878	26	\N
713	3	2025-08-11 20:00:00-04	135925	47	\N
714	3	2025-08-12 20:00:00-04	135989	64	\N
715	3	2025-08-13 20:00:00-04	136056	67	\N
716	3	2025-08-14 20:00:00-04	136142	86	\N
717	3	2025-08-17 20:00:00-04	136189	47	\N
718	3	2025-08-18 20:00:00-04	136218	29	\N
719	3	2025-08-19 20:00:00-04	136282	64	\N
720	3	2025-08-20 20:00:00-04	136300	18	\N
721	3	2025-08-21 20:00:00-04	136357	57	\N
722	3	2025-08-24 20:00:00-04	136443	86	\N
723	3	2025-08-25 20:00:00-04	136503	60	\N
724	3	2025-08-26 20:00:00-04	136547	44	\N
725	3	2025-08-27 20:00:00-04	136609	62	\N
726	3	2025-08-28 20:00:00-04	136661	52	\N
727	9	2025-07-31 20:00:00-04	56804	0	\N
728	9	2025-08-03 20:00:00-04	56819	15	\N
729	9	2025-08-04 20:00:00-04	56841	22	\N
730	9	2025-08-05 20:00:00-04	56883	42	\N
731	9	2025-08-06 20:00:00-04	56995	112	\N
732	9	2025-08-07 20:00:00-04	57030	35	\N
733	9	2025-08-10 20:00:00-04	57704	674	\N
735	9	2025-08-12 20:00:00-04	57199	44	\N
736	9	2025-08-13 20:00:00-04	57230	31	\N
737	9	2025-08-14 20:00:00-04	57307	77	\N
738	9	2025-08-17 20:00:00-04	57449	142	\N
739	9	2025-08-18 20:00:00-04	57542	93	\N
740	9	2025-08-19 20:00:00-04	57563	21	\N
741	9	2025-08-20 20:00:00-04	57612	49	\N
742	9	2025-08-21 20:00:00-04	57670	58	\N
743	9	2025-08-24 20:00:00-04	57721	51	\N
744	9	2025-08-25 20:00:00-04	57749	28	\N
745	9	2025-08-26 20:00:00-04	57792	43	\N
746	9	2025-08-27 20:00:00-04	57834	42	\N
747	9	2025-08-28 20:00:00-04	57852	18	\N
748	5	2025-07-31 20:00:00-04	45198	0	\N
749	5	2025-08-03 20:00:00-04	45383	185	\N
750	5	2025-08-04 20:00:00-04	45571	188	\N
751	5	2025-08-05 20:00:00-04	45715	144	\N
752	5	2025-08-06 20:00:00-04	45874	159	\N
753	5	2025-08-07 20:00:00-04	46154	280	\N
754	5	2025-08-10 20:00:00-04	46311	157	\N
755	5	2025-08-11 20:00:00-04	46497	186	\N
756	5	2025-08-12 20:00:00-04	46712	215	\N
757	5	2025-08-13 20:00:00-04	46961	249	\N
758	5	2025-08-14 20:00:00-04	47218	257	\N
759	5	2025-08-17 20:00:00-04	47518	300	\N
760	5	2025-08-18 20:00:00-04	47709	191	\N
761	5	2025-08-19 20:00:00-04	48132	423	\N
762	5	2025-08-20 20:00:00-04	48280	148	\N
763	5	2025-08-21 20:00:00-04	48550	270	\N
764	5	2025-08-24 20:00:00-04	48842	292	\N
765	5	2025-08-25 20:00:00-04	48976	134	\N
766	5	2025-08-26 20:00:00-04	49141	165	\N
767	5	2025-08-27 20:00:00-04	49371	230	\N
768	5	2025-08-28 20:00:00-04	49568	197	\N
769	6	2025-07-31 20:00:00-04	29851	0	\N
770	6	2025-08-03 20:00:00-04	29982	131	\N
771	6	2025-08-04 20:00:00-04	30002	20	\N
772	6	2025-08-05 20:00:00-04	30047	45	\N
773	6	2025-08-06 20:00:00-04	30186	139	\N
774	6	2025-08-07 20:00:00-04	30215	29	\N
775	6	2025-08-10 20:00:00-04	30532	317	\N
776	6	2025-08-11 20:00:00-04	30594	62	\N
777	6	2025-08-12 20:00:00-04	30651	57	\N
778	6	2025-08-13 20:00:00-04	30783	132	\N
779	6	2025-08-14 20:00:00-04	30961	178	\N
780	6	2025-08-17 20:00:00-04	30990	29	\N
781	6	2025-08-18 20:00:00-04	31000	10	CAMBIO TONER
782	6	2025-08-19 20:00:00-04	31114	114	\N
783	6	2025-08-20 20:00:00-04	31276	162	\N
784	6	2025-08-21 20:00:00-04	31336	60	\N
785	6	2025-08-24 20:00:00-04	31392	56	\N
786	6	2025-08-25 20:00:00-04	31414	22	\N
787	6	2025-08-26 20:00:00-04	31481	67	\N
788	6	2025-08-27 20:00:00-04	31586	105	\N
789	6	2025-08-28 20:00:00-04	31874	288	\N
790	10	2025-07-31 20:00:00-04	26603	0	\N
791	10	2025-08-03 20:00:00-04	26772	169	\N
792	10	2025-08-04 20:00:00-04	26925	153	\N
793	10	2025-08-05 20:00:00-04	27112	187	\N
794	10	2025-08-06 20:00:00-04	27308	196	\N
795	10	2025-08-07 20:00:00-04	27621	313	\N
796	10	2025-08-10 20:00:00-04	27670	49	\N
797	10	2025-08-11 20:00:00-04	27903	233	\N
798	10	2025-08-12 20:00:00-04	28147	244	\N
799	10	2025-08-13 20:00:00-04	28306	159	\N
800	10	2025-08-14 20:00:00-04	28513	207	\N
801	10	2025-08-17 20:00:00-04	28562	49	\N
802	10	2025-08-18 20:00:00-04	28815	253	\N
803	10	2025-08-19 20:00:00-04	29047	232	\N
804	10	2025-08-20 20:00:00-04	29217	170	\N
805	10	2025-08-21 20:00:00-04	29419	202	CAMBIO TONER
806	10	2025-08-24 20:00:00-04	29828	409	\N
807	10	2025-08-25 20:00:00-04	29979	151	\N
808	10	2025-08-26 20:00:00-04	30101	122	\N
809	10	2025-08-27 20:00:00-04	30347	246	\N
810	10	2025-08-28 20:00:00-04	30511	164	\N
811	3	2025-08-31 20:00:00-04	136747	0	\N
812	3	2025-09-01 20:00:00-04	136781	34	\N
813	3	2025-09-02 20:00:00-04	136824	43	\N
814	3	2025-09-03 20:00:00-04	136915	91	\N
815	3	2025-09-04 20:00:00-04	136959	44	\N
816	3	2025-09-07 20:00:00-04	136984	25	\N
817	3	2025-09-08 20:00:00-04	137028	44	\N
818	3	2025-09-09 20:00:00-04	137081	53	\N
819	3	2025-09-10 20:00:00-04	137153	72	CAMBIO TONER
820	3	2025-09-11 20:00:00-04	137193	40	\N
821	3	2025-09-14 20:00:00-04	137362	169	\N
822	3	2025-09-15 20:00:00-04	137456	94	MANTENIMIENTO EXYCOPY
823	3	2025-09-16 20:00:00-04	137532	76	\N
824	3	2025-09-17 20:00:00-04	137597	65	\N
825	3	2025-09-18 20:00:00-04	137665	68	\N
826	3	2025-09-21 20:00:00-04	137746	81	\N
827	3	2025-09-22 20:00:00-04	137847	101	\N
828	3	2025-09-23 20:00:00-04	137926	79	\N
829	3	2025-09-24 20:00:00-04	137998	72	\N
830	3	2025-09-25 20:00:00-04	138095	97	\N
831	3	2025-09-28 20:00:00-04	138122	27	\N
832	3	2025-09-29 20:00:00-04	138173	51	\N
833	9	2025-08-31 20:00:00-04	57896	0	\N
834	9	2025-09-01 20:00:00-04	57980	84	\N
835	9	2025-09-02 20:00:00-04	58001	21	\N
836	9	2025-09-03 20:00:00-04	58030	29	\N
837	9	2025-09-04 20:00:00-04	58115	85	\N
838	9	2025-09-07 20:00:00-04	58144	29	\N
839	9	2025-09-08 20:00:00-04	58319	175	\N
840	9	2025-09-09 20:00:00-04	58339	20	\N
841	9	2025-09-10 20:00:00-04	58358	19	\N
842	9	2025-09-11 20:00:00-04	58384	26	\N
843	9	2025-09-14 20:00:00-04	58412	28	\N
844	9	2025-09-15 20:00:00-04	58443	31	\N
845	9	2025-09-16 20:00:00-04	58474	31	\N
846	9	2025-09-17 20:00:00-04	58521	47	\N
847	9	2025-09-18 20:00:00-04	58571	50	\N
848	9	2025-09-21 20:00:00-04	58606	35	MANTENIMIENTO EXYCOPY - CAMBIO TONER Y DRUM
849	9	2025-09-22 20:00:00-04	58714	108	\N
850	9	2025-09-23 20:00:00-04	58762	48	\N
851	9	2025-09-24 20:00:00-04	58791	29	\N
852	9	2025-09-25 20:00:00-04	58844	53	\N
853	9	2025-09-28 20:00:00-04	58875	31	\N
854	9	2025-09-29 20:00:00-04	58901	26	\N
855	5	2025-08-31 20:00:00-04	49952	0	\N
856	5	2025-09-01 20:00:00-04	50201	249	\N
857	5	2025-09-02 20:00:00-04	50269	68	\N
858	5	2025-09-03 20:00:00-04	50396	127	\N
859	5	2025-09-04 20:00:00-04	50693	297	\N
860	5	2025-09-07 20:00:00-04	50781	88	\N
861	5	2025-09-08 20:00:00-04	51102	321	\N
862	5	2025-09-09 20:00:00-04	51196	94	\N
863	5	2025-09-10 20:00:00-04	51376	180	\N
864	5	2025-09-11 20:00:00-04	51619	243	\N
865	5	2025-09-14 20:00:00-04	51894	275	\N
866	5	2025-09-15 20:00:00-04	52027	133	\N
867	5	2025-09-16 20:00:00-04	52220	193	\N
868	5	2025-09-17 20:00:00-04	52333	113	\N
869	5	2025-09-18 20:00:00-04	52556	223	\N
870	5	2025-09-21 20:00:00-04	52921	365	CAMBIO TONER
871	5	2025-09-22 20:00:00-04	53477	556	\N
872	5	2025-09-23 20:00:00-04	53636	159	\N
873	5	2025-09-24 20:00:00-04	53762	126	\N
874	5	2025-09-25 20:00:00-04	53967	205	\N
875	5	2025-09-28 20:00:00-04	54232	265	\N
876	5	2025-09-29 20:00:00-04	54413	181	\N
877	11	2025-08-31 20:00:00-04	31917	0	RRHH
878	11	2025-09-01 20:00:00-04	31985	68	CAMBIO TONER
879	11	2025-09-02 20:00:00-04	32041	56	RRHH
880	11	2025-09-03 20:00:00-04	32045	4	RRHH
881	11	2025-09-04 20:00:00-04	32602	557	RRHH
882	11	2025-09-07 20:00:00-04	32672	70	RRHH
883	11	2025-09-08 20:00:00-04	32956	284	RRHH
884	11	2025-09-09 20:00:00-04	32956	0	CAMBIO DEPARTAMENTO Comercial OV-JD
885	11	2025-09-10 20:00:00-04	32956	0	\N
886	11	2025-09-11 20:00:00-04	32956	0	\N
887	11	2025-09-14 20:00:00-04	32956	0	CAMBIO TONER
888	11	2025-09-15 20:00:00-04	33037	81	MANTENIMIENTO EXYCOPY
889	11	2025-09-16 20:00:00-04	33104	67	\N
890	11	2025-09-17 20:00:00-04	33143	39	\N
891	11	2025-09-18 20:00:00-04	33175	32	\N
892	11	2025-09-21 20:00:00-04	33181	6	\N
893	11	2025-09-22 20:00:00-04	33204	23	\N
894	11	2025-09-23 20:00:00-04	33208	4	\N
895	11	2025-09-24 20:00:00-04	33296	88	\N
896	11	2025-09-25 20:00:00-04	33296	0	\N
897	11	2025-09-28 20:00:00-04	33296	0	\N
898	11	2025-09-29 20:00:00-04	33296	0	\N
899	10	2025-08-31 20:00:00-04	30749	0	\N
900	10	2025-09-01 20:00:00-04	30979	230	\N
901	10	2025-09-02 20:00:00-04	31087	108	\N
902	10	2025-09-03 20:00:00-04	31242	155	\N
903	10	2025-09-04 20:00:00-04	31482	240	\N
904	10	2025-09-07 20:00:00-04	31514	32	\N
905	10	2025-09-08 20:00:00-04	31957	443	\N
906	10	2025-09-09 20:00:00-04	32167	210	\N
907	10	2025-09-10 20:00:00-04	32321	154	\N
908	10	2025-09-11 20:00:00-04	32531	210	\N
909	10	2025-09-14 20:00:00-04	32688	157	\N
910	10	2025-09-15 20:00:00-04	32993	305	\N
911	10	2025-09-16 20:00:00-04	33204	211	\N
912	10	2025-09-17 20:00:00-04	33510	306	\N
913	10	2025-09-18 20:00:00-04	33658	148	\N
914	10	2025-09-21 20:00:00-04	33935	277	\N
915	10	2025-09-22 20:00:00-04	34357	422	\N
916	10	2025-09-23 20:00:00-04	34545	188	\N
917	10	2025-09-24 20:00:00-04	34731	186	\N
918	10	2025-09-25 20:00:00-04	34971	240	\N
919	10	2025-09-28 20:00:00-04	35034	63	\N
920	10	2025-09-29 20:00:00-04	35237	203	\N
921	3	2025-09-30 20:00:00-04	138209	0	\N
922	3	2025-10-01 20:00:00-04	138268	59	\N
923	3	2025-10-02 20:00:00-04	138306	38	\N
924	3	2025-10-05 20:00:00-04	138355	49	\N
925	3	2025-10-06 20:00:00-04	138373	18	\N
926	3	2025-10-07 20:00:00-04	138418	45	\N
927	3	2025-10-08 20:00:00-04	138507	89	\N
928	3	2025-10-09 20:00:00-04	138579	72	\N
929	3	2025-10-12 20:00:00-04	138647	68	\N
930	3	2025-10-13 20:00:00-04	138738	91	\N
931	3	2025-10-14 20:00:00-04	138799	61	\N
932	3	2025-10-15 20:00:00-04	138821	22	\N
933	3	2025-10-16 20:00:00-04	138954	133	\N
934	3	2025-10-19 20:00:00-04	139021	67	CAMBIO TONER
935	3	2025-10-20 20:00:00-04	139164	143	\N
936	3	2025-10-21 20:00:00-04	139260	96	\N
937	3	2025-10-22 20:00:00-04	139301	41	\N
938	3	2025-10-23 20:00:00-04	139369	68	\N
939	3	2025-10-26 20:00:00-04	139463	94	\N
940	3	2025-10-27 20:00:00-04	139513	50	\N
941	3	2025-10-28 20:00:00-04	139554	41	\N
942	3	2025-10-29 20:00:00-04	139586	32	\N
943	3	2025-10-30 20:00:00-04	139606	20	\N
944	9	2025-09-30 20:00:00-04	58942	0	\N
945	9	2025-10-01 20:00:00-04	58972	30	\N
946	9	2025-10-02 20:00:00-04	58992	20	\N
947	9	2025-10-05 20:00:00-04	59031	39	\N
948	9	2025-10-06 20:00:00-04	59081	50	\N
949	9	2025-10-07 20:00:00-04	59109	28	\N
950	9	2025-10-08 20:00:00-04	59138	29	\N
951	9	2025-10-09 20:00:00-04	59194	56	\N
952	9	2025-10-12 20:00:00-04	59215	21	\N
953	9	2025-10-13 20:00:00-04	59252	37	\N
954	9	2025-10-14 20:00:00-04	59348	96	\N
955	9	2025-10-15 20:00:00-04	59475	127	\N
956	9	2025-10-16 20:00:00-04	59613	138	\N
957	9	2025-10-19 20:00:00-04	59671	58	\N
958	9	2025-10-20 20:00:00-04	59721	50	\N
959	9	2025-10-21 20:00:00-04	59782	61	\N
960	9	2025-10-22 20:00:00-04	59837	55	\N
961	9	2025-10-23 20:00:00-04	59979	142	\N
962	9	2025-10-26 20:00:00-04	60210	231	\N
963	9	2025-10-27 20:00:00-04	60250	40	\N
964	9	2025-10-28 20:00:00-04	60289	39	\N
965	9	2025-10-29 20:00:00-04	60455	166	\N
966	9	2025-10-30 20:00:00-04	60523	68	\N
967	5	2025-09-30 20:00:00-04	54586	0	\N
968	5	2025-10-01 20:00:00-04	54701	115	\N
969	5	2025-10-02 20:00:00-04	54888	187	\N
970	5	2025-10-05 20:00:00-04	55185	297	\N
971	5	2025-10-06 20:00:00-04	55306	121	\N
972	5	2025-10-07 20:00:00-04	55649	343	\N
973	5	2025-10-08 20:00:00-04	55938	289	\N
974	5	2025-10-09 20:00:00-04	56194	256	\N
975	5	2025-10-12 20:00:00-04	56310	116	\N
976	5	2025-10-13 20:00:00-04	56561	251	\N
977	5	2025-10-14 20:00:00-04	56721	160	\N
978	5	2025-10-15 20:00:00-04	56856	135	\N
979	5	2025-10-16 20:00:00-04	57095	239	\N
980	5	2025-10-19 20:00:00-04	57224	129	\N
981	5	2025-10-20 20:00:00-04	57482	258	\N
982	5	2025-10-21 20:00:00-04	57648	166	\N
983	5	2025-10-22 20:00:00-04	57857	209	\N
984	5	2025-10-23 20:00:00-04	58089	232	\N
985	5	2025-10-26 20:00:00-04	58619	530	\N
986	5	2025-10-27 20:00:00-04	59026	407	\N
987	5	2025-10-28 20:00:00-04	59197	171	\N
988	5	2025-10-29 20:00:00-04	59458	261	\N
989	5	2025-10-30 20:00:00-04	59661	203	\N
990	11	2025-09-30 20:00:00-04	33215	0	\N
991	11	2025-10-01 20:00:00-04	33230	15	\N
992	11	2025-10-02 20:00:00-04	33349	119	\N
993	11	2025-10-05 20:00:00-04	33350	1	\N
994	11	2025-10-06 20:00:00-04	33366	16	\N
995	11	2025-10-07 20:00:00-04	33371	5	\N
996	11	2025-10-08 20:00:00-04	33383	12	\N
997	11	2025-10-09 20:00:00-04	33387	4	\N
998	11	2025-10-12 20:00:00-04	33389	2	\N
999	11	2025-10-13 20:00:00-04	33407	18	\N
1000	11	2025-10-14 20:00:00-04	33412	5	\N
1001	11	2025-10-15 20:00:00-04	33430	18	\N
1002	11	2025-10-16 20:00:00-04	33436	6	\N
1003	11	2025-10-19 20:00:00-04	33446	10	\N
1004	11	2025-10-20 20:00:00-04	33446	0	\N
1005	11	2025-10-21 20:00:00-04	33446	0	\N
1006	11	2025-10-22 20:00:00-04	33449	3	\N
1007	11	2025-10-23 20:00:00-04	33462	13	\N
1008	11	2025-10-26 20:00:00-04	33469	7	\N
1009	11	2025-10-27 20:00:00-04	33473	4	\N
1010	11	2025-10-28 20:00:00-04	33489	16	\N
1011	11	2025-10-29 20:00:00-04	33489	0	\N
1012	11	2025-10-30 20:00:00-04	33510	21	\N
1013	10	2025-09-30 20:00:00-04	35540	0	\N
1014	10	2025-10-01 20:00:00-04	35702	162	\N
1015	10	2025-10-02 20:00:00-04	35992	290	\N
1016	10	2025-10-05 20:00:00-04	36174	182	\N
1017	10	2025-10-06 20:00:00-04	36352	178	\N
1018	10	2025-10-07 20:00:00-04	36688	336	\N
1019	10	2025-10-08 20:00:00-04	36901	213	\N
1020	10	2025-10-09 20:00:00-04	37111	210	\N
1021	10	2025-10-12 20:00:00-04	37281	170	\N
1022	10	2025-10-13 20:00:00-04	37616	335	\N
1023	10	2025-10-14 20:00:00-04	37811	195	\N
1024	10	2025-10-15 20:00:00-04	38026	215	\N
1025	10	2025-10-16 20:00:00-04	38355	329	\N
1026	10	2025-10-19 20:00:00-04	38514	159	\N
1027	10	2025-10-20 20:00:00-04	38731	217	\N
1028	10	2025-10-21 20:00:00-04	39038	307	\N
1029	10	2025-10-22 20:00:00-04	39230	192	CAMBIO TONER
1030	10	2025-10-23 20:00:00-04	39364	134	\N
1031	10	2025-10-26 20:00:00-04	39607	243	\N
1032	10	2025-10-27 20:00:00-04	39854	247	\N
1033	10	2025-10-28 20:00:00-04	40053	199	\N
1034	10	2025-10-29 20:00:00-04	40248	195	\N
1035	10	2025-10-30 20:00:00-04	40585	337	\N
1036	3	2025-11-02 20:00:00-04	139693	0	\N
1037	3	2025-11-03 20:00:00-04	139722	29	\N
1038	3	2025-11-04 20:00:00-04	139796	74	\N
1039	3	2025-11-05 20:00:00-04	139820	24	\N
1040	3	2025-11-06 20:00:00-04	139859	39	\N
1041	3	2025-11-09 20:00:00-04	139941	82	\N
1042	3	2025-11-10 20:00:00-04	139994	53	\N
1043	3	2025-11-11 20:00:00-04	140086	92	\N
1044	3	2025-11-12 20:00:00-04	140161	75	\N
1045	3	2025-11-13 20:00:00-04	140274	113	\N
1046	3	2025-11-16 20:00:00-04	140485	66	\N
1047	3	2025-11-17 20:00:00-04	140568	83	\N
1048	3	2025-11-18 20:00:00-04	140633	65	CAMBIO TONER
1049	3	2025-11-23 20:00:00-04	140793	160	\N
1050	3	2025-11-24 20:00:00-04	140879	86	\N
1051	3	2025-11-25 20:00:00-04	141004	125	\N
1052	3	2025-11-26 20:00:00-04	141081	77	\N
1053	3	2025-11-27 20:00:00-04	141182	101	\N
1054	9	2025-11-02 20:00:00-04	60549	0	\N
1055	9	2025-11-03 20:00:00-04	60647	98	\N
1056	9	2025-11-04 20:00:00-04	60697	50	\N
1057	9	2025-11-05 20:00:00-04	60859	162	\N
1058	9	2025-11-06 20:00:00-04	61006	147	\N
1059	9	2025-11-09 20:00:00-04	61052	46	\N
1060	9	2025-11-10 20:00:00-04	61080	28	\N
1061	9	2025-11-11 20:00:00-04	61116	36	\N
1062	9	2025-11-12 20:00:00-04	61222	106	\N
1063	9	2025-11-13 20:00:00-04	61247	25	\N
1064	9	2025-11-16 20:00:00-04	61331	30	\N
1065	9	2025-11-17 20:00:00-04	61364	33	\N
1066	9	2025-11-18 20:00:00-04	61392	28	\N
1067	9	2025-11-23 20:00:00-04	61448	56	\N
1068	9	2025-11-24 20:00:00-04	61536	88	\N
1069	9	2025-11-25 20:00:00-04	61555	19	\N
1070	9	2025-11-26 20:00:00-04	61594	39	\N
1071	9	2025-11-27 20:00:00-04	61689	95	\N
1072	5	2025-11-02 20:00:00-04	59955	0	\N
1073	5	2025-11-03 20:00:00-04	60082	127	\N
1074	5	2025-11-04 20:00:00-04	60228	146	\N
1075	5	2025-11-05 20:00:00-04	60321	93	\N
1076	5	2025-11-06 20:00:00-04	60483	162	\N
1077	5	2025-11-09 20:00:00-04	60763	280	\N
1078	5	2025-11-10 20:00:00-04	60862	99	\N
1079	5	2025-11-11 20:00:00-04	60970	108	\N
1080	5	2025-11-12 20:00:00-04	61085	115	\N
1081	5	2025-11-13 20:00:00-04	61110	25	\N
1082	5	2025-11-16 20:00:00-04	61801	168	\N
1083	5	2025-11-17 20:00:00-04	62053	252	\N
1084	5	2025-11-18 20:00:00-04	62305	252	\N
1085	5	2025-11-23 20:00:00-04	62773	468	\N
1086	5	2025-11-24 20:00:00-04	63000	227	\N
1087	5	2025-11-25 20:00:00-04	63206	206	\N
1088	5	2025-11-26 20:00:00-04	63799	593	\N
1089	5	2025-11-27 20:00:00-04	64209	410	\N
1090	11	2025-11-02 20:00:00-04	33511	0	\N
1091	11	2025-11-03 20:00:00-04	33516	5	\N
1092	11	2025-11-04 20:00:00-04	33525	9	\N
1093	11	2025-11-05 20:00:00-04	33530	5	\N
1094	11	2025-11-06 20:00:00-04	33535	5	\N
1095	11	2025-11-09 20:00:00-04	33571	36	\N
1096	11	2025-11-10 20:00:00-04	33571	0	\N
1097	11	2025-11-11 20:00:00-04	33573	2	\N
1098	11	2025-11-12 20:00:00-04	33573	0	\N
1099	11	2025-11-13 20:00:00-04	33580	7	\N
1100	11	2025-11-16 20:00:00-04	33597	5	\N
1101	11	2025-11-17 20:00:00-04	33607	10	\N
1102	11	2025-11-18 20:00:00-04	33610	3	\N
1103	11	2025-11-23 20:00:00-04	33620	10	\N
1104	11	2025-11-24 20:00:00-04	33625	5	\N
1105	11	2025-11-25 20:00:00-04	33632	7	\N
1106	11	2025-11-26 20:00:00-04	33633	1	\N
1107	11	2025-11-27 20:00:00-04	33638	5	\N
1108	10	2025-11-02 20:00:00-04	40804	0	\N
1109	10	2025-11-03 20:00:00-04	41096	292	\N
1110	10	2025-11-04 20:00:00-04	41399	303	\N
1111	10	2025-11-05 20:00:00-04	41636	237	\N
1112	10	2025-11-06 20:00:00-04	41903	267	\N
1113	10	2025-11-09 20:00:00-04	42105	202	\N
1114	10	2025-11-10 20:00:00-04	42285	180	\N
1115	10	2025-11-11 20:00:00-04	42374	89	\N
1116	10	2025-11-12 20:00:00-04	42627	253	\N
1117	10	2025-11-13 20:00:00-04	42912	285	\N
1118	10	2025-11-16 20:00:00-04	43570	230	\N
1119	10	2025-11-17 20:00:00-04	43763	193	\N
1120	10	2025-11-18 20:00:00-04	43910	147	\N
1121	10	2025-11-23 20:00:00-04	44234	324	\N
1122	10	2025-11-24 20:00:00-04	44512	278	\N
1123	10	2025-11-25 20:00:00-04	44660	148	\N
1124	10	2025-11-26 20:00:00-04	44916	256	\N
1125	10	2025-11-27 20:00:00-04	45017	101	\N
1126	3	2025-11-30 20:00:00-04	141269	0	\N
1127	3	2025-12-01 20:00:00-04	141340	71	\N
1128	3	2025-12-02 20:00:00-04	141434	94	\N
1129	3	2025-12-03 20:00:00-04	141563	129	\N
1130	3	2025-12-04 20:00:00-04	141628	65	\N
1131	3	2025-12-07 20:00:00-04	141825	197	\N
1132	3	2025-12-08 20:00:00-04	141966	141	\N
1133	3	2025-12-09 20:00:00-04	142092	126	\N
1134	3	2025-12-10 20:00:00-04	142170	78	\N
1135	3	2025-12-11 20:00:00-04	142221	51	\N
1136	3	2025-12-14 20:00:00-04	142333	112	\N
1137	3	2025-12-15 20:00:00-04	142459	126	\N
1138	3	2025-12-16 20:00:00-04	142502	43	\N
1139	3	2025-12-17 20:00:00-04	142537	35	\N
1140	3	2025-12-18 20:00:00-04	142578	41	\N
1141	3	2025-12-21 20:00:00-04	142623	45	\N
1142	3	2025-12-22 20:00:00-04	142725	102	\N
1143	3	2025-12-23 20:00:00-04	142725	0	FERIADO
1144	3	2025-12-24 20:00:00-04	142725	0	FERIADO
1145	3	2025-12-25 20:00:00-04	142778	53	\N
1146	3	2025-12-28 20:00:00-04	142858	80	CAMBIO DRUM
1147	3	2025-12-29 20:00:00-04	142914	56	\N
1148	3	2025-12-30 20:00:00-04	142968	54	FERIADO
1149	9	2025-11-30 20:00:00-04	61718	0	\N
1150	9	2025-12-01 20:00:00-04	61739	21	\N
1151	9	2025-12-02 20:00:00-04	61881	142	\N
1152	9	2025-12-03 20:00:00-04	61904	23	\N
1153	9	2025-12-04 20:00:00-04	61987	83	\N
1154	9	2025-12-07 20:00:00-04	62121	134	\N
1155	9	2025-12-08 20:00:00-04	62174	53	\N
1156	9	2025-12-09 20:00:00-04	62221	47	\N
1157	9	2025-12-10 20:00:00-04	62335	114	\N
1158	9	2025-12-11 20:00:00-04	62397	62	\N
1159	9	2025-12-14 20:00:00-04	62449	52	\N
1160	9	2025-12-15 20:00:00-04	62567	118	\N
1161	9	2025-12-16 20:00:00-04	62593	26	\N
1162	9	2025-12-17 20:00:00-04	62703	110	\N
1163	9	2025-12-18 20:00:00-04	62731	28	\N
1164	9	2025-12-21 20:00:00-04	62766	35	\N
1165	9	2025-12-22 20:00:00-04	62872	106	\N
1166	9	2025-12-23 20:00:00-04	62872	0	FERIADO
1167	9	2025-12-24 20:00:00-04	62872	0	FERIADO
1168	9	2025-12-25 20:00:00-04	62997	125	\N
1169	9	2025-12-28 20:00:00-04	63208	211	\N
1170	9	2025-12-29 20:00:00-04	63226	18	\N
1171	9	2025-12-30 20:00:00-04	63239	13	FERIADO
1172	5	2025-11-30 20:00:00-04	64421	0	\N
1173	5	2025-12-01 20:00:00-04	64816	395	\N
1174	5	2025-12-02 20:00:00-04	65563	747	\N
1175	5	2025-12-03 20:00:00-04	65643	80	\N
1176	5	2025-12-04 20:00:00-04	65759	116	\N
1177	5	2025-12-07 20:00:00-04	66522	763	\N
1178	5	2025-12-08 20:00:00-04	66659	137	\N
1179	5	2025-12-09 20:00:00-04	67126	467	\N
1180	5	2025-12-10 20:00:00-04	67408	282	\N
1181	5	2025-12-11 20:00:00-04	67809	401	\N
1182	5	2025-12-14 20:00:00-04	68098	289	\N
1183	5	2025-12-15 20:00:00-04	68440	342	\N
1184	5	2025-12-16 20:00:00-04	68745	305	\N
1185	5	2025-12-17 20:00:00-04	68885	140	\N
1186	5	2025-12-18 20:00:00-04	68974	89	\N
1187	5	2025-12-21 20:00:00-04	69218	244	\N
1188	5	2025-12-22 20:00:00-04	69620	402	\N
1189	5	2025-12-23 20:00:00-04	69620	0	FERIADO
1190	5	2025-12-24 20:00:00-04	69620	0	FERIADO
1191	5	2025-12-25 20:00:00-04	69738	118	\N
1192	5	2025-12-28 20:00:00-04	69925	187	\N
1193	5	2025-12-29 20:00:00-04	70016	91	\N
1194	5	2025-12-30 20:00:00-04	70099	83	FERIADO
1195	11	2025-11-30 20:00:00-04	33644	0	\N
1196	11	2025-12-01 20:00:00-04	33650	6	\N
1197	11	2025-12-02 20:00:00-04	33656	6	\N
1198	11	2025-12-03 20:00:00-04	33693	37	\N
1199	11	2025-12-04 20:00:00-04	33693	0	\N
1200	11	2025-12-07 20:00:00-04	33695	2	\N
1201	11	2025-12-08 20:00:00-04	33702	7	\N
1202	11	2025-12-09 20:00:00-04	33709	7	\N
1203	11	2025-12-10 20:00:00-04	33714	5	\N
1204	11	2025-12-11 20:00:00-04	33851	137	\N
1205	11	2025-12-14 20:00:00-04	33922	71	\N
1206	11	2025-12-15 20:00:00-04	34092	170	\N
1207	11	2025-12-16 20:00:00-04	34096	4	\N
1208	11	2025-12-17 20:00:00-04	34097	1	\N
1209	11	2025-12-18 20:00:00-04	34105	8	CAMBIO TONER
1210	11	2025-12-21 20:00:00-04	34111	6	\N
1211	11	2025-12-22 20:00:00-04	34113	2	\N
1212	11	2025-12-23 20:00:00-04	34113	0	FERIADO
1213	11	2025-12-24 20:00:00-04	34113	0	FERIADO
1214	11	2025-12-25 20:00:00-04	34119	6	\N
1215	11	2025-12-28 20:00:00-04	34119	0	\N
1216	11	2025-12-29 20:00:00-04	34119	0	\N
1217	11	2025-12-30 20:00:00-04	34127	8	FERIADO
1218	10	2025-11-30 20:00:00-04	45230	0	\N
1219	10	2025-12-01 20:00:00-04	45460	230	\N
1220	10	2025-12-02 20:00:00-04	45881	421	\N
1221	10	2025-12-03 20:00:00-04	45981	100	\N
1222	10	2025-12-04 20:00:00-04	46090	109	\N
1223	10	2025-12-07 20:00:00-04	46250	160	\N
1224	10	2025-12-08 20:00:00-04	46474	224	\N
1225	10	2025-12-09 20:00:00-04	46696	222	\N
1226	10	2025-12-10 20:00:00-04	46904	208	EN REPARACION
1227	10	2025-12-11 20:00:00-04	46904	0	EN REPARACION
1228	10	2025-12-14 20:00:00-04	48391	1487	EN REPARACION
1229	10	2025-12-15 20:00:00-04	48410	19	\N
1230	10	2025-12-16 20:00:00-04	48605	195	\N
1231	10	2025-12-17 20:00:00-04	48848	243	\N
1232	10	2025-12-18 20:00:00-04	49007	159	\N
1233	10	2025-12-21 20:00:00-04	49114	107	\N
1234	10	2025-12-22 20:00:00-04	49379	265	\N
1235	10	2025-12-23 20:00:00-04	49379	0	FERIADO
1236	10	2025-12-24 20:00:00-04	49379	0	FERIADO
1237	10	2025-12-25 20:00:00-04	49546	167	\N
1238	10	2025-12-28 20:00:00-04	49620	74	\N
1239	10	2025-12-29 20:00:00-04	49786	166	\N
1240	10	2025-12-30 20:00:00-04	49924	138	FERIADO
1241	3	2026-01-04 20:00:00-04	143017	0	\N
1242	3	2026-01-05 20:00:00-04	143073	56	\N
1243	3	2026-01-06 20:00:00-04	143121	48	\N
1244	3	2026-01-07 20:00:00-04	143192	71	\N
1245	3	2026-01-08 20:00:00-04	143233	41	\N
1246	3	2026-01-11 20:00:00-04	143332	99	CAMBIO TONER
1247	3	2026-01-12 20:00:00-04	143441	109	\N
1248	3	2026-01-13 20:00:00-04	143589	148	\N
1249	3	2026-01-14 20:00:00-04	143589	0	DAÑADA
1250	3	2026-01-15 20:00:00-04	143589	0	DAÑADA
1251	3	2026-01-18 20:00:00-04	143589	0	DAÑADA
1252	3	2026-01-19 20:00:00-04	143589	0	DAÑADA
1253	3	2026-01-20 20:00:00-04	143589	0	DAÑADA
1254	3	2026-01-21 20:00:00-04	143589	0	DAÑADA
1255	3	2026-01-22 20:00:00-04	143589	0	DAÑADA
1256	3	2026-01-25 20:00:00-04	143589	0	DAÑADA
1257	3	2026-01-26 20:00:00-04	143613	24	INSTALADA TONER NUEVO
1258	3	2026-01-27 20:00:00-04	143704	91	\N
1259	3	2026-01-28 20:00:00-04	143748	44	\N
1260	3	2026-01-29 20:00:00-04	143814	66	\N
1261	9	2026-01-04 20:00:00-04	63270	0	\N
1262	9	2026-01-05 20:00:00-04	63301	31	\N
1263	9	2026-01-06 20:00:00-04	63323	22	\N
1264	9	2026-01-07 20:00:00-04	63411	88	\N
1265	9	2026-01-08 20:00:00-04	63538	127	\N
1266	9	2026-01-11 20:00:00-04	63637	99	\N
1267	9	2026-01-12 20:00:00-04	64003	366	\N
1268	9	2026-01-13 20:00:00-04	64202	199	\N
1269	9	2026-01-14 20:00:00-04	64420	218	\N
1270	9	2026-01-15 20:00:00-04	64461	41	\N
1271	9	2026-01-18 20:00:00-04	64712	251	\N
1272	9	2026-01-19 20:00:00-04	64784	72	\N
1273	9	2026-01-20 20:00:00-04	64797	13	\N
1274	9	2026-01-21 20:00:00-04	65188	391	\N
1275	9	2026-01-22 20:00:00-04	65235	47	\N
1276	9	2026-01-25 20:00:00-04	65458	223	\N
1277	9	2026-01-26 20:00:00-04	65716	258	\N
1278	9	2026-01-27 20:00:00-04	65813	97	\N
1279	9	2026-01-28 20:00:00-04	65949	136	\N
1280	9	2026-01-29 20:00:00-04	66032	83	\N
1281	5	2026-01-04 20:00:00-04	70190	0	\N
1282	5	2026-01-05 20:00:00-04	70233	43	\N
1283	5	2026-01-06 20:00:00-04	70277	44	\N
1284	5	2026-01-07 20:00:00-04	70367	90	\N
1285	5	2026-01-08 20:00:00-04	70599	232	\N
1286	5	2026-01-11 20:00:00-04	70990	391	\N
1287	5	2026-01-12 20:00:00-04	71557	567	\N
1288	5	2026-01-13 20:00:00-04	71950	393	\N
1289	5	2026-01-14 20:00:00-04	72104	154	\N
1290	5	2026-01-15 20:00:00-04	72253	149	\N
1291	5	2026-01-18 20:00:00-04	72432	179	\N
1292	5	2026-01-19 20:00:00-04	72755	323	\N
1293	5	2026-01-20 20:00:00-04	73081	326	\N
1294	5	2026-01-21 20:00:00-04	73452	371	\N
1295	5	2026-01-22 20:00:00-04	73910	458	\N
1296	5	2026-01-25 20:00:00-04	74592	682	\N
1297	5	2026-01-26 20:00:00-04	74800	208	\N
1298	5	2026-01-27 20:00:00-04	74908	108	\N
1299	5	2026-01-28 20:00:00-04	75389	481	\N
1300	5	2026-01-29 20:00:00-04	75709	320	\N
1301	11	2026-01-04 20:00:00-04	34138	0	\N
1302	11	2026-01-05 20:00:00-04	34146	8	\N
1303	11	2026-01-06 20:00:00-04	34147	1	\N
1304	11	2026-01-07 20:00:00-04	34151	4	\N
1305	11	2026-01-08 20:00:00-04	34155	4	\N
1306	11	2026-01-11 20:00:00-04	34155	0	\N
1307	11	2026-01-12 20:00:00-04	34156	1	\N
1308	11	2026-01-13 20:00:00-04	34165	9	\N
1309	11	2026-01-14 20:00:00-04	34169	4	\N
1310	11	2026-01-15 20:00:00-04	34170	1	\N
1311	11	2026-01-18 20:00:00-04	34170	0	\N
1312	11	2026-01-19 20:00:00-04	34177	7	\N
1313	11	2026-01-20 20:00:00-04	34194	17	\N
1314	11	2026-01-21 20:00:00-04	34215	21	\N
1315	11	2026-01-22 20:00:00-04	34219	4	\N
1316	11	2026-01-25 20:00:00-04	34219	0	\N
1317	11	2026-01-26 20:00:00-04	34238	19	\N
1318	11	2026-01-27 20:00:00-04	34275	37	\N
1319	11	2026-01-28 20:00:00-04	34279	4	\N
1320	11	2026-01-29 20:00:00-04	34285	6	\N
1321	10	2026-01-04 20:00:00-04	50089	0	\N
1322	10	2026-01-05 20:00:00-04	50222	133	\N
1323	10	2026-01-06 20:00:00-04	50351	129	\N
1324	10	2026-01-07 20:00:00-04	50439	88	\N
1325	10	2026-01-08 20:00:00-04	50542	103	CAMBIO TONER
1326	10	2026-01-11 20:00:00-04	50687	145	\N
1327	10	2026-01-12 20:00:00-04	50975	288	\N
1328	10	2026-01-13 20:00:00-04	51167	192	\N
1329	10	2026-01-14 20:00:00-04	51361	194	\N
1330	10	2026-01-15 20:00:00-04	51574	213	\N
1331	10	2026-01-18 20:00:00-04	51682	108	\N
1332	10	2026-01-19 20:00:00-04	51869	187	\N
1333	10	2026-01-20 20:00:00-04	52151	282	\N
1334	10	2026-01-21 20:00:00-04	52299	148	\N
1335	10	2026-01-22 20:00:00-04	52416	117	\N
1336	10	2026-01-25 20:00:00-04	52629	213	\N
1337	10	2026-01-26 20:00:00-04	52818	189	\N
1338	10	2026-01-27 20:00:00-04	52945	127	\N
1339	10	2026-01-28 20:00:00-04	53050	105	\N
1340	10	2026-01-29 20:00:00-04	53255	205	\N
1341	3	2026-02-01 20:00:00-04	143873	0	\N
1342	3	2026-02-02 20:00:00-04	143986	113	\N
1343	3	2026-02-03 20:00:00-04	144077	91	\N
1344	3	2026-02-04 20:00:00-04	144156	79	\N
1345	3	2026-02-05 20:00:00-04	144230	74	\N
1346	3	2026-02-08 20:00:00-04	144318	88	\N
1347	3	2026-02-09 20:00:00-04	144430	112	\N
1348	3	2026-02-10 20:00:00-04	144496	66	\N
1349	3	2026-02-11 20:00:00-04	144599	103	\N
1350	3	2026-02-12 20:00:00-04	144702	103	\N
1351	3	2026-02-15 20:00:00-04	144702	0	FERIADO
1352	3	2026-02-16 20:00:00-04	144702	0	FERIADO
1353	3	2026-02-17 20:00:00-04	144814	112	\N
1354	3	2026-02-18 20:00:00-04	144895	81	CAMBIO TONER
1355	3	2026-02-19 20:00:00-04	145036	141	\N
1356	3	2026-02-22 20:00:00-04	145192	156	\N
1357	3	2026-02-23 20:00:00-04	145367	175	\N
1358	3	2026-02-24 20:00:00-04	145525	158	\N
1359	3	2026-02-25 20:00:00-04	145652	127	\N
1360	3	2026-02-26 20:00:00-04	145788	136	\N
1361	9	2026-02-01 20:00:00-04	66132	0	\N
1362	9	2026-02-02 20:00:00-04	66329	197	\N
1363	9	2026-02-03 20:00:00-04	66351	22	\N
1364	9	2026-02-04 20:00:00-04	66372	21	\N
1365	9	2026-02-05 20:00:00-04	66633	261	\N
1366	9	2026-02-08 20:00:00-04	66751	118	\N
1367	9	2026-02-09 20:00:00-04	66974	223	\N
1368	9	2026-02-10 20:00:00-04	67179	205	CAMBIO TONER
1369	9	2026-02-11 20:00:00-04	67232	53	\N
1370	9	2026-02-12 20:00:00-04	67336	104	\N
1371	9	2026-02-15 20:00:00-04	67336	0	FERIADO
1372	9	2026-02-16 20:00:00-04	67336	0	FERIADO
1373	9	2026-02-17 20:00:00-04	67473	137	\N
1374	9	2026-02-18 20:00:00-04	67646	173	\N
1375	9	2026-02-19 20:00:00-04	67744	98	\N
1376	9	2026-02-22 20:00:00-04	67897	153	\N
1377	9	2026-02-23 20:00:00-04	68026	129	\N
1378	9	2026-02-24 20:00:00-04	68083	57	\N
1379	9	2026-02-25 20:00:00-04	68138	55	\N
1380	9	2026-02-26 20:00:00-04	68228	90	\N
1381	5	2026-02-01 20:00:00-04	76318	0	\N
1382	5	2026-02-02 20:00:00-04	76566	248	\N
1383	5	2026-02-03 20:00:00-04	76698	132	\N
1384	5	2026-02-04 20:00:00-04	76838	140	\N
1385	5	2026-02-05 20:00:00-04	77202	364	\N
1386	5	2026-02-08 20:00:00-04	77655	453	\N
1387	5	2026-02-09 20:00:00-04	78376	721	\N
1388	5	2026-02-10 20:00:00-04	78822	446	\N
1389	5	2026-02-11 20:00:00-04	79129	307	\N
1390	5	2026-02-12 20:00:00-04	79664	535	\N
1391	5	2026-02-15 20:00:00-04	79664	0	FERIADO
1392	5	2026-02-16 20:00:00-04	79664	0	FERIADO
1393	5	2026-02-17 20:00:00-04	80034	370	\N
1394	5	2026-02-18 20:00:00-04	80334	300	\N
1395	5	2026-02-19 20:00:00-04	80662	328	CAMBIO TONER
1396	5	2026-02-22 20:00:00-04	81190	528	\N
1397	5	2026-02-23 20:00:00-04	81582	392	\N
1398	5	2026-02-24 20:00:00-04	81770	188	\N
1399	5	2026-02-25 20:00:00-04	81994	224	\N
1400	5	2026-02-26 20:00:00-04	82226	232	\N
1401	12	2026-02-01 20:00:00-04	34289	0	\N
1402	12	2026-02-02 20:00:00-04	34300	11	\N
1403	12	2026-02-03 20:00:00-04	34310	10	\N
1404	12	2026-02-04 20:00:00-04	34320	10	\N
1405	12	2026-02-05 20:00:00-04	34323	3	\N
1406	12	2026-02-08 20:00:00-04	34351	28	\N
1407	12	2026-02-09 20:00:00-04	34405	54	\N
1408	12	2026-02-10 20:00:00-04	34433	28	\N
1409	12	2026-02-11 20:00:00-04	34442	9	\N
1410	12	2026-02-12 20:00:00-04	34450	8	\N
1411	12	2026-02-15 20:00:00-04	34450	0	FERIADO
1412	12	2026-02-16 20:00:00-04	34450	0	FERIADO
1413	12	2026-02-17 20:00:00-04	34458	8	\N
1414	12	2026-02-18 20:00:00-04	34458	0	\N
1415	12	2026-02-19 20:00:00-04	34471	13	\N
1416	12	2026-02-22 20:00:00-04	34485	14	\N
1417	12	2026-02-23 20:00:00-04	34489	4	\N
1418	12	2026-02-24 20:00:00-04	34507	18	\N
1419	12	2026-02-25 20:00:00-04	34544	37	\N
1420	12	2026-02-26 20:00:00-04	34544	0	\N
1421	10	2026-02-01 20:00:00-04	53390	0	\N
1422	10	2026-02-02 20:00:00-04	53595	205	\N
1423	10	2026-02-03 20:00:00-04	53652	57	\N
1424	10	2026-02-04 20:00:00-04	53860	208	\N
1425	10	2026-02-05 20:00:00-04	54079	219	\N
1426	10	2026-02-08 20:00:00-04	54266	187	\N
1427	10	2026-02-09 20:00:00-04	54546	280	\N
1428	10	2026-02-10 20:00:00-04	54670	124	\N
1429	10	2026-02-11 20:00:00-04	54826	156	\N
1430	10	2026-02-12 20:00:00-04	55163	337	\N
1431	10	2026-02-15 20:00:00-04	55163	0	FERIADO
1432	10	2026-02-16 20:00:00-04	55163	0	FERIADO
1433	10	2026-02-17 20:00:00-04	55385	222	\N
1434	10	2026-02-18 20:00:00-04	55641	256	\N
1435	10	2026-02-19 20:00:00-04	55715	74	\N
1436	10	2026-02-22 20:00:00-04	55987	272	\N
1437	10	2026-02-23 20:00:00-04	56198	211	\N
1438	10	2026-02-24 20:00:00-04	56359	161	\N
1439	10	2026-02-25 20:00:00-04	56585	226	\N
1440	10	2026-02-26 20:00:00-04	56743	158	\N
1441	3	2026-03-01 20:00:00-04	146022	0	\N
1442	3	2026-03-02 20:00:00-04	146204	182	\N
1443	3	2026-03-03 20:00:00-04	146368	164	CAMBIO TONER
1444	3	2026-03-04 20:00:00-04	146521	153	\N
1445	3	2026-03-05 20:00:00-04	146654	133	\N
1446	3	2026-03-08 20:00:00-04	146841	187	\N
1447	3	2026-03-09 20:00:00-04	146935	94	\N
1448	3	2026-03-10 20:00:00-04	147064	129	\N
1449	3	2026-03-11 20:00:00-04	0	0	\N
1450	3	2026-03-12 20:00:00-04	0	0	\N
1451	3	2026-03-15 20:00:00-04	0	0	\N
1452	3	2026-03-16 20:00:00-04	0	0	\N
1453	3	2026-03-17 20:00:00-04	0	0	\N
1454	3	2026-03-18 20:00:00-04	0	0	\N
1455	3	2026-03-19 20:00:00-04	0	0	\N
1456	3	2026-03-22 20:00:00-04	0	0	\N
1457	3	2026-03-23 20:00:00-04	0	0	\N
1458	3	2026-03-24 20:00:00-04	0	0	\N
1459	3	2026-03-25 20:00:00-04	0	0	\N
1460	3	2026-03-26 20:00:00-04	0	0	\N
1461	3	2026-03-29 20:00:00-04	0	0	\N
1462	3	2026-03-30 20:00:00-04	0	0	\N
1463	9	2026-03-01 20:00:00-04	68067	0	\N
1464	9	2026-03-02 20:00:00-04	68416	349	\N
1465	9	2026-03-03 20:00:00-04	68465	49	\N
1466	9	2026-03-04 20:00:00-04	68509	44	\N
1467	9	2026-03-05 20:00:00-04	68761	252	CAMBIO TONER
1468	9	2026-03-08 20:00:00-04	68889	128	\N
1469	9	2026-03-09 20:00:00-04	69090	201	\N
1470	9	2026-03-10 20:00:00-04	69155	65	\N
1471	9	2026-03-11 20:00:00-04	0	0	\N
1472	9	2026-03-12 20:00:00-04	0	0	\N
1473	9	2026-03-15 20:00:00-04	0	0	\N
1474	9	2026-03-16 20:00:00-04	0	0	\N
1475	9	2026-03-17 20:00:00-04	0	0	\N
1476	9	2026-03-18 20:00:00-04	0	0	\N
1477	9	2026-03-19 20:00:00-04	0	0	\N
1478	9	2026-03-22 20:00:00-04	0	0	\N
1479	9	2026-03-23 20:00:00-04	0	0	\N
1480	9	2026-03-24 20:00:00-04	0	0	\N
1481	9	2026-03-25 20:00:00-04	0	0	\N
1482	9	2026-03-26 20:00:00-04	0	0	\N
1483	9	2026-03-29 20:00:00-04	0	0	\N
1484	9	2026-03-30 20:00:00-04	0	0	\N
1486	5	2026-03-02 20:00:00-04	83367	492	\N
1487	5	2026-03-03 20:00:00-04	83764	397	\N
1488	5	2026-03-04 20:00:00-04	84225	461	MANTENIMIENTO EXICOPY
1489	5	2026-03-05 20:00:00-04	84710	485	\N
1490	5	2026-03-08 20:00:00-04	85269	559	\N
1491	5	2026-03-09 20:00:00-04	85789	520	\N
1492	5	2026-03-10 20:00:00-04	86087	298	\N
1493	5	2026-03-11 20:00:00-04	0	0	\N
1494	5	2026-03-12 20:00:00-04	0	0	\N
1495	5	2026-03-15 20:00:00-04	0	0	\N
1496	5	2026-03-16 20:00:00-04	0	0	\N
1497	5	2026-03-17 20:00:00-04	0	0	\N
1498	5	2026-03-18 20:00:00-04	0	0	\N
1499	5	2026-03-19 20:00:00-04	0	0	\N
1500	5	2026-03-22 20:00:00-04	0	0	\N
1501	5	2026-03-23 20:00:00-04	0	0	\N
1502	5	2026-03-24 20:00:00-04	0	0	\N
1503	5	2026-03-25 20:00:00-04	0	0	\N
1504	5	2026-03-26 20:00:00-04	0	0	\N
1505	5	2026-03-29 20:00:00-04	0	0	\N
1506	5	2026-03-30 20:00:00-04	0	0	\N
1507	12	2026-03-01 20:00:00-04	34550	0	\N
1508	12	2026-03-02 20:00:00-04	34554	4	\N
1509	12	2026-03-03 20:00:00-04	34556	2	\N
1510	12	2026-03-04 20:00:00-04	34573	17	\N
1511	12	2026-03-05 20:00:00-04	34578	5	\N
1512	12	2026-03-08 20:00:00-04	34586	8	\N
1513	12	2026-03-09 20:00:00-04	34607	21	\N
1514	12	2026-03-10 20:00:00-04	34616	9	\N
1515	12	2026-03-11 20:00:00-04	0	0	\N
1516	12	2026-03-12 20:00:00-04	0	0	\N
1517	12	2026-03-15 20:00:00-04	0	0	\N
1518	12	2026-03-16 20:00:00-04	0	0	\N
1519	12	2026-03-17 20:00:00-04	0	0	\N
1520	12	2026-03-18 20:00:00-04	0	0	\N
1521	12	2026-03-19 20:00:00-04	0	0	\N
1522	12	2026-03-22 20:00:00-04	0	0	\N
1523	12	2026-03-23 20:00:00-04	0	0	\N
1524	12	2026-03-24 20:00:00-04	0	0	\N
1525	12	2026-03-25 20:00:00-04	0	0	\N
1526	12	2026-03-26 20:00:00-04	0	0	\N
1527	12	2026-03-29 20:00:00-04	0	0	\N
1528	12	2026-03-30 20:00:00-04	0	0	\N
1529	10	2026-03-01 20:00:00-04	56917	0	\N
1530	10	2026-03-02 20:00:00-04	57215	298	\N
1531	10	2026-03-03 20:00:00-04	57366	151	\N
1532	10	2026-03-04 20:00:00-04	57551	185	\N
1533	10	2026-03-05 20:00:00-04	57744	193	\N
1534	10	2026-03-08 20:00:00-04	58028	284	\N
1535	10	2026-03-09 20:00:00-04	58295	267	CAMBIO TONER
1536	10	2026-03-10 20:00:00-04	58468	173	\N
1537	10	2026-03-11 20:00:00-04	0	0	\N
1538	10	2026-03-12 20:00:00-04	0	0	\N
1539	10	2026-03-15 20:00:00-04	0	0	\N
1540	10	2026-03-16 20:00:00-04	0	0	\N
1541	10	2026-03-17 20:00:00-04	0	0	\N
1542	10	2026-03-18 20:00:00-04	0	0	\N
1543	10	2026-03-19 20:00:00-04	0	0	\N
1544	10	2026-03-22 20:00:00-04	0	0	\N
1545	10	2026-03-23 20:00:00-04	0	0	\N
1546	10	2026-03-24 20:00:00-04	0	0	\N
1547	10	2026-03-25 20:00:00-04	0	0	\N
1548	10	2026-03-26 20:00:00-04	0	0	\N
1549	10	2026-03-29 20:00:00-04	0	0	\N
1550	10	2026-03-30 20:00:00-04	0	0	\N
1485	5	2026-03-01 20:00:00-04	82875	0	Funciona
1551	8	2024-12-01 20:00:00-04	590799	0	\N
1552	8	2024-12-02 20:00:00-04	590867	68	\N
1553	8	2024-12-03 20:00:00-04	590878	11	\N
1554	8	2024-12-04 20:00:00-04	590912	34	\N
1555	8	2024-12-05 20:00:00-04	590946	34	\N
1556	8	2024-12-08 20:00:00-04	590966	20	\N
1557	8	2024-12-09 20:00:00-04	590987	21	\N
1558	8	2024-12-10 20:00:00-04	591028	41	\N
1559	8	2024-12-11 20:00:00-04	591058	30	\N
1560	8	2024-12-12 20:00:00-04	591098	40	\N
1561	8	2024-12-15 20:00:00-04	591131	33	\N
1562	8	2024-12-16 20:00:00-04	591155	24	\N
1563	8	2024-12-17 20:00:00-04	591238	83	\N
1564	8	2024-12-18 20:00:00-04	591238	0	\N
1565	8	2024-12-19 20:00:00-04	591359	121	\N
1566	8	2024-12-22 20:00:00-04	591397	38	\N
1567	8	2024-12-25 20:00:00-04	591415	18	\N
1568	8	2024-12-26 20:00:00-04	591485	70	\N
1569	8	2024-12-29 20:00:00-04	591501	16	\N
1570	14	2024-12-01 20:00:00-04	68819	0	\N
1571	14	2024-12-02 20:00:00-04	69219	400	\N
1572	14	2024-12-03 20:00:00-04	69560	341	\N
1573	14	2024-12-04 20:00:00-04	69869	309	\N
1574	14	2024-12-05 20:00:00-04	70117	248	\N
1575	14	2024-12-08 20:00:00-04	70347	230	\N
1576	14	2024-12-09 20:00:00-04	70925	578	\N
1577	14	2024-12-10 20:00:00-04	71577	652	\N
1578	14	2024-12-11 20:00:00-04	71751	174	\N
1579	14	2024-12-12 20:00:00-04	71908	157	\N
1580	14	2024-12-15 20:00:00-04	72251	343	\N
1581	14	2024-12-16 20:00:00-04	72486	235	\N
1582	14	2024-12-17 20:00:00-04	72748	262	\N
1583	14	2024-12-18 20:00:00-04	73717	969	\N
1584	14	2024-12-19 20:00:00-04	73967	250	\N
1585	14	2024-12-22 20:00:00-04	74219	252	\N
1586	14	2024-12-25 20:00:00-04	74493	274	\N
1587	14	2024-12-26 20:00:00-04	74692	199	\N
1588	14	2024-12-29 20:00:00-04	74979	287	\N
1589	15	2024-12-01 20:00:00-04	56546	0	\N
1590	15	2024-12-02 20:00:00-04	56816	270	\N
1591	15	2024-12-03 20:00:00-04	57291	475	\N
1592	15	2024-12-04 20:00:00-04	57805	514	\N
1593	15	2024-12-05 20:00:00-04	58226	421	\N
1594	15	2024-12-08 20:00:00-04	58550	324	\N
1595	15	2024-12-09 20:00:00-04	59053	503	\N
1596	15	2024-12-10 20:00:00-04	59465	412	\N
1597	15	2024-12-11 20:00:00-04	59703	238	\N
1598	15	2024-12-12 20:00:00-04	60509	806	\N
1599	15	2024-12-15 20:00:00-04	61026	517	\N
1600	15	2024-12-16 20:00:00-04	61440	414	\N
1601	15	2024-12-17 20:00:00-04	61715	275	\N
1602	15	2024-12-18 20:00:00-04	61995	280	\N
1603	15	2024-12-19 20:00:00-04	62179	184	\N
1604	15	2024-12-22 20:00:00-04	62391	212	\N
1605	15	2024-12-25 20:00:00-04	62391	0	\N
1606	15	2024-12-26 20:00:00-04	62391	0	\N
1607	15	2024-12-29 20:00:00-04	62391	0	\N
734	9	2025-08-11 20:00:00-04	57155	0	\N
1608	16	2024-12-01 20:00:00-04	165229	0	\N
1609	16	2024-12-02 20:00:00-04	165264	35	\N
1610	16	2024-12-03 20:00:00-04	165382	118	\N
1611	16	2024-12-04 20:00:00-04	165504	122	\N
1612	16	2024-12-05 20:00:00-04	165621	117	\N
1613	16	2024-12-08 20:00:00-04	165695	74	\N
1614	16	2024-12-09 20:00:00-04	165756	61	\N
1615	16	2024-12-10 20:00:00-04	165839	83	\N
1616	16	2024-12-11 20:00:00-04	165910	71	\N
1617	16	2024-12-12 20:00:00-04	165934	24	\N
1618	16	2024-12-15 20:00:00-04	166043	109	\N
1619	16	2024-12-16 20:00:00-04	166074	31	\N
1620	16	2024-12-17 20:00:00-04	166142	68	\N
1621	16	2024-12-18 20:00:00-04	166163	21	\N
1622	16	2024-12-19 20:00:00-04	166195	32	\N
1623	16	2024-12-22 20:00:00-04	166215	20	\N
1624	16	2024-12-25 20:00:00-04	166269	54	\N
1625	16	2024-12-26 20:00:00-04	166290	21	\N
1626	16	2024-12-29 20:00:00-04	166344	54	\N
1627	3	2025-01-01 20:00:00-04	119601	0	\N
1628	3	2025-01-02 20:00:00-04	119629	28	\N
1629	3	2025-01-05 20:00:00-04	119733	104	\N
1630	3	2025-01-06 20:00:00-04	119791	58	\N
1631	3	2025-01-07 20:00:00-04	119891	100	\N
1632	3	2025-01-08 20:00:00-04	119967	76	\N
1633	3	2025-01-09 20:00:00-04	119967	0	\N
1634	3	2025-01-12 20:00:00-04	119985	18	\N
1635	3	2025-01-13 20:00:00-04	120104	119	\N
1636	3	2025-01-14 20:00:00-04	120228	124	\N
1637	3	2025-01-15 20:00:00-04	120321	93	\N
1638	3	2025-01-16 20:00:00-04	120456	135	\N
1639	3	2025-01-19 20:00:00-04	120602	146	\N
1640	3	2025-01-20 20:00:00-04	120716	114	\N
1641	3	2025-01-21 20:00:00-04	120910	194	\N
1642	3	2025-01-22 20:00:00-04	121050	140	\N
1643	3	2025-01-23 20:00:00-04	121199	149	\N
1644	3	2025-01-26 20:00:00-04	121457	258	\N
1645	3	2025-01-27 20:00:00-04	121650	193	\N
1646	3	2025-01-28 20:00:00-04	121775	125	\N
1647	3	2025-01-29 20:00:00-04	121830	55	\N
1648	3	2025-01-30 20:00:00-04	121925	95	\N
1649	4	2025-01-01 20:00:00-04	1835	0	\N
1650	4	2025-01-02 20:00:00-04	1846	11	\N
1651	4	2025-01-05 20:00:00-04	2007	161	\N
1652	4	2025-01-06 20:00:00-04	2041	34	\N
1653	4	2025-01-07 20:00:00-04	2072	31	\N
1654	4	2025-01-08 20:00:00-04	2091	19	\N
1655	4	2025-01-09 20:00:00-04	2091	0	\N
1656	4	2025-01-12 20:00:00-04	2091	0	REEMPLAZADA MF264
1657	4	2025-01-13 20:00:00-04	50457	0	\N
1658	4	2025-01-14 20:00:00-04	50463	6	\N
1659	4	2025-01-15 20:00:00-04	50485	22	\N
1660	4	2025-01-16 20:00:00-04	50588	103	\N
1661	4	2025-01-19 20:00:00-04	50602	14	\N
1662	4	2025-01-20 20:00:00-04	50619	17	\N
1663	4	2025-01-21 20:00:00-04	50656	37	\N
1664	4	2025-01-22 20:00:00-04	50688	32	\N
1665	4	2025-01-23 20:00:00-04	50705	17	\N
1666	4	2025-01-26 20:00:00-04	50754	49	\N
1667	4	2025-01-27 20:00:00-04	50780	26	\N
1668	4	2025-01-28 20:00:00-04	50787	7	\N
1669	4	2025-01-29 20:00:00-04	50806	19	\N
1670	4	2025-01-30 20:00:00-04	50831	25	\N
1671	5	2025-01-01 20:00:00-04	12626	0	\N
1672	5	2025-01-02 20:00:00-04	12748	122	\N
1673	5	2025-01-05 20:00:00-04	13116	368	\N
1674	5	2025-01-06 20:00:00-04	13301	185	\N
1675	5	2025-01-07 20:00:00-04	13395	94	\N
1676	5	2025-01-08 20:00:00-04	13656	261	\N
1677	5	2025-01-09 20:00:00-04	13656	0	\N
1678	5	2025-01-12 20:00:00-04	13710	54	\N
1679	5	2025-01-13 20:00:00-04	13996	458	\N
1680	5	2025-01-14 20:00:00-04	14168	172	\N
1681	5	2025-01-15 20:00:00-04	14324	156	\N
1682	5	2025-01-16 20:00:00-04	14620	296	\N
1683	5	2025-01-19 20:00:00-04	14912	292	\N
1684	5	2025-01-20 20:00:00-04	15059	147	\N
1685	5	2025-01-21 20:00:00-04	15363	304	\N
1686	5	2025-01-22 20:00:00-04	15616	253	\N
1687	5	2025-01-23 20:00:00-04	15896	280	\N
1688	5	2025-01-26 20:00:00-04	16360	464	\N
1689	5	2025-01-27 20:00:00-04	16568	208	\N
1690	5	2025-01-28 20:00:00-04	16729	161	\N
1691	5	2025-01-29 20:00:00-04	16983	254	\N
1692	5	2025-01-30 20:00:00-04	17191	208	\N
1693	6	2025-01-01 20:00:00-04	16199	0	\N
1694	6	2025-01-02 20:00:00-04	16208	9	\N
1695	6	2025-01-05 20:00:00-04	16273	65	\N
1696	6	2025-01-06 20:00:00-04	16292	19	\N
1697	6	2025-01-07 20:00:00-04	16298	6	\N
1698	6	2025-01-08 20:00:00-04	16306	8	\N
1699	6	2025-01-09 20:00:00-04	16306	0	\N
1700	6	2025-01-12 20:00:00-04	16322	16	\N
1701	6	2025-01-13 20:00:00-04	16518	211	\N
1702	6	2025-01-14 20:00:00-04	16533	15	\N
1703	6	2025-01-15 20:00:00-04	16672	139	\N
1704	6	2025-01-16 20:00:00-04	16796	124	\N
1705	6	2025-01-19 20:00:00-04	16913	117	\N
1706	6	2025-01-20 20:00:00-04	16933	20	\N
1707	6	2025-01-21 20:00:00-04	16976	43	\N
1708	6	2025-01-22 20:00:00-04	16998	22	\N
1709	6	2025-01-23 20:00:00-04	17030	32	\N
1710	6	2025-01-26 20:00:00-04	17097	67	\N
1711	6	2025-01-27 20:00:00-04	17226	129	\N
1712	6	2025-01-28 20:00:00-04	17236	10	\N
1713	6	2025-01-29 20:00:00-04	17377	141	\N
1714	6	2025-01-30 20:00:00-04	17426	49	\N
1715	8	2025-01-01 20:00:00-04	591501	0	\N
1716	8	2025-01-02 20:00:00-04	591501	0	\N
1717	8	2025-01-05 20:00:00-04	591504	3	\N
1718	8	2025-01-06 20:00:00-04	591523	19	\N
1719	8	2025-01-07 20:00:00-04	591527	4	\N
1720	8	2025-01-08 20:00:00-04	591545	18	\N
1721	8	2025-01-09 20:00:00-04	591545	0	\N
1722	8	2025-01-12 20:00:00-04	591550	5	\N
1723	8	2025-01-13 20:00:00-04	591598	48	\N
1724	8	2025-01-14 20:00:00-04	591610	0	REEMPLAZADA MF455
1725	8	2025-01-15 20:00:00-04	2123	0	\N
1726	8	2025-01-16 20:00:00-04	2168	45	\N
1727	8	2025-01-19 20:00:00-04	2220	52	\N
1728	8	2025-01-20 20:00:00-04	2230	10	\N
1729	8	2025-01-21 20:00:00-04	2266	36	\N
1730	8	2025-01-22 20:00:00-04	2282	16	\N
1731	8	2025-01-23 20:00:00-04	2336	54	\N
1732	8	2025-01-26 20:00:00-04	2539	203	\N
1733	8	2025-01-27 20:00:00-04	2600	61	\N
1734	8	2025-01-28 20:00:00-04	2664	64	\N
1735	8	2025-01-29 20:00:00-04	2794	130	\N
1736	8	2025-01-30 20:00:00-04	2803	9	\N
1737	14	2025-01-01 20:00:00-04	74979	0	\N
1738	14	2025-01-02 20:00:00-04	75808	829	\N
1739	14	2025-01-05 20:00:00-04	76164	356	\N
1740	14	2025-01-06 20:00:00-04	76534	370	\N
1741	14	2025-01-07 20:00:00-04	76819	285	\N
1742	14	2025-01-08 20:00:00-04	77196	377	\N
1743	14	2025-01-09 20:00:00-04	77196	0	\N
1744	14	2025-01-12 20:00:00-04	77986	790	\N
1745	14	2025-01-13 20:00:00-04	78560	574	\N
1746	14	2025-01-14 20:00:00-04	78936	376	\N
1747	14	2025-01-15 20:00:00-04	79175	239	\N
1748	14	2025-01-16 20:00:00-04	79567	392	\N
1749	14	2025-01-19 20:00:00-04	80124	557	\N
1750	14	2025-01-20 20:00:00-04	80466	342	\N
1751	14	2025-01-21 20:00:00-04	81144	678	\N
1752	14	2025-01-22 20:00:00-04	81763	619	\N
1753	14	2025-01-23 20:00:00-04	82213	450	\N
1754	14	2025-01-26 20:00:00-04	82937	724	\N
1755	14	2025-01-27 20:00:00-04	83757	820	\N
1756	14	2025-01-28 20:00:00-04	84872	1115	\N
1757	14	2025-01-29 20:00:00-04	85625	753	\N
1758	14	2025-01-30 20:00:00-04	86379	754	\N
1759	15	2025-01-01 20:00:00-04	62391	0	\N
1760	15	2025-01-02 20:00:00-04	62391	0	\N
1761	15	2025-01-05 20:00:00-04	62391	0	\N
1762	15	2025-01-06 20:00:00-04	62391	0	\N
1763	15	2025-01-07 20:00:00-04	62391	0	\N
1764	15	2025-01-08 20:00:00-04	62391	0	\N
1765	15	2025-01-09 20:00:00-04	62391	0	\N
1766	15	2025-01-12 20:00:00-04	62490	99	\N
1767	15	2025-01-13 20:00:00-04	62653	163	\N
1768	15	2025-01-14 20:00:00-04	63140	487	\N
1769	15	2025-01-15 20:00:00-04	63594	454	\N
1770	15	2025-01-16 20:00:00-04	64070	476	\N
1771	15	2025-01-19 20:00:00-04	64504	434	\N
1772	15	2025-01-20 20:00:00-04	64888	384	\N
1773	15	2025-01-21 20:00:00-04	65280	392	\N
1774	15	2025-01-22 20:00:00-04	65695	415	\N
1775	15	2025-01-23 20:00:00-04	65695	0	\N
1776	15	2025-01-26 20:00:00-04	65945	250	\N
1777	15	2025-01-27 20:00:00-04	66497	552	\N
1778	15	2025-01-28 20:00:00-04	67107	610	\N
1779	15	2025-01-29 20:00:00-04	67746	639	\N
1780	15	2025-01-30 20:00:00-04	68380	634	\N
1781	16	2025-01-01 20:00:00-04	166344	0	\N
1782	16	2025-01-02 20:00:00-04	166403	59	\N
1783	16	2025-01-05 20:00:00-04	166457	54	\N
1784	16	2025-01-06 20:00:00-04	166533	76	\N
1785	16	2025-01-07 20:00:00-04	166597	64	\N
1786	16	2025-01-08 20:00:00-04	166597	0	\N
1787	16	2025-01-09 20:00:00-04	166653	56	\N
1788	16	2025-01-12 20:00:00-04	166750	97	\N
1789	16	2025-01-13 20:00:00-04	166824	74	\N
1790	16	2025-01-14 20:00:00-04	166864	40	\N
1791	16	2025-01-15 20:00:00-04	166914	50	\N
1792	16	2025-01-16 20:00:00-04	166934	20	\N
1793	16	2025-01-19 20:00:00-04	167033	99	\N
1794	16	2025-01-20 20:00:00-04	167114	81	\N
1795	16	2025-01-21 20:00:00-04	167221	107	\N
1796	16	2025-01-22 20:00:00-04	167290	69	\N
1797	16	2025-01-23 20:00:00-04	167334	44	\N
1798	16	2025-01-26 20:00:00-04	167468	134	\N
1799	16	2025-01-27 20:00:00-04	167580	112	\N
1800	16	2025-01-28 20:00:00-04	167628	48	\N
1801	16	2025-01-29 20:00:00-04	167714	86	\N
1802	16	2025-01-30 20:00:00-04	167784	70	\N
1803	14	2025-02-02 20:00:00-04	86945	0	\N
1804	14	2025-02-03 20:00:00-04	87837	892	\N
1805	14	2025-02-04 20:00:00-04	88159	322	\N
1806	14	2025-02-05 20:00:00-04	88777	618	\N
1807	14	2025-02-06 20:00:00-04	89198	421	\N
1808	14	2025-02-09 20:00:00-04	89844	646	\N
1809	14	2025-02-10 20:00:00-04	90611	767	\N
1810	14	2025-02-11 20:00:00-04	91103	492	\N
1811	14	2025-02-12 20:00:00-04	91953	850	\N
1812	14	2025-02-13 20:00:00-04	92653	700	\N
1813	14	2025-02-16 20:00:00-04	93228	575	\N
1814	14	2025-02-17 20:00:00-04	93679	451	\N
1815	14	2025-02-18 20:00:00-04	94364	685	\N
1816	14	2025-02-19 20:00:00-04	94891	527	\N
1817	14	2025-02-20 20:00:00-04	95321	430	\N
1818	14	2025-02-23 20:00:00-04	96073	752	\N
1819	14	2025-02-24 20:00:00-04	96700	627	\N
1820	14	2025-02-25 20:00:00-04	98222	1522	\N
1821	14	2025-02-26 20:00:00-04	98749	527	\N
1822	14	2025-02-27 20:00:00-04	99212	463	\N
1823	15	2025-02-02 20:00:00-04	69021	0	\N
1824	15	2025-02-03 20:00:00-04	69775	754	\N
1825	15	2025-02-04 20:00:00-04	70231	456	\N
1826	15	2025-02-05 20:00:00-04	70841	610	\N
1827	15	2025-02-06 20:00:00-04	71156	315	\N
1828	15	2025-02-09 20:00:00-04	71673	517	\N
1829	15	2025-02-10 20:00:00-04	72293	620	\N
1830	15	2025-02-11 20:00:00-04	72293	0	\N
1831	15	2025-02-12 20:00:00-04	72448	155	\N
1832	15	2025-02-13 20:00:00-04	72701	253	\N
1833	15	2025-02-16 20:00:00-04	72986	285	\N
1834	15	2025-02-17 20:00:00-04	73721	735	\N
1835	15	2025-02-18 20:00:00-04	74334	613	\N
1836	15	2025-02-19 20:00:00-04	75169	835	\N
1837	15	2025-02-20 20:00:00-04	76229	1060	\N
1838	15	2025-02-23 20:00:00-04	76229	0	\N
1839	15	2025-02-24 20:00:00-04	76806	577	\N
1840	15	2025-02-25 20:00:00-04	77359	553	\N
1841	15	2025-02-26 20:00:00-04	77946	587	\N
1842	15	2025-02-27 20:00:00-04	78397	451	\N
1843	16	2025-02-02 20:00:00-04	167889	0	\N
1844	16	2025-02-03 20:00:00-04	167990	101	\N
1845	16	2025-02-04 20:00:00-04	168077	87	\N
1846	16	2025-02-05 20:00:00-04	168135	58	\N
1847	16	2025-02-06 20:00:00-04	168215	80	\N
1848	16	2025-02-09 20:00:00-04	168290	75	\N
1849	16	2025-02-10 20:00:00-04	168370	80	\N
1850	16	2025-02-11 20:00:00-04	168449	79	\N
1851	16	2025-02-12 20:00:00-04	168661	212	\N
1852	16	2025-02-13 20:00:00-04	168700	39	\N
1853	16	2025-02-16 20:00:00-04	168752	52	\N
1854	16	2025-02-17 20:00:00-04	168947	195	\N
1855	16	2025-02-18 20:00:00-04	169018	71	\N
1856	16	2025-02-19 20:00:00-04	169071	53	\N
1857	16	2025-02-20 20:00:00-04	169099	28	\N
1858	16	2025-02-23 20:00:00-04	169226	127	\N
1859	16	2025-02-24 20:00:00-04	169336	110	\N
1860	16	2025-02-25 20:00:00-04	169398	62	\N
1861	16	2025-02-26 20:00:00-04	169492	94	\N
1862	16	2025-02-27 20:00:00-04	169552	60	\N
1863	14	2025-03-04 20:00:00-04	99787	0	\N
1864	14	2025-03-05 20:00:00-04	100556	769	\N
1865	14	2025-03-06 20:00:00-04	101150	594	\N
1866	14	2025-03-09 20:00:00-04	101952	802	\N
1867	14	2025-03-10 20:00:00-04	102968	1016	\N
1868	14	2025-03-11 20:00:00-04	103050	82	\N
1869	14	2025-03-12 20:00:00-04	103611	561	\N
1870	14	2025-03-13 20:00:00-04	104030	419	\N
1871	14	2025-03-16 20:00:00-04	104878	848	\N
1872	14	2025-03-17 20:00:00-04	105372	494	\N
1873	14	2025-03-18 20:00:00-04	106142	770	\N
1874	14	2025-03-19 20:00:00-04	107226	1084	\N
1875	14	2025-03-20 20:00:00-04	108074	848	\N
1876	14	2025-03-23 20:00:00-04	108999	925	\N
1877	14	2025-03-24 20:00:00-04	109189	190	\N
1878	14	2025-03-25 20:00:00-04	109637	448	\N
1879	14	2025-03-26 20:00:00-04	109997	360	\N
1880	14	2025-03-27 20:00:00-04	110684	687	\N
1881	14	2025-03-30 20:00:00-04	110991	307	\N
1882	15	2025-03-04 20:00:00-04	79167	0	\N
1883	15	2025-03-05 20:00:00-04	79719	552	\N
1884	15	2025-03-06 20:00:00-04	80130	411	\N
1885	15	2025-03-09 20:00:00-04	80546	416	\N
1886	15	2025-03-10 20:00:00-04	81564	1018	\N
1887	15	2025-03-11 20:00:00-04	82186	622	\N
1888	15	2025-03-12 20:00:00-04	82752	566	\N
1889	15	2025-03-13 20:00:00-04	83258	506	\N
1890	15	2025-03-16 20:00:00-04	83991	733	\N
1891	15	2025-03-17 20:00:00-04	84522	531	\N
1892	15	2025-03-18 20:00:00-04	85469	947	\N
1893	15	2025-03-19 20:00:00-04	86542	1073	\N
1894	15	2025-03-20 20:00:00-04	87033	491	\N
1895	15	2025-03-23 20:00:00-04	87621	588	\N
1896	15	2025-03-24 20:00:00-04	88000	379	\N
1897	15	2025-03-25 20:00:00-04	89229	1229	\N
1898	15	2025-03-26 20:00:00-04	89951	722	\N
1899	15	2025-03-27 20:00:00-04	90341	390	\N
1900	15	2025-03-30 20:00:00-04	90556	215	\N
1901	16	2025-03-04 20:00:00-04	169656	0	\N
1902	16	2025-03-05 20:00:00-04	169765	109	\N
1903	16	2025-03-06 20:00:00-04	169813	48	\N
1904	16	2025-03-09 20:00:00-04	169863	50	\N
1905	16	2025-03-10 20:00:00-04	169997	134	\N
1906	16	2025-03-11 20:00:00-04	170036	39	\N
1907	16	2025-03-12 20:00:00-04	170112	76	\N
1908	16	2025-03-13 20:00:00-04	170184	72	\N
1909	16	2025-03-16 20:00:00-04	170272	88	\N
1910	16	2025-03-17 20:00:00-04	170352	80	\N
1911	16	2025-03-18 20:00:00-04	170478	126	\N
1912	16	2025-03-19 20:00:00-04	170501	23	\N
1913	16	2025-03-20 20:00:00-04	170581	80	\N
1914	16	2025-03-23 20:00:00-04	170665	84	\N
1915	16	2025-03-24 20:00:00-04	170807	142	\N
1916	16	2025-03-25 20:00:00-04	170873	66	\N
1917	16	2025-03-26 20:00:00-04	170902	29	\N
1918	16	2025-03-27 20:00:00-04	170956	54	\N
1919	16	2025-03-30 20:00:00-04	171028	72	\N
1920	14	2025-03-31 20:00:00-04	111698	0	\N
1921	14	2025-04-01 20:00:00-04	112238	540	\N
1922	14	2025-04-02 20:00:00-04	113075	837	\N
1923	14	2025-04-03 20:00:00-04	113723	648	\N
1924	14	2025-04-06 20:00:00-04	114678	955	\N
1925	14	2025-04-07 20:00:00-04	115776	1098	\N
1926	14	2025-04-08 20:00:00-04	116705	929	\N
1927	14	2025-04-09 20:00:00-04	117569	864	\N
1928	14	2025-04-10 20:00:00-04	118423	854	\N
1929	14	2025-04-13 20:00:00-04	118828	405	\N
1930	14	2025-04-14 20:00:00-04	119254	426	\N
1931	14	2025-04-15 20:00:00-04	119735	481	\N
1932	14	2025-04-20 20:00:00-04	119995	260	\N
1933	14	2025-04-21 20:00:00-04	120465	470	\N
1934	14	2025-04-22 20:00:00-04	121265	800	\N
1935	14	2025-04-23 20:00:00-04	122198	933	\N
1936	14	2025-04-24 20:00:00-04	122813	615	\N
1937	14	2025-04-27 20:00:00-04	123783	970	\N
1938	14	2025-04-28 20:00:00-04	124571	788	\N
1939	14	2025-04-29 20:00:00-04	125508	937	\N
1940	15	2025-03-31 20:00:00-04	90987	0	\N
1941	15	2025-04-01 20:00:00-04	91392	405	\N
1942	15	2025-04-02 20:00:00-04	91428	36	\N
1943	15	2025-04-03 20:00:00-04	91428	0	\N
1944	15	2025-04-06 20:00:00-04	91435	7	\N
1945	15	2025-04-07 20:00:00-04	91439	4	\N
1946	15	2025-04-08 20:00:00-04	91440	1	\N
1947	15	2025-04-09 20:00:00-04	91996	556	\N
1948	15	2025-04-10 20:00:00-04	92214	218	\N
1949	15	2025-04-13 20:00:00-04	92338	124	\N
1950	15	2025-04-14 20:00:00-04	93107	769	\N
1951	15	2025-04-15 20:00:00-04	93984	877	\N
1952	15	2025-04-20 20:00:00-04	94459	475	\N
1953	15	2025-04-21 20:00:00-04	94999	540	\N
1954	15	2025-04-22 20:00:00-04	95133	134	\N
1955	15	2025-04-23 20:00:00-04	95654	521	\N
1956	15	2025-04-24 20:00:00-04	95958	304	\N
1957	15	2025-04-27 20:00:00-04	96608	650	\N
1958	15	2025-04-28 20:00:00-04	96954	346	\N
1959	15	2025-04-29 20:00:00-04	97234	280	\N
1960	16	2025-03-31 20:00:00-04	171097	0	\N
1961	16	2025-04-01 20:00:00-04	171192	95	\N
1962	16	2025-04-02 20:00:00-04	171273	81	\N
1963	16	2025-04-03 20:00:00-04	171341	68	\N
1964	16	2025-04-06 20:00:00-04	171381	40	\N
1965	16	2025-04-07 20:00:00-04	171498	117	\N
1966	16	2025-04-08 20:00:00-04	171557	59	\N
1967	16	2025-04-09 20:00:00-04	171613	56	\N
1968	16	2025-04-10 20:00:00-04	171690	77	\N
1969	16	2025-04-13 20:00:00-04	171821	131	\N
1970	16	2025-04-14 20:00:00-04	171879	58	\N
1971	16	2025-04-15 20:00:00-04	171911	32	\N
1972	16	2025-04-20 20:00:00-04	172008	97	\N
1973	16	2025-04-21 20:00:00-04	172096	88	\N
1974	16	2025-04-22 20:00:00-04	172146	50	\N
1975	16	2025-04-23 20:00:00-04	172201	55	\N
1976	16	2025-04-24 20:00:00-04	172262	61	\N
1977	16	2025-04-27 20:00:00-04	172365	103	\N
1978	16	2025-04-28 20:00:00-04	172418	53	\N
1979	16	2025-04-29 20:00:00-04	172519	101	\N
1980	14	2025-05-01 20:00:00-04	126915	0	\N
1981	14	2025-05-04 20:00:00-04	127465	550	\N
1982	14	2025-05-05 20:00:00-04	128001	536	\N
1983	14	2025-05-06 20:00:00-04	128790	789	\N
1984	14	2025-05-07 20:00:00-04	129612	822	\N
1985	14	2025-05-08 20:00:00-04	130425	813	\N
1986	14	2025-05-11 20:00:00-04	131188	763	\N
1987	14	2025-05-12 20:00:00-04	131965	777	\N
1988	14	2025-05-13 20:00:00-04	132514	549	\N
1989	14	2025-05-14 20:00:00-04	133545	1031	\N
1990	14	2025-05-15 20:00:00-04	134018	473	\N
1991	14	2025-05-18 20:00:00-04	134415	397	\N
1992	14	2025-05-19 20:00:00-04	134499	84	\N
1993	14	2025-05-20 20:00:00-04	134585	86	\N
1994	14	2025-05-21 20:00:00-04	134612	27	\N
1995	14	2025-05-22 20:00:00-04	134717	105	\N
1996	14	2025-05-25 20:00:00-04	134727	10	\N
1997	14	2025-05-26 20:00:00-04	135514	787	\N
1998	14	2025-05-27 20:00:00-04	136215	701	\N
1999	14	2025-05-28 20:00:00-04	136999	784	\N
2000	14	2025-05-29 20:00:00-04	137547	548	\N
2001	15	2025-05-01 20:00:00-04	97606	0	\N
2002	15	2025-05-04 20:00:00-04	97806	200	\N
2003	15	2025-05-05 20:00:00-04	98113	307	\N
2004	15	2025-05-06 20:00:00-04	98419	306	\N
2005	15	2025-05-07 20:00:00-04	99001	582	\N
2006	15	2025-05-08 20:00:00-04	99621	620	\N
2007	15	2025-05-11 20:00:00-04	100010	389	\N
2008	15	2025-05-12 20:00:00-04	100932	922	\N
2009	15	2025-05-13 20:00:00-04	101340	408	\N
2010	15	2025-05-14 20:00:00-04	101998	658	\N
2011	15	2025-05-15 20:00:00-04	102608	610	\N
2012	15	2025-05-18 20:00:00-04	102747	139	\N
2013	15	2025-05-19 20:00:00-04	102987	240	\N
2014	15	2025-05-20 20:00:00-04	103151	164	\N
2015	15	2025-05-21 20:00:00-04	103698	547	\N
2016	15	2025-05-22 20:00:00-04	104085	387	\N
2017	15	2025-05-25 20:00:00-04	105468	1383	\N
2018	15	2025-05-26 20:00:00-04	106895	1427	\N
2019	15	2025-05-27 20:00:00-04	107850	955	\N
2020	15	2025-05-28 20:00:00-04	108215	365	\N
2021	15	2025-05-29 20:00:00-04	109747	1532	\N
2022	16	2025-05-01 20:00:00-04	172568	0	\N
2023	16	2025-05-04 20:00:00-04	172663	95	\N
2024	16	2025-05-05 20:00:00-04	172715	52	\N
2025	16	2025-05-06 20:00:00-04	172793	78	\N
2026	16	2025-05-07 20:00:00-04	172837	44	\N
2027	16	2025-05-08 20:00:00-04	172878	41	\N
2028	16	2025-05-11 20:00:00-04	172949	71	\N
2029	16	2025-05-12 20:00:00-04	173014	65	\N
2030	16	2025-05-13 20:00:00-04	173068	54	\N
2031	16	2025-05-14 20:00:00-04	173127	59	\N
2032	16	2025-05-15 20:00:00-04	173185	58	\N
2033	16	2025-05-18 20:00:00-04	173221	36	\N
2034	16	2025-05-19 20:00:00-04	173254	33	\N
2035	16	2025-05-20 20:00:00-04	173298	44	\N
2036	16	2025-05-21 20:00:00-04	173312	14	\N
2037	16	2025-05-22 20:00:00-04	173335	23	\N
2038	16	2025-05-25 20:00:00-04	173361	26	\N
2039	16	2025-05-26 20:00:00-04	173412	51	\N
2040	16	2025-05-27 20:00:00-04	173484	72	\N
2041	16	2025-05-28 20:00:00-04	173564	80	\N
2042	16	2025-05-29 20:00:00-04	173679	115	\N
2043	14	2025-06-01 20:00:00-04	137693	0	\N
2044	14	2025-06-02 20:00:00-04	138012	319	\N
2045	14	2025-06-03 20:00:00-04	138342	330	\N
2046	14	2025-06-04 20:00:00-04	139012	670	\N
2047	14	2025-06-05 20:00:00-04	139868	856	\N
2048	14	2025-06-08 20:00:00-04	140807	939	\N
2049	14	2025-06-09 20:00:00-04	141625	818	\N
2050	14	2025-06-10 20:00:00-04	142987	1362	\N
2051	14	2025-06-11 20:00:00-04	143698	711	\N
2052	14	2025-06-12 20:00:00-04	143999	301	\N
2053	14	2025-06-15 20:00:00-04	144061	62	\N
2054	14	2025-06-16 20:00:00-04	144347	286	\N
2055	14	2025-06-17 20:00:00-04	145545	1198	\N
2056	14	2025-06-18 20:00:00-04	146087	542	\N
2057	14	2025-06-19 20:00:00-04	146569	482	\N
2058	14	2025-06-22 20:00:00-04	147871	1302	\N
2059	14	2025-06-24 20:00:00-04	148614	743	\N
2060	14	2025-06-25 20:00:00-04	149578	964	\N
2061	14	2025-06-26 20:00:00-04	150141	563	\N
2062	14	2025-06-29 20:00:00-04	150411	270	\N
2063	15	2025-06-01 20:00:00-04	111306	0	\N
2064	15	2025-06-02 20:00:00-04	111677	371	\N
2065	15	2025-06-03 20:00:00-04	112039	362	\N
2066	15	2025-06-04 20:00:00-04	112427	388	\N
2067	15	2025-06-05 20:00:00-04	112730	303	\N
2068	15	2025-06-08 20:00:00-04	113288	558	\N
2069	15	2025-06-09 20:00:00-04	114214	926	\N
2070	15	2025-06-10 20:00:00-04	114687	473	\N
2071	15	2025-06-11 20:00:00-04	114998	311	\N
2072	15	2025-06-12 20:00:00-04	115012	14	\N
2073	15	2025-06-15 20:00:00-04	115022	10	\N
2074	15	2025-06-16 20:00:00-04	115268	246	\N
2075	15	2025-06-17 20:00:00-04	116245	977	\N
2076	15	2025-06-18 20:00:00-04	116732	487	\N
2077	15	2025-06-19 20:00:00-04	117112	380	\N
2078	15	2025-06-22 20:00:00-04	118303	1191	\N
2079	15	2025-06-24 20:00:00-04	119056	753	\N
2080	15	2025-06-25 20:00:00-04	119965	909	\N
2081	15	2025-06-26 20:00:00-04	120586	621	\N
2082	15	2025-06-29 20:00:00-04	120990	404	\N
2083	16	2025-06-01 20:00:00-04	173727	0	\N
2084	16	2025-06-02 20:00:00-04	173796	69	\N
2085	16	2025-06-03 20:00:00-04	173833	37	\N
2086	16	2025-06-04 20:00:00-04	173889	56	\N
2087	16	2025-06-05 20:00:00-04	173941	52	\N
2088	16	2025-06-08 20:00:00-04	173958	17	\N
2089	16	2025-06-09 20:00:00-04	174012	54	\N
2090	16	2025-06-10 20:00:00-04	174054	42	\N
2091	16	2025-06-11 20:00:00-04	174097	43	\N
2092	16	2025-06-12 20:00:00-04	174146	49	\N
2093	16	2025-06-15 20:00:00-04	174187	41	\N
2094	16	2025-06-16 20:00:00-04	174231	44	\N
2095	16	2025-06-17 20:00:00-04	174312	81	\N
2096	16	2025-06-18 20:00:00-04	174384	72	\N
2097	16	2025-06-19 20:00:00-04	174452	68	\N
2098	16	2025-06-22 20:00:00-04	174574	122	\N
2099	16	2025-06-24 20:00:00-04	174617	43	\N
2100	16	2025-06-25 20:00:00-04	174696	79	\N
2101	16	2025-06-26 20:00:00-04	174737	41	\N
2102	16	2025-06-29 20:00:00-04	174855	118	\N
2103	14	2025-07-31 20:00:00-04	169587	0	\N
2104	14	2025-08-03 20:00:00-04	169933	346	\N
2105	14	2025-08-04 20:00:00-04	170695	762	\N
2106	14	2025-08-05 20:00:00-04	171276	581	\N
2107	14	2025-08-06 20:00:00-04	172172	896	\N
2108	14	2025-08-07 20:00:00-04	172252	80	\N
2109	14	2025-08-10 20:00:00-04	172469	217	\N
2110	14	2025-08-11 20:00:00-04	173520	1051	\N
2111	14	2025-08-12 20:00:00-04	174101	581	\N
2112	14	2025-08-13 20:00:00-04	174986	885	\N
2113	14	2025-08-14 20:00:00-04	175723	737	\N
2114	14	2025-08-17 20:00:00-04	176285	562	\N
2115	14	2025-08-18 20:00:00-04	176944	659	\N
2116	14	2025-08-19 20:00:00-04	177872	928	\N
2117	14	2025-08-20 20:00:00-04	178849	977	\N
2118	14	2025-08-21 20:00:00-04	179353	504	\N
2119	14	2025-08-24 20:00:00-04	180069	716	\N
2120	14	2025-08-25 20:00:00-04	180396	327	CAMBIO TONER
2121	14	2025-08-26 20:00:00-04	181714	1318	\N
2122	14	2025-08-27 20:00:00-04	182647	933	\N
2123	14	2025-08-28 20:00:00-04	183353	706	\N
2124	15	2025-07-31 20:00:00-04	136429	0	\N
2125	15	2025-08-03 20:00:00-04	136545	116	\N
2126	15	2025-08-04 20:00:00-04	137121	576	\N
2127	15	2025-08-05 20:00:00-04	137749	628	\N
2128	15	2025-08-06 20:00:00-04	138521	772	\N
2129	15	2025-08-07 20:00:00-04	139210	689	\N
2130	15	2025-08-10 20:00:00-04	139434	224	\N
2131	15	2025-08-11 20:00:00-04	140599	1165	\N
2132	15	2025-08-12 20:00:00-04	141598	999	\N
2133	15	2025-08-13 20:00:00-04	142431	833	\N
2134	15	2025-08-14 20:00:00-04	143336	905	\N
2135	15	2025-08-17 20:00:00-04	143835	499	\N
2136	15	2025-08-18 20:00:00-04	144728	893	\N
2137	15	2025-08-19 20:00:00-04	145442	714	\N
2138	15	2025-08-20 20:00:00-04	146101	659	\N
2139	15	2025-08-21 20:00:00-04	146858	757	\N
2140	15	2025-08-24 20:00:00-04	147828	970	\N
2141	15	2025-08-25 20:00:00-04	148272	444	\N
2142	15	2025-08-26 20:00:00-04	149294	1022	\N
2143	15	2025-08-27 20:00:00-04	149952	658	\N
2144	15	2025-08-28 20:00:00-04	150732	780	\N
2145	16	2025-07-31 20:00:00-04	176355	0	\N
2146	16	2025-08-03 20:00:00-04	176424	69	\N
2147	16	2025-08-04 20:00:00-04	176453	29	\N
2148	16	2025-08-05 20:00:00-04	176544	91	\N
2149	16	2025-08-06 20:00:00-04	176581	37	\N
2150	16	2025-08-07 20:00:00-04	176676	95	\N
2151	16	2025-08-10 20:00:00-04	176804	128	CAMBIO TONER
2152	16	2025-08-11 20:00:00-04	176897	93	\N
2153	16	2025-08-12 20:00:00-04	176976	79	\N
2154	16	2025-08-13 20:00:00-04	177100	124	\N
2155	16	2025-08-14 20:00:00-04	177210	110	\N
2156	16	2025-08-17 20:00:00-04	177368	158	\N
2157	16	2025-08-18 20:00:00-04	177449	81	\N
2158	16	2025-08-19 20:00:00-04	177577	128	\N
2159	16	2025-08-20 20:00:00-04	177674	97	\N
2160	16	2025-08-21 20:00:00-04	177702	28	\N
2161	16	2025-08-24 20:00:00-04	177948	246	\N
2162	16	2025-08-25 20:00:00-04	178040	92	\N
2163	16	2025-08-26 20:00:00-04	178204	164	\N
2164	16	2025-08-27 20:00:00-04	178335	131	\N
2165	16	2025-08-28 20:00:00-04	178402	67	\N
2166	14	2025-08-31 20:00:00-04	184943	0	\N
2167	14	2025-09-01 20:00:00-04	185579	636	\N
2168	14	2025-09-02 20:00:00-04	185945	366	\N
2169	14	2025-09-03 20:00:00-04	186354	409	\N
2170	14	2025-09-04 20:00:00-04	187595	1241	\N
2171	14	2025-09-07 20:00:00-04	187905	310	\N
2172	14	2025-09-08 20:00:00-04	188798	893	\N
2173	14	2025-09-09 20:00:00-04	189292	494	\N
2174	14	2025-09-10 20:00:00-04	189819	527	\N
2175	14	2025-09-11 20:00:00-04	190412	593	CAMBIO TONER
2176	14	2025-09-14 20:00:00-04	191218	806	\N
2177	14	2025-09-15 20:00:00-04	192086	868	MANTENIMIENTO EXYCOPY
2178	14	2025-09-16 20:00:00-04	192604	518	\N
2179	14	2025-09-17 20:00:00-04	193469	865	\N
2180	14	2025-09-18 20:00:00-04	193920	451	\N
2181	14	2025-09-21 20:00:00-04	194691	771	\N
2182	14	2025-09-22 20:00:00-04	196301	1610	\N
2183	14	2025-09-23 20:00:00-04	197247	946	\N
2184	14	2025-09-24 20:00:00-04	197820	573	\N
2185	14	2025-09-25 20:00:00-04	198415	595	\N
2186	14	2025-09-28 20:00:00-04	198611	196	\N
2187	14	2025-09-29 20:00:00-04	199585	974	\N
2188	15	2025-08-31 20:00:00-04	151608	0	\N
2189	15	2025-09-01 20:00:00-04	152460	852	\N
2190	15	2025-09-02 20:00:00-04	152808	348	\N
2191	15	2025-09-03 20:00:00-04	153315	507	\N
2192	15	2025-09-04 20:00:00-04	154398	1083	\N
2193	15	2025-09-07 20:00:00-04	154669	271	\N
2194	15	2025-09-08 20:00:00-04	155599	930	\N
2195	15	2025-09-09 20:00:00-04	155943	344	\N
2196	15	2025-09-10 20:00:00-04	156177	234	\N
2197	15	2025-09-11 20:00:00-04	157161	984	\N
2198	15	2025-09-14 20:00:00-04	157361	200	\N
2199	15	2025-09-15 20:00:00-04	158161	800	MANTENIMIENTO EXYCOPY
2200	15	2025-09-16 20:00:00-04	159005	844	\N
2201	15	2025-09-17 20:00:00-04	159857	852	\N
2202	15	2025-09-18 20:00:00-04	160461	604	\N
2203	15	2025-09-21 20:00:00-04	161039	578	\N
2204	15	2025-09-22 20:00:00-04	162193	1154	\N
2205	15	2025-09-23 20:00:00-04	162968	775	\N
2206	15	2025-09-24 20:00:00-04	163748	780	\N
2207	15	2025-09-25 20:00:00-04	164417	669	\N
2208	15	2025-09-28 20:00:00-04	164625	208	\N
2209	15	2025-09-29 20:00:00-04	165241	616	\N
2210	16	2025-08-31 20:00:00-04	178608	0	\N
2211	16	2025-09-01 20:00:00-04	178687	79	\N
2212	16	2025-09-02 20:00:00-04	178767	80	\N
2213	16	2025-09-03 20:00:00-04	178861	94	\N
2214	16	2025-09-04 20:00:00-04	178948	87	\N
2215	16	2025-09-07 20:00:00-04	179089	141	CAMBIO TONER
2216	16	2025-09-08 20:00:00-04	179172	83	\N
2217	16	2025-09-09 20:00:00-04	179262	90	\N
2218	16	2025-09-10 20:00:00-04	179398	136	\N
2219	16	2025-09-11 20:00:00-04	179522	124	\N
2220	16	2025-09-14 20:00:00-04	179594	72	\N
2221	16	2025-09-15 20:00:00-04	179729	135	\N
2222	16	2025-09-16 20:00:00-04	179821	92	\N
2223	16	2025-09-17 20:00:00-04	179911	90	\N
2224	16	2025-09-18 20:00:00-04	179987	76	\N
2225	16	2025-09-21 20:00:00-04	180172	185	MANTENIMIENTO EXYCOPY
2226	16	2025-09-22 20:00:00-04	180299	127	\N
2227	16	2025-09-23 20:00:00-04	180428	129	\N
2228	16	2025-09-24 20:00:00-04	180508	80	\N
2229	16	2025-09-25 20:00:00-04	180582	74	\N
2230	16	2025-09-28 20:00:00-04	180720	138	\N
2231	16	2025-09-29 20:00:00-04	180814	94	\N
2232	17	2025-08-31 20:00:00-04	0	0	\N
2233	17	2025-09-01 20:00:00-04	0	0	\N
2234	17	2025-09-02 20:00:00-04	0	0	\N
2235	17	2025-09-03 20:00:00-04	0	0	\N
2236	17	2025-09-04 20:00:00-04	0	0	\N
2237	17	2025-09-07 20:00:00-04	0	0	\N
2238	17	2025-09-08 20:00:00-04	0	0	NUEVA
2239	17	2025-09-09 20:00:00-04	191	191	\N
2240	17	2025-09-10 20:00:00-04	283	92	\N
2241	17	2025-09-11 20:00:00-04	343	60	\N
2242	17	2025-09-14 20:00:00-04	788	445	\N
2243	17	2025-09-15 20:00:00-04	1180	392	\N
2244	17	2025-09-16 20:00:00-04	1283	103	\N
2245	17	2025-09-17 20:00:00-04	1410	127	\N
2246	17	2025-09-18 20:00:00-04	1579	169	\N
2247	17	2025-09-21 20:00:00-04	1650	71	\N
2248	17	2025-09-22 20:00:00-04	1746	96	\N
2249	17	2025-09-23 20:00:00-04	1781	35	\N
2250	17	2025-09-24 20:00:00-04	1825	44	\N
2251	17	2025-09-25 20:00:00-04	1840	15	\N
2252	17	2025-09-28 20:00:00-04	1840	0	\N
2253	17	2025-09-29 20:00:00-04	2018	178	\N
2254	14	2025-09-30 20:00:00-04	200674	0	\N
2255	14	2025-10-01 20:00:00-04	200674	0	\N
2256	14	2025-10-02 20:00:00-04	200674	0	\N
2257	14	2025-10-05 20:00:00-04	200674	0	\N
2258	14	2025-10-06 20:00:00-04	201223	549	\N
2259	14	2025-10-07 20:00:00-04	201753	530	\N
2260	14	2025-10-08 20:00:00-04	202393	640	\N
2261	14	2025-10-09 20:00:00-04	202843	450	\N
2262	14	2025-10-12 20:00:00-04	203338	495	\N
2263	14	2025-10-13 20:00:00-04	203870	532	\N
2264	14	2025-10-14 20:00:00-04	204750	880	\N
2265	14	2025-10-15 20:00:00-04	205634	884	\N
2266	14	2025-10-16 20:00:00-04	206254	620	\N
2267	14	2025-10-19 20:00:00-04	206651	397	\N
2268	14	2025-10-20 20:00:00-04	207237	586	\N
2269	14	2025-10-21 20:00:00-04	207934	697	\N
2270	14	2025-10-22 20:00:00-04	208982	1048	\N
2271	14	2025-10-23 20:00:00-04	210217	1235	\N
2272	14	2025-10-26 20:00:00-04	210997	780	\N
2273	14	2025-10-27 20:00:00-04	211665	668	\N
2274	14	2025-10-28 20:00:00-04	212895	1230	\N
2275	14	2025-10-29 20:00:00-04	213530	635	\N
2276	14	2025-10-30 20:00:00-04	213964	434	\N
2277	15	2025-09-30 20:00:00-04	165870	0	\N
2278	15	2025-10-01 20:00:00-04	166769	899	\N
2279	15	2025-10-02 20:00:00-04	167626	857	\N
2280	15	2025-10-05 20:00:00-04	168724	1098	\N
2281	15	2025-10-06 20:00:00-04	169232	508	\N
2282	15	2025-10-07 20:00:00-04	171121	1889	\N
2283	15	2025-10-08 20:00:00-04	171234	113	\N
2284	15	2025-10-09 20:00:00-04	171332	98	\N
2285	15	2025-10-12 20:00:00-04	171701	369	CAMBIO TONER
2286	15	2025-10-13 20:00:00-04	172573	872	\N
2287	15	2025-10-14 20:00:00-04	173262	689	\N
2288	15	2025-10-15 20:00:00-04	174161	899	\N
2289	15	2025-10-16 20:00:00-04	174698	537	\N
2290	15	2025-10-19 20:00:00-04	175415	717	\N
2291	15	2025-10-20 20:00:00-04	176325	910	\N
2292	15	2025-10-21 20:00:00-04	176987	662	\N
2293	15	2025-10-22 20:00:00-04	177961	974	\N
2294	15	2025-10-23 20:00:00-04	179147	1186	\N
2295	15	2025-10-26 20:00:00-04	179965	818	CAMBIO TONER POR DAÑOS EN EL ANTERIOR
2296	15	2025-10-27 20:00:00-04	181552	1587	\N
2297	15	2025-10-28 20:00:00-04	182668	1116	\N
2298	15	2025-10-29 20:00:00-04	183666	998	CAMBIO TONER POR DAÑOS EN EL ANTERIOR
2299	15	2025-10-30 20:00:00-04	184749	1083	\N
2300	16	2025-09-30 20:00:00-04	180926	0	\N
2301	16	2025-10-01 20:00:00-04	181017	91	\N
2302	16	2025-10-02 20:00:00-04	181086	69	\N
2303	16	2025-10-05 20:00:00-04	181266	180	\N
2304	16	2025-10-06 20:00:00-04	181338	72	\N
2305	16	2025-10-07 20:00:00-04	181434	96	\N
2306	16	2025-10-08 20:00:00-04	181538	104	\N
2307	16	2025-10-09 20:00:00-04	181585	47	\N
2308	16	2025-10-12 20:00:00-04	181748	163	\N
2309	16	2025-10-13 20:00:00-04	181800	52	\N
2310	16	2025-10-14 20:00:00-04	181906	106	\N
2311	16	2025-10-15 20:00:00-04	181993	87	\N
2312	16	2025-10-16 20:00:00-04	182135	142	\N
2313	16	2025-10-19 20:00:00-04	182295	160	CAMBIO TONER
2314	16	2025-10-20 20:00:00-04	182449	154	\N
2315	16	2025-10-21 20:00:00-04	182525	76	\N
2316	16	2025-10-22 20:00:00-04	182629	104	\N
2317	16	2025-10-23 20:00:00-04	182752	123	\N
2318	16	2025-10-26 20:00:00-04	182922	170	\N
2319	16	2025-10-27 20:00:00-04	183054	132	\N
2320	16	2025-10-28 20:00:00-04	183149	95	\N
2321	16	2025-10-29 20:00:00-04	183264	115	\N
2322	16	2025-10-30 20:00:00-04	183346	82	\N
2323	17	2025-09-30 20:00:00-04	2135	0	\N
2324	17	2025-10-01 20:00:00-04	2198	63	\N
2325	17	2025-10-02 20:00:00-04	2238	40	\N
2326	17	2025-10-05 20:00:00-04	2387	149	\N
2327	17	2025-10-06 20:00:00-04	2569	182	\N
2328	17	2025-10-07 20:00:00-04	2973	404	\N
2329	17	2025-10-08 20:00:00-04	3052	79	\N
2330	17	2025-10-09 20:00:00-04	3158	106	\N
2331	17	2025-10-12 20:00:00-04	3211	53	\N
2332	17	2025-10-13 20:00:00-04	3241	30	\N
2333	17	2025-10-14 20:00:00-04	3362	121	\N
2334	17	2025-10-15 20:00:00-04	3458	96	\N
2335	17	2025-10-16 20:00:00-04	3969	511	\N
2336	17	2025-10-19 20:00:00-04	4066	97	\N
2337	17	2025-10-20 20:00:00-04	4319	253	\N
2338	17	2025-10-21 20:00:00-04	5068	749	\N
2339	17	2025-10-22 20:00:00-04	5605	537	\N
2340	17	2025-10-23 20:00:00-04	5938	333	\N
2341	17	2025-10-26 20:00:00-04	6253	315	\N
2342	17	2025-10-27 20:00:00-04	6310	57	\N
2343	17	2025-10-28 20:00:00-04	6457	147	\N
2344	17	2025-10-29 20:00:00-04	6493	36	\N
2345	17	2025-10-30 20:00:00-04	6526	33	\N
2346	3	2025-11-19 20:00:00-04	140360	86	\N
2347	3	2025-11-20 20:00:00-04	140419	59	\N
2348	9	2025-11-19 20:00:00-04	61262	15	\N
2349	9	2025-11-20 20:00:00-04	61301	39	\N
2350	5	2025-11-19 20:00:00-04	61324	214	CAMBIO TONER
2351	5	2025-11-20 20:00:00-04	61633	309	\N
2352	11	2025-11-19 20:00:00-04	33583	3	\N
2353	11	2025-11-20 20:00:00-04	33592	9	\N
2354	10	2025-11-19 20:00:00-04	43149	237	\N
2355	10	2025-11-20 20:00:00-04	43340	191	\N
2356	14	2025-11-02 20:00:00-04	214684	0	\N
2357	14	2025-11-03 20:00:00-04	215222	538	\N
2358	14	2025-11-04 20:00:00-04	215669	447	\N
2359	14	2025-11-05 20:00:00-04	216002	333	\N
2360	14	2025-11-06 20:00:00-04	216872	870	\N
2361	14	2025-11-09 20:00:00-04	217439	567	\N
2362	14	2025-11-10 20:00:00-04	217766	327	\N
2363	14	2025-11-11 20:00:00-04	218472	706	\N
2364	14	2025-11-12 20:00:00-04	219012	540	\N
2365	14	2025-11-13 20:00:00-04	219461	449	\N
2366	14	2025-11-19 20:00:00-04	220089	628	\N
2367	14	2025-11-20 20:00:00-04	222158	2069	\N
2368	14	2025-11-16 20:00:00-04	222535	377	\N
2369	14	2025-11-17 20:00:00-04	223237	702	\N
2370	14	2025-11-18 20:00:00-04	223271	34	\N
2371	14	2025-11-23 20:00:00-04	224166	895	CAMBIO TONER
2372	14	2025-11-24 20:00:00-04	225908	1742	\N
2373	14	2025-11-25 20:00:00-04	226927	1019	\N
2374	14	2025-11-26 20:00:00-04	227803	876	\N
2375	14	2025-11-27 20:00:00-04	228661	858	\N
2376	15	2025-11-02 20:00:00-04	185783	0	\N
2377	15	2025-11-03 20:00:00-04	186392	609	\N
2378	15	2025-11-04 20:00:00-04	186930	538	\N
2379	15	2025-11-05 20:00:00-04	187948	1018	\N
2380	15	2025-11-06 20:00:00-04	189076	1128	\N
2381	15	2025-11-09 20:00:00-04	189746	670	\N
2382	15	2025-11-10 20:00:00-04	190327	581	\N
2383	15	2025-11-11 20:00:00-04	191125	798	CAMBIO TONER DAÑADO
2384	15	2025-11-12 20:00:00-04	191748	623	\N
2385	15	2025-11-13 20:00:00-04	192335	587	\N
2386	15	2025-11-19 20:00:00-04	192973	638	\N
2387	15	2025-11-20 20:00:00-04	193804	831	\N
2388	15	2025-11-16 20:00:00-04	194690	886	\N
2389	15	2025-11-17 20:00:00-04	195548	858	\N
2390	15	2025-11-18 20:00:00-04	197174	1626	\N
2391	15	2025-11-23 20:00:00-04	198034	860	\N
2392	15	2025-11-24 20:00:00-04	199194	1160	\N
2393	15	2025-11-25 20:00:00-04	199904	710	\N
2394	15	2025-11-26 20:00:00-04	200721	817	\N
2395	15	2025-11-27 20:00:00-04	201528	807	\N
2396	16	2025-11-02 20:00:00-04	183525	0	\N
2397	16	2025-11-03 20:00:00-04	183631	106	CAMBIO TONER
2398	16	2025-11-04 20:00:00-04	183718	87	\N
2399	16	2025-11-05 20:00:00-04	183814	96	\N
2400	16	2025-11-06 20:00:00-04	183944	130	\N
2401	16	2025-11-09 20:00:00-04	184151	207	\N
2402	16	2025-11-10 20:00:00-04	184220	69	\N
2403	16	2025-11-11 20:00:00-04	184290	70	\N
2404	16	2025-11-12 20:00:00-04	184422	132	\N
2405	16	2025-11-13 20:00:00-04	184491	69	\N
2406	16	2025-11-19 20:00:00-04	184691	200	\N
2407	16	2025-11-20 20:00:00-04	184801	110	\N
2408	16	2025-11-16 20:00:00-04	184955	154	\N
2409	16	2025-11-17 20:00:00-04	185104	149	\N
2410	16	2025-11-18 20:00:00-04	185175	71	CAMBIO TONER
2411	16	2025-11-23 20:00:00-04	185398	223	\N
2412	16	2025-11-24 20:00:00-04	185506	108	\N
2413	16	2025-11-25 20:00:00-04	185616	110	\N
2414	16	2025-11-26 20:00:00-04	185750	134	\N
2415	16	2025-11-27 20:00:00-04	185859	109	\N
2416	17	2025-11-02 20:00:00-04	6572	0	\N
2417	17	2025-11-03 20:00:00-04	6632	60	\N
2418	17	2025-11-04 20:00:00-04	6802	170	\N
2419	17	2025-11-05 20:00:00-04	6901	99	\N
2420	17	2025-11-06 20:00:00-04	6996	95	\N
2421	17	2025-11-09 20:00:00-04	7051	55	\N
2422	17	2025-11-10 20:00:00-04	7118	67	\N
2423	17	2025-11-11 20:00:00-04	7184	66	\N
2424	17	2025-11-12 20:00:00-04	7222	38	\N
2425	17	2025-11-13 20:00:00-04	7475	253	\N
2426	17	2025-11-19 20:00:00-04	7601	126	\N
2427	17	2025-11-20 20:00:00-04	7730	129	\N
2428	17	2025-11-16 20:00:00-04	7801	71	\N
2429	17	2025-11-17 20:00:00-04	7919	118	\N
2430	17	2025-11-18 20:00:00-04	8080	161	\N
2431	17	2025-11-23 20:00:00-04	8182	102	CAMBIO TONER
2432	17	2025-11-24 20:00:00-04	8263	81	\N
2433	17	2025-11-25 20:00:00-04	8502	239	\N
2434	17	2025-11-26 20:00:00-04	8528	26	\N
2435	17	2025-11-27 20:00:00-04	8718	190	\N
2436	14	2025-11-30 20:00:00-04	229411	0	\N
2437	14	2025-12-01 20:00:00-04	229855	444	\N
2438	14	2025-12-02 20:00:00-04	230383	528	\N
2439	14	2025-12-03 20:00:00-04	230759	376	\N
2440	14	2025-12-04 20:00:00-04	231309	550	\N
2441	14	2025-12-07 20:00:00-04	231970	661	\N
2442	14	2025-12-08 20:00:00-04	232644	674	\N
2443	14	2025-12-09 20:00:00-04	233120	476	\N
2444	14	2025-12-10 20:00:00-04	233924	804	\N
2445	14	2025-12-11 20:00:00-04	234654	730	\N
2446	14	2025-12-14 20:00:00-04	235207	553	CAMBIO TONER
2447	14	2025-12-15 20:00:00-04	235806	599	\N
2448	14	2025-12-16 20:00:00-04	236469	663	\N
2449	14	2025-12-17 20:00:00-04	236970	501	\N
2450	14	2025-12-18 20:00:00-04	237236	266	\N
2451	14	2025-12-21 20:00:00-04	237561	325	\N
2452	14	2025-12-22 20:00:00-04	237794	233	\N
2453	14	2025-12-23 20:00:00-04	237794	0	FERIADO
2454	14	2025-12-24 20:00:00-04	237794	0	FERIADO
2455	14	2025-12-25 20:00:00-04	238048	254	\N
2456	14	2025-12-28 20:00:00-04	238227	179	\N
2457	14	2025-12-29 20:00:00-04	238404	177	\N
2458	14	2025-12-30 20:00:00-04	238515	111	FERIADO
2459	15	2025-11-30 20:00:00-04	203839	0	\N
2460	15	2025-12-01 20:00:00-04	204250	411	\N
2461	15	2025-12-02 20:00:00-04	205060	810	\N
2462	15	2025-12-03 20:00:00-04	205552	492	\N
2463	15	2025-12-04 20:00:00-04	206264	712	\N
2464	15	2025-12-07 20:00:00-04	206770	506	\N
2465	15	2025-12-08 20:00:00-04	207228	458	\N
2466	15	2025-12-09 20:00:00-04	208284	1056	\N
2467	15	2025-12-10 20:00:00-04	208758	474	\N
2468	15	2025-12-11 20:00:00-04	209001	243	\N
2469	15	2025-12-14 20:00:00-04	209809	808	\N
2470	15	2025-12-15 20:00:00-04	210362	553	\N
2471	15	2025-12-16 20:00:00-04	210817	455	\N
2472	15	2025-12-17 20:00:00-04	211303	486	\N
2473	15	2025-12-18 20:00:00-04	211646	343	\N
2474	15	2025-12-21 20:00:00-04	211973	327	\N
2475	15	2025-12-22 20:00:00-04	212399	426	\N
2476	15	2025-12-23 20:00:00-04	212399	0	FERIADO
2477	15	2025-12-24 20:00:00-04	212399	0	FERIADO
2478	15	2025-12-25 20:00:00-04	212520	121	\N
2479	15	2025-12-28 20:00:00-04	212710	190	\N
2480	15	2025-12-29 20:00:00-04	212973	263	\N
2481	15	2025-12-30 20:00:00-04	213288	315	FERIADO
2482	16	2025-11-30 20:00:00-04	186042	0	\N
2483	16	2025-12-01 20:00:00-04	186130	88	\N
2484	16	2025-12-02 20:00:00-04	186202	72	\N
2485	16	2025-12-03 20:00:00-04	186226	24	\N
2486	16	2025-12-04 20:00:00-04	186284	58	\N
2487	16	2025-12-07 20:00:00-04	186447	163	\N
2488	16	2025-12-08 20:00:00-04	186511	64	\N
2489	16	2025-12-09 20:00:00-04	186620	109	\N
2490	16	2025-12-10 20:00:00-04	186725	105	\N
2491	16	2025-12-11 20:00:00-04	186825	100	\N
2492	16	2025-12-14 20:00:00-04	186936	111	\N
2493	16	2025-12-15 20:00:00-04	187013	77	\N
2494	16	2025-12-16 20:00:00-04	187075	62	\N
2495	16	2025-12-17 20:00:00-04	187175	100	\N
2496	16	2025-12-18 20:00:00-04	187217	42	\N
2497	16	2025-12-21 20:00:00-04	187298	81	\N
2498	16	2025-12-22 20:00:00-04	187403	105	\N
2499	16	2025-12-23 20:00:00-04	187403	0	FERIADO
2500	16	2025-12-24 20:00:00-04	187403	0	FERIADO
2501	16	2025-12-25 20:00:00-04	187462	59	\N
2502	16	2025-12-28 20:00:00-04	187500	38	\N
2503	16	2025-12-29 20:00:00-04	187524	24	\N
2504	16	2025-12-30 20:00:00-04	187549	25	FERIADO
2505	17	2025-11-30 20:00:00-04	8772	0	\N
2506	17	2025-12-01 20:00:00-04	8872	100	\N
2507	17	2025-12-02 20:00:00-04	9025	153	\N
2508	17	2025-12-03 20:00:00-04	9107	82	\N
2509	17	2025-12-04 20:00:00-04	9186	79	\N
2510	17	2025-12-07 20:00:00-04	9483	297	\N
2511	17	2025-12-08 20:00:00-04	9552	69	\N
2512	17	2025-12-09 20:00:00-04	9783	231	\N
2513	17	2025-12-10 20:00:00-04	9809	26	\N
2514	17	2025-12-11 20:00:00-04	10008	199	\N
2515	17	2025-12-14 20:00:00-04	10143	135	\N
2516	17	2025-12-15 20:00:00-04	10215	72	\N
2517	17	2025-12-16 20:00:00-04	10281	66	\N
2518	17	2025-12-17 20:00:00-04	10326	45	\N
2519	17	2025-12-18 20:00:00-04	10676	350	\N
2520	17	2025-12-21 20:00:00-04	11019	343	\N
2521	17	2025-12-22 20:00:00-04	11091	72	\N
2522	17	2025-12-23 20:00:00-04	11091	0	FERIADO
2523	17	2025-12-24 20:00:00-04	11091	0	FERIADO
2524	17	2025-12-25 20:00:00-04	11102	11	\N
2525	17	2025-12-28 20:00:00-04	11546	444	\N
2526	17	2025-12-29 20:00:00-04	11946	400	\N
2527	17	2025-12-30 20:00:00-04	12000	54	FERIADO
2528	14	2026-01-04 20:00:00-04	238515	0	\N
2529	14	2026-01-05 20:00:00-04	238961	446	\N
2530	14	2026-01-06 20:00:00-04	239754	793	\N
2531	14	2026-01-07 20:00:00-04	240326	572	\N
2532	14	2026-01-08 20:00:00-04	240678	352	\N
2533	14	2026-01-11 20:00:00-04	241242	564	\N
2534	14	2026-01-12 20:00:00-04	241984	742	\N
2535	14	2026-01-13 20:00:00-04	242753	769	\N
2536	14	2026-01-14 20:00:00-04	243420	667	\N
2537	14	2026-01-15 20:00:00-04	244151	731	\N
2538	14	2026-01-18 20:00:00-04	244444	293	\N
2539	14	2026-01-19 20:00:00-04	244894	450	\N
2540	14	2026-01-20 20:00:00-04	245499	605	\N
2541	14	2026-01-21 20:00:00-04	246313	814	\N
2542	14	2026-01-22 20:00:00-04	246869	556	INACTIVA POR TONER
2543	14	2026-01-25 20:00:00-04	246869	0	INACTIVA POR TONER
2544	14	2026-01-26 20:00:00-04	247557	688	TONER INSTALADO
2545	14	2026-01-27 20:00:00-04	248171	614	\N
2546	14	2026-01-28 20:00:00-04	248969	798	\N
2547	14	2026-01-29 20:00:00-04	249641	672	\N
2548	15	2026-01-04 20:00:00-04	213526	0	\N
2549	15	2026-01-05 20:00:00-04	213526	0	\N
2550	15	2026-01-06 20:00:00-04	213526	0	\N
2551	15	2026-01-07 20:00:00-04	213530	4	\N
2552	15	2026-01-08 20:00:00-04	213532	2	\N
2553	15	2026-01-11 20:00:00-04	213532	0	\N
2554	15	2026-01-12 20:00:00-04	213688	156	\N
2555	15	2026-01-13 20:00:00-04	214238	550	\N
2556	15	2026-01-14 20:00:00-04	214832	594	\N
2557	15	2026-01-15 20:00:00-04	215514	682	\N
2558	15	2026-01-18 20:00:00-04	215796	282	\N
2559	15	2026-01-19 20:00:00-04	216785	989	\N
2560	15	2026-01-20 20:00:00-04	217649	864	\N
2561	15	2026-01-21 20:00:00-04	218602	953	CAMBIO TONER DAÑADO
2562	15	2026-01-22 20:00:00-04	219256	654	\N
2563	15	2026-01-25 20:00:00-04	220059	803	\N
2564	15	2026-01-26 20:00:00-04	221331	1272	\N
2565	15	2026-01-27 20:00:00-04	222203	872	\N
2566	15	2026-01-28 20:00:00-04	223133	930	\N
2567	15	2026-01-29 20:00:00-04	224026	893	CAMBIO TONER DAÑADO
2568	16	2026-01-04 20:00:00-04	187573	0	\N
2569	16	2026-01-05 20:00:00-04	187623	50	\N
2570	16	2026-01-06 20:00:00-04	187638	15	\N
2571	16	2026-01-07 20:00:00-04	187683	45	\N
2572	16	2026-01-08 20:00:00-04	187734	51	\N
2573	16	2026-01-11 20:00:00-04	187760	26	\N
2574	16	2026-01-12 20:00:00-04	187810	50	\N
2575	16	2026-01-13 20:00:00-04	187893	83	\N
2576	16	2026-01-14 20:00:00-04	187957	64	\N
2577	16	2026-01-15 20:00:00-04	188061	104	\N
2578	16	2026-01-18 20:00:00-04	188178	117	\N
2579	16	2026-01-19 20:00:00-04	188325	147	\N
2580	16	2026-01-20 20:00:00-04	188470	145	\N
2581	16	2026-01-21 20:00:00-04	188606	136	\N
2582	16	2026-01-22 20:00:00-04	188805	199	\N
2583	16	2026-01-25 20:00:00-04	188969	164	\N
2584	16	2026-01-26 20:00:00-04	189248	279	\N
2585	16	2026-01-27 20:00:00-04	189367	119	\N
2586	16	2026-01-28 20:00:00-04	189468	101	\N
2587	16	2026-01-29 20:00:00-04	189626	158	CAMBIO TONER
2588	17	2026-01-04 20:00:00-04	12031	0	\N
2589	17	2026-01-05 20:00:00-04	12103	72	\N
2590	17	2026-01-06 20:00:00-04	12227	124	\N
2591	17	2026-01-07 20:00:00-04	12250	23	\N
2592	17	2026-01-08 20:00:00-04	12989	739	\N
2593	17	2026-01-11 20:00:00-04	13078	89	\N
2594	17	2026-01-12 20:00:00-04	13200	122	\N
2595	17	2026-01-13 20:00:00-04	13254	54	\N
2596	17	2026-01-14 20:00:00-04	13336	82	\N
2597	17	2026-01-15 20:00:00-04	13464	128	\N
2598	17	2026-01-18 20:00:00-04	13571	107	\N
2599	17	2026-01-19 20:00:00-04	13617	46	\N
2600	17	2026-01-20 20:00:00-04	13751	134	\N
2601	17	2026-01-21 20:00:00-04	13830	79	\N
2602	17	2026-01-22 20:00:00-04	14139	309	\N
2603	17	2026-01-25 20:00:00-04	14326	187	\N
2604	17	2026-01-26 20:00:00-04	14353	27	\N
2605	17	2026-01-27 20:00:00-04	14511	158	\N
2606	17	2026-01-28 20:00:00-04	14636	125	\N
2607	17	2026-01-29 20:00:00-04	14714	78	\N
2608	14	2026-02-01 20:00:00-04	250576	0	\N
2609	14	2026-02-02 20:00:00-04	251268	692	\N
2610	14	2026-02-03 20:00:00-04	252178	910	\N
2611	14	2026-02-04 20:00:00-04	252968	790	\N
2612	14	2026-02-05 20:00:00-04	254020	1052	\N
2613	14	2026-02-08 20:00:00-04	254700	680	\N
2614	14	2026-02-09 20:00:00-04	255534	834	\N
2615	14	2026-02-10 20:00:00-04	256249	715	\N
2616	14	2026-02-11 20:00:00-04	256763	514	\N
2617	14	2026-02-12 20:00:00-04	257439	676	\N
2618	14	2026-02-15 20:00:00-04	257439	0	FERIADO
2619	14	2026-02-16 20:00:00-04	257439	0	FERIADO
2620	14	2026-02-17 20:00:00-04	258073	634	\N
2621	14	2026-02-18 20:00:00-04	258800	727	\N
2622	14	2026-02-19 20:00:00-04	259311	511	\N
2623	14	2026-02-22 20:00:00-04	260054	743	\N
2624	14	2026-02-23 20:00:00-04	260086	32	PENDIENTE POR MANTENIMIENTO
2625	14	2026-02-24 20:00:00-04	260161	75	PENDIENTE POR MANTENIMIENTO
2626	14	2026-02-25 20:00:00-04	260315	154	PENDIENTE POR MANTENIMIENTO
2627	14	2026-02-26 20:00:00-04	260484	169	PENDIENTE POR MANTENIMIENTO
2628	15	2026-02-01 20:00:00-04	224982	0	DAÑADA (EN REPARACION)
2629	15	2026-02-02 20:00:00-04	224982	0	DAÑADA (EN REPARACION)
2630	15	2026-02-03 20:00:00-04	225156	174	INSTALADA
2631	15	2026-02-04 20:00:00-04	225907	751	\N
2632	15	2026-02-05 20:00:00-04	226594	687	\N
2633	15	2026-02-08 20:00:00-04	227290	696	\N
2634	15	2026-02-09 20:00:00-04	228078	788	\N
2635	15	2026-02-10 20:00:00-04	228840	762	\N
2636	15	2026-02-11 20:00:00-04	229803	963	CAMBIO TONER DAÑADO (YA SE CONVERSO CON PROVEEDOR PARA REEMPLAZO)
2637	15	2026-02-12 20:00:00-04	231053	1250	\N
2638	15	2026-02-15 20:00:00-04	231053	0	FERIADO
2639	15	2026-02-16 20:00:00-04	231053	0	FERIADO
2640	15	2026-02-17 20:00:00-04	232465	1412	\N
2641	15	2026-02-18 20:00:00-04	233729	1264	\N
2642	15	2026-02-19 20:00:00-04	234430	701	\N
2643	15	2026-02-22 20:00:00-04	235699	1269	\N
2644	15	2026-02-23 20:00:00-04	236786	1087	\N
2645	15	2026-02-24 20:00:00-04	237980	1194	CAMBIO TONER DAÑADO (YA SE CONVERSO CON PROVEEDOR PARA REEMPLAZO)
2646	15	2026-02-25 20:00:00-04	239016	1036	\N
2647	15	2026-02-26 20:00:00-04	239929	913	\N
2648	16	2026-02-01 20:00:00-04	189871	0	\N
2649	16	2026-02-02 20:00:00-04	189969	98	\N
2650	16	2026-02-03 20:00:00-04	190073	104	\N
2651	16	2026-02-04 20:00:00-04	190172	99	\N
2652	16	2026-02-05 20:00:00-04	190265	93	\N
2653	16	2026-02-08 20:00:00-04	190478	213	\N
2654	16	2026-02-09 20:00:00-04	190583	105	\N
2655	16	2026-02-10 20:00:00-04	190715	132	\N
2656	16	2026-02-11 20:00:00-04	190844	129	\N
2657	16	2026-02-12 20:00:00-04	190960	116	\N
2658	16	2026-02-15 20:00:00-04	190960	0	FERIADO
2659	16	2026-02-16 20:00:00-04	190960	0	FERIADO
2660	16	2026-02-17 20:00:00-04	191167	207	\N
2661	16	2026-02-18 20:00:00-04	191356	189	CAMBIO TONER
2662	16	2026-02-19 20:00:00-04	191463	107	\N
2663	16	2026-02-22 20:00:00-04	191681	218	\N
2664	16	2026-02-23 20:00:00-04	191769	88	\N
2665	16	2026-02-24 20:00:00-04	191823	54	\N
2666	16	2026-02-25 20:00:00-04	191895	72	\N
2667	16	2026-02-26 20:00:00-04	191931	36	\N
2668	17	2026-02-01 20:00:00-04	14793	0	\N
2669	17	2026-02-02 20:00:00-04	14844	51	\N
2670	17	2026-02-03 20:00:00-04	14947	103	\N
2671	17	2026-02-04 20:00:00-04	15067	120	\N
2672	17	2026-02-05 20:00:00-04	15400	333	\N
2673	17	2026-02-08 20:00:00-04	15658	258	\N
2674	17	2026-02-09 20:00:00-04	15720	62	\N
2675	17	2026-02-10 20:00:00-04	15817	97	\N
2676	17	2026-02-11 20:00:00-04	15970	153	\N
2677	17	2026-02-12 20:00:00-04	16072	102	\N
2678	17	2026-02-15 20:00:00-04	16072	0	FERIADO
2679	17	2026-02-16 20:00:00-04	16072	0	FERIADO
2680	17	2026-02-17 20:00:00-04	16142	70	\N
2681	17	2026-02-18 20:00:00-04	16209	67	\N
2682	17	2026-02-19 20:00:00-04	16336	127	\N
2683	17	2026-02-22 20:00:00-04	16709	373	\N
2684	17	2026-02-23 20:00:00-04	16897	188	\N
2685	17	2026-02-24 20:00:00-04	16920	23	\N
2686	17	2026-02-25 20:00:00-04	17041	121	\N
2687	17	2026-02-26 20:00:00-04	17114	73	\N
2688	14	2026-03-01 20:00:00-04	260392	0	PENDIENTE POR MANTENIMIENTO
2689	14	2026-03-02 20:00:00-04	260392	0	PENDIENTE POR MANTENIMIENTO
2690	14	2026-03-03 20:00:00-04	260392	0	PENDIENTE POR MANTENIMIENTO
2691	14	2026-03-04 20:00:00-04	260392	0	MANTENIMIENTO EXICOPY
2692	14	2026-03-05 20:00:00-04	261405	1013	\N
2693	14	2026-03-08 20:00:00-04	261647	242	\N
2694	14	2026-03-09 20:00:00-04	261716	69	\N
2695	14	2026-03-10 20:00:00-04	261888	172	\N
2696	14	2026-03-11 20:00:00-04	0	0	\N
2697	14	2026-03-12 20:00:00-04	0	0	\N
2698	14	2026-03-15 20:00:00-04	0	0	\N
2699	14	2026-03-16 20:00:00-04	0	0	\N
2700	14	2026-03-17 20:00:00-04	0	0	\N
2701	14	2026-03-18 20:00:00-04	0	0	\N
2702	14	2026-03-19 20:00:00-04	0	0	\N
2703	14	2026-03-22 20:00:00-04	0	0	\N
2704	14	2026-03-23 20:00:00-04	0	0	\N
2705	14	2026-03-24 20:00:00-04	0	0	\N
2706	14	2026-03-25 20:00:00-04	0	0	\N
2707	14	2026-03-26 20:00:00-04	0	0	\N
2708	14	2026-03-29 20:00:00-04	0	0	\N
2709	14	2026-03-30 20:00:00-04	0	0	\N
2710	15	2026-03-01 20:00:00-04	241385	0	\N
2711	15	2026-03-02 20:00:00-04	242105	720	\N
2712	15	2026-03-03 20:00:00-04	242905	800	CAMBIO TONER
2713	15	2026-03-04 20:00:00-04	243536	631	\N
2714	15	2026-03-05 20:00:00-04	244472	936	\N
2715	15	2026-03-08 20:00:00-04	245716	1244	\N
2716	15	2026-03-09 20:00:00-04	246398	682	\N
2717	15	2026-03-10 20:00:00-04	247246	848	\N
2718	15	2026-03-11 20:00:00-04	0	0	\N
2719	15	2026-03-12 20:00:00-04	0	0	\N
2720	15	2026-03-15 20:00:00-04	0	0	\N
2721	15	2026-03-16 20:00:00-04	0	0	\N
2722	15	2026-03-17 20:00:00-04	0	0	\N
2723	15	2026-03-18 20:00:00-04	0	0	\N
2724	15	2026-03-19 20:00:00-04	0	0	\N
2725	15	2026-03-22 20:00:00-04	0	0	\N
2726	15	2026-03-23 20:00:00-04	0	0	\N
2727	15	2026-03-24 20:00:00-04	0	0	\N
2728	15	2026-03-25 20:00:00-04	0	0	\N
2729	15	2026-03-26 20:00:00-04	0	0	\N
2730	15	2026-03-29 20:00:00-04	0	0	\N
2731	15	2026-03-30 20:00:00-04	0	0	\N
2732	16	2026-03-01 20:00:00-04	192071	0	\N
2733	16	2026-03-02 20:00:00-04	192119	48	\N
2734	16	2026-03-03 20:00:00-04	192203	84	\N
2735	16	2026-03-04 20:00:00-04	192283	80	MANTENIMIENTO EXICOPY
2736	16	2026-03-05 20:00:00-04	192371	88	\N
2737	16	2026-03-08 20:00:00-04	192593	222	CAMBIO TONER
2738	16	2026-03-09 20:00:00-04	192661	68	\N
2739	16	2026-03-10 20:00:00-04	192748	87	\N
2740	16	2026-03-11 20:00:00-04	0	0	\N
2741	16	2026-03-12 20:00:00-04	0	0	\N
2742	16	2026-03-15 20:00:00-04	0	0	\N
2743	16	2026-03-16 20:00:00-04	0	0	\N
2744	16	2026-03-17 20:00:00-04	0	0	\N
2745	16	2026-03-18 20:00:00-04	0	0	\N
2746	16	2026-03-19 20:00:00-04	0	0	\N
2747	16	2026-03-22 20:00:00-04	0	0	\N
2748	16	2026-03-23 20:00:00-04	0	0	\N
2749	16	2026-03-24 20:00:00-04	0	0	\N
2750	16	2026-03-25 20:00:00-04	0	0	\N
2751	16	2026-03-26 20:00:00-04	0	0	\N
2752	16	2026-03-29 20:00:00-04	0	0	\N
2753	16	2026-03-30 20:00:00-04	0	0	\N
2754	17	2026-03-01 20:00:00-04	17251	0	\N
2755	17	2026-03-02 20:00:00-04	17411	160	\N
2756	17	2026-03-03 20:00:00-04	17454	43	\N
2757	17	2026-03-04 20:00:00-04	17521	67	\N
2758	17	2026-03-05 20:00:00-04	17656	135	\N
2759	17	2026-03-08 20:00:00-04	17761	105	\N
2760	17	2026-03-09 20:00:00-04	17820	59	\N
2761	17	2026-03-10 20:00:00-04	17897	77	\N
2762	17	2026-03-11 20:00:00-04	0	0	\N
2763	17	2026-03-12 20:00:00-04	0	0	\N
2764	17	2026-03-15 20:00:00-04	0	0	\N
2765	17	2026-03-16 20:00:00-04	0	0	\N
2766	17	2026-03-17 20:00:00-04	0	0	\N
2767	17	2026-03-18 20:00:00-04	0	0	\N
2768	17	2026-03-19 20:00:00-04	0	0	\N
2769	17	2026-03-22 20:00:00-04	0	0	\N
2770	17	2026-03-23 20:00:00-04	0	0	\N
2771	17	2026-03-24 20:00:00-04	0	0	\N
2772	17	2026-03-25 20:00:00-04	0	0	\N
2773	17	2026-03-26 20:00:00-04	0	0	\N
2774	17	2026-03-29 20:00:00-04	0	0	\N
2775	17	2026-03-30 20:00:00-04	0	0	\N
\.


--
-- Data for Name: Suministros; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Suministros" ("Id", "Nombre", "Tipo", "CantidadActual", "StockMinimo", "Observacion") FROM stdin;
8	Toner 2	Tóner	1	1	\N
7	Toner 1	Tóner	1	1	Impresora canon RRHH
\.


--
-- Data for Name: Tickets; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Tickets" ("Id", "Descripcion", "Prioridad", "Estado", "FechaCreacion", "FechaCierre", "SLAHoras", "Titulo", "Departamento", "Categoria", "EquipoAfectado", "SolucionAplicada", "SuministrosUtilizados", "FechaResolucion", "Solicitante", "TelegramChatId", "CodigoNomenclatura") FROM stdin;
2	Falla de conexión en el almacén de Jadever	Media	Resuelto	2026-09-30 11:24:21.738-04	2026-09-30 12:39:28.504-04	24	Falla de conexión en el almacén	Facturación	Software	Monitor Dell	\N	\N	\N	Carlos Pérez	\N	FAC-001
3	Se cayo y ahora no prende y tampoco reacciona	Media	Abierto	2026-09-30 12:09:56.948-04	\N	24	Raton dañado	Almacén	Impresoras	Antivirus Corporativo	\N	\N	\N	María Rodríguez	\N	ALM-001
4	el dispositivo no enciende.	Media	Abierto	2026-10-07 06:52:06.573-04	\N	24	PC no enciende	Administración	Periféricos	Mouse USB	\N	\N	\N	José Martínez	\N	ADM-001
5	Lapto del jefe se crasheo y la necesita	Alta	Resuelto	2026-10-07 07:05:04.069-04	2026-10-08 04:13:33.879-04	24	Lapto no responde	Operaciones	Red	Impresora MF455	\N	\N	\N	Ana Torres	\N	OPE-001
6		Alta	Abierto	2026-10-08 04:14:24.652-04	\N	24	servicio suspendido	TI	Hardware	PC-005	\N	\N	\N	Luis Ramírez	\N	TI-001
7		Baja	Abierto	2026-10-08 04:36:13.277-04	\N	24	Raton dañado	Gerencia	Software	Monitor Dell	\N	\N	\N	Laura Gómez	\N	GER-002
9	Reporte de Francisco (Telegram):\n\nMi computadora no enciende necesito ayuda	Media	Abierto	2026-10-08 16:20:26.089318-04	\N	0	Reporte vía Telegram	Almacén	Periféricos	Mouse USB	\N	\N	\N	María Rodríguez	\N	ALM-002
1	El sistema predictivo indica agotamiento de tóner en menos de 15 días. Realizar reemplazo preventivo.	Alta	Abierto	-infinity	\N	48	Reemplazo preventivo de Tóner - MF264 Stargas	Gerencia	Hardware	PC-005	\N	\N	\N	Laura Gómez	\N	GER-001
10	Reporte de Francisco (Telegram):\n\nLa impresora se atasco	Media	Abierto	2026-10-08 16:21:11.557935-04	\N	0	Reporte vía Telegram	Administración	Red	Impresora MF455	\N	\N	\N	José Martínez	\N	ADM-002
11	Reporte de Francisco (Telegram):\n\nNecesitamos otro toner en la impresora de RRHH	Media	Abierto	2026-10-08 16:41:24.038179-04	\N	0	Reporte vía Telegram	Operaciones	Hardware	PC-005	\N	\N	\N	Ana Torres	\N	OPE-002
12	Reporte de Geraldo (Telegram):\n\nLa computadora no enciende	Media	Abierto	2026-10-08 16:44:29.390022-04	\N	0	Reporte vía Telegram	TI	Software	Monitor Dell	\N	\N	\N	Luis Ramírez	\N	TI-002
13	No quiere imprimir esta cosa	Media	Resuelto	2026-10-08 16:47:36.352938-04	2026-10-28 17:21:21.026-04	0	Reporte de Geraldo (Vía Telegram)	Gerencia	Impresoras	Antivirus Corporativo	\N	\N	\N	Laura Gómez	\N	GER-003
14	Cambio de lugar de equipos	Media	Abierto	2026-10-13 16:28:00.818-04	\N	24	Resolviendo problemas en Marketing	Facturación	Periféricos	Mouse USB	\N	\N	\N	Carlos Pérez	\N	FAC-003
16	Necesito un mouse nuevo	Media	Resuelto	2026-10-29 10:48:55.304763-04	2026-10-29 10:50:21.179-04	0	Reporte de Geraldo (Vía Telegram)	Administración	Hardware	PC-005	\N	\N	\N	José Martínez	\N	ADM-003
17	El windows del equipo esta fallando	Alta	Resuelto	2026-10-29 13:25:15.291-04	2026-10-29 13:26:26.627-04	24	Laptop con pantalla azul	Gerencia	Software	Laptop HP de la Gerente General	Se reinstalo el Windows 10 sin borrar los archivos del equipo.	Ninguno	2026-10-29 13:26:26.627-04	Laura Gómez	\N	GER-004
18	La impresora no quiere imprimir	Media	Abierto	2026-10-29 13:28:48.211129-04	\N	0	Reporte de Geraldo (Vía Telegram)	TI	Impresoras	Antivirus Corporativo	\N	\N	\N	Luis Ramírez	\N	TI-003
19	Necesito un teclado y un cable USB	Media	Abierto	2026-10-30 02:28:16.565159-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	Gerencia	Periféricos	Mouse USB	\N	\N	\N	Geraldo Unda	6393449172	GER-005
20	Pantalla en negro	Alta	Abierto	2026-10-30 02:34:47.332625-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	PSP	Red	Impresora MF455	\N	\N	\N	Geraldo Unda	6393449172	PSP-001
21	No prende el UPS	Alta	Abierto	2026-10-30 02:48:42.665279-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	PSP	Hardware	PC-005	\N	\N	\N	Geraldo Unda	6393449172	PSP-002
22	Necesito otro monitor para agregar una nueva camara de vigilancia	Alta	Abierto	2026-10-30 02:50:08.332249-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	PSP	Software	Monitor Dell	\N	\N	\N	Geraldo Unda	6393449172	PSP-003
8	Reporte de Francisco (Telegram):\n\n/start	Media	Abierto	2026-10-08 16:19:59.1528-04	\N	0	Reporte vía Telegram	Facturación	Impresoras	Antivirus Corporativo	\N	\N	\N	Carlos Pérez	\N	FAC-002
15		Alta	Resuelto	2026-10-28 17:31:29.884-04	2026-10-29 11:59:33.702-04	24	no prende el monitor	Almacén	Red	Monitor Dell	Cambio de cable dañado	Cable generico de energia	2026-10-29 11:59:33.702-04	María Rodríguez	\N	ALM-003
23	Necesito un mouse	Media	Abierto	2026-10-30 02:51:11.743379-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	Operaciones	Impresoras	Antivirus Corporativo	\N	\N	\N	Geraldo Unda	6393449172	OPE-003
24	Tienen un pendrive en TI que pueda usar?	Baja	Abierto	2026-10-30 02:52:16.174487-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Periféricos	Mouse USB	\N	\N	\N	Geraldo Unda	6393449172	TI-004
25	Cable de impresora hay?	Baja	Abierto	2026-10-30 09:40:00.073563-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Impresoras	Impresora MF455	\N	\N	\N	Geraldo Unda	6393449172	TI-005
27	Papel tipo carta hay?	Baja	Abierto	2026-10-30 09:42:55.647539-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Impresoras	Monitor Dell	\N	\N	\N	Geraldo Unda	6393449172	TI-007
28	Quedan mouse y teclados en almacén?	Baja	Abierto	2026-10-30 09:44:54.724481-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Periféricos	Antivirus Corporativo	\N	\N	\N	Geraldo Unda	6393449172	TI-008
29	Como instalo los programas de Photoshop para los de Marketing	Baja	Abierto	2026-10-30 09:46:03.521631-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Periféricos	Mouse USB	\N	\N	\N	Geraldo Unda	6393449172	TI-009
30	Necesito 3 camaras de vigilancia para el área de almacén	Media	Abierto	2026-10-30 09:50:30.097734-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	Almacén	Red	Impresora MF455	\N	\N	\N	Geraldo Unda	6393449172	ALM-004
31	Necesito 3 camaras nuevas para el área de almacén	Alta	Abierto	2026-10-30 10:37:11.851044-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	PCP	Periféricos	PC-005	\N	\N	\N	Geraldo Unda	6393449172	PCP-001
32	Me faltó 1 camara para un punto ciego en el almacén	Alta	Abierto	2026-10-30 10:37:46.363921-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	PCP	Periféricos	Monitor Dell	\N	\N	\N	Geraldo Unda	6393449172	PCP-002
26	Aún queda cables de red?	Baja	Abierto	2026-10-30 09:42:03.665127-04	\N	24	Reporte de Geraldo Unda (Vía Telegram)	TI	Red	PC-005	\N	\N	\N	Geraldo Unda	6393449172	TI-006
\.


--
-- Data for Name: Usuarios; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."Usuarios" ("Id", "NombreCompleto", "Username", "PasswordHash", "Rol", "Activo", "Cargo", "Departamento", "PrioridadPersonalizada", "TelegramUsuarioOId") FROM stdin;
1	Administrador del Sistema	admin	admin123	Admin	t	\N	\N	\N	\N
4	Carlos Pérez	carlos.perez		Empleado	t	Analista de Facturación	Facturación	\N	0414-3289104
5	María Rodríguez	maria.rodriguez		Empleado	t	Encargada de Almacén	Almacén	\N	0424-4910238
6	José Martínez	jose.martinez		Empleado	t	Asistente Administrativo	Administración	\N	0412-8841209
7	Ana Torres	ana.torres		Empleado	t	Supervisora de Operaciones	Operaciones	\N	0414-4092817
8	Luis Ramírez	luis.ramirez		Empleado	t	Técnico de Soporte TI	TI	\N	0424-3117845
3	Laura Gómez	laura.gomez		Empleado	t	Gerente General	Gerencia	Alta	0424-4910238
15	Geraldo Unda	geraldounda		Empleado	t	Operador de videovigilancia	PCP	Alta	0424-3666182|6393449172
2	Francisco Geraldo Unda Solorzano	fgus	4745902	Ingeniero	t	\N	\N	\N	\N
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260906061636_InitialCreate	10.0.11
20260908212948_AnalisisPredictivo	10.0.11
20260908224741_AlertasDinamicasConsumibles	10.0.11
20260923133757_ModuloTickets	10.0.11
20261001020151_MonitorRedSnmp	10.0.11
20261006084409_AgregarRutaManual	10.0.11
20261006151533_AgregarSuministros	10.0.11
20261006162752_RemoverCostoUnitario	10.0.11
20261006164958_AgregarMovimientosInventario	10.0.11
20261007051912_AgregarObservacionSuministro	10.0.11
20261007052848_EliminarMovimientosInventario	10.0.11
20261007062149_AgregarRegistroConsumoDiario	10.0.11
20261007063823_AgregarConsumoDiario	10.0.11
20261007081047_CambiarBitacoraActividades	10.0.11
20261007081614_AgregarBitacora	10.0.11
20261007091450_AgregarMantenimientosImpresora	10.0.11
20261007092204_AgregarMantenimientoImpresora	10.0.11
20261007093355_AgregarUsuarios	10.0.11
20261007101735_AgregarComunidadSnmp	10.0.11
\.


--
-- Name: BitacoraActividades_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."BitacoraActividades_Id_seq"', 7, true);


--
-- Name: CategoriasTicket_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."CategoriasTicket_Id_seq"', 50, true);


--
-- Name: Consumibles_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Consumibles_Id_seq"', 1, true);


--
-- Name: Departamentos_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Departamentos_Id_seq"', 61, true);


--
-- Name: HistorialConsumo_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."HistorialConsumo_Id_seq"', 1, false);


--
-- Name: Impresoras_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Impresoras_Id_seq"', 17, true);


--
-- Name: MantenimientosImpresora_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."MantenimientosImpresora_Id_seq"', 1, true);


--
-- Name: MetricasRed_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."MetricasRed_Id_seq"', 1, false);


--
-- Name: RegistrosConsumoDiario_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."RegistrosConsumoDiario_Id_seq"', 2775, true);


--
-- Name: Suministros_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Suministros_Id_seq"', 8, true);


--
-- Name: Tickets_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Tickets_Id_seq"', 32, true);


--
-- Name: Usuarios_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."Usuarios_Id_seq"', 15, true);


--
-- Name: BitacoraActividades PK_BitacoraActividades; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BitacoraActividades"
    ADD CONSTRAINT "PK_BitacoraActividades" PRIMARY KEY ("Id");


--
-- Name: CategoriasTicket PK_CategoriasTicket; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CategoriasTicket"
    ADD CONSTRAINT "PK_CategoriasTicket" PRIMARY KEY ("Id");


--
-- Name: Consumibles PK_Consumibles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Consumibles"
    ADD CONSTRAINT "PK_Consumibles" PRIMARY KEY ("Id");


--
-- Name: Departamentos PK_Departamentos; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Departamentos"
    ADD CONSTRAINT "PK_Departamentos" PRIMARY KEY ("Id");


--
-- Name: HistorialConsumo PK_HistorialConsumo; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."HistorialConsumo"
    ADD CONSTRAINT "PK_HistorialConsumo" PRIMARY KEY ("Id");


--
-- Name: Impresoras PK_Impresoras; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Impresoras"
    ADD CONSTRAINT "PK_Impresoras" PRIMARY KEY ("Id");


--
-- Name: MantenimientosImpresora PK_MantenimientosImpresora; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MantenimientosImpresora"
    ADD CONSTRAINT "PK_MantenimientosImpresora" PRIMARY KEY ("Id");


--
-- Name: MetricasRed PK_MetricasRed; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MetricasRed"
    ADD CONSTRAINT "PK_MetricasRed" PRIMARY KEY ("Id");


--
-- Name: RegistrosConsumoDiario PK_RegistrosConsumoDiario; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RegistrosConsumoDiario"
    ADD CONSTRAINT "PK_RegistrosConsumoDiario" PRIMARY KEY ("Id");


--
-- Name: Suministros PK_Suministros; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Suministros"
    ADD CONSTRAINT "PK_Suministros" PRIMARY KEY ("Id");


--
-- Name: Tickets PK_Tickets; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Tickets"
    ADD CONSTRAINT "PK_Tickets" PRIMARY KEY ("Id");


--
-- Name: Usuarios PK_Usuarios; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Usuarios"
    ADD CONSTRAINT "PK_Usuarios" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_BitacoraActividades_AnalistaId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BitacoraActividades_AnalistaId" ON public."BitacoraActividades" USING btree ("AnalistaId");


--
-- Name: IX_BitacoraActividades_Estado; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BitacoraActividades_Estado" ON public."BitacoraActividades" USING btree ("Estado");


--
-- Name: IX_CategoriasTicket_Nombre; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_CategoriasTicket_Nombre" ON public."CategoriasTicket" USING btree ("Nombre");


--
-- Name: IX_Departamentos_Nombre; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Departamentos_Nombre" ON public."Departamentos" USING btree ("Nombre");


--
-- Name: IX_Departamentos_Sigla; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Departamentos_Sigla" ON public."Departamentos" USING btree ("Sigla");


--
-- Name: IX_HistorialConsumo_ConsumibleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_HistorialConsumo_ConsumibleId" ON public."HistorialConsumo" USING btree ("ConsumibleId");


--
-- Name: IX_HistorialConsumo_FechaCambio; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_HistorialConsumo_FechaCambio" ON public."HistorialConsumo" USING btree ("FechaCambio");


--
-- Name: IX_HistorialConsumo_ImpresoraId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_HistorialConsumo_ImpresoraId" ON public."HistorialConsumo" USING btree ("ImpresoraId");


--
-- Name: IX_Impresoras_AlertaStockCritico; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_AlertaStockCritico" ON public."Impresoras" USING btree ("AlertaStockCritico");


--
-- Name: IX_Impresoras_ConsumibleTonerId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_ConsumibleTonerId" ON public."Impresoras" USING btree ("ConsumibleTonerId");


--
-- Name: IX_Impresoras_Departamento; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_Departamento" ON public."Impresoras" USING btree ("Departamento");


--
-- Name: IX_Impresoras_DiasEstimadosAgotamientoTonner; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_DiasEstimadosAgotamientoTonner" ON public."Impresoras" USING btree ("DiasEstimadosAgotamientoTonner");


--
-- Name: IX_Impresoras_DiasEstimadosMantenimiento; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_DiasEstimadosMantenimiento" ON public."Impresoras" USING btree ("DiasEstimadosMantenimiento");


--
-- Name: IX_Impresoras_Ip; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Impresoras_Ip" ON public."Impresoras" USING btree ("Ip");


--
-- Name: IX_Impresoras_NivelTonnerNegro; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Impresoras_NivelTonnerNegro" ON public."Impresoras" USING btree ("NivelTonnerNegro");


--
-- Name: IX_MantenimientosImpresora_Fecha; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MantenimientosImpresora_Fecha" ON public."MantenimientosImpresora" USING btree ("Fecha");


--
-- Name: IX_MantenimientosImpresora_ImpresoraId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MantenimientosImpresora_ImpresoraId" ON public."MantenimientosImpresora" USING btree ("ImpresoraId");


--
-- Name: IX_MetricasRed_DispositivoIP; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MetricasRed_DispositivoIP" ON public."MetricasRed" USING btree ("DispositivoIP");


--
-- Name: IX_MetricasRed_FechaHora; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_MetricasRed_FechaHora" ON public."MetricasRed" USING btree ("FechaHora");


--
-- Name: IX_RegistrosConsumoDiario_Fecha; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RegistrosConsumoDiario_Fecha" ON public."RegistrosConsumoDiario" USING btree ("Fecha");


--
-- Name: IX_RegistrosConsumoDiario_ImpresoraId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_RegistrosConsumoDiario_ImpresoraId" ON public."RegistrosConsumoDiario" USING btree ("ImpresoraId");


--
-- Name: IX_Suministros_Nombre; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Suministros_Nombre" ON public."Suministros" USING btree ("Nombre");


--
-- Name: IX_Suministros_Tipo; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Suministros_Tipo" ON public."Suministros" USING btree ("Tipo");


--
-- Name: IX_Tickets_Categoria; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_Categoria" ON public."Tickets" USING btree ("Categoria");


--
-- Name: IX_Tickets_CodigoNomenclatura; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_CodigoNomenclatura" ON public."Tickets" USING btree ("CodigoNomenclatura");


--
-- Name: IX_Tickets_Departamento; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_Departamento" ON public."Tickets" USING btree ("Departamento");


--
-- Name: IX_Tickets_Estado; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_Estado" ON public."Tickets" USING btree ("Estado");


--
-- Name: IX_Tickets_FechaCreacion; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_FechaCreacion" ON public."Tickets" USING btree ("FechaCreacion");


--
-- Name: IX_Tickets_Solicitante; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_Solicitante" ON public."Tickets" USING btree ("Solicitante");


--
-- Name: IX_Tickets_TelegramChatId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Tickets_TelegramChatId" ON public."Tickets" USING btree ("TelegramChatId");


--
-- Name: IX_Usuarios_Departamento; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Usuarios_Departamento" ON public."Usuarios" USING btree ("Departamento");


--
-- Name: IX_Usuarios_TelegramUsuarioOId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Usuarios_TelegramUsuarioOId" ON public."Usuarios" USING btree ("TelegramUsuarioOId");


--
-- Name: IX_Usuarios_Username; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Usuarios_Username" ON public."Usuarios" USING btree ("Username");


--
-- Name: HistorialConsumo FK_HistorialConsumo_Consumibles_ConsumibleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."HistorialConsumo"
    ADD CONSTRAINT "FK_HistorialConsumo_Consumibles_ConsumibleId" FOREIGN KEY ("ConsumibleId") REFERENCES public."Consumibles"("Id") ON DELETE RESTRICT;


--
-- Name: HistorialConsumo FK_HistorialConsumo_Impresoras_ImpresoraId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."HistorialConsumo"
    ADD CONSTRAINT "FK_HistorialConsumo_Impresoras_ImpresoraId" FOREIGN KEY ("ImpresoraId") REFERENCES public."Impresoras"("Id") ON DELETE CASCADE;


--
-- Name: Impresoras FK_Impresoras_Consumibles_ConsumibleTonerId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Impresoras"
    ADD CONSTRAINT "FK_Impresoras_Consumibles_ConsumibleTonerId" FOREIGN KEY ("ConsumibleTonerId") REFERENCES public."Consumibles"("Id") ON DELETE SET NULL;


--
-- Name: MantenimientosImpresora FK_MantenimientosImpresora_Impresoras_ImpresoraId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."MantenimientosImpresora"
    ADD CONSTRAINT "FK_MantenimientosImpresora_Impresoras_ImpresoraId" FOREIGN KEY ("ImpresoraId") REFERENCES public."Impresoras"("Id") ON DELETE CASCADE;


--
-- Name: RegistrosConsumoDiario FK_RegistrosConsumoDiario_Impresoras_ImpresoraId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."RegistrosConsumoDiario"
    ADD CONSTRAINT "FK_RegistrosConsumoDiario_Impresoras_ImpresoraId" FOREIGN KEY ("ImpresoraId") REFERENCES public."Impresoras"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

