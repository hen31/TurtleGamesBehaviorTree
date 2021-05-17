using Godot;
using System;
using System.Diagnostics;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class SequenceNode : BehaviorTreeNode<SequenceNodeDefinition>
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        int _lastSlot = 0;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SetSlot(0, true, 1, Colors.White, false, 0, Colors.White);
            GetNode<Button>("VBoxContainer/AddButton").Connect("pressed", this, nameof(AddSlotClicked));
            GetNode<Button>("VBoxContainer/RemoveButton").Connect("pressed", this, nameof(RemoveSlotClicked));
            if (_lastSlot == 0)
            {
                AddSlotClicked();
            }
        }

        public void AddSlotClicked()
        {
            _lastSlot = _lastSlot + 1;
            AddChild(new Label() { Text = (_lastSlot).ToString() });
            SetSlot(_lastSlot, false, 0, Colors.White, true, 1, Colors.White);
            Debug.WriteLine($"Add slot ({_lastSlot}) ({GetChildCount() - 1})");
        }

        public void RemoveSlotClicked()
        {
            if (_lastSlot != 1)
            {
                var treeGraphEdit = GetParent<BehaviorTreeGraphEdit>();
                treeGraphEdit.RemoveConnectionsToNodeAndSlot(Name, _lastSlot-1);
                GetChild(_lastSlot).QueueFree();
                _lastSlot--;
            }
        }

        public override SequenceNodeDefinition GetTypedDefinitionFromNode()
        {
            SequenceNodeDefinition sequenceNodeDefinition =  base.GetTypedDefinitionFromNode();
            sequenceNodeDefinition.SlotCount = _lastSlot;
            return sequenceNodeDefinition;
        }

        internal void InitializeSlots(int slotCount)
        {
            while(_lastSlot != slotCount)
            {
                AddSlotClicked();
            }
        }
    }
}