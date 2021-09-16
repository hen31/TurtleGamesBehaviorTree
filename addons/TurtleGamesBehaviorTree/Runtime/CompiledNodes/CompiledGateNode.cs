using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledGateNode : CompiledNode
    {
        List<CompiledCondition> _conditions = new List<CompiledCondition>();
        private CompiledNode _subNode;
        private bool _checkContinuously;
        private bool _checkedInitial = false;
        private AbortOption _abortOption;

        public override void CompileFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {
            var conditionNodeDefinition = currentNode as ConditionNodeDefinition;
            foreach (var condition in conditionNodeDefinition.Conditions)
            {
                CompileCondition(condition, compiledBehaviorTree);
            }
            var subNode = behaviorTreeDefinition.GetOutgoingConnections(currentNode).FirstOrDefault();
            _subNode = behaviorTreeDefinition.CompileNode(subNode, compiledBehaviorTree, this);
            _checkContinuously = conditionNodeDefinition.CheckContinuously;
            _abortOption = conditionNodeDefinition.AbortOption;
        }

        private void CompileCondition(ConditionStorage condition, CompiledBehaviorTree compiledBehaviorTree)
        {
            CompiledCondition compiledCondition = new CompiledCondition();
            compiledCondition.Operator = condition.ConditionOperator;
            if (condition.Values?.Count > 0)
            {
                compiledCondition.Value = condition.Values[0];
            }
            compiledCondition.TreeValueDefinition = new ValueDefinitionKey(condition.Key, compiledBehaviorTree);
            _conditions.Add(compiledCondition);
        }
        public override void Initialize()
        {
            _checkedInitial = false;
            base.Initialize();
            _subNode?.Initialize();
        }

        public override void Process(float delta)
        {
            if (_checkContinuously)
            {
                CheckConditions();

            }
            else if (!_checkedInitial)
            {
                CheckConditions();
                _checkedInitial = true;
            }
            if (ExecutionState != TreeExecutionState.Failed)
            {
                _subNode?.Process(delta);
            }
            if (_subNode.ExecutionState == TreeExecutionState.Completed)
            {
                FinishExecution();
            }
            else if (_subNode.ExecutionState == TreeExecutionState.Failed)
            {
                FailExecution();
            }
        }

        private void CheckConditions()
        {
            foreach (var condition in _conditions)
            {
                if (!CheckCondition(condition))
                {
                    if (_abortOption == AbortOption.AbortSelf || _abortOption == AbortOption.Both)
                    {
                        FailExecution();
                    }
                    if (_abortOption == AbortOption.AbortNodesAfter || _abortOption == AbortOption.Both)
                    {
                        if (PreviousNode is CompiledMultipleOutConnectionsNode multipleOutConnectionsNode)
                        {
                            multipleOutConnectionsNode.AbortAfterCurrent();
                        }
                    }
                }
            }
        }

        private bool CheckCondition(CompiledCondition condition)
        {
            var valueFromTreeValue = condition.TreeValueDefinition.GetValue();
            switch (condition.Operator)
            {
                case ConditionOperator.IsSet:
                    if (valueFromTreeValue is bool valueAsBool)
                    {
                        return valueAsBool == true;
                    }
                    else
                    {
                        return condition.TreeValueDefinition.GetValue() != null;
                    }
                case ConditionOperator.IsNotSet:
                    if (valueFromTreeValue is bool treeValueAsBool)
                    {
                        return treeValueAsBool != true;
                    }
                    else
                    {
                        return condition.TreeValueDefinition.GetValue() != null;
                    }
                case ConditionOperator.Equal:
                    return valueFromTreeValue.Equals(condition.Value);
                case ConditionOperator.NotEqual:
                    return !valueFromTreeValue.Equals(condition.Value);
                case ConditionOperator.MoreThen:
                case ConditionOperator.LessThen:
                case ConditionOperator.EqualOrLessThen:
                case ConditionOperator.EqualOrMoreThen:
                    if (valueFromTreeValue is float floatValue)
                    {
                        var valueAsFloat = (float)condition.Value;
                        return (condition.Operator == ConditionOperator.LessThen && floatValue < valueAsFloat)
                            || (condition.Operator == ConditionOperator.EqualOrLessThen && floatValue <= valueAsFloat)
                            || (condition.Operator == ConditionOperator.MoreThen && floatValue > valueAsFloat)
                            || (condition.Operator == ConditionOperator.EqualOrMoreThen && floatValue >= valueAsFloat);
                    }
                    else if (valueFromTreeValue is double doubleValue)
                    {
                        var valueAsDouble = (double)condition.Value;
                        return (condition.Operator == ConditionOperator.LessThen && doubleValue < valueAsDouble)
                            || (condition.Operator == ConditionOperator.EqualOrLessThen && doubleValue <= valueAsDouble)
                            || (condition.Operator == ConditionOperator.MoreThen && doubleValue > valueAsDouble)
                            || (condition.Operator == ConditionOperator.EqualOrMoreThen && doubleValue >= valueAsDouble);
                    }
                    else if (valueFromTreeValue is int intValue)
                    {
                        var valueAsInt = (int)condition.Value;
                        return (condition.Operator == ConditionOperator.LessThen && intValue < valueAsInt)
                            || (condition.Operator == ConditionOperator.EqualOrLessThen && intValue <= valueAsInt)
                            || (condition.Operator == ConditionOperator.MoreThen && intValue > valueAsInt)
                            || (condition.Operator == ConditionOperator.EqualOrMoreThen && intValue >= valueAsInt);
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }
    }

    public class CompiledCondition
    {
        public ConditionOperator Operator { get; set; }
        public object Value { get; set; }
        public ValueDefinitionKey TreeValueDefinition { get; set; }
    }

}
