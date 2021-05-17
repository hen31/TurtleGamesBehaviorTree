using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class PrintTextAction : BaseTreeAction
    {
        [Parameter]
        public string MessageText { get; set; }

        [Parameter]
        public ValueDefinitionKey MessageValueKey { get; set; }

        public override void DoAction(float delta)
        {
            if (!string.IsNullOrWhiteSpace(MessageText))
            {
                Debug.WriteLine(MessageText);
            }
            else if (MessageValueKey != null)
            {
                Debug.WriteLine(MessageValueKey.GetValueAs<string>());
            }
            else
            {
                Debug.WriteLine("Hello World");
            }
            FinishAction();
        }

        protected override void Initialize()
        {

        }
    }
}
