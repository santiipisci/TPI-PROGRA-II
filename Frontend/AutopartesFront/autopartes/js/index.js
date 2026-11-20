/* ========= Página de inicio ========= */
UI.init("inicio");

(async () => {
  let models = [], parts = [], tipos = [];
  try {
    [models, parts, tipos] = await Promise.all([Api.modelos(), Api.autopartes(), Api.tiposCliente()]);
  } catch (e) {
    document.getElementById("featured").innerHTML = UI.stateBox(e.message, "error");
  }

  // Estadísticas (datos reales de la base)
  document.getElementById("stPartes").textContent = parts.length;
  document.getElementById("stModelos").textContent = models.length;
  document.getElementById("stMarcas").textContent = new Set(models.map((m) => m.marca)).size;
  document.getElementById("stTipos").textContent = tipos.length;

  // Segmentos de cliente (texto institucional, no son datos de negocio)
  const segments = [
    { icon: "bi-building", title: "Concesionarias", text: "Abastecimiento continuo para posventa y taller oficial, con facturación y entregas programadas." },
    { icon: "bi-shop", title: "Autopartistas", text: "Listas de precios mayoristas, reposición ágil y stock permanente de rotación alta." },
    { icon: "bi-truck", title: "Empresas y flotas", text: "Planes de mantenimiento para flotas, con cotización por volumen y atención prioritaria." },
    { icon: "bi-person", title: "Consumidor final", text: "Repuestos originales y equivalentes con garantía y asesoramiento personalizado." },
  ];
  document.getElementById("segments").innerHTML = segments
    .map(
      (s) => `
      <div class="col-sm-6 col-lg-3">
        <div class="mtx-glass mtx-segment-card">
          <div class="mtx-icon-tile mb-3"><i class="bi ${s.icon}"></i></div>
          <h3 class="h5 fw-bold">${s.title}</h3>
          <p class="mtx-text-muted-2 small mb-0">${s.text}</p>
        </div>
      </div>`
    )
    .join("");

  // Destacados: más compatibilidades + stock
  const featured = [...parts].sort((a, b) => b.modelos.length - a.modelos.length).slice(0, 4);
  if (parts.length) document.getElementById("featured").innerHTML = featured.map((p) => UI.partCard(p, models)).join("");

  // Buscador rápido
  const selMarca = document.getElementById("qMarca");
  const selModelo = document.getElementById("qModelo");
  const selAnio = document.getElementById("qAnio");

  const marcas = [...new Set(models.map((m) => m.marca))].sort();
  selMarca.innerHTML = `<option value="">Todas las marcas</option>` + marcas.map((m) => `<option>${UI.esc(m)}</option>`).join("");
  selModelo.innerHTML = `<option value="">Todos los modelos</option>`;
  const thisYear = new Date().getFullYear();
  let years = `<option value="">Cualquier año</option>`;
  for (let y = thisYear; y >= 2005; y--) years += `<option>${y}</option>`;
  selAnio.innerHTML = years;

  selMarca.addEventListener("change", () => {
    const list = models.filter((m) => m.marca === selMarca.value);
    selModelo.disabled = !selMarca.value;
    selModelo.innerHTML = `<option value="">Todos los modelos</option>` + list.map((m) => `<option>${UI.esc(m.nombre)}</option>`).join("");
  });

  document.getElementById("quickSearch").addEventListener("submit", (e) => {
    e.preventDefault();
    const q = new URLSearchParams();
    if (selMarca.value) q.set("marca", selMarca.value);
    if (selModelo.value) q.set("modelo", selModelo.value);
    if (selAnio.value) q.set("anio", selAnio.value);
    location.href = "partes.html" + (q.toString() ? "?" + q.toString() : "");
  });

/* ========= Animaciones al hacer scroll ========= */
(() => {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  // Contador animado para las estadísticas
  function countUp(el) {
    const target = parseInt(el.dataset.target, 10);
    if (reduce || isNaN(target)) { el.textContent = target; return; }
    const dur = 1200, t0 = performance.now();
    const tick = (t) => {
      const p = Math.min((t - t0) / dur, 1);
      el.textContent = Math.round(target * (1 - Math.pow(1 - p, 3)));
      if (p < 1) requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
  }
  ["stPartes", "stModelos", "stMarcas", "stTipos"].forEach((id) => {
    const el = document.getElementById(id);
    el.dataset.target = el.textContent;
    el.textContent = "0";
  });
  const stats = document.querySelector(".mtx-stat-num").closest(".mtx-glass");

  Animations.init(
    [
      [".mtx-hero .col-lg-7 > *", "up", 120],
      [".mtx-hero .col-lg-5 > *", "right", 0],
      ["main > section.container:nth-of-type(2) .mtx-glass", "zoom", 0],
      [".mtx-section .text-center", "up", 0],
      ["#segments > *", "up", 120],
      [".mtx-section .d-flex.justify-content-between", "up", 0],
      ["#featured > *", "up", 100],
      ["main > section.container:last-of-type .mtx-glass", "zoom", 0],
    ],
    { onReveal: (el) => { if (el === stats) document.querySelectorAll("[data-target]").forEach(countUp); } }
  );
})();
})();
