/* ========= Login de administrador =========
 * La autenticación la resuelve el backend (POST /auth/login). El front no conoce
 * credenciales ni guarda nada: la sesión es una cookie HttpOnly emitida por la API.
 */
UI.init("");

(async () => {
  Animations.init([["#loginForm", "zoom", 0]]);

  // Si ya hay sesión activa, ir directo al panel
  try { await Api.me(); location.replace("admin.html"); return; } catch (e) { /* sin sesión: mostrar formulario */ }

  const form = document.getElementById("loginForm");
  const btn = form.querySelector('[type="submit"]');
  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const usuario = document.getElementById("lUser").value.trim();
    const clave = document.getElementById("lPass").value;
    if (!usuario || !clave) { UI.alertBox("warning", "Completá los datos", "Ingresá usuario y contraseña."); return; }
    btn.disabled = true;
    try {
      await Api.login(usuario, clave);
      UI.notify("success", "¡Bienvenido!");
      setTimeout(() => { location.href = "admin.html"; }, 700);
    } catch (err) {
      btn.disabled = false;
      UI.alertBox("error", "No pudimos ingresar", err.status === 401 || err.status === 400 ? "Usuario o contraseña incorrectos." : err.message);
    }
  });
})();
