using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Workspace.Api.Settings;
using model = Workspace.Api.Model;

namespace Workspace.Api.DataAccess
{
    public interface ICentralCommandDataAccess
    {
        Task<IEnumerable<model.Workspace>> GetWorkspacesAsync();
        Task<model.WorkspaceShardDto> GetShardByWorkspaceIdAsync(long id);
    }

    public class CentralCommandDataAccess : ICentralCommandDataAccess
    {
        private readonly ILogger<CentralCommandDataAccess> _logger;
        private readonly DBSettings _dBSettings;

        public CentralCommandDataAccess(ILogger<CentralCommandDataAccess> logger, DBSettings dBSettings)
        {
            _logger = logger;
            _dBSettings = dBSettings;
        }

        public async Task<IEnumerable<model.Workspace>> GetWorkspacesAsync()
        {
            IEnumerable<model.Workspace> result = new List<model.Workspace>();

            try
            {
                var query = "SELECT Id, Name FROM Workspace;";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryAsync<model.Workspace>(query);
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CentralCommandDataAccess)}.{nameof(GetWorkspacesAsync)}: " + ex.ToString());
                return result;
            }
        }

        public async Task<model.WorkspaceShardDto> GetShardByWorkspaceIdAsync(long workspaceId)
        {
            model.WorkspaceShardDto result = null;

            try
            {
                var query = @$"SELECT s.SqlServerName, s.DatabaseName, r.ApiUrl FROM WorkspaceShard s 
INNER JOIN Workspace w ON s.Id = w.WorkspaceShardId
INNER JOIN Region r ON s.RegionId = r.Id
WHERE w.Id = @workspaceId";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryFirstOrDefaultAsync<model.WorkspaceShardDto>(query, new { workspaceId = workspaceId });
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CentralCommandDataAccess)}.{nameof(CentralCommandDataAccess.GetShardByWorkspaceIdAsync)}: " + ex.ToString());
                return result;
            }
        }
    }
}
