using BlockiFinAid.Services;
using BlockiFinAid.Services.SmartContracts.Student;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentInputDto = BlockiFinAid.Services.SmartContracts.Student.StudentInputDto;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartContractsController : ControllerBase
    {
        private readonly StudentContract _studentService;


        public SmartContractsController(StudentContract studentService)
        {
            _studentService = studentService;
        }
        // GET: api/<SmartContractsController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var institutions = await _studentService.GetAllStudentsAsync();
                return Ok(institutions);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("bystdno/{id}")]
        public async Task<IActionResult> GetByStdNo(uint id)
        {
            try
            {
                var student = await _studentService.GetStudentByStudentNumberAsync(id);

                if (student == null)
                {
                    return NotFound();
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/<SmartContractsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST api/<SmartContractsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StudentInputDto request)
        {
            try
            {
                await _studentService.AddStudentAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT api/<SmartContractsController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] StudentUpdateDto request)
        {
            try
            {
                var results = await _studentService.UpdateStudentByStudentNumberAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }

        // DELETE api/<SmartContractsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
