SET QUOTED_IDENTIFIER ON;
/* =====================================================================
   BASES DE DATOS I - TPI Grupal - Caso N3 (Fábrica automotriz)
   Modelo completo y funcional - SQL Server
   Se puede ejecutar completo, de una vez. Es re-ejecutable: si la base ya existe, la recrea desde cero.

   *** ATENCIÓN: este script ELIMINA db_Automotriz si ya existe (se pierden todos sus datos). ***
   Usarlo para crear la base por primera vez o para reiniciarla en un entorno de desarrollo.
   Para conservar datos, borrar o comentar el bloque "DROP DATABASE" de abajo.

   Cambios respecto del script original (marcados con [NUEVO] / [CAMBIO]):
   [CAMBIO] CLIENTE: email obligatorio y único + claveHash (login de clientes del sitio)
   [CAMBIO] MODELO: anioDesde / anioHasta (rango de años que muestra el sitio)
   [CAMBIO] IMAGEN_PRODUCTO / PRECIO_PRODUCTO: un único registro principal / vigente por producto
   [NUEVO]  USUARIO_ADMIN: administradores del panel
   [NUEVO]  Índices de búsqueda y carga inicial de todos los catálogos
   [NUEVO]  Sección 9 (OPCIONAL): datos de demostración del catálogo
   ===================================================================== */
USE master;
CREATE DATABASE db_Automotriz_final;
GO
USE db_Automotriz_final;
GO

/* ---------------------------------------------------------------------
   1) CATÁLOGOS
   --------------------------------------------------------------------- */
CREATE TABLE TIPO_CLIENTE (
    idTipoCliente INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(50) NOT NULL UNIQUE      -- Consumidor final, Empresa, Concesionario, Vendedora de autopartes
);

CREATE TABLE TIPO_PRODUCTO (
    idTipoProducto INT IDENTITY(1,1) PRIMARY KEY,
    descripcion    VARCHAR(30) NOT NULL UNIQUE     -- Vehículo, Autoparte
);

CREATE TABLE MARCA (
    idMarca INT IDENTITY(1,1) PRIMARY KEY,
    nombre  VARCHAR(50) NOT NULL UNIQUE
);

-- [CAMBIO] anioDesde / anioHasta: rango de años del modelo (opcionales)
-- activo: baja lógica (bool/BIT) según convenciones de negocio
CREATE TABLE MODELO (
    idModelo   INT IDENTITY(1,1) PRIMARY KEY,
    idMarca    INT NOT NULL REFERENCES MARCA(idMarca),
    nombre     VARCHAR(50) NOT NULL,
    anioDesde  SMALLINT NULL CHECK (anioDesde >= 1900),
    anioHasta  SMALLINT NULL,
    activo     BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_ModeloAnios CHECK (
        (anioDesde IS NULL AND anioHasta IS NULL)
        OR (anioDesde IS NOT NULL AND anioHasta IS NOT NULL AND anioHasta >= anioDesde)
    )
);
-- Unicidad solo entre modelos activos (permite historial de bajas lógicas)
CREATE UNIQUE INDEX UQ_Modelo_Activo ON MODELO(idMarca, nombre) WHERE activo = 1;

CREATE TABLE TIPO_CARROCERIA (
    idTipoCarroceria INT IDENTITY(1,1) PRIMARY KEY,
    descripcion      VARCHAR(30) NOT NULL UNIQUE   -- Sedán, Hatchback, SUV, Pick-up, etc.
);

CREATE TABLE TIPO_COMBUSTIBLE (
    idCombustible INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(30) NOT NULL UNIQUE      -- Nafta, Diésel, GNC, Híbrido, Eléctrico
);

CREATE TABLE TRANSMISION (
    idTransmision INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(30) NOT NULL UNIQUE      -- Manual, Automática
);

CREATE TABLE CATEGORIA_AUTOPARTE (
    idCategoria INT IDENTITY(1,1) PRIMARY KEY,
    nombre      VARCHAR(50) NOT NULL UNIQUE        -- Eléctrico, Escape, Filtros, Frenos, Motor, Refrigeración, Suspensión, Transmisión
);

CREATE TABLE ESTADO_ORDEN (
    idEstadoOrden INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(30) NOT NULL UNIQUE      -- Pendiente, En producción, Lista, Entregada, Cancelada
);

CREATE TABLE ESTADO_AUTOPLAN (
    idEstadoAutoplan INT IDENTITY(1,1) PRIMARY KEY,
    descripcion      VARCHAR(30) NOT NULL UNIQUE   -- Vigente, Finalizado, Rescindido
);

/* ---------------------------------------------------------------------
   2) CLIENTES Y USUARIOS
   --------------------------------------------------------------------- */
-- [CAMBIO] email NOT NULL (usuario de login) y claveHash (hash generado por el backend, nunca texto plano).
-- claveHash NULL = cliente cargado por el negocio que todavía no creó su cuenta en el sitio.
-- activo: baja lógica (bool/BIT) según convenciones de negocio
CREATE TABLE CLIENTE (
    idCliente         INT IDENTITY(1,1) PRIMARY KEY,
    idTipoCliente     INT          NOT NULL REFERENCES TIPO_CLIENTE(idTipoCliente),
    nombreRazonSocial VARCHAR(100) NOT NULL,
    documentoCuit     VARCHAR(13)  NOT NULL,
    direccion         VARCHAR(150) NULL,
    telefono          VARCHAR(20)  NULL,
    email             VARCHAR(100) NOT NULL,
    claveHash         VARCHAR(255) NULL,
    fechaAlta         DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
    activo            BIT          NOT NULL DEFAULT 1
);
-- Unicidad solo entre clientes activos (permite conservar historial de bajas lógicas)
CREATE UNIQUE INDEX UX_Cliente_Documento_Activo ON CLIENTE(documentoCuit) WHERE activo = 1;
CREATE UNIQUE INDEX UX_Cliente_Email_Activo ON CLIENTE(email) WHERE activo = 1;

-- [NUEVO] Administradores del panel (login en /auth/login)
CREATE TABLE USUARIO_ADMIN (
    idUsuario  INT IDENTITY(1,1) PRIMARY KEY,
    usuario    VARCHAR(50)  NOT NULL UNIQUE,
    claveHash  VARCHAR(255) NOT NULL,
    nombre     VARCHAR(100) NULL,
    activo     BIT          NOT NULL DEFAULT 1
);

/* ---------------------------------------------------------------------
   3) PRODUCTOS (supertipo), subtipos e imágenes
   --------------------------------------------------------------------- */
CREATE TABLE PRODUCTO (
    idProducto     INT IDENTITY(1,1) PRIMARY KEY,
    idTipoProducto INT          NOT NULL REFERENCES TIPO_PRODUCTO(idTipoProducto),
    descripcion    VARCHAR(150) NOT NULL,
    activo         BIT          NOT NULL DEFAULT 1      -- baja lógica
);

-- Tabla de imágenes asociada a los productos (Vehículos y Autopartes)
CREATE TABLE IMAGEN_PRODUCTO (
    idImagen       INT IDENTITY(1,1) PRIMARY KEY,
    idProducto     INT          NOT NULL REFERENCES PRODUCTO(idProducto),
    urlImagen      VARCHAR(255) NOT NULL,
    descripcion    VARCHAR(100) NULL,
    esPrincipal    BIT          NOT NULL DEFAULT 0,
    orden          TINYINT      NOT NULL DEFAULT 1 CHECK (orden > 0)
);
-- [CAMBIO] Como máximo una imagen principal por producto
CREATE UNIQUE INDEX UX_Imagen_Principal ON IMAGEN_PRODUCTO(idProducto) WHERE esPrincipal = 1;

-- Subtipo: relación 1 a 0..1 con PRODUCTO (la PK es a la vez FK)
CREATE TABLE VEHICULO (
    idProducto       INT PRIMARY KEY REFERENCES PRODUCTO(idProducto),
    idModelo         INT           NOT NULL REFERENCES MODELO(idModelo),
    anio             SMALLINT      NOT NULL CHECK (anio >= 1900),
    version          VARCHAR(50)   NULL,
    color            VARCHAR(30)   NULL,
    idTipoCarroceria INT           NOT NULL REFERENCES TIPO_CARROCERIA(idTipoCarroceria),
    idCombustible    INT           NOT NULL REFERENCES TIPO_COMBUSTIBLE(idCombustible),
    idTransmision    INT           NOT NULL REFERENCES TRANSMISION(idTransmision),
    cilindradaCc     INT           NULL CHECK (cilindradaCc > 0),
    cantidadPuertas  TINYINT       NULL
);

-- Subtipo: relación 1 a 0..1 con PRODUCTO
CREATE TABLE AUTOPARTE (
    idProducto    INT PRIMARY KEY REFERENCES PRODUCTO(idProducto),
    idCategoria   INT         NOT NULL REFERENCES CATEGORIA_AUTOPARTE(idCategoria),
    codigoParte   VARCHAR(30) NOT NULL UNIQUE,
    unidadMedida  VARCHAR(15) NOT NULL DEFAULT 'unidad',
    stockActual   INT         NOT NULL DEFAULT 0 CHECK (stockActual >= 0),
    stockMinimo   INT         NOT NULL DEFAULT 0 CHECK (stockMinimo >= 0)
);

-- N:M compatibilidad autoparte <-> modelo
CREATE TABLE AUTOPARTE_MODELO (
    idProducto INT NOT NULL REFERENCES AUTOPARTE(idProducto),
    idModelo   INT NOT NULL REFERENCES MODELO(idModelo),
    PRIMARY KEY (idProducto, idModelo)
);

CREATE TABLE PRODUCCION_AUTOPARTE (
    idProduccion INT IDENTITY(1,1) PRIMARY KEY,
    idProducto   INT  NOT NULL REFERENCES AUTOPARTE(idProducto),
    fecha        DATE NOT NULL,
    cantidad     INT  NOT NULL CHECK (cantidad > 0)
);

-- Historial de precios (precio vigente: fechaHasta IS NULL)
CREATE TABLE PRECIO_PRODUCTO (
    idPrecio   INT IDENTITY(1,1) PRIMARY KEY,
    idProducto INT           NOT NULL REFERENCES PRODUCTO(idProducto),
    precio     DECIMAL(14,2) NOT NULL CHECK (precio > 0),
    fechaDesde DATE          NOT NULL,
    fechaHasta DATE          NULL,
    CONSTRAINT CK_PrecioFechas CHECK (fechaHasta IS NULL OR fechaHasta >= fechaDesde),
    CONSTRAINT UQ_PrecioVigencia UNIQUE (idProducto, fechaDesde)
);
-- [CAMBIO] Un solo precio vigente (fechaHasta IS NULL) por producto
CREATE UNIQUE INDEX UX_Precio_Vigente ON PRECIO_PRODUCTO(idProducto) WHERE fechaHasta IS NULL;

/* ---------------------------------------------------------------------
   4) ÓRDENES DE PEDIDO
   --------------------------------------------------------------------- */
-- activo: baja lógica (bool/BIT) para preservar historial transaccional
CREATE TABLE ORDEN_PEDIDO (
    idOrdenPedido INT IDENTITY(1,1) PRIMARY KEY,
    idCliente     INT  NOT NULL REFERENCES CLIENTE(idCliente),
    idEstadoOrden INT  NOT NULL REFERENCES ESTADO_ORDEN(idEstadoOrden),
    fechaPedido   DATE NOT NULL,
    fechaEntrega  DATE NULL,
    activo        BIT  NOT NULL DEFAULT 1,
    CONSTRAINT CK_OrdenFechas CHECK (fechaEntrega IS NULL OR fechaEntrega >= fechaPedido)
);

CREATE TABLE DETALLE_ORDEN_PEDIDO (
    idOrdenPedido INT NOT NULL REFERENCES ORDEN_PEDIDO(idOrdenPedido),
    idProducto    INT NOT NULL REFERENCES PRODUCTO(idProducto),
    cantidad      INT NOT NULL CHECK (cantidad > 0),
    PRIMARY KEY (idOrdenPedido, idProducto)
);

-- [NUEVO] Historial de estados de cada orden
CREATE TABLE SEGUIMIENTO_ORDEN (
    idSeguimiento INT IDENTITY(1,1) PRIMARY KEY,
    idOrdenPedido INT NOT NULL,
    idEstadoOrden INT NOT NULL,
    fecha         DATE NOT NULL,
    observacion   VARCHAR(300) NULL,
    CONSTRAINT FK_Seguimiento_Orden  FOREIGN KEY (idOrdenPedido) REFERENCES ORDEN_PEDIDO(idOrdenPedido) ON DELETE CASCADE,
    CONSTRAINT FK_Seguimiento_Estado FOREIGN KEY (idEstadoOrden) REFERENCES ESTADO_ORDEN(idEstadoOrden)
);

CREATE INDEX IX_Seguimiento_Orden ON SEGUIMIENTO_ORDEN(idOrdenPedido, fecha);

/* ---------------------------------------------------------------------
   5) FACTURACIÓN
   --------------------------------------------------------------------- */
CREATE TABLE ESCALA_DESCUENTO_MAYORISTA (
    idEscala            INT IDENTITY(1,1) PRIMARY KEY,
    cantidadMinima      INT          NOT NULL UNIQUE CHECK (cantidadMinima > 0),
    porcentajeDescuento DECIMAL(5,2) NOT NULL CHECK (porcentajeDescuento BETWEEN 0 AND 100)
);

CREATE TABLE FACTURA (
    idFactura     INT IDENTITY(1,1) PRIMARY KEY,
    idCliente     INT           NOT NULL REFERENCES CLIENTE(idCliente),
    idOrdenPedido INT           NULL REFERENCES ORDEN_PEDIDO(idOrdenPedido),  -- opcional: solo si hubo orden
    fecha         DATE          NOT NULL,
    total         DECIMAL(14,2) NOT NULL CHECK (total >= 0)
);
-- Una orden de pedido se asocia como máximo a una factura (1 a 0..1)
CREATE UNIQUE INDEX UX_Factura_Orden ON FACTURA(idOrdenPedido) WHERE idOrdenPedido IS NOT NULL;

CREATE TABLE DETALLE_FACTURA (
    idDetalleFactura    INT IDENTITY(1,1) PRIMARY KEY,
    idFactura           INT           NOT NULL REFERENCES FACTURA(idFactura),
    idProducto          INT           NOT NULL REFERENCES PRODUCTO(idProducto),
    idEscala            INT           NULL REFERENCES ESCALA_DESCUENTO_MAYORISTA(idEscala),
    cantidad            INT           NOT NULL CHECK (cantidad > 0),
    precioUnitario      DECIMAL(14,2) NOT NULL CHECK (precioUnitario >= 0),  -- foto del precio al facturar
    porcentajeDescuento DECIMAL(5,2)  NOT NULL DEFAULT 0
                        CHECK (porcentajeDescuento BETWEEN 0 AND 100),        -- foto del descuento al facturar
    subtotal AS CAST(cantidad * precioUnitario * (1 - porcentajeDescuento / 100.0) AS DECIMAL(14,2)) PERSISTED,
    CONSTRAINT UQ_DetalleFactura UNIQUE (idFactura, idProducto)
);

/* ---------------------------------------------------------------------
   6) AUTOPLAN
   --------------------------------------------------------------------- */
-- Tasa según cantidad de cuotas, con vigencia (para no perder la tasa de planes ya firmados)
CREATE TABLE TASA_INTERES (
    idTasa         INT IDENTITY(1,1) PRIMARY KEY,
    cantidadCuotas INT          NOT NULL CHECK (cantidadCuotas > 0),
    tasa           DECIMAL(6,2) NOT NULL CHECK (tasa >= 0),   -- porcentaje
    fechaDesde     DATE         NOT NULL,
    fechaHasta     DATE         NULL,
    CONSTRAINT CK_TasaFechas CHECK (fechaHasta IS NULL OR fechaHasta >= fechaDesde),
    CONSTRAINT UQ_Tasa UNIQUE (cantidadCuotas, fechaDesde)
);

-- Un autoplan por línea de factura (1 a 0..1 con DETALLE_FACTURA).
-- Regla de negocio: solo clientes "Consumidor final" (se valida en la capa de negocio).
CREATE TABLE AUTOPLAN (
    idAutoplan       INT IDENTITY(1,1) PRIMARY KEY,
    idDetalleFactura INT           NOT NULL UNIQUE REFERENCES DETALLE_FACTURA(idDetalleFactura),
    idTasa           INT           NOT NULL REFERENCES TASA_INTERES(idTasa),   -- fija tasa y cantidad de cuotas
    idEstadoAutoplan INT           NOT NULL REFERENCES ESTADO_AUTOPLAN(idEstadoAutoplan),
    fechaInicio      DATE          NOT NULL,
    montoFinanciado  DECIMAL(14,2) NOT NULL CHECK (montoFinanciado > 0),
    montoTotal       DECIMAL(14,2) NOT NULL,
    valorCuota       DECIMAL(14,2) NOT NULL CHECK (valorCuota > 0),
    CONSTRAINT CK_AutoplanMontos CHECK (montoTotal >= montoFinanciado)
);

CREATE TABLE CUOTA_AUTOPLAN (
    idAutoplan       INT           NOT NULL REFERENCES AUTOPLAN(idAutoplan),
    nroCuota         INT           NOT NULL CHECK (nroCuota > 0),
    fechaVencimiento DATE          NOT NULL,
    importe          DECIMAL(14,2) NOT NULL CHECK (importe > 0),
    fechaPago        DATE          NULL,               -- NULL = impaga
    PRIMARY KEY (idAutoplan, nroCuota)
);
GO

/* ---------------------------------------------------------------------
   7) ÍNDICES DE BÚSQUEDA  [NUEVO]
   SQL Server no indexa automáticamente las columnas FK; se agregan las que usan las consultas del sitio.
   --------------------------------------------------------------------- */
CREATE INDEX IX_Producto_Activo        ON PRODUCTO(activo) INCLUDE (idTipoProducto, descripcion);
CREATE INDEX IX_Modelo_Activo          ON MODELO(activo) INCLUDE (idMarca, nombre);
CREATE INDEX IX_Cliente_Activo         ON CLIENTE(activo) INCLUDE (idTipoCliente, nombreRazonSocial, email);
CREATE INDEX IX_Autoparte_Categoria    ON AUTOPARTE(idCategoria);
CREATE INDEX IX_AutoparteModelo_Modelo ON AUTOPARTE_MODELO(idModelo);
CREATE INDEX IX_Orden_Cliente          ON ORDEN_PEDIDO(idCliente);
CREATE INDEX IX_Orden_Activo           ON ORDEN_PEDIDO(activo) INCLUDE (idCliente, idEstadoOrden, fechaPedido);
CREATE INDEX IX_Orden_Estado_Fecha     ON ORDEN_PEDIDO(idEstadoOrden, fechaPedido DESC);
CREATE INDEX IX_Factura_Fecha          ON FACTURA(fecha) INCLUDE (total);
CREATE INDEX IX_Factura_Cliente        ON FACTURA(idCliente);
GO

/* ---------------------------------------------------------------------
   8) CARGA INICIAL DE CATÁLOGOS
   --------------------------------------------------------------------- */
INSERT INTO CATEGORIA_AUTOPARTE (nombre) VALUES
('Eléctrico'), ('Escape'), ('Filtros'), ('Frenos'), ('Motor'), ('Refrigeración'), ('Suspensión'), ('Transmisión');

INSERT INTO TIPO_CLIENTE (descripcion) VALUES
('Consumidor final'), ('Empresa'), ('Concesionario'), ('Vendedora de autopartes');

INSERT INTO TIPO_PRODUCTO (descripcion) VALUES ('Vehículo'), ('Autoparte');

INSERT INTO ESTADO_ORDEN (descripcion) VALUES
('Pendiente'), ('En producción'), ('Lista'), ('Entregada'), ('Cancelada');

INSERT INTO ESTADO_AUTOPLAN (descripcion) VALUES ('Vigente'), ('Finalizado'), ('Rescindido');

INSERT INTO TIPO_CARROCERIA (descripcion) VALUES
('Sedán'), ('Hatchback'), ('SUV'), ('Pick-up'), ('Utilitario'), ('Coupé');

INSERT INTO TIPO_COMBUSTIBLE (descripcion) VALUES
('Nafta'), ('Diésel'), ('GNC'), ('Híbrido'), ('Eléctrico');

INSERT INTO TRANSMISION (descripcion) VALUES ('Manual'), ('Automática');
GO

/* ---------------------------------------------------------------------
   9) DATOS DE DEMOSTRACIÓN (OPCIONAL)  [NUEVO]
   Marcas, modelos y 14 autopartes con precio, stock y compatibilidades,
   para que el sitio muestre contenido apenas se conecta la API.
   Para una base vacía, borrar o comentar desde acá hasta el final.
   No incluye clientes, órdenes ni facturas: los genera el sistema.
   --------------------------------------------------------------------- */
INSERT INTO MARCA (nombre) VALUES
('Toyota'),
('Ford'),
('Volkswagen'),
('Chevrolet'),
('Fiat'),
('Renault'),
('Peugeot');

INSERT INTO MODELO (idMarca, nombre, anioDesde, anioHasta)
SELECT ma.idMarca, v.nombre, v.desde, v.hasta
FROM (VALUES
('Toyota','Hilux',2016,2026),
('Toyota','Corolla',2014,2026),
('Ford','Ranger',2012,2026),
('Ford','Focus',2008,2019),
('Volkswagen','Amarok',2010,2026),
('Volkswagen','Gol Trend',2008,2019),
('Chevrolet','Cruze',2011,2022),
('Fiat','Cronos',2018,2026),
('Renault','Sandero',2008,2022),
('Peugeot','208',2013,2026)
) AS v(marca, nombre, desde, hasta)
JOIN MARCA ma ON ma.nombre = v.marca;

CREATE TABLE #Seed (codigo VARCHAR(30), descripcion VARCHAR(150), categoria VARCHAR(50), precio DECIMAL(14,2), stock INT, minimo INT);
INSERT INTO #Seed VALUES
('FRE-001','Pastillas de freno delanteras','Frenos',38500,140,10),
('FRE-002','Disco de freno ventilado','Frenos',62900,64,10),
('SUS-001','Amortiguador delantero','Suspensión',91000,38,10),
('SUS-002','Brazo de suspensión inferior','Suspensión',54200,12,10),
('MOT-001','Kit de distribución con bomba','Motor',148000,22,10),
('MOT-002','Junta de tapa de cilindros','Motor',47800,9,10),
('FIL-001','Filtro de aceite','Filtros',6900,520,10),
('FIL-002','Filtro de aire de alto flujo','Filtros',14300,210,10),
('TRA-001','Kit de embrague completo','Transmisión',176500,17,10),
('ELE-001','Bobina de encendido','Eléctrico',28700,75,10),
('ESC-001','Silenciador trasero','Escape',84600,6,10),
('REF-001','Radiador de aluminio','Refrigeración',132000,14,10),
('REF-002','Bomba de agua','Refrigeración',39800,48,10),
('ELE-002','Alternador reconstruido','Eléctrico',119000,4,10);

INSERT INTO PRODUCTO (idTipoProducto, descripcion)
SELECT (SELECT idTipoProducto FROM TIPO_PRODUCTO WHERE descripcion = 'Autoparte'), descripcion
FROM #Seed;

INSERT INTO AUTOPARTE (idProducto, idCategoria, codigoParte, stockActual, stockMinimo)
SELECT p.idProducto, c.idCategoria, s.codigo, s.stock, s.minimo
FROM #Seed s
JOIN PRODUCTO p ON p.descripcion = s.descripcion
JOIN CATEGORIA_AUTOPARTE c ON c.nombre = s.categoria;

INSERT INTO PRECIO_PRODUCTO (idProducto, precio, fechaDesde)
SELECT a.idProducto, s.precio, CAST(GETDATE() AS DATE)
FROM #Seed s
JOIN AUTOPARTE a ON a.codigoParte = s.codigo;

CREATE TABLE #Compat (codigo VARCHAR(30), marca VARCHAR(50), modelo VARCHAR(50));
INSERT INTO #Compat VALUES
('FRE-001','Toyota','Hilux'),
('FRE-001','Ford','Ranger'),
('FRE-001','Volkswagen','Amarok'),
('FRE-002','Toyota','Corolla'),
('FRE-002','Chevrolet','Cruze'),
('FRE-002','Fiat','Cronos'),
('SUS-001','Toyota','Hilux'),
('SUS-001','Ford','Ranger'),
('SUS-001','Volkswagen','Amarok'),
('SUS-002','Ford','Focus'),
('SUS-002','Volkswagen','Gol Trend'),
('SUS-002','Renault','Sandero'),
('MOT-001','Toyota','Corolla'),
('MOT-001','Chevrolet','Cruze'),
('MOT-001','Fiat','Cronos'),
('MOT-001','Peugeot','208'),
('MOT-002','Ford','Focus'),
('MOT-002','Volkswagen','Gol Trend'),
('MOT-002','Renault','Sandero'),
('FIL-001','Toyota','Hilux'),
('FIL-001','Toyota','Corolla'),
('FIL-001','Ford','Ranger'),
('FIL-001','Ford','Focus'),
('FIL-001','Volkswagen','Amarok'),
('FIL-001','Volkswagen','Gol Trend'),
('FIL-001','Chevrolet','Cruze'),
('FIL-001','Fiat','Cronos'),
('FIL-001','Renault','Sandero'),
('FIL-001','Peugeot','208'),
('FIL-002','Toyota','Hilux'),
('FIL-002','Ford','Ranger'),
('FIL-002','Volkswagen','Amarok'),
('FIL-002','Fiat','Cronos'),
('TRA-001','Ford','Focus'),
('TRA-001','Volkswagen','Gol Trend'),
('TRA-001','Fiat','Cronos'),
('TRA-001','Renault','Sandero'),
('TRA-001','Peugeot','208'),
('ELE-001','Toyota','Corolla'),
('ELE-001','Ford','Focus'),
('ELE-001','Chevrolet','Cruze'),
('ELE-001','Peugeot','208'),
('ESC-001','Volkswagen','Gol Trend'),
('ESC-001','Fiat','Cronos'),
('ESC-001','Renault','Sandero'),
('REF-001','Toyota','Hilux'),
('REF-001','Ford','Ranger'),
('REF-001','Volkswagen','Amarok'),
('REF-001','Chevrolet','Cruze'),
('REF-002','Toyota','Corolla'),
('REF-002','Ford','Focus'),
('REF-002','Volkswagen','Gol Trend'),
('REF-002','Fiat','Cronos'),
('REF-002','Renault','Sandero'),
('REF-002','Peugeot','208'),
('ELE-002','Toyota','Hilux'),
('ELE-002','Ford','Ranger'),
('ELE-002','Volkswagen','Amarok');

INSERT INTO AUTOPARTE_MODELO (idProducto, idModelo)
SELECT a.idProducto, m.idModelo
FROM #Compat k
JOIN AUTOPARTE a ON a.codigoParte = k.codigo
JOIN MARCA ma ON ma.nombre = k.marca
JOIN MODELO m ON m.idMarca = ma.idMarca AND m.nombre = k.modelo;

DROP TABLE #Compat;
DROP TABLE #Seed;
GO

/* ---------------------------------------------------------------------
   10) ADMINISTRADOR INICIAL
   La clave se guarda con hash, no se puede insertar desde SQL.
   Crear el primer administrador desde el backend (por ejemplo, un endpoint
   o un seeder que genere el hash con PasswordHasher) y que inserte en USUARIO_ADMIN.
   --------------------------------------------------------------------- */

USE db_Automotriz_final;
GO

-- 1. Insertar estados (sin especificar idEstadoOrden por ser IDENTITY)
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Pendiente')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Pendiente');
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'En producción')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('En producción');
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Lista')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Lista');
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Entregada')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Entregada');
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Cancelada')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Cancelada');
GO

-- 2. Cargar clientes existentes en una variable de tabla para asignar IDs válidos
DECLARE @Clientes TABLE (
    fila INT IDENTITY(1,1) PRIMARY KEY,
    idCliente INT
);

INSERT INTO @Clientes (idCliente)
SELECT idCliente
FROM dbo.CLIENTE
ORDER BY idCliente;

DECLARE @TotalClientes INT = (SELECT COUNT(*) FROM @Clientes);

IF @TotalClientes = 0
BEGIN
    RAISERROR('La tabla dbo.CLIENTE está vacía. Inserta al menos un cliente antes de ejecutar este script.', 16, 1);
    RETURN;
END;
-- Obtener IDs reales de cada estado
DECLARE @EstPendiente INT   = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Pendiente');
DECLARE @EstProduccion INT  = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'En producción');
DECLARE @EstLista INT       = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Lista');
DECLARE @EstEntregada INT   = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Entregada');
DECLARE @EstCancelada INT   = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Cancelada');

-- Datos base variados por mes (Junio a Noviembre 2026)
DECLARE @DatosOrdenes TABLE (
    idx INT IDENTITY(1,1) PRIMARY KEY,
    codEstado VARCHAR(20),
    fechaPedido DATE,
    fechaEntrega DATE NULL
);

INSERT INTO @DatosOrdenes (codEstado, fechaPedido, fechaEntrega) VALUES
-- JUNIO 2026 (4 órdenes)
('ENTREGADA', '2026-06-05', '2026-06-10'),
('ENTREGADA', '2026-06-14', '2026-06-19'),
('CANCELADA', '2026-06-21', NULL),
('ENTREGADA', '2026-06-27', '2026-07-02'),

-- JULIO 2026 (6 órdenes)
('ENTREGADA', '2026-07-03', '2026-07-08'),
('ENTREGADA', '2026-07-09', '2026-07-13'),
('CANCELADA', '2026-07-15', NULL),
('ENTREGADA', '2026-07-18', '2026-07-24'),
('ENTREGADA', '2026-07-22', '2026-07-27'),
('ENTREGADA', '2026-07-29', '2026-08-03'),
-- AGOSTO 2026 (9 órdenes)
('ENTREGADA', '2026-08-02', '2026-08-06'),
('ENTREGADA', '2026-08-06', '2026-08-12'),
('ENTREGADA', '2026-08-10', '2026-08-15'),
('CANCELADA', '2026-08-13', NULL),
('ENTREGADA', '2026-08-17', '2026-08-21'),
('ENTREGADA', '2026-08-20', '2026-08-26'),
('ENTREGADA', '2026-08-24', '2026-08-29'),
('CANCELADA', '2026-08-27', NULL),
('ENTREGADA', '2026-08-30', '2026-09-04'),

-- SEPTIEMBRE 2026 (12 órdenes)
('ENTREGADA', '2026-09-02', '2026-09-07'),
('ENTREGADA', '2026-09-05', '2026-09-10'),
('ENTREGADA', '2026-09-08', '2026-09-14'),
('CANCELADA', '2026-09-11', NULL),
('ENTREGADA', '2026-09-14', '2026-09-18'),
('ENTREGADA', '2026-09-16', '2026-09-22'),
('ENTREGADA', '2026-09-19', '2026-09-24'),
('ENTREGADA', '2026-09-21', '2026-09-26'),
('CANCELADA', '2026-09-24', NULL),
('ENTREGADA', '2026-09-26', '2026-10-01'),
('ENTREGADA', '2026-09-28', '2026-10-03'),
('ENTREGADA', '2026-09-30', '2026-10-05'),

-- OCTUBRE 2026 (15 órdenes)
('ENTREGADA', '2026-10-02', '2026-10-07'),
('ENTREGADA', '2026-10-04', '2026-10-09'),
('ENTREGADA', '2026-10-06', '2026-10-12'),
('CANCELADA', '2026-10-08', NULL),
('ENTREGADA', '2026-10-10', '2026-10-15'),
('ENTREGADA', '2026-10-12', '2026-10-17'),
('LISTA',     '2026-10-15', NULL),
('ENTREGADA', '2026-10-17', '2026-10-22'),
('ENTREGADA', '2026-10-19', '2026-10-24'),
('CANCELADA', '2026-10-21', NULL),
('LISTA',     '2026-10-23', NULL),
('ENTREGADA', '2026-10-25', '2026-10-30'),
('PRODUCCION','2026-10-27', NULL),
('ENTREGADA', '2026-10-29', '2026-11-03'),
('LISTA',     '2026-10-31', NULL),
-- NOVIEMBRE 2026 (18 órdenes)
('ENTREGADA', '2026-11-01', '2026-11-05'),
('ENTREGADA', '2026-11-02', '2026-11-07'),
('ENTREGADA', '2026-11-04', '2026-11-09'),
('CANCELADA', '2026-11-05', NULL),
('ENTREGADA', '2026-11-06', '2026-11-11'),
('LISTA',     '2026-11-08', NULL),
('ENTREGADA', '2026-11-09', '2026-11-14'),
('PRODUCCION','2026-11-10', NULL),
('LISTA',     '2026-11-11', NULL),
('ENTREGADA', '2026-11-12', '2026-11-17'),
('PRODUCCION','2026-11-13', NULL),
('PENDIENTE', '2026-11-14', NULL),
('PRODUCCION','2026-11-15', NULL),
('LISTA',     '2026-11-16', NULL),
('PENDIENTE', '2026-11-17', NULL),
('PRODUCCION','2026-11-18', NULL),
('PENDIENTE', '2026-11-19', NULL),
('PENDIENTE', '2026-11-20', NULL);

-- Insertar en ORDEN_PEDIDO rotando por los clientes existentes
INSERT INTO dbo.ORDEN_PEDIDO (idCliente, idEstadoOrden, fechaPedido, fechaEntrega)
SELECT
    c.idCliente,
    CASE d.codEstado
        WHEN 'PENDIENTE'  THEN @EstPendiente
        WHEN 'PRODUCCION' THEN @EstProduccion
        WHEN 'LISTA'      THEN @EstLista
        WHEN 'ENTREGADA'  THEN @EstEntregada
        WHEN 'CANCELADA'  THEN @EstCancelada
    END,
    d.fechaPedido,
    d.fechaEntrega
FROM @DatosOrdenes d
INNER JOIN @Clientes c
    ON c.fila = ((d.idx - 1) % @TotalClientes) + 1;
GO

-- Detalle de las órdenes sembradas: 3 autopartes por orden, rotando por el catálogo
DECLARE @Partes TABLE (fila INT IDENTITY(1,1), idProducto INT);
INSERT INTO @Partes (idProducto) SELECT idProducto FROM dbo.AUTOPARTE ORDER BY idProducto;
DECLARE @TotalPartes INT = (SELECT COUNT(*) FROM @Partes);

INSERT INTO dbo.DETALLE_ORDEN_PEDIDO (idOrdenPedido, idProducto, cantidad)
SELECT o.idOrdenPedido, p.idProducto, 1 + ((o.idOrdenPedido + n.k) % 5)
FROM dbo.ORDEN_PEDIDO o
CROSS JOIN (VALUES (0), (1), (2)) n(k)
INNER JOIN @Partes p ON p.fila = ((o.idOrdenPedido * 3 + n.k) % @TotalPartes) + 1
WHERE NOT EXISTS (SELECT 1 FROM dbo.DETALLE_ORDEN_PEDIDO d WHERE d.idOrdenPedido = o.idOrdenPedido);
GO

USE db_Automotriz_final;
GO

-- 1. Insertar los 4 tipos de cliente del desplegable
IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Consumidor final')
    INSERT INTO dbo.TIPO_CLIENTE (descripcion) VALUES ('Consumidor final');

IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Empresa')
    INSERT INTO dbo.TIPO_CLIENTE (descripcion) VALUES ('Empresa');

IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Concesionario')
    INSERT INTO dbo.TIPO_CLIENTE (descripcion) VALUES ('Concesionario');

IF NOT EXISTS (SELECT 1 FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Vendedora de autopartes')
    INSERT INTO dbo.TIPO_CLIENTE (descripcion) VALUES ('Vendedora de autopartes');
GO

-- 2. Obtener los IDs reales de cada tipo de cliente
DECLARE @TipoConsumidor INT = (SELECT TOP 1 idTipoCliente FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Consumidor final');
DECLARE @TipoEmpresa    INT = (SELECT TOP 1 idTipoCliente FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Empresa');
DECLARE @TipoConcesio   INT = (SELECT TOP 1 idTipoCliente FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Concesionario');
DECLARE @TipoAutopartes INT = (SELECT TOP 1 idTipoCliente FROM dbo.TIPO_CLIENTE WHERE descripcion = 'Vendedora de autopartes');

INSERT INTO dbo.CLIENTE (
    idTipoCliente,
    nombreRazonSocial,
    documentoCuit,
    direccion,
    telefono,
    email,
    claveHash,
    fechaAlta
) VALUES

(@TipoConsumidor, 'Carlos Alberto Méndez',      '20-28456123-4', 'Av. Colón 1450, Córdoba',            '3515123401', 'cmendez.racing@gmail.com',      NULL, '2026-05-10T09:30:00'),
(@TipoConsumidor, 'Lucía Fernanda Giménez',     '27-33912845-1', 'Bv. San Juan 820, Córdoba',          '3515123402', 'lucia.gimenez@hotmail.com',     NULL, '2026-05-18T11:15:00'),
(@TipoConsumidor, 'Martín Ezequiel Domínguez',  '20-31789456-8', 'Ruta 19 Km 24, Monte Cristo',        '3515123403', 'mdominguez.motor@gmail.com',    NULL, '2026-06-01T10:00:00'),

(@TipoEmpresa, 'Escudería Sur Motorsport S.A.',            '30-71623489-8', 'Parque Industrial Ferreyra, Lote 12','3514881101', 'logistica@escuderiasur.com.ar',  NULL, '2026-06-08T13:00:00'),
(@TipoEmpresa, 'Rectificadora y Motores Mediterránea S.A.','30-70541236-9', 'Calle Juan B. Justo 5210, Córdoba',  '3514881102', 'taller@rectificadoramed.com.ar', NULL, '2026-07-10T11:20:00'),
(@TipoEmpresa, 'Talleres Mecánicos del Centro S.R.L.',     '30-71458923-5', 'Av. Sabattini 3400, Córdoba',        '3514881103', 'compras@talleresdelcentro.com.ar',NULL, '2026-05-15T09:00:00'),

(@TipoConcesio, 'Concesionaria Automotores del Valle S.A.', '30-71890345-2', 'Av. Caraffa 2100, Córdoba',          '3514881104', 'flotas@automotoresdelvalle.com.ar',NULL, '2026-08-03T09:50:00'),
(@TipoConcesio, 'MotorHaus Concesionario Oficial S.R.L.',    '30-71554890-6', 'Av. Rafael Núñez 4120, Córdoba',     '3514881105', 'ventas@motorhaus.com.ar',         NULL, '2026-06-22T15:40:00'),
(@TipoConcesio, 'Alta Gama Automotores del Centro S.A.',     '30-71902341-7', 'Av. Fuerza Aérea 2910, Córdoba',     '3514881106', 'clientes@altagamaauto.com.ar',    NULL, '2026-07-19T14:10:00'),

(@TipoAutopartes, 'Repuestos y Competición Córdoba S.A.',    '30-70984512-1', 'Av. Circunvalación Norte 1850, Cba', '3514881107', 'ventas@repuestoscompeticion.com.ar',NULL, '2026-05-25T10:30:00'),
(@TipoAutopartes, 'Autopartes y Frenos Ruta 19 S.R.L.',      '30-71209874-3', 'Av. San Martín 980, Monte Cristo',   '3514881108', 'administracion@autopartesr19.com.ar',NULL,'2026-06-12T16:20:00'),
(@TipoAutopartes, 'Todo Suspensión y Escape Racing S.A.',    '30-71776543-0', 'Calle Belgrano 415, Río Primero',    '3514881109', 'pedidos@todosuspension.com.ar',   NULL, '2026-07-02T08:45:00');
GO

USE db_Automotriz_final;
GO

-- 1. Insertar los 5 estados de orden (sin idEstadoOrden por ser IDENTITY)
IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Pendiente')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Pendiente');

IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'En producción')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('En producción');

IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Lista')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Lista');

IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Entregada')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Entregada');

IF NOT EXISTS (SELECT 1 FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Cancelada')
    INSERT INTO dbo.ESTADO_ORDEN (descripcion) VALUES ('Cancelada');
GO

-- 2. Obtener IDs reales de cada estado
DECLARE @EstPendiente  INT = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Pendiente');
DECLARE @EstProduccion INT = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'En producción');
DECLARE @EstLista      INT = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Lista');
DECLARE @EstEntregada  INT = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Entregada');
DECLARE @EstCancelada  INT = (SELECT TOP 1 idEstadoOrden FROM dbo.ESTADO_ORDEN WHERE descripcion = 'Cancelada');

DECLARE @OrdenesSeed TABLE (
    cuitCliente  VARCHAR(13),
    codEstado    VARCHAR(20),
    fechaPedido  DATE,
    fechaEntrega DATE NULL
);

INSERT INTO @OrdenesSeed (cuitCliente, codEstado, fechaPedido, fechaEntrega) VALUES
-- JUNIO 2026 (4 órdenes)
('20-28456123-4', 'ENTREGADA',  '2026-06-05', '2026-06-10'), -- Consumidor final
('30-70984512-1', 'ENTREGADA',  '2026-06-14', '2026-06-19'), -- Vendedora de autopartes
('27-33912845-1', 'CANCELADA',  '2026-06-21', NULL),       -- Consumidor final
('30-71890345-2', 'ENTREGADA',  '2026-06-27', '2026-07-02'), -- Concesionario

-- JULIO 2026 (6 órdenes)
('30-71623489-8', 'ENTREGADA',  '2026-07-03', '2026-07-08'), -- Empresa
('20-31789456-8', 'ENTREGADA',  '2026-07-09', '2026-07-13'), -- Consumidor final
('30-71209874-3', 'CANCELADA',  '2026-07-15', NULL),       -- Vendedora de autopartes
('30-71554890-6', 'ENTREGADA',  '2026-07-18', '2026-07-24'), -- Concesionario
('30-70541236-9', 'ENTREGADA',  '2026-07-22', '2026-07-27'), -- Empresa
('30-71776543-0', 'ENTREGADA',  '2026-07-29', '2026-08-03'), -- Vendedora de autopartes

-- AGOSTO 2026 (9 órdenes)
('20-28456123-4', 'ENTREGADA',  '2026-08-02', '2026-08-06'), -- Consumidor final
('30-71458923-5', 'ENTREGADA',  '2026-08-06', '2026-08-12'), -- Empresa
('30-70984512-1', 'ENTREGADA',  '2026-08-10', '2026-08-15'), -- Vendedora de autopartes
('30-71902341-7', 'CANCELADA',  '2026-08-13', NULL),       -- Concesionario
('30-71623489-8', 'ENTREGADA',  '2026-08-17', '2026-08-21'), -- Empresa
('27-33912845-1', 'ENTREGADA',  '2026-08-20', '2026-08-26'), -- Consumidor final
('30-71890345-2', 'ENTREGADA',  '2026-08-24', '2026-08-29'), -- Concesionario
('20-31789456-8', 'CANCELADA',  '2026-08-27', NULL),       -- Consumidor final
('30-71209874-3', 'ENTREGADA',  '2026-08-30', '2026-09-04'), -- Vendedora de autopartes

-- SEPTIEMBRE 2026 (12 órdenes)
('30-71554890-6', 'ENTREGADA',  '2026-09-02', '2026-09-07'), -- Concesionario
('30-70541236-9', 'ENTREGADA',  '2026-09-05', '2026-09-10'), -- Empresa
('30-71776543-0', 'ENTREGADA',  '2026-09-08', '2026-09-14'), -- Vendedora de autopartes
('20-28456123-4', 'CANCELADA',  '2026-09-11', NULL),       -- Consumidor final
('30-71458923-5', 'ENTREGADA',  '2026-09-14', '2026-09-18'), -- Empresa
('30-70984512-1', 'ENTREGADA',  '2026-09-16', '2026-09-22'), -- Vendedora de autopartes
('30-71902341-7', 'ENTREGADA',  '2026-09-19', '2026-09-24'), -- Concesionario
('27-33912845-1', 'ENTREGADA',  '2026-09-21', '2026-09-26'), -- Consumidor final
('30-71209874-3', 'CANCELADA',  '2026-09-24', NULL),       -- Vendedora de autopartes
('30-71623489-8', 'ENTREGADA',  '2026-09-26', '2026-10-01'), -- Empresa
('20-31789456-8', 'ENTREGADA',  '2026-09-28', '2026-10-03'), -- Consumidor final
('30-71890345-2', 'ENTREGADA',  '2026-09-30', '2026-10-05'), -- Concesionario

-- OCTUBRE 2026 (15 órdenes)
('30-70984512-1', 'ENTREGADA',  '2026-10-02', '2026-10-07'), -- Vendedora de autopartes
('30-71554890-6', 'ENTREGADA',  '2026-10-04', '2026-10-09'), -- Concesionario
('30-70541236-9', 'ENTREGADA',  '2026-10-06', '2026-10-12'), -- Empresa
('20-28456123-4', 'CANCELADA',  '2026-10-08', NULL),       -- Consumidor final
('30-71776543-0', 'ENTREGADA',  '2026-10-10', '2026-10-15'), -- Vendedora de autopartes
('30-71458923-5', 'ENTREGADA',  '2026-10-12', '2026-10-17'), -- Empresa
('30-71902341-7', 'LISTA',      '2026-10-15', NULL),       -- Concesionario
('27-33912845-1', 'ENTREGADA',  '2026-10-17', '2026-10-22'), -- Consumidor final
('30-71209874-3', 'ENTREGADA',  '2026-10-19', '2026-10-24'), -- Vendedora de autopartes
('20-31789456-8', 'CANCELADA',  '2026-10-21', NULL),       -- Consumidor final
('30-71623489-8', 'LISTA',      '2026-10-23', NULL),       -- Empresa
('30-71890345-2', 'ENTREGADA',  '2026-10-25', '2026-10-30'), -- Concesionario
('30-70984512-1', 'PRODUCCION', '2026-10-27', NULL),       -- Vendedora de autopartes
('30-70541236-9', 'ENTREGADA',  '2026-10-29', '2026-11-03'), -- Empresa
('30-71554890-6', 'LISTA',      '2026-10-31', NULL),       -- Concesionario

-- NOVIEMBRE 2026 (18 órdenes)
('30-71776543-0', 'ENTREGADA',  '2026-11-01', '2026-11-05'), -- Vendedora de autopartes
('30-71458923-5', 'ENTREGADA',  '2026-11-02', '2026-11-07'), -- Empresa
('30-71902341-7', 'ENTREGADA',  '2026-11-04', '2026-11-09'), -- Concesionario
('27-33912845-1', 'CANCELADA',  '2026-11-05', NULL),       -- Consumidor final
('30-71209874-3', 'ENTREGADA',  '2026-11-06', '2026-11-11'), -- Vendedora de autopartes
('30-71623489-8', 'LISTA',      '2026-11-08', NULL),       -- Empresa
('30-71890345-2', 'ENTREGADA',  '2026-11-09', '2026-11-14'), -- Concesionario
('30-70984512-1', 'PRODUCCION', '2026-11-10', NULL),       -- Vendedora de autopartes
('30-71554890-6', 'LISTA',      '2026-11-11', NULL),       -- Concesionario
('20-28456123-4', 'ENTREGADA',  '2026-11-12', '2026-11-17'), -- Consumidor final
('30-70541236-9', 'PRODUCCION', '2026-11-13', NULL),       -- Empresa
('20-31789456-8', 'PENDIENTE',  '2026-11-14', NULL),       -- Consumidor final
('30-71776543-0', 'PRODUCCION', '2026-11-15', NULL),       -- Vendedora de autopartes
('30-71458923-5', 'LISTA',      '2026-11-16', NULL),       -- Empresa
('30-71902341-7', 'PENDIENTE',  '2026-11-17', NULL),       -- Concesionario
('30-71209874-3', 'PRODUCCION', '2026-11-18', NULL),       -- Vendedora de autopartes
('27-33912845-1', 'PENDIENTE',  '2026-11-19', NULL),       -- Consumidor final
('30-71890345-2', 'PENDIENTE',  '2026-11-20', NULL);       -- Concesionario

INSERT INTO dbo.ORDEN_PEDIDO (idCliente, idEstadoOrden, fechaPedido, fechaEntrega)
SELECT
    c.idCliente,
    CASE s.codEstado
        WHEN 'PENDIENTE'  THEN @EstPendiente
        WHEN 'PRODUCCION' THEN @EstProduccion
        WHEN 'LISTA'      THEN @EstLista
        WHEN 'ENTREGADA'  THEN @EstEntregada
        WHEN 'CANCELADA'  THEN @EstCancelada
    END,
    s.fechaPedido,
    s.fechaEntrega
FROM @OrdenesSeed s
INNER JOIN dbo.CLIENTE c
    ON c.documentoCuit = s.cuitCliente;
GO