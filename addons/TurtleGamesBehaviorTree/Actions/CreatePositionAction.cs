using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class CreatePositionAction : BaseTreeAction
    {
        private Random random = new Random();

        private Vector3[] _positions = new Vector3[]
        {
            new Vector3(2f,0f, 10f),
            new Vector3(4f,0f, 0f),
            new Vector3(9f,0f, 0f ),
            new Vector3(10f,0f, 14f),
            new Vector3(5f,0f, 2f)
        };

        [Parameter]
        public ValueDefinitionKey PosistionValueKey { get; set; }

        public override void DoAction(float delta)
        {
            PosistionValueKey.SetValue(_positions[random.Next(0, _positions.Length - 1)]);
            FinishAction();
        }

        protected override void Initialize()
        {

        }
    }
}
