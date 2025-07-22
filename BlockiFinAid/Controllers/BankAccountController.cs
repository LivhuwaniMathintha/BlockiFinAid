using BlockiFinAid.Data.Models;
using BlockiFinAid.Services.Repository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers;

[Route("api/[controller]/")]
[ApiController]
public class BankAccountController : ControllerBase
{
    private readonly IBaseRepository<BankAccountModel> _bankRepository;

    public BankAccountController(IBaseRepository<BankAccountModel> bankAccountRepository)
    {
        _bankRepository = bankAccountRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var results = await _bankRepository.GetAllAsync();
        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _bankRepository.GetByIdAsync(id);

        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] BankAccountModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _bankRepository.CreateAsync(model);

        if (result is null)
        {
            return BadRequest(model);
        }
        return StatusCode(201, result);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] BankAccountModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _bankRepository.UpdateAsync(model);

        if (result is false)
        {
            return BadRequest(model);
        }
        return NoContent();
    }
}