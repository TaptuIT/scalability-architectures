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
            // VALIDATE request.
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
            // VALIDATE request
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
        /// This method replaces Handler service/object.
        /// </summary>
        private async Task<bool> HandleShard(long workspaceId)
        {
            string thisApiUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;

            // RETRIEVE Shard
            _logger.LogInformation($"... Get Shard info for Workspace: {workspaceId}");
            var shard = await _centralCommandDataAccess.GetShardByWorkspaceIdAsync(workspaceId);
            if (shard == null) return false;

            // Validate Region
            if (thisApiUrl.Equals(shard.ApiUrl, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            _logger.LogInformation($"... Use Shard info and SAVE data from: {shard.DatabaseName} Database.");
            _workspaceDataAccess.Shard = shard;

            // INIT Shard
            await _workspaceDataAccess.EnsureWorkspaceInitialized(workspaceId);
            return true;
        }
    }
}
