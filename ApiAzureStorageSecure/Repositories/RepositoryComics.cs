using ApiAzureStorageSecure.Data;
using ApiAzureStorageSecure.Models;

using Microsoft.EntityFrameworkCore;

namespace ApiAzureStorageSecure.Repositories
{
    public class RepositoryComics
    {
        private ComicsContext context;

        public RepositoryComics(ComicsContext context)
        {
            this.context = context;
        }

        // ── Comics ────────────────────────────────────────────────────────

        public async Task<List<Comic>> GetComicsAsync()
        {
            return await this.context.Comics.ToListAsync();
        }

        public async Task<Comic> FindComicAsync(int id)
        {
            return await this.context.Comics
                .Where(c => c.IdComic == id)
                .FirstOrDefaultAsync();
        }

        // ── Usuarios / Login ──────────────────────────────────────────────

        public async Task<Usuario> LoginAsync(string username, string password)
        {
            return await this.context.Usuarios
                .Where(u => u.Username == username && u.Password == password)
                .FirstOrDefaultAsync();
        }
    }
}