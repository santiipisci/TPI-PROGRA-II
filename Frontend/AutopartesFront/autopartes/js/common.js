/* ========= Utilidades compartidas: tema, header, footer ========= */
const UI = (() => {
  // Sin almacenamiento en el navegador: el tema sigue al sistema y el botón lo cambia solo en la página actual.
  let currentTheme = null;

  const CAT_ICONS = {
    "Frenos": "bi-disc",
    "Suspensión": "bi-activity",
    "Motor": "bi-gear-wide-connected",
    "Filtros": "bi-funnel",
    "Transmisión": "bi-nut",
    "Eléctrico": "bi-lightning-charge",
    "Escape": "bi-wind",
    "Refrigeración": "bi-thermometer-snow",
  };


  /* Íconos propios (SVG, trazo con currentColor) para categorías sin ícono adecuado en Bootstrap Icons */
  const CAT_SVG = {
    // Filtro de aire cónico de alto flujo: cuello, brida, cuerpo con pliegues y tapa
    "Filtros": `
      <rect x="26" y="5" width="12" height="9" rx="1.5"/>
      <rect x="20" y="14" width="24" height="5" rx="2"/>
      <path d="M18 19h28l6 31H12z"/>
      <path d="M25 22l-3 25M32 22v25M39 22l3 25"/>
      <rect x="12" y="50" width="40" height="7" rx="2.5"/>`,
    // Disco de freno con pinza: disco con campana, orificios y pinza al borde
    "Frenos": `
      <path d="M42.7 17A21 21 0 1 0 42.7 47"/>
      <circle cx="28" cy="32" r="7"/><circle cx="28" cy="32" r="2.4"/>
      <circle cx="28" cy="18.5" r="1.6"/><circle cx="40.8" cy="27.8" r="1.6"/>
      <circle cx="35.9" cy="42.9" r="1.6"/><circle cx="20.1" cy="42.9" r="1.6"/>
      <circle cx="15.2" cy="27.8" r="1.6"/>
      <path d="M43 17H57Q62 17 62 22V42Q62 47 57 47H43Q48 32 43 17Z"/>
      <circle cx="56.5" cy="24" r="1.8"/><circle cx="56.5" cy="40" r="1.8"/>`,
    // Motor V8 visto desde arriba: dos bancadas de 4 cilindros, múltiple de admisión y polea
    "Motor": `
      <rect x="9" y="8" width="17" height="42" rx="2.5"/>
      <rect x="38" y="8" width="17" height="42" rx="2.5"/>
      <circle cx="17.5" cy="16" r="2.6"/><circle cx="17.5" cy="25" r="2.6"/>
      <circle cx="17.5" cy="34" r="2.6"/><circle cx="17.5" cy="43" r="2.6"/>
      <circle cx="46.5" cy="16" r="2.6"/><circle cx="46.5" cy="25" r="2.6"/>
      <circle cx="46.5" cy="34" r="2.6"/><circle cx="46.5" cy="43" r="2.6"/>
      <rect x="27" y="12" width="10" height="34" rx="3"/>
      <path d="M29 20h6M29 28h6M29 36h6"/>
      <rect x="29" y="3" width="6" height="9" rx="1.5"/>
      <rect x="9" y="50" width="46" height="5" rx="2"/>
      <circle cx="32" cy="59" r="3.6"/>`,
    // Amortiguador completo con resorte: soporte superior, vástago, resorte helicoidal, cuerpo y ojal
    "Suspensión": `
      <rect x="25" y="2" width="14" height="5" rx="2"/>
      <path d="M32 7V24M17 22h30M17 52h30"/>
      <path d="M17 26L28.5 27.9M35.5 29.1L47 31M17 32.5L28.5 34.4M35.5 35.6L47 37.5M17 39L28.5 40.9M35.5 42.1L47 44M17 45.5L28.5 47.4M35.5 48.6L47 50.5"/>
      <rect x="28.5" y="24" width="7" height="28" rx="2"/>
      <path d="M32 52v2.5"/>
      <circle cx="32" cy="58.5" r="3.5"/>`,
  };

  function catIcon(cat, extraClass = "") {
    if (CAT_SVG[cat]) {
      return `<svg class="cat-svg ${extraClass}" viewBox="0 0 64 64" width="1em" height="1em" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${CAT_SVG[cat]}</svg>`;
    }
    return `<i class="bi ${CAT_ICONS[cat] || "bi-gear"} ${extraClass}"></i>`;
  }

  const esc = (s) =>
    String(s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

  const money = (n) =>
    new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 0 }).format(n);

  // Fechas: "yyyy-MM-dd" (columnas DATE) se interpreta en hora local para no correrse un día
  const parseDate = (v) => (typeof v === "string" && /^\d{4}-\d{2}-\d{2}$/.test(v) ? new Date(v + "T00:00:00") : new Date(v));
  const fecha = (v) => (v ? parseDate(v).toLocaleDateString("es-AR") : "–");

  // Bloques de estado para contenedores que se cargan desde la API
  const stateBox = (msg, kind = "muted") =>
    `<div class="col-12"><div class="glass p-4 text-center ${kind === "error" ? "text-danger" : "text-muted-2"}">${kind === "error" ? '<i class="bi bi-exclamation-triangle me-2"></i>' : ""}${esc(msg)}</div></div>`;
  const stateRow = (cols, msg, kind = "muted") =>
    `<tr><td colspan="${cols}" class="${kind === "error" ? "text-danger" : "text-muted-2"}">${esc(msg)}</td></tr>`;

  /* ---------- Tema ---------- */
  function getTheme() {
    if (currentTheme) return currentTheme;
    return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }
  function applyTheme(t) {
    document.documentElement.setAttribute("data-bs-theme", t);
    document.querySelectorAll(".theme-toggle i").forEach((i) => {
      i.className = t === "dark" ? "bi bi-sun-fill" : "bi bi-moon-stars-fill";
    });
  }
  function toggleTheme() {
    const next = getTheme() === "dark" ? "light" : "dark";
    currentTheme = next;
    applyTheme(next);
    document.dispatchEvent(new CustomEvent("themechange", { detail: next }));
  }
  // Aplicar lo antes posible para evitar parpadeo
  applyTheme(getTheme());

  /* ---------- Logo ---------- */
  const LOGO = `
    <svg class="brand-logo" viewBox="0 0 48 48" aria-hidden="true">
      <defs><linearGradient id="lg" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0" stop-color="#e63946"/><stop offset="1" stop-color="#ff7b00"/></linearGradient></defs>
      <rect width="48" height="48" rx="12" fill="url(#lg)"/>
      <g fill="none" stroke="#fff" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="24" cy="24" r="7"/>
        <path d="M24 8v5M24 35v5M8 24h5M35 24h5M12.7 12.7l3.5 3.5M31.8 31.8l3.5 3.5M12.7 35.3l3.5-3.5M31.8 16.2l3.5-3.5"/>
      </g>
    </svg>`;

  /* ---------- Header / Footer ---------- */
  const NAV = [
    { id: "inicio", href: "index.html", label: "Inicio" },
    { id: "partes", href: "partes.html", label: "Partes en venta" },
    { id: "modelos", href: "modelos.html", label: "Modelos de autos" },
    { id: "contacto", href: "contacto.html", label: "Contacto" },
  ];

  function renderHeader(active) {
    const el = document.getElementById("app-header");
    if (!el) return;
    el.innerHTML = `
    <nav class="navbar navbar-expand-lg sticky-top glass-navbar">
      <div class="container">
        <a class="navbar-brand d-flex align-items-center gap-2" href="index.html">
          ${LOGO}<span class="brand-name fs-5">Motorix<span>.</span> Autopartes</span>
        </a>
        <button class="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#mainNav" aria-label="Menú">
          <i class="bi bi-list fs-2"></i>
        </button>
        <div class="collapse navbar-collapse" id="mainNav">
          <ul class="navbar-nav mx-auto mb-2 mb-lg-0 gap-lg-1">
            ${NAV.map((n) => `<li class="nav-item"><a class="nav-link ${n.id === active ? "active" : ""}" href="${n.href}">${n.label}</a></li>`).join("")}
          </ul>
          <div class="d-flex align-items-center gap-2 flex-wrap" id="authArea">
            <a class="btn btn-glass" href="iniciar-sesion.html"><i class="bi bi-box-arrow-in-right me-1"></i>Iniciar sesión</a>
            <a class="btn btn-accent" href="registro.html"><i class="bi bi-person-plus me-1"></i>Registrarse</a>
            <button class="btn btn-glass theme-toggle" type="button" aria-label="Cambiar tema"><i class="bi bi-moon-stars-fill"></i></button>
          </div>
        </div>
      </div>
    </nav>`;
  }

  function renderFooter() {
    const el = document.getElementById("app-footer");
    if (!el) return;
    el.innerHTML = `
    <footer class="glass-footer mt-5 pt-5 pb-3">
      <div class="container">
        <div class="row g-4">
          <div class="col-lg-4">
            <div class="d-flex align-items-center gap-2 mb-3">${LOGO}<span class="brand-name fs-5">Motorix<span>.</span> Autopartes</span></div>
            <p class="text-muted-2 small mb-0">Fabricación y venta de autopartes para concesionarias, autopartistas, empresas y consumidor final.</p>
          </div>
          <div class="col-6 col-lg-2">
            <h6 class="fw-bold">Sitio</h6>
            <ul class="list-unstyled small">
              ${NAV.map((n) => `<li class="mb-1"><a class="footer-link" href="${n.href}">${n.label}</a></li>`).join("")}
            </ul>
          </div>
          <div class="col-6 col-lg-3">
            <h6 class="fw-bold">Contacto</h6>
            <ul class="list-unstyled small text-muted-2">
              <li class="mb-1"><i class="bi bi-telephone me-2"></i>+54 351 000-0000</li>
              <li class="mb-1"><i class="bi bi-envelope me-2"></i>ventas@motorix.example</li>
              <li class="mb-1"><i class="bi bi-clock me-2"></i>Lun a Vie · 8 a 17 hs</li>
            </ul>
          </div>
          <div class="col-lg-3">
            <h6 class="fw-bold">Atención</h6>
            <p class="small text-muted-2 mb-0">Listas de precios diferenciales para concesionarias y autopartistas. Consultá por venta mayorista.</p>
          </div>
        </div>
        <hr class="opacity-25 mt-4">
        <div class="d-flex flex-column flex-md-row justify-content-between align-items-center small text-muted-2 gap-2">
          <span>© ${new Date().getFullYear()} Motorix Autopartes. Todos los derechos reservados.</span>
          <a class="footer-link admin-link" href="login.html"><i class="bi bi-shield-lock me-1"></i>Acceso administrador</a>
        </div>
      </div>
    </footer>`;
  }

  /* ---------- Avisos con SweetAlert2 (con respaldo nativo si no carga el CDN) ---------- */
  const BTN = {
    confirmButton: "btn btn-accent mx-1",
    cancelButton: "btn btn-glass mx-1",
    denyButton: "btn btn-danger mx-1",
  };
  const swal = () => (window.Swal ? Swal.mixin({ buttonsStyling: false, reverseButtons: true, customClass: BTN }) : null);

  // Aviso breve, no bloqueante (esquina superior derecha)
  function notify(icon, title) {
    const S = swal();
    if (!S) return Promise.resolve();
    return S.fire({ toast: true, position: "top-end", icon, title, showConfirmButton: false, timer: 2600 });
  }
  // Mensaje modal con botón Aceptar
  function alertBox(icon, title, text = "") {
    const S = swal();
    if (!S) { window.alert(text ? `${title}\n${text}` : title); return Promise.resolve(); }
    return S.fire({ icon, title, text, confirmButtonText: "Aceptar" });
  }
  // Confirmación → Promise<boolean>
  async function confirmBox({ title, text = "", confirmText = "Confirmar", icon = "question", danger = false }) {
    if (!window.Swal) return window.confirm(text ? `${title}\n${text}` : title);
    const r = await Swal.fire({
      icon, title, text, showCancelButton: true, confirmButtonText: confirmText, cancelButtonText: "Cancelar",
      reverseButtons: true, buttonsStyling: false, focusCancel: danger,
      customClass: { ...BTN, confirmButton: danger ? "btn btn-danger mx-1" : BTN.confirmButton },
    });
    return r.isConfirmed;
  }

  /* ---------- Stock: se compara contra stockMinimo (columna AUTOPARTE.stockMinimo) ---------- */
  const isLowStock = (p) => p.stockActual <= p.stockMinimo;
  function stockBadge(p) {
    return p.stockActual <= 0 ? `<span class="badge text-bg-danger">Sin stock</span>`
      : isLowStock(p) ? `<span class="badge text-bg-warning">Últimas unidades</span>`
      : `<span class="badge text-bg-success">En stock</span>`;
  }

  /* ---------- Tarjeta de autoparte reutilizable ---------- */
  function partCard(p, models) {
    const compat = (p.modelos || [])
      .map((id) => models.find((m) => m.idModelo === id))
      .filter(Boolean);
    const shown = compat.slice(0, 3).map((m) => `<span class="compat-chip">${esc(m.marca)} ${esc(m.nombre)}</span>`).join("");
    const more = compat.length > 3 ? `<span class="compat-chip">+${compat.length - 3}</span>` : "";
    const badge = stockBadge(p).replace('class="badge', 'class="stock-badge badge');
    const thumb = p.imagenUrl
      ? `<img class="part-img" src="${esc(p.imagenUrl)}" alt="${esc(p.descripcion)}" loading="lazy">`
      : catIcon(p.categoria);
    return `
    <div class="col-sm-6 col-lg-4 col-xl-3">
      <article class="glass part-card">
        <div class="part-thumb" data-cat="${esc(p.categoria)}">
          ${thumb}${badge}
        </div>
        <div class="p-3 d-flex flex-column flex-grow-1">
          <span class="part-sku">${esc(p.codigoParte)} · ${esc(p.categoria)}</span>
          <h3 class="h6 fw-bold my-1">${esc(p.descripcion)}</h3>
          <div class="my-2">${shown}${more}</div>
          <div class="mt-auto d-flex align-items-center justify-content-between">
            <span class="fw-bold fs-5">${p.precio != null ? money(p.precio) : "Consultar"}</span>
            <a class="btn btn-accent btn-sm" href="contacto.html?codigo=${encodeURIComponent(p.codigoParte)}">Consultar</a>
          </div>
        </div>
      </article>
    </div>`;
  }

  /* ---------- Fila de autoparte para la vista en grilla ---------- */
  function partRow(p, models) {
    const compat = (p.modelos || []).map((id) => models.find((m) => m.idModelo === id)).filter(Boolean);
    const shown = compat.slice(0, 2).map((m) => `<span class="compat-chip">${esc(m.marca)} ${esc(m.nombre)}</span>`).join("");
    const more = compat.length > 2 ? `<span class="compat-chip">+${compat.length - 2}</span>` : "";
    return `
    <tr>
      <td class="part-sku part-row-sku">${esc(p.codigoParte)}</td>
      <td class="fw-semibold">${esc(p.descripcion)}</td>
      <td>${catIcon(p.categoria, "me-1")}${esc(p.categoria)}</td>
      <td>${shown}${more || (shown ? "" : '<span class="text-muted-2 small">–</span>')}</td>
      <td class="text-end fw-bold text-nowrap">${p.precio != null ? money(p.precio) : "–"}</td>
      <td>${stockBadge(p)}</td>
      <td class="text-end"><a class="btn btn-accent btn-sm" href="contacto.html?codigo=${encodeURIComponent(p.codigoParte)}">Consultar</a></td>
    </tr>`;
  }

  /* ---------- Sesión de cliente: si hay cuenta activa, reemplaza los botones por el nombre y "Salir" ---------- */
  async function initAuth() {
    const area = document.getElementById("authArea");
    if (!area || typeof Api === "undefined") return;
    let cliente;
    try { cliente = await Api.clienteActual(); } catch (e) { return; } // sin sesión: quedan los botones
    if (!cliente) return;
    const theme = area.querySelector(".theme-toggle");
    area.innerHTML = `<span class="small d-flex align-items-center"><i class="bi bi-person-circle fs-5 me-2"></i>${esc(cliente.nombreRazonSocial)}</span>
      <button class="btn btn-glass" type="button" id="clienteLogout"><i class="bi bi-box-arrow-right me-1"></i>Salir</button>`;
    area.appendChild(theme);
    document.getElementById("clienteLogout").addEventListener("click", async () => {
      try { await Api.logoutCliente(); } catch (e) { /* igual se recarga */ }
      location.href = "index.html";
    });
    applyTheme(getTheme());
  }

  function init(active) {
    renderHeader(active);
    initAuth();
    renderFooter();
    applyTheme(getTheme());
    document.addEventListener("click", (e) => {
      if (e.target.closest(".theme-toggle")) toggleTheme();
    });
  }

  return { init, esc, money, fecha, parseDate, stateBox, stateRow, isLowStock, getTheme, applyTheme, toggleTheme, partCard, partRow, catIcon, CAT_ICONS, LOGO, notify, alertBox, confirmBox };
})();
