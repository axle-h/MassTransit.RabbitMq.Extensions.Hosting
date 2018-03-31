using System;
using MassTransit;

namespace Example.Client.Messages
{
    public interface ICommand : CorrelatedBy<Guid>
    {
        int Count { get; }
    }
}