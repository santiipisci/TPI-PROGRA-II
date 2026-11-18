# Contrato de API que consume el front

El front (HTML/CSS/JS estático) **no guarda datos** ni usa `localStorage`/`sessionStorage`. Todo pasa por la API del proyecto de Visual Studio.
URL base: `js/config.js` → `API_BASE`. JSON en camelCase, con nombres de campos = columnas de `db_Automotriz`.
Todas las llamadas usan `credentials: "include"`: en ASP.NET habilitar CORS con `AllowCredentials()` y origen explícito. La sesión es una cookie HttpOnly.
Errores: status HTTP + `{ "mensaje": "..." }` (o ProblemDetails `detail`/`title`), que se muestra tal cual. 401 en rutas admin → el front redirige a `login.html`. 409 → duplicado.

## Públicos
| Método | Ruta | Detalle |
|---|---|---|
| GET | `/autopartes` | Solo `PRODUCTO.activo = 1`: `[{ idProducto, codigoParte, descripcion, idCategoria, categoria, unidadMedida, precio, stockActual, stockMinimo, imagenUrl, modelos: [idModelo] }]`. `precio` = `PRECIO_PRODUCTO` con `fechaHasta IS NULL`; `imagenUrl` = `IMAGEN_PRODUCTO` principal (o null); `modelos` = `AUTOPARTE_MODELO` |
| GET | `/modelos` | `[{ idModelo, idMarca, marca, nombre, anioDesde, anioHasta }]` (años opcionales, null si no se derivan de `VEHICULO.anio`) |
| GET | `/categorias` | `[{ idCategoria, nombre }]` |
| GET | `/tipos-cliente` | `[{ idTipoCliente, descripcion }]` |

## Cuenta de cliente (tabla CLIENTE)
| Método | Ruta | Detalle |
|---|---|---|
| POST | `/clientes/registro` | `{ idTipoCliente, nombreRazonSocial, documentoCuit, telefono, direccion, email, clave }` → 201; 409 si ya existe el documento o el email |
| POST | `/clientes/login` | `{ email, clave }` → 200 + cookie / 401 |
| GET | `/clientes/me` | `{ idCliente, nombreRazonSocial, ... }` con sesión, 401 sin sesión (se consulta en cada página para mostrar el nombre en la barra) |
| POST | `/clientes/logout` | 204 |

La clave nunca se guarda en claro: hashear en el backend (p. ej. `PasswordHasher` de Identity).

## Sesión admin
| Método | Ruta | Detalle |
|---|---|---|
| POST | `/auth/login` | `{ usuario, clave }` → 200 + cookie / 401 |
| GET | `/auth/me` | 200 con sesión, 401 sin sesión |
| POST | `/auth/logout` | 204 |

## Administración (requieren sesión)
| Método | Ruta | Detalle |
|---|---|---|
| GET | `/admin/resumen` | `{ ventasUltimoMes, ventasUltimoAnio, ordenesTotales, ventasPorMes: [{anio, mes, total}], ordenesPorEstado: [{estado, cantidad}] }` |
| GET | `/estados-orden` | `[{ idEstadoOrden, descripcion }]` |
| GET | `/admin/ordenes?idEstadoOrden=&q=&pagina=1&tamano=10` | `{ items: [{ idOrdenPedido, fechaPedido, fechaEntrega, cliente, tipoCliente, estado, idEstadoOrden, total }], totalRegistros, sumaTotal }`. `q` busca por cliente o N° de orden; orden por `fechaPedido` desc |
| POST | `/autopartes` | `{ codigoParte, descripcion, idCategoria, unidadMedida, precio, stockActual, stockMinimo }`: crea PRODUCTO + AUTOPARTE + PRECIO_PRODUCTO; 409 si el código existe |
| PUT | `/autopartes/{id}` | `{ descripcion, idCategoria, precio }`: si cambia el precio, cierra el vigente y abre uno nuevo |
| PATCH | `/autopartes/{id}/stock` | `{ stockActual }` → 204 |
| DELETE | `/autopartes/{id}` | baja lógica (`activo = 0`) → 204 |
| PUT / DELETE | `/modelos/{idModelo}/autopartes/{idProducto}` | alta / baja en `AUTOPARTE_MODELO` → 204 |
| POST | `/modelos` | `{ marca, nombre }`: crea la MARCA si no existe; devuelve `{ idModelo, ... }`; 409 si ya existe |

## Base de datos
Todo esto ya está resuelto en `db_Automotriz_completo.sql`:
1. **Contraseña del cliente**: `CLIENTE.claveHash` (hash generado por el backend) y `CLIENTE.email` obligatorio y único.
2. **Usuarios administradores**: tabla `USUARIO_ADMIN`. El primer administrador hay que crearlo desde el backend, porque la clave se guarda con hash.
3. **Años por modelo**: `MODELO.anioDesde` / `anioHasta`.
4. **Stock crítico**: se calcula con `AUTOPARTE.stockMinimo` (`stockActual <= stockMinimo`).

El contacto no usa API ni base de datos: los botones "Consultar" abren Gmail con el mail de la empresa (`EMPRESA_EMAIL` en `js/config.js`) como destinatario.
