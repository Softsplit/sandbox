namespace Softsplit;

public partial class PlayerState
{
	public List<GameObject> SpawnedPropsList { get; set; } = new();

	private float undoPropHeldTimer = 0;

	protected void CheckPropUndo()
	{
		if ( Input.Pressed( "undo" ) )
		{
			undoPropHeldTimer = 0;
			if ( SpawnedPropsList.Count > 0 )
			{
				DestroyLastSpawnedProp( SpawnedPropsList.Last() );
				SpawnedPropsList.RemoveAt( SpawnedPropsList.IndexOf( SpawnedPropsList.Last() ) );
			}
		}
		else if ( Input.Down( "undo" ) )
		{
			undoPropHeldTimer += 0.1f;
			if ( undoPropHeldTimer > 1 )
				if ( SpawnedPropsList.Count > 0 )
				{
					DestroyLastSpawnedProp( SpawnedPropsList.Last() );
					SpawnedPropsList.RemoveAt( SpawnedPropsList.IndexOf( SpawnedPropsList.Last() ) );
					undoPropHeldTimer = 0;
				}
		}
	}

	[Broadcast]
	public void DestroyLastSpawnedProp( GameObject propToDestroy )
	{
		propToDestroy?.Destroy();
	}
}
