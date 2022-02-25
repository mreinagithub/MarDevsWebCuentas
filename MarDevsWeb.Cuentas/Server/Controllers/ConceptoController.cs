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

            var queryable = ConceptosUsuario;

            return await queryable.Select(c => new ConceptoDTO
            {
                Id = c.Id.Value,
                Descripcion = c.Descripcion
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
                modelo.Descripcion = concepto.Descripcion;
            }

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
                    }
                    else
                    {
                        concepto = new Concepto();
                    }

                    conceptoEditar.Descripcion = conceptoEditar.Descripcion.Trim();

                    var existe = await ConceptosUsuario.AnyAsync(c => c.Id != concepto.Id && c.Descripcion.Equals(conceptoEditar.Descripcion));
                    if (existe)
                        throw new ExcepcionNegocios("Ya existe un concepto con la misma descripción.\nNo puede cargar otro.");

                    concepto.Descripcion = conceptoEditar.Descripcion;

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

                var existe = await GastosUsuario.AnyAsync(g => g.ConceptoID == concepto.Id.Value);
                if(existe)
                    throw new ExcepcionNegocios("No puede eliminar el concepto seleccionado porque ya fue utilizado en al menos un gasto de sus cuentas.");


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
