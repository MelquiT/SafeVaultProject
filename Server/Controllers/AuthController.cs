using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Inyectamos configuración para leer la "Secret Key" (simulada aquí)
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginRequest)
        {
            // 1. PREVENCIÓN XSS (Cross-Site Scripting)
            // La primera línea de defensa es validar que los datos sean EXACTAMENTE lo que esperamos.
            // Si alguien envía un script <script>alert(1)</script> en el email, 
            // el ModelState lo marcará como inválido por el atributo [EmailAddress].
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fakeUserEmail = "admin@prueba.com";
            var fakeUserPassword = "Admin123!"; // En producción, esto debe ser un Hash

            var fakeUserEmail2 = "user@prueba.com";
            var fakeUserPassword2 = "User123!";

            // 3. PREVENCIÓN SQL INJECTION
            // Al no concatenar strings y usar comparación directa de variables en C#,
            // la inyección SQL es imposible aquí. 
            // Cuando conectes la BD, usar Entity Framework (EF Core) mantiene esta protección automáticamente.
            bool credentialsMatch = ((loginRequest.Email == fakeUserEmail) && (loginRequest.Password == fakeUserPassword)) ||
                                    ((loginRequest.Email == fakeUserEmail2) && (loginRequest.Password == fakeUserPassword2));

            if (!credentialsMatch)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            // 4. GENERACIÓN DEL TOKEN JWT
            // Si las credenciales son válidas, generamos el token.
            var tokenString = GenerateJwtToken(loginRequest.Email, loginRequest.Email.StartsWith("admin") ? "Admin" : "Manager");

            return Ok(new LoginResponse
            {
                Token = tokenString,
                Expiration = DateTime.UtcNow.AddHours(1).ToString()
            });
        }

        private string GenerateJwtToken(string email, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role), // Simulamos un rol
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "TuApp.com",
                audience: "TuApp.com",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Las DataAnnotations actúan como firewall contra datos maliciosos (XSS)
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(100)] // Limitar longitud previene ataques de desbordamiento de búfer
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6)]
        [MaxLength(100)]
        // En un escenario real, una Regex aquí puede evitar caracteres extraños
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Expiration { get; set; } = string.Empty;
    }
}
