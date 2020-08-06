using UnityEngine;
namespace Logger
{

    public class LoggerLink : MonoBehaviour
    {
        public Logger logger;

        private void Start()
        {
            Logger.Log(LogModeOptions.Minimal, $"LogMode Setting :: {logger.logMode}");
        }
    }
}
