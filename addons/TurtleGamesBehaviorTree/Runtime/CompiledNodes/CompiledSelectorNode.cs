﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledSelectorNode : CompiledMultipleOutConnectionsNode
    {
        private CompiledNode[] _subNodes;

        private CompiledNode _currentlyExecuting;
        private int _currentIndex = 0;
        private bool _aborting = false;
        private BehaviorTreeNodeDefinition _definition;

        public override void CompileFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {
            _definition = currentNode;
            var outgoingConnections = behaviorTreeDefinition.GetOutgoingConnections(currentNode);
            _subNodes = outgoingConnections.Select(b => behaviorTreeDefinition.CompileNode(b, compiledBehaviorTree, this)).ToArray();
        }

        public override void Initialize()
        {
            base.Initialize();
            _aborting = false;
            _currentIndex = 0;
            if (_subNodes.Length > 0)
            {
                SetCurrentlyExecuting();
            }
        }

        public override void AbortAfterCurrent()
        {
            _aborting = true;
        }
        public override void Process(float delta)
        {
            if (_currentlyExecuting != null)
            {
                _currentlyExecuting.Process(delta);
                while (_currentlyExecuting.ExecutionState == TreeExecutionState.Failed)
                {
                    if (_currentIndex == _subNodes.Length - 1 || _aborting)
                    {
                        FailExecution();
                        break;
                    }
                    else
                    {
                        _currentIndex++;
                        SetCurrentlyExecuting();
                        _currentlyExecuting.Process(delta);
                    }
                }

                if (_currentlyExecuting.ExecutionState == TreeExecutionState.Completed)
                {
                    FinishExecution();
                }
            }
            else
            {
                FailExecution();
            }
        }

        private void SetCurrentlyExecuting()
        {
            _currentlyExecuting = _subNodes[_currentIndex];
            _currentlyExecuting.Initialize();
        }
    }
}
