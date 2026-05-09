using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Services
{
    public class PerfilUsuarioService
    {
        private readonly UsuarioContext _context;
        private readonly ILogger<PerfilUsuarioService> _logger;

        public PerfilUsuarioService(UsuarioContext context, ILogger<PerfilUsuarioService> logger)
        {
            _context = context;
            _logger = logger;
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
            {
                existente.Nombre = dto.Nombre;
                existente.Edad = dto.Edad;
                existente.Ocupacion = dto.Ocupacion;
                existente.ObjetivoPrincipal = dto.ObjetivoPrincipal;

                if (!string.IsNullOrWhiteSpace(dto.Rol))
                    existente.Rol = dto.Rol;

                await _context.SaveChangesAsync();
                return MapToDto(existente);
            }

            var perfil = new PerfilUsuario
            {
                IdUsuario = idUsuario,
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Ocupacion = dto.Ocupacion,
                ObjetivoPrincipal = dto.ObjetivoPrincipal,
                FechaRegistro = DateTime.UtcNow,
                Rol = dto.Rol
            };

            _context.PerfilUsuarios.Add(perfil);
            await _context.SaveChangesAsync();
            return MapToDto(perfil);
        }

        public async Task<PerfilUsuarioDto?> ActualizarPerfilAsync(Guid idUsuario, PerfilUsuarioDto dto)
        {
            _logger.LogInformation("[PerfilUsuario] Actualizando perfil del usuario {IdUsuario}", idUsuario);

            var filas = await _context.PerfilUsuarios
                .Where(p => p.IdUsuario == idUsuario)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Nombre, dto.Nombre)
                    .SetProperty(p => p.Edad, dto.Edad)
                    .SetProperty(p => p.Ocupacion, dto.Ocupacion)
                    .SetProperty(p => p.ObjetivoPrincipal, dto.ObjetivoPrincipal)
                );

            if (filas == 0)
                return null;

            _logger.LogInformation("[PerfilUsuario] Perfil del usuario {IdUsuario} actualizado correctamente.", idUsuario);

            return new PerfilUsuarioDto
            {
                IdUsuario = idUsuario,
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Ocupacion = dto.Ocupacion,
                ObjetivoPrincipal = dto.ObjetivoPrincipal
            };
        }

        public async Task<bool> EliminarPerfilAsync(Guid idUsuario)
        {
            var filas = await _context.PerfilUsuarios
                .Where(p => p.IdUsuario == idUsuario)
                .ExecuteDeleteAsync();

            return filas > 0;
        }

        private static PerfilUsuarioDto MapToDto(PerfilUsuario perfil)
        {
            return new PerfilUsuarioDto
            {
                IdPerfil = perfil.IdPerfil,
                IdUsuario = perfil.IdUsuario,
                Nombre = perfil.Nombre,
                Rol = perfil.Rol,
                Edad = perfil.Edad,
                Ocupacion = perfil.Ocupacion,
                ObjetivoPrincipal = perfil.ObjetivoPrincipal,
                FechaRegistro = perfil.FechaRegistro
            };
        }
    }
}
