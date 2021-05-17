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

        public BehaviorTreeDefinition BehaviorTreeDefinition { get; private set; }

        private CompiledBehaviorTree _compiledBehavior;
        public override void _Ready()
        {
            base._Ready();
            if (!string.IsNullOrWhiteSpace(BehaviorTree))
            {
                Godot.File file = new File();
                if (file.Open(BehaviorTree, File.ModeFlags.Read) == Error.Ok)
                {
                    string json = file.GetAsText();
                    file.Close();
                    var jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
                    BehaviorTreeDefinition = JsonConvert.DeserializeObject<BehaviorTreeDefinition>(json, jsonSerializerSettings);
                }
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
                _compiledBehavior = BehaviorTreeDefinition.Compile(GetParent());
                _initialized = true;
                BehaviorTreeDefinition = null;
            }
            if (Play)
            {
                _compiledBehavior.Process(delta);
            }
        }

    }
}
