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
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]    
    public class MovimientoController : MiBaseController
    {
        public MovimientoController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("resumenHome")]
        public async Task<ResumenHomeDTO> GetResumen()
        {

            var flags = PreferenciaUsuario;
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación. Si el problema persiste contacte al proveedor del sistema.");

            var resumen = new ResumenHomeDTO();
            resumen.FechaDesde = ObtenerFechaActual();
            var ttlEgresos = await MovimientoUsuario.Where(g => g.Fecha >= resumen.FechaDesde && g.Tipo.ToLower().Equals("egreso")).SumAsync(g => g.Importe);
            var ttlIngresos = await MovimientoUsuario.Where(g => g.Fecha >= resumen.FechaDesde && g.Tipo.ToLower().Equals("ingreso")).SumAsync(g => g.Importe);
            decimal saldoInicial = 0;
            if (flags.MostrarSaldoAcumuladoEntrePeriodos)
                saldoInicial = await MovimientoUsuario.Where(g => g.Fecha < resumen.FechaDesde).SumAsync(g => g.Importe * (g.Tipo.ToLower().Equals("egreso") ? -1 : 1));

            resumen.MostrarSaldoInicial = flags.MostrarSaldoAcumuladoEntrePeriodos;

            resumen.SaldoInicial = saldoInicial;
            resumen.TotalIngresos = ttlIngresos;
            resumen.TotalEgresos = ttlEgresos;
            resumen.Saldo = saldoInicial + ttlIngresos - ttlEgresos;


            return resumen;
        }
        
        [HttpGet("egreso-rubro-chart")]
        public async Task<List<ResumenPorRubroDTO>> GetEgresoPorRubro()
        {
            var resumenPorRubroDTO = new List<ResumenPorRubroDTO>();
            var fechaDesde = ObtenerFechaActual();

            var query = from m in MovimientoUsuario where m.Tipo == "Egreso" && m.Fecha.Date >= fechaDesde.Date
                        join c in ConceptosUsuario on m.ConceptoID equals c.Id
                        join r in RubrosUsuario on c.RubroID equals r.Id into rub
                        from rConVacios in rub.DefaultIfEmpty()                        
                        group new { rConVacios, m } by new { Descripcion = rConVacios.Descripcion, Color = rConVacios.Color }
                        into grp
                        select new ResumenPorRubroDTO
                        {
                            Rubro = grp.Key.Descripcion ?? "[Sin Rubro]",
                            Color = grp.Key.Color ?? "Black",
                            Importe = grp.Sum(p => p.m.Importe)
                        };

            resumenPorRubroDTO = await query.ToListAsync();            

            return resumenPorRubroDTO;

        }
        [HttpGet("ingreso-rubro-chart")]
        public async Task<List<ResumenPorRubroDTO>> GetIngresoPorRubro()
        {
            var resumenPorRubroDTO = new List<ResumenPorRubroDTO>();
            var fechaDesde = ObtenerFechaActual();

            var query = from m in MovimientoUsuario
                        where m.Tipo == "Ingreso" && m.Fecha.Date >= fechaDesde.Date
                        join c in ConceptosUsuario on m.ConceptoID equals c.Id
                        join r in RubrosUsuario on c.RubroID equals r.Id into rub
                        from rConVacios in rub.DefaultIfEmpty()
                        group new { rConVacios, m } by new { Descripcion = rConVacios.Descripcion, Color = rConVacios.Color }
                        into grp
                        select new ResumenPorRubroDTO
                        {
                            Rubro = grp.Key.Descripcion ?? "[Sin Rubro]",
                            Color = grp.Key.Color ?? "Black",
                            Importe = grp.Sum(p => p.m.Importe)
                        };

            resumenPorRubroDTO = await query.ToListAsync();

            return resumenPorRubroDTO;

        }


        [HttpGet("buscar")]
        public async Task<ActionResult<HeaderMovimientoDTO>> Get(DateTime? fechaDesde, DateTime? fechaHasta, string textoBuscar, string tipoMovimiento, string rubro = null)
        {

            var queryable = MovimientoUsuario
                .Include(g => g.Concepto)
                .ThenInclude(c => c.Rubro)
                .Where(g => g.CreadoPor == YO);

            if(!string.IsNullOrWhiteSpace(tipoMovimiento) && !tipoMovimiento.Equals("TODOS"))
                queryable = queryable.Where(g => g.Tipo.ToLower().Equals(tipoMovimiento.ToLower()));

            if (rubro != null)
            {
                if(rubro.Equals("[sin_rubro]"))
                {
                    queryable = queryable.Where(q => q.Concepto.RubroID == null);
                }
                else
                {
                    queryable = queryable.Where(q => q.Concepto.RubroID == Guid.Parse(rubro));
                }
                
            }

            if (!string.IsNullOrWhiteSpace(textoBuscar))
                queryable = queryable.Where(g => g.Concepto.Descripcion.Contains(textoBuscar) || g.Observaciones.Contains(textoBuscar));

            if (fechaDesde != null)
                queryable = queryable.Where(g => g.Fecha >= fechaDesde.Value);
            if (fechaHasta != null)
                queryable = queryable.Where(g => g.Fecha <= fechaHasta.Value);

            queryable = queryable.OrderByDescending(p => p.Fecha).ThenByDescending(p => p.CreadoEl);

                
                

            var modelo = queryable.Select(p => new MovimientoDTO
            {
                Id = p.Id.Value,
                Fecha = p.Fecha,
                Tipo = p.Concepto.Tipo,
                Concepto = p.Concepto.Descripcion,
                Rubro = (p.Concepto.Rubro != null ? p.Concepto.Rubro.Descripcion : ""),
                Importe = p.Importe,
                Observaciones = p.Observaciones
            });

            decimal saldoInicial = 0;
            if(fechaDesde != null)
                saldoInicial = await MovimientoUsuario.Where(g => g.Fecha < fechaDesde.Value).SumAsync(g => g.Importe * (g.Tipo.ToLower().Equals("egreso") ? -1 : 1));            

            var header = new HeaderMovimientoDTO
            {
                SaldoInicial = saldoInicial,
                Movimientos = await modelo.ToListAsync()
            };

            var sumMov = header.Movimientos.Sum(m => m.Importe * (m.Tipo.ToLower().Equals("egreso") ? -1 : 1));

            //header.Saldo = header.SaldoInicial + sumMov;

            return header;
        }

        [HttpGet("obtenerModeloEgreso/{egresoId?}")]
        public async Task<EditarEgresoDTO> GetModeloEgreso(Guid? egresoId = null)
        {

            var modelo = new EditarEgresoDTO();
            
            if(egresoId != null)
            {
                var gasto = await MovimientoUsuario.FirstOrDefaultAsync(g => g.Id == egresoId.Value);
                if (gasto == null)
                    throw new ExcepcionNegocios("El egreso que intenta modificar no fue encontrado.");

                modelo.Importe = gasto.Importe;
                modelo.Fecha = gasto.Fecha;
                modelo.Observaciones = gasto.Observaciones;
                modelo.EgresoID = gasto.Id.Value;
                modelo.ConceptoID = gasto.ConceptoID;
            }


            var conceptos = ConceptosUsuario.Where(c => c.Tipo == "Egreso");
            conceptos = conceptos.OrderBy(c => c.Descripcion);
            modelo.ConceptosDisponibles = await conceptos.Select(c => new ConceptoDisponibleDTO
            {
                ConceptoID = c.Id.Value,
                Descripcion = c.Descripcion
            }).ToListAsync();


            return modelo;

        }
        [HttpGet("obtenerModeloIngreso/{ingresoId?}")]
        public async Task<EditarIngresoDTO> GetModeloIngreso(Guid? ingresoId = null)
        {

            var modelo = new EditarIngresoDTO();

            if (ingresoId != null)
            {
                var gasto = await MovimientoUsuario.FirstOrDefaultAsync(g => g.Id == ingresoId.Value);
                if (gasto == null)
                    throw new ExcepcionNegocios("El ingreso que intenta modificar no fue encontrado.");

                modelo.Importe = gasto.Importe;
                modelo.Fecha = gasto.Fecha;
                modelo.Observaciones = gasto.Observaciones;
                modelo.IngresoID = gasto.Id.Value;
                modelo.ConceptoID = gasto.ConceptoID;
            }


            var conceptos = ConceptosUsuario.Where(c => c.Tipo.ToLower().Equals("ingreso"));
            conceptos = conceptos.OrderBy(c => c.Descripcion);
            modelo.ConceptosDisponibles = await conceptos.Select(c => new ConceptoDisponibleDTO
            {
                ConceptoID = c.Id.Value,
                Descripcion = c.Descripcion
            }).ToListAsync();


            return modelo;

        }

        [HttpPost("editar-egreso")]
        public async Task<ActionResult> EditarEgreso([FromBody] EditarEgresoDTO egresoaeditar)
        {
            if (ModelState.IsValid)
            {
                if (egresoaeditar.Fecha.Date > DateTime.Today.Date)
                    throw new ExcepcionNegocios("La fecha del egreso no puede ser futura.");

                try
                {
                    Movimiento gasto;
                    if(egresoaeditar.EgresoID != null)
                    {
                        gasto = await MovimientoUsuario.FirstOrDefaultAsync(g => g.Id == egresoaeditar.EgresoID);
                        if (gasto == null)
                            throw new ExcepcionNegocios("No se encontró el egreso a editar.");
                    }
                    else
                    {
                        gasto = new Movimiento();
                    }

                    gasto.Tipo = "Egreso";
                    gasto.ConceptoID = egresoaeditar.ConceptoID.Value;
                    gasto.Fecha = egresoaeditar.Fecha;
                    gasto.Importe = egresoaeditar.Importe.Value;
                    gasto.Observaciones = egresoaeditar.Observaciones;

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
        [HttpPost("editar-ingreso")]
        public async Task<ActionResult> EditarIngreso([FromBody] EditarIngresoDTO ingresoEditar)
        {
            if (ModelState.IsValid)
            {
                if (ingresoEditar.Fecha.Date > DateTime.Today.Date)
                    throw new ExcepcionNegocios("La fecha del ingreso no puede ser futura.");

                try
                {
                    Movimiento gasto;
                    if (ingresoEditar.IngresoID != null)
                    {
                        gasto = await MovimientoUsuario.FirstOrDefaultAsync(g => g.Id == ingresoEditar.IngresoID);
                        if (gasto == null)
                            throw new ExcepcionNegocios("No se encontró el ingreso a editar.");
                    }
                    else
                    {
                        gasto = new Movimiento();
                    }

                    gasto.Tipo = "Ingreso";
                    gasto.ConceptoID = ingresoEditar.ConceptoID.Value;
                    gasto.Fecha = ingresoEditar.Fecha;
                    gasto.Importe = ingresoEditar.Importe.Value;
                    gasto.Observaciones = ingresoEditar.Observaciones;

                    if (gasto.Id != null)
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


        [HttpDelete("eliminar/{movimientoId}")]
        public async Task<ActionResult> EliminarMovimiento(Guid movimientoId)
        {
            try
            {
                Movimiento gasto = await MovimientoUsuario.FirstOrDefaultAsync(g => g.Id == movimientoId);
                if (gasto == null)
                    throw new ExcepcionNegocios("No se encontró el movimiento a eliminar.");

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

        [HttpGet("obtener-listas-filtros")]
        public async Task<ActionResult<ListasFiltroMovimientosDTO>> ObtenerListasFiltros()
        {
            //Periodos
            var queryPeriodo = PeriodosUsuario;
            var listPeriodos = new List<PeriodoDTO>();
            PeriodoDTO nuevo;
            foreach (var q in queryPeriodo)
            {
                nuevo = new PeriodoDTO
                {
                    Id = q.Id.Value,
                    Desde = q.FechaDesde
                };

                var periodos = await queryPeriodo.Where(p => p.FechaDesde > nuevo.Desde).ToListAsync();
                if (periodos != null)
                {
                    var periodo = periodos.MinBy(f => f.FechaDesde);
                    if (periodo != null)
                        nuevo.Hasta = periodo.FechaDesde.AddDays(-1);
                    else
                        nuevo.Hasta = (nuevo.Desde > DateTime.Now.Date ? nuevo.Desde : DateTime.Now.Date);
                }
                listPeriodos.Add(nuevo);
            }

            //Rubros
            var queryRubro = RubrosUsuario;
            var listaRubros = queryRubro.Select(r => new RubroDTO
            {
                Id = r.Id.Value,
                Descripcion = r.Descripcion
            }).ToList();


            return new ListasFiltroMovimientosDTO
            {
                Periodos = listPeriodos.OrderBy(p => p.Desde).ToList(),
                Rubros = listaRubros.OrderBy(r => r.Descripcion).ToList()        
            };            
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
