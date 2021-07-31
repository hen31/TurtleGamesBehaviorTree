using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TurtleGames.BehaviourTreePlugin;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Storage;

[Tool]
public class BehaviourTreeDock : Control
{
    // Declare member variables here. Examples:
    BehaviorTreeGraphEdit _graphEdit;
    FileDialog _fileDialog;
    CurrentFileUsage _currentFileUsage;
    GridContainer _valueDefinitionsContainer;
    private Dictionary<BehaviorTreeValueDefinition, (LineEdit NameEdit, OptionButton ValueTypeButton, Node DefaultValueControl, Button DeleteButton)> _definitionNodeMapping = new Dictionary<BehaviorTreeValueDefinition, (LineEdit NameEdit, OptionButton ValueTypeButton, Node DefaultValueControl, Button DeleteButton)>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _graphEdit = GetNode<BehaviorTreeGraphEdit>("VBoxContainer/HBox/GraphEdit");
        GetNode<Button>("VBoxContainer/HBoxContainer/NewBtn").Connect("pressed", this, nameof(NewTreeClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/AddSequenceBtn").Connect("pressed", this, nameof(AddSequenceClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/AddSelectorBtn").Connect("pressed", this, nameof(AddSelectorClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/SaveBtn").Connect("pressed", this, nameof(SaveClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/OpenBtn").Connect("pressed", this, nameof(OpenClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/AddActionBtn").Connect("pressed", this, nameof(AddActionClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/AddServiceBtn").Connect("pressed", this, nameof(AddServiceClicked));
        GetNode<Button>("VBoxContainer/HBox/VBoxContainer/AddValueBtn").Connect("pressed", this, nameof(AddValueDefinitionClicked));
        GetNode<Button>("VBoxContainer/HBoxContainer/AddGateBtn").Connect("pressed", this, nameof(AddGateClicked));

        _valueDefinitionsContainer = GetNode<GridContainer>("VBoxContainer/HBox/VBoxContainer/ScrollContainer/ValueDefinitionsGrid");

        _fileDialog = GetNode<FileDialog>("FileDialog");
        _fileDialog.Connect("file_selected", this, nameof(FileSelected));

        BehaviorTreeRegistry.Instance.InitializeRegistry();
        //VBoxContainer/HBoxContainer/SaveBtn
    }

    private void AddServiceClicked()
    {
        var node = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/ServiceNode.tscn").Instance() as ServiceNode;
        _graphEdit.AddChild(node);
    }

    private void AddSelectorClicked()
    {
        var node = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/SelectorNode.tscn").Instance() as SelectorNode;
        _graphEdit.AddChild(node);
    }

    private void AddGateClicked()
    {
        var node = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/GateNode.tscn").Instance() as GateNode;
        _graphEdit.AddChild(node);
    }

    private void AddValueDefinitionClicked()
    {
        BehaviorTreeValueDefinition behaviorTreeValueDefinition = new BehaviorTreeValueDefinition();
        behaviorTreeValueDefinition.ValueType = TurtleGames.BehaviourTreePlugin.ValueTypeDefinition.String;
        behaviorTreeValueDefinition.Key = "Value " + _graphEdit.GetBehaviorTreeValueDefinitions().Count;
        _graphEdit.AddValueDefinition(behaviorTreeValueDefinition);
        AddControlsForValueDefinition(behaviorTreeValueDefinition);
    }

    public void AddControlsForValueDefinition(BehaviorTreeValueDefinition behaviorTreeValueDefinition)
    {
        LineEdit textEdit = new LineEdit();
        textEdit.Text = behaviorTreeValueDefinition.Key;
        textEdit.Connect("text_changed", this, nameof(ValueDefinitionNameChanged), new Godot.Collections.Array() { behaviorTreeValueDefinition });
        textEdit.SizeFlagsHorizontal = (int)(SizeFlags.ExpandFill);

        _valueDefinitionsContainer.AddChild(textEdit);


        OptionButton optionButton = new OptionButton();
        foreach (var enumValue in Enum.GetValues(typeof(ValueTypeDefinition)))
        {
            optionButton.AddItem(GetDescriptionFromEnum((ValueTypeDefinition)enumValue), (int)enumValue);
        }
        optionButton.SizeFlagsHorizontal = (int)(SizeFlags.ExpandFill);
        optionButton.Selected = (int)behaviorTreeValueDefinition.ValueType;
        optionButton.Connect("item_selected", this, nameof(ValueTypeSelectionChanged), new Godot.Collections.Array() { behaviorTreeValueDefinition });
        _valueDefinitionsContainer.AddChild(optionButton);

        var dynamicControl = GetControlForValueType(behaviorTreeValueDefinition);
        _valueDefinitionsContainer.AddChild(dynamicControl);

        Button deleteBtn = new Button();
        deleteBtn.Text = "Delete";
        deleteBtn.Connect("pressed", this, nameof(DeleteValueDefinition), new Godot.Collections.Array() { behaviorTreeValueDefinition });
        _valueDefinitionsContainer.AddChild(deleteBtn);

        _definitionNodeMapping.Add(behaviorTreeValueDefinition, (textEdit, optionButton, dynamicControl, deleteBtn));
    }

    private void DeleteValueDefinition(BehaviorTreeValueDefinition valueDefinition)
    {
        _graphEdit.RemoveValueDefinition(valueDefinition);
        var controls = _definitionNodeMapping[valueDefinition];
        controls.DefaultValueControl.QueueFree();
        controls.DeleteButton.QueueFree();
        controls.NameEdit.QueueFree();
        controls.ValueTypeButton.QueueFree();
    }

    private Node GetControlForValueType(BehaviorTreeValueDefinition valueDefinition)
    {
        DynamicInputControl dynamicInputControl = new DynamicInputControl();
        Debug.WriteLine($"DefaultValue: {valueDefinition.DefaultValue} Type {valueDefinition.ValueType}");
        dynamicInputControl.DefaultValue = valueDefinition.DefaultValue;
        dynamicInputControl.ValueType = valueDefinition.ValueType;
        dynamicInputControl.Connect(nameof(DynamicInputControl.ValueHasChanged), this, nameof(ValueHasChanged), new Godot.Collections.Array() { valueDefinition });
        dynamicInputControl.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
        return dynamicInputControl;
    }

    private void ValueHasChanged(GodotWrapper newValue, BehaviorTreeValueDefinition behaviorTreeValueDefinition)
    {
        behaviorTreeValueDefinition.DefaultValue = newValue.Value;
    }

    private void ValueTypeSelectionChanged(int index, BehaviorTreeValueDefinition behaviorTreeValueDefinition)
    {
        behaviorTreeValueDefinition.ValueType = (ValueTypeDefinition)index;
        var controls = _definitionNodeMapping[behaviorTreeValueDefinition];
        var newNode = GetControlForValueType(behaviorTreeValueDefinition);
        _valueDefinitionsContainer.AddChildBelowNode(controls.DefaultValueControl, newNode);
        controls.DefaultValueControl.QueueFree();
        controls.DefaultValueControl = newNode;
        _definitionNodeMapping[behaviorTreeValueDefinition] = controls;
    }

    public static string GetDescriptionFromEnum(Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            System.Reflection.FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return value.ToString();
    }
    private void ValueDefinitionNameChanged(string newText, BehaviorTreeValueDefinition behaviorTreeValueDefinition)
    {
        behaviorTreeValueDefinition.Key = newText;
        _graphEdit.NameOfValueDefinitionHasChanged(behaviorTreeValueDefinition);
    }

    private void OpenClicked()
    {
        _currentFileUsage = CurrentFileUsage.Open;
        _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
        _fileDialog.ShowModal();
    }

    private void FileSelected(string path)
    {
        if (_currentFileUsage == CurrentFileUsage.Save)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
            string jsonString = JsonConvert.SerializeObject(_graphEdit.GetTreeDefinition(), jsonSerializerSettings);
            var dir = new Directory();
            if (dir.FileExists(path))
            {
                dir.Remove(path);
            }
            Godot.File file = new File();
            file.Open(path, File.ModeFlags.Write);

            file.StoreString(jsonString);
            file.Close();
        }
        else if (_currentFileUsage == CurrentFileUsage.Open)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
            Godot.File file = new File();
            file.Open(path, File.ModeFlags.Read);
            string jsonString = file.GetAsText();
            file.Close();
            Debug.Write(jsonString);
            BehaviorTreeDefinition behaviorTreeDefinition = JsonConvert.DeserializeObject<BehaviorTreeDefinition>(jsonString, jsonSerializerSettings);
            RemoveAllValueDefinitions();
            foreach (var valueDefinitionStorage in behaviorTreeDefinition.ValueDefinitions)
            {
                var valueDefinitionConverted = valueDefinitionStorage.ToValueDefinition();
                _graphEdit.AddValueDefinition(valueDefinitionConverted);
                AddControlsForValueDefinition(valueDefinitionConverted);
            }
            _graphEdit.InitializeFromDefinition(behaviorTreeDefinition);
        }
    }

    private void RemoveAllValueDefinitions()
    {
        while (_graphEdit.GetBehaviorTreeValueDefinitions().Count != 0)
        {
            DeleteValueDefinition(_graphEdit.GetBehaviorTreeValueDefinitions()[0]);
        }
    }

    private void SaveClicked()
    {
        _currentFileUsage = CurrentFileUsage.Save;
        _fileDialog.Mode = FileDialog.ModeEnum.SaveFile;
        _fileDialog.ShowModal();
    }

    private void AddSequenceClicked()
    {
        var node = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/SequenceNode.tscn").Instance() as SequenceNode;
        _graphEdit.AddChild(node);
    }
    private void AddActionClicked()
    {
        var node = GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/ActionNode.tscn").Instance() as ActionNode;
        _graphEdit.AddChild(node);
    }


    private void NewTreeClicked()
    {
        RemoveAllValueDefinitions();
        _graphEdit.ClearGraphEdit();
        _graphEdit.AddChild(GD.Load<PackedScene>("res://addons/TurtleGamesBehaviorTree/BehaviorTreeNodes/TreeRootNode.tscn").Instance());
    }


    public override void _UnhandledKeyInput(InputEventKey keyInputEvent)
    {
        base._UnhandledKeyInput(keyInputEvent);
        if (keyInputEvent.Scancode == (uint)KeyList.Delete)
        {
            if (_graphEdit.CurrentSelection.Count == 1 && (_graphEdit.CurrentSelection[0] as BehaviorTreeNode).CanDelete())
            {
                _graphEdit.DeleteGraphNode(_graphEdit.CurrentSelection[0]);
            }
        }
    }

    private enum CurrentFileUsage { Open, Save };
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
