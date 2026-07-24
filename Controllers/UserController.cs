using ExpenSR.Exceptions;
using ExpenSR.Helpers;
using ExpenSR.Models.DTOs;
using ExpenSR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenSR.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // POST api/User/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            try
            {
                var companyId = User.GetCompanyId();
                var result = await _userService.CreateUserAsync(companyId, dto);
                return CreatedAtAction(nameof(CreateUser), new { id = result.UserId }, result);
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET api/User/GetPendingSignups
        [HttpGet]
        public async Task<IActionResult> GetPendingSignups()
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.GetPendingSignupsAsync(companyId);
            return Ok(result);
        }

        // POST api/User/ApproveSignup
        [HttpPost]
        public async Task<IActionResult> ApproveSignup([FromBody] ApproveSignupDTO dto)
        {
            try
            {
                var companyId = User.GetCompanyId();
                var adminId = User.GetUserId();
                var result = await _userService.ApproveSignupAsync(companyId, adminId, dto);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/User/RejectSignup
        [HttpPost]
        public async Task<IActionResult> RejectSignup([FromBody] RejectSignupDTO dto)
        {
            try
            {
                var companyId = User.GetCompanyId();
                var adminId = User.GetUserId();
                var result = await _userService.RejectSignupAsync(companyId, adminId, dto);
                return Ok(result);
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

        // PUT api/User/AssignManager
        [HttpPut]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerDTO dto)
        {
            try
            {
                var companyId = User.GetCompanyId();
                var result = await _userService.AssignManagerAsync(companyId, dto);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/User/ChangeRole
        [HttpPut]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDTO dto)
        {
            try
            {
                var companyId = User.GetCompanyId();
                var result = await _userService.ChangeRoleAsync(companyId, dto);
                return Ok(result);
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

        // GET api/User/GetAllUsers
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.GetAllUsersAsync(companyId);
            return Ok(result);
        }

        // GET api/User/GetManagers
        [HttpGet]
        public async Task<IActionResult> GetManagers()
        {
            var companyId = User.GetCompanyId();
            var result = await _userService.GetManagersAsync(companyId);
            return Ok(result);
        }
    }
}