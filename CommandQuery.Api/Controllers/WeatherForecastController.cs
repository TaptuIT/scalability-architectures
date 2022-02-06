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
    private static string Summary = "Ok";

    /// <summary>
    /// Static variable to simulate a database access
    /// </summary>
    private static float Temperature = 0;

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Query]
    public WeatherDto Get()
    {
        _logger.LogInformation("Getting Weather Forecast Information.");
        _logger.LogInformation("Temperature around " + Temperature);
        _logger.LogInformation("Summary: " + Summary);
        return new WeatherDto
        {
            Summary = Summary,
            Temperature = Temperature
        };
    }

    [Command]
    [HttpPut]
    public IActionResult Set([FromBody] WeatherDto weather)
    {
        _logger.LogInformation("Setting Weather Forecast Information.");
        // Modify details in the static fields here - in production you would be communicating with database
        Summary = weather.Summary;
        Temperature = weather.Temperature;
        return NoContent();
    }
}
