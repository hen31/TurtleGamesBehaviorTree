using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class BehaviorTreeValueDefinitionStorage
    {
        public BehaviorTreeValueDefinitionStorage()
        {
        }
        public BehaviorTreeValueDefinitionStorage(BehaviorTreeValueDefinition behaviorTreeValueDefinition)
        {
            Key = behaviorTreeValueDefinition.Key;
            ValueType = behaviorTreeValueDefinition.ValueType;
            DefaultValue = behaviorTreeValueDefinition.DefaultValue;
        }

        public string Key { get; set; }

        public ValueTypeDefinition ValueType { get; set; } = ValueTypeDefinition.String;

        public object DefaultValue { get; set; }

        internal BehaviorTreeValueDefinition ToValueDefinition()
        {
            BehaviorTreeValueDefinition behaviorTreeValueDefinition = new BehaviorTreeValueDefinition();
            behaviorTreeValueDefinition.Key = Key;
            behaviorTreeValueDefinition.ValueType = ValueType;
            behaviorTreeValueDefinition.DefaultValue = DefaultValue;
            return behaviorTreeValueDefinition;
        }
    }
}
