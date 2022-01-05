using System;

namespace CommandQuery.Api.Middleware
{
    /// <summary>
    /// This attribute (currently) does nothing but should be added to endpoints that don't write to the database.
    /// There is a goal to make sure that either this or the [Command] attribute are added to an endpoint to ensure
    /// the intent of the endpoint is explicit within the codebase.
    /// </summary>
    public class QueryAttribute : Attribute
    {
    }
}
