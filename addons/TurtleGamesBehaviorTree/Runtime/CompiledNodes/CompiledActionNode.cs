﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledActionNode : CompiledNode
    {
        private BaseTreeAction _actionToExecute;

        public override void CompileFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {

            var actionNodeDefinition = currentNode as ActionNodeDefinition;
            var actionType = Type.GetType(BehaviorTreeRegistry.Instance.TreeActions.Single(b => b.Name == actionNodeDefinition.ActionName).FullName);
            _actionToExecute = Activator.CreateInstance(actionType) as BaseTreeAction;
            _actionToExecute.SubjectOfTree = compiledBehaviorTree.SubjectOfTree;
            foreach (var parameterName in actionNodeDefinition.ParameterValues.Keys)
            {
                CompileBehaviorTreeUtils.SetParameterValueInProperty(_actionToExecute, compiledBehaviorTree, actionType, parameterName, actionNodeDefinition.ParameterValues[parameterName]);
            }
        }

       

        public override void Initialize()
        {
            base.Initialize();
            _actionToExecute.InitializeAction();
        }

        public override void Process(float delta)
        {
            _actionToExecute.DoAction(delta);
            if (_actionToExecute.ActionState == ActionState.Failed)
            {
                FailExecution();
            }
            else if (_actionToExecute.ActionState == ActionState.Succeeded)
            {
                FinishExecution();
            }
        }
    }
}