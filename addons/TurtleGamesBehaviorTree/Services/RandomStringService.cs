using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Actions;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin.Services
{
    public class RandomStringService : BaseTreeService
    {
        private string _allowedCharacters = "qtyuiopasdfghjklzxcvbnm";
        private Random _random = new Random();

        [Parameter]
        public ValueDefinitionKey StringValueKey { get; set; }

        [Parameter]
        public int StringSize { get; set; }

        public override void DoService(float delta)
        {
            StringValueKey.SetValue(GenerateRandomValue());
        }

        private string GenerateRandomValue()
        {
            string newValue = "";
            for (int i = 0; i < StringSize; i++)
            {
                newValue += _allowedCharacters[_random.Next(0, _allowedCharacters.Length - 1)];
            }
            return newValue;
        }

        protected override void Initialize()
        {
        }
    }
}
