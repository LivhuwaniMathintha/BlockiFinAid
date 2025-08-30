using BlockiFinAid.Services.SmartContracts.Funder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunderContractsController : ControllerBase
    {
        private readonly FunderContract _funderService;

        public FunderContractsController(FunderContract funderService)
        {
            _funderService = funderService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FunderInputDto request)
        {
            try
            {
                var results = await _funderService.AddFunderAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _funderService.GetAllFundersAsync());
        }

        [HttpGet("/byname/{id}")]
        public async Task<IActionResult> GetByName(string name)
        {
            return Ok(await _funderService.GetFunderByNameAsync(name));
        }
        
        [HttpGet("/byid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _funderService.GetFunderByIdAsync(id));
        }
        
    }
}
