using System;
using System.Collections.Generic;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public abstract class CompiledNode
    {

       
        public TreeExecutionState ExecutionState { get; private set; }
        public virtual void CompileFromDefinition(Storage.BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {

        }

        public virtual void Process(float delta)
        {
        }

        public CompiledNode PreviousNode { get; set; }

        public virtual void Initialize()
        {
            ExecutionState = TreeExecutionState.InProgress;
        }

        public void FailExecution()
        {
            ExecutionState = TreeExecutionState.Failed;
        }

        public void FinishExecution()
        {
            ExecutionState = TreeExecutionState.Completed;
        }
    }
}