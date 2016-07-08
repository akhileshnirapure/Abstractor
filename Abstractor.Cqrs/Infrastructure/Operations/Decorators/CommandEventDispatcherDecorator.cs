﻿using System;
using System.Diagnostics;
using Abstractor.Cqrs.Interfaces.Events;
using Abstractor.Cqrs.Interfaces.Operations;

namespace Abstractor.Cqrs.Infrastructure.Operations.Decorators
{
    /// <summary>
    ///     Dispatches the event registered for the current command after the execution of
    ///     <see cref="ICommandHandler{TCommand}" />.
    /// </summary>
    /// <typeparam name="TCommand">Comando que será executado.</typeparam>
    [DebuggerStepThrough]
    public sealed class CommandEventDispatcherDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand, IEvent
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Func<ICommandHandler<TCommand>> _handlerFactory;

        public CommandEventDispatcherDecorator(
            IEventDispatcher eventDispatcher,
            Func<ICommandHandler<TCommand>> handlerFactory)
        {
            _eventDispatcher = eventDispatcher;
            _handlerFactory = handlerFactory;
        }

        /// <summary>
        ///     Dispatches the event after the command execution.
        /// </summary>
        /// <param name="command"></param>
        public void Handle(TCommand command)
        {
            _handlerFactory().Handle(command);
            _eventDispatcher.Dispatch(command);
        }
    }
}