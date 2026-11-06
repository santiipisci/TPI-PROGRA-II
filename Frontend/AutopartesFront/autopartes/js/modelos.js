/* ========= Modelos de autos y sus autopartes ========= */
UI.init("modelos");

(async () => {
  let models = [], parts = [];
  const grid = document.getElementById("modelsGrid");
  const search = document.getElementById("mBuscar");
  const modalEl = document.getElementById("modelModal");
  const modal = new bootstrap.Modal(modalEl);

  const partsOf = (id) => parts.filter((p) => p.modelos.includes(id));
  const years = (m) =>
    m.anioDesde != null && m.anioHasta != null
      ? `${m.anioDesde} – ${m.anioHasta >= new Date().getFullYear() ? "actualidad" : m.anioHasta}`
      : "";

  function render() {
    const q = search.value.trim().toLowerCase();
    const list = models
      .filter((m) => !q || (m.marca + " " + m.nombre).toLowerCase().includes(q))
      .sort((a, b) => (a.marca + a.nombre).localeCompare(b.marca + b.nombre));
    grid.innerHTML = list.length
      ? list.map((m) => `
        <div class="col-sm-6 col-lg-4 col-xl-3">
          <div class="glass model-card" data-id="${m.idModelo}" role="button" tabindex="0">
            <div class="model-brand">${UI.esc(m.marca)}</div>
            <h2 class="h5 fw-bold mb-1">${UI.esc(m.nombre)}</h2>
            <div class="small text-muted-2 mb-3">${years(m) || "&nbsp;"}</div>
            <span class="badge rounded-pill text-bg-danger">${partsOf(m.idModelo).length} autopartes</span>
          </div>
        </div>`).join("")
      : `<div class="col-12"><div class="glass p-4 text-center text-muted-2">No se encontraron modelos.</div></div>`;
    Animations.scan();
  }

  function open(id) {
    const m = models.find((x) => x.idModelo === id);
    if (!m) return;
    document.getElementById("mdBrand").textContent = m.marca;
    document.getElementById("mdTitle").textContent = m.nombre;
    document.getElementById("mdYears").textContent = years(m) ? `Años ${years(m)}` : "";
    const list = partsOf(id);
    document.getElementById("mdParts").innerHTML = list.length
      ? list.map((p) => `
        <div class="list-group-item bg-transparent d-flex justify-content-between align-items-center gap-3">
          <div><div class="fw-semibold">${UI.esc(p.descripcion)}</div><div class="part-sku">${UI.esc(p.codigoParte)} · ${UI.esc(p.categoria)}</div></div>
          <div class="text-end"><div class="fw-bold">${p.precio != null ? UI.money(p.precio) : "–"}</div><a class="small" href="contacto.html?codigo=${encodeURIComponent(p.codigoParte)}">Consultar</a></div>
        </div>`).join("")
      : `<p class="text-muted-2 mb-0">Todavía no hay autopartes asociadas a este modelo.</p>`;
    modal.show();
  }

  grid.addEventListener("click", (e) => {
    const c = e.target.closest(".model-card");
    if (c) open(Number(c.dataset.id));
  });
  grid.addEventListener("keydown", (e) => {
    if (e.key === "Enter" || e.key === " ") {
      const c = e.target.closest(".model-card");
      if (c) { e.preventDefault(); open(Number(c.dataset.id)); }
    }
  });
  search.addEventListener("input", render);
  grid.innerHTML = UI.stateBox("Cargando modelos…");
  try {
    [models, parts] = await Promise.all([Api.modelos(), Api.autopartes()]);
    render();
  } catch (e) {
    grid.innerHTML = UI.stateBox(e.message, "error");
  }
  Animations.init([
    ["main > .mb-4", "up", 0],
    ["main > .glass", "up", 0],
    ["#modelsGrid > *", "up", 60],
  ]);
})();
