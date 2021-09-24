using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Nodes
{
    public class BehaviorTreePlayer : Node
    {

        [Signal]
        public delegate void SubBehaviorTreePlayerCreated(BehaviorTreePlayer subBehaviorTreePlayer);

        private List<(Dictionary<string, object> TreeValues, Dictionary<string, object> KeyValues)> _keyValuesAndInitialValues = new List<(Dictionary<string, object> TreeValues, Dictionary<string, object> KeyValues)>();

        [Export(PropertyHint.File, "*.json")]
        public string BehaviorTree { get; set; }
        private bool _initialized = false;
        [Export]
        public bool Play { get; set; } = true;

        public bool Ended { get; private set; }
        public bool Result { get; private set; }

        public bool Stopping { get; private set; }
        public void StopBehaviorTree(bool succes)
        {
            Debug.WriteLine($"BehaviorTreeStopping({Name})");
            Debug.WriteLine("Rree ended: " + BehaviorTree);

            Stopping = true;
            Ended = true;
            Result = succes;
            QueueFree();
        }


        public BehaviorTreeDefinition BehaviorTreeDefinition { get; private set; }

        public CompiledBehaviorTree GetCompiledBehaviorTree()
        {
            return _compiledBehavior;
        }

        private CompiledBehaviorTree _compiledBehavior;
        private List<SubBehaviorTreeValueLink> _valueDefinitionValues;
        private CompiledBehaviorTree _otherBehaviorTree;
        private Dictionary<string, object> _initialValues;

        public override void _Ready()
        {
            base._Ready();
            if (!string.IsNullOrWhiteSpace(BehaviorTree) && BehaviorTreeDefinition == null)
            {
                BehaviorTreeDefinition = BehaviorTreeDefinition.LoadBehaviorTreeFromFile(BehaviorTree);
            }
        }

        public void SetBehaviorTreeValue<T>(string key, T value)
        {
            _compiledBehavior.SetTreeValue<T>(key, value);
        }

        public override void _Process(float delta)
        {
            if (!_initialized)
            {
                Node subject = GetParent();
                while (subject is BehaviorTreePlayer)
                {
                    subject = subject.GetParent();
                }
                _compiledBehavior = BehaviorTreeDefinition.Compile(subject, this);
                if (_valueDefinitionValues != null)
                {
                    _compiledBehavior.SetLinkedValues(_valueDefinitionValues, _otherBehaviorTree);
                }

                if (_initialValues != null)
                {
                    _compiledBehavior.SetValues(_initialValues, this);
                    _initialValues = null;
                }
                else if (_keyValuesAndInitialValues != null)
                {
                    foreach (var keyAndValues in _keyValuesAndInitialValues)
                    {
                        bool matching = true;
                        foreach (var key in keyAndValues.KeyValues)
                        {
                            var treeValue = _compiledBehavior.GetTreeValue(key.Key);
                            if (!treeValue.Value.Equals(treeValue.GetConvertedValue(key.Value, this)))
                            {
                                matching = false;
                                break;
                            }
                        }
                        if (matching)
                        {
                            _compiledBehavior.SetValues(keyAndValues.TreeValues, this);
                            _keyValuesAndInitialValues = null;
                            break;
                        }
                    }
                }
                _initialized = true;
            }

            if (Play && _initialized)
            {
                _compiledBehavior.Process(delta);
                /*  if (_compiledBehavior.RootNode.ExecutionState != TreeExecutionState.InProgress)
                  {
                      Debug.WriteLine("SubTree ended");
                      Play = false;
                      Ended = true;
                  }*/
            }

        }

        public bool GetInitialized()
        {
            return _initialized;
        }

        internal void SetLinkedValues(List<SubBehaviorTreeValueLink> valueDefinitionValues, CompiledBehaviorTree otherBehaviorTree)
        {
            _valueDefinitionValues = valueDefinitionValues;
            _otherBehaviorTree = otherBehaviorTree;
        }

        internal void SetInitialValues(Dictionary<string, object> treeValues)
        {
            _initialValues = treeValues;
        }

        internal void AddInitialValuesIfKeyValuesMatch(Dictionary<string, object> treeValues, Dictionary<string, object> keyValues)
        {
            _keyValuesAndInitialValues.Add((treeValues, keyValues));
        }

        public void SetTreeDefinition(BehaviorTreeDefinition subTreeDefinition)
        {
            BehaviorTreeDefinition = subTreeDefinition;
        }

        public void SetValueDefinitions()
        {

        }
    }
}
