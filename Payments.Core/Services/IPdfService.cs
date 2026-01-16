using System.Threading.Tasks;

namespace Payments.Core.Services
{
    public interface IPdfService<TData>
    {
        Task<byte[]> GeneratePdfAsync(TData data);
    }
}
