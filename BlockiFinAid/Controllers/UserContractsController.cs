

using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class UserContractsController : ControllerBase
    {
        private readonly UserContract _studentContrat;

        public UserContractsController(UserContract studentContract)
        {
            _studentContrat = studentContract;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _studentContrat.GetAllUsersAsync());
        }

        [HttpGet("{studentNumber}")]
        public async Task<IActionResult> GetByStudentNumber(string studentNumber)
        {
            var result = await _studentContrat.GetUserByStudentNumberAsync(studentNumber);

            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] UserInputDto request)
        {
            
            var student = await _studentContrat.GetUserByStudentNumberAsync(request.StudentNumber);
            if (student is not null)
                return Conflict($"Student number: {request.StudentNumber} already exists. Cannot add it.");
            var results = await _studentContrat.AddUserAsync(request);
            
            if (results.IsSuccess)
            {
                return Ok(results);
            }
            return BadRequest(results);
            
            
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserUpdateDto request)
        {
            try
            {
                var results = await _studentContrat.UpdateUserAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
