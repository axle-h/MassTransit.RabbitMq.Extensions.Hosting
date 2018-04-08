using System;
using MassTransit;

namespace Integration.Contracts
{
    public interface ISomeCommand : CorrelatedBy<Guid>
    {
        DateTime PublishedDate { get; }

        string Bs { get; }
    }
}
