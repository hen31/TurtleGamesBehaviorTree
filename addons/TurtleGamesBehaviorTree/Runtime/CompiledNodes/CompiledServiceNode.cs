using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledServiceNode : CompiledNode
    {
        private BaseTreeService _serviceToExecute;
        private CompiledNode _subNode;
        private double _executionTimer = 0f;
        private double _sinceLastTime = 0f;
        public override void CompileFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {

            var serviceNodeDefinition = currentNode as ServiceNodeDefinition;
            var serviceType = Type.GetType(BehaviorTreeRegistry.Instance.TreeServices.Single(b => b.Name == serviceNodeDefinition.ServiceName).FullName);
            _serviceToExecute = Activator.CreateInstance(serviceType) as BaseTreeService;
            _serviceToExecute.SubjectOfTree = compiledBehaviorTree.SubjectOfTree;
            foreach (var parameterName in serviceNodeDefinition.ParameterValues.Keys)
            {
                CompileBehaviorTreeUtils.SetParameterValueInProperty(_serviceToExecute, compiledBehaviorTree, serviceType, parameterName, serviceNodeDefinition.ParameterValues[parameterName]);
            }
            _executionTimer = serviceNodeDefinition.ExecutionTimer;
            var subNode = behaviorTreeDefinition.GetOutgoingConnections(currentNode).FirstOrDefault();
            _subNode = behaviorTreeDefinition.CompileNode(subNode, compiledBehaviorTree, this);
        }

        public override void Initialize()
        {
            base.Initialize();
            _serviceToExecute.InitializeService();
            _sinceLastTime = 0f;
            _subNode.Initialize();
        }

        public override void Process(float delta)
        {
            _sinceLastTime += delta;
            if(_sinceLastTime >= _executionTimer)
            {
                double time = _sinceLastTime - _executionTimer;
                _serviceToExecute.DoService((float)_sinceLastTime - (float)time);
                _sinceLastTime = time;
            }
            _subNode.Process(delta);
            if (_subNode.ExecutionState == TreeExecutionState.Failed)
            {
                FailExecution();
            }
            else if (_subNode.ExecutionState == TreeExecutionState.Completed)
            {
                FinishExecution();
            }
        }
    }
}