using System;

namespace AbpDemo.Engineering.Changes.Events;

public class EcnExecutedEvent
{
    public Guid EcnId { get; set; }
    public string AffectedBomVersionIds { get; set; }
    public string AffectedProcessRouteIds { get; set; }

    public EcnExecutedEvent(Guid ecnId, string affectedBomVersionIds, string affectedProcessRouteIds)
    {
        EcnId = ecnId;
        AffectedBomVersionIds = affectedBomVersionIds;
        AffectedProcessRouteIds = affectedProcessRouteIds;
    }
}