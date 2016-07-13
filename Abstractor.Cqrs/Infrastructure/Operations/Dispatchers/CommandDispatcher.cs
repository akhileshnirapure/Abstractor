﻿using System.Diagnostics;
using System.Threading.Tasks;
using Abstractor.Cqrs.Infrastructure.CrossCuttingConcerns;
using Abstractor.Cqrs.Interfaces.CompositionRoot;
using Abstractor.Cqrs.Interfaces.Operations;

namespace Abstractor.Cqrs.Infrastructure.Operations.Dispatchers
{
    /// <summary>
    ///     Dispatcher for a command handler.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IContainer _container;

        public CommandDispatcher(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        ///     Delegates the command parameters to the handler that implements <see cref="ICommandHandler{ICommand}" />.
        /// </summary>
        /// <param name="command">Command to be dispatched.</param>
        public void Dispatch(ICommand command)
        {
            Guard.ArgumentIsNotNull(command, nameof(command));

            var handlerType = typeof (ICommandHandler<>).MakeGenericType(command.GetType());
            dynamic handler = _container.GetInstance(handlerType);

            handler.Handle((dynamic) command);
        }

        /// <summary>
        ///     Delegates the command parameters asynchronously to the handler that implements
        ///     <see cref="ICommandHandler{ICommand}" />.
        /// </summary>
        /// <param name="command">Command to be dispatched.</param>
        /// <returns>Asynchronous task.</returns>
        public async Task DispatchAsync(ICommand command)
        {
            Guard.ArgumentIsNotNull(command, nameof(command));

            var handlerType = typeof (ICommandHandler<>).MakeGenericType(command.GetType());
            dynamic handler = _container.GetInstance(handlerType);

            await Task.Run(() => { handler.Handle((dynamic) command); });
        }
    }
}