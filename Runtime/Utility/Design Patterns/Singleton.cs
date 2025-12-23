using UnityEngine;

namespace Konfus.Utility.Design_Patterns
{
    /// <summary>
    /// Inherit from if you want a persistant singleton GameObject.
    /// A singleton enforces that only one instance of it is present at any given time, and that it isn't destroyed on
    /// load.
    /// So it will persist between scenes. If you do not want it to persist between scenes inherit from the
    /// <see cref="Singleton" /> class.
    /// </summary>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T? Instance { get; private set; }

        /// <summary>
        /// Use this for initialization, if no instance of singleton already then
        /// assigns this as the singleton instance else destroy this.
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                if (Instance == null)
                    Debug.LogError($"Could not create instance of {typeof(T)}");
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inherit from if you want a singleton GameObject.
    /// A singleton enforces that only one instance of it is present at any given time.
    /// So it will persist between scenes. If you want it to persist between scenes inherit from the
    /// <see cref="PersistentSingleton" /> class.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T? Instance { get; private set; }

        /// <summary>
        /// Use this for initialization, if no instance of singleton already then
        /// assigns this as the singleton instance else destroy this.
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                if (Instance == null)
                    Debug.LogError($"Could not create instance of {typeof(T)}");
            }
            else
                Destroy(gameObject);
        }
    }
}