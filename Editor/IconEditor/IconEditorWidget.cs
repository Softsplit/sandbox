using Editor;
using Sandbox;

namespace Softsplit;

[CustomEditor( typeof( IconSettings ) )]
public class IconEditorWidget : ControlWidget
{
	public Color HighlightColor = Theme.Yellow;

	public IconEditorWidget( SerializedProperty property ) : base( property )
	{
		SetSizeMode( SizeMode.Default, SizeMode.Default );

		Layout = Layout.Column();
		Layout.Spacing = 2;
		Cursor = CursorShape.Finger;
	}

	protected override Vector2 SizeHint() => new Vector2( 10000, 22 );

	protected override void PaintOver()
	{
		var rect = LocalRect.Shrink( 8, 0 );
		var alpha = Paint.HasMouseOver ? 1f : 0.7f;

		Paint.SetBrush( Theme.ButtonDefault.Darken( 0.2f ) );
		Paint.DrawRect( LocalRect, 2 );

		// icon
		{
			Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
			var r = Paint.DrawIcon( rect, "track_changes", 17, TextFlag.LeftCenter );
			rect.Left += r.Width + 8;
		}

		Paint.SetPen( Theme.ControlText.WithAlphaMultiplied( alpha ) );
		Paint.DrawText( rect, "Open Icon Editor", TextFlag.LeftCenter );
	}

	protected override void OnMousePress( MouseEvent e )
	{
		base.OnMousePress( e );

		if ( e.LeftMouseButton )
		{
			var editor = new IconEditorPopup( this, SerializedProperty );
			editor.Visible = true;
			editor.Position = e.ScreenPosition - editor.Size * new Vector2( 1, 0.5f );
			editor.ConstrainToScreen();
		}
	}
}
