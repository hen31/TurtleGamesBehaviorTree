using Godot;
using Godot.Collections;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class ActionNode : BehaviorTreeNode<ActionNodeDefinition>
    {
        private OptionButton _actionList;
        private GridContainer _parameterList;
        private System.Collections.Generic.Dictionary<ParameterDefinition, (Label NameLabel, Node InputControl)> _controls = new System.Collections.Generic.Dictionary<ParameterDefinition, (Label NameLabel, Node InputControl)>();
        private System.Collections.Generic.Dictionary<ParameterDefinition, object> _values = new System.Collections.Generic.Dictionary<ParameterDefinition, object>();
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SetSlot(0, true, 1, Colors.White, false, 0, Colors.White);
            _actionList = GetNode<OptionButton>("VBoxContainer/ActionList");
            _actionList.Connect("item_selected", this, nameof(ActionSelectionChanged));
            _parameterList = GetNode<GridContainer>("VBoxContainer/ParameterBox");
            BehaviorTreeRegistry.Instance.InitializeRegistry();
            /*    if (_actionList.Items.Count == 0)
                {*/
            _actionList.Clear();
            Debug.WriteLine($"Count of actions is: {BehaviorTreeRegistry.Instance.TreeActions.Length}");
            foreach (var actionDefinitionName in BehaviorTreeRegistry.Instance.TreeActions.Select(b => b.Name))
            {
                Debug.WriteLine($"AddAction: {actionDefinitionName}");
                _actionList.AddItem(actionDefinitionName);
            }
            //}
            if (_actionList.Selected >= 0)
            {
                AddControlsForParameters(BehaviorTreeRegistry.Instance.TreeActions[_actionList.Selected]);
                GetParent<BehaviorTreeGraphEdit>().Connect(nameof(BehaviorTreeGraphEdit.ValueDefinitionsChanged), this, nameof(ValueDefinitionsHasChanged));

            }

        }

        private void ActionSelectionChanged(int newIndex)
        {
            if (newIndex >= 0)
            {
                var selectedAction = BehaviorTreeRegistry.Instance.TreeActions[newIndex];
                RemoveOldParameterControls();
                AddControlsForParameters(selectedAction);
            }
        }

        private void RemoveOldParameterControls()
        {
            foreach (var key in _controls.Keys)
            {
                var controls = _controls[key];
                controls.InputControl.QueueFree();
                controls.NameLabel.QueueFree();
            }
            _controls.Clear();
            _values.Clear();
        }

        private void AddControlsForParameters(TreeActionDefinition selectedAction)
        {
            foreach (var parameterDefinition in selectedAction.ParameterDefinitions)
            {
                Label parameterLabel = new Label();
                parameterLabel.Text = parameterDefinition.Name;

                var dynamicControl = new DynamicInputControl();
                dynamicControl.ControlCreationOverrides.Add((int)ParameterTypeDefinition.ValueKey, (DynamicInputControl inputControl, int typeDefinition) =>
                {
                    var graphEdit = GetParent<BehaviorTreeGraphEdit>();
                    var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();
                    BehaviorTreeValueDefinition defaultValue = null;
                    if (inputControl.DefaultValue != null && inputControl.DefaultValue is string valueDefinitionKey)
                    {
                        defaultValue = valueDefinitions.FirstOrDefault(b => b.Key == valueDefinitionKey);
                    }
                    else if (inputControl.DefaultValue != null && inputControl.DefaultValue is BehaviorTreeValueDefinition defaultValueDefinition)
                    {
                        defaultValue = defaultValueDefinition;
                    }

                    OptionButton optionButton = CreateOptionForValueDefinitionSelector(valueDefinitions, defaultValue);

                    optionButton.Connect("item_selected", this, nameof(ValueKeySelectorKeyChanged), new Godot.Collections.Array() { optionButton });

                    return optionButton;
                });

                dynamicControl.ValueType = (ValueTypeDefinition)parameterDefinition.ParameterType;
                if (_values.ContainsKey(parameterDefinition))
                {
                    dynamicControl.DefaultValue = _values[parameterDefinition];
                }

                dynamicControl.Connect(nameof(DynamicInputControl.ValueHasChanged), this, nameof(ValueChanged), new Godot.Collections.Array() { new GodotWrapper(parameterDefinition) });

                dynamicControl.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                _parameterList.AddChild(parameterLabel);
                _parameterList.AddChild(dynamicControl);

                _controls.Add(parameterDefinition, (parameterLabel, dynamicControl));
            }
        }



        private void ValueKeySelectorKeyChanged(int index, OptionButton optionButton)
        {
            if (index == 0)
            {
                optionButton.GetParent<DynamicInputControl>().EmitSignal(nameof(DynamicInputControl.ValueHasChanged), new GodotWrapper(null));
            }
            else
            {
                optionButton.GetParent<DynamicInputControl>().EmitSignal(nameof(DynamicInputControl.ValueHasChanged), new GodotWrapper((optionButton.GetSelectedMetadata() as BehaviorTreeValueDefinition)));
            }
        }
        private void ValueDefinitionsHasChanged()
        {
            if (_actionList.Selected != -1)
            {
                var selectedAction = BehaviorTreeRegistry.Instance.TreeActions[_actionList.Selected];

                var graphEdit = GetParent<BehaviorTreeGraphEdit>();
                var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();

                foreach (var parameter in selectedAction.ParameterDefinitions)
                {
                    if (parameter.ParameterType == ParameterTypeDefinition.ValueKey)
                    {
                        var optionButton = _controls[parameter].InputControl.GetChild<OptionButton>(0);
                        UpdateValueDefinitionsInOptionButton(valueDefinitions, optionButton);
                    }
                }
            }
        }

        private void ValueChanged(GodotWrapper newValue, GodotWrapper parameterDefinitionWrapped)
        {
            var parameterDefinition = parameterDefinitionWrapped.Value as ParameterDefinition;

            if (!_values.ContainsKey(parameterDefinition))
            {
                _values.Add(parameterDefinition, newValue.Value);
            }
            else
            {
                _values[parameterDefinition] = newValue.Value;
            }
        }

        public override ActionNodeDefinition GetTypedDefinitionFromNode()
        {
            var definition = base.GetTypedDefinitionFromNode();
            if (_actionList.Selected != -1)
            {
                definition.ActionName = BehaviorTreeRegistry.Instance.TreeActions[_actionList.Selected].Name;
            }
            foreach (var parameterDefinition in _values.Keys)
            {
                if (_values[parameterDefinition] is BehaviorTreeValueDefinition valueDefinition)
                {
                    definition.ParameterValues.Add(parameterDefinition.Name, valueDefinition.Key);

                }
                else
                {
                    definition.ParameterValues.Add(parameterDefinition.Name, _values[parameterDefinition]);
                }
            }
            return definition;
        }

        internal void InitializeWithActionName(string actionName, GodotWrapper parameterValues)
        {
            RemoveOldParameterControls();
            var itemIndex = BehaviorTreeRegistry.Instance.TreeActions.Select(b => b.Name).TakeWhile(x => x != actionName).Count();
            _actionList.Select(itemIndex);
            var selectedAction = BehaviorTreeRegistry.Instance.TreeActions[itemIndex];
            var parameterValuesDictionary = parameterValues.Value as System.Collections.Generic.Dictionary<string, object>;
            foreach (var parameter in selectedAction.ParameterDefinitions)
            {
                if (parameterValuesDictionary.ContainsKey(parameter.Name))
                {
                    _values.Add(parameter, parameterValuesDictionary[parameter.Name]);
                }
            }
            AddControlsForParameters(selectedAction);

        }


    }
}