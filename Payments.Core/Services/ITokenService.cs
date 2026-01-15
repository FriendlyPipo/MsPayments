using System.Threading.Tasks;

namespace Payments.Core.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
    }
}
