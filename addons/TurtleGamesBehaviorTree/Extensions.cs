using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin
{

    public static class EnumExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name).GetCustomAttribute<TAttribute>();
        }

        public static T GetChildOfType<T>(this Node node) where T : Node
        {
            foreach (Node child in node.GetChildren())
            {
                if (child is T typedNode)
                {
                    return typedNode;
                }
                else if (child.GetChildCount() > 0)
                {
                    return GetChildOfType<T>(child);
                }
            }
            return null;
        }

        public static bool EqualsWithMargin(this Vector3 compare, Vector3 compareTo, float margin = 0.001f)
        {
            return Mathf.Abs((compare - compareTo).Length()) < margin;
        }

        public static bool EqualsWithMargin(this float compare, float compareTo, float margin = 0.001f)
        {
            return Mathf.Abs(compare - compareTo) < margin;
        }

        public static bool EqualsWithMargin(this Vector2 compare, Vector2 compareTo, float margin = 0.001f)
        {
            return Mathf.Abs((compare - compareTo).Length()) < margin;
        }
    }
}
