using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MonitoringPlatform.API.Configurations;
using MonitoringPlatform.API.Hubs;
using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Services;
using MonitoringPlatform.API.Services;

namespace MonitoringPlatform.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/cloud-status")]
    public class CloudStatusController : ControllerBase
    {
        private readonly CloudStatusService _service;
        private readonly CloudStatusIngestionCoordinator _coordinator;
        private readonly CloudStatusOptions _options;
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly ILogger<CloudStatusController> _logger;

        public CloudStatusController(
            CloudStatusService service,
            CloudStatusIngestionCoordinator coordinator,
            IOptions<CloudStatusOptions> options,
            IHubContext<MonitoringHub> hubContext,
            ILogger<CloudStatusController> logger)
        {
            _service = service;
            _coordinator = coordinator;
            _options = options.Value;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview(
            [FromQuery] string? provider,
            [FromQuery] int? severity,
            [FromQuery] bool activeOnly = false,
            [FromQuery] int take = 100)
        {
            var result = await _service.GetOverviewAsync(new CloudStatusQueryDto
            {
                Provider = provider,
                Severity = severity,
                ActiveOnly = activeOnly,
                Take = take,
            });

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            var result = await _coordinator.IngestAsync(_options.ToSeedDtos(), cancellationToken);

            if (result.ChangedIncidents > 0)
            {
                await _hubContext.Clients.All.SendAsync(
                    "CloudStatusChanged",
                    new
                    {
                        updatedAt = DateTime.UtcNow,
                        changedIncidents = result.ChangedIncidents,
                        source = "manual-refresh",
                    },
                    cancellationToken);
            }

            return Ok(result);
        }

        [HttpPost("translate")]
        public async Task<IActionResult> TranslateIncident(
            [FromBody] CloudIncidentTranslationRequestDto request,
            [FromServices] CloudStatusTranslationService translationService,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = "Se requiere contenido para traducir.",
                    Instance = HttpContext.Request.Path,
                });
            }

            try
            {
                var result = await translationService.TranslateIncidentAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = ex.Message,
                    Instance = HttpContext.Request.Path,
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Cloud incident translation failed for incident {IncidentId}.",
                    request.IncidentId ?? "(no-incident-id)");

                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "Traducción no disponible",
                    Detail = "No se pudo traducir este incidente en este momento. Intenta nuevamente en unos instantes.",
                    Instance = HttpContext.Request.Path,
                });
            }
        }
    }
}
