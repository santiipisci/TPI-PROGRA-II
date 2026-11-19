/* ========= Configuración del front =========
 * Única cosa a tocar al conectar con el proyecto de Visual Studio:
 * URL base de la API (ASP.NET). Debe permitir CORS con credenciales
 * desde el origen donde se sirve este sitio (ver API_CONTRATO.md).
 */
window.MX_CONFIG = {
  API_BASE: location.port === "5500" ? "https://localhost:7174/api" : "/api",
  // Casilla de la empresa: los botones "Consultar" abren Gmail con este destinatario ya cargado
  EMPRESA_EMAIL: "ventas@motorix.example",
};
