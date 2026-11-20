# TPI Programación II – Sistema de Gestión de Autopartes (Motorix)

Trabajo Práctico Integrador de la asignatura **Programación II**.

| **Universidad / Facultad** | UTN FRC |
| **Carrera** | Tecnicatura en Programación |
| **Cátedra** | Programación II |
| **Año / Ciclo lectivo** |2026|
| **Comisión** |1w1|

## Equipo

**Docentes**

| Rol | Nombre y apellido |
|---|---|
| Profesor titular | Martín Dario Polliotto |
| Profesor titular | Roger Damian Martinez |
| Ayudante | Diego Ángel Bravo |
| Ayudante | Joaquín Mellibosky |

**Estudiantes**

| Apellido y Nombre | Legajo | Usuario de GitHub |
|---|---|---|
| Bayúgar Ignacio | 421252 | IgnacioBayugar |
| Parra Samir | 412190 | ParraSamir421 |
| Piscitello Santiago | 430156 | santiipisci |
| Ramos Carlos | 430090 | ramoscarlosdeveloper |

## Índice

1. [Descripción del proyecto](#descripción-del-proyecto)
2. [Tecnologías utilizadas](#tecnologías-utilizadas)
3. [Estructura del repositorio](#estructura-del-repositorio)
4. [Arquitectura del backend](#arquitectura-del-backend)
5. [Base de datos](#base-de-datos)
6. [Puesta en marcha](#puesta-en-marcha)
7. [API](#api)
8. [Seguridad](#seguridad)
9. [Funcionalidades](#funcionalidades)
10. [Documentación adicional](#documentación-adicional)

## Descripción del proyecto

Aplicación web para la gestión y venta de autopartes de una empresa automotriz. Permite a los clientes (minoristas y mayoristas) consultar el catálogo, filtrar repuestos por modelo de vehículo y registrarse; y a los administradores gestionar el catálogo, los precios, el stock y el seguimiento de las órdenes, con un resumen de ventas.

El sistema se compone de tres partes: una base de datos relacional en SQL Server, una API REST en ASP.NET Core y un frontend estático en HTML, CSS y JavaScript.


## Tecnologías utilizadas

| Capa | Tecnología |
|---|---|
| Backend | C#, .NET 8, ASP.NET Core Web API |
| Acceso a datos | Entity Framework Core 8 (SQL Server) |
| Autenticación | Cookies (esquemas separados para cliente y administrador), `PasswordHasher` de ASP.NET Core Identity |
| Documentación de la API | Swagger (Swashbuckle) |
| Base de datos | Microsoft SQL Server / SQL Server Express |
| Frontend | HTML5, CSS3, JavaScript (sin frameworks) |

## Estructura del repositorio

```
TPI-PROGRA-II/
├── Backend/
│   └── TP-PROGII/
│       ├── TP-PROGII.slnx
│       ├── WebAPI-TP-PROGII/     # API: controllers, middleware, seguridad, configuración
│       └── tpRepository/         # Modelos, DbContext, DTOs, repositorios y servicios
├── Base de Datos/
│   ├── db_Automotriz_completo.sql
│   └── db_Automotriz_completo_fix.sql
└── Frontend/
    └── AutopartesFront/
        └── autopartes/           # Sitio estático (pages, js, styles) y API_CONTRATO.md
```

## Arquitectura del backend

El backend sigue una arquitectura en capas:

- **Controllers** (`WebAPI-TP-PROGII/Controllers`): exponen los endpoints HTTP (`Auth`, `Clientes`, `Autopartes`, `Modelos`, `Catalogos`, `Ordenes`, `Admin`).
- **Services** (`tpRepository/Services`): lógica de negocio.
- **Repositories** (`tpRepository/Repository`): acceso a datos mediante Entity Framework Core.
- **Data** (`tpRepository/Data`): entidades, `DbContext` y DTOs.
- **Middleware** (`ApiExceptionMiddleware`): manejo centralizado de errores, que devuelve `{ "mensaje": "..." }` con el código HTTP correspondiente.


## Base de datos

El script crea la base `db_Automotriz_final` con las tablas del dominio, entre ellas: `CLIENTE`, `USUARIO_ADMIN`, `MARCA`, `MODELO`, `VEHICULO`, `PRODUCTO`, `AUTOPARTE`, `AUTOPARTE_MODELO`, `PRECIO_PRODUCTO`, `ORDEN_PEDIDO`, `DETALLE_ORDEN_PEDIDO`, `ESCALA_DESCUENTO_MAYORISTA`, `FACTURA`, `AUTOPLAN` y `CUOTA_AUTOPLAN`, además de las tablas de catálogos (tipos, estados, categorías).


## Puesta en marcha

### Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server o SQL Server Express
- Visual Studio 2022 (opcional) o la CLI de `dotnet`
- Un servidor estático para el frontend (por ejemplo, la extensión Live Server de VS Code)

### 1. Base de datos

1. Abrir SQL Server Management Studio y conectarse a la instancia local.
2. Ejecutar `Base de Datos/db_Automotriz_completo.sql`.
3. Ejecutar `Base de Datos/db_Automotriz_completo_fix.sql`.

### 2. Backend

1. Revisar la cadena de conexión `DefaultConnection` en `Backend/TP-PROGII/WebAPI-TP-PROGII/appsettings.json` y adaptarla a la instancia local.
2. Ejecutar:

```bash
cd Backend/TP-PROGII/WebAPI-TP-PROGII
dotnet run
```

Por defecto la API escucha en `http://localhost:5000` (ver `Properties/launchSettings.json`).

### 3. Frontend

1. Verificar en `Frontend/AutopartesFront/autopartes/js/config.js` que `API_BASE` apunte a la API (`http://localhost:5000/api`).
2. Servir la carpeta `autopartes` desde un origen permitido por CORS (`http://localhost:5500` o `http://127.0.0.1:5500` por defecto; se configuran en `appsettings.json`, sección `Cors`).
3. Abrir `index.html`.

### Usuario administrador

La sección `AdminSeed` de `appsettings.json` define las credenciales del administrador inicial. Son solo para desarrollo y deben modificarse fuera de ese entorno.

## API

Todos los endpoints responden JSON en camelCase. La autenticación se realiza con cookies HttpOnly, por lo que las llamadas deben enviarse con credenciales.

| Grupo | Ejemplos de rutas | Acceso |
|---|---|---|
| Catálogo | `GET /api/autopartes`, `/api/modelos`, `/api/categorias`, `/api/tipos-cliente` | Público |
| Clientes | `POST /api/clientes/registro`, `/login`, `/logout`, `GET /api/clientes/me` | Público / Cliente |
| Autenticación admin | `POST /api/auth/login`, `/logout`, `GET /api/auth/me` | Administrador |
| Administración | `GET /api/admin/resumen`, `/api/admin/ordenes`, `POST/PUT /api/autopartes` | Administrador |

El detalle de rutas, parámetros y respuestas se encuentra en [API_CONTRATO.md](Frontend/AutopartesFront/autopartes/API_CONTRATO.md).

## Seguridad

- Las contraseñas se almacenan hasheadas, nunca en texto plano.
- Esquemas de autenticación separados para clientes y administradores; los endpoints de administración requieren sesión de administrador.
- CORS restringido a orígenes explícitos, con credenciales habilitadas.

## Funcionalidades

**Clientes**
- Consulta del catálogo de autopartes con precio vigente y stock.
- Filtrado por modelo y categoría.
- Registro e inicio de sesión (minorista / mayorista).
- Contacto con la empresa.

**Administración**
- Alta y modificación de autopartes, con historial de precios.
- Panel de resumen de ventas (último mes, último año, órdenes por estado).
- Listado de órdenes con filtros, búsqueda y paginación.

---

*Programación II – UTN FRC - TUP – 2026*
