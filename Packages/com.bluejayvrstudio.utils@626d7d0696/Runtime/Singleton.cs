using System;
using UnityEngine;

namespace bluejayvrstudio
{
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly Lazy<T> LzInstance = new Lazy<T>(InstantiateSingleton);

        public static T Instance => LzInstance.Value;

        private static T InstantiateSingleton()
        {
            var Handle = new GameObject($"{typeof(T).Name} PersistentSingleton");
            var inst = Handle.AddComponent<T>();
            DontDestroyOnLoad(Handle);
            return inst;
        }
    }

    public class TempSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // singleton only in the current scene
        private static T inst;
        public static T CurrInst
        {
            get
            {
                if (inst == null)
                {
                    inst = GameObject.FindObjectOfType<T>();
                }

                return inst;
            }
        }
    }
}