﻿using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Workspace.Api.Settings;
using Workspace.Api.Models;

namespace Workspace.Api.DataAccess
{
    public interface ICentralCommandDataAccess
    {
        /// <summary>
        /// Retrieves a list of all workspaces
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Models.Workspace>> GetWorkspacesAsync();

        /// <summary>
        /// Retrieves shard information for a particular workspace. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<WorkspaceShardDto> GetShardByWorkspaceIdAsync(long id);
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

        public async Task<IEnumerable<Models.Workspace>> GetWorkspacesAsync()
        {
            IEnumerable<Models.Workspace> result = new List<Models.Workspace>();

            try
            {
                var query = "SELECT Id, Name FROM Workspace;";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryAsync<Models.Workspace>(query);
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

        public async Task<WorkspaceShardDto> GetShardByWorkspaceIdAsync(long workspaceId)
        {
            WorkspaceShardDto result = null;

            try
            {
                var query = @$"SELECT s.SqlServerName, s.DatabaseName, r.ApiUrl FROM WorkspaceShard s 
INNER JOIN Workspace w ON s.Id = w.WorkspaceShardId
INNER JOIN Region r ON s.RegionId = r.Id
WHERE w.Id = @workspaceId";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryFirstOrDefaultAsync<WorkspaceShardDto>(query, new { workspaceId = workspaceId });
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
