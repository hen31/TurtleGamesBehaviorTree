using Godot;
using System;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class BehaviourTreePlugin : EditorPlugin
    {

        Control mainPanelInstance;
        // Called when the node enters the scene tree for the first time.
        public override void _EnterTree()
        {
            mainPanelInstance = (Control)GD.Load<PackedScene>("addons/TurtleGamesBehaviorTree/BehaviourTreeDock.tscn").Instance();
            GetEditorInterface().GetEditorViewport().AddChild(mainPanelInstance);
            MakeVisible(false);

            var behaviorTreePlayerScript = GD.Load<Script>("res://addons/TurtleGamesBehaviorTree/Nodes/BehaviorTreePlayer.cs");
            AddCustomType("BehaviorTreePlayer", "Node", behaviorTreePlayerScript, GetPluginIcon());
            var characterMovementScript = GD.Load<Script>("res://addons/TurtleGamesBehaviorTree/Nodes/CharacterMovementNode.cs");
            AddCustomType("CharacterMovement", "Spatial", characterMovementScript, GetPluginIcon());
        }

        public override void MakeVisible(bool visible)
        {
            if (mainPanelInstance != null)
            {
                mainPanelInstance.Visible = visible;
            }
        }

        public override string GetPluginName()
        {
            return "Behavior tree";
        }

        public override bool HasMainScreen()
        {
            return true;
        }

        public override void _ExitTree()
        {
            if (mainPanelInstance != null)
            {
                mainPanelInstance.QueueFree();
            }

        }
        public override Texture GetPluginIcon()
        {
            return GetEditorInterface().GetBaseControl().GetIcon("Node", "EditorIcons");
        }
        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}