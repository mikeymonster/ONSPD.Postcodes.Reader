using ONSPD.Postcodes.Reader.Model;
using ONSPD.Postcodes.Reader.Model.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Services
{
    public interface IPostcodeService
    {
        Task<long> LoadPostcodes();
        Task<IEnumerable<PostcodeSearchResult>> Search(
            string postcode, 
            string filter = null, 
            SearchMethod method = SearchMethod.SqlSpatial);
    }
}
