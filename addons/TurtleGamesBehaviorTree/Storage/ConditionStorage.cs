using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    public class ConditionStorage
    {
        public string Key { get; set; }
        public ConditionOperator ConditionOperator { get; set; }

        public List<object> Values { get; set; } = new List<object>();
    }

    public enum ConditionOperator
    {
        [OperatorForValueType(ValueTypeDefinition.NodeReference, ValueTypeDefinition.Array, ValueTypeDefinition.Vector2, ValueTypeDefinition.Vector3, ValueTypeDefinition.String, ValueTypeDefinition.Bool)] IsSet,
        [OperatorForValueType(ValueTypeDefinition.NodeReference, ValueTypeDefinition.Array, ValueTypeDefinition.Vector2, ValueTypeDefinition.Vector3, ValueTypeDefinition.String, ValueTypeDefinition.Bool)] IsNotSet,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int)] LessThen,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int)] MoreThen,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int)] EqualOrLessThen,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int)] EqualOrMoreThen,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int, ValueTypeDefinition.String)] Equal,
        [OperatorForValueType(ValueTypeDefinition.Float, ValueTypeDefinition.Int, ValueTypeDefinition.String)] NotEqual,
    }

}
