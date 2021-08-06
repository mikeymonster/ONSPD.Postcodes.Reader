using System.Diagnostics;

namespace ONSPD.Postcodes.Reader.Model
{
    [DebuggerDisplay("{DebuggerDisplay(), nq}")]
    public class PostcodeSearchResult
    {
        public string Postcode { get; init; }
        public double Distance { get; init; }
    
        private string DebuggerDisplay()
            => $"{Postcode} " +
            $"{Distance} miles";
    }
}
