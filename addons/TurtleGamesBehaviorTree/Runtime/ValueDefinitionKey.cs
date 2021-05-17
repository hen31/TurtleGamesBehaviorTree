using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public class ValueDefinitionKey
    {
        public ValueDefinitionKey(string key, CompiledBehaviorTree compiledBehaviorTree)
        {
            Key = key;
            CompiledBehaviorTree = compiledBehaviorTree;
        }
        public string Key { get; private set; }

        public CompiledBehaviorTree CompiledBehaviorTree { get; private set; }
        private TreeValue _treeValue;
        public T GetValueAs<T>()
        {
            return (T)GetValue();
        }

        public void SetValue<T>(T value)
        {
            if (_treeValue == null)
            {
                _treeValue = CompiledBehaviorTree.GetTreeValue(this);
            }
            _treeValue.Value = value;
        }

        public object GetValue()
        {
            if (_treeValue == null)
            {
                _treeValue = CompiledBehaviorTree.GetTreeValue(this);
            }
            return _treeValue.Value;
        }
    }
}
