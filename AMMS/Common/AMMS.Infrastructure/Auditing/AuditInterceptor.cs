using AMMS.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AMMS.Infrastructure.Auditing
{
    public sealed partial class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly AuditDbContext _auditDbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrentOrganizationService _currentOrganizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditModuleContext _moduleContext;
        private readonly ILogger<AuditInterceptor> _logger;

        private readonly Dictionary<DbContextId, List<AuditEntry>> _pendingEntriesByContext = new();

        public AuditInterceptor(
            AuditDbContext auditDbContext,
            ICurrentUserService currentUserService,
            ICurrentOrganizationService currentOrganizationService,
            IHttpContextAccessor httpContextAccessor,
            IAuditModuleContext moduleContext,
            ILogger<AuditInterceptor> logger)
        {
            _auditDbContext = auditDbContext;
            _currentUserService = currentUserService;
            _currentOrganizationService = currentOrganizationService;
            _httpContextAccessor = httpContextAccessor;
            _moduleContext = moduleContext;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                _pendingEntriesByContext[eventData.Context.ContextId] = CaptureEntries(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                _pendingEntriesByContext[eventData.Context.ContextId] = CaptureEntries(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            PersistPendingEntriesAsync(eventData.Context, result, CancellationToken.None).GetAwaiter().GetResult();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            await PersistPendingEntriesAsync(eventData.Context, result, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task PersistPendingEntriesAsync(
            DbContext? context,
            int result,
            CancellationToken cancellationToken)
        {
            if (context is null)
            {
                return;
            }

            if (!_pendingEntriesByContext.Remove(context.ContextId, out var pendingEntries))
            {
                return;
            }

            if (result <= 0 || pendingEntries.Count == 0)
            {
                return;
            }

            // Audit hatası ana transaction'ı etkilememeli; exception middleware'e taşınmaz.
            try
            {
                await WriteEntriesAsync(pendingEntries, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Operation failed. {Module} {Operation}",
                    _moduleContext.ModuleName,
                    "AuditWrite");
            }
        }

        private async Task WriteEntriesAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken)
        {
            if (entries.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                _auditDbContext.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = entry.OrganizationId,
                    UserId = entry.UserId,
                    ModuleName = entry.ModuleName,
                    EntityName = entry.EntityName,
                    EntityId = entry.EntityId,
                    OperationType = entry.OperationType,
                    OldValues = entry.OldValues,
                    NewValues = entry.NewValues,
                    ChangedColumns = entry.ChangedColumns,
                    IpAddress = entry.IpAddress,
                    TraceId = entry.TraceId,
                    CreatedAt = now
                });
            }

            await _auditDbContext.SaveChangesAsync(cancellationToken);
        }
    }



}
