using FocusFlowAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusFlowAPI.Models
{
    public class UsuarioContext : DbContext
    {
        public UsuarioContext(DbContextOptions<UsuarioContext> options) : base(options) { }

        public DbSet<PerfilUsuario> PerfilUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PerfilUsuario>().ToTable("perfil_usuario");
            modelBuilder.Entity<PlanPersonalizado>().ToTable("planes_personalizados");
            modelBuilder.Entity<Ticket>().ToTable("tickets");
            modelBuilder.Entity<TicketRespuesta>().ToTable("respuestas_tickets");
        }

        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }
        public DbSet<SesionEnfoque> SesionesEnfoque { get; set; }
        public DbSet<RegistroEmocional> RegistrosEmocionales { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Cuestionario> Cuestionarios { get; set; }
        public DbSet<RespuestaCuestionario> RespuestasCuestionarios { get; set; }
        public DbSet<PlanPersonalizado> PlanesPersonalizados { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketRespuesta> RespuestasTickets { get; set; }
    }
}
