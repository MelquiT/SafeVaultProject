using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { Message = "Esta información es pública y no requiere login." });
        }

        [HttpGet("authenticated")]
        [Authorize]
        public IActionResult GetAuthenticatedData()
        {
            // El Identity/ClaimsPrincipal te da acceso a la información del usuario en el token.
            var userEmail = User.Identity?.Name;

            return Ok(new
            {
                Message = $"Hola, {userEmail}. Tienes acceso porque estás logueado.",
                RoleCheck = "Solo se requería un Token válido (cualquier rol)."
            });
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            var userEmail = User.Identity?.Name;

            return Ok(new
            {
                Message = $"¡Bienvenido, Administrador {userEmail}!",
                RoleCheck = "Acceso exitoso porque el Token incluye el rol 'Admin'."
            });
        }

        [HttpGet("admin-or-manager")]
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult GetAdminOrManagerData()
        {
            return Ok(new
            {
                Message = "Acceso concedido para Admin o Manager."
            });
        }
    }
}
