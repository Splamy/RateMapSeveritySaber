using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static RamsesBeatsaberPlugin.Logger;

namespace RamsesBeatsaberPlugin
{
	public class MainApp : MonoBehaviour
	{
		public static MainApp Instance;
		public UI CurrentUI { get; set; }

		internal static void OnLoad()
		{
			if (Instance != null)
			{
				return;
			}

			new GameObject("RaMSeS Plugin").AddComponent<MainApp>();
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
			// We could alternatively load it via this...
			//var levelDetailController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
			//Log("Lets check " + levelDetailController);

			var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
			CurrentUI = new UI(flowCoordinator);

			CurrentUI.LevelDetailViewController.didPresentContentEvent += LevelDetailController_didPresentContentEvent;
			CurrentUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += LevelDetailController_didChangeDifficultyBeatmapEvent;

			Log("Hooked into flow");
		}

		private void LevelDetailController_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController ldc, IDifficultyBeatmap difficulty)
		{
			LoadSongAnalysis(difficulty);
		}

		private void LevelDetailController_didPresentContentEvent(StandardLevelDetailViewController ldc, StandardLevelDetailViewController.ContentType arg2)
		{
			if (ldc != null && ldc.selectedDifficultyBeatmap != null)
				LoadSongAnalysis(ldc.selectedDifficultyBeatmap);
		}

		private void LoadSongAnalysis(IDifficultyBeatmap difficulty)
		{
			var level = difficulty?.level;
			if (level != null)
			{
				Log($"Song name: {level.songName}");
				Log($"ID: {level.levelID}");

				var score = RamsesFileParser.GetRamsesScore(level, difficulty);
				CurrentUI.SetAnkhRating(score.Avg);
				Log($"Ankh Score (Avg): {score.Avg}");
			}
			else
			{
				CurrentUI.SetAnkhRating(null);
				Log("LDC has no selected level");
			}
		}
	}
}