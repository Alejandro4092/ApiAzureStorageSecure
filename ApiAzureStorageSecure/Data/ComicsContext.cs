using ApiAzureStorageSecure.Models;

using Microsoft.EntityFrameworkCore;

namespace ApiAzureStorageSecure.Data
{
    public class ComicsContext : DbContext
    {
        public ComicsContext(DbContextOptions<ComicsContext> options)
            : base(options) { }

        public DbSet<Comic> Comics { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}