using BlockiFinAid.Services.SmartContracts.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class PaymentContractsController : ControllerBase
    {
        private readonly PaymentContract _paymentContract;

        public PaymentContractsController(PaymentContract paymentContract)
        {
            _paymentContract = paymentContract;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentInputDto request)
        {
            try
            {
                var result = await _paymentContract.AddPaymentAsync(request);
                if (result.IsSuccess)
                    return Ok(result.Data);
                return BadRequest("Could not add payment");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _paymentContract.GetAllPaymentsAsync());
        }

        [HttpGet("{studentNumber}")]
        public async Task<IActionResult> Get(string studentNumber)
        {
            var result = await _paymentContract.GetPaymentByStudentNumberAsync(studentNumber);
            
            if(result is not null)
                return Ok(result);
            return NotFound(studentNumber);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PaymentUpdateDto request)
        {
            try
            {
                var result = await _paymentContract.UpdatePaymentByIdAsync(request);
                if (result.IsSuccess)
                    return Ok(result.Data);
                return BadRequest("Could not update payment");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
