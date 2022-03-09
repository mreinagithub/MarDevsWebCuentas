using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ParametroController : MiBaseController
    {
        public ParametroController(MarDevsContext context) : base(context)
        {

        }

        [HttpGet("obtenerModeloParametro")]
        public async Task<ParametrosDTO> Get()
        {

            var modelo = new ParametrosDTO();

            var flags = await _context.FlagsCuentas.FirstOrDefaultAsync();
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación");


            modelo.MostrarSaldoAcumuladoEntrePeriodos = flags.MostrarSaldoAcumuladoEntrePeriodos;

            return modelo;

        }

        [HttpPost("editar-parametros")]
        public async Task<ActionResult> Editar([FromBody] ParametrosDTO parametroDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var flags = await _context.FlagsCuentas.FirstOrDefaultAsync();
                    if (flags == null)
                        throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación");

                    flags.MostrarSaldoAcumuladoEntrePeriodos = parametroDTO.MostrarSaldoAcumuladoEntrePeriodos;

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
    }
}
