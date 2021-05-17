using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Nodes
{
    public class CharacterMovementNode : Spatial
    {

        public override void _Ready()
        {
            _spatialParent = GetParent<Spatial>();
        }

        private Vector3? _target;
        private Spatial _spatialParent;

        [Export]
        public float MaximumMovementSpeed { get; set; }

        [Export]
        public float Acceleration { get; set; }

        [Export]
        public Vector3 Direction { get; set; }



        internal void MoveTo(Vector3 target)
        {
            Direction = (target - _spatialParent.GlobalTransform.origin).Normalized();
            _target = target;
        }
        float _timer = 0f;
        public override void _Process(float delta)
        {
            if (_target.HasValue)
            {
                _timer += delta;
                var lenghtOfDistance = MaximumMovementSpeed * delta;
                var distanceToTarget = _spatialParent.GlobalTransform.origin.DistanceTo(_target.Value);
                if (!distanceToTarget.EqualsWithMargin(0f))
                {
                    if (_timer > 1f)
                    {
                        Debug.WriteLine($"Distance to target: { distanceToTarget}, position {_spatialParent.GlobalTransform.origin} target {_target.Value}");
                        _timer = 0f;
                    }

                    _spatialParent.Translation += Direction * Mathf.Min(lenghtOfDistance, distanceToTarget);
                }
            }
        }
    }
}
