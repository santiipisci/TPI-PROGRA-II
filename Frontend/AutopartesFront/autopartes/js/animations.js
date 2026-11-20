/* ========= Animaciones de aparición al hacer scroll (todas las páginas) =========
 * Uso: Animations.init([[selector, variante, escalonado_ms], ...], { onReveal })
 * Variantes: "up" (por defecto), "left", "right", "zoom".
 * Si el contenido se vuelve a dibujar (filtros, búsquedas), llamar Animations.scan().
 */
const Animations = (() => {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const BASE_PLAN = [["#app-footer .glass-footer .row > *", "up", 100]];
  let plan = [];
  let onReveal = null;
  let io = null;

  function show(el) {
    if (el.classList.contains("is-visible")) return;
    el.classList.add("is-visible");
    if (io) io.unobserve(el);
    setTimeout(() => el.classList.add("is-done"), 1500); // libera el delay para el hover
    if (onReveal) onReveal(el);
  }

  function scan() {
    if (!io) return;
    plan.forEach(([sel, variant = "up", step = 0]) => {
      document.querySelectorAll(sel).forEach((el, i) => {
        if (el.classList.contains("reveal")) return;
        el.classList.add("reveal");
        el.dataset.reveal = variant;
        el.style.setProperty("--d", Math.min(i * step, 700) + "ms");
        io.observe(el);
      });
    });
  }

  function setupParallax() {
    let ticking = false;
    const update = () => {
      const y = scrollY;
      document.documentElement.style.setProperty("--parallax", Math.min(y * 0.12, 120) + "px");
      // Al llegar al final de la página, mostrar lo que haya quedado pendiente
      if (y + innerHeight >= document.documentElement.scrollHeight - 2) {
        document.querySelectorAll(".reveal:not(.is-visible)").forEach(show);
      }
      ticking = false;
    };
    addEventListener("scroll", () => { if (!ticking) { ticking = true; requestAnimationFrame(update); } }, { passive: true });
    update();
  }

  function init(pagePlan = [], opts = {}) {
    plan = [...pagePlan, ...BASE_PLAN];
    onReveal = opts.onReveal || null;
    io = new IntersectionObserver(
      (entries) => entries.forEach((e) => { if (e.isIntersecting) show(e.target); }),
      { threshold: 0.12, rootMargin: "0px 0px -6% 0px" }
    );
    scan();
    if (!reduce) setupParallax();
  }

  return { init, scan };
})();
