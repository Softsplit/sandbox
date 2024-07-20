namespace Softsplit;

public partial class PlayerState
{
	public List<Thing> SpawnedThings { get; private set; } = new();

	private float undoPropHeldTimer = 0;
	private float undoPropRate = 1;

	protected void CheckPropUndo()
	{
		if ( Input.Pressed( "undo" ) )
		{
			undoPropRate = 1;
			undoPropHeldTimer = 0;
			HandlePropDestroyInitiation();
		}
		else if ( Input.Down( "undo" ) )
		{
			undoPropRate -= 0.0045f;
			undoPropHeldTimer += 0.04f;
			if ( undoPropHeldTimer > undoPropRate )
			{
				HandlePropDestroyInitiation();
				undoPropHeldTimer = 0;
			}
		}
	}

	private void HandlePropDestroyInitiation()
	{
		if ( SpawnedThings.Count > 0 )
		{
			DestroyLastSpawnedProp( SpawnedThings.Last() );
			SpawnedThings.RemoveAt( SpawnedThings.IndexOf( SpawnedThings.Last() ) );
		}
	}

	[Broadcast]
	public void DestroyLastSpawnedProp( Thing propToDestroy )
	{
		if(propToDestroy.gameObject != null) propToDestroy?.gameObject.Destroy();
		if(propToDestroy.component != null) propToDestroy?.component.Destroy();
	}

	public class Thing
	{
		public GameObject gameObject {get;set;}
		public Component component {get;set;}
	}
}