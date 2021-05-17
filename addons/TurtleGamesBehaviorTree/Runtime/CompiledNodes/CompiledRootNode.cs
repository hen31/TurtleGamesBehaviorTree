using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledRootNode : CompiledNode
    {
        private CompiledNode _subNode;
        private bool _reintialize = true;
        public override void CompileFromDefinition(Storage.BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {
            var subNode = behaviorTreeDefinition.GetOutgoingConnections(currentNode).FirstOrDefault();
            _subNode = behaviorTreeDefinition.CompileNode(subNode, compiledBehaviorTree, this);
        }

        public override void Process(float delta)
        {
            if (_reintialize)
            {
                _subNode.Initialize();
                _reintialize = false;
            }
            _subNode.Process(delta);
            if (_subNode.ExecutionState != TreeExecutionState.InProgress)
            {
                _reintialize = true;
            }


        }
    }
}
