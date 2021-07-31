using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Runtime;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin
{
    [Tool]
    public class GateNode : BehaviorTreeNode<ConditionNodeDefinition>
    {
        private GridContainer _conditionsContainer;
        private CheckBox _continuoslyCheckbox;
        private System.Collections.Generic.Dictionary<ConditionUI, (OptionButton ValueDefinitionSelector, OptionButton OperatorSelector, DynamicInputControl ValueControl, Container ValueContainer)> _controlsMapping = new System.Collections.Generic.Dictionary<ConditionUI, (OptionButton ValueDefinitionSelector, OptionButton OperatorSelector, DynamicInputControl ValueControl, Container ValueContainer)>();
        private OptionButton _abortOptionsButton;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            SetSlot(0, true, 1, Colors.White, true, 1, Colors.White);
            GetNode<Button>("VBoxContainer/AddConditionBtn").Connect("pressed", this, nameof(AddConditionClicked));
            _conditionsContainer = GetNode<GridContainer>("VBoxContainer/ConditionsContainer");
            GetParent<BehaviorTreeGraphEdit>().Connect(nameof(BehaviorTreeGraphEdit.ValueDefinitionsChanged), this, nameof(ValueDefinitionsHasChanged));
            _continuoslyCheckbox = GetNode<CheckBox>("VBoxContainer/CheckContinuoslyCB");
            _abortOptionsButton = GetNode<OptionButton>("VBoxContainer/AbortOptionsBtn");
            foreach (AbortOption abortOption in (AbortOption[])Enum.GetValues(typeof(AbortOption)))
            {
                _abortOptionsButton.AddItem(abortOption.ToString(), (int)abortOption);
            }



        }

        private void ValueDefinitionsHasChanged()
        {
            var graphEdit = GetParent<BehaviorTreeGraphEdit>();
            foreach (ConditionUI conditionUI in _controlsMapping.Keys)
            {
                var controls = _controlsMapping[conditionUI];
                AddValueDefinitionsToOptionButton(conditionUI, controls.ValueDefinitionSelector, graphEdit.GetBehaviorTreeValueDefinitions());
            }
        }

        private void AddConditionClicked()
        {
            ConditionUI conditionUI = new ConditionUI();
            AddControlsForCondition(conditionUI);
        }

        private void AddControlsForCondition(ConditionUI conditionUI)
        {
            var graphEdit = GetParent<BehaviorTreeGraphEdit>();
            OptionButton treeValueDefinitionSelector = new OptionButton();
            treeValueDefinitionSelector.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();
            AddValueDefinitionsToOptionButton(conditionUI, treeValueDefinitionSelector, valueDefinitions);
            treeValueDefinitionSelector.Connect("item_selected", this, nameof(ValueKeySelectorKeyChanged), new Godot.Collections.Array() { treeValueDefinitionSelector, new GodotWrapper(conditionUI) });

            OptionButton operatorSelector = CreateConditionSelector(conditionUI, null);
            operatorSelector.Connect("item_selected", this, nameof(OperatorChanged), new Godot.Collections.Array() { operatorSelector, new GodotWrapper(conditionUI) });

            treeValueDefinitionSelector.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            operatorSelector.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            Container valueContainer = new Container();
            valueContainer.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

            DynamicInputControl dynamicInputControl = GetInputControl(conditionUI, conditionUI.Operator, conditionUI.TreeValue);
            valueContainer.AddChild(dynamicInputControl);

            _conditionsContainer.AddChild(treeValueDefinitionSelector);
            _conditionsContainer.AddChild(operatorSelector);
            _conditionsContainer.AddChild(valueContainer);
            _controlsMapping.Add(conditionUI, (treeValueDefinitionSelector, operatorSelector, dynamicInputControl, valueContainer));
        }

        private static void AddValueDefinitionsToOptionButton(ConditionUI conditionUI, OptionButton treeValueDefinitionSelector, ReadOnlyCollection<BehaviorTreeValueDefinition> valueDefinitions)
        {
            treeValueDefinitionSelector.Clear();
            treeValueDefinitionSelector.AddItem("<None>", 0);
            int selectedIndex = -1;
            for (int i = 0; i < valueDefinitions.Count; i++)
            {
                var valueDefinition = valueDefinitions[i];
                treeValueDefinitionSelector.AddItem(valueDefinition.Key, i + 1);
                treeValueDefinitionSelector.SetItemMetadata(i + 1, valueDefinition);
                if (valueDefinition == conditionUI.TreeValue)
                {
                    selectedIndex = i + 1;
                }
            }
            if (selectedIndex != -1)
            {
                treeValueDefinitionSelector.CallDeferred("select", selectedIndex);
            }
        }

        private void OperatorChanged(int index, OptionButton optionButton, GodotWrapper conditionUIWrapped)
        {
            if (conditionUIWrapped.Value != null)
            {
                var condition = (ConditionUI)conditionUIWrapped.Value;
                condition.Values.Clear();
                var chosenOperator = (ConditionOperator)_controlsMapping[condition].OperatorSelector.GetSelectedMetadata();
                _controlsMapping[condition].ValueControl?.QueueFree();
                DynamicInputControl dynamicInputControl = GetInputControl(condition, chosenOperator, _controlsMapping[condition].ValueDefinitionSelector.GetSelectedMetadata() as BehaviorTreeValueDefinition);

                if (dynamicInputControl != null)
                {
                    _controlsMapping[condition].ValueContainer.AddChild(dynamicInputControl);
                }

                _controlsMapping[condition] = (_controlsMapping[condition].ValueDefinitionSelector, _controlsMapping[condition].OperatorSelector, dynamicInputControl, _controlsMapping[condition].ValueContainer);
                //Change value stuff
            }
        }

        private DynamicInputControl GetInputControl(ConditionUI condition, ConditionOperator chosenOperator, BehaviorTreeValueDefinition valueDefinition)
        {
            if (chosenOperator != ConditionOperator.IsNotSet && chosenOperator != ConditionOperator.IsSet)
            {
                DynamicInputControl dynamicInputControl = new DynamicInputControl();
                dynamicInputControl.ValueType = valueDefinition.ValueType;
                if (condition.Values.Count > 0)
                {
                    dynamicInputControl.DefaultValue = condition.Values[0];
                }
                dynamicInputControl.Connect(nameof(DynamicInputControl.ValueHasChanged), this, nameof(OperandValueChanged), new Godot.Collections.Array() { new GodotWrapper(condition) });
                dynamicInputControl.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;

                return dynamicInputControl;
            }
            else
            {
                return null;
            }
        }

        internal void InitializeFromStorage(GodotWrapper nodeDefinition)
        {
            ConditionNodeDefinition conditionNodeDefinition = nodeDefinition.Value as ConditionNodeDefinition;
            foreach (var condition in conditionNodeDefinition.Conditions)
            {
                ConditionUI conditionUI = new ConditionUI();
                conditionUI.Operator = condition.ConditionOperator;
                conditionUI.Values = condition.Values;
                conditionUI.TreeValue = GetParent<BehaviorTreeGraphEdit>().GetBehaviorTreeValueDefinitions().FirstOrDefault(b => b.Key == condition.Key);
                AddControlsForCondition(conditionUI);
            }

        }

        private void OperandValueChanged(GodotWrapper newValue, GodotWrapper condition)
        {
            (condition.Value as ConditionUI).Values.Clear();
            (condition.Value as ConditionUI).Values.Add(newValue.Value);
        }

        private void ValueKeySelectorKeyChanged(int index, OptionButton optionButton, GodotWrapper conditionUIWrapped)
        {
            ConditionUI conditionUI = conditionUIWrapped.Value as ConditionUI;
            conditionUI.TreeValue = optionButton.GetSelectedMetadata() as BehaviorTreeValueDefinition;
            conditionUI.Values.Clear();
            _controlsMapping[conditionUI].ValueControl?.QueueFree();

            var controls = _controlsMapping[conditionUI];
            var operatorSelector = controls.OperatorSelector;
            CreateConditionSelector(conditionUI, operatorSelector);
            _controlsMapping[conditionUI] = (_controlsMapping[conditionUI].ValueDefinitionSelector, _controlsMapping[conditionUI].OperatorSelector, null, _controlsMapping[conditionUI].ValueContainer);
            //TODO: Update stuff
        }

        private static OptionButton CreateConditionSelector(ConditionUI conditionUI, OptionButton operatorSelector)
        {
            if (operatorSelector == null)
            {
                operatorSelector = new OptionButton();
            }
            operatorSelector.Clear();
            operatorSelector.AddItem("<None>", 0);
            operatorSelector.SetItemMetadata(0, -1);

            int select = -1;

            if (conditionUI.TreeValue != null)
            {
                var enumValues = (ConditionOperator[])Enum.GetValues(typeof(ConditionOperator));
                int added = 1;
                for (int i = 0; i < enumValues.Length; i++)
                {
                    ConditionOperator conditionOperator = enumValues[i];
                    if (conditionOperator.GetAttribute<OperatorForValueTypeAttribute>().ValidForValueTypes.Contains(conditionUI.TreeValue.ValueType))
                    {

                        operatorSelector.AddItem(conditionOperator.ToString(), added);
                        operatorSelector.SetItemMetadata(added, conditionOperator);
                        if (conditionOperator == conditionUI.Operator)
                        {
                            select = added;
                        }
                        added++;
                    }
                }
            }
            if (select != -1)
            {
                operatorSelector.CallDeferred("select", select);
            }
            return operatorSelector;
        }

        public override ConditionNodeDefinition GetTypedDefinitionFromNode()
        {
            var conditionNodeDefinition = base.GetTypedDefinitionFromNode();
            Debug.WriteLine("1");
            conditionNodeDefinition.CheckContinuously = _continuoslyCheckbox.Pressed;
            Debug.WriteLine("2");
            conditionNodeDefinition.AbortOption = (AbortOption)_abortOptionsButton.GetSelectedId();
            Debug.WriteLine("3");
            foreach (var condition in _controlsMapping.Keys)
            {
                Debug.WriteLine($"ConditionOperator {(ConditionOperator)_controlsMapping[condition].OperatorSelector.GetSelectedMetadata()}");
                Debug.WriteLine("4");

                conditionNodeDefinition.Conditions.Add(new ConditionStorage() { Key = (_controlsMapping[condition].ValueDefinitionSelector.GetSelectedMetadata() as BehaviorTreeValueDefinition)?.Key, ConditionOperator = (ConditionOperator)_controlsMapping[condition].OperatorSelector.GetSelectedMetadata(), Values = condition.Values });
            }
            Debug.WriteLine("5");

            return conditionNodeDefinition;
        }
    }

    public class ConditionUI
    {
        public BehaviorTreeValueDefinition TreeValue { get; set; }

        public ConditionOperator Operator { get; set; }

        public List<object> Values { get; set; } = new List<object>();
    }
}
