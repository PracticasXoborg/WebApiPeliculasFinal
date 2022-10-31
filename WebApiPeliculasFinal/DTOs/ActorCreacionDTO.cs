using System.ComponentModel.DataAnnotations;
using WebApiPeliculasFinal.Validaciones;

namespace WebApiPeliculasFinal.DTOs
{
    public class ActorCreacionDTO: ActorPatchDTO
    {


        //Archivo de imagen
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes:4)]
        [TipoArchivoValidacion(grupoTipoArchivo:GrupoTipoArchivo.Imagen)]
        public IFormFile Foto { get; set; }
    }
}
