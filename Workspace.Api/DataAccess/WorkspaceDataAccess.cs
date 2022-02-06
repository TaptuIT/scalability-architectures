using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Workspace.Api.Settings;
using Workspace.Api.Models;

namespace Workspace.Api.DataAccess
{
    public interface IWorkspaceDataAccess
    {
        /// <summary>
        /// Gets and sets shard infomration used to connect to the database
        /// </summary>
        /// <value></value>
        WorkspaceShardDto Shard { set; get; }

        /// <summary>
        /// Gets weather forecasts for the current shard
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <returns></returns>
        Task<IEnumerable<WeatherRecord>> GetWeatherforecasts(long workspaceId);

        /// <summary>
        /// Saves a weather forecast to the current shard
        /// </summary>
        /// <param name="weather"></param>
        /// <returns></returns>
        Task<WeatherRecord> SaveWeatherRecord(WeatherRecord weather);

        /// <summary>
        /// Ensures that a workspace record is created in the appropriate shard. 
        /// </summary>
        /// <param name="workspaceId"></param>
        Task EnsureWorkspaceInitialized(long workspaceId);
    }
    public class WorkspaceDataAccess : IWorkspaceDataAccess
    {
        private WorkspaceShardDto _shard;
        private readonly ILogger<WorkspaceDataAccess> _logger;
        private readonly DBSettings _dBSettings;

        public WorkspaceDataAccess(ILogger<WorkspaceDataAccess> logger, DBSettings dBSettings)
        {
            _logger = logger;
            _dBSettings = dBSettings;
        }

        public WorkspaceShardDto Shard { get => _shard; set => _shard = value; }

        public async Task<IEnumerable<WeatherRecord>> GetWeatherforecasts(long workspaceId)
        {
            if (_shard == null)
            {
                _logger.LogError("Connection string is not set!!");
                return null;
            }

            IEnumerable<WeatherRecord> result = new List<WeatherRecord>();

            try
            {
                var query = $"SELECT * FROM WeatherRecord WHERE WorkspaceId = @workspaceId";
                using (var conn = new SqlConnection(getConnectionString()))
                {
                    conn.Open();
                    result = await conn.QueryAsync<WeatherRecord>(query, new { workspaceId = workspaceId });
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(WorkspaceDataAccess)}.{nameof(GetWeatherforecasts)}: " + ex.ToString());
                return result;
            }
        }

        public async Task<WeatherRecord> SaveWeatherRecord(WeatherRecord weather)
        {
            WeatherRecord result = null;
            if (_shard == null)
            {
                _logger.LogError("Connection string is not set!!");
                return result;
            }

            try
            {
                var query = $"INSERT INTO WeatherRecord (WorkspaceId, Summary, Temperature, Date) VALUES (@WorkspaceId, @Summary, @Temperature, @Date); SELECT * FROM WeatherRecord WHERE Id = SCOPE_IDENTITY();";

                using (var conn = new SqlConnection(getConnectionString()))
                {
                    conn.Open();
                    result = await conn.QueryFirstOrDefaultAsync<WeatherRecord>(query, weather);
                    conn.Close();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(WorkspaceDataAccess)}.{nameof(GetWeatherforecasts)}: " + ex.ToString());
                return result;
            }
        }
        public async Task EnsureWorkspaceInitialized(long workspaceId)
        {
            if (_shard == null)
            {
                _logger.LogError("Connection string is not set!!");
                return;
            }

            try
            {
                var query = @$"
IF NOT EXISTS (SELECT * FROM Workspace WHERE Id= @workspaceId)
BEGIN 
    INSERT INTO Workspace (Id) VALUES (@workspaceId);
END;";

                using (var conn = new SqlConnection(getConnectionString()))
                {
                    conn.Open();
                    await conn.ExecuteAsync(query, new { workspaceId = workspaceId });
                    conn.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(WorkspaceDataAccess)}.{nameof(EnsureWorkspaceInitialized)}: " + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Builds a conection string based on the current shard information
        /// </summary>
        /// <returns>connection staring as a string</returns>
        private string getConnectionString()
            => new StringBuilder()
                    .Append("Server=")
                    .Append(_shard.SqlServerName)
                    .Append(";")
                    .Append("Database=")
                    .Append(_shard?.DatabaseName)
                    .Append(";")
                    .Append(_dBSettings.WorkspaceConnectionProperties)
                    .ToString();
    }
}
