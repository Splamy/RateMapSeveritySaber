using BS_Utils.Utilities;
using IPA;
using System;
using UnityEngine.SceneManagement;
using static RamsesBeatsaberPlugin.Logger;

namespace RamsesBeatsaberPlugin
{
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

		public void OnActiveSceneChanged(Scene arg0, Scene arg1) { }

		public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }

		public void OnSceneUnloaded(Scene scene) { }

		public void OnApplicationQuit() { }

		public void OnUpdate() { }

		public void OnFixedUpdate() { }
	}
}
