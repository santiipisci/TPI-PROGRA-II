/* ========= Catálogo de partes con filtros ========= */
UI.init("partes");

(async () => {
  let models = [], parts = [];
  const grid0 = document.getElementById("partsGrid");
  grid0.innerHTML = UI.stateBox("Cargando autopartes…");
  try {
    [models, parts] = await Promise.all([Api.modelos(), Api.autopartes()]);
  } catch (e) {
    grid0.innerHTML = UI.stateBox(e.message, "error");
    document.getElementById("resultCount").textContent = "";
    return;
  }

  const selMarca = document.getElementById("fMarca");
  const selModelo = document.getElementById("fModelo");
  const selAnio = document.getElementById("fAnio");
  const inpTexto = document.getElementById("fTexto");
  const grid = document.getElementById("partsGrid");
  const empty = document.getElementById("emptyState");
  const count = document.getElementById("resultCount");
  const catsEl = document.getElementById("cats");
  const tableWrap = document.getElementById("partsTableWrap");
  const tableBody = document.getElementById("partsTableBody");
  const viewSwitch = document.getElementById("viewSwitch");
  let categoria = "";
  let view = "cards"; // sin almacenamiento en el navegador: la vista no se recuerda entre visitas

  // Opciones de filtros
  const marcas = [...new Set(models.map((m) => m.marca))].sort();
  selMarca.innerHTML = `<option value="">Todas</option>` + marcas.map((m) => `<option>${UI.esc(m)}</option>`).join("");
  const thisYear = new Date().getFullYear();
  let years = `<option value="">Todos</option>`;
  for (let y = thisYear; y >= 2005; y--) years += `<option>${y}</option>`;
  selAnio.innerHTML = years;

  function fillModelos() {
    const list = models.filter((m) => !selMarca.value || m.marca === selMarca.value);
    selModelo.disabled = !selMarca.value;
    selModelo.innerHTML = `<option value="">Todos</option>` + list.map((m) => `<option>${UI.esc(m.nombre)}</option>`).join("");
  }
  fillModelos();

  // Chips de categoría
  const cats = [...new Set(parts.map((p) => p.categoria))].sort();
  function renderCats() {
    catsEl.innerHTML = ["", ...cats]
      .map((c) => `<button type="button" class="cat-chip ${c === categoria ? "active" : ""}" data-cat="${UI.esc(c)}">${c ? `${UI.catIcon(c, "me-1")}${UI.esc(c)}` : "Todas"}</button>`)
      .join("");
  }
  renderCats();
  catsEl.addEventListener("click", (e) => {
    const b = e.target.closest(".cat-chip");
    if (!b) return;
    categoria = b.dataset.cat;
    renderCats();
    render();
  });

  // Lectura de parámetros de URL (desde el buscador de inicio)
  const qs = new URLSearchParams(location.search);
  if (qs.get("marca") && marcas.includes(qs.get("marca"))) {
    selMarca.value = qs.get("marca");
    fillModelos();
    if (qs.get("modelo")) selModelo.value = qs.get("modelo");
  }
  if (qs.get("anio")) selAnio.value = qs.get("anio");

  function matches(p) {
    const txt = inpTexto.value.trim().toLowerCase();
    if (categoria && p.categoria !== categoria) return false;
    if (txt && !(p.descripcion + " " + p.codigoParte).toLowerCase().includes(txt)) return false;
    const marca = selMarca.value, modelo = selModelo.value, anio = Number(selAnio.value) || 0;
    if (!marca && !modelo && !anio) return true;
    // La parte debe ser compatible con al menos un modelo que cumpla todos los filtros
    return p.modelos.some((id) => {
      const m = models.find((x) => x.idModelo === id);
      if (!m) return false;
      if (marca && m.marca !== marca) return false;
      if (modelo && m.nombre !== modelo) return false;
      // El rango de años es opcional (lo deriva la API desde VEHICULO.anio); sin rango no se descarta el modelo
      if (anio && m.anioDesde != null && m.anioHasta != null && (anio < m.anioDesde || anio > m.anioHasta)) return false;
      return true;
    });
  }

  function render() {
    const list = parts.filter(matches);
    const isTable = view === "table";
    grid.classList.toggle("d-none", isTable || list.length === 0);
    tableWrap.classList.toggle("d-none", !isTable || list.length === 0);
    grid.innerHTML = isTable ? "" : list.map((p) => UI.partCard(p, models)).join("");
    tableBody.innerHTML = isTable ? list.map((p) => UI.partRow(p, models)).join("") : "";
    viewSwitch.checked = isTable;
    document.getElementById("vlCards").classList.toggle("active", !isTable);
    document.getElementById("vlTable").classList.toggle("active", isTable);
    empty.classList.toggle("d-none", list.length > 0);
    count.textContent = `${list.length} ${list.length === 1 ? "autoparte encontrada" : "autopartes encontradas"}`;
    Animations.scan(); // anima las tarjetas recién dibujadas
  }

  selMarca.addEventListener("change", () => { fillModelos(); render(); });
  [selModelo, selAnio].forEach((el) => el.addEventListener("change", render));
  inpTexto.addEventListener("input", render);
  viewSwitch.addEventListener("change", () => {
    view = viewSwitch.checked ? "table" : "cards";
    render();
  });
  document.getElementById("fLimpiar").addEventListener("click", () => {
    selMarca.value = ""; fillModelos(); selAnio.value = ""; inpTexto.value = ""; categoria = "";
    renderCats(); render();
  });
  document.getElementById("filters").addEventListener("submit", (e) => e.preventDefault());

  render();
  Animations.init([
    ["main > .mb-4", "up", 0],
    [".filter-bar", "up", 0],
    ["#partsGrid > *", "up", 70],
  ]);
})();
