namespace WebApiPeliculasFinal.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;

        private int cantidadRegistrosPagina = 10;
        private readonly int cantidadMaximaRegistrosPorPagina = 50;
        public int CantidadRegistrosPorPagina 
        { 
            get => cantidadRegistrosPagina;
            set
            {
                cantidadRegistrosPagina = (value > cantidadMaximaRegistrosPorPagina) ? cantidadMaximaRegistrosPorPagina : value;
            }
        }
    }
}
