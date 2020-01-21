using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HMUI;
using System.Reflection;
using Image = UnityEngine.UI.Image;
using BS_Utils.Utilities;
using TMPro;

namespace RamsesBeatsaberPlugin
{
	public class UI
	{
		public static Sprite AnkhIcon;

		private RectTransform _ankhRatingButton;

		public StandardLevelDetailViewController LevelDetailViewController { get; set; }

		static UI()
		{
			AnkhIcon = LoadSpriteFromResources("RamsesBeatsaberPlugin.Assets.Ankh.png");
		}

		public UI(FlowCoordinator flowCoordinator)
		{
			Initialize(flowCoordinator);
		}

		private void Initialize(FlowCoordinator flowCoordinator)
		{
			var statsPanel = ExtractPanel(flowCoordinator);
			var statTransforms = statsPanel.GetComponentsInChildren<RectTransform>();

			_ankhRatingButton = UnityEngine.Object.Instantiate(statTransforms[1], statsPanel.transform, false);
			SetStatButtonIcon(_ankhRatingButton, AnkhIcon);
			DestroyHoverHint(_ankhRatingButton);
		}

		// Ramses

		public void SetAnkhRating(float? ankhRating)
		{
			if (_ankhRatingButton == null) return;
			SetStatButtonText(_ankhRatingButton, ankhRating?.ToString("F2") ?? "N/A");
		}

		// BS UI

		private static void SetStatButtonIcon(RectTransform rect, Sprite icon)
		{
			Image img = rect.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "Icon");
			if (img != null)
			{
				img.sprite = icon;
				img.color = Color.white;
			}
		}

		private static void DestroyHoverHint(RectTransform button)
		{
			HoverHint currentHoverHint = button.GetComponentsInChildren<HMUI.HoverHint>().First();
			if (currentHoverHint != null)
			{
				UnityEngine.GameObject.DestroyImmediate(currentHoverHint);
			}
		}

		private static void SetStatButtonText(RectTransform rect, String text)
		{
			var txt = rect.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(x => x.name == "ValueText");
			if (txt != null)
			{
				txt.text = text;
			}
		}

		private LevelParamsPanel ExtractPanel(FlowCoordinator flowCoordinator)
		{
			var LevelSelectionFlowCoordinator = flowCoordinator;
			var LevelSelectionNavigationController = LevelSelectionFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
			LevelDetailViewController = LevelSelectionNavigationController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
			var StandardLevelDetailView = LevelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
			return StandardLevelDetailView.GetPrivateField<LevelParamsPanel>("_levelParamsPanel");
		}

		// Sprite stuff

		public static Sprite LoadSpriteFromResources(string resourcePath, float PixelsPerUnit = 100.0f)
		{
			return LoadSpriteRaw(GetResource(Assembly.GetCallingAssembly(), resourcePath), PixelsPerUnit);
		}

		public static Sprite LoadSpriteRaw(byte[] image, float PixelsPerUnit = 100.0f)
		{
			return LoadSpriteFromTexture(LoadTextureRaw(image), PixelsPerUnit);
		}

		public static Sprite LoadSpriteFromTexture(Texture2D SpriteTexture, float PixelsPerUnit = 100.0f)
		{
			if (SpriteTexture)
				return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
			return null;
		}

		public static Texture2D LoadTextureRaw(byte[] file)
		{
			if (file.Count() > 0)
			{
				var Tex2D = new Texture2D(2, 2);
				if (Tex2D.LoadImage(file))
					return Tex2D;
			}
			return null;
		}

		public static byte[] GetResource(Assembly asm, string ResourceName)
		{
			System.IO.Stream stream = asm.GetManifestResourceStream(ResourceName);
			byte[] data = new byte[stream.Length];
			stream.Read(data, 0, (int)stream.Length);
			return data;
		}
	}
}
