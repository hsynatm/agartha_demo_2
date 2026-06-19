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
        private readonly string _moduleName;
        private readonly ILogger<AuditInterceptor> _logger;

        private readonly Dictionary<DbContextId, List<AuditEntry>> _pendingEntriesByContext = new();

        public AuditInterceptor(
            AuditDbContext auditDbContext,
            ICurrentUserService currentUserService,
            ICurrentOrganizationService currentOrganizationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditInterceptor> logger,
            string moduleName)
        {
            _auditDbContext = auditDbContext;
            _currentUserService = currentUserService;
            _currentOrganizationService = currentOrganizationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _moduleName = moduleName;
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
                    _moduleName,
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
                AddAuditLog(entry, now);
            }

            await _auditDbContext.SaveChangesAsync(cancellationToken);
        }

        private void AddAuditLog(AuditEntry entry, DateTime now)
        {
            var auditLog = CreateAuditLogEntity(entry.ModuleName);
            auditLog.Id = Guid.NewGuid();
            auditLog.OrganizationId = entry.OrganizationId;
            auditLog.UserId = entry.UserId;
            auditLog.ModuleName = entry.ModuleName;
            auditLog.EntityName = entry.EntityName;
            auditLog.EntityId = entry.EntityId;
            auditLog.OperationType = entry.OperationType;
            auditLog.OldValues = entry.OldValues;
            auditLog.NewValues = entry.NewValues;
            auditLog.ChangedColumns = entry.ChangedColumns;
            auditLog.IpAddress = entry.IpAddress;
            auditLog.TraceId = entry.TraceId;
            auditLog.CreatedAt = now;

            switch (entry.ModuleName)
            {
                case AuditModuleNames.AssetManagement:
                    _auditDbContext.AssetManagementAuditLogs.Add((AssetManagementAuditLog)auditLog);
                    break;
                case AuditModuleNames.FaultManagement:
                    _auditDbContext.FaultManagementAuditLogs.Add((FaultManagementAuditLog)auditLog);
                    break;
                case AuditModuleNames.MaintenanceManagement:
                    _auditDbContext.MaintenanceManagementAuditLogs.Add((MaintenanceManagementAuditLog)auditLog);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported audit module: {entry.ModuleName}");
            }
        }

        private static AuditLogEntry CreateAuditLogEntity(string moduleName) =>
            moduleName switch
            {
                AuditModuleNames.AssetManagement => new AssetManagementAuditLog(),
                AuditModuleNames.FaultManagement => new FaultManagementAuditLog(),
                AuditModuleNames.MaintenanceManagement => new MaintenanceManagementAuditLog(),
                _ => throw new InvalidOperationException($"Unsupported audit module: {moduleName}")
            };
    }
}
