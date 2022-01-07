using System;

namespace Workspace.Api.Models
{
    public class WeatherRecord
    {
        public long Id { get; set; }
        public long WorkspaceId { get; set; }
        public string Summary { get; set; }
        public float Temperature { get; set; }
        public DateTime Date { get; set; }

    }
}
