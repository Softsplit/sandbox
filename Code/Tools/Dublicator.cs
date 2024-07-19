using Sandbox;
using System.Text.Json.Nodes;

namespace Tools
{
	public struct SavedObject
	{
		public string Name { get; set; }
		public string ModelName { get; set; }
		public Vector3 Scale { get; set; }
	}

	public class DublicatorTool : Tool
	{
		SavedObject Saved_Object;

		protected override async void OnUpdate()
		{
			if ( !IsUsing() )
				return;
			var aim = DoTrace();
			GameObject picker = aim.GameObject;
			if ( picker != null && !Owner.IsProxy && aim.Body.BodyType == PhysicsBodyType.Dynamic )
			{
				if ( Input.Pressed( "attack2" ) )
				{
					var prop = picker.Components.Get<Prop>();
					if ( prop != null )
					{
						Saved_Object = new SavedObject
						{
							ModelName = prop.Model.Name,
							Name = picker.Name,
							Scale = picker.Transform.Scale
						};
						Log.Info( Saved_Object );
					}
				}
			}

			if ( Input.Pressed( "attack1" ) )
			{
				Log.Info( Saved_Object );
				var newObject = await Softsplit.GameMode.Spawn( Saved_Object.ModelName );
				if ( newObject != null )
				{
					newObject.Transform.Scale = Saved_Object.Scale;
					// newObject.Transform.Position = aim.EndPosition;
				}
			}
		}

		/*
		static public void Dublicate(SceneTraceResult aim, Player Player)
		{
			GameObject picker = aim.GameObject;
			if (picker != null && Player.isMe && aim.Body.BodyType == PhysicsBodyType.Dynamic)
			{
				if (Input.Pressed("attack2"))
				{
					List<GameObject> gameObjects = RemoveTool.TraceConnections(picker);
					GameObject main = new GameObject();
					main.Transform.Position = aim.EndPosition;
					foreach (var item in gameObjects)
					{
						item.Parent = main;
						// item.Transform.Position -= main.Transform.Position;
					}
					Player.test = main.Serialize();
				}
			}

			if (Input.Pressed("attack1"))
			{
				Log.Info(Player.test);
				GameObject objectq = new GameObject();
				objectq.Name = "BackUpped";
				objectq.Deserialize(Player.test);
				objectq.Transform.Position = aim.EndPosition;
				objectq.NetworkSpawn(Player.Network.OwnerConnection);
			}
		}
		*/
	}
}
