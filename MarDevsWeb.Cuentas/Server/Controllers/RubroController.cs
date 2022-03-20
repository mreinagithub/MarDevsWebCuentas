using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Server.Models;
using MarDevsWeb.Cuentas.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RubroController : MiBaseController
    {
        public RubroController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<RubroDTO>>> Get()
        {

            var queryable = RubrosUsuario                
                .OrderBy(c => c.Descripcion);

            return await queryable.Select(c => new RubroDTO
            {
                Id = c.Id.Value,                
                Descripcion = c.Descripcion,
                Color = c.Color ?? "#000000"

            }).ToListAsync();
        }
        [HttpGet("obtenerModeloRubro/{rubroId?}")]
        public async Task<EditarRubroDTO> GetModeloRubro(Guid? rubroId = null)
        {

            var modelo = new EditarRubroDTO();

            if (rubroId != null)
            {
                var rubro = await RubrosUsuario.FirstOrDefaultAsync(c => c.Id == rubroId.Value);
                if (rubro == null)
                    throw new ExcepcionNegocios("El rubro que intenta modificar no fue encontrado.");

                modelo.RubroId = rubro.Id.Value;             
                modelo.Descripcion = rubro.Descripcion;
                modelo.Color = rubro.Color;
            }         

            return modelo;

        }

        [HttpPost("editar-rubro")]
        public async Task<ActionResult> EditarRubro([FromBody] EditarRubroDTO rubroEditar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Rubro rubro;
                    if (rubroEditar.RubroId != null)
                    {
                        rubro = await RubrosUsuario.FirstOrDefaultAsync(c => c.Id == rubroEditar.RubroId);
                        if (rubro == null)
                            throw new ExcepcionNegocios("No se encontró el rubro a editar.");                        
                    }
                    else
                    {
                        rubro = new Rubro();
                    }
                    
                    rubroEditar.Descripcion = rubroEditar.Descripcion.Trim();

                    var existe = await RubrosUsuario.AnyAsync(c => c.Id != rubro.Id && c.Descripcion.Equals(rubroEditar.Descripcion));
                    if (existe)
                        throw new ExcepcionNegocios("Ya existe un rubro con la misma descripción.\nNo puede cargar otro.");
                                        
                    rubro.Descripcion = rubroEditar.Descripcion;
                    rubro.Color = rubroEditar.Color;


                    if (rubro.Id != null)
                    {
                        _context.Update(rubro);
                    }
                    else
                    {
                        rubro.Id = Guid.NewGuid();
                        rubro.CreadoEl = DateTime.Now;
                        rubro.CreadoPor = YO;
                        _context.Add(rubro);
                    }

                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (ExcepcionNegocios exN)
                {
                    return BadRequest(exN.Message);
                }
                catch (Exception ex)
                {
                    throw WrapException(ex);
                }
            }
            else
            {
                string err = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToString();
                return BadRequest(err);
            }
        }

        [HttpDelete("eliminar/{rubroId}")]
        public async Task<ActionResult> EliminarRubro(Guid rubroId)
        {
            try
            {
                var rubro = await RubrosUsuario.FirstOrDefaultAsync(c => c.Id == rubroId);
                if (rubro == null)
                    throw new ExcepcionNegocios("No se encontró el rubro a eliminar.");

                var existe = await ConceptosUsuario.AnyAsync(g => g.RubroID == rubro.Id.Value);
                if (existe)
                    throw new ExcepcionNegocios("No puede eliminar el rubro seleccionado porque ya fue utilizado en al menos un concepto.");


                _context.Remove(rubro);

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (ExcepcionNegocios exN)
            {
                return BadRequest(exN.Message);
            }
            catch (Exception ex)
            {
                throw WrapException(ex);
            }
        }
        [HttpGet("obtener-descripcion")]
        public async Task<object> ObtenerDescripcion(Guid rubroId)
        {
            var desc = (await RubrosUsuario.FirstOrDefaultAsync(r => r.Id == rubroId))?.Descripcion;

            return desc;
        }
    }
}
