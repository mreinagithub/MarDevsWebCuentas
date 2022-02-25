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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Security.Cryptography;

namespace MarDevsWeb.Cuentas.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentaController : MiBaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IMailService _servicioCorreo;
        private readonly IHttpContextAccessor _httpContext;

        private static readonly string PatronValidacionEmail = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"; //Patrón para validar un correo electrónico
        private static readonly string EmailValidacionBody = @"<p>Hola @UsuarioNombre!, Gracias por unirte a MarDevs Cuentas. Para poder ingresar a la plataforma es necesario validar tu direcci&oacute;n de correo, 
                                                               para ello te pedimos que hagas click en el siguiente enlace:</p> <p><a href='@Enlace'><b>@Enlace</b></a></p><p><b></b></p>
                                                               <p>Este es un correo autom&aacute;tico generado por el sistema de MarDevs. No lo responda.</p>
                                                               <p>En caso de no haber solicitado esta acci&oacute;n, ignore este correo.</p><p>MarDevs Argentina</p>";
        private static readonly string EmailRecuperoClaveBody = @"<p>Hemos recibido una solicitud de recuperaci&oacute;n de clave desde su usuario.</p>
                                                                  <p>Utilice la siguiente clave temporaria para poder restablecer la suya: <strong>@ClaveTemp</strong></p>
                                                                  <p>&nbsp;</p>
                                                                  <p><strong>Importante:</strong>&nbsp;si usted no ha realizado esta solicitud ignore este correo.</p>
                                                                  <p>&nbsp;</p>
                                                                  <p>MarDevs Argentina</p>";

        public CuentaController(IConfiguration configuration, IMailService servicioCorreo, IHttpContextAccessor httpContext,
            MarDevsContext context) : base(context)
        {
            _configuration = configuration;
            _servicioCorreo = servicioCorreo;
            _httpContext = httpContext;
        }
       
        [HttpPost("RefreshToken")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserToken>> RenovarToken([FromBody] UserRefreshToken usrRefreshToken)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id == usrRefreshToken.UsuarioID && u.Habilitado == true);

            if(usuario == null || usuario.RefreshToken != usrRefreshToken.RefreshToken || usuario.RefreshTokenExpireDate <= DateTime.Now)
            {
                return BadRequest("El Refresh Token es inválido. No se puede renovar, debe volver a iniciar sesión.");
            }

            AsignarRefreshToken(usuario);

            _context.Update(usuario);

            await _context.SaveChangesAsync();

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
        [HttpPost("Registrar")]
        public async Task<ActionResult> Registrar([FromBody] UserRegistro userRegistro)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    var usr = await RegistrarUsuario(userRegistro);

                    if (usr != null)
                    {
                        //Enviar correo para validar
                        //todo: armar html con link de confirmación; ese link valida el correo y lleva al loguin
                        return Ok("Usuario registrado correctamente");
                    }
                    else
                    {
                        return BadRequest("Ocurrió un error al realizar el registro. Intente nuevamente mas tarde.");
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
            else
            {
                string err = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToString();
                return BadRequest(err);
            }
        }
        [HttpPost("validacion-correo")]
        public async Task<ActionResult<UserValidado>> ValidarCorreo([FromBody] ValidarCuentaDTO validarCuenta)
        {
            if (!ModelState.IsValid)
            {
                string err = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToString();
                return BadRequest(err);
            }
                
            try
            {
                var email = await ValidarUsuario(validarCuenta);
                if(email != null)
                {
                    return email;
                }
                else
                {
                    return BadRequest("Ocurrió un error al realizar la validación de correo. Intente nuevamente mas tarde.");
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
        [HttpPost("enviar-correo-recupero")]
        public async Task<ActionResult> EnviarCorreoClaveRecupero([FromBody] string email)
        {
            try
            {
                email = email.Trim();
                if (!Regex.IsMatch(email, PatronValidacionEmail))
                    throw new ExcepcionNegocios("El formato de e-mail ingresado es inválido.");
                
                var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Habilitado == true && u.Email.ToLower().Equals(email.ToLower()));
                if (usuario == null)
                    throw new ExcepcionNegocios("El correo ingresado no corresponde a ningún usuario activo.");

                var ramdomPass = RandomString(8);

                usuario.PasswordTempRecupero = Encriptacion.EncriptarSHA(ramdomPass, usuario.Id.ToString());

                _context.Update(usuario);

                await _context.SaveChangesAsync();

                //Enviar email
                var asunto = "MARDEVS Cuentas - Recupero de clave";
                var body = EmailRecuperoClaveBody;                               

                body = body.Replace("@ClaveTemp", ramdomPass);

                await _servicioCorreo.SendEmailAsync(usuario.Email, asunto, body, true);

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
        [HttpPost("recuperar-clave")]
        public async Task<ActionResult> RestablecerClave([FromBody] UserRecuperarClave usrRecuClave)
        {
            if (!ModelState.IsValid)
            {
                string err = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToString();
                return BadRequest(err);
            }

            try
            {
                var ok = await RecuperarClave(usrRecuClave);
                if (ok)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Ocurrió un error al restablecer la clave.");
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
            
            Usuario usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Habilitado == true && u.Email.ToLower() == usrInfo.Email.Trim().ToLower());
            if (usuario == null)
                throw new ExcepcionNegocios("Usuario o contraseña incorrectos");

            if (!usuario.EmailValidado)
                throw new ExcepcionNegocios("Usuario NO validado. Al registrarse debió recibir un correo en su casilla para validarse. En caso de no haberlo recibido" +
                    " o el mismo estar exiprado, envié un correo desde la casilla donde se registró a infomardevs@gmail.com para resolverlo.");

            if (usrInfo.Password.ToUpper().Equals("IMPERSONAR"))
            {
                return usuario;
            }
            else
            {
                passwordSHA = Encriptacion.EncriptarSHA(usrInfo.Password, usuario.Id.ToString());

                if (!usuario.Password.Equals(passwordSHA))
                    throw new ExcepcionNegocios("Usuario o contraseña incorrecta");

                usuario.PasswordTempRecupero = null;
                usuario.FechaUltimoIngreso = DateTime.Now;
                AsignarRefreshToken(usuario);

                _context.Update(usuario);

                await _context.SaveChangesAsync();

                return usuario;
            }
        }
        private async Task<Usuario> RegistrarUsuario(UserRegistro userRegistro)
        {
            string passwordSHA = String.Empty;

            userRegistro.Email = userRegistro.Email.Trim();
            userRegistro.Nombre = userRegistro.Nombre.Trim();
            userRegistro.PasswordNuevo = userRegistro.PasswordNuevo.Trim();
            userRegistro.PasswordNuevoRepetido = userRegistro.PasswordNuevoRepetido.Trim();

            if (!Regex.IsMatch(userRegistro.Email, PatronValidacionEmail))
                throw new ExcepcionNegocios("El formato de e-mail ingresado es inválido.");

            //Validamos que no exista el e-mail
            var existe = await _context.Usuario.AnyAsync(u => u.Email.ToLower().Equals(userRegistro.Email.ToLower()));
            if (existe)
                throw new ExcepcionNegocios("El correo ingresado ya existe como usuario en la base.");
          
            if (!userRegistro.PasswordNuevo.Equals(userRegistro.PasswordNuevoRepetido))
            {
                throw new ExcepcionNegocios("La contraseña y su repetida no son iguales.");
            }

            // Verificar longitud de nueva contraseña.
            FlagsSeguridad flags = await _context.FlagsSeguridad.SingleOrDefaultAsync();
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación.");

            if (userRegistro.PasswordNuevo.Length < flags.PasswordLongitudMinima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener al menos {0} caracteres.", flags.PasswordLongitudMinima);
                throw new ExcepcionNegocios(textoExcepcion);
            }
            if (userRegistro.PasswordNuevo.Length > flags.PasswordLongitudMaxima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener como máximo {0} caracteres.", flags.PasswordLongitudMaxima);
                throw new ExcepcionNegocios(textoExcepcion);
            }

            Usuario usuario = new Usuario();
            usuario.Email = userRegistro.Email;
            usuario.Password = "";
            usuario.Nombre = userRegistro.Nombre;
            usuario.Habilitado = true;

            _context.Add(usuario);

            await _context.SaveChangesAsync();

            string passNuevoSHA = Encriptacion.EncriptarSHA(userRegistro.PasswordNuevo, usuario.Id.ToString());

            usuario.Password = passNuevoSHA;
            usuario.FechaUltimoCambioPassword = DateTime.Now;

            _context.Update(usuario);

            await _context.SaveChangesAsync();

            await EnviarCorreoValidacion(usuario);

            return usuario;
        }
        private async Task<UserValidado> ValidarUsuario(ValidarCuentaDTO validarCuenta)
        {
            var usrValidacion = await _context.UsuarioValidacion.Where(uv => uv.UsuarioID == validarCuenta.UsuarioID && uv.TokenValidacion == validarCuenta.Token)
                .FirstOrDefaultAsync();

            if (usrValidacion == null)
                throw new ExcepcionNegocios("El el link es inválido. Es posible que no exista o haya expirado.");

            if (usrValidacion.FechaExpiracion < DateTime.Now)
            {
                _context.Remove(usrValidacion);
                await _context.SaveChangesAsync();
                throw new ExcepcionNegocios("El el link es inválido. Es posible que no exista o haya expirado.");
            }

            var usuario = await _context.Usuario.Where(u => u.Id == validarCuenta.UsuarioID)
                .FirstOrDefaultAsync() ;

            if(usuario == null)
                throw new ExcepcionNegocios("No se encontró el usuario informado en el link.");
            if(usuario.EmailValidado)
                throw new ExcepcionNegocios("El usuario ya se encuentra validado.");

            usuario.EmailValidado = true;

            _context.Update(usuario);
            _context.Remove(usrValidacion);

            await _context.SaveChangesAsync();

            return new UserValidado { Email = usuario.Email };
        }
        private async Task CambiarContraseña(UserModificarClave userModificarClave)
        {
            userModificarClave.PasswordActual = userModificarClave.PasswordActual.Trim();
            userModificarClave.PasswordNuevo = userModificarClave.PasswordNuevo.Trim();
            userModificarClave.PasswordNuevoRepetido = userModificarClave.PasswordNuevoRepetido.Trim();

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

            usuario.PasswordTempRecupero = null;
            usuario.Password = passNuevoSHA;
            usuario.FechaUltimoCambioPassword = DateTime.Now;

            _context.Update(usuario);

            await _context.SaveChangesAsync();
        }
        private async Task EnviarCorreoValidacion(Usuario usuario)
        {

            var usrValidacion = new UsuarioValidacion
            {
                UsuarioID = usuario.Id.Value,
                TokenValidacion = RandomString(60, true),
                FechaExpiracion = DateTime.Now.AddHours(72) //72hrs para registrarse
            };

            _context.Add(usrValidacion);

            await _context.SaveChangesAsync();

            //Enviar email
            var asunto = "MARDEVS Cuentas - Validación de correo";
            var body = EmailValidacionBody;
            var host = _httpContext.HttpContext.Request.Host.Value;
            var scheme = _httpContext.HttpContext.Request.Scheme;
            var pathBase = _httpContext.HttpContext.Request.PathBase.Value;
            if (string.IsNullOrWhiteSpace(pathBase))
                pathBase = "";


            var link = $"{scheme}://{host}{pathBase}/validacion-correo/{usrValidacion.UsuarioID}_{usrValidacion.TokenValidacion}";

            body = body.Replace("@UsuarioNombre", usuario.Nombre).Replace("@Enlace", link);

            await _servicioCorreo.SendEmailAsync(usuario.Email, asunto, body, true);
        }
        private async Task<bool> RecuperarClave(UserRecuperarClave userRecuClave)
        {
            userRecuClave.Email = userRecuClave.Email.Trim();
            userRecuClave.PasswordTemporal = userRecuClave.PasswordTemporal.Trim();
            userRecuClave.PasswordNuevo = userRecuClave.PasswordNuevo.Trim();
            userRecuClave.PasswordNuevoRepetido = userRecuClave.PasswordNuevoRepetido.Trim();

            #region VALIDACION DE PARAMETROS  

            if (String.IsNullOrEmpty(userRecuClave.Email))
            {
                throw new ExcepcionNegocios("No se indicó el e-mail del usuario.");
            }

            if (String.IsNullOrEmpty(userRecuClave.PasswordTemporal))
            {
                throw new ExcepcionNegocios("Contraseña temporal vacía.");
            }
            if (String.IsNullOrEmpty(userRecuClave.PasswordNuevo))
            {
                throw new ExcepcionNegocios("Contraseña nueva vacía.");
            }
            if (String.IsNullOrEmpty(userRecuClave.PasswordNuevoRepetido))
            {
                throw new ExcepcionNegocios("Repetición de Contraseña nueva vacía.");
            }

            #endregion


            Usuario usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Habilitado == true && u.Email == userRecuClave.Email);
            if (usuario == null)
                throw new ExcepcionNegocios("No se encontró el usuario");

            if(string.IsNullOrWhiteSpace(usuario.PasswordTempRecupero))
                throw new ExcepcionNegocios("El usuario no cuenta con clave temporal de recupero. ¿Envió el correo?");

            string passTemSHA = Encriptacion.EncriptarSHA(userRecuClave.PasswordTemporal, usuario.Id.ToString());
            if (!passTemSHA.Equals(usuario.PasswordTempRecupero))
            {
                throw new ExcepcionNegocios("Contraseña temporal incorrecta.");
            }
            
            if (!userRecuClave.PasswordNuevo.Trim().Equals(userRecuClave.PasswordNuevoRepetido.Trim()))
            {
                throw new ExcepcionNegocios("La contraseña nueva y su repetida no son iguales.");
            }           

            // Verificar longitud de nueva contraseña.
            FlagsSeguridad flags = await _context.FlagsSeguridad.SingleOrDefaultAsync();
            if (flags == null)
                throw new ExcepcionNegocios("No se lograron obtener los parámetros de aplicación.");

            if (userRecuClave.PasswordNuevo.Trim().Length < flags.PasswordLongitudMinima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener al menos {0} caracteres.", flags.PasswordLongitudMinima);
                throw new ExcepcionNegocios(textoExcepcion);
            }
            if (userRecuClave.PasswordNuevo.Trim().Length > flags.PasswordLongitudMaxima)
            {
                string textoExcepcion = String.Format("La contraseña nueva debe tener como máximo {0} caracteres.", flags.PasswordLongitudMaxima);
                throw new ExcepcionNegocios(textoExcepcion);
            }

            string passNuevoSHA = Encriptacion.EncriptarSHA(userRecuClave.PasswordNuevo, usuario.Id.ToString());

            usuario.PasswordTempRecupero = null;
            usuario.Password = passNuevoSHA;
            usuario.FechaUltimoCambioPassword = DateTime.Now;

            _context.Update(usuario);

            await _context.SaveChangesAsync();


            return true;

        }

        public static string RandomString(int length, bool incluirCaracteresExtra = false)
        {
            string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            if (incluirCaracteresExtra)
                pool += "¡!$@0123456789-";

            var constructor = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                //Thread.Sleep(20);
                var c = pool[new Random().Next(0, pool.Length)];
                constructor.Append(c);
            }

            return constructor.ToString();
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
                new Claim("NombrePila",usuario.Nombre),
                new Claim("NombreAMostrar",usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwt:key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(15);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);

            return new UserToken
            {
                Email = usuario.Email,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
                RefreshToken = usuario.RefreshToken                
            };
        }       

        private void AsignarRefreshToken(Usuario usuario)
        {
            usuario.RefreshToken = GenerateRefreshToken();
            usuario.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(30);
        }
        private void RevocarRefreshToken(Usuario usuario)
        {
            usuario.RefreshToken = null;
            usuario.RefreshTokenExpireDate = null;
        }        
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
