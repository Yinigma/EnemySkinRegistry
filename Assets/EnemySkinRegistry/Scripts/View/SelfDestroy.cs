using UnityEngine;

namespace AntlerShed.SkinRegistry.View
{
    public class SelfDestroy : MonoBehaviour
    {
        public void DestroySelf()
        {
            GameObject.Destroy(gameObject);
        }
    }
}
