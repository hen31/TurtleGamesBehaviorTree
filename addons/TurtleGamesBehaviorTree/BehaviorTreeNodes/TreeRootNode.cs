using Godot;
using System;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class TreeRootNode : BehaviorTreeNode<TreeRootNodeDefinition>
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SetSlot(0, false, 0, Colors.White, true, 1, Colors.White);
        }

        public override bool CanDelete()
        {
            return false;
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}