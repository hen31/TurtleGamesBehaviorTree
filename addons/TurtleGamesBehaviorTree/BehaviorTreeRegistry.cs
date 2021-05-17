using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TurtleGames.BehaviourTreePlugin.Actions;
using TurtleGames.BehaviourTreePlugin.Runtime;

namespace TurtleGames.BehaviourTreePlugin
{
    public class BehaviorTreeRegistry
    {
        private static BehaviorTreeRegistry _instance;
        public static BehaviorTreeRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BehaviorTreeRegistry();
                    _instance.InitializeRegistry();
                }
                return _instance;
            }
        }

        private bool _initialized;
        public void InitializeRegistry()
        {
            if (!_initialized)
            {
                GatherAllActionsAndServices();
                _initialized = true;
            }
        }

        private void GatherAllActionsAndServices()
        {
            List<TreeActionDefinition> treeActions = new List<TreeActionDefinition>();
            List<TreeServiceDefinition> treeServices = new List<TreeServiceDefinition>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(BaseTreeAction)))
                {
                    TreeActionDefinition treeActionDefinition = new TreeActionDefinition();
                    treeActionDefinition.FullName = type.FullName;
                    treeActionDefinition.Name = type.Name;
                    treeActions.Add(treeActionDefinition);
                    foreach (var property in type.GetProperties())
                    {
                        var parameterAttribute = property.GetCustomAttribute(typeof(ParameterAttribute));
                        if (parameterAttribute != null)
                        {
                            treeActionDefinition.AddParameter(property, parameterAttribute);
                        }
                    }

                }
                else if (type.IsSubclassOf(typeof(BaseTreeService)))
                {
                    TreeServiceDefinition treeServiceDefinition = new TreeServiceDefinition();
                    treeServiceDefinition.FullName = type.FullName;
                    treeServiceDefinition.Name = type.Name;
                    treeServices.Add(treeServiceDefinition);
                    foreach (var property in type.GetProperties())
                    {
                        var parameterAttribute = property.GetCustomAttribute(typeof(ParameterAttribute));
                        if (parameterAttribute != null)
                        {
                            treeServiceDefinition.AddParameter(property, parameterAttribute);
                        }
                    }

                }
            }
            Debug.WriteLine($"Total action count {treeActions.Count}");
            Debug.WriteLine($"Total service count {treeServices.Count}");

            TreeServices = treeServices.ToArray();
            TreeActions = treeActions.ToArray();
        }

        public TreeActionDefinition[] TreeActions { get; set; }
        public TreeServiceDefinition[] TreeServices { get; set; }


    }
}
