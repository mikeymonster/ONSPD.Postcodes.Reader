using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Data
{
    public interface IDataRepository
    {
        Task<IEnumerable<PostcodeLocation>> GetPostcodes();
        Task AddPostcode(PostcodeLocation postcode);
        Task UpsertPostcodes(IEnumerable<PostcodeLocation> postcodes);
        Task UpsertPostcodesUsingAdo(IEnumerable<PostcodeLocation> postcodes);
    }
}
