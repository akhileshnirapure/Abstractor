using System;
using System.Threading.Tasks;
using Abstractor.Cqrs.Interfaces.CrossCuttingConcerns;
using Abstractor.Cqrs.Interfaces.Operations;

namespace Abstractor.Cqrs.Infrastructure.Operations.Decorators
{
    /// <summary>
    ///     Logs the execution of the query handler.
    /// </summary>
    /// <typeparam name="TQuery">Query to be handled.</typeparam>
    /// <typeparam name="TResult">Return type.</typeparam>
    public sealed class QueryAsyncLoggerDecorator<TQuery, TResult> : IQueryAsyncHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private readonly IAttributeFinder _attributeFinder;
        private readonly Func<IQueryAsyncHandler<TQuery, TResult>> _handlerFactory;
        private readonly ILogger _logger;
        private readonly ILoggerSerializer _loggerSerializer;
        private readonly IStopwatch _stopwatch;

        public QueryAsyncLoggerDecorator(
            Func<IQueryAsyncHandler<TQuery, TResult>> handlerFactory,
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
        ///     Logs the execution of the query handler.
        /// </summary>
        /// <param name="query">Query to be handled.</param>
        /// <returns>Returns an Task of type <typeparamref name="TResult" />.</returns>
        public Task<TResult> HandleAsync(TQuery query)
        {
            var handler = _handlerFactory();

            if (!_attributeFinder.Decorates(query.GetType(), typeof (LogAttribute)))
                return handler.HandleAsync(query);

            _stopwatch.Start();

            try
            {
                _logger.Log($"Executing query \"{query.GetType().Name}\" with the parameters:");

                try
                {
                    var parameters = _loggerSerializer.Serialize(query);
                    _logger.Log(parameters);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Could not serialize the parameters: {ex.Message}");
                }

                return handler.HandleAsync(query);
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

                _logger.Log($"Query \"{query.GetType().Name}\" executed in {_stopwatch.GetElapsed()}.");
            }
        }
    }
}