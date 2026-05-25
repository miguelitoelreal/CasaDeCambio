using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.API.Services;
using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Infrastructure.Persistence;

namespace MonitoringPlatform.API.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertController : ControllerBase
    {
        private readonly IAlertRuleRepository _ruleRepository;
        private readonly IAlertHistoryRepository _historyRepository;
        private readonly ICurrentUserContext _currentUser;
        private readonly UserAlertPreferenceService _preferenceService;
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;

        public AlertController(
            IAlertRuleRepository ruleRepository,
            IAlertHistoryRepository historyRepository,
            ICurrentUserContext currentUser,
            UserAlertPreferenceService preferenceService,
            AppDbContext dbContext,
            IEmailService emailService)
        {
            _ruleRepository = ruleRepository;
            _historyRepository = historyRepository;
            _currentUser = currentUser;
            _preferenceService = preferenceService;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [HttpGet("rules")]
        public async Task<IActionResult> GetRules()
        {
            var rules = await _ruleRepository.GetAllByTenantAsync(_currentUser.TenantId);
            return Ok(rules);
        }

        [HttpPost("rules")]
        public async Task<IActionResult> CreateRule([FromBody] CreateAlertRuleDto dto)
        {
            var rule = await _ruleRepository.CreateAsync(_currentUser.TenantId, dto);
            return Ok(rule);
        }

        [HttpPut("rules/{id:guid}")]
        public async Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateAlertRuleDto dto)
        {
            var rule = await _ruleRepository.UpdateAsync(id, dto);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

        [HttpDelete("rules/{id:guid}")]
        public async Task<IActionResult> DeleteRule(Guid id)
        {
            var deleted = await _ruleRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int limit = 50)
        {
            var history = await _historyRepository.GetByTenantAsync(_currentUser.TenantId, limit);
            return Ok(history);
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetMyPreferences(CancellationToken cancellationToken)
        {
            var preferences = await _preferenceService.GetMyPreferencesAsync(cancellationToken);
            return Ok(preferences);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdateMyPreferences([FromBody] UserAlertPreferenceDto dto, CancellationToken cancellationToken)
        {
            await _preferenceService.UpdateMyPreferencesAsync(dto, cancellationToken);
            return NoContent();
        }

        [HttpGet("cloud-providers")]
        public async Task<IActionResult> GetCloudProviders(CancellationToken cancellationToken)
        {
            var providers = await _dbContext.CloudProviders
                .AsNoTracking()
                .Where(c => c.IsEnabled)
                .OrderBy(c => c.Name)
                .Select(c => new CloudProviderOptionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync(cancellationToken);
            return Ok(providers);
        }

        [HttpPost("test-alert")]
        public async Task<IActionResult> SendTestAlert([FromBody] TestAlertRequestDto request, CancellationToken cancellationToken)
        {
            var pref = await _preferenceService.GetMyPreferencesAsync(cancellationToken);
            if (!pref.EmailEnabled)
            {
                return BadRequest(new { message = "Tienes desactivadas las alertas por email. Actívalas en preferencias para recibir la prueba." });
            }

            var user = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

            if (user == null) return NotFound();

            var tenant = await _dbContext.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == _currentUser.TenantId, cancellationToken);

            var typeLabel = request.Type?.ToLowerInvariant() switch
            {
                "critical" => "Incidencia crítica",
                "major" => "Incidencia mayor",
                _ => "Monitor caído",
            };

            var subject = $"[PRUEBA] Alerta de {typeLabel} - MonitoringPlatform";
            var body = $@"
<html>
<body style='font-family:Segoe UI, Arial, sans-serif; color:#333;'>
<h2 style='color:#2563eb;'>Alerta de prueba</h2>
<p><strong>Tipo:</strong> {typeLabel}</p>
<p><strong>Usuario:</strong> {user.FullName} ({user.Email})</p>
<p><strong>Tenant:</strong> {tenant?.Name ?? "N/A"}</p>
<p><strong>Hora:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
<hr style='border:none; border-top:1px solid #e5e7eb; margin:16px 0;'>
<p style='color:#6b7280; font-size:12px;'>Este es un correo de prueba generado desde tus preferencias de alertas. Si lo recibes, tu configuración de notificaciones está funcionando correctamente.</p>
</body>
</html>";

            var recipients = new List<string> { user.Email! };
            recipients.AddRange(pref.AdditionalEmails);
            recipients = recipients.Distinct().ToList();

            var errors = new List<string>();
            foreach (var email in recipients)
            {
                var ok = await _emailService.SendEmailAsync(email, subject, body);
                if (!ok) errors.Add($"Falló envío a {email}");
            }

            if (errors.Count > 0)
                return StatusCode(500, new { message = "Algunos envíos fallaron.", errors });

            return Ok(new { message = "Alerta de prueba enviada correctamente.", recipients });
        }
    }
}
