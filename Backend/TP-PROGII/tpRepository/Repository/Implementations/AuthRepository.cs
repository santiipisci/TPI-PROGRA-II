using Microsoft.EntityFrameworkCore;
using tpRepository.Models;
using tpRepository.Repository.Interfaces;

namespace tpRepository.Repository.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly AutopartesDbContext _context;

    public AuthRepository(AutopartesDbContext context)
    {
        _context = context;
    }

    public Task<UsuarioAdmin?> GetAdminPorUsuarioAsync(string usuario)
    {
        var nombre = usuario.Trim();
        return _context.UsuarioAdmins.FirstOrDefaultAsync(u => u.Usuario == nombre && u.Activo);
    }

    public Task<UsuarioAdmin?> GetAdminPorIdAsync(int idUsuario) =>
        _context.UsuarioAdmins.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo);
}
