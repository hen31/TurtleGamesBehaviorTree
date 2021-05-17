using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public static class CompileBehaviorTreeUtils
    {
        public static void SetParameterValueInProperty(object objectToRecieveValue, CompiledBehaviorTree compiledBehaviorTree, Type actionType, string parameterName, object parameterValue)
        {
            var property = actionType.GetProperty(parameterName);
            if (property != null)
            {
                if (property.PropertyType == typeof(ValueDefinitionKey))
                {
                    if (parameterValue != null)
                    {
                        ValueDefinitionKey valueDefinitionKey = new ValueDefinitionKey(parameterValue as string, compiledBehaviorTree);
                        parameterValue = valueDefinitionKey;
                    }
                }
                if (property.PropertyType == typeof(double) && parameterValue is float)
                {
                    parameterValue = Convert.ToDouble(parameterValue);
                }
                else if (property.PropertyType == typeof(float) && parameterValue is double)
                {
                    parameterValue = Convert.ToSingle(parameterValue);
                }
                property.SetValue(objectToRecieveValue, parameterValue);

            }
        }
    }
}
