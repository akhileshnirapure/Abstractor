using System;
using System.Collections.Generic;
using System.Linq;
using Abstractor.Cqrs.Interfaces.CrossCuttingConcerns;
using Abstractor.Cqrs.Interfaces.Events;
using Abstractor.Cqrs.Interfaces.Operations;

namespace Abstractor.Cqrs.Infrastructure.Operations.Decorators
{
    /// <summary>
    ///     Logs the execution of the command handler.
    /// </summary>
    /// <typeparam name="TCommand">Command to be handled.</typeparam>
    public sealed class CommandLoggerDecorator<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly IAttributeFinder _attributeFinder;
        private readonly Func<ICommandHandler<TCommand>> _handlerFactory;
        private readonly ILogger _logger;
        private readonly ILoggerSerializer _loggerSerializer;
        private readonly IStopwatch _stopwatch;

        public CommandLoggerDecorator(
            Func<ICommandHandler<TCommand>> handlerFactory,
            IAttributeFinder attributeFinder,
            IStopwatch stopwatch,
            ILoggerSerializer loggerSerializer,
            ILogger logger)
        {
            _handlerFactory = handlerFactory;
            _attributeFinder = attributeFinder;
            _stopwatch = stopwatch;
            _loggerSerializer = loggerSerializer;
            _logger = logger;
        }

        /// <summary>
        ///     Logs the execution of the command handler.
        /// </summary>
        /// <param name="command">Command to be handled.</param>
        /// <returns>List of domain events raised by the command, if any.</returns>
        public IEnumerable<IDomainEvent> Handle(TCommand command)
        {
            if (!_attributeFinder.Decorates(command.GetType(), typeof (LogAttribute)))
                return _handlerFactory().Handle(command)?.ToList();

            _stopwatch.Start();

            try
            {
                _logger.Log($"Executing command \"{command.GetType().Name}\" with the parameters:");

                try
                {
                    var parameters = _loggerSerializer.Serialize(command);
                    _logger.Log(parameters);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Could not serialize the parameters: {ex.Message}");
                }

                return _handlerFactory().Handle(command)?.ToList();
            }
            catch (Exception ex)
            {
                _logger.Log("Exception caught: " + ex.Message);

                if (ex.InnerException != null)
                    _logger.Log("Inner exception caught: " + ex.InnerException.Message);

                throw;
            }
            finally
            {
                _stopwatch.Stop();

                _logger.Log($"Command \"{command.GetType().Name}\" executed in {_stopwatch.GetElapsed()}.");
            }
        }
    }
}