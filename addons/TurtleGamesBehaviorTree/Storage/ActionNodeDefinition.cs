using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class ActionNodeDefinition : BehaviorTreeNodeDefinition
    {
        public ActionNodeDefinition()
        {
        }

        public string ActionName { get; set; }
        public Dictionary<string, object> ParameterValues { get; set; } = new Dictionary<string, object>();
    }
}
