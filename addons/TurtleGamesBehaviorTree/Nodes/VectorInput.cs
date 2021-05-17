using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Nodes
{
    public class VectorInput : HBoxContainer
    {
        [Export]
        public bool IsVector2 { get; set; }

        [Export]
        public Vector2 Vector2 { get; set; }
        [Export]
        public Vector3 Vector3 { get; set; }

        [Signal]
        public delegate void VectorChanged(Vector2 vector2, Vector3 vector3);

        SpinBox _xSpinner;
        SpinBox _ySpinner;
        SpinBox _zSpinner;
        public override void _Ready()
        {
            _xSpinner = CreateSpinBox("X", nameof(ValueChangedOfSpinBoxX));

            AddChild(_xSpinner);
            _ySpinner = CreateSpinBox("Y", nameof(ValueChangedOfSpinBoxY));
            AddChild(_ySpinner);

            if (!IsVector2)
            {
                _zSpinner = CreateSpinBox("Z", nameof(ValueChangedOfSpinBoxZ));
                AddChild(_zSpinner);

                Debug.WriteLine(Vector3);
                if (Vector3 != Vector3.Zero)
                {
                    _xSpinner.Value = Vector3.x;
                    _ySpinner.Value = Vector3.y;
                    _zSpinner.Value = Vector3.z;
                }
            }
            else
            {
                Debug.WriteLine(Vector2);
                if (Vector2 != Vector2.Zero)
                {
                    _xSpinner.Value = Vector2.x;
                    _ySpinner.Value = Vector2.y;
                }

            }
        }

        private SpinBox CreateSpinBox(string preFix, string signalMethod)
        {
            SpinBox spinBox = new SpinBox();
            spinBox.Step = 0.1f;
            spinBox.AllowLesser = true;
            spinBox.AllowGreater = true;
            spinBox.Prefix = preFix;
            spinBox.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            spinBox.Connect("value_changed", this, signalMethod);
            return spinBox;
        }

        private void ValueChangedOfSpinBoxX(float newValue)
        {
            Vector2 = new Vector2(newValue, Vector2.y);
            Vector3 = new Vector3(newValue, Vector3.y, Vector3.z);
            EmitSignal(nameof(VectorChanged), Vector2, Vector3);
        }
        private void ValueChangedOfSpinBoxZ(float newValue)
        {
            Vector3 = new Vector3(Vector3.x, Vector3.y, newValue);
            EmitSignal(nameof(VectorChanged), Vector2, Vector3);
        }

        private void ValueChangedOfSpinBoxY(float newValue)
        {
            Vector2 = new Vector2(Vector2.x, newValue);
            Vector3 = new Vector3(Vector3.x, newValue, Vector3.z);
            EmitSignal(nameof(VectorChanged), Vector2, Vector3);
        }
    }
}
