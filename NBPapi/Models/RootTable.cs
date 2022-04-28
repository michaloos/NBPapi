namespace NBPapi.Models
{
    public class RootTable
    {
        public string table { get; set; }
        public string no { get; set; }
        public string effectiveDate { get; set; }
        public List<Rates> rates { get; set; }
    }
}
