using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FaqsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public FaqsController(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _context.Faqs.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId).OrderBy(x => x.SortOrder).Select(x => new FaqResponse(x.Id, x.StoreId, x.Question, x.Answer, x.Category, x.SortOrder, x.IsActive)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var faq = await _context.Faqs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return faq is null ? NotFound() : Ok(new FaqResponse(faq.Id, faq.StoreId, faq.Question, faq.Answer, faq.Category, faq.SortOrder, faq.IsActive));
    }

    [HttpPost]
    public async Task<IActionResult> Create(FaqRequest request, CancellationToken ct)
    {
        var faq = new Faq { TenantId = _currentUser.TenantId, StoreId = request.StoreId, Question = request.Question, Answer = request.Answer, Category = request.Category, SortOrder = request.SortOrder, IsActive = request.IsActive };
        await _context.Faqs.AddAsync(faq, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = faq.Id }, new FaqResponse(faq.Id, faq.StoreId, faq.Question, faq.Answer, faq.Category, faq.SortOrder, faq.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, FaqRequest request, CancellationToken ct)
    {
        var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (faq is null) return NotFound();
        faq.Question = request.Question; faq.Answer = request.Answer; faq.Category = request.Category; faq.SortOrder = request.SortOrder; faq.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return Ok(new FaqResponse(faq.Id, faq.StoreId, faq.Question, faq.Answer, faq.Category, faq.SortOrder, faq.IsActive));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var faq = await _context.Faqs.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (faq is null) return NotFound();
        _context.Faqs.Remove(faq);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
