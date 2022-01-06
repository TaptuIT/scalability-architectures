using CentralCommand.Api.DataAccess;
using CentralCommand.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CentralCommand.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkspaceController : ControllerBase
    {
        private readonly ILogger<WorkspaceController> _logger;
        private readonly ICentralCommandDataAccess _centralCommandDataAccess;

        public WorkspaceController(ILogger<WorkspaceController> logger, ICentralCommandDataAccess centralCommandDataAccess)
        {
            _logger = logger;
            _centralCommandDataAccess = centralCommandDataAccess;
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetWorkspaces()
        {
            return Ok(await _centralCommandDataAccess.GetWorkspacesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkspaceDto request)
        {
            if (request == null) return BadRequest($"Invalid value of {nameof(request)} object");

            _logger.LogInformation("... determine where to store data.");
            
            var shard = await _centralCommandDataAccess.GetShardByRegionAsync(request.Region);
            if (shard == null) return BadRequest($"Invalid Region!");

            var workspace = new Workspace { Name = request.Name, WorkspaceShardId = shard.Id };
            var newWorkspace = await _centralCommandDataAccess.CreateWorkspaceAsync(workspace);
            
            _logger.LogInformation($"... Data created in Shard: {shard.DatabaseName}");

            return Ok(newWorkspace);
        }
    }
}
