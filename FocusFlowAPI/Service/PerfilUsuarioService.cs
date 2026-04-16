using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class PerfilUsuarioService
    {
        private readonly UsuarioContext _context;

        public PerfilUsuarioService(UsuarioContext context)
        {
            _context = context;
        }

        public async Task<PerfilUsuarioDto?> ObtenerPerfilAsync(Guid idUsuario)
        {
            var perfil = await _context.PerfilUsuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            return perfil == null ? null : MapToDto(perfil);
        }

        public async Task<PerfilUsuarioDto> CrearPerfilAsync(Guid idUsuario, PerfilUsuarioDto dto)
        {
            var existente = await _context.PerfilUsuarios
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            if (existente != null)
                return MapToDto(existente);

            var perfil = new PerfilUsuario
            {
                IdUsuario = idUsuario,
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Ocupacion = dto.Ocupacion,
                ObjetivoPrincipal = dto.ObjetivoPrincipal
            };

            _context.PerfilUsuarios.Add(perfil);
            await _context.SaveChangesAsync();

            return MapToDto(perfil);
        }

        public async Task<PerfilUsuarioDto?> ActualizarPerfilAsync(Guid idUsuario, PerfilUsuarioDto dto)
        {
            var perfil = await _context.PerfilUsuarios
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            if (perfil == null)
                return null;

            perfil.Nombre = dto.Nombre;
            perfil.Edad = dto.Edad;
            perfil.Ocupacion = dto.Ocupacion;
            perfil.ObjetivoPrincipal = dto.ObjetivoPrincipal;

            await _context.SaveChangesAsync();

            return MapToDto(perfil);
        }

        public async Task<bool> EliminarPerfilAsync(Guid idUsuario)
        {
            var perfil = await _context.PerfilUsuarios
                .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

            if (perfil == null)
                return false;

            _context.PerfilUsuarios.Remove(perfil);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PerfilUsuarioDto MapToDto(PerfilUsuario perfil)
        {
            return new PerfilUsuarioDto
            {
                IdPerfil = perfil.IdPerfil,
                IdUsuario = perfil.IdUsuario,
                Nombre = perfil.Nombre,
                Edad = perfil.Edad,
                Ocupacion = perfil.Ocupacion,
                ObjetivoPrincipal = perfil.ObjetivoPrincipal,
                FechaRegistro = perfil.FechaRegistro
            };
        }
    }
}
