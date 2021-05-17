using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;
using TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class BehaviorTreeDefinition
    {
        public List<BehaviorTreeNodeDefinition> BehaviorTreeNodes { get; set; } = new List<BehaviorTreeNodeDefinition>();
        public List<BehaviorTreeConnectionDefinition> BehaviorTreeConnections { get; set; } = new List<BehaviorTreeConnectionDefinition>();
        public List<BehaviorTreeValueDefinitionStorage> ValueDefinitions { get; set; } = new List<BehaviorTreeValueDefinitionStorage>();

        public CompiledBehaviorTree Compile(Godot.Node subject)
        {
            CompiledBehaviorTree compiledBehaviorTree = new CompiledBehaviorTree();
            compiledBehaviorTree.SubjectOfTree = subject;
            foreach (var valueDefinition in ValueDefinitions)
            {
                compiledBehaviorTree.AddTreeValue(valueDefinition.Key, valueDefinition.DefaultValue, valueDefinition.ValueType);
            }

            var rootNode = BehaviorTreeNodes.Single(b => b is TreeRootNodeDefinition) as TreeRootNodeDefinition;
            compiledBehaviorTree.AddRootNode(CompileNode(rootNode, compiledBehaviorTree, null) as CompiledRootNode);
            return compiledBehaviorTree;
        }

        public CompiledNode CompileNode(BehaviorTreeNodeDefinition node, CompiledBehaviorTree compiledBehaviorTree, CompiledNode previousNode)
        {
            CompiledNode compiledNode = null;
            if (node is TreeRootNodeDefinition rootNode)
            {
                CompiledRootNode compiledRootNode = new CompiledRootNode();
                compiledRootNode.CompileFromDefinition(this, rootNode, compiledBehaviorTree);
                compiledNode = compiledRootNode;
            }
            else if (node is SelectorNodeDefinition selectorNodeDefinition)
            {
                CompiledSelectorNode compiledSequenceNode = new CompiledSelectorNode();
                compiledSequenceNode.CompileFromDefinition(this, selectorNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledSequenceNode;
            }
            else if (node is SequenceNodeDefinition sequenceNodeDefinition)
            {
                CompiledSequenceNode compiledSequenceNode = new CompiledSequenceNode();
                compiledSequenceNode.CompileFromDefinition(this, sequenceNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledSequenceNode;
            }
            else if (node is ActionNodeDefinition actionNodeDefinition)
            {
                CompiledActionNode compiledActionNode = new CompiledActionNode();
                compiledActionNode.CompileFromDefinition(this, actionNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledActionNode;
            }
            else if (node is ServiceNodeDefinition serviceNodeDefinition)
            {
                CompiledServiceNode compiledServiceNode = new CompiledServiceNode();
                compiledServiceNode.CompileFromDefinition(this, serviceNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledServiceNode;
            }
            else if (node is ConditionNodeDefinition conditionNodeDefinition)
            {
                CompiledGateNode compiledActionNode = new CompiledGateNode();
                compiledActionNode.CompileFromDefinition(this, conditionNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledActionNode;
            }
            var nodeBefore = previousNode;
            while (nodeBefore is CompiledServiceNode || nodeBefore is CompiledGateNode)
            {
                nodeBefore = nodeBefore.PreviousNode;
            }
            compiledNode.PreviousNode = nodeBefore;
            return compiledNode;
        }

        public BehaviorTreeNodeDefinition[] GetOutgoingConnections(BehaviorTreeNodeDefinition definition)
        {
            return BehaviorTreeConnections.Where(b => b.FromNode == definition.NodeName).OrderBy(b => b.FromSlot).Select(b => BehaviorTreeNodes.Single(c => c.NodeName == b.ToNode)).ToArray();

        }
    }
}
