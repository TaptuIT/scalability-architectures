using System;

namespace CommandQuery.Api;

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public float TemperatureC { get; set; }

    public float TemperatureF => 32 + (TemperatureC / 0.5556f);

#nullable enable

    public string? Summary { get; set; }

#nullable disable
}
