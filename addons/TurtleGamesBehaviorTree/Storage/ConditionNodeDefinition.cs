using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public enum AbortOption { AbortSelf, AbortNodesAfter, Both }
    public class ConditionNodeDefinition : BehaviorTreeNodeDefinition
    {
        public ConditionNodeDefinition()
        {
        }

        public List<ConditionStorage> Conditions { get; set; } = new List<ConditionStorage>();
        public bool CheckContinuously { get; set; }
        public AbortOption AbortOption { get; set; }
    }
}
