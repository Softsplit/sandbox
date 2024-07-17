﻿using Editor;
using Sandbox;

public class HideObject
{
	[Menu("Editor", "Settings/Sandbox/ToggleSelection", Shortcut = "h" )]
	static void ToggleSelection()
	{
		using var scope = SceneEditorSession.Scope();

		foreach ( var item in EditorScene.Selection )
		{
			var thing = item as GameObject;

			thing.Enabled = !thing.Enabled;
		}

		SceneEditorSession.Active.FullUndoSnapshot( "Toggle Selection" );
	}
}
