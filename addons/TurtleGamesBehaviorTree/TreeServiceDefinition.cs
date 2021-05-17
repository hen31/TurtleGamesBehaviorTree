using System;
using System.Collections.Generic;
using System.Reflection;

namespace TurtleGames.BehaviourTreePlugin
{
    public class TreeServiceDefinition
    {
        public string Name { get; set; }
        public string FullName { get; set; }

        public List<ParameterDefinition> ParameterDefinitions { get; } = new List<ParameterDefinition>();

        internal void AddParameter(PropertyInfo property, Attribute parameterAttribute)
        {
            ParameterDefinition parameterDefinition = new ParameterDefinition();
            parameterDefinition.Name = property.Name;
            parameterDefinition.SetTypeOfParameter(property.PropertyType);
            ParameterDefinitions.Add(parameterDefinition);
        }
    }
}