using ExpenSR.Exceptions;
using ExpenSR.Helpers;
using ExpenSR.Models.DTOs;
using ExpenSR.Models.Enums;
using ExpenSR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenSR.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        // ---------------- Employee / Manager: own claims ----------------

        // POST api/Expense/CreateExpense
        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDTO dto)
        {
            try
            {
                var userId = User.GetUserId();
                var companyId = User.GetCompanyId();
                var result = await _expenseService.CreateExpenseAsync(userId, companyId, dto);
                return CreatedAtAction(nameof(GetExpenseById), new { id = result.ExpenseId }, result);
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

        // GET api/Expense/GetMyExpenses?status=Pending
        [HttpGet]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> GetMyExpenses([FromQuery] ExpenseStatus? status)
        {
            var userId = User.GetUserId();
            var result = await _expenseService.GetMyExpensesAsync(userId, status);
            return Ok(result);
        }

        // PUT api/Expense/UpdateExpense/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseDTO dto)
        {
            try
            {
                var userId = User.GetUserId();
                var result = await _expenseService.UpdateExpenseAsync(userId, id, dto);
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

        // DELETE api/Expense/DeleteExpense/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> DeleteExpense(Guid id)
        {
            try
            {
                var userId = User.GetUserId();
                await _expenseService.DeleteExpenseAsync(userId, id);
                return NoContent();
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

        // ---------------- Manager: team claims ----------------

        // GET api/Expense/GetPendingApprovals
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var managerId = User.GetUserId();
            var result = await _expenseService.GetPendingApprovalsAsync(managerId);
            return Ok(result);
        }

        // GET api/Expense/GetTeamExpenses?status=Approved
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetTeamExpenses([FromQuery] ExpenseStatus? status)
        {
            var managerId = User.GetUserId();
            var result = await _expenseService.GetTeamExpensesAsync(managerId, status);
            return Ok(result);
        }

        // POST api/Expense/ReviewExpense
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReviewExpense([FromBody] ReviewExpenseDTO dto)
        {
            try
            {
                var managerId = User.GetUserId();
                var result = await _expenseService.ReviewExpenseAsync(managerId, dto);
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

        // ---------------- Admin: company-wide ----------------

        // GET api/Expense/GetAllExpenses?status=Pending
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllExpenses([FromQuery] ExpenseStatus? status)
        {
            var companyId = User.GetCompanyId();
            var result = await _expenseService.GetAllExpensesAsync(companyId, status);
            return Ok(result);
        }

        // ---------------- Shared ----------------

        // GET api/Expense/GetExpenseById/{id}
        // Owner, the owner's manager, or an Admin in the same company can view it.
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetExpenseById(Guid id)
        {
            try
            {
                var userId = User.GetUserId();
                var role = User.GetRole();
                var companyId = User.GetCompanyId();
                var result = await _expenseService.GetExpenseByIdAsync(userId, role, companyId, id);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET api/Expense/GetCategories
        // Available to Employee/Manager for populating the claim form's category dropdown.
        [HttpGet]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> GetCategories()
        {
            var companyId = User.GetCompanyId();
            var result = await _expenseService.GetCategoriesAsync(companyId);
            return Ok(result);
        }
    }
}