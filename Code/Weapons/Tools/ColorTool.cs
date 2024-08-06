namespace Softsplit;

public sealed class ColorTool : ToolComponent
{
	Dictionary<GameObject, int> colorind = new Dictionary<GameObject, int>();
	List<Color> colorlist;
	protected override void Start()
	{
		ToolName = "Color";
		ToolDes = "ColorTool";
		colorlist = new List<Color>() { 
			Color.Red,
			Color.Blue,
			Color.Gray,
			Color.Green,
			Color.Yellow,
			Color.Orange,
			Color.Cyan,
			Color.Magenta,
			Color.White,
			Color.Black,
		};
	}
	protected override void PrimaryAction()
	{
		SceneTraceResult trace = Trace();
		if ( !trace.Hit )
			return;
		if ( !colorind.ContainsKey( trace.GameObject ) )
			colorind.Add( trace.GameObject, 0 );
		else
			colorind[trace.GameObject]=(colorind[trace.GameObject]+1)%colorlist.Count;
		trace.GameObject.Components.GetInChildrenOrSelf<ModelRenderer>().Tint = colorlist[colorind[trace.GameObject]];
	}
}
