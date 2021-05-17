using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Nodes
{
    public class GodotWrapper : Godot.Object
    {
        public GodotWrapper(object value)
        {
            Value = value;
        }
        public object Value { get; set; }
    }
    public class DynamicInputControl : HBoxContainer
    {
        [Export]
        public ValueTypeDefinition ValueType { get; set; }

        public object DefaultValue { get; set; }

        public override void _Ready()
        {
            AddChild(GetControlForValueType(ValueType));
        }

        [Signal]
        public delegate void ValueHasChanged(GodotWrapper newValue);

        public Dictionary<int, Func<DynamicInputControl, int,  Node>> ControlCreationOverrides { get; private set; } = new Dictionary<int, Func<DynamicInputControl, int, Node>>();

        private Node GetControlForValueType(ValueTypeDefinition valueType)
        {
            if (ControlCreationOverrides.ContainsKey((int)valueType))
            {
                var node = ControlCreationOverrides[(int)valueType].Invoke(this, (int)valueType);
                return node;
            }
            switch (valueType)
            {
                case ValueTypeDefinition.String:
                    LineEdit valueEdit = new LineEdit();
                    valueEdit.Connect("text_changed", this, nameof(TextValueChanged));
                    valueEdit.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null && DefaultValue is string defaultString)
                    {
                        valueEdit.Text = defaultString;
                    }
                    return valueEdit;
                case ValueTypeDefinition.Int:
                    SpinBox spinBox = new SpinBox();
                    spinBox.Rounded = true;
                    spinBox.Step = 1f;
                    spinBox.AllowGreater = true;
                    spinBox.AllowLesser = true;
                    spinBox.Connect("value_changed", this, nameof(IntValueChanged));
                    spinBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null)
                    {
                        spinBox.Value = Convert.ToInt32(DefaultValue);
                    }
                    return spinBox;
                case ValueTypeDefinition.Float:
                    SpinBox floatBox = new SpinBox();
                    floatBox.Rounded = false;
                    floatBox.AllowGreater = true;
                    floatBox.AllowLesser = true;
                    floatBox.Step = 0.1f;
                    floatBox.Connect("value_changed", this, nameof(FloatValueChanged));
                    floatBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null)
                    {
                        floatBox.Value = Convert.ToDouble(DefaultValue);
                    }
                    return floatBox;
                case ValueTypeDefinition.Bool:
                    CheckBox checkBox = new CheckBox();
                    checkBox.Connect("pressed", this, nameof(BoolValueChanged), new Godot.Collections.Array() { checkBox });
                    checkBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null && DefaultValue is bool defaultBool)
                    {
                        checkBox.Pressed = defaultBool;
                    }
                    return checkBox;
                case ValueTypeDefinition.Vector2:
                    VectorInput vectorInput = new VectorInput();
                    vectorInput.IsVector2 = true;
                    vectorInput.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null && DefaultValue is Vector2 defaultVector2)
                    {
                        vectorInput.Vector2 = defaultVector2;
                    }
                    vectorInput.Connect(nameof(VectorInput.VectorChanged), this, nameof(Vector2ValueChanged));
                    return vectorInput;
                case ValueTypeDefinition.Vector3:
                    VectorInput vectorInputForVector3 = new VectorInput();
                    vectorInputForVector3.IsVector2 = false;
                    vectorInputForVector3.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    if (DefaultValue != null && DefaultValue is Vector3 defaultVector3)
                    {
                        vectorInputForVector3.Vector3 = defaultVector3;
                    }
                    vectorInputForVector3.Connect(nameof(VectorInput.VectorChanged), this, nameof(Vector3ValueChanged));
                    return vectorInputForVector3;
                default:
                    return new Control();
            }
        }

        private void Vector3ValueChanged(Vector2 vector2, Vector3 vector3)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper(vector3));
        }

        private void Vector2ValueChanged(Vector2 vector2, Vector3 vector3)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper(vector2));
        }

        private void BoolValueChanged(CheckBox checkBox)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper(checkBox.Pressed));
        }
        private void IntValueChanged(float value)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper((int)value));
        }

        private void FloatValueChanged(float value)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper(value));
        }

        private void TextValueChanged(string newText)
        {
            EmitSignal(nameof(ValueHasChanged), new GodotWrapper(newText));
        }
    }
}
