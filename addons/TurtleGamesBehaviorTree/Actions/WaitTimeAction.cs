using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class WaitTimeAction : BaseTreeAction
    {
        [Parameter]
        public float WaitFor { get; set; } = 10f;
        private float _waitedTime = 0f;
        public override void DoAction(float delta)
        {
            _waitedTime += delta;
            if (_waitedTime > WaitFor)
            {
                Debug.Write($"waited for {_waitedTime} {WaitFor}");
                FinishAction();
            }
        }

        protected override void Initialize()
        {
            _waitedTime = 0f;
            Debug.Write($"Reset waited time");

        }
    }
}
