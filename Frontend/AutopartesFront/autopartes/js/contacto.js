/* ========= Formulario de contacto → consulta enviada a la API ========= */
UI.init("contacto");

(async () => {
  const form = document.getElementById("contactForm");
  const $ = (id) => document.getElementById(id);
  const submitBtn = form.querySelector('[type="submit"]');

  Animations.init([
    ["main > .mb-4", "up", 0],
    ["#contactForm", "left", 0],
    [".col-lg-5 > .glass", "right", 150],
  ]);

  // Tipos de cliente (tabla TIPO_CLIENTE)
  submitBtn.disabled = true;
  try {
    const tipos = await Api.tiposCliente();
    $("cTipo").innerHTML = tipos.map((t) => `<option value="${t.idTipoCliente}">${UI.esc(t.descripcion)}</option>`).join("");
    submitBtn.disabled = false;
  } catch (e) {
    $("formAlert").innerHTML = `<div class="alert alert-danger small">${UI.esc(e.message)}</div>`;
  }

  // Prefill si viene desde una tarjeta de autoparte (?codigo=...)
  const codigo = new URLSearchParams(location.search).get("codigo");
  if (codigo) {
    try {
      const part = (await Api.autopartes()).find((p) => p.codigoParte === codigo);
      if (part) {
        $("cAsunto").value = `Consulta por ${part.descripcion} (${part.codigoParte})`;
        $("cMensaje").value = `Hola, quisiera consultar precio y disponibilidad de: ${part.descripcion} (${part.codigoParte}).`;
      }
    } catch (e) { /* el prefill es opcional */ }
  }

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const data = {
      nombre: $("cNombre").value.trim(),
      idTipoCliente: Number($("cTipo").value),
      email: $("cEmail").value.trim(),
      telefono: $("cTel").value.trim(),
      asunto: $("cAsunto").value.trim(),
      mensaje: $("cMensaje").value.trim(),
    };
    const emailOk = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email);
    if (!data.nombre || !data.asunto || !data.mensaje || !emailOk || !data.idTipoCliente) {
      UI.alertBox("warning", "Revisá el formulario", "Completá los campos obligatorios y verificá el email.");
      return;
    }
    submitBtn.disabled = true;
    try {
      await Api.enviarConsulta(data);
      form.reset();
      UI.alertBox("success", "¡Gracias!", "Recibimos tu consulta y te contactaremos pronto.");
    } catch (err) {
      UI.alertBox("error", "No pudimos enviar tu consulta", err.message);
    } finally {
      submitBtn.disabled = false;
    }
  });
})();
