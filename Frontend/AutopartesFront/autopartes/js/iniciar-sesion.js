/* ========= Iniciar sesión de cliente =========
 * El login lo resuelve el backend (POST /clientes/login). El front no guarda nada en el navegador.
 */
UI.init("");

(async () => {
  const $ = (id) => document.getElementById(id);
  Animations.init([[".mtx-glass.p-4", "zoom", 0]]);

  // Si ya hay sesión de cliente, no tiene sentido mostrar el formulario
  try { if (await Api.clienteActual()) { location.replace("index.html"); return; } } catch (e) { /* sin sesión */ }


  /* ---------- Iniciar sesión ---------- */
  $("clienteLoginForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = $("liEmail").value.trim();
    const clave = $("liClave").value;
    if (!email || !clave) { UI.alertBox("warning", "Completá los datos", "Ingresá tu email y contraseña."); return; }
    const btn = e.target.querySelector('[type="submit"]');
    btn.disabled = true;
    try {
      await Api.loginCliente(email, clave);
      location.href = "index.html";
    } catch (err) {
      btn.disabled = false;
      UI.alertBox("error", "No pudimos ingresar", err.status === 401 || err.status === 400 ? "Email o contraseña incorrectos." : err.message);
    }
  });

})();
