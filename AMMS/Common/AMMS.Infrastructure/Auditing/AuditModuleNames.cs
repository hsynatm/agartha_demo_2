namespace AMMS.Infrastructure.Auditing;

internal static class AuditModuleNames
{
    public const string AssetManagement = "AssetManagement";
    public const string FaultManagement = "FaultManagement";
    public const string MaintenanceManagement = "MaintenanceManagement";

    public static string GetTableName(string moduleName) =>
        moduleName switch
        {
            AssetManagement => "audit_AssetManagement",
            FaultManagement => "audit_FaultManagement",
            MaintenanceManagement => "audit_MaintenanceManagement",
            _ => throw new InvalidOperationException($"Unsupported audit module: {moduleName}")
        };
}
