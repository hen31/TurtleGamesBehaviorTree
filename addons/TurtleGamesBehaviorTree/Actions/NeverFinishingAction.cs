using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class NeverFinishingAction : BaseTreeAction
    {
        public override void DoAction(float delta)
        {
        }

        protected override void Initialize()
        {

        }
    }
}
