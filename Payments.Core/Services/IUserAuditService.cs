using Payments.Core.Dtos;
using System.Threading.Tasks;

namespace Payments.Core.Services
{
    public interface IUserAuditService
    {
        Task AuditAsync(UserAuditDto auditDto);
    }
}
