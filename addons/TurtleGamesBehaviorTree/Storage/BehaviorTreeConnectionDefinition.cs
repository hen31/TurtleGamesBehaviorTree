using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class BehaviorTreeConnectionDefinition
    {
        public string FromNode { get; set; }
        public string ToNode { get; set; }
        public int FromSlot { get; set; }
        public int ToSlot { get; set; }
    }
}
