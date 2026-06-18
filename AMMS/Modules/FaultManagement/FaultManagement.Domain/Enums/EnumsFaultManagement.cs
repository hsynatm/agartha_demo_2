namespace FaultManagement.Domain.Enums
{
    public enum FaultType
    {
        Mechanical = 1,
        Electrical = 2,
        Hydraulic = 3,
        Software = 4,
        Structural = 5,
        HumanError = 6,
        Other = 99
    }

    public enum FaultPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum FaultImpactType
    {
        FlightSafety = 1,
        TimeLoss = 2,
        Cost = 3,
        OperationalAvailability = 4
    }

    public enum FaultStatus
    {
        Reported = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Reclassified = 5,
        WorkOrderCreated = 6,
        InRepair = 7,
        Resolved = 8,
        Closed = 9
    }

    public enum FaultActivityType
    {
        Created = 1,
        Reviewed = 2,
        Approved = 3,
        Rejected = 4,
        Reclassified = 5,
        Assigned = 6,
        WorkOrderCreated = 7,
        RepairStarted = 8,
        RepairCompleted = 9,
        Closed = 10
    }

    public enum AttachmentType
    {
        Image = 1,
        Video = 2,
        Document = 3,
        Other = 99
    }
}
