using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public abstract class BehaviorTreeNode<T> : BehaviorTreeNode where T : BehaviorTreeNodeDefinition, new()
    {

        public virtual T GetTypedDefinitionFromNode()
        {
            T behaviorTreeNodeDefinition = new T();
            behaviorTreeNodeDefinition.NodeName = Name;
            behaviorTreeNodeDefinition.Location = RectPosition;
            return behaviorTreeNodeDefinition;
        }

        public sealed override BehaviorTreeNodeDefinition GetDefinitionFromNode()
        {
            return GetTypedDefinitionFromNode();
        }
    }

    public abstract class BehaviorTreeNode : GraphNode
    {
        public override void _Ready()
        {
            base._Ready();
            Connect("resize_request", this, nameof(ResizeRequest));

        }

        private void ResizeRequest(Vector2 new_minsize)
        {
            RectSize = new_minsize;
        }
        public virtual bool CanDelete()
        {
            return true;
        }
        public abstract BehaviorTreeNodeDefinition GetDefinitionFromNode();

    }
}
