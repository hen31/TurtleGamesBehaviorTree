using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Actions;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Services
{
    public class SetPlayerSpeedService : BaseTreeService
    {
        private CharacterMovementNode _characterMovementNode;

        [Parameter]
        public float Speed { get; set; }

        public override void DoService(float delta)
        {
            _characterMovementNode.MaximumMovementSpeed = Speed;
        }

        protected override void Initialize()
        {
            _characterMovementNode = SubjectOfTree.GetChildOfType<CharacterMovementNode>();
        }
    }
}
