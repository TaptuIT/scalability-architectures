using System;
using System.Collections.Generic;
using System.Linq;
using CommandQuery.Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Api.Controllers;

[ApiController]
[Route("forecast")]
public class WeatherForecastController : ControllerBase
{
    private static List<string> Summaries = new List<string>
    {
        "Ok"
    };

    private static float Temperature = 0;

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Query]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("Getting Weather Forecast Information.");
        _logger.LogInformation("Temperature around " + Temperature);
        _logger.LogInformation("Summaries Available: " + Summaries.Count);
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Temperature + Random.Shared.Next(-10, 10),
            Summary = Summaries[Random.Shared.Next(Summaries.Count)]
        })
        .ToArray();
    }

    [Command]
    [HttpPut]
    public IActionResult Set([FromBody] WeatherDto weather)
    {
        _logger.LogInformation("Setting Weather Forecast Information.");
        if(weather.Summary != null){
            Summaries.AddRange(weather.Summary);
        }
        
        Temperature = weather.Temperature;
        return NoContent();
    }
}
