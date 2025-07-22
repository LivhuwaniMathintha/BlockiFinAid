using System.Runtime.InteropServices.JavaScript;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class FundingContractsController : ControllerBase
    {
        private readonly FundingContract _fundingContract;

        public FundingContractsController(FundingContract fundingContract)
        {
            _fundingContract = fundingContract;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var results = await _fundingContract.GetAllFundingsAsync();
            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound(results);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByStudentId(Guid id)
        {
            return Ok(await _fundingContract.GetFundingByStudentIdAsync(id));
        }

        [HttpGet("inactive/student/{id}")]
        public async Task<IActionResult> GetInactiveByStudentId(Guid id)
        {
            var results = await _fundingContract.GetInactiveFundingsByStudentIdAsync(id);

            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound(id);
        }
        
        [HttpGet("inactive")]
        public async Task<IActionResult> GetAllInactive()
        {
            var results = await  _fundingContract.GetAllInactiveFundingsAsync();

            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound(results);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FundingInputDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var results = await _fundingContract.AddFundingAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("/confirm")]
        public async Task<IActionResult> Confirm([FromBody] FundingDataConfirmedByIdUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var results = await _fundingContract.UpdateFundingDataConfirmedByIdAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Post([FromBody] FundingUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var results = await _fundingContract.UpdateFundingAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
