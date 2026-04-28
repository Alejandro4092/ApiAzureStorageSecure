using ApiAzureStorageSecure.Models;
using ApiAzureStorageSecure.Repositories;
using ApiAzureStorageSecure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAzureStorageSecure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ComicsController : ControllerBase
    {
        private RepositoryComics repo;
        private ServiceStorageBlobs serviceStorage;

        public ComicsController(RepositoryComics repo, ServiceStorageBlobs serviceStorage)
        {
            this.repo = repo;
            this.serviceStorage = serviceStorage;
        }

        /// <summary>
        /// GET: api/Comics
        /// Ahora requiere Token Bearer en la cabecera.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Comic>>> GetComics()
        {
            List<Comic> comics = await this.repo.GetComicsAsync();

            foreach (Comic c in comics)
            {
                if (!string.IsNullOrEmpty(c.Imagen))
                {
                    // Si ya es una URL externa (http), no la tocamos
                    if (!c.Imagen.StartsWith("http"))
                    {
                        c.Imagen = this.serviceStorage.GetBlobUrl(c.Imagen);
                    }
                }
            }

            return Ok(comics);
        }

        /// <summary>
        /// GET: api/Comics/{id}
        /// También requiere Token.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Comic>> FindComic(int id)
        {
            Comic comic = await this.repo.FindComicAsync(id);

            if (comic == null)
            {
                return NotFound(new { message = $"Comic con ID {id} no encontrado." });
            }

            if (!string.IsNullOrEmpty(comic.Imagen))
            {
                if (!comic.Imagen.StartsWith("http"))
                {
                    comic.Imagen = this.serviceStorage.GetBlobUrl(comic.Imagen);
                }
            }

            return Ok(comic);
        }

        // Si quisieras que un método fuera PÚBLICO aunque la clase tenga [Authorize]
        // podrías ponerle encima el atributo [AllowAnonymous]
    }
}