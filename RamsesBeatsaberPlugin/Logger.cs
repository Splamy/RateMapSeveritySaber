using UnityEngine;

namespace RamsesBeatsaberPlugin
{
	static class Logger
	{
		public static void Log(string text)
		{
			Debug.Log($"-RaMSeS- {text}");
		}

		public static void Log(object objectData)
		{
			Log(objectData.ToString());
		}
	}
}
