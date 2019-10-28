using IllusionPlugin;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ramses
{
    public class RamsesPlugin : IPlugin
    {
        public string Name => "Ramses";
        public string Version => "0.0.1";
        public void OnApplicationStart()
        {
            Log("Ramses initialized.");
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1) {
            var levelDetailController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
            if (levelDetailController != null)
            {
                Log("LDC hooked");
                levelDetailController.didPresentContentEvent += (ldc, _) => {
                    Log($"Song name: {ldc?.selectedDifficultyBeatmap?.level?.songName}");
                    Log($"ID: {ldc?.selectedDifficultyBeatmap?.level?.levelID}");
                    Log($"Data: {Newtonsoft.Json.JsonConvert.SerializeObject(ldc?.selectedDifficultyBeatmap?.beatmapData?.beatmapLinesData?.Take(5))}");
                };
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) {}

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnLevelWasLoaded(int level) {}

        public void OnLevelWasInitialized(int level) {}

        public void OnUpdate() {}

        public void OnFixedUpdate() {}

        private void Log(string text)
        {
            Debug.Log($"-Ram- {text}");
        }
    }
}