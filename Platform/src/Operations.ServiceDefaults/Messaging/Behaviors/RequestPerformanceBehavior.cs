// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults.Messaging.Behaviors;

//
// public partial class RequestPerformanceBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
//     where TRequest : IRequest<TResponse>
// {
//     public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//     {
//         if (!logger.IsEnabled(LogLevel.Debug))
//         {
//             return await next();
//         }
//
//         LogRequestReceived(typeof(TRequest).Name, request);
//
//         var startedTime = Stopwatch.GetTimestamp();
//
//         var response = await next();
//
//         var elapsedTime = Stopwatch.GetElapsedTime(startedTime);
//
//         LogRequestCompleted(typeof(TRequest).Name, elapsedTime.TotalMilliseconds);
//
//         return response;
//     }
//
//     [LoggerMessage(
//         EventId = 1,
//         Level = LogLevel.Debug,
//         Message = "{RequestType} received. Request: {@Request}")]
//     private partial void LogRequestReceived(string requestType, TRequest request);
//
//     [LoggerMessage(
//         EventId = 2,
//         Level = LogLevel.Debug,
//         Message = "{RequestType} completed in {RequestExecutionTime:000} ms")]
//     private partial void LogRequestCompleted(string requestType, double requestExecutionTime);
// }
