namespace Softsplit;

public sealed class Duplicator : ToolComponent
{
	JsonObject storedObject;

	protected override void Start()
	{
		ToolName = "Duplicator";
		ToolDes = "Duplicate Creations.";
	}

	protected override void PrimaryAction()
	{
		var hit = Trace();
		if ( hit.Hit )
		{
			Recoil( hit.EndPosition );
			SpawnObject( storedObject?.ToJsonString(), hit.EndPosition + Vector3.Up * 50, Rotation.LookAt( Equipment.Owner.Transform.World.Forward ) );
		}
	}

	protected override void SecondaryAction()
	{
		var hit = Trace();
		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			Recoil( hit.EndPosition );

			GameObject copied = new();
			copied.Transform.Position = hit.EndPosition;

			List<GameObject> weldConnections = PhysGunComponent.GetAllConnectedWelds( hit.GameObject );
			if ( weldConnections?.Count > 1 )
			{
				for ( int i = 0; i < weldConnections.Count; i++ )
				{
					if ( !copied.Children.Contains( weldConnections[i] ) )
					{
						weldConnections[i].SetParent( copied );
					}
				}
			}
			else
			{
				hit.GameObject.SetParent( copied );
			}

			storedObject = copied.Serialize();

			while ( copied.Children.Count > 0 )
			{
				copied.Children[0].SetParent( Scene );
			}

			copied.Destroy();
		}
	}

	[Broadcast]
	public static void SpawnObject( string gameObjectText, Vector3 position, Rotation rotation )
	{
		if ( !Networking.IsHost )
			return;

		if ( gameObjectText == null ) return;

		JsonObject gameObject = Json.Deserialize<JsonObject>( gameObjectText );

		SceneUtility.MakeIdGuidsUnique( gameObject );

		GameObject newObject = new();
		newObject.Deserialize( gameObject );
		newObject.Transform.Position = position;
		newObject.Transform.Rotation = rotation;

		PlayerState.Thing thing = new()
		{
			gameObjects = new List<GameObject>()
		};

		while ( newObject.Children.Count > 0 )
		{
			GameObject go = newObject.Children[0];
			go.SetParent( Game.ActiveScene );
			go.NetworkSpawn();

			thing.gameObjects.Add( go );
		}

		Log.Info( thing.gameObjects.Count );

		newObject.Destroy();
	}
}
