using System;

namespace AbpDemo.Engineering.Changes.Events;

public class EcnApprovedEvent
{
    public Guid EcnId { get; set; }
    public string AffectedProductIds { get; set; }
    public Guid ApprovedBy { get; set; }

    public EcnApprovedEvent(Guid ecnId, string affectedProductIds, Guid approvedBy)
    {
        EcnId = ecnId;
        AffectedProductIds = affectedProductIds;
        ApprovedBy = approvedBy;
    }
}