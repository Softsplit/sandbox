using System.Text.Json.Nodes;
using Sandbox.Events;
using Sandbox.Physics;

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
        if(hit.Hit)
        {
            Recoil(hit.EndPosition);
            Log.Info(storedObject.ToJsonString());
            SpawnObject(PlayerState.Local.PlayerPawn, storedObject,hit.EndPosition + Vector3.Up*50,Rotation.LookAt(Equipment.Owner.Transform.World.Forward));
        }
    }
	protected override void SecondaryAction()
	{
        var hit = Trace();
        if(hit.Hit && hit.GameObject.Name != "Map")
        {
            Recoil(hit.EndPosition);
            GameObject copied = new GameObject();
            copied.Transform.Position = hit.EndPosition;

            WeldContext weldContext = hit.GameObject.Components.Get<WeldContext>();

            if(weldContext != null)
            {
                List<WeldContext> weldContexts = PhysGunComponent.GetAllConnectedWelds(weldContext);

                for(int i = 0; i < weldContexts.Count; i++)
                {
                    if(!copied.Children.Contains(weldContexts[i].GameObject))
                    {
                        weldContexts[i].GameObject.SetParent(copied);
                    }
                }
            }
            else
            {
                hit.GameObject.SetParent(copied);
            }

            storedObject = copied.Serialize();
            
            
            while(copied.Children.Count>0)
            {
                copied.Children[0].SetParent(Scene);
            }

            copied.Destroy();
            
            
        }
	}

    [Broadcast]
    public static void SpawnObject(PlayerPawn owner, JsonObject gameObject, Vector3 position, Rotation rotation)
    {
        if ( !Networking.IsHost )
			return;
        GameObject newObject = new GameObject();
        if(owner == PlayerState.Local.PlayerPawn)
        {
            PlayerState.Thing thing = new PlayerState.Thing
            {
                gameObjects = newObject.Children.ToList()
            };
            owner.PlayerState.SpawnedThings.Add(thing);
        }
        SceneUtility.MakeIdGuidsUnique(gameObject);
        newObject.Deserialize(gameObject);
        newObject.Transform.Position = position;
        newObject.Transform.Rotation = rotation;
        while (newObject.Children.Count > 0)
        {
            GameObject go = newObject.Children[0];
            go.SetParent(Game.ActiveScene);
            go.NetworkSpawn();
        }
        newObject.Destroy();
    }
}
