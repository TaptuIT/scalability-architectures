using CentralCommand.Api.Models;
using CentralCommand.Api.Settings;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CentralCommand.Api.DataAccess
{
    public interface ICentralCommandDataAccess
    {
        Task<IEnumerable<Workspace>> GetWorkspacesAsync();
        Task<Workspace> CreateWorkspaceAsync(Workspace workspace);
        Task<WorkspaceShard> GetShardByRegionAsync(string region);
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

        public async Task<IEnumerable<Workspace>> GetWorkspacesAsync()
        {
            IEnumerable<Workspace> result = new List<Workspace>();

            try
            {
                var query = "SELECT Id, Name FROM Workspace;";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryAsync<Workspace>(query);
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CentralCommandDataAccess)}.{nameof(CentralCommandDataAccess.GetWorkspacesAsync)}: " + ex.ToString());
                return result;
            }
        }

        public async Task<Workspace> CreateWorkspaceAsync(Workspace workspace)
        {
            Workspace result = null;

            try
            {
                var query = "INSERT INTO Workspace (WorkspaceShardId, Name) Values(@WorkspaceShardId, @Name); SELECT * FROM Workspace WHERE Id = SCOPE_IDENTITY();";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryFirstOrDefaultAsync<Workspace>(query, workspace);
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CentralCommandDataAccess)}.{nameof(CentralCommandDataAccess.CreateWorkspaceAsync)}: " + ex.ToString());
                return result;
            }
        }

        public async Task<WorkspaceShard> GetShardByRegionAsync(string region)
        {
            WorkspaceShard result = null;

            try
            {
                var query = $"SELECT s.* FROM Region r INNER JOIN WorkspaceShard s ON r.Id = s.RegionId WHERE LOWER(r.Name) = LOWER(@region) AND IsDefaultForRegion = 1";
                using (var conn = new SqlConnection(_dBSettings.CentralCommandConnectionString))
                {
                    conn.Open();
                    result = await conn.QueryFirstOrDefaultAsync<WorkspaceShard>(query, new { region = region });
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CentralCommandDataAccess)}.{nameof(CentralCommandDataAccess.GetShardByRegionAsync)}: " + ex.ToString());
                return result;
            }
        }
    }
}
