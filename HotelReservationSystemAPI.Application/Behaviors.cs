using FluentValidation;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                _logger.LogWarning("Validation errors for request {RequestType} - {Errors}", typeof(TRequest).Name, string.Join("; ", failures.Select(f => f.ErrorMessage)));
            
                var message = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var apiResponse = APIResponse<object>.Fail(HttpStatusCode.BadRequest, message);
               
                if (typeof(TResponse) == typeof(APIResponse<UserDto>))
                {
                    return (TResponse)(object)APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, message);
                }
                else if (typeof(TResponse) == typeof(APIResponse<RoleDto>))
                {
                    return (TResponse)(object)APIResponse<RoleDto>.Fail(HttpStatusCode.BadRequest, message);
                }
               
                else if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(APIResponse<>))
                {
                    var dtoType = typeof(TResponse).GetGenericArguments()[0];
                    var failMethod = typeof(APIResponse<>).MakeGenericType(dtoType).GetMethod("Fail", new[] { typeof(HttpStatusCode), typeof(string) });
                    var response = failMethod.Invoke(null, new object[] { HttpStatusCode.BadRequest, message });
                    return (TResponse)response;
                }
              
                throw new ValidationException(failures);
            }
        }

        return await next();  
    }
}