using Godot;
using Godot.Collections;
using Newtonsoft.Json;
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
    public class SubBehaviorTreeNode : BehaviorTreeNode<SubBehaviorTreeNodeDefinition>
    {
        [Export]
        public NodePath SelectedLineEdit { get; set; }
        private LineEdit _selectedLineEdit;
        [Export]
        public NodePath SelectTreeBtn { get; set; }
        private Button _selectTreeBtn;
        [Export]
        public NodePath SelectTreeDialog { get; set; }
        private FileDialog _selectTreeDialog;

        [Export]
        public NodePath TreeValuesContainer { get; set; }
        private GridContainer _treeValuesContainer;

        private System.Collections.Generic.Dictionary<BehaviorTreeValueDefinitionStorage, (OptionButton ValueDefinitionControl, CheckButton SyncButton)> _controlsMapping;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            _controlsMapping = new System.Collections.Generic.Dictionary<BehaviorTreeValueDefinitionStorage, (OptionButton ValueDefinitionControl, CheckButton SyncButton)>();
            SetSlot(0, true, 1, Colors.White, false, 0, Colors.White);
            _selectedLineEdit = this.GetNode<LineEdit>(SelectedLineEdit);
            _selectTreeBtn = this.GetNode<Button>(SelectTreeBtn);
            _selectTreeBtn.Connect("pressed", this, nameof(SelectBehaviorTreeClicked));
            _selectTreeDialog = this.GetNode<FileDialog>(SelectTreeDialog);
            _selectTreeDialog.Connect("file_selected", this, nameof(FileSelected));
            _treeValuesContainer = this.GetNode<GridContainer>(TreeValuesContainer);
            GetParent<BehaviorTreeGraphEdit>().Connect(nameof(BehaviorTreeGraphEdit.ValueDefinitionsChanged), this, nameof(ValueDefinitionsHasChanged));
        }

        private void FileSelected(string path)
        {
            if (path != _selectedLineEdit.Text)
            {
                CreateControlsForParameters(path, null);
            }
            _selectedLineEdit.Text = path;

        }

        private void CreateControlsForParameters(string path, List<SubBehaviorTreeValueLink> valueDefinitionValues)
        {
            foreach (Node node in _treeValuesContainer.GetChildren())
            {
                node.QueueFree();
            }
            _controlsMapping.Clear();
            File treeDefinitionFile = new File();
            treeDefinitionFile.Open(path, File.ModeFlags.Read);
            string jsonOfTree = treeDefinitionFile.GetAsText();
            treeDefinitionFile.Close();
            var treeDefinition = JsonConvert.DeserializeObject<BehaviorTreeDefinition>(jsonOfTree);

            foreach (var valueDefinition in treeDefinition.ValueDefinitions)
            {
                var storedValue = valueDefinitionValues?.FirstOrDefault(b => b.Name == valueDefinition.Key);
                Label valueDefinitionLabel = new Label();
                valueDefinitionLabel.Text = valueDefinition.Key;
                _treeValuesContainer.AddChild(valueDefinitionLabel);

                var graphEdit = GetParent<BehaviorTreeGraphEdit>();
                var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();

                BehaviorTreeValueDefinition selectedBehaviorTreeValue = null;
                if (storedValue != null)
                {
                    selectedBehaviorTreeValue = valueDefinitions.FirstOrDefault(b => b.Key == storedValue.LinkedTo);
                }
        
                OptionButton optionButton = CreateOptionForValueDefinitionSelector(valueDefinitions, selectedBehaviorTreeValue);
                _treeValuesContainer.AddChild(optionButton);

                CheckButton checkButton = new CheckButton();
                checkButton.Text = "Sync";
                if(storedValue != null)
                {
                    checkButton.Pressed = storedValue.KeepInSync;
                }
                _treeValuesContainer.AddChild(checkButton);

                _controlsMapping.Add(valueDefinition, (optionButton, checkButton));
            }
        }

        private void SelectBehaviorTreeClicked()
        {
            _selectTreeDialog.ShowModal();
        }


        private void ValueDefinitionsHasChanged()
        {
            var graphEdit = GetParent<BehaviorTreeGraphEdit>();
            var valueDefinitions = graphEdit.GetBehaviorTreeValueDefinitions();

            foreach (Node valueControl in _treeValuesContainer.GetChildren())
            {
                if (valueControl is OptionButton optionButton)
                {
                    UpdateValueDefinitionsInOptionButton(valueDefinitions, optionButton);
                }
            }
        }

        public override SubBehaviorTreeNodeDefinition GetTypedDefinitionFromNode()
        {
            var definition = base.GetTypedDefinitionFromNode();
            if (!string.IsNullOrWhiteSpace(_selectedLineEdit.Text))
            {
                definition.SelectedSubTree = _selectedLineEdit.Text;
            }
            foreach (var parameterDefinition in _controlsMapping.Keys)
            {
                var controls = _controlsMapping[parameterDefinition];
                if (controls.ValueDefinitionControl.Selected != 0 && controls.ValueDefinitionControl.Selected != -1)
                {

                    definition.ValueDefinitionValues.Add(
                        new SubBehaviorTreeValueLink()
                        {
                            Name = parameterDefinition.Key,
                            LinkedTo = (controls.ValueDefinitionControl.GetSelectedMetadata() as BehaviorTreeValueDefinition).Key,
                            KeepInSync = controls.SyncButton.Pressed
                        });
                }
            }
            return definition;
        }

        internal void InitializeFromStorage(GodotWrapper wrappedDefinition)
        {
            var definition = wrappedDefinition.Value as SubBehaviorTreeNodeDefinition;
            if (!string.IsNullOrWhiteSpace(definition.SelectedSubTree))
            {
                _selectedLineEdit.Text = definition.SelectedSubTree;
                CreateControlsForParameters(definition.SelectedSubTree, definition.ValueDefinitionValues);
            }

        }
    }
}