using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Actions
{
    public class CreateRandomStringAction : BaseTreeAction
    {
        private  Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [Parameter]
        public ValueDefinitionKey MessageValueKey { get; set; }

        public override void DoAction(float delta)
        {
            MessageValueKey.SetValue<string>(RandomString(12));
            FinishAction();
        }

        protected override void Initialize()
        {

        }
    }
}
