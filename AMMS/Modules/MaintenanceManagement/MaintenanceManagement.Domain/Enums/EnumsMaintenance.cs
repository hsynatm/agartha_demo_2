namespace MaintenanceManagement.Domain.Enums
{
    public enum MaintenanceType
    {
        TimeBased = 1,
        CounterBased = 2,
        Reactive = 3,
        Predictive = 4,
        ServiceBulletin = 5,
        AirworthinessDirective = 6,
        Calibration = 7,
        PeriodicInspection = 8
    }

    public enum MaintenanceTriggerType
    {
        CalendarDay = 1,
        FlightHour = 2,
        LandingCycle = 3,
        UsageCounter = 4,
        FaultReport = 5,
        InspectionFinding = 6,
        PredictiveAlert = 7
    }

    public enum WorkOrderType
    {
        Corrective = 1,
        Preventive = 2,
        Predictive = 3,
        Calibration = 4,
        Inspection = 5,
        ServiceBulletin = 6,
        AirworthinessDirective = 7
    }

    public enum WorkOrderStatus
    {
        Open = 1,
        Assigned = 2,
        InProgress = 3,
        OnHold = 4,
        Completed = 5,
        Closed = 6,
        Cancelled = 7
    }

    public enum WorkOrderPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum AssignmentRole
    {
        Technician = 1,
        Inspector = 2,
        Engineer = 3,
        Supervisor = 4
    }

    public enum ApprovalStepType
    {
        TechnicalApproval = 1,
        SupervisorApproval = 2,
        QualityApproval = 3,
        FinalApproval = 4
    }

    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum AttachmentType
    {
        Image = 1,
        Video = 2,
        Document = 3,
        Pdf = 4,
        DigitalSignature = 5,
        TestResult = 6,
        MaintenanceRecord = 7,
        Other = 99
    }
}
