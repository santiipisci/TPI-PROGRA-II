/* ========= Cliente de la API (reemplaza a data.js) =========
 * El front NO guarda datos: todo viene y va a la API del proyecto de Visual Studio,
 * que es la dueña de la lógica de negocio y de la base de datos db_Automotriz.
 * Los nombres de campos siguen los de las tablas/columnas del script SQL.
 * Contrato completo de endpoints: API_CONTRATO.md
 */
class ApiError extends Error {
  constructor(message, status) { super(message); this.status = status; }
}

const Api = (() => {
  const base = () => String(window.MX_CONFIG.API_BASE).replace(/\/+$/, "");

  async function request(method, path, { body, query, noRedirect = false } = {}) {
    let url = base() + path;
    if (query) {
      const qs = new URLSearchParams();
      Object.entries(query).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== "") qs.set(k, v); });
      if (qs.toString()) url += "?" + qs.toString();
    }
    let res;
    try {
      res = await fetch(url, {
        method,
        credentials: "include", // cookie de sesión HttpOnly emitida por el backend
        headers: { Accept: "application/json", ...(body !== undefined ? { "Content-Type": "application/json" } : {}) },
        body: body !== undefined ? JSON.stringify(body) : undefined,
      });
    } catch (e) {
      throw new ApiError("No se pudo conectar con el servidor. Intentá de nuevo en unos minutos.", 0);
    }
    if (res.status === 401 && !noRedirect) { location.replace("login.html"); throw new ApiError("Sesión expirada.", 401); }
    let data = null;
    if (res.status !== 204) { try { data = await res.json(); } catch (e) { /* cuerpo vacío o no JSON */ } }
    if (!res.ok) {
      const msg = (data && (data.mensaje || data.detail || data.title)) || `Error del servidor (${res.status}).`;
      throw new ApiError(msg, res.status);
    }
    return data;
  }
  const get = (p, o) => request("GET", p, o);
  const post = (p, body, o) => request("POST", p, { ...o, body });
  const put = (p, body, o) => request("PUT", p, { ...o, body });
  const patch = (p, body, o) => request("PATCH", p, { ...o, body });
  const del = (p, o) => request("DELETE", p, o);

  return {
    /* --- Públicos --- */
    autopartes: () => get("/autopartes"),
    modelos: () => get("/modelos"),
    categorias: () => get("/categorias"),
    tiposCliente: () => get("/tipos-cliente"),
    enviarConsulta: (dto) => post("/consultas", dto),

    /* --- Cuenta de cliente (tabla CLIENTE) --- */
    registrarCliente: (dto) => post("/clientes/registro", dto, { noRedirect: true }),
    loginCliente: (email, clave) => post("/clientes/login", { email, clave }, { noRedirect: true }),
    clienteActual: () => get("/clientes/me", { noRedirect: true }),
    logoutCliente: () => post("/clientes/logout", undefined, { noRedirect: true }),

    /* --- Sesión de administrador --- */
    login: (usuario, clave) => post("/auth/login", { usuario, clave }, { noRedirect: true }),
    me: () => get("/auth/me", { noRedirect: true }),
    logout: () => post("/auth/logout", undefined, { noRedirect: true }),

    /* --- Administración --- */
    resumen: () => get("/admin/resumen"),
    estadosOrden: () => get("/estados-orden"),
    ordenes: (q) => get("/admin/ordenes", { query: q }),
    crearAutoparte: (dto) => post("/autopartes", dto),
    editarAutoparte: (id, dto) => put(`/autopartes/${id}`, dto),
    actualizarStock: (id, stockActual) => patch(`/autopartes/${id}/stock`, { stockActual }),
    eliminarAutoparte: (id) => del(`/autopartes/${id}`),
    asociarAutoparte: (idModelo, idProducto) => put(`/modelos/${idModelo}/autopartes/${idProducto}`),
    desasociarAutoparte: (idModelo, idProducto) => del(`/modelos/${idModelo}/autopartes/${idProducto}`),
    crearModelo: (dto) => post("/modelos", dto),
    consultas: () => get("/consultas"),
    estadoConsulta: (id, estado) => patch(`/consultas/${id}/estado`, { estado }),
    eliminarConsulta: (id) => del(`/consultas/${id}`),
  };
})();
