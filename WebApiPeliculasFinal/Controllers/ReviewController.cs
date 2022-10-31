﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApiPeliculasFinal.DTOs;
using WebApiPeliculasFinal.Entidades;
using WebApiPeliculasFinal.Helpers;

namespace WebApiPeliculasFinal.Controllers
{
    [Route("api/peliculas/{peliculaId:int}/reviews")]
    [ServiceFilter(typeof(PeliculaExisteAttribute))]
    public class ReviewController: CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ReviewController(ApplicationDbContext context,
            IMapper mapper)
            :base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> Get(int peliculaId,
            [FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Reviews.Include(x => x.Usuario).AsQueryable();
            queryable = queryable.Where(x => x.PeliculaId == peliculaId);
            return await Get<Review, ReviewDTO>(paginacionDTO, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int peliculaId, [FromBody] ReviewCreacionDTO reviewCreacionDTO)
        {
            var existePelicula = await context.Peliculas.AnyAsync(x => x.Id == peliculaId);

            if (!existePelicula)
            {
                return NotFound();
            }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            var reviewExiste = await context.Reviews
                .AnyAsync(x => x.PeliculaId == peliculaId && x.UsuarioId == usuarioId);

            if (reviewExiste)
            {
                return BadRequest("El usuario ya ha escrito una review de la película");
            }

            var review = mapper.Map<Review>(reviewCreacionDTO);
            review.PeliculaId = peliculaId;
            review.UsuarioId = usuarioId;

            context.Add(review);
            await context.SaveChangesAsync();
            return NoContent();

        }

        [HttpPut("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int peliculaId, int reviewId, 
            [FromBody] ReviewCreacionDTO reviewCreacionDTO)
        {
            var reviewDB = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);
            if(reviewDB == null) { return NotFound(); }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            if(reviewDB.UsuarioId != usuarioId)
            { 
                return BadRequest("No tiene permiso para editar este comentario"); 
            }

            reviewDB = mapper.Map(reviewCreacionDTO, reviewDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int reviewId)
        {
            var reviewDB = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);
            if (reviewDB == null) { return NotFound(); }

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if (reviewDB.UsuarioId != usuarioId) { return Forbid(); }

            context.Remove(reviewDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
