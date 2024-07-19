using Softsplit;

namespace Tools;
public class Tool : Equipment, IDescription, IEquipment
{
	/*
	[Sync]
	public Tool Parent { get; set; }
	*/
	protected virtual float MaxTraceDistance => 10000.0f;
	// IDescription
	string IDescription.DisplayName => "data";

	protected override void OnStart()
	{
		// Resource = new EquipmentResource { };
		if ( Connection.Local.IsHost )
		{
			// CreatePreviews();
		}
		base.OnStart();
	}
	/*
	public virtual void Deactivate()
	{
		// DeletePreviews();
	}

	public virtual void OnDraw()
	{
		// UpdatePreviews();
	}
	public virtual void CreateHitEffects( Vector3 pos )
	{
		// Parent?.CreateHitEffects( pos );
	}*/
	public bool IsUsing()
	{
		return Owner != null && Owner.CurrentEquipment == this;
	}
	/*
	System.NullReferenceException: Object reference not set to an instance of an object.
	at Tools.Tool.DoTrace() in C:\Users\sony\Documents\s&box projects\sbox-classic\sbox-classic\Code\Tools\Tool.cs:line 45
	at Physgun_Classic.OnPreRender() in C:\Users\sony\Documents\s&box projects\sbox-classic\sbox-classic\Code\Tools\Physgun_Classic.cs:line 151
	at Sandbox.Component.ExceptionWrap(String name, Action a)
	*/
	public SceneTraceResult DoTrace()
	{
		return Scene.Trace.Ray( Owner.AimRay, MaxTraceDistance )
			.WithoutTags( "player" )
			.IgnoreGameObject( Owner.GameObject )
			.Run();
	}
}
