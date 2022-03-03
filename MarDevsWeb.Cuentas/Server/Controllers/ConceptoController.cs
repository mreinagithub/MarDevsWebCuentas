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
    public class ConceptoController : MiBaseController
    {
        public ConceptoController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ConceptoDTO>>> Get()
        {

            var queryable = ConceptosUsuario
                .Include(c => c.Rubro)
                .OrderBy(c => c.Tipo);            

            return await queryable.Select(c => new ConceptoDTO
            {
                Id = c.Id.Value,
                Tipo = c.Tipo,
                Descripcion = c.Descripcion,
                Rubro = (c.Rubro == null ? "" : c.Rubro.Descripcion)
               
            }).ToListAsync();
        }
        [HttpGet("obtenerModeloConcepto/{conceptoId?}")]
        public async Task<EditarConceptoDTO> GetModeloConcepto(Guid? conceptoId = null)
        {

            var modelo = new EditarConceptoDTO();

            if (conceptoId != null)
            {
                var concepto = await ConceptosUsuario.FirstOrDefaultAsync(c => c.Id == conceptoId.Value);
                if (concepto == null)
                    throw new ExcepcionNegocios("El concepto que intenta modificar no fue encontrado.");

                modelo.ConceptoId = concepto.Id.Value;
                modelo.TipoConcepto = concepto.Tipo;
                modelo.Descripcion = concepto.Descripcion;
                modelo.RubroID = concepto.RubroID;
            }

            var rubros = RubrosUsuario;
            rubros = rubros.OrderBy(c => c.Descripcion);
            modelo.RubrosDisponibles = await rubros.Select(c => new RubrosDisponiblesDTO
            {
                RubroID = c.Id.Value,
                Descripcion = c.Descripcion
            }).ToListAsync();

            return modelo;

        }

        [HttpPost("editarConcepto")]
        public async Task<ActionResult> EditarConcepto([FromBody] EditarConceptoDTO conceptoEditar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Concepto concepto;
                    if (conceptoEditar.ConceptoId != null)
                    {
                        concepto = await ConceptosUsuario.FirstOrDefaultAsync(c => c.Id == conceptoEditar.ConceptoId);
                        if (concepto == null)
                            throw new ExcepcionNegocios("No se encontró el concepto a editar.");
                        if(concepto.Tipo != conceptoEditar.TipoConcepto)
                        {
                            var fueUsado = await MovimientoUsuario.AnyAsync(g => g.ConceptoID == concepto.Id.Value);
                            if(fueUsado)
                                throw new ExcepcionNegocios("No puede modificar el tipo de concepto ya que este fue usado en al menos un movimiento.");
                        }
                    }
                    else
                    {
                        concepto = new Concepto();
                    }

                    conceptoEditar.TipoConcepto = conceptoEditar.TipoConcepto.Trim();
                    conceptoEditar.Descripcion = conceptoEditar.Descripcion.Trim();

                    var existe = await ConceptosUsuario.AnyAsync(c => c.Id != concepto.Id && c.Descripcion.Equals(conceptoEditar.Descripcion));
                    if (existe)
                        throw new ExcepcionNegocios("Ya existe un concepto con la misma descripción.\nNo puede cargar otro.");
                                        

                    concepto.Tipo = conceptoEditar.TipoConcepto;
                    concepto.Descripcion = conceptoEditar.Descripcion;
                    concepto.RubroID = conceptoEditar.RubroID;


                    if (concepto.Id != null)
                    {
                        _context.Update(concepto);
                    }
                    else
                    {
                        concepto.Id = Guid.NewGuid();
                        concepto.CreadoEl = DateTime.Now;
                        concepto.CreadoPor = YO;
                        _context.Add(concepto);
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

        [HttpDelete("eliminar/{conceptoId}")]
        public async Task<ActionResult> EliminarConcepto(Guid conceptoId)
        {
            try
            {
                var concepto = await ConceptosUsuario.FirstOrDefaultAsync(c => c.Id == conceptoId);
                if (concepto == null)
                    throw new ExcepcionNegocios("No se encontró el concepto a eliminar.");

                var existe = await MovimientoUsuario.AnyAsync(g => g.ConceptoID == concepto.Id.Value);
                if(existe)
                    throw new ExcepcionNegocios("No puede eliminar el concepto seleccionado porque ya fue utilizado en al menos un movimiento de sus cuentas.");


                _context.Remove(concepto);

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
    }
}
