using IPA;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;
using BS_Utils.Utilities;
using System.Reflection;

namespace RamsesBeatsaberPlugin
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RamsesPlugin : IBeatSaberPlugin
	{
		public string Name => "RaMSeS";
		public string Version => "1.0.5";

		public void OnApplicationStart()
		{
			BSEvents.menuSceneLoadedFresh += OnMenuSceneLoadedFresh;
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

						var score = RamsesFileParser.GetRamsesScore(level, difficulty);
						UI.SetAnkhRating(score.Avg);
						Log($"avg Score: {score.Avg}");
					}
					else
					{
						Log("LDC has no selected level");
					}
				};
			}
		}

		private void OnMenuSceneLoadedFresh()
		{
			try
			{
				MainApp.OnLoad();
			}
			catch (Exception e)
			{
				Log("Exception on fresh menu scene change: " + e);
			}
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }

		public void OnSceneUnloaded(Scene scene) { }

		public void OnApplicationQuit() { }

		public void OnUpdate() { }

		public void OnFixedUpdate() { }

		public static void Log(string text)
		{
			Debug.Log($"-RaMSeS- {text}");
		}

		public static void Log(object objectData)
		{
			Log(objectData.ToString());
		}
	}

	class MainApp : MonoBehaviour
	{
		public static MainApp Instance;

		internal static void OnLoad()
		{
			if (Instance != null)
			{
				return;
			}

			new GameObject("RaMSeS Plugin").AddComponent<MainApp>();

			Console.WriteLine("SongBrowser Plugin Loaded()");
		}

		private void Awake()
		{
			Instance = this;
		}

		public void Start()
		{
			Button soloFreePlayButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloFreePlayButton");
			soloFreePlayButton.onClick.AddListener(HandleSoloModeSelection);
		}

		private void HandleSoloModeSelection()
		{
			var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
			UI.Initialize(flowCoordinator);
		}
	}
}