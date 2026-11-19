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

    public async Task<UsuarioAdmin?> GetAdminPorUsuarioAsync(string usuario)
    {
        var nombre = usuario.Trim();

        return await _context.UsuarioAdmins
                     .FirstOrDefaultAsync(u => u.Usuario == nombre && u.Activo);
    }

    public async Task<UsuarioAdmin?> GetAdminPorIdAsync(int idUsuario)
    {
        return await _context.UsuarioAdmins
                     .AsNoTracking()
                     .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.Activo);
    }
}
