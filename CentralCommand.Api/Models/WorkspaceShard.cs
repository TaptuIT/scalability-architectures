namespace CentralCommand.Api.Models
{
    public class WorkspaceShard
    {
        public long Id { get; set; }
        public long RegionId { get; set; }
        public string SqlServerName { get; set; }
        public string DatabaseName { get; set; }
        public bool IsDefaultForRegion { get; set; }
    }
}
