using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Actions;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class MoveToAction : BaseTreeAction
    {
        [Parameter]
        public bool ContinuesUpdateTarget { get; set; }

        [Parameter]
        public ValueDefinitionKey MoveToPosisition { get; set; }

        private Vector3? _target;

        private Godot.Collections.Array _path;
        private CharacterMovementNode _characterMovementNode;
        public override void DoAction(float delta)
        {
            if (_path == null || _path.Count == 0)
            {
                FinishAction();
                return;
            }
            ChangeTargetIfNeeded();
            var nextPoint = (Vector3)_path[0];
            _characterMovementNode.MoveTo(nextPoint);
            if (GetSubjectOfTree<Spatial>().GlobalTransform.origin.EqualsWithMargin(_target.Value))
            {
                _path.RemoveAt(0);
            }

            if (_path.Count == 0)
            {
                FinishAction();
                return;
            }
        }

        private void ChangeTargetIfNeeded()
        {
            if (MoveToPosisition != null)
            {
                if (_target == null)
                {
                    _target = MoveToPosisition.GetValueAs<Vector3>();
                    CalculateNewPath();
                }
                else if (ContinuesUpdateTarget)
                {
                    var newPosition = MoveToPosisition.GetValueAs<Vector3>();
                    if (newPosition != _target)
                    {
                        _target = newPosition;
                        CalculateNewPath();
                    }
                }
            }
        }

        private void CalculateNewPath()
        {
            _path = new Godot.Collections.Array();
            _path.Add(_target);
        }

        protected override void Initialize()
        {
            _characterMovementNode = SubjectOfTree.GetChildOfType<CharacterMovementNode>();
            _target = null;
            if (MoveToPosisition != null)
            {
                ChangeTargetIfNeeded();
            }
        }
    }
}
