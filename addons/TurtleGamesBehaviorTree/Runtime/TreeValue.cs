using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public class TreeValue
    {
        public TreeValue(string key, ValueTypeDefinition valueType, bool isKeyValue)
        {
            Key = key;
            ValueType = valueType;
            IsKeyValue = isKeyValue;
        }
        public string Key { get; private set; }
        public ValueTypeDefinition ValueType { get; }
        public bool IsKeyValue { get; }

        object _value;
        public object Value
        {
            get => _value; set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
        public Action<object> OnValueChanged { get; internal set; }

        internal void SetValue(object value, Nodes.BehaviorTreePlayer behaviorTreePlayer)
        {
            Value = GetConvertedValue(value, behaviorTreePlayer);
        }

        public object GetConvertedValue(object value, Nodes.BehaviorTreePlayer behaviorTreePlayer)
        {
            if (ValueType == ValueTypeDefinition.Guid)
            {
                if (value is Guid guid)
                {
                    return guid;

                }
                else if (value is string guidString)
                {
                    return Guid.Parse(guidString);
                }

                else
                {
                    return value;
                }
            }
            else if (ValueType == ValueTypeDefinition.NodeReference)
            {
                return null; //TODO: Generic find node
            }
            else
            {
                return value;
            }
        }
    }

    public class TreeValue<T> : TreeValue
    {
        public TreeValue(string key, ValueTypeDefinition valueType, bool isKeyValue) : base(key, valueType, isKeyValue)
        {
        }

        public T TypedValue
        {
            get
            {
                return (T)Value;
            }
            set
            {
                Value = value;
            }
        }
    }

}
