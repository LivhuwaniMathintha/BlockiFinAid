using BlockiFinAid.Services.SmartContracts.Funding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class FundingConditionContractsController : ControllerBase
    {
        private readonly FundingConditionsContract _fundingConditionsContract;

        public FundingConditionContractsController(FundingConditionsContract fundingConditionsContract)
        {
            _fundingConditionsContract = fundingConditionsContract;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFundingConditionContracts()
        {
            var results = await _fundingConditionsContract.GetAllConditionsAsync();

            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound(results);
        }

        [HttpGet("inactive")]
        public async Task<IActionResult> GetAllInactiveFundingConditionContracts()
        {
            var results = await _fundingConditionsContract.GetAllInactiveConditionsAsync();

            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFundingConditionContract(string id)
        {
            var result = await _fundingConditionsContract.GetConditionByIdAsync(id);

            if (result != null)
            {
                return Ok(result);
            }
            return NotFound(id);
        }

        [HttpPost]
        public async Task<IActionResult> AddFundingCondition([FromBody] FundingConditionInputDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var results = await _fundingConditionsContract.AddConditionAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("confirm")]
        public async Task<IActionResult> ConfirmFundingCondition([FromBody] FundingConditionDataConfirmedByIdUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var results = await _fundingConditionsContract.UpdateDataConfirmedByIdAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFundingCondition([FromBody] FundingConditionUpdateDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var results = await _fundingConditionsContract.UpdateConditionAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
