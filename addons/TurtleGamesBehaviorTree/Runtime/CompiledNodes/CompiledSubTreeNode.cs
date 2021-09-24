using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Nodes;
using TurtleGames.BehaviourTreePlugin.Storage;

namespace TurtleGames.BehaviourTreePlugin.Runtime.CompiledNodes
{
    public class CompiledSubTreeNode : CompiledNode
    {

        BehaviorTreePlayer _subBehaviorTreePlayer;
        BehaviorTreePlayer _parentBehaviorTreePlayer;
        private SubBehaviorTreeNodeDefinition _subBehaviorTreeStorage;
        private BehaviorTreeDefinition _subTreeDefinition;
        private CompiledBehaviorTree _compiledBehaviorTree;
        private string _file;
        public override void CompileFromDefinition(BehaviorTreeDefinition behaviorTreeDefinition, BehaviorTreeNodeDefinition currentNode, CompiledBehaviorTree compiledBehaviorTree)
        {
            _parentBehaviorTreePlayer = compiledBehaviorTree.CurrentPlayer;
            _subBehaviorTreeStorage = currentNode as SubBehaviorTreeNodeDefinition;
            _subTreeDefinition = BehaviorTreeDefinition.LoadBehaviorTreeFromFile(_subBehaviorTreeStorage.SelectedSubTree);
            _file = _subBehaviorTreeStorage.SelectedSubTree;
            _compiledBehaviorTree = compiledBehaviorTree;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (_subBehaviorTreePlayer?.Ended == false && _subBehaviorTreePlayer?.Stopping == false)
            {
                _subBehaviorTreePlayer?.QueueFree();

            }
            _subBehaviorTreePlayer = new BehaviorTreePlayer
            {
                Name = _file,
                BehaviorTree = _file
            };
            _subBehaviorTreePlayer.SetTreeDefinition(_subTreeDefinition);
            _subBehaviorTreePlayer.SetLinkedValues(_subBehaviorTreeStorage.ValueDefinitionValues, _compiledBehaviorTree);
            GetTopMostBehaviorTreePlayer().EmitSignal(nameof(BehaviorTreePlayer.SubBehaviorTreePlayerCreated), _subBehaviorTreePlayer);
            _parentBehaviorTreePlayer.AddChild(_subBehaviorTreePlayer);
        }

        private BehaviorTreePlayer GetTopMostBehaviorTreePlayer()
        {
            var parent = _parentBehaviorTreePlayer.GetParent();
            if (parent is BehaviorTreePlayer)
            {
                while (parent.GetParent() is BehaviorTreePlayer parentAsBehaviorTree)
                {
                    parent = parentAsBehaviorTree;
                }
                return parent as BehaviorTreePlayer;
            }
            else
            {
                return _parentBehaviorTreePlayer;
            }
        }

        public override void Process(float delta)
        {
            if (_subBehaviorTreePlayer.Ended)
            {
                Debug.WriteLine("Sub tree ended: " + _file);
                FinishExecution();
            }
        }
    }
}
