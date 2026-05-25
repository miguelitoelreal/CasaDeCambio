using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Interfaces;

namespace MonitoringPlatform.Application.Services
{
    public class CloudStatusIngestionCoordinator
    {
        private readonly ICloudStatusIngestionRepository _repository;
        private readonly IReadOnlyList<ICloudStatusSourceAdapter> _sourceAdapters;

        public CloudStatusIngestionCoordinator(
            ICloudStatusIngestionRepository repository,
            IEnumerable<ICloudStatusSourceAdapter> sourceAdapters)
        {
            _repository = repository;
            _sourceAdapters = sourceAdapters.ToList();
        }

        public async Task<CloudStatusIngestionResultDto> IngestAsync(
            IReadOnlyCollection<CloudProviderSeedDto> configuredProviders,
            CancellationToken cancellationToken)
        {
            await _repository.SyncProvidersAsync(configuredProviders, cancellationToken);
            var providers = await _repository.GetEnabledProvidersAsync(cancellationToken);
            var providerResults = new List<CloudStatusProviderIngestionResultDto>();

            foreach (var provider in providers)
            {
                var adapter = _sourceAdapters.FirstOrDefault(x => x.CanHandle(provider.SourceType));
                if (adapter is null)
                {
                    providerResults.Add(await _repository.MarkSyncFailureAsync(
                        provider,
                        DateTime.UtcNow,
                        $"No adapter registered for source type {provider.SourceType}",
                        cancellationToken));
                    continue;
                }

                try
                {
                    var incidents = await adapter.GetIncidentsAsync(provider, cancellationToken);
                    providerResults.Add(await _repository.UpsertIncidentsAsync(
                        provider,
                        incidents,
                        DateTime.UtcNow,
                        cancellationToken));
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    providerResults.Add(await _repository.MarkSyncFailureAsync(
                        provider,
                        DateTime.UtcNow,
                        ex.Message,
                        cancellationToken));
                }
            }

            return new CloudStatusIngestionResultDto
            {
                ProcessedProviders = providerResults.Count,
                SuccessfulProviders = providerResults.Count(x => x.Success),
                FailedProviders = providerResults.Count(x => !x.Success),
                ChangedIncidents = providerResults.Sum(x => x.InsertedIncidents + x.UpdatedIncidents),
                ProviderResults = providerResults,
            };
        }
    }
}
