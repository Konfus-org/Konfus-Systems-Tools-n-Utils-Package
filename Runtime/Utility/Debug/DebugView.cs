using UnityEngine;

namespace Konfus.Runtime.Utility.Debug
{
    public class DebugView : MonoBehaviour
    {
        private static string _myLog = "";
        private string _output;
        private string _stack;
        private bool _showStack;

        private void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                // Toggle debug view on/off
                _showStack = !_showStack;
            }
        }

        private void Log(string logString, string stackTrace, LogType type)
        {
            _output = logString;
            _stack = stackTrace;
            _myLog = _output + "\n" + _myLog;
            if (_myLog.Length > 5000)
            {
                // Log too long, remove old bits...
                _myLog = _myLog.Substring(0, 4000);
            }
        }

        private void OnGUI()
        {
            if (_showStack)
            {
                _myLog = GUI.TextArea(
                    position: new Rect(10, 10, Screen.width - 10, Screen.height - 10),
                    text: _myLog);
            }
        }
    }
}