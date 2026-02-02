namespace AOIntuneAlerts.Domain.Events;

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
