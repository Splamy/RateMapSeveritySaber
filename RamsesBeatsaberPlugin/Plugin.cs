using IPA;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using RateMapSeveritySaber;

namespace RamsesBeatsaberPlugin
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RamsesPlugin : IBeatSaberPlugin
	{
		public string Name => "RaMSeS";
		public string Version => "1.0.5";

		public void OnApplicationStart()
		{
			RamsesFileParser.LoadCachedFiles();
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
					var difficulty = ldc?.selectedDifficultyBeatmap;
					var level = difficulty?.level;
					if (level != null)
					{
						Log($"Song name: {level.songName}");
						Log($"ID: {level.levelID}");

						RamsesFileParser.GetRamsesScore(level, difficulty);
					}
					else
					{
						Log("LDC has no selected level");
					}
				};
			}
		}

		public void OnApplicationQuit()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		}

		public void OnSceneUnloaded(Scene scene) {}
		public void OnUpdate() {}
		public void OnFixedUpdate() {}
		public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) {}

		public static void Log(string text)
		{
			Debug.Log($"-Ramses- {text}");
		}
	}
}