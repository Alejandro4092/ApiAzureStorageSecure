using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiAzureStorageSecure.Models
{
    [Table("COMICS")]
    public class Comic
    {
        [Key]
        [Column("ID_COMIC")]
        public int IdComic { get; set; }

        [Column("TITULO")]
        public string Titulo { get; set; }

        [Column("IMAGEN")]
        public string Imagen { get; set; }   // URL del blob en Azure Blob Storage

        [Column("DESCRIPCION")]
        public string Descripcion { get; set; }

        [Column("YEAR")]
        public int Year { get; set; }
    }
}