using AMMS.Shared.Entities;
using FaultManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FaultManagement.Domain.Entities
{
    public sealed class FaultActivity : BaseEntity
    {
        public Guid FaultReportId { get; private set; }

        public FaultActivityType ActivityType { get; private set; }

        public string Description { get; private set; } = null!;

        public Guid PerformedByUserId { get; private set; }
        public string PerformedByDisplayName { get; private set; } = null!;

        public DateTime PerformedAt { get; private set; }
    }


}
