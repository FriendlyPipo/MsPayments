using MediatR;
using System;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Payments.Core.Services;
using Payments.Core.Dtos;

namespace Payments.Application.Services
{
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUserAuditService _auditService;

        public AuditBehavior(IUserAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            if (!requestName.EndsWith("Command"))
            {
                return await next();
            }

            var payload = JsonSerializer.Serialize(request);

            Guid? affectedUserId = null;
            var userIdProp = request.GetType().GetProperty("UserId");
            if (userIdProp != null)
            {
                var value = userIdProp.GetValue(request);
                if (value is Guid guidValue)
                {
                    affectedUserId = guidValue;
                }
            }
            else
            {
                var props = request.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var subObj = prop.GetValue(request);
                    if (subObj == null) continue;
                    if (subObj is string || subObj.GetType().IsValueType) continue; 

                    var subUserIdProp = subObj.GetType().GetProperty("UserId");
                    if (subUserIdProp != null)
                    {
                        var value = subUserIdProp.GetValue(subObj);
                        if (value is Guid guidValue)
                        {
                            affectedUserId = guidValue;
                            break;
                        }
                    }
                }
            }

            if (affectedUserId == null || affectedUserId == Guid.Empty)
            {
                return await next();
            }

            UserAuditDto? audit = null;

            try
            {
                var response = await next();
                audit = new UserAuditDto
                {
                    UserId = affectedUserId,
                    Title = requestName,
                    JsonData = payload,
                    IsSuccess = true,
                    ErrorMessage = null
                };
                return response;
            }
            catch (Exception ex)
            {
                audit = new UserAuditDto
                {
                    UserId = affectedUserId,
                    Title = requestName,
                    JsonData = payload,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
                throw;
            }
            finally
            {
                if (audit != null)
                {
                    await _auditService.AuditAsync(audit);
                }
            }
        }
    }
}
