using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class SequenceNodeDefinition : BehaviorTreeNodeDefinition
    {
        public SequenceNodeDefinition()
        {
        }

        public int SlotCount { get; set; }
        public bool Selector { get; set; }
    }
}
