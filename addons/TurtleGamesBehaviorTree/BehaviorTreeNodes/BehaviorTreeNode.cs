using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public abstract class BehaviorTreeNode<T> : BehaviorTreeNode where T : BehaviorTreeNodeDefinition, new()
    {

        public virtual T GetTypedDefinitionFromNode()
        {
            T behaviorTreeNodeDefinition = new T();
            behaviorTreeNodeDefinition.NodeName = Name;
            behaviorTreeNodeDefinition.Location = RectPosition;
            return behaviorTreeNodeDefinition;
        }

        public sealed override BehaviorTreeNodeDefinition GetDefinitionFromNode()
        {
            return GetTypedDefinitionFromNode();
        }
    }

    public abstract class BehaviorTreeNode : GraphNode
    {
        public override void _Ready()
        {
            base._Ready();
            Connect("resize_request", this, nameof(ResizeRequest));

        }

        private void ResizeRequest(Vector2 new_minsize)
        {
            RectSize = new_minsize;
        }
        public virtual bool CanDelete()
        {
            return true;
        }
        public abstract BehaviorTreeNodeDefinition GetDefinitionFromNode();



        protected static OptionButton CreateOptionForValueDefinitionSelector(ReadOnlyCollection<BehaviorTreeValueDefinition> valueDefinitions, BehaviorTreeValueDefinition defaultValue)
        {
            OptionButton optionButton = new OptionButton();
            optionButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;


            optionButton.AddItem("<None>", 0);

            int selectedIndex = -1;
            for (int i = 0; i < valueDefinitions.Count; i++)
            {
                var valueDefinition = valueDefinitions[i];
                optionButton.AddItem(valueDefinition.Key, i + 1);
                optionButton.SetItemMetadata(i + 1, valueDefinition);
                if (valueDefinition == defaultValue)
                {
                    selectedIndex = i + 1;
                }

            }

            if (selectedIndex != -1)
            {
                optionButton.CallDeferred("select", selectedIndex);
            }

            return optionButton;
        }

        protected static void UpdateValueDefinitionsInOptionButton(ReadOnlyCollection<BehaviorTreeValueDefinition> valueDefinitions, OptionButton optionButton)
        {
            var selectedKey = optionButton.GetSelectedMetadata() as BehaviorTreeValueDefinition;
            optionButton.Clear();
            optionButton.AddItem("<None>", 0);
            //Change in Items
            int indexOfKey = -1;
            for (int i = 0; i < valueDefinitions.Count; i++)
            {
                var valueDefinition = valueDefinitions[i];
                optionButton.AddItem(valueDefinition.Key == "" ? "NoText" : valueDefinition.Key, i + 1);//NOT working??
                optionButton.SetItemMetadata(i + 1, valueDefinition);
                if (valueDefinition == selectedKey)
                {
                    indexOfKey = i + 1;
                }
            }
            if (indexOfKey != -1)
            {
                optionButton.CallDeferred("select", indexOfKey);
            }
        }

    }
}
