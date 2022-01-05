using System;

namespace CommandQuery.Api.Middleware
{
    /// <summary>
    /// This attribute specifies that the endpoint will eventually write to the database.
    /// This allows the api to determine whether it can write the require request or whether it should forward
    /// it onto the primary Orchestration API.
    /// </summary>
    public class CommandAttribute : Attribute
    {
    }
}
