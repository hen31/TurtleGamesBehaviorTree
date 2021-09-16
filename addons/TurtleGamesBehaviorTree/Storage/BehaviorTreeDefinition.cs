using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Runtime;
using TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class BehaviorTreeDefinition
    {
        public List<BehaviorTreeNodeDefinition> BehaviorTreeNodes { get; set; } = new List<BehaviorTreeNodeDefinition>();
        public List<BehaviorTreeConnectionDefinition> BehaviorTreeConnections { get; set; } = new List<BehaviorTreeConnectionDefinition>();
        public List<BehaviorTreeValueDefinitionStorage> ValueDefinitions { get; set; } = new List<BehaviorTreeValueDefinitionStorage>();

        public static BehaviorTreeDefinition LoadBehaviorTreeFromFile(string behaviorTreeFile)
        {
            Godot.File file = new File();
            if (file.Open(behaviorTreeFile, File.ModeFlags.Read) == Error.Ok)
            {
                string json = file.GetAsText();
                file.Close();
                var jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
                return JsonConvert.DeserializeObject<BehaviorTreeDefinition>(json, jsonSerializerSettings);
            }
            return null;
        }

        public CompiledBehaviorTree Compile(Godot.Node subject, BehaviorTreePlayer behaviorTreePlayer)
        {
            CompiledBehaviorTree compiledBehaviorTree = new CompiledBehaviorTree();
            compiledBehaviorTree.CurrentPlayer = behaviorTreePlayer;
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
            else if (node is SequenceNodeDefinition selectorNodeDefinition && selectorNodeDefinition.Selector)
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
            else if (node is SubBehaviorTreeNodeDefinition subBehaviorTreeNodeDefinition)
            {
                CompiledSubTreeNode compiledSubTreeNode = new CompiledSubTreeNode();
                compiledSubTreeNode.CompileFromDefinition(this, subBehaviorTreeNodeDefinition, compiledBehaviorTree);
                compiledNode = compiledSubTreeNode;
            }
            if (node == null)
            {
                System.Diagnostics.Debug.WriteLine($"node is not existing {previousNode.GetType().FullName}");
                return null;
            }
            if (compiledNode == null)
            {
                System.Diagnostics.Debug.WriteLine($"node not found {node.GetType().FullName}");
                return null;
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
