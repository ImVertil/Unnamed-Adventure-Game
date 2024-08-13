using System.Collections;
using UnityEngine;

namespace Managers.Coroutine
{
    public class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager Instance
        {
            get
            {
                if (Instance == null)
                {
                    GameObject obj = new GameObject("CoroutineManagerObject");
                    Instance = obj.AddComponent<CoroutineManager>();
                }
                return Instance;
            }
            private set
            {
                Instance = value;
            }
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}