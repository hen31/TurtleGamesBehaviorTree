using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public abstract class BaseTreeService
    {

        public Node SubjectOfTree { get; set; }

        public T GetSubjectOfTree<T>() where T : Node
        {
            return (T)SubjectOfTree;
        }

        protected abstract void Initialize();

        public abstract void DoService(float delta);

        public void InitializeService()
        {
            Initialize();
        }



     

    }
}
