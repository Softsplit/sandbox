using Sandbox;

public sealed class CoverFinder : Component
{
	[Property] public float coverHeight { get; set; } = 60f;
    [Property] public float coverCheckDistance { get; set; } = 150f;
    [Property] public float coverSpacing { get; set; } = 50f;
    [Property] public float flatSpacing { get; set; } = 100f;
    [Property] public float zSpacing { get; set; } = 10f;
    [Property] public MapInstance mapInstance {get;set;}
    [Property] Vector3 pointCloudBox {get;set;}
    protected override async void OnStart()
    {
        await Task.Frame();

        if(mapInstance!=null)
        {
            BBox bBox = mapInstance.Bounds;
            Transform.Position = bBox.Center;
            pointCloudBox = bBox.Size;
        }
        float timeBefore = Time.Now;
        await GenerateCoverPoints();
        Log.Info(Time.Now-timeBefore);
    }

	protected override void DrawGizmos()
	{
        
		Gizmo.Draw.LineBBox(BBox.FromPositionAndSize(Vector3.Zero,pointCloudBox));

        List<Vector3> rayPoints = GenerateSquarePoints(Vector3.Zero+Vector3.Up,flatSpacing,coverSpacing);

        for(int i = 0; i < rayPoints.Count; i++)
        {
            Gizmo.Draw.Line(Vector3.Zero,rayPoints[i]);
        }
	}


	async Task GenerateCoverPoints()
    {
        for (float x = -pointCloudBox.x / 2; x <= pointCloudBox.x / 2; x += flatSpacing)
        {
            for (float y = -pointCloudBox.y / 2; y <= pointCloudBox.y / 2; y += flatSpacing)
            {
                for (float z = -pointCloudBox.z / 2; z <= pointCloudBox.z / 2; z += zSpacing)
                {
                    
                    Vector3 point = Scene.NavMesh.GetClosestPoint(new Vector3(Transform.Position.x + x, Transform.Position.y + y, Transform.Position.z + z)) ?? Vector3.Zero;
                    //CreateCoverPoint(point,Vector3.Forward);
                    if(point == Vector3.Zero) continue;

                    List<Vector3> rayPoints = GenerateSquarePoints(point+Vector3.Up,flatSpacing*2,coverSpacing);

                    for(int i = 0; i < rayPoints.Count; i++)
                    {
                        var Trace = Scene.Trace.Ray(point,rayPoints[i]).WithTag("map").Run();
                        if(Trace.Hit) 
                        {
                            var HeightCheck = Scene.Trace.Ray(point,Trace.EndPosition+((Vector3.Up*coverHeight)-Vector3.Up)+(Trace.EndPosition-point).Normal).WithTag("map").Run();
                            if(HeightCheck.Hit)
                            {
                                CreateCoverPoint(Trace.EndPosition,-Trace.Normal);
                            }
                        }
                    }

                    //Vector3 closestEdge = Scene.NavMesh.GetClosestEdge(point, zSpacing) ?? Vector3.Zero;
                    
                    //if (closestEdge == Vector3.Zero) continue;

                    //CheckEdgeForCover(point, closestEdge);
                }
            }
        }
    }

    public static List<Vector3> GenerateSquarePoints(Vector3 location, float size, float pointSpacing)
    {
        List<Vector3> points = new List<Vector3>();

        float halfSize = size / 2;

        Vector3 bottomLeft = new Vector3(location.x - halfSize, location.y - halfSize, location.z);
        Vector3 bottomRight = new Vector3(location.x + halfSize, location.y - halfSize, location.z);
        Vector3 topLeft = new Vector3(location.x - halfSize, location.y + halfSize, location.z);
        Vector3 topRight = new Vector3(location.x + halfSize, location.y + halfSize, location.z);

        void AddPoints(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).Normal;
            float distance = Vector3.DistanceBetween(start, end);

            for (float i = 0; i <= distance; i += pointSpacing)
            {
                points.Add(start + direction * i);
            }
        }

        AddPoints(bottomLeft, bottomRight);
        AddPoints(bottomRight, topRight);
        AddPoints(topRight, topLeft);
        AddPoints(topLeft, bottomLeft);

        points.Add(bottomLeft);
        points.Add(bottomRight);
        points.Add(topRight);
        points.Add(topLeft);

        return points;
    }

	void CreateCoverPoint(Vector3 location, Vector3 direction)
	{
		GameObject point = new GameObject();
		point.Tags.Add("cover");
		point.Components.Create<SphereCollider>();
        //point.Components.Create<ModelRenderer>();
		point.Transform.Position = location;

        (Vector3 centerDirection, float angle) = CheckCoverDirections(location,direction,coverCheckDistance);

		point.Transform.Rotation = Rotation.LookAt(centerDirection);
        point.Tags.Add(angle.ToString());   

		point.SetParent(GameObject);
	}

    void CheckEdgeForCover(Vector3 edge, Vector3 edgeDirection)
    {

        Vector3 rayOrigin = edge + Vector3.Up * coverHeight;

        if (RaycastForCover(rayOrigin, edgeDirection))
        {
            CreateCoverPoint(edge,edgeDirection);
        }
    }

    public static (Vector3 centerDirection, float angle) CheckCoverDirections(Vector3 position, Vector3 initialDirection, float Range, float angleIncrement = 5f)
    {
        float currentAngle = angleIncrement;
        Vector3 leftDirection = initialDirection;
        Vector3 rightDirection = initialDirection;
        bool hitLeftFound = true, hitRightFound = true;

        while ((hitLeftFound || hitRightFound) && currentAngle < 180f)
        {
            currentAngle += angleIncrement;
            if(hitLeftFound)
            {
                leftDirection = (Rotation)new Angles(0, -currentAngle, 0) * initialDirection;
                hitLeftFound = Game.ActiveScene.Trace.Ray(position, position+leftDirection*Range).WithTag("map").Run().Hit;
            }
            if(hitRightFound)
            {
                rightDirection = (Rotation)new Angles(0, currentAngle, 0) * initialDirection;
                hitRightFound = Game.ActiveScene.Trace.Ray(position, position+rightDirection*Range).WithTag("map").Run().Hit;
            }
        }

        if (!hitLeftFound && !hitRightFound)
        {
            Vector3 centerDirection = (leftDirection + rightDirection)/2;

            float angle = GetAngleBetweenDirections(leftDirection,rightDirection)/2;

            return (centerDirection, angle);
        }
        else
        {
            return (Vector3.Zero, 40000);
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
    public static float GetAngleBetweenDirections(Vector3 direction1, Vector3 direction2)
    {
        Vector3 dir1Normalized = direction1.Normal;
        Vector3 dir2Normalized = direction2.Normal;

        float dotProduct = Vector3.Dot(dir1Normalized, dir2Normalized);
        
        float angleInRadians = MathF.Acos(dotProduct);

        float angleInDegrees = MathX.RadianToDegree(angleInRadians);
        
        return angleInDegrees;
    }
}


