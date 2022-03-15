using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Server.Models.Seguridad;
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
        public ParametrosDTO Get()
        {

            var modelo = new ParametrosDTO();


            var flags = PreferenciaUsuario;
            if (flags == null)
            {
                throw new ExcepcionNegocios("No se lograron obtener las preferencias de usuario.");
            }
            
            modelo.MostrarSaldoAcumuladoEntrePeriodos = flags.MostrarSaldoAcumuladoEntrePeriodos;
            modelo.Tema = flags.Tema;

            return modelo;

        }

        [HttpPost("editar-parametros")]
        public async Task<ActionResult> Editar([FromBody] ParametrosDTO parametroDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var flags = PreferenciaUsuario;
                    if (flags == null)
                        throw new ExcepcionNegocios("No se lograron obtener las preferencias del usuario");

                    flags.MostrarSaldoAcumuladoEntrePeriodos = parametroDTO.MostrarSaldoAcumuladoEntrePeriodos;
                    flags.Tema = parametroDTO.Tema;

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

        [HttpGet("obtener-tema")]
        //[AllowAnonymous]
        public UserTemaDTO ObtenerTema()
        {
            var flags = PreferenciaUsuario;
            if (flags != null)
                return new UserTemaDTO { Tema = flags.Tema};
            else
                return new UserTemaDTO { Tema = "CLARO" };

        }
    }
}
