using System.Diagnostics;

namespace ONSPD.Postcodes.Reader.Model
{
    [DebuggerDisplay("{DebuggerDisplay(), nq}")]
    public class PostcodeLocation
    {
        public string Postcode { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    
        private string DebuggerDisplay()
            => $"{Postcode} " +
            $"({Latitude}, {Longitude})";
    }
}
