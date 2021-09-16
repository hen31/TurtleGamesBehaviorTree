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
            Stopping = true;
            Ended = true;
            Result = succes;
            QueueFree();
        }


        public BehaviorTreeDefinition BehaviorTreeDefinition { get; private set; }

        private CompiledBehaviorTree _compiledBehavior;
        private List<SubBehaviorTreeValueLink> _valueDefinitionValues;
        private CompiledBehaviorTree _otherBehaviorTree;

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
                _initialized = true;
            }
            if (Play)
            {
                _compiledBehavior.Process(delta);
                if (_compiledBehavior.RootNode.ExecutionState != TreeExecutionState.InProgress)
                {
                    Debug.WriteLine("SubTree ended");
                    Play = false;
                    Ended = true;
                }
            }
        }

        internal void SetLinkedValues(List<SubBehaviorTreeValueLink> valueDefinitionValues, CompiledBehaviorTree otherBehaviorTree)
        {
            _valueDefinitionValues = valueDefinitionValues;
            _otherBehaviorTree = otherBehaviorTree;
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
