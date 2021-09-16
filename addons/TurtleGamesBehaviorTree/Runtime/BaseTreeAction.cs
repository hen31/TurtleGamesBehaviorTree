using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes;

namespace TurtleGames.BehaviourTreePlugin.Runtime
{
    public enum ActionState { Initializing, InProgress, Succeeded, Failed }
    public abstract class BaseTreeAction
    {
        public CompiledActionNode CurrentActionNode { get; set; }
        public Node SubjectOfTree { get; set; }

        public T GetSubjectOfTree<T>() where T : Node
        {
            return (T)SubjectOfTree;
        }

        public ActionState ActionState { get; protected set; }

        protected abstract void Initialize();

        public abstract void DoAction(float delta);

        public void InitializeAction()
        {
            ActionState = ActionState.Initializing;
            Initialize();
            ActionState = ActionState.InProgress;
        }


        public void FinishAction()
        {
            ActionState = ActionState.Succeeded;
        }

        public void FailAction()
        {
            ActionState = ActionState.Failed;
        }




    }
}
