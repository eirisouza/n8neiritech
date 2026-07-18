using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> Full(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(_ => true, ct);
        return StatusCode(report.Status == HealthStatus.Healthy ? 200 : 503, report);
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(_ => true, ct);
        return StatusCode(report.Status == HealthStatus.Healthy ? 200 : 503, new { status = report.Status.ToString() });
    }

    [HttpGet("live")]
    public IActionResult Live() => Ok(new { status = "Live" });
}
