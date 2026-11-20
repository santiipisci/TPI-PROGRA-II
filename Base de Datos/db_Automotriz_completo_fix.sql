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
CREATE TABLE MODELO (
    idModelo   INT IDENTITY(1,1) PRIMARY KEY,
    idMarca    INT NOT NULL REFERENCES MARCA(idMarca),
    nombre     VARCHAR(50) NOT NULL,
    anioDesde  SMALLINT NULL CHECK (anioDesde >= 1900),
    anioHasta  SMALLINT NULL,
    CONSTRAINT UQ_Modelo UNIQUE (idMarca, nombre),
    CONSTRAINT CK_ModeloAnios CHECK (
        (anioDesde IS NULL AND anioHasta IS NULL)
        OR (anioDesde IS NOT NULL AND anioHasta IS NOT NULL AND anioHasta >= anioDesde)
    )
);

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
-- [CAMBIO] email NOT NULL UNIQUE (es el usuario de login) y claveHash (hash generado por el backend, nunca texto plano).
-- claveHash NULL = cliente cargado por el negocio que todavía no creó su cuenta en el sitio.
CREATE TABLE CLIENTE (
    idCliente         INT IDENTITY(1,1) PRIMARY KEY,
    idTipoCliente     INT          NOT NULL REFERENCES TIPO_CLIENTE(idTipoCliente),
    nombreRazonSocial VARCHAR(100) NOT NULL,
    documentoCuit     VARCHAR(13)  NOT NULL UNIQUE,
    direccion         VARCHAR(150) NULL,
    telefono          VARCHAR(20)  NULL,
    email             VARCHAR(100) NOT NULL UNIQUE,
    claveHash         VARCHAR(255) NULL,
    fechaAlta         DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
);

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
CREATE TABLE ORDEN_PEDIDO (
    idOrdenPedido INT IDENTITY(1,1) PRIMARY KEY,
    idCliente     INT  NOT NULL REFERENCES CLIENTE(idCliente),
    idEstadoOrden INT  NOT NULL REFERENCES ESTADO_ORDEN(idEstadoOrden),
    fechaPedido   DATE NOT NULL,
    fechaEntrega  DATE NULL,
    CONSTRAINT CK_OrdenFechas CHECK (fechaEntrega IS NULL OR fechaEntrega >= fechaPedido)
);

CREATE TABLE DETALLE_ORDEN_PEDIDO (
    idOrdenPedido INT NOT NULL REFERENCES ORDEN_PEDIDO(idOrdenPedido),
    idProducto    INT NOT NULL REFERENCES PRODUCTO(idProducto),
    cantidad      INT NOT NULL CHECK (cantidad > 0),
    PRIMARY KEY (idOrdenPedido, idProducto)
);

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
CREATE INDEX IX_Autoparte_Categoria    ON AUTOPARTE(idCategoria);
CREATE INDEX IX_AutoparteModelo_Modelo ON AUTOPARTE_MODELO(idModelo);
CREATE INDEX IX_Orden_Cliente          ON ORDEN_PEDIDO(idCliente);
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
   La clave se guarda con hash, así que no se puede insertar desde SQL.
   Crear el primer administrador desde el backend (por ejemplo, un endpoint
   o un seeder que genere el hash con PasswordHasher) y que inserte en USUARIO_ADMIN.
   --------------------------------------------------------------------- */

