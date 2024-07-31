namespace Softsplit;

public partial class PlayerState
{
	public List<Thing> SpawnedThings { get; private set; } = new();

	private float undoPropHeldTimer = -2;
	private float undoPropRate = 1;

	public void AddPropToList( GameObject gameObject )
	{
		PlayerState.Thing thing = new PlayerState.Thing
		{
			gameObjects = new List<GameObject> { gameObject }
		};
		SpawnedThings.Add( thing );
	}

	protected void CheckPropUndo()
	{
		if ( Input.Pressed( "undo" ) )
		{
			undoPropRate = 1;
			undoPropHeldTimer = -2;
			HandlePropDestroyInitiation();
		}
		else if ( Input.Down( "undo" ) )
		{
			undoPropRate -= 0.0045f * Time.Delta;
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
		if ( SpawnedThings?.Count > 0 )
		{
			var lastObj = SpawnedThings.Last();
			using ( Rpc.FilterInclude( Connection.Host ) )
				DestroyLastSpawnedProp( lastObj );

			SpawnedThings?.RemoveAt( SpawnedThings.IndexOf( lastObj ) );
		}
	}

	[Broadcast]
	public void DestroyLastSpawnedProp( Thing propToDestroy )
	{
		if ( propToDestroy.gameObjects != null )
		{
			foreach ( GameObject g in propToDestroy.gameObjects )
			{
				g?.Destroy();
			}
		}
		if ( propToDestroy.components != null )
		{
			foreach ( Component c in propToDestroy.components )
			{
				if ( !c.IsValid() ) continue;

				GameObject gameObject = c.GameObject;
				gameObject.Network.TakeOwnership();
				c.Destroy();
				gameObject?.Network.DropOwnership();
			}
		}
	}

	public struct Thing
	{
		public List<GameObject> gameObjects { get; set; }
		public List<Component> components { get; set; }
	}
}
