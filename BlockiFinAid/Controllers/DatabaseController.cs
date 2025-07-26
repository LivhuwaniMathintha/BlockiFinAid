using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IBaseRepository<BankAccountModel> _bankRepository;
        private readonly IBaseRepository<PaymentModel> _paymentRepository;
        private readonly IBaseRepository<FundingModel> _fundingRepository;
        private readonly IBaseRepository<FundingConditionsModel> _fundingConditionRepository;
        private readonly IBaseRepository<UserModel> _userRepository;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(IBaseRepository<BankAccountModel> bankAccountRepository, 
            IBaseRepository<PaymentModel> paymentRepository, 
            IBaseRepository<FundingModel> fundingRepository,
            IBaseRepository<FundingConditionsModel> fundingConditionsRepository,
            IBaseRepository<UserModel> userRepository, ILogger<DatabaseController> logger)
        {
            _bankRepository = bankAccountRepository;
            _paymentRepository = paymentRepository;
            _fundingRepository = fundingRepository;
            _fundingConditionRepository = fundingConditionsRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        #region BankAccount
        
        [HttpGet("bankaccount")]
        public async Task<IActionResult> Get()
        {
            var results = await _bankRepository.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("bankaccount/{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _bankRepository.GetByIdAsync(id);

            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("bankaccount")]
        public async Task<IActionResult> Post([FromBody] BankAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }
            
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                _logger.LogError($"User {model.UserIdPerformingAction} does not exist. You're unathorized to make this");
                return Unauthorized();
            }

            var doesAccountExists = await _bankRepository.GetByIdAsync(model.Id);
        
            if (doesAccountExists is not null)
                return Conflict();
        
            // check if the student exists
            
            var doesStudentExists = await _userRepository.GetByIdAsync(model.UserId);
            if (doesStudentExists is null)
            {
                var userType = model.IsStudent ? "Student" : "Institution Service profile";
                return NotFound($"{userType} not found, cannot save this bank details. create the user first or check the userId provided");
            }
            var result = await _bankRepository.CreateAsync(model, actionUser.Name);

            if (result is null)
            {
                return BadRequest(model);
            }
            return StatusCode(201, result);
        }

        [HttpPut("bankaccount")]
        public async Task<IActionResult> Put([FromBody] BankAccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
        
            var result = await _bankRepository.UpdateAsync(model, actionUser.Name);

            if (result is false)
            {
                return BadRequest(model);
            }
            return NoContent();
        }
        
        #endregion 
        
        #region Payment

        [HttpGet("payment")]
        public async Task<IActionResult> GetPayment()
        {
            return Ok(await _paymentRepository.GetAllAsync());
        }

        [HttpGet("payment/{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var result = await _paymentRepository.GetByIdAsync(id);

            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("payment")]
        public async Task<IActionResult> PostPayment([FromBody] PaymentModel model)
        {
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            
            var doesPaymentExists = await _paymentRepository.GetByIdAsync(model.Id);
            if (doesPaymentExists is not null) return Conflict("Payment already exists");
            
            var doesStudentExists = await _userRepository.GetByIdAsync(model.StudentId);
            if (doesStudentExists is null)
            {
                return NotFound("Student not found, cannot save this payment for processing");
            }

            var doesServiceAccountExists = await _userRepository.GetByIdAsync(model.InstitutionServiceId);
            if (doesServiceAccountExists is null)
            {
                return NotFound("Institution Service profile not found, cannot save this payment for processing");
            }
            
            
            var result = await _paymentRepository.CreateAsync(model,  actionUser.Name);
            return StatusCode(201, result);
        }

        [HttpPut("payment")]
        public async Task<IActionResult> PutPayment([FromBody] PaymentModel model)
        {
            
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            var doesPaymentExists = await _paymentRepository.GetByIdAsync(model.Id);
            if (doesPaymentExists is not null) return Conflict("Payment already exists");
            
            var doesStudentExists = await _userRepository.GetByIdAsync(model.StudentId);
            if (doesStudentExists is null)
            {
                return NotFound("Student not found, cannot save this payment for processing");
            }

            var doesServiceAccountExists = await _userRepository.GetByIdAsync(model.InstitutionServiceId);
            if (doesServiceAccountExists is null)
            {
                return NotFound("Institution Service profile not found, cannot save this payment for processing");
            }

            var result = await _paymentRepository.UpdateAsync(model, actionUser.Name);
            if (result)
            {
                return NoContent();
            }
            return BadRequest(model);
        }
        
        #endregion
        
        #region Funding

        [HttpGet("funding")]
        public async Task<IActionResult> GetFunding()
        {
            return Ok(await _fundingRepository.GetAllAsync());
        }

        [HttpGet("funding/{id}")]
        public async Task<IActionResult> GetFunding(Guid id)
        {
            var result = await _fundingRepository.GetByIdAsync(id);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("funding")]
        public async Task<IActionResult> PostFunding([FromBody] FundingModel model, [FromQuery] string user)
        {
            var doesFundingExists = await _fundingRepository.GetByIdAsync(model.Id);
            if (doesFundingExists is not null) return Conflict();
            try
            {
                var result = await _fundingRepository.CreateAsync(model, user);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("funding")]
        public async Task<IActionResult> PutFunding([FromBody] FundingModel model)
        {
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            var doesFundingExists = await _fundingRepository.GetByIdAsync(model.Id);
            if (doesFundingExists is not null) return Conflict();
            try
            {
                var user = await _userRepository.GetByIdAsync(model.StudentId);
                var result = await _fundingRepository.UpdateAsync(model, actionUser.Name);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        #endregion
        
        #region FundingConditions

        [HttpGet("fundingConditions")]
        public async Task<IActionResult> GetFundingConditions()
        {
            return Ok(await _fundingConditionRepository.GetAllAsync());
        }

        [HttpGet("fundingConditions/{id}")]
        public async Task<IActionResult> GetFundingConditions(Guid id)
        {
            var result = await _fundingConditionRepository.GetByIdAsync(id);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("fundingConditions")]
        public async Task<IActionResult> PostFundingConditions([FromBody] FundingConditionsModel model)
        {
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            var doesFundingConditionExists = await _fundingConditionRepository.GetByIdAsync(model.Id);
            if (doesFundingConditionExists is not null) return Conflict();
            try
            {
                var result = await _fundingConditionRepository.CreateAsync(model, actionUser.Name);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("fundingConditions")]
        public async Task<IActionResult> PutFundingConditions([FromBody] FundingConditionsModel model)
        {
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            var doesFundingConditionExists = await _fundingConditionRepository.GetByIdAsync(model.Id);
            if (doesFundingConditionExists is not null) return Conflict();
            try
            {
                var result = await _fundingConditionRepository.UpdateAsync(model, actionUser.Name);
                if (result is false)
                {
                    return BadRequest(model);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        #endregion
        
        # region Users

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _userRepository.GetAllAsync());
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUsers(Guid id)
        {
            var  result = await _userRepository.GetByIdAsync(id);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost("users")]
        public async Task<IActionResult> PostUsers([FromBody] UserModel model)
        {
            var actionUser = await _userRepository.GetByIdAsync(model.UserIdPerformingAction);

            if (actionUser == null)
            {
                return Unauthorized();
            }
            var doesUserExists = await _userRepository.GetByIdAsync(model.Id);
            if (doesUserExists is not null) return Conflict();
            try
            {
                // check for institution data
                var result = await _userRepository.CreateAsync(model, actionUser.Name);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
