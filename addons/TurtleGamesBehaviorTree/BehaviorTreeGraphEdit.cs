using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class BehaviorTreeGraphEdit : GraphEdit
    {
        // Declare member variables here. Examples:
        public List<Node> CurrentSelection { get; set; } = new List<Node>();
        private List<BehaviorTreeValueDefinition> _valueDefinitions = new List<BehaviorTreeValueDefinition>();

        [Signal]
        public delegate void ValueDefinitionsChanged();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Connect("connection_request", this, nameof(ConnectionRequest));
            Connect("disconnection_request", this, nameof(DisconnectionRequest));
            Connect("connection_to_empty", this, nameof(ConnectionToEmpty));
            Connect("node_selected", this, nameof(NodeSelected));
            Connect("node_unselected", this, nameof(NodeUnselected));

        }

        private void NodeUnselected(Node node)
        {
            Debug.WriteLine($"Node unselected {node.Name}");
            CurrentSelection.Remove(node);
        }

        private void NodeSelected(Node node)
        {
            if (!CurrentSelection.Contains(node))
            {
                Debug.WriteLine($"Node selected {node.Name}");
                CurrentSelection.Add(node);
            }
        }

        public ReadOnlyCollection<BehaviorTreeValueDefinition> GetBehaviorTreeValueDefinitions()
        {
            return new ReadOnlyCollection<BehaviorTreeValueDefinition>(_valueDefinitions);
        }

        public void AddValueDefinition(BehaviorTreeValueDefinition behaviorTreeValue)
        {
            _valueDefinitions.Add(behaviorTreeValue);
            EmitSignal(nameof(ValueDefinitionsChanged));
        }

        public void RemoveValueDefinition(BehaviorTreeValueDefinition behaviorTreeValue)
        {
            _valueDefinitions.Remove(behaviorTreeValue);
            EmitSignal(nameof(ValueDefinitionsChanged));
        }

        private void ConnectionToEmpty(string from, int from_slot, Vector2 release_position)
        {
            RemoveConnectionsFromNodeAndSlot(from, from_slot);
        }

        public void ConnectionRequest(string from, int from_slot, string to, int to_slot)
        {
            base.ConnectNode(from, from_slot, to, to_slot);
        }

        public void DisconnectionRequest(string from, int from_slot, string to, int to_slot)
        {
            base.DisconnectNode(from, from_slot, to, to_slot);
        }
        internal void RemoveConnectionsFromNodeAndSlot(string name, int slot)
        {
            Debug.WriteLine($"remove connection of {name} {slot}");
            foreach (Godot.Collections.Dictionary connection in GetConnectionList())
            {
                Debug.WriteLine($"connection is {connection["from"].ToString()} {(int)connection["from_port"]}  to {connection["to"].ToString()} {(int)connection["to_port"]}");

                if (connection["from"].ToString().Equals(name))
                {
                    var fromPort = (int)connection["from_port"];
                    if (fromPort == slot)
                    {
                        var toPort = (int)connection["to_port"];
                        DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);
                    }
                }
              /*  else if (connection["to"].ToString().Equals(name))
                {
                    var toPort = (int)connection["to_port"];
                    if (toPort == slot)
                    {
                        var fromPort = (int)connection["from_port"];
                        DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);
                    }
                }*/
            }
        }

        internal void RemoveConnectionsToNodeAndSlot(string name, int slot)
        {
            Debug.WriteLine($"remove connection of {name} {slot}");
            foreach (Godot.Collections.Dictionary connection in GetConnectionList())
            {
                Debug.WriteLine($"connection is {connection["from"].ToString()} {(int)connection["from_port"]}  to {connection["to"].ToString()} {(int)connection["to_port"]}");

                if (connection["from"].ToString().Equals(name))
                {
                    var fromPort = (int)connection["from_port"];
                    if (fromPort == slot)
                    {
                        var toPort = (int)connection["to_port"];
                        DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);
                    }
                }
                else if (connection["to"].ToString().Equals(name))
                {
                    var toPort = (int)connection["to_port"];
                    if (toPort == slot)
                    {
                        var fromPort = (int)connection["from_port"];
                        DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);
                    }
                }
            }
        }

        internal void InitializeFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition)
        {
            ClearGraphEdit();
            Debug.WriteLine($"Amount of nodes {behaviorTreeDefinition.BehaviorTreeNodes.Count}");
            foreach (var behaviorTreeItem in behaviorTreeDefinition.BehaviorTreeNodes)
            {
                if (behaviorTreeItem is TreeRootNodeDefinition treeRootNodeDefinition)
                {
                    TreeRootNode treeRootNode = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/TreeRootNode.tscn").Instance() as TreeRootNode;
                    treeRootNode.Name = treeRootNodeDefinition.NodeName;
                    treeRootNode.Offset = treeRootNodeDefinition.Location;
                    AddChild(treeRootNode);
                }
                else if (behaviorTreeItem is SequenceNodeDefinition sequenceNodeDefinition)
                {
                    SequenceNode sequenceNode = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/SequenceNode.tscn").Instance() as SequenceNode;
                    sequenceNode.Name = sequenceNodeDefinition.NodeName;
                    sequenceNode.InitializeSlots(sequenceNodeDefinition.SlotCount);
                    sequenceNode.Offset = sequenceNodeDefinition.Location;
                    AddChild(sequenceNode);
                }
                else if (behaviorTreeItem is ServiceNodeDefinition serviceNodeDefinition)
                {
                    ServiceNode serviceNode = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/ServiceNode.tscn").Instance() as ServiceNode;
                    serviceNode.Name = serviceNodeDefinition.NodeName;
                    serviceNode.CallDeferred(nameof(serviceNode.InitializeWithServiceName), serviceNodeDefinition.ServiceName, new GodotWrapper(serviceNodeDefinition.ParameterValues), serviceNodeDefinition.ExecutionTimer);
                    serviceNode.Offset = serviceNodeDefinition.Location;
                    AddChild(serviceNode);
                }
                else if (behaviorTreeItem is ActionNodeDefinition actionNodeDefinition)
                {
                    ActionNode actionNode = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/ActionNode.tscn").Instance() as ActionNode;
                    actionNode.Name = actionNodeDefinition.NodeName;
                    actionNode.CallDeferred(nameof(actionNode.InitializeWithActionName), actionNodeDefinition.ActionName, new GodotWrapper(actionNodeDefinition.ParameterValues));
                    actionNode.Offset = actionNodeDefinition.Location;
                    AddChild(actionNode);
                }
                else if (behaviorTreeItem is ConditionNodeDefinition conditionNodeDefinition)
                {
                    GateNode gateNode = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/GateNode.tscn").Instance() as GateNode;
                    gateNode.Name = conditionNodeDefinition.NodeName;
                    gateNode.Offset = conditionNodeDefinition.Location;
                    gateNode.CallDeferred(nameof(gateNode.InitializeFromStorage), new GodotWrapper(conditionNodeDefinition));

                    AddChild(gateNode);
                }

            }

            foreach (var connectionDefinition in behaviorTreeDefinition.BehaviorTreeConnections)
            {
                ConnectNode(connectionDefinition.FromNode, connectionDefinition.FromSlot, connectionDefinition.ToNode, connectionDefinition.ToSlot);
            }

        }

        internal void DeleteGraphNode(Node node)
        {
            Debug.Write("Remove graph node");
            if (node is BehaviorTreeNode behaviorTreeNode)
            {
                CurrentSelection.Remove(behaviorTreeNode);
                RemoveGraphNodeConnections(behaviorTreeNode);
                behaviorTreeNode.QueueFree();
            }
        }

        internal void NameOfValueDefinitionHasChanged(BehaviorTreeValueDefinition behaviorTreeValueDefinition)
        {
            EmitSignal(nameof(ValueDefinitionsChanged));
        }

        public void RemoveGraphNodeConnections(BehaviorTreeNode behaviorTreeNode)
        {
            foreach (Godot.Collections.Dictionary connection in GetConnectionList())
            {
                Debug.WriteLine($"connection is {connection["from"].ToString()} {(int)connection["from_port"]}  to {connection["to"].ToString()} {(int)connection["to_port"]}");

                if (connection["from"].ToString().Equals(behaviorTreeNode.Name))
                {
                    var fromPort = (int)connection["from_port"];
                    var toPort = (int)connection["to_port"];
                    DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);
                }
                else if (connection["to"].ToString().Equals(behaviorTreeNode.Name))
                {
                    var toPort = (int)connection["to_port"];
                    var fromPort = (int)connection["from_port"];
                    DisconnectionRequest(connection["from"].ToString(), fromPort, connection["to"].ToString(), toPort);

                }
            }
        }

        public BehaviorTreeDefinition GetTreeDefinition()
        {
            BehaviorTreeDefinition behaviorTreeDefinition = new BehaviorTreeDefinition();
            foreach (Node child in GetChildren())
            {
                if (child is BehaviorTreeNode behaviorTreeNode)
                {
                    behaviorTreeDefinition.BehaviorTreeNodes.Add(behaviorTreeNode.GetDefinitionFromNode());
                }
            }

            foreach (Godot.Collections.Dictionary connection in GetConnectionList())
            {
                BehaviorTreeConnectionDefinition behaviorTreeConnection = new BehaviorTreeConnectionDefinition();
                behaviorTreeConnection.FromNode = connection["from"].ToString();
                behaviorTreeConnection.FromSlot = (int)connection["from_port"];
                behaviorTreeConnection.ToNode = connection["to"].ToString();
                behaviorTreeConnection.ToSlot = (int)connection["to_port"];
                behaviorTreeDefinition.BehaviorTreeConnections.Add(behaviorTreeConnection);
            }

            behaviorTreeDefinition.ValueDefinitions = _valueDefinitions.Select(b => new BehaviorTreeValueDefinitionStorage(b)).ToList();

            return behaviorTreeDefinition;
        }

        internal void ClearGraphEdit()
        {
            if (GetChildCount() > 0)
            {
                ClearConnections();
                foreach (Node node in GetChildren())
                {
                    if (node is BehaviorTreeNode)
                    {
                        node.QueueFree();
                    }
                }
            }
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}
