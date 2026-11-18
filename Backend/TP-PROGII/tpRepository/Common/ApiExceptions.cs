namespace tpRepository.Common;

/// <summary>Recurso inexistente → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string mensaje) : base(mensaje) { }
}

/// <summary>Registro duplicado (código, email, CUIT, modelo) → HTTP 409.</summary>
public class DuplicateException : Exception
{
    public DuplicateException(string mensaje) : base(mensaje) { }
}

/// <summary>Credenciales inválidas o falta de sesión → HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string mensaje) : base(mensaje) { }
}

/// <summary>Dato inválido o regla de negocio incumplida → HTTP 400.</summary>
public class BusinessException : Exception
{
    public BusinessException(string mensaje) : base(mensaje) { }
}
