using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public class TreeValue
    {
        public TreeValue(string key, ValueTypeDefinition valueType)
        {
            Key = key;
            ValueType = valueType;
        }
        public string Key { get; private set; }
        public ValueTypeDefinition ValueType { get; }
        public object Value { get; set; }
    }

    public class TreeValue<T> : TreeValue
    {
        public TreeValue(string key, ValueTypeDefinition valueType) : base(key, valueType)
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
