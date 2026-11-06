/* ========= Panel de administración =========
 * Todo se lee y se escribe contra la API (proyecto de Visual Studio). Este archivo solo
 * presenta datos y envía acciones: no calcula precios, descuentos ni reglas de negocio.
 */
(async () => {
  UI.init("");
  try {
    await Api.me(); // 401 → Api redirige a login.html
  } catch (e) {
    if (e.status !== 401) UI.alertBox("error", "No se pudo verificar la sesión", e.message);
    return;
  }
  document.getElementById("adminShell").hidden = false;
  initAdmin();
})();

function initAdmin() {
  const $ = (id) => document.getElementById(id);
  let parts = [];
  let models = [];
  let cats = [];      // CATEGORIA_AUTOPARTE
  let estados = [];   // ESTADO_ORDEN
  let tickets = [];
  let resumen = null;
  let selectedModel = null;
  let ticketFilter = "todos";
  let charts = {};

  const fail = (title, e) => UI.alertBox("error", title, e.message || String(e));

  $("logoutBtn").addEventListener("click", async () => {
    try { await Api.logout(); } catch (e) { /* igual salimos a login */ }
    location.href = "login.html";
  });

  /* ---------- Estados de orden (ESTADO_ORDEN.descripcion) ---------- */
  const ESTADO_STYLE = {
    "pendiente": { badge: "text-bg-warning", color: "#f59e0b" },
    "en producción": { badge: "text-bg-primary", color: "#3b82f6" },
    "lista": { badge: "text-bg-info", color: "#06b6d4" },
    "entregada": { badge: "text-bg-success", color: "#22c55e" },
    "cancelada": { badge: "text-bg-secondary", color: "#94a3b8" },
  };
  const estadoStyle = (d) => ESTADO_STYLE[String(d).toLowerCase()] || { badge: "text-bg-secondary", color: "#8b5cf6" };

  /* ---------- Dashboard ---------- */
  function renderKpis() {
    $("kStock").textContent = parts.reduce((a, p) => a + p.stockActual, 0).toLocaleString("es-AR");
    $("kMes").textContent = resumen ? UI.money(resumen.ventasUltimoMes) : "–";
    $("kAnio").textContent = resumen ? UI.money(resumen.ventasUltimoAnio) : "–";
    $("kOrdenes").textContent = resumen ? resumen.ordenesTotales.toLocaleString("es-AR") : "–";
  }

  function chartColors() {
    const dark = UI.getTheme() === "dark";
    return { text: dark ? "#cbd2e6" : "#374151", grid: dark ? "rgba(255,255,255,.08)" : "rgba(0,0,0,.07)" };
  }

  function renderCharts() {
    Object.values(charts).forEach((c) => c.destroy());
    charts = {};
    if (!resumen) return;
    const col = chartColors();
    Chart.defaults.color = col.text;
    Chart.defaults.font.family = "Inter, system-ui, sans-serif";

    // Torta de estados
    const porEstado = resumen.ordenesPorEstado || [];
    charts.estados = new Chart($("chartEstados"), {
      type: "pie",
      data: {
        labels: porEstado.map((e) => e.estado),
        datasets: [{ data: porEstado.map((e) => e.cantidad), backgroundColor: porEstado.map((e) => estadoStyle(e.estado).color), borderWidth: 2, borderColor: "transparent" }],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: "bottom" } } },
    });

    // Barras: ventas por mes (la API envía { anio, mes, total }; los meses sin dato se completan con 0)
    const now = new Date();
    const months = [];
    for (let i = 11; i >= 0; i--) months.push(new Date(now.getFullYear(), now.getMonth() - i, 1));
    const totals = months.map((m) => {
      const r = (resumen.ventasPorMes || []).find((x) => x.anio === m.getFullYear() && x.mes === m.getMonth() + 1);
      return r ? r.total : 0;
    });
    charts.ventas = new Chart($("chartVentas"), {
      type: "bar",
      data: {
        labels: months.map((m) => m.toLocaleDateString("es-AR", { month: "short", year: "2-digit" })),
        datasets: [{ label: "Ventas (ARS)", data: totals, backgroundColor: "#e63946", borderRadius: 8 }],
      },
      options: {
        responsive: true, maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { callbacks: { label: (c) => UI.money(c.parsed.y) } } },
        scales: { y: { grid: { color: col.grid }, ticks: { callback: (v) => "$" + (v / 1e6).toFixed(1) + "M" } }, x: { grid: { display: false } } },
      },
    });
  }

  function renderLowStock() {
    const low = parts.filter(UI.isLowStock).sort((a, b) => a.stockActual - b.stockActual);
    $("lowStock").innerHTML = low.length
      ? low.map((p) => `<tr><td class="part-sku">${UI.esc(p.codigoParte)}</td><td>${UI.esc(p.descripcion)}</td><td>${UI.esc(p.categoria)}</td><td class="text-end text-muted-2">${p.stockMinimo}</td><td class="text-end"><span class="badge ${p.stockActual <= 0 ? "text-bg-danger" : "text-bg-warning"}">${p.stockActual}</span></td></tr>`).join("")
      : UI.stateRow(5, "Sin alertas de stock.");
  }

  async function loadResumen() {
    try { resumen = await Api.resumen(); } catch (e) { resumen = null; fail("No se pudo cargar el resumen", e); }
    renderKpis();
    renderCharts();
  }

  /* ---------- Historial de ventas (órdenes de pedido, paginado en el servidor) ---------- */
  const HIST_STEP = 10;
  const hist = { pagina: 0, items: [], totalRegistros: 0, sumaTotal: 0, seq: 0 };

  function renderHistory() {
    $("histBody").innerHTML = hist.items.length
      ? hist.items.map((o) => {
          const st = estadoStyle(o.estado);
          return `<tr>
            <td class="part-sku">#${o.idOrdenPedido}</td>
            <td class="text-nowrap">${UI.fecha(o.fechaPedido)}</td>
            <td>${UI.esc(o.cliente)}</td>
            <td>${UI.esc(o.tipoCliente)}</td>
            <td><span class="badge ${st.badge}">${UI.esc(o.estado)}</span></td>
            <td class="text-end fw-semibold text-nowrap">${UI.money(o.total)}</td>
          </tr>`;
        }).join("")
      : UI.stateRow(6, "No hay ventas que coincidan con el filtro.");
    $("histResumen").textContent = hist.totalRegistros ? `Mostrando ${hist.items.length} de ${hist.totalRegistros} · Total: ${UI.money(hist.sumaTotal)}` : "";
    $("histMore").classList.toggle("d-none", hist.items.length >= hist.totalRegistros);
  }

  async function loadHistory(reset) {
    const seq = ++hist.seq; // descarta respuestas viejas si el filtro cambió mientras cargaba
    if (reset) { hist.pagina = 0; hist.items = []; $("histBody").innerHTML = UI.stateRow(6, "Cargando…"); }
    try {
      const r = await Api.ordenes({
        idEstadoOrden: $("histEstado").value === "todos" ? "" : $("histEstado").value,
        q: $("histBuscar").value.trim(),
        pagina: hist.pagina + 1,
        tamano: HIST_STEP,
      });
      if (seq !== hist.seq) return;
      hist.pagina += 1;
      hist.items = hist.items.concat(r.items);
      hist.totalRegistros = r.totalRegistros;
      hist.sumaTotal = r.sumaTotal;
      renderHistory();
    } catch (e) {
      if (seq !== hist.seq) return;
      $("histBody").innerHTML = UI.stateRow(6, e.message, "error");
    }
  }
  let histTimer;
  $("histEstado").addEventListener("change", () => loadHistory(true));
  $("histBuscar").addEventListener("input", () => { clearTimeout(histTimer); histTimer = setTimeout(() => loadHistory(true), 300); });
  $("histMore").addEventListener("click", () => loadHistory(false));

  /* ---------- Inventario (AUTOPARTE + PRODUCTO + PRECIO_PRODUCTO vigente) ---------- */
  const catOptions = (sel) => cats.map((c) => `<option value="${c.idCategoria}" ${c.idCategoria === sel ? "selected" : ""}>${UI.esc(c.nombre)}</option>`).join("");

  function renderInventory() {
    const q = $("invBuscar").value.trim().toLowerCase();
    const cat = $("invCat").value;
    const list = parts.filter((p) => (!cat || String(p.idCategoria) === cat) && (!q || `${p.descripcion} ${p.codigoParte}`.toLowerCase().includes(q)));
    if (!list.length) {
      $("invBody").innerHTML = UI.stateRow(6, "No se encontraron autopartes.");
      return;
    }
    $("invBody").innerHTML = list
      .map((p) => `
      <tr>
        <td class="part-sku">${UI.esc(p.codigoParte)}</td>
        <td>${UI.esc(p.descripcion)}</td>
        <td>${UI.esc(p.categoria)}</td>
        <td>${p.precio != null ? UI.money(p.precio) : "–"}</td>
        <td><input type="number" min="0" class="form-control form-control-sm stock-input" data-id="${p.idProducto}" value="${p.stockActual}"></td>
        <td class="text-end text-nowrap">
          <button class="btn btn-sm btn-glass me-1" data-edit="${p.idProducto}" title="Editar" aria-label="Editar ${UI.esc(p.descripcion)}"><i class="bi bi-pencil"></i></button>
          <button class="btn btn-sm btn-glass text-danger" data-del="${p.idProducto}" title="Dar de baja" aria-label="Dar de baja ${UI.esc(p.descripcion)}"><i class="bi bi-trash"></i></button>
        </td>
      </tr>`).join("");
  }

  function renderCategoryControls() {
    $("npCat").innerHTML = catOptions();
    $("invCat").innerHTML = `<option value="">Todas las categorías</option>` + cats.map((c) => `<option value="${c.idCategoria}">${UI.esc(c.nombre)}</option>`).join("");
  }

  async function reloadParts() {
    try {
      parts = await Api.autopartes();
    } catch (e) {
      fail("No se pudo actualizar el inventario", e);
      return;
    }
    refreshParts();
  }

  $("invBody").addEventListener("change", async (e) => {
    const inp = e.target.closest(".stock-input");
    if (!inp) return;
    const p = parts.find((x) => x.idProducto === Number(inp.dataset.id));
    if (!p) return;
    const v = Math.max(0, parseInt(inp.value, 10) || 0);
    inp.disabled = true;
    try {
      await Api.actualizarStock(p.idProducto, v);
      p.stockActual = v;
      inp.value = v;
      renderKpis(); renderLowStock();
    } catch (err) {
      inp.value = p.stockActual; // vuelve al valor guardado
      fail("No se pudo actualizar el stock", err);
    } finally {
      inp.disabled = false;
    }
  });
  $("invBuscar").addEventListener("input", renderInventory);
  $("invCat").addEventListener("change", renderInventory);

  async function editPart(p) {
    const { value } = await Swal.fire({
      title: "Editar autoparte",
      html: `
        <div class="text-start">
          <p class="small text-muted-2 mb-3">Código: <span class="part-sku">${UI.esc(p.codigoParte)}</span></p>
          <label class="form-label small" for="epNombre">Descripción</label>
          <input id="epNombre" class="form-control mb-3" maxlength="150" value="${UI.esc(p.descripcion)}">
          <label class="form-label small" for="epCat">Categoría</label>
          <select id="epCat" class="form-select mb-3">${catOptions(p.idCategoria)}</select>
          <label class="form-label small" for="epPrecio">Precio vigente (ARS)</label>
          <input id="epPrecio" type="number" min="0" step="0.01" class="form-control" value="${p.precio ?? ""}">
          <div class="form-text">Al cambiar el precio, el sistema registra uno nuevo en el historial de precios.</div>
        </div>`,
      showCancelButton: true,
      confirmButtonText: "Guardar cambios",
      cancelButtonText: "Cancelar",
      reverseButtons: true,
      buttonsStyling: false,
      focusConfirm: false,
      customClass: { confirmButton: "btn btn-accent mx-1", cancelButton: "btn btn-glass mx-1" },
      preConfirm: () => {
        const descripcion = document.getElementById("epNombre").value.trim();
        const idCategoria = Number(document.getElementById("epCat").value);
        const raw = document.getElementById("epPrecio").value;
        const precio = Number(raw);
        if (!descripcion) { Swal.showValidationMessage("La descripción no puede estar vacía."); return false; }
        if (raw === "" || !Number.isFinite(precio) || precio <= 0) { Swal.showValidationMessage("Ingresá un precio válido (mayor a 0)."); return false; }
        return { descripcion, idCategoria, precio };
      },
    });
    if (!value) return;
    try {
      await Api.editarAutoparte(p.idProducto, value);
    } catch (e) { fail("No se pudo guardar la autoparte", e); return; }
    await reloadParts();
    UI.notify("success", "Autoparte actualizada");
  }

  $("invBody").addEventListener("click", async (e) => {
    const ed = e.target.closest("[data-edit]");
    if (ed) {
      const p = parts.find((x) => x.idProducto === Number(ed.dataset.edit));
      if (p) editPart(p);
      return;
    }
    const b = e.target.closest("[data-del]");
    if (!b) return;
    const id = Number(b.dataset.del);
    const p = parts.find((x) => x.idProducto === id);
    if (!p) return;
    const ok = await UI.confirmBox({ title: "¿Dar de baja la autoparte?", text: `"${p.descripcion}" dejará de mostrarse en el catálogo. El historial de ventas se conserva.`, confirmText: "Sí, dar de baja", icon: "warning", danger: true });
    if (!ok) return;
    try {
      await Api.eliminarAutoparte(id);
    } catch (err) { fail("No se pudo dar de baja la autoparte", err); return; }
    await reloadParts();
    UI.notify("success", "Autoparte dada de baja");
  });

  $("partForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const dto = {
      codigoParte: $("npSku").value.trim(),
      descripcion: $("npNombre").value.trim(),
      idCategoria: Number($("npCat").value),
      unidadMedida: $("npUnidad").value.trim() || "unidad",
      precio: Number($("npPrecio").value),
      stockActual: Math.max(0, parseInt($("npStock").value, 10) || 0),
      stockMinimo: Math.max(0, parseInt($("npMin").value, 10) || 0),
    };
    if (!(dto.precio > 0)) { UI.alertBox("warning", "Precio inválido", "El precio debe ser mayor a 0."); return; }
    const btn = form.querySelector('[type="submit"]');
    btn.disabled = true;
    try {
      await Api.crearAutoparte(dto); // el backend valida, p. ej. código duplicado (409)
      form.reset();
      await reloadParts();
      UI.notify("success", "Autoparte agregada");
    } catch (err) {
      UI.alertBox(err.status === 409 ? "warning" : "error", err.status === 409 ? "Código duplicado" : "No se pudo agregar", err.message);
    } finally {
      btn.disabled = false;
    }
  });

  /* ---------- Compatibilidad autoparte ↔ modelo (AUTOPARTE_MODELO) ---------- */
  const yearsOf = (m) => (m.anioDesde != null && m.anioHasta != null ? `${m.anioDesde}–${m.anioHasta}` : "");
  const countFor = (idModelo) => parts.filter((p) => p.modelos.includes(idModelo)).length;

  function renderAssocModels() {
    const sorted = [...models].sort((a, b) => (a.marca + a.nombre).localeCompare(b.marca + b.nombre));
    $("assocModels").innerHTML = sorted.length
      ? sorted.map((m) => `
      <div class="assoc-model ${m.idModelo === selectedModel ? "active" : ""}" data-id="${m.idModelo}">
        <div class="fw-semibold">${UI.esc(m.marca)} ${UI.esc(m.nombre)}</div>
        <div class="small text-muted-2">${yearsOf(m) ? yearsOf(m) + " · " : ""}${countFor(m.idModelo)} partes</div>
      </div>`).join("")
      : `<div class="small text-muted-2 p-2">Todavía no hay modelos cargados.</div>`;
    $("brandList").innerHTML = [...new Set(models.map((m) => m.marca))].sort().map((b) => `<option value="${UI.esc(b)}"></option>`).join("");
  }

  function renderAssocParts() {
    const m = models.find((x) => x.idModelo === selectedModel);
    if (!m) { $("assocParts").innerHTML = ""; $("asCount").textContent = ""; $("asBrand").textContent = "Seleccioná un modelo"; $("asModel").textContent = ""; return; }
    $("asBrand").textContent = m.marca;
    $("asModel").textContent = yearsOf(m) ? `${m.nombre} (${yearsOf(m)})` : m.nombre;
    $("asCount").textContent = `${countFor(m.idModelo)} asociadas`;
    $("assocParts").innerHTML = parts
      .map((p) => `
      <div class="col-md-6">
        <label class="glass p-2 px-3 d-flex align-items-center gap-2 w-100" style="cursor:pointer;border-radius:.8rem">
          <input class="form-check-input mt-0" type="checkbox" data-part="${p.idProducto}" ${p.modelos.includes(m.idModelo) ? "checked" : ""}>
          <span><span class="d-block small fw-semibold">${UI.esc(p.descripcion)}</span><span class="part-sku">${UI.esc(p.codigoParte)} · ${UI.esc(p.categoria)}</span></span>
        </label>
      </div>`).join("");
  }

  $("assocModels").addEventListener("click", (e) => {
    const el = e.target.closest(".assoc-model");
    if (!el) return;
    selectedModel = Number(el.dataset.id);
    renderAssocModels(); renderAssocParts();
  });
  $("assocParts").addEventListener("change", async (e) => {
    const cb = e.target.closest("[data-part]");
    if (!cb || selectedModel == null) return;
    const p = parts.find((x) => x.idProducto === Number(cb.dataset.part));
    if (!p) return;
    const want = cb.checked;
    cb.disabled = true;
    try {
      await (want ? Api.asociarAutoparte(selectedModel, p.idProducto) : Api.desasociarAutoparte(selectedModel, p.idProducto));
      p.modelos = want ? [...new Set([...p.modelos, selectedModel])] : p.modelos.filter((id) => id !== selectedModel);
      renderAssocModels();
      $("asCount").textContent = `${countFor(selectedModel)} asociadas`;
    } catch (err) {
      cb.checked = !want;
      fail("No se pudo guardar la asociación", err);
    } finally {
      cb.disabled = false;
    }
  });

  $("modelForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const dto = { marca: $("nmMarca").value.trim(), nombre: $("nmModelo").value.trim() };
    if (!dto.marca || !dto.nombre) return;
    const btn = form.querySelector('[type="submit"]');
    btn.disabled = true;
    try {
      const created = await Api.crearModelo(dto); // el backend crea la MARCA si no existe
      models = await Api.modelos();
      selectedModel = created && created.idModelo != null ? created.idModelo : selectedModel;
      form.reset();
      renderAssocModels(); renderAssocParts();
      UI.notify("success", "Modelo agregado");
    } catch (err) {
      UI.alertBox(err.status === 409 ? "warning" : "error", err.status === 409 ? "Modelo duplicado" : "No se pudo agregar el modelo", err.message);
    } finally {
      btn.disabled = false;
    }
  });

  /* ---------- Soporte (consultas del formulario de contacto) ---------- */
  function renderTickets() {
    const open = tickets.filter((t) => t.estado === "abierto").length;
    $("ticketBadge").textContent = open;
    $("ticketBadge").classList.toggle("d-none", open === 0);
    const list = tickets.filter((t) => ticketFilter === "todos" || t.estado === ticketFilter);
    $("ticketList").innerHTML = list.length
      ? list.map((t) => `
      <div class="glass ticket ${t.estado}">
        <div class="d-flex justify-content-between flex-wrap gap-2">
          <div>
            <span class="badge ${t.estado === "abierto" ? "text-bg-warning" : "text-bg-success"} me-2">${t.estado === "abierto" ? "Abierta" : "Resuelta"}</span>
            <span class="badge text-bg-secondary">${UI.esc(t.tipoCliente)}</span>
            <h3 class="h6 fw-bold mt-2 mb-1">${UI.esc(t.asunto)}</h3>
          </div>
          <div class="small text-muted-2">${UI.parseDate(t.fecha).toLocaleString("es-AR")}</div>
        </div>
        <p class="small mb-2" style="white-space:pre-wrap">${UI.esc(t.mensaje)}</p>
        <div class="small text-muted-2 mb-3"><i class="bi bi-person me-1"></i>${UI.esc(t.nombre)} · <i class="bi bi-envelope ms-2 me-1"></i>${UI.esc(t.email)}${t.telefono ? ` · <i class="bi bi-telephone ms-2 me-1"></i>${UI.esc(t.telefono)}` : ""}</div>
        <div class="d-flex gap-2">
          <a class="btn btn-sm btn-accent" href="mailto:${encodeURIComponent(t.email)}?subject=${encodeURIComponent("Re: " + t.asunto)}"><i class="bi bi-reply me-1"></i>Responder</a>
          <button class="btn btn-sm btn-glass" data-toggle="${t.idConsulta}">${t.estado === "abierto" ? "Marcar resuelta" : "Reabrir"}</button>
          <button class="btn btn-sm btn-glass text-danger" data-remove="${t.idConsulta}"><i class="bi bi-trash"></i></button>
        </div>
      </div>`).join("")
      : `<div class="glass p-4 text-center text-muted-2">No hay consultas para mostrar.</div>`;
  }

  $("ticketList").addEventListener("click", async (e) => {
    const tg = e.target.closest("[data-toggle]");
    const rm = e.target.closest("[data-remove]");
    try {
      if (tg) {
        const t = tickets.find((x) => x.idConsulta === Number(tg.dataset.toggle));
        if (!t) return;
        const next = t.estado === "abierto" ? "resuelto" : "abierto";
        await Api.estadoConsulta(t.idConsulta, next);
        t.estado = next;
      } else if (rm) {
        const ok = await UI.confirmBox({ title: "¿Eliminar consulta?", text: "Esta acción no se puede deshacer.", confirmText: "Sí, eliminar", icon: "warning", danger: true });
        if (!ok) return;
        const id = Number(rm.dataset.remove);
        await Api.eliminarConsulta(id);
        tickets = tickets.filter((x) => x.idConsulta !== id);
        UI.notify("success", "Consulta eliminada");
      } else return;
    } catch (err) { fail("No se pudo completar la acción", err); return; }
    renderTickets();
  });
  $("ticketFilter").addEventListener("click", (e) => {
    const b = e.target.closest("[data-f]");
    if (!b) return;
    ticketFilter = b.dataset.f;
    document.querySelectorAll("#ticketFilter .btn").forEach((x) => x.classList.toggle("active", x === b));
    renderTickets();
  });

  /* ---------- Render global y carga inicial ---------- */
  function refreshParts() {
    renderKpis(); renderLowStock(); renderInventory(); renderAssocModels(); renderAssocParts();
  }

  Animations.init([
    [".admin-shell > .d-flex", "up", 0],
    [".admin-tabs", "up", 0],
    ["#tab-dashboard .kpi", "up", 90],
    ["#tab-dashboard .row.g-4 > *", "up", 120],
  ]);
  document.addEventListener("themechange", renderCharts);

  $("histBody").innerHTML = UI.stateRow(6, "Cargando…");
  $("lowStock").innerHTML = UI.stateRow(5, "Cargando…");
  $("invBody").innerHTML = UI.stateRow(6, "Cargando…");

  // Cada bloque carga por separado: si uno falla, el resto del panel sigue funcionando
  (async () => {
    try {
      [parts, models, cats] = await Promise.all([Api.autopartes(), Api.modelos(), Api.categorias()]);
      selectedModel = models[0] ? models[0].idModelo : null;
      renderCategoryControls();
      refreshParts();
    } catch (e) {
      const msg = UI.stateRow(6, e.message, "error");
      $("invBody").innerHTML = msg; $("lowStock").innerHTML = UI.stateRow(5, e.message, "error");
    }
  })();
  (async () => {
    try {
      estados = await Api.estadosOrden();
      $("histEstado").innerHTML = `<option value="todos">Todos los estados</option>` + estados.map((s) => `<option value="${s.idEstadoOrden}">${UI.esc(s.descripcion)}</option>`).join("");
    } catch (e) { /* el filtro queda en "todos" */ }
    loadHistory(true);
  })();
  loadResumen();
  (async () => {
    try { tickets = await Api.consultas(); } catch (e) { $("ticketList").innerHTML = `<div class="glass p-4 text-center text-danger">${UI.esc(e.message)}</div>`; return; }
    renderTickets();
  })();
}
