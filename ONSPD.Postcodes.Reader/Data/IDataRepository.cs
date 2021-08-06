using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Data
{
    public interface IDataRepository
    {
        Task AddPostcode(PostcodeLocation postcode);

        Task<IEnumerable<PostcodeLocation>> GetPostcodes();
        Task<IEnumerable<PostcodeLocation>> GetPostcodes(string filter);

        Task UpsertPostcodes(IEnumerable<PostcodeLocation> postcodes);
        Task UpsertPostcodesUsingAdo(IEnumerable<PostcodeLocation> postcodes);

        Task<IEnumerable<PostcodeSearchResult>> PerformDistanceSearch(string postcode, string filter);
    }
}
