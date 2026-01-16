using System.Threading.Tasks;
using Payments.Core.Dtos;

namespace Payments.Core.Services
{
    public interface IUserLogService
    {
        Task LogAsync(UserLogDto logDto);
    }
}
