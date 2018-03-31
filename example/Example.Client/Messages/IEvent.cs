using System;
using MassTransit;

namespace Example.Client.Messages
{
    public interface IEvent : CorrelatedBy<Guid>
    {
        int Count { get; }
    }
}
