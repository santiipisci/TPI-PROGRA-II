/* ========= Página Acerca de ========= */
UI.init("acerca");

(() => {
  Animations.init([
    ["main > .mb-4", "up", 0],
    ["main .row > .col-12 > .mtx-glass, main .row > .col-lg-6 > .mtx-glass", "up", 120],
    ["main .row.g-3 > *", "up", 100],
  ]);
})();
