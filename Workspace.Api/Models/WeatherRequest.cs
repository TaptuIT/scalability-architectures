

using System;
namespace Workspace.Api.Models
{
    public class WeatherRequest
    {
#nullable enable
        public string? Summary { get; set; }

        public float Temperature { get; set; }

        public DateTime Date { get; set; }
    }
}