
using BlockiFinAid.Services.SmartContracts.BankAccount;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers;

[Route("api/[controller]/")]
[ApiController]
public class BankAccountContractsController : ControllerBase
{
    private readonly BankAccountContract _bankAccount;

    public BankAccountContractsController(BankAccountContract bac)
    {
        _bankAccount = bac;
    }


    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _bankAccount.GetAllAsync());
    }

    [HttpGet("{studentNumber}")]
    public async Task<IActionResult> GetStudentNumber(string studentNumber)
    {
        var results = await _bankAccount.GetByStudentNumberAsync(studentNumber);
        if(results is null)
            return NotFound();
        return Ok(results);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] BankAccountInputDto request)
    {
        try
        {
            var results = await _bankAccount.AddBankAccountAsync(request);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] BankAccountUpdateDto request)
    {
        try
        {
            var results = await _bankAccount.UpdateBankAccountByNumberAsync(request);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("accountNumber")]
    public async Task<IActionResult> GetBankAccountByAccNumberAsync(string accountNumber)
    {
        return Ok(await _bankAccount.GetByBankAccountNumberAsync(accountNumber));
    }

    [HttpPut("confirm")]
    public async Task<IActionResult> ConfirmBankDetails([FromBody] BankAccountConfirmationDto request)
    {
        try
        {
            var results = await _bankAccount.StudentDataChangeConfirmationAsync(request);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

