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
        private readonly FundingConditionsContract _fundingConditions;
        private readonly ILogger<FundingContractsController> _logger;

        public FundingContractsController(ILogger<FundingContractsController> logger, FundingContract fundingContract, FunderContract funderContract, FundingConditionsContract fundingConditions)
        {
            _fundingContract = fundingContract;
            _funderContract = funderContract;
            _fundingConditions = fundingConditions;
            _logger = logger;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentFundingByStudentId([FromRoute] Guid studentId)
        {
            return Ok(await _fundingContract.GetFundingByStudentIdAsync(studentId));
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
                    var allFunders = await _funderContract.GetAllFundersAsync();
                    var funderDetails = allFunders.FirstOrDefault(x => x.Id == funding.FunderId);
                    _logger.LogInformation($"[Funding Contract Controller] - Fetching Funder: {funderDetails.Name}");
                    var conditions = await _fundingConditions.GetConditionByIdAsync(funding.FunderContractConditionId.ToString());
                    var paymentDate = funderDetails is not null ? DateTime.Parse(funderDetails.PaymentDate) : (DateTime?)null;
                    var date = paymentDate?.Day;
                    var today = DateTime.Today.Day;
                    var currentMonth = DateTime.Today.Month;
                    var nextPaymentDate = today < date ? $"{date}/{currentMonth}/{paymentDate?.Year}" : $"{date}/{currentMonth + 1}/{paymentDate?.Year}" ;
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
                        ModifiedBy = funding.ModifiedBy,
                        FundingCondition = conditions ?? null
                    };
                    if(response.Funder == "N/A")
                        response.IsActive = false;
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
                var existingFundings = await _fundingContract.GetAllFundingsAsync();
                var studentFundings = existingFundings.Where(x => x.StudentId == Guid.Parse(request.StudentId) && x.IsActive).ToList();

                if (existingFundings.Any())
                {
                    foreach (var funding in studentFundings)
                    {
                        var dto = new FundingUpdateDto
                        {
                            AccommodationBalance = funding.AccommodationBalance,
                            FoodBalance = funding.FoodBalance,
                            TuitionBalance = funding.TuitionBalance,
                            LaptopBalance = funding.LaptopBalance,
                            Id = funding.Id.ToString(),
                            IsActive = false,
                            ModifiedBy = "api-admin",
                        };
                        
                        var result = await _fundingContract.UpdateFundingAsync(dto);
                        if (result.IsSuccess)
                        {
                            _logger.LogInformation($"Successfully updated active status to false for funding in {request.StudentId}");
                            
                            //disable funding conditions
                            var fundingConditions = await _fundingConditions.GetConditionByIdAsync(funding.FunderContractConditionId.ToString());

                            if (fundingConditions?.ModifiedBy != "")
                            {
                                // disable funding condition

                                var conditionUpdate = new FundingConditionUpdateDto
                                {
                                    AverageMark = fundingConditions.AverageMark,
                                    IsActive = false,
                                    ModifiedBy = "api-admin",
                                };
                                
                                var conditionsUpdate = await _fundingConditions.UpdateConditionAsync(conditionUpdate);
                                if (conditionsUpdate.IsSuccess)
                                {
                                    _logger.LogInformation($"Successfully updated active status to false for funding condition for {request.StudentId}");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError($"Failed to update funding in {request.StudentId}");
                        }
                    }
                }
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
