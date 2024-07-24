using Sandbox;

public sealed class CoverFinder : Component
{
	[Property] public float coverHeight {get;set;} = 60f;
    [Property] public float coverCheckDistance {get;set;} = 700f;  
    [Property] public int numberOfRandomPoints {get;set;} = 100;

    protected override void OnStart()
    {
        GenerateCoverPoints();
    }

    void GenerateCoverPoints()
    {
        for (int i = 0; i < numberOfRandomPoints; i++)
        {
            Vector3 randomPoint = Scene.NavMesh.GetRandomPoint(Vector3.Zero,coverCheckDistance) ?? Vector3.Zero;
            
            Vector3 closestEdge = Scene.NavMesh.GetClosestEdge(randomPoint).Value;

            if(!closestEdge.IsNearlyZero()) CheckEdgeForCover(randomPoint, closestEdge);
        }
    }

	void CreateCoverPoint(Vector3 location, Vector3 direction)
	{
		GameObject point = new GameObject();
		point.Tags.Add("cover");
		point.Components.Create<SphereCollider>();
		point.Transform.Position = location;
		point.Transform.Rotation = Rotation.LookAt(direction);
		point.SetParent(GameObject);
	}

    void CheckEdgeForCover(Vector3 start, Vector3 end)
    {
		Log.Info("wag");
        Vector3 edgeDirection = (end - start).Normal;
        Vector3 perpendicularDirection = Vector3.Cross(edgeDirection, Vector3.Up).Normal;

        Vector3 rayOrigin1 = start + Vector3.Up * coverHeight;
        Vector3 rayOrigin2 = end + Vector3.Up * coverHeight;

        if (RaycastForCover(rayOrigin1, perpendicularDirection))
        {
            CreateCoverPoint(start,perpendicularDirection);
        }
		else if (RaycastForCover(rayOrigin1, -perpendicularDirection))
		{
			CreateCoverPoint(start,-perpendicularDirection);
		}

        if (RaycastForCover(rayOrigin2, perpendicularDirection))
        {
            CreateCoverPoint(end,perpendicularDirection);
        }
		else if (RaycastForCover(rayOrigin2, -perpendicularDirection))
		{
			CreateCoverPoint(end,-perpendicularDirection);
		}
    }

    bool RaycastForCover(Vector3 origin, Vector3 direction)
    {

        var hit = Game.SceneTrace.Ray(origin,origin+direction*coverCheckDistance).Run();
        if (hit.Hit)
        {
            return true;
        }
        return false;
    }
}
