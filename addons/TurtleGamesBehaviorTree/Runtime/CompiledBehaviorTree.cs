using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes;
using TurtleGames.BehaviourTreePlugin.Storage;

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

        public void AddTreeValue(string key, object defaultValue, ValueTypeDefinition valueType, bool isKeyValue)
        {
            if (_treeValues == null)
            {
                _treeValues = new List<TreeValue>();
            }
            if (valueType == ValueTypeDefinition.Guid)
            {
                if (defaultValue is Guid guid)
                {
                    _treeValues.Add(new TreeValue(key, valueType, isKeyValue) { Value = guid });

                }
                else if (defaultValue is string guidString)
                {
                    _treeValues.Add(new TreeValue(key, valueType, isKeyValue) { Value = Guid.Parse(guidString) });
                }

            }
            else
            {
                _treeValues.Add(new TreeValue(key, valueType, isKeyValue) { Value = defaultValue });
            }
        }


       

        public BehaviorTreePlayer CurrentPlayer { get; internal set; }

        internal TreeValue GetTreeValue(ValueDefinitionKey valueDefinitionKey)
        {
            return _treeValues.FirstOrDefault(b => b.Key == valueDefinitionKey.Key);
        }

        internal TreeValue GetTreeValue(string key)
        {
            return _treeValues.FirstOrDefault(b => b.Key == key);
        }

        public CompiledNode RootNode { get; private set; }
        public Node SubjectOfTree { get; internal set; }
        public string DefinitionFile { get; internal set; }

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

        internal void SetLinkedValues(List<SubBehaviorTreeValueLink> valueDefinitionValues, CompiledBehaviorTree otherBehaviorTree)
        {
            foreach (var treeValueLinkDefinition in valueDefinitionValues)
            {
                if (treeValueLinkDefinition.KeepInSync)
                {
                    var fromValue = TreeValues.FirstOrDefault(b => b.Key == treeValueLinkDefinition.Name);
                    var otherValue = otherBehaviorTree.TreeValues.FirstOrDefault(b => b.Key == treeValueLinkDefinition.LinkedTo);
                    fromValue.OnValueChanged += (newValue) => otherValue.Value = newValue;
                }
                else
                {
                    var fromValue = TreeValues.FirstOrDefault(b => b.Key == treeValueLinkDefinition.Name);
                    var otherValue = otherBehaviorTree.TreeValues.FirstOrDefault(b => b.Key == treeValueLinkDefinition.LinkedTo);
                    if (fromValue != null && otherValue != null)
                    {
                        fromValue.Value = otherValue.Value;
                        Debug.WriteLine($"Set {treeValueLinkDefinition.Name} to {otherValue.Value.ToString()}");

                    }
                }
            }
        }

        internal void SetValues(Dictionary<string, object> initialValues, BehaviorTreePlayer treePlayer)
        {
            foreach (string key in initialValues.Keys)
            {
                TreeValue treeValue = TreeValues.First(b => b.Key == key);
                treeValue.SetValue(initialValues[key], treePlayer);
            }
        }
    }
    public enum TreeExecutionState { InProgress, Completed, Failed }
}
