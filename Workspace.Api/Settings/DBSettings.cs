using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspace.Api.Settings
{
    public class DBSettings
    {
        public readonly static string Name = "DBSettings";
        public string CentralCommandConnectionString { get; set; }
        public string WorkspaceConnectionProperties { get; set; }
    }
}
