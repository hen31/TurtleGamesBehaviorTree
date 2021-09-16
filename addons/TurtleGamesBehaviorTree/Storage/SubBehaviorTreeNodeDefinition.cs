using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class SubBehaviorTreeNodeDefinition : BehaviorTreeNodeDefinition
    {
        public SubBehaviorTreeNodeDefinition()
        {
        }

        public string SelectedSubTree { get; set; }
        public List<SubBehaviorTreeValueLink> ValueDefinitionValues { get; set; } = new List<SubBehaviorTreeValueLink>();
    }

    public class SubBehaviorTreeValueLink
    {
        public string Name { get; set; }
        public string LinkedTo { get; set; }
        public bool KeepInSync { get; set; }
    }
}
