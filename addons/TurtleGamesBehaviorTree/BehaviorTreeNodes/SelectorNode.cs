using Godot;
using System;
using System.Diagnostics;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class SelectorNode : SequenceNode
    {
        public override SequenceNodeDefinition GetTypedDefinitionFromNode()
        {
            SequenceNodeDefinition sequenceNodeDefinition = base.GetTypedDefinitionFromNode();
            sequenceNodeDefinition.Selector =true;
            return sequenceNodeDefinition;
        }
    }
}