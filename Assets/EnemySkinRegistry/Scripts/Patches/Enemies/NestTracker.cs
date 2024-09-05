using UnityEngine;

namespace AntlerShed.SkinRegistry
{ 
    class NestTracker : MonoBehaviour
    {
        public void OnDestroy()
        {
            //Praise be composition over inheritence
            EnemySkinRegistry.RemoveSkinner(gameObject);
        }
    }
}
