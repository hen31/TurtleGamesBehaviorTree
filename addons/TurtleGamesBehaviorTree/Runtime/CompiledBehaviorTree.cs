using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public class CompiledBehaviorTree
    {
        private List<TreeValue> _treeValues;
        public ReadOnlyCollection<TreeValue> TreeValues
        {
            get
            {
                return new ReadOnlyCollection<TreeValue>(_treeValues);
            }
        }

        public void AddTreeValue(string key, object defaultValue, ValueTypeDefinition valueType)
        {
            if (_treeValues == null)
            {
                _treeValues = new List<TreeValue>();
            }
            _treeValues.Add(new TreeValue(key, valueType) { Value = defaultValue });
        }

        internal TreeValue GetTreeValue(ValueDefinitionKey valueDefinitionKey)
        {
            return _treeValues.FirstOrDefault(b => b.Key == valueDefinitionKey.Key);
        }

        public CompiledNode RootNode { get; private set; }
        public Node SubjectOfTree { get; internal set; }

        public void Process(float delta)
        {
            RootNode.Process(delta);
        }

        internal void AddRootNode(CompiledRootNode compiledRootNode)
        {
            RootNode = compiledRootNode;
        }

        internal void SetTreeValue<T>(string key, T value)
        {
            var treeValue = _treeValues.FirstOrDefault(b => b.Key == key);
            treeValue.Value = value;
        }
    }
    public enum TreeExecutionState { InProgress, Completed, Failed }
}
