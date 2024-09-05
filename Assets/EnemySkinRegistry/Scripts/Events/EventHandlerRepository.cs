using System.Collections.Generic;

namespace AntlerShed.SkinRegistry.Events
{
    class EnemyEventHandlerContainer
    {
        private Dictionary<EnemyAI, List<EnemyEventHandler>> eventHandlers = new Dictionary<EnemyAI, List<EnemyEventHandler>>();

        internal void RegisterEnemyEventHandler(EnemyAI instance, EnemyEventHandler handler)
        {
            if(instance != null && handler != null)
            {
                if(!eventHandlers.ContainsKey(instance))
                {
                    eventHandlers.Add(instance, new List<EnemyEventHandler>());
                }
                eventHandlers[instance].Add(handler);
            }
        }

        internal List<EnemyEventHandler> GetEventHandlers(EnemyAI instance)
        {
            if (instance != null )
            {
                if (!eventHandlers.ContainsKey(instance))
                {
                    eventHandlers.Add(instance, new List<EnemyEventHandler>());
                }
                if (eventHandlers[instance] != null)
                {
                    return eventHandlers[instance];
                }
            }
            return new List<EnemyEventHandler>();
        }

        internal void RemoveEnemyEventHandler(EnemyAI instance, EnemyEventHandler handler)
        {
            if (instance != null)
            {
                if(eventHandlers.ContainsKey(instance))
                {
                    eventHandlers[instance].Remove(handler);
                }
                
            }
        }
    }
}