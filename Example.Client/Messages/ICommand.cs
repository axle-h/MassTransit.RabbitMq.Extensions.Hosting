using System;

namespace Example.Client.Messages
{
    public interface ICommand
    {
        Guid Id { get; }

        DateTimeOffset Date { get; }
    }
}