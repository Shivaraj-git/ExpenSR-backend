using ExpenSR.Exceptions;
using ExpenSR.Models.DTOs;
using ExpenSR.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenSR.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/Auth/SignUpUser
        [HttpPost]
        public async Task<IActionResult> SignUpUser([FromBody] SignUpUserDTO dto)
        {
            try
            {
                var result = await _authService.SignUpUserAsync(dto);
                return CreatedAtAction(nameof(SignUpUser), new { id = result.UserId }, result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST api/Auth/LoginUser
        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDTO dto)
        {
            try
            {
                var result = await _authService.LoginUserAsync(dto);
                return Ok(result);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (AccountNotApprovedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        // POST api/Auth/LoginAdmin
        [HttpPost]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginAdminDTO dto)
        {
            try
            {
                var result = await _authService.LoginAdminAsync(dto);
                return Ok(result);
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (AccountNotApprovedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}