using System.Runtime.InteropServices.JavaScript;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
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
        private readonly FunderContract _funderContract;

        public FundingContractsController(FundingContract fundingContract, FunderContract funderContract)
        {
            _fundingContract = fundingContract;
            _funderContract = funderContract;
        }
        
        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudentId(string studentId)
        {
            var allfundings = await _fundingContract.GetAllFundingsAsync();
            if(!allfundings.Any()) return NotFound();
            
            var fundings = allfundings.Where(x => x.StudentId == Guid.Parse(studentId)).ToList();
            var responses = new List<FundingWebResponse>(); 
            foreach (var funding in fundings)
            {
                if (funding.IsActive)
                {
                    var funderDetails = await _funderContract.GetFunderByIdAsync(funding.FunderId);
                
                    if(funding?.FoodBalance == 0) return NotFound();
                    var paymentDate = funderDetails is not null ? DateTime.Parse(funderDetails.PaymentDate) : (DateTime?)null;
                    var date = paymentDate?.Day;
                    var today = DateTime.Today.Day;
                    var nextPaymentDate = today < date ? $"{date}-{paymentDate?.Month}-{paymentDate?.Year}" : $"{date}-{paymentDate?.Month + 1}-{paymentDate?.Year}" ;
                    var response = new FundingWebResponse()
                    {
                        Funder = funderDetails is not null ? funderDetails.Name : "N/A",
                        SignedOn = funding.SignedOn,
                        IsActive = funding.IsActive,
                        FoodBalance = funding.FoodBalance,
                        TuitionBalance = funding.TuitionBalance,
                        LaptopBalance = funding.LaptopBalance,
                        AccommodationBalance = funding.AccommodationBalance,
                        DataConfirmedById = funding.DataConfirmedById,
                        PaymentDate = nextPaymentDate,
                        UpdatedAt = funding.UpdatedAt,
                        FunderContractConditionId = funding.FunderContractConditionId,
                    };
                    responses.Add(response);
                }
            }
            return Ok(responses);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _fundingContract.GetAllFundingsAsync();
            if (results is not [])
            {
                return Ok(results);
            }
            return NotFound(results);
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
