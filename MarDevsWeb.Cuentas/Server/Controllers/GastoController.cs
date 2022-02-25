using MarDevsWeb.Cuentas.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GastoController : MiBaseController
    {
        public GastoController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("resumenHome")]
        public async Task<ResumenHomeDTO> GetResumen()
        {

            var resumen = new ResumenHomeDTO();
            resumen.FechaDesde = ObtenerFechaActual();
            var ttlGastos = await GastosUsuario.Where(g => g.Fecha >= resumen.FechaDesde && g.CreadoPor == YO).SumAsync(g => g.Importe);

            resumen.TotalGastos = ttlGastos;            

            return resumen;

        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<GastoDTO>>> Get(DateTime? fechaDesde, DateTime? fechaHasta, string textoBuscar)
        {

            var queryable = GastosUsuario
                .Include(g => g.Concepto)
                .Where(g => g.CreadoPor == YO);

            if(!string.IsNullOrWhiteSpace(textoBuscar))
                queryable = queryable.Where(g => g.Concepto.Descripcion.Contains(textoBuscar) || g.Observaciones.Contains(textoBuscar));

            if (fechaDesde != null)
                queryable = queryable.Where(g => g.Fecha >= fechaDesde.Value);
            if (fechaHasta != null)
                queryable = queryable.Where(g => g.Fecha <= fechaHasta.Value);

            queryable = queryable.OrderByDescending(p => p.Fecha);
                

            var modelo = queryable.Select(p => new GastoDTO
            {
                Id = p.Id.Value,
                Fecha = p.Fecha,
                Concepto = p.Concepto.Descripcion,
                Importe = p.Importe,
                Observaciones = p.Observaciones
            });

            return await modelo.ToListAsync();
        }

        [HttpGet("obtenerModeloGasto/{gastoId?}")]
        public async Task<EditarGastoDTO> GetModeloGasto(Guid? gastoId = null)
        {

            var modelo = new EditarGastoDTO();
            
            if(gastoId != null)
            {
                var gasto = await GastosUsuario.FirstOrDefaultAsync(g => g.Id == gastoId.Value);
                if (gasto == null)
                    throw new ExcepcionNegocios("El gasto que intenta modificar no fue encontrado.");

                modelo.Importe = gasto.Importe;
                modelo.Fecha = gasto.Fecha;
                modelo.Observaciones = gasto.Observaciones;
                modelo.GastoID = gasto.Id.Value;
                modelo.ConceptoID = gasto.ConceptoID;
            }


            var conceptos = ConceptosUsuario;
            conceptos = conceptos.OrderBy(c => c.Descripcion);
            modelo.ConceptosDisponibles = await conceptos.Select(c => new ConceptoDisponibleDTO
            {
                ConceptoID = c.Id.Value,
                Descripcion = c.Descripcion
            }).ToListAsync();


            return modelo;

        }

        [HttpPost("editarGasto")]
        public async Task<ActionResult> EditarGasto([FromBody] EditarGastoDTO gastoaeditar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Gasto gasto;
                    if(gastoaeditar.GastoID != null)
                    {
                        gasto = await GastosUsuario.FirstOrDefaultAsync(g => g.Id == gastoaeditar.GastoID);
                        if (gasto == null)
                            throw new ExcepcionNegocios("No se encontró el gasto a editar.");
                    }
                    else
                    {
                        gasto = new Gasto();
                    }

                    gasto.ConceptoID = gastoaeditar.ConceptoID.Value;
                    gasto.Fecha = gastoaeditar.Fecha;
                    gasto.Importe = gastoaeditar.Importe.Value;
                    gasto.Observaciones = gastoaeditar.Observaciones;

                    if(gasto.Id != null)
                    {
                        _context.Update(gasto);
                    }
                    else
                    {
                        gasto.Id = Guid.NewGuid();
                        gasto.CreadoEl = DateTime.Now;
                        gasto.CreadoPor = YO;
                        _context.Add(gasto);
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

        [HttpDelete("eliminar/{gastoId}")]
        public async Task<ActionResult> EliminarGasto(Guid gastoId)
        {
            try
            {
                Gasto gasto = await GastosUsuario.FirstOrDefaultAsync(g => g.Id == gastoId);
                if (gasto == null)
                    throw new ExcepcionNegocios("No se encontró el gasto a eliminar.");

                _context.Remove(gasto);

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

        [HttpGet("obtenerFechaActual")]
        public DateTime GetFechaActual()
        {

            return ObtenerFechaActual();
        }

        private DateTime ObtenerFechaActual()
        {
            var periodos = PeriodosUsuario.Where(p => p.FechaDesde.Date <= DateTime.Now.Date);
            if (periodos == null || periodos.Count() == 0)
                return DateTime.MinValue;
            else
            {
                return periodos.AsEnumerable().MaxBy(p => p.FechaDesde).FechaDesde;
            }
        }

    }
}
