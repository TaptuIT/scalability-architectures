using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workspace.Api.DataAccess;
using Workspace.Api.Models;

namespace Workspace.Api.Controllers
{

    [ApiController]
    [Route("[controller]/{workspaceId}")]
    public class WeatherRecordController : ControllerBase
    {

        private readonly ILogger<WeatherRecordController> _logger;
        private readonly ICentralCommandDataAccess _centralCommandDataAccess;
        private readonly IWorkspaceDataAccess _workspaceDataAccess;

        public WeatherRecordController(ILogger<WeatherRecordController> logger, ICentralCommandDataAccess centralCommandDataAccess, IWorkspaceDataAccess workspaceDataAccess)
        {
            _logger = logger;
            _centralCommandDataAccess = centralCommandDataAccess;
            _workspaceDataAccess = workspaceDataAccess;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<WeatherRecord>>> GetAllForecast([FromRoute] long workspaceId)
        {
            // VALIDATE request as normal.
            if (workspaceId < 0) return NotFound($"Invalid Workspace Id: {workspaceId}");

            // Configure Shard and validate.
            if (await HandleShard(workspaceId) == false)
            {
                return BadRequest($"Workspace Not Found in this Region.");
            }

            // DO the work.
            IEnumerable<WeatherRecord> data = await _workspaceDataAccess.GetWeatherforecasts(workspaceId);
            _logger.LogInformation($"... '{data?.ToList().Count ?? 0}' data found from '{_workspaceDataAccess.Shard.DatabaseName}' Database.");

            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult<WeatherRecord>> AddForecast([FromRoute] long workspaceId, [FromBody] WeatherRequest weather)
        {
            // VALIDATE request as normal
            if (weather == null || weather.Summary.Length == 0)
            {
                _logger.LogError("Empty Dataset.");
                return BadRequest("Empty Dataset.");
            }
            if (workspaceId <= 0) return NotFound($"Invalid Workspace Id: {workspaceId}");

            // Configure Shard and validate.
            if (await HandleShard(workspaceId) == false)
            {
                return NotFound($"Workspace Not Found in this Region.");
            }

            // DO the work.
            WeatherRecord record = new WeatherRecord { Date = weather.Date, Summary = weather.Summary, Temperature = weather.Temperature, WorkspaceId = workspaceId };
            WeatherRecord data = await _workspaceDataAccess.SaveWeatherRecord(record);

            return Ok(data);
        }

        /// <summary>
        /// This method handles retrieving shard information
        /// In a production app you would likely have a service or handler responsible for this that injects the shard information into the required location (such as a scoped request context)
        /// </summary>
        private async Task<bool> HandleShard(long workspaceId)
        {
            //Calculate the URL of this API that is running
            string thisApiUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            // RETRIEVE the shard information. You probably should ultimately cache this information as it is likely to be relatively static. 
            _logger.LogInformation($"... Get Shard info for Workspace: {workspaceId}");
            var shard = await _centralCommandDataAccess.GetShardByWorkspaceIdAsync(workspaceId);
            if (shard == null) return false;

            // Validate that the shard is managed by this API. 
            // We are using the API url here, but you can also use the sql server name to validate this (this is what we do in our app)
            if (thisApiUrl.Equals(shard.ApiUrl, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            // Send the shard information directly in the data access object.
            // For a more complicated app you can store this in a context scoped to the curent request. 
            _logger.LogInformation($"... Use Shard info and SAVE data from: {shard.DatabaseName} Database.");
            _workspaceDataAccess.Shard = shard;

            // Ensure that the required workspace records are present. 
            await _workspaceDataAccess.EnsureWorkspaceInitialized(workspaceId);
            return true;
        }
    }
}
