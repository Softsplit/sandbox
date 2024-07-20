using Editor;
using Sandbox;

namespace Softsplit;

public class IconEditorPopup : PopupWidget
{
	public SerializedProperty Property { get; private set; }
	private readonly IconEditor _editor;

	public IconEditorPopup( Widget parent, SerializedProperty property ) : base( parent )
	{
		Property = property;
		MinimumSize = new Vector2( 375, 480 );

		_editor = new IconEditor( this );
		_editor.Size = MinimumSize;
		_editor.MinimumSize = _editor.Size;

		Layout = Layout.Column();
		Layout.Margin = 8;
		Layout.Add( _editor );
	}
}
