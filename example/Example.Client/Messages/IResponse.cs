using System;
using MassTransit;

namespace Example.Client.Messages
{
    public interface IResponse : CorrelatedBy<Guid>
    {
        int Count { get; }
    }
}
