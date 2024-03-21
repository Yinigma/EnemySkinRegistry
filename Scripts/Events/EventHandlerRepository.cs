using AntlerShed.EnemySkinKit.Events;
using System.Collections.Generic;
using UnityEngine;

namespace AntlerShed.SkinRegistry.Events
{
    class EnemyEventHandlerContainer
    {
        private Dictionary<EnemyAI, EnemyEventHandler> eventHandlers = new Dictionary<EnemyAI, EnemyEventHandler>();

        internal void RegisterEnemyEventHandler(EnemyAI instance, EnemyEventHandler handler)
        {
            if(instance != null && handler != null)
            {
                eventHandlers.Add(instance, handler);
            }
        }

        internal EnemyEventHandler GetEventHandler(EnemyAI instance)
        {
            return instance != null && eventHandlers.ContainsKey(instance) ? eventHandlers[instance] : null;
        }

        internal void RemoveBrackenEventHandler(EnemyAI instance)
        {
            if (instance != null)
            {
                eventHandlers.Remove(instance);
            }
        }
    }
}