using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class ServiceNodeDefinition : BehaviorTreeNodeDefinition
    {
        public ServiceNodeDefinition()
        {
        }

        public string ServiceName { get; set; }
        public Dictionary<string, object> ParameterValues { get; set; } = new Dictionary<string, object>();
        public double ExecutionTimer { get;  set; }
        public bool AlwaysExecute { get; set; }
    }
}
