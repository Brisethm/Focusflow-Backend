using FocusFlowAPI.DTOs;
using FocusFlowAPI.Models;
using FocusFlowAPI.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FocusFlow.Tests.Services
{
    public class TicketServiceTests : IDisposable
    {
        private readonly UsuarioContext _context;
        private readonly TicketService _service;
        private bool _disposed;

        public TicketServiceTests()
        {
            var options = new DbContextOptionsBuilder<UsuarioContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioContext(options);
            _service = new TicketService(_context);
        }

        [Fact]
        public async Task CreateTicket_DeberiaPersistirTicketYRetornarDto()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var dto = new CreateTicketDto 
            { 
                Asunto = "Error de carga", 
                Descripcion = "No carga el dashboard", 
                Categoria = "Bug", 
                Prioridad = "Alta" 
            };

            // Act
            var result = await _service.CreateTicket(idUsuario, dto);

            // Assert
            Assert.NotEqual(0, result.IdTicket);
            Assert.Equal("open", result.Estado);
            var entity = await _context.Tickets.FindAsync(result.IdTicket);
            Assert.NotNull(entity);
            Assert.Equal(idUsuario, entity!.IdUsuario);
            Assert.Equal(DateTimeKind.Utc, entity.FechaCreacion.Kind);
        }

        [Fact]
        public async Task IsStaffAsync_DeberiaRetornarTrue_ParaAdminYSupport()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var supportId = Guid.NewGuid();
            var idUsuario = Guid.NewGuid();

            _context.PerfilUsuarios.AddRange(
                new PerfilUsuario { IdUsuario = adminId, Nombre = "Admin", Rol = "admin" },
                new PerfilUsuario { IdUsuario = supportId, Nombre = "Support", Rol = "support" },
                new PerfilUsuario { IdUsuario = idUsuario, Nombre = "User", Rol = "user" }
            );
            await _context.SaveChangesAsync();

            // Act & Assert
            Assert.True(await _service.IsStaffAsync(adminId));
            Assert.True(await _service.IsStaffAsync(supportId));
            Assert.False(await _service.IsStaffAsync(idUsuario));
        }

        [Fact]
        public async Task AddResponse_DeberiaCambiarEstadoAInProgress_CuandoRespondeSoporte()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var supportId = Guid.NewGuid();
            var ticket = new Ticket 
            { 
                IdUsuario = idUsuario, 
                Asunto = "Test", 
                Estado = "open", 
                Prioridad = "Baja", 
                FechaCreacion = DateTime.UtcNow 
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.AddResponse(ticket.IdTicket, supportId, "Estamos revisando su caso");

            // Assert
            var ticketActualizado = await _context.Tickets.FindAsync(ticket.IdTicket);
            Assert.Equal("in_progress", ticketActualizado!.Estado);
            Assert.True(result!.EsSoporte);
        }

        [Fact]
        public async Task CancelTicket_DeberiaRetornarFalse_SiTicketNoPerteneceAlUsuario()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var ticket = new Ticket 
            { 
                IdUsuario = ownerId, 
                Asunto = "Test", 
                Estado = "open", 
                Prioridad = "Media", 
                FechaCreacion = DateTime.UtcNow 
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CancelTicket(ticket.IdTicket, strangerId);

            // Assert
            Assert.False(result);
            var entity = await _context.Tickets.FindAsync(ticket.IdTicket);
            Assert.Equal("open", entity!.Estado);
        }

        [Fact]
        public async Task GetTicketResponses_DeberiaMarcarEsSoporteCorrectamente()
        {
            // Arrange
            var idUsuario = Guid.NewGuid();
            var supportId = Guid.NewGuid();
            var ticket = new Ticket { IdUsuario = idUsuario, Asunto = "T", Prioridad = "P", FechaCreacion = DateTime.UtcNow };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            _context.RespuestasTickets.AddRange(
                new TicketRespuesta { IdTicket = ticket.IdTicket, IdAutor = idUsuario, Mensaje = "Ayuda", Fecha = DateTime.UtcNow },
                new TicketRespuesta { IdTicket = ticket.IdTicket, IdAutor = supportId, Mensaje = "Hola", Fecha = DateTime.UtcNow.AddMinutes(1) }
            );
            await _context.SaveChangesAsync();

            // Act
            var respuestas = (await _service.GetTicketResponses(ticket.IdTicket)).ToList();

            // Assert
            Assert.Equal(2, respuestas.Count);
            Assert.False(respuestas[0].EsSoporte);
            Assert.True(respuestas[1].EsSoporte);  
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Database.EnsureDeleted();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}