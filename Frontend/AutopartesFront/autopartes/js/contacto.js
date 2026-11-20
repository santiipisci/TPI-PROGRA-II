/* ========= Contacto: abre Gmail con el mail de la empresa como destinatario ========= */
UI.init("contacto");

(() => {
  document.getElementById("gmailBtn").href = UI.gmailLink("Consulta desde el sitio web", "Hola, quisiera consultar por...");
  Animations.init([
    ["main > .mb-4", "up", 0],
    [".col-lg-7 > .glass", "left", 0],
    [".col-lg-5 > .glass", "right", 150],
  ]);
})();
