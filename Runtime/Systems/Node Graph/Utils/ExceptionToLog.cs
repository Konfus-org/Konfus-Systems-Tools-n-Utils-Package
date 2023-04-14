using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    public static class ExceptionToLog
    {
        public static void Call(Action a)
        {
#if UNITY_EDITOR
            try
            {
#endif
                a?.Invoke();
#if UNITY_EDITOR
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
        }
    }
}