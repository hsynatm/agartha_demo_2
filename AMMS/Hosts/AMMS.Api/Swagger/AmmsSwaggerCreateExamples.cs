namespace AMMS.Api.Swagger;

internal static class AmmsSwaggerCreateExamples
{
    public const string FaultReport = """
        {
          "faultNumber": "FLT-001",
          "assetId": "3e681af3-2418-49d4-8316-ea5a82549eb0",
          "assetCode": "AST-001",
          "assetName": "Test Asset",
          "assetSerialNumber": null,
          "assetTailNumber": null,
          "title": "Motor arızası",
          "description": "Arıza açıklaması",
          "type": 1,
          "priority": 1,
          "impactType": 1,
          "status": 1,
          "rpnScore": null,
          "reportedAt": "2026-06-19T10:00:00Z",
          "reportedByUserId": "22222222-2222-2222-2222-222222222222",
          "reportedByDisplayName": "Test User",
          "assignedToUserId": null,
          "assignedToDisplayName": null,
          "latitude": null,
          "longitude": null,
          "attachments": [],
          "activities": [],
          "repairActions": []
        }
        """;

    public const string Asset = """
        {
          "assetCode": "AST-001",
          "name": "Test Asset",
          "category": 1,
          "status": 1,
          "manufacturer": null,
          "model": null,
          "serialNumber": null,
          "documents": [],
          "locationHistories": [],
          "statusHistories": [],
          "parts": [],
          "lifeLimits": []
        }
        """;

    public const string WorkOrder = """
        {
          "workOrderNumber": "WO-001",
          "assetId": "3e681af3-2418-49d4-8316-ea5a82549eb0",
          "assetCode": "AST-001",
          "assetName": "Test Asset",
          "type": 1,
          "status": 1,
          "priority": 1,
          "title": "Periyodik bakım",
          "description": "İş emri açıklaması",
          "plannedStartDate": "2026-06-20T08:00:00Z",
          "plannedEndDate": "2026-06-20T17:00:00Z",
          "tasks": [],
          "assignments": [],
          "materials": [],
          "tools": [],
          "approvals": [],
          "attachments": [],
          "statusHistories": []
        }
        """;

    public static string? Resolve(string controllerName) =>
        controllerName switch
        {
            "FaultManagement" => FaultReport,
            "AssetManagement" => Asset,
            "MaintenanceManagement" => WorkOrder,
            _ => null
        };
}
