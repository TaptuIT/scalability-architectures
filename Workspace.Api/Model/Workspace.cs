﻿namespace Workspace.Api.Model
{
    public class Workspace
    {
        public long Id { get; set; }
        internal long WorkspaceShardId { get; set; }
        public string Name { get; set; }
    }
}
