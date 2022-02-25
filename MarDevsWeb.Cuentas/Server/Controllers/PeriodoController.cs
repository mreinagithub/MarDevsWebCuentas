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
    public class PeriodoController : MiBaseController
    {
        public PeriodoController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<PeriodoDTO>>> Get()
        {

            var queryable = PeriodosUsuario;            

            var listPeriodos = new List<PeriodoDTO>();
            PeriodoDTO nuevo;
            foreach (var q in queryable)
            {
                nuevo = new PeriodoDTO
                {
                    Id = q.Id.Value,
                    Desde = q.FechaDesde
                };

                var periodos = await queryable.Where(p => p.FechaDesde > nuevo.Desde).ToListAsync();
                if (periodos != null)
                {
                    var periodo = periodos.MinBy(f => f.FechaDesde);
                    if (periodo != null)
                        nuevo.Hasta = periodo.FechaDesde.AddDays(-1);
                }

                listPeriodos.Add(nuevo);
            }            

            return listPeriodos.OrderBy(p => p.Desde).ToList();
        }
        [HttpGet("obtenerModeloPeriodo/{periodoId?}")]
        public async Task<EditarPeriodoDTO> GetModeloPeriodo(Guid? periodoId = null)
        {

            var modelo = new EditarPeriodoDTO();

            if (periodoId != null)
            {
                var periodo = await PeriodosUsuario.FirstOrDefaultAsync(g => g.Id == periodoId.Value);
                if (periodo == null)
                    throw new ExcepcionNegocios("El periodo que intenta modificar no fue encontrado.");

                modelo.PeriodoId = periodo.Id.Value;
                modelo.FechaDesde = periodo.FechaDesde;                
            }           

            return modelo;

        }

        [HttpPost("editarPeriodo")]
        public async Task<ActionResult> EditarPeriodo([FromBody] EditarPeriodoDTO periodoEditar)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Periodo periodo;
                    if (periodoEditar.PeriodoId != null)
                    {
                        periodo = await PeriodosUsuario.FirstOrDefaultAsync(g => g.Id == periodoEditar.PeriodoId);
                        if (periodo == null)
                            throw new ExcepcionNegocios("No se encontró el periodo a editar.");                        
                    }
                    else
                    {
                        periodo = new Periodo();
                    }

                    var existe = await PeriodosUsuario.AnyAsync(p => p.Id != periodo.Id && p.FechaDesde == periodoEditar.FechaDesde);
                    if(existe)
                        throw new ExcepcionNegocios("Ya existe un periodo con la misma fecha de comienzo.\nNo puede cargar otro.");

                    periodo.FechaDesde = periodoEditar.FechaDesde.Value;

                    if (periodo.Id != null)
                    {
                        _context.Update(periodo);
                    }
                    else
                    {
                        periodo.Id = Guid.NewGuid();
                        periodo.CreadoEl = DateTime.Now;
                        periodo.CreadoPor = YO;
                        _context.Add(periodo);
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

        [HttpDelete("eliminar/{periodoId}")]
        public async Task<ActionResult> EliminarPeriodo(Guid periodoId)
        {
            try
            {
                var periodo = await PeriodosUsuario.FirstOrDefaultAsync(p => p.Id == periodoId);
                if (periodo == null)
                    throw new ExcepcionNegocios("No se encontró el período a eliminar.");

                _context.Remove(periodo);

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
