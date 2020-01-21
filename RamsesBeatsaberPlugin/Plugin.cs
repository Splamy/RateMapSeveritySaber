using IPA;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace Ramses
{
	public class RamsesPlugin : IBeatSaberPlugin
	{
		public string Name => "RaMSeS";
		public string Version => "1.0.5";

		public void OnApplicationStart()
		{
			Log("Ramses initialized.");
		}

		public void OnActiveSceneChanged(Scene arg0, Scene arg1)
		{
			var levelDetailController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
			if (levelDetailController != null)
			{
				Log("LDC hooked");
				levelDetailController.didPresentContentEvent += (ldc, _) =>
				{
					Log($"Song name: {ldc?.selectedDifficultyBeatmap?.level?.songName}");
					Log($"ID: {ldc?.selectedDifficultyBeatmap?.level?.levelID}");
					Log($"Data: {JsonConvert.SerializeObject(ldc?.selectedDifficultyBeatmap?.beatmapData?.beatmapLinesData?.Take(5))}");
				};
			}
		}

		public void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) { }

		public void OnSceneUnloaded(Scene scene)
		{
			throw new System.NotImplementedException();
		}

		public void OnApplicationQuit()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void OnLevelWasLoaded(int level) { }

		public void OnLevelWasInitialized(int level) { }

		public void OnUpdate() { }

		public void OnFixedUpdate() { }

		private void Log(string text)
		{
			Debug.Log($"-Ram- {text}");
		}
	}
}