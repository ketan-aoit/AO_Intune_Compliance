using MediatR;

namespace AOIntuneAlerts.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
