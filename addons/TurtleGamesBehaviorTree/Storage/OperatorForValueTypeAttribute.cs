using System;

namespace TurtleGames.BehaviourTreePlugin.Storage
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public  class OperatorForValueTypeAttribute : Attribute
    {
        public ValueTypeDefinition[] ValidForValueTypes { get; set; }

        public OperatorForValueTypeAttribute(params ValueTypeDefinition[] validForValueTypes)
        {
            if(validForValueTypes == null)
            {
                ValidForValueTypes = new ValueTypeDefinition[0];
            }
            ValidForValueTypes = validForValueTypes;
        }

    }
}