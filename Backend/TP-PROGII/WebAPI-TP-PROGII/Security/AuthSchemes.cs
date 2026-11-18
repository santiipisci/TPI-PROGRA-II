namespace WebAPI_TP_PROGII.Security;

/// <summary>
/// Nombres de los esquemas de autenticación por cookie. Se usan dos cookies
/// independientes: una para administradores y otra para clientes del sitio.
/// </summary>
public static class AuthSchemes
{
    public const string Admin = "AdminCookie";
    public const string Cliente = "ClienteCookie";
}
