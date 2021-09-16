using Godot;
using Godot.Collections;
using System;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin
{
    public class ParameterDefinition
    {
        public string Name { get; set; }
        public ParameterTypeDefinition ParameterType { get; set; }

        internal void SetTypeOfParameter(Type propertyType)
        {
            ParameterType = DetermineCLRTypeToParameterType(propertyType);
        }


        private ParameterTypeDefinition DetermineCLRTypeToParameterType(Type propertyType)
        {
            if (propertyType == typeof(float))
            {
                return ParameterTypeDefinition.Float;
            }
            else if (propertyType == typeof(Guid))
            {
                return ParameterTypeDefinition.Guid;
            }
            else if (propertyType == typeof(int))
            {
                return ParameterTypeDefinition.Int;
            }
            else if (propertyType == typeof(string))
            {
                return ParameterTypeDefinition.String;
            }
            else if (propertyType == typeof(bool))
            {
                return ParameterTypeDefinition.Bool;
            }
            else if (propertyType == typeof(Vector2))
            {
                return ParameterTypeDefinition.Vector2;
            }
            else if (propertyType == typeof(Godot.Collections.Array))
            {
                return ParameterTypeDefinition.Array;
            }
            else if (propertyType == typeof(Vector3))
            {
                return ParameterTypeDefinition.Vector3;
            }
            else if(propertyType == typeof(ValueDefinitionKey))
            {
                return ParameterTypeDefinition.ValueKey;
            }
            else
            {
                throw new Exception("Type not a valueDefinition");
            }
        }
    }
}