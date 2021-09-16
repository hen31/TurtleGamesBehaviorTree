using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin
{
    public class BehaviorTreeValueDefinition : Godot.Object
    {

        public string Key { get; set; }

        public ValueTypeDefinition ValueType { get; set; } = ValueTypeDefinition.String;

        public object DefaultValue { get; set; }


    }

    public enum ValueTypeDefinition
    {
        [Description("Integer")] Int, [Description("String")] String, [Description("Float")] Float, [Description("Vector2")] Vector2, [Description("Vector3")] Vector3, [Description("Node reference")] NodeReference,
        Bool, [Description("Array")] Array, [Description("Guid")] Guid
    }

    public enum ParameterTypeDefinition
    {
        [Description("Integer")] Int, [Description("String")] String, [Description("Float")] Float, [Description("Vector2")] Vector2, [Description("Vector3")] Vector3, [Description("Value key")] ValueKey,
        Bool,
        [Description("Array")] Array, [Description("Guid")] Guid
    }

}
