/* ========= Registro de cliente =========
 * El alta la resuelve el backend (POST /clientes/registro). El front solo valida formato básico.
 */
UI.init("");

(async () => {
  const $ = (id) => document.getElementById(id);
  Animations.init([[".glass.p-4", "zoom", 0]]);

  // Si ya hay sesión de cliente, no tiene sentido mostrar el formulario
  try { if (await Api.clienteActual()) { location.replace("index.html"); return; } } catch (e) { /* sin sesión */ }


  /* ---------- Tipos de cliente (TIPO_CLIENTE) ---------- */
  try {
    const tipos = await Api.tiposCliente();
    $("rTipo").innerHTML = `<option value="">Seleccioná…</option>` + tipos.map((t) => `<option value="${t.idTipoCliente}">${UI.esc(t.descripcion)}</option>`).join("");
  } catch (e) {
    $("rTipo").innerHTML = `<option value="">No se pudieron cargar</option>`;
  }

  /* ---------- Registrarse ---------- */
  $("registroForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const dto = {
      idTipoCliente: Number($("rTipo").value),
      nombreRazonSocial: $("rNombre").value.trim(),
      documentoCuit: $("rDoc").value.trim(),
      telefono: $("rTel").value.trim(),
      direccion: $("rDir").value.trim(),
      email: $("rEmail").value.trim(),
      clave: $("rClave").value,
    };
    const warn = (t) => UI.alertBox("warning", "Revisá el formulario", t);
    if (!dto.idTipoCliente) return warn("Elegí el tipo de cliente.");
    if (!dto.nombreRazonSocial || !dto.telefono || !dto.direccion) return warn("Completá todos los campos.");
    if (!/^[0-9\-]{7,13}$/.test(dto.documentoCuit)) return warn("El documento / CUIL debe tener entre 7 y 13 dígitos (se admiten guiones).");
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(dto.email)) return warn("Verificá el email.");
    if (dto.clave.length < 8) return warn("La contraseña debe tener al menos 8 caracteres.");
    if (dto.clave !== $("rClave2").value) return warn("Las contraseñas no coinciden.");

    const btn = e.target.querySelector('[type="submit"]');
    btn.disabled = true;
    try {
      await Api.registrarCliente(dto); // el backend valida duplicados (documento/email) → 409
      await UI.alertBox("success", "¡Cuenta creada!", "Ya podés iniciar sesión con tu email y contraseña.");
      location.href = "iniciar-sesion.html";
    } catch (err) {
      UI.alertBox(err.status === 409 ? "warning" : "error", err.status === 409 ? "Ya existe una cuenta" : "No pudimos crear la cuenta", err.message);
    } finally {
      btn.disabled = false;
    }
  });
})();
