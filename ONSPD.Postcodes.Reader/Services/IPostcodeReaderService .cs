using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Services
{
    public interface IPostcodeReaderService
    {
        Task<long> LoadPostcodes();
    }
}
