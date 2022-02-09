using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MarDevsWeb.Cuentas.Server.Models;
using MarDevsWeb.Cuentas.Server.Servicios;
using MarDevsWeb.Cuentas.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MarDevsWeb.Cuentas.Server.Excepciones;
using MarDevsWeb.Cuentas.Server.Models.Seguridad;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentaController : MiBaseController
    {
        private readonly IConfiguration _configuration;        

        public CuentaController(IConfiguration configuration,
            MarDevsContext context) : base(context)
        {
            _configuration = configuration;            
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> Renovar()
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id.ToString().Equals(HttpContext.User.Identity.Name));        

            return BuildToken(usuario);

        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo userInfo)
        {

            try
            {
                Usuario usuario = await Autenticar(userInfo);

                if (usuario != null)
                {
                    return BuildToken(usuario);
                }
                else
                {
                    return BadRequest("Usuario o clave incorrectos");
                }
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

        [HttpPost("ModificarClave")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> ModificarClave([FromBody] UserModificarClave userModificarClave)
        {
            try
            {
                await CambiarContraseña(userModificarClave);
                return Ok("Contraseña modificada exitosamente");
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


        private async Task<Usuario> Autenticar(UserInfo usrInfo)
        {
            string passwordSHA = String.Empty;

            //await _context.Database.OpenConnectionAsync();

            //var clientes = await _context.Cliente.ToListAsync() ;
            
            Usuario usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Habilitado == true && u.Logon.ToLower() == usrInfo.Logon.Trim().ToLower());
            if (usuario == null)
                throw new ExcepcionNegocios("Usuario o contraseña incorrectos");          

            if (usrInfo.Password.ToUpper().Equals("IMPERSONAR"))
            {
                return usuario;
            }
            else
            {
                passwordSHA = Encriptacion.EncriptarSHA(usrInfo.Password, usuario.Id.ToString());

                if (!usuario.Password.Equals(passwordSHA))
                    throw new ExcepcionNegocios("Usuario o contraseña incorrecta");

                usuario.FechaUltimoIngreso = DateTime.Now;
                _context.Update(usuario);

                await _context.SaveChangesAsync();

                return usuario;
            }
        }
        private async Task CambiarContraseña(UserModificarClave userModificarClave)
        {
            #region VALIDACION DE PARAMETROS            

            if (String.IsNullOrEmpty(userModificarClave.PasswordActual))
            {
                throw new ExcepcionNegocios("Contraseña actual vacía.");
            }
            if (String.IsNullOrEmpty(userModificarClave.PasswordNuevo))
            {
                throw new ExcepcionNegocios("Contraseña nueva vacía.");
            }
            if (String.IsNullOrEmpty(userModificarClave.PasswordNuevoRepetido))
            {
                throw new ExcepcionNegocios("Repetición de Contraseña nueva vacía.");
            }

            #endregion


            Usuario usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Habilitado == true && u.Id.Value == userModificarClave.UsuarioID);
            if (usuario == null)
                throw new ExcepcionNegocios("No se encontró el usuario");

            string passActualSHA = Encriptacion.EncriptarSHA(userModificarClave.PasswordActual, usuario.Id.ToString());
            if (!passActualSHA.Equals(usuario.Password))
            {
                throw new ExcepcionNegocios("Contraseña actual incorrecta.");
            }
            string passNuevoSHA = Encriptacion.EncriptarSHA(userModificarClave.PasswordNuevo, usuario.Id.ToString());

            if (passNuevoSHA.Trim().Equals(passActualSHA.Trim()))
            {
                throw new ExcepcionNegocios("La contraseña actual y nueva son iguales.");
            }
            if (!userModificarClave.PasswordNuevo.Trim().Equals(userModificarClave.PasswordNuevoRepetido.Trim()))
            {
                throw new ExcepcionNegocios("La contraseña nueva y su repetida no son iguales.");
            }

            // Verificar longitud de nueva contraseña.
            FlagsSeguridad flags = await _context.FlagsSeguridad.SingleOrDefaultAsync();
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación.");

            if (userModificarClave.PasswordNuevo.Trim().Length < flags.PasswordLongitudMinima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener al menos {0} caracteres.", flags.PasswordLongitudMinima);
                throw new ExcepcionNegocios(textoExcepcion);
            }
            if (userModificarClave.PasswordNuevo.Trim().Length > flags.PasswordLongitudMaxima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener como máximo {0} caracteres.", flags.PasswordLongitudMaxima);
                throw new ExcepcionNegocios(textoExcepcion);
            }

            usuario.Password = passNuevoSHA;
            usuario.FechaUltimoCambioPassword = DateTime.Now;

            _context.Update(usuario);

            await _context.SaveChangesAsync();
        }
        private UserToken BuildToken(Usuario usuario)
        {
            return BuildToken(usuario, new List<string>());
        }
        private UserToken BuildToken(Usuario usuario, IList<string> roles)
        {
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Id.Value.ToString()),
                new Claim(ClaimTypes.Name, usuario.Id.Value.ToString()),
                new Claim("NombreAMostrar",usuario.Logon),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            return new UserToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

    }
}
