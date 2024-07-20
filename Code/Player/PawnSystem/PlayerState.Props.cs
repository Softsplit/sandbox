namespace Softsplit;

public partial class PlayerState
{
	public List<GameObject> SpawnedPropsList { get; private set; } = new();

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
		if ( SpawnedPropsList.Count > 0 )
		{
			DestroyLastSpawnedProp( SpawnedPropsList.Last() );
			SpawnedPropsList.RemoveAt( SpawnedPropsList.IndexOf( SpawnedPropsList.Last() ) );
		}
	}

	[Broadcast]
	public void DestroyLastSpawnedProp( GameObject propToDestroy )
	{
		propToDestroy?.Destroy();
	}
}