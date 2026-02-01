using System;
using HugsLib;
using UnityEngine;
using Verse;

namespace AllowTool;

public class ThingDesignatorDef : Def
{
	private class DeferredTextureResolver
	{
		private bool resolved;

		private Texture2D texture;

		public void ResolveTexture(string path, Action<Texture2D> onLoaded)
		{
			if (resolved)
			{
				onLoaded(texture);
				return;
			}
			HugsLibController.Instance.DoLater.DoNextUpdate(delegate
			{
				resolved = true;
				texture = ContentFinder<Texture2D>.Get(path);
				onLoaded(texture);
			});
		}
	}

	private readonly DeferredTextureResolver iconResolver = new DeferredTextureResolver();

	private readonly DeferredTextureResolver highlightResolver = new DeferredTextureResolver();

	public Type designatorClass;

	public string iconTex;

	public string dragHighlightTex;

	public SoundDef soundSucceeded = null;

	public KeyBindingDef hotkeyDef = null;

	public string messageSuccess = null;

	public string messageFailure = null;

	public void GetIconTexture(Action<Texture2D> onLoaded)
	{
		iconResolver.ResolveTexture(iconTex, onLoaded);
	}

	public void GetDragHighlightTexture(Action<Texture2D> onLoaded)
	{
		highlightResolver.ResolveTexture(dragHighlightTex, onLoaded);
	}

	public override void PostLoad()
	{
		Assert(designatorClass != null, "designatorClass field must be set");
		Assert(designatorClass != null && typeof(Designator_DefBased).IsAssignableFrom(designatorClass), "designatorClass must extend Designator_DefBased");
		Assert(iconTex != null, "icon texture must be set");
		Assert(dragHighlightTex != null, "drag highlight texture must be set");
	}

	private void Assert(bool check, string errorMessage)
	{
		if (!check)
		{
			Log.Error("[AllowTool] Invalid data in ThingDesignatorDef " + defName + ": " + errorMessage);
		}
	}
}
