using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using System.Linq;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class ServiceNode : BehaviorTreeNode<ServiceNodeDefinition>
    {
        private CheckBox _alwaysExecute;
        private SpinBox _executionTimeSelector;
        private OptionButton _actionList;
        private GridContainer _parameterList;
        private System.Collections.Generic.Dictionary<ParameterDefinition, (Label NameLabel, Node InputControl)> _controls = new System.Collections.Generic.Dictionary<ParameterDefinition, (Label NameLabel, Node InputControl)>();
        private System.Collections.Generic.Dictionary<ParameterDefinition, object> _values = new System.Collections.Generic.Dictionary<ParameterDefinition, object>();


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            SetSlot(0, true, 1, Colors.White, true, 1, Colors.White);
            _executionTimeSelector = GetNode<SpinBox>("VBoxContainer/HBoxContainer/ExcutionTimerSelector");
            _alwaysExecute = GetNode<CheckBox>("VBoxContainer/HBoxContainer2/CheckBox");
            _actionList = GetNode<OptionButton>("VBoxContainer/ServiceList");
            _actionList.Connect("item_selected", this, nameof(ServiceSelectionChanged));
            _parameterList = GetNode<GridContainer>("VBoxContainer/ParameterBox");
            BehaviorTreeRegistry.Instance.InitializeRegistry();
            /*    if (_actionList.Items.Count == 0)
                {*/
            _actionList.Clear();
            Debug.WriteLine($"Count of actions is: {BehaviorTreeRegistry.Instance.TreeServices.Length}");
            foreach (var serviceDefinitionName in BehaviorTreeRegistry.Instance.TreeServices.Select(b => b.Name))
            {
                Debug.WriteLine($"AddAction: {serviceDefinitionName}");
                _actionList.AddItem(serviceDefinitionName);
            }
            //}
            if (_actionList.Selected >= 0)
            {
                AddControlsForParameters(BehaviorTreeRegistry.Instance.TreeServices[_actionList.Selected]);
                GetParent<BehaviorTreeGraphEdit>().Connect(nameof(BehaviorTreeGraphEdit.ValueDefinitionsChanged), this, nameof(ValueDefinitionsHasChanged));

            }

        }

        private void ServiceSelectionChanged(int newIndex)
        {
            if (newIndex >= 0)
            {
                var selectedAction = BehaviorTreeRegistry.Instance.TreeServices[newIndex];
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

        private void AddControlsForParameters(TreeServiceDefinition selectedService)
        {
            foreach (var parameterDefinition in selectedService.ParameterDefinitions)
            {
                Label parameterLabel = new Label();
                parameterLabel.Text = parameterDefinition.Name;

                var dynamicControl = new DynamicInputControl();
                dynamicControl.ControlCreationOverrides.Add((int)ParameterTypeDefinition.ValueKey, (DynamicInputControl inputControl, int typeDefinition) =>
                {
                    var graphEdit = GetParent<BehaviorTreeGraphEdit>();
                    OptionButton optionButton = new OptionButton();
                    optionButton.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
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
                var selectedAction = BehaviorTreeRegistry.Instance.TreeServices[_actionList.Selected];

                var graphEdit = GetParent<BehaviorTreeGraphEdit>();
                var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();

                foreach (var parameter in selectedAction.ParameterDefinitions)
                {
                    if (parameter.ParameterType == ParameterTypeDefinition.ValueKey)
                    {
                        var optionButton = _controls[parameter].InputControl.GetChild<OptionButton>(0);
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

        public override ServiceNodeDefinition GetTypedDefinitionFromNode()
        {
            var definition = base.GetTypedDefinitionFromNode();
            if (_actionList.Selected != -1)
            {
                definition.ServiceName = BehaviorTreeRegistry.Instance.TreeServices[_actionList.Selected].Name;
            }
            definition.ExecutionTimer = _executionTimeSelector.Value;
            definition.AlwaysExecute = _alwaysExecute.Pressed;
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

        internal void InitializeWithServiceName(string actionName, GodotWrapper parameterValues, double excutionTime, bool alwaysExecute)
        {
            RemoveOldParameterControls();
            var itemIndex = BehaviorTreeRegistry.Instance.TreeServices.Select(b => b.Name).TakeWhile(x => x != actionName).Count();
            _actionList.Select(itemIndex);
            var selectedAction = BehaviorTreeRegistry.Instance.TreeServices[itemIndex];
            var parameterValuesDictionary = parameterValues.Value as System.Collections.Generic.Dictionary<string, object>;
            foreach (var parameter in selectedAction.ParameterDefinitions)
            {
                if (parameterValuesDictionary.ContainsKey(parameter.Name))
                {
                    _values.Add(parameter, parameterValuesDictionary[parameter.Name]);
                }
            }
            AddControlsForParameters(selectedAction);
            _executionTimeSelector.Value = excutionTime;
            _alwaysExecute.Pressed =alwaysExecute;

        }


    }
}