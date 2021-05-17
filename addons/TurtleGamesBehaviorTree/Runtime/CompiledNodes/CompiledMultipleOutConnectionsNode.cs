using System;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public abstract class CompiledMultipleOutConnectionsNode : CompiledNode
    {
        public abstract void AbortAfterCurrent();
    }
}