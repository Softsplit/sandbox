using Sandbox;
using Softsplit;

public sealed class CoverFinder : Component
{
	[Property] public float coverHeight { get; set; } = 38f;
	[Property] public float wallHeight { get; set; } = 60f;
    [Property] public float coverCheckDistance { get; set; } = 150f;
    [Property] public float coverSpacing { get; set; } = 50f;
    [Property] public float CoverRadius { get; set; } = 20f;
    [Property] public float flatFaceAngleCheck { get; set; } = 45f;
    [Property] public float flatSpacing { get; set; } = 100f;
    [Property] public float zSpacing { get; set; } = 10f;
    [Property] public float EnemyDistance { get; set; } = 150f;
    [Property] Vector2 pointGrid {get;set;}
    List<Vector3> generatedPoints = new List<Vector3>();

    protected override async void OnStart()
    {
        await Task.Frame();
        await Scene.NavMesh.Generate(Scene.PhysicsWorld);
    }

    public CoverContext GetClosestCover(Vector3 position,Vector3 enemyPosition,float AttackDistance)
    {
        GenerateCoverPoints(position);
        List<GameObject> gameObjects = Scene.FindInPhysics(BBox.FromPositionAndSize(position,MathF.Max(pointGrid.x,pointGrid.y))).ToList();

        CoverContext idealCover = null;
        float idealDis = pointGrid.x+pointGrid.y;

        foreach(GameObject g in gameObjects)
        {
            if(!g.Tags.Contains("cover")) continue;

            float distance = Vector3.DistanceBetween(position,g.Transform.Position);

            if(distance > idealDis) continue;

            CoverContext coverContext = g.Components.Get<CoverContext>();

            if(coverContext == null) continue;

            if(coverContext.owned) continue;

            if(!IsValidCover(coverContext, enemyPosition, AttackDistance)) continue;

            idealCover = coverContext;
            idealDis = distance;

        }

        return idealCover;
    }
    public bool IsValidCover(CoverContext cover, Vector3 enemyPosition, float AttackDistance)
    {

        if(Vector3.DistanceBetween(cover.Transform.Position, enemyPosition) < EnemyDistance) return false;

        if(Vector3.DistanceBetween(cover.Transform.Position, enemyPosition) > AttackDistance) return false;

        if(GetAngleBetweenDirections(cover.Transform.World.Forward,enemyPosition-cover.Transform.Position) > cover.angle) return false;

        return true;
    }
 

	void GenerateCoverPoints(Vector3 position)
	{
        Vector3 clampedPos = new Vector3
        (
            MathF.Round(position.x / pointGrid.x)*pointGrid.x,
            MathF.Round(position.y / pointGrid.y)*pointGrid.y,
            MathF.Round(position.z / zSpacing)*zSpacing
        );
        if(generatedPoints.Contains(clampedPos)) return;
        generatedPoints.Add(clampedPos);

        position = clampedPos;
		for ( float x = -pointGrid.x / 2; x <= pointGrid.x / 2; x += flatSpacing )
		{
			for ( float y = -pointGrid.y / 2; y <= pointGrid.y / 2; y += flatSpacing )
			{
				Vector3 point = Scene.NavMesh.GetClosestPoint( new Vector3( position.x + x, position.y + y, Transform.Position.z ) ) ?? Vector3.Zero;
				if ( point == Vector3.Zero ) continue;

				List<Vector3> rayPoints = GenerateSquarePoints( point + Vector3.Up, flatSpacing * 2, coverSpacing );

				for ( int i = 0; i < rayPoints.Count; i++ )
				{
					var Trace = Scene.Trace.Ray( point, rayPoints[i] ).WithTag( "map" ).Run();
					if ( Trace.Hit )
					{
						var HeightCheck = Scene.Trace.Ray( point, Trace.EndPosition + ((Vector3.Up * coverHeight) - Vector3.Up) + (Trace.EndPosition - point).Normal ).WithTag( "map" ).Run();
						if ( HeightCheck.Hit )
						{
                            var WallCheck = Scene.Trace.Ray( point, Trace.EndPosition + ((Vector3.Up * wallHeight) - Vector3.Up) + (Trace.EndPosition - point).Normal ).WithTag( "map" ).Run();
							CreateCoverPoint( Trace.EndPosition-Trace.Direction*CoverRadius, -Trace.Normal, WallCheck.Hit);
						}
					}
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

	void CreateCoverPoint(Vector3 location, Vector3 direction, bool wall)
	{
        (Vector3 centerDirection, float angle, float firstStopAngle) = CheckCoverDirections(location,direction,coverCheckDistance);
        if(firstStopAngle > flatFaceAngleCheck && wall)
        {
            return;
        }
		GameObject point = new GameObject();
		point.Components.Create<SphereCollider>();
        //point.Components.Create<ModelRenderer>();
		point.Transform.Position = location;
		point.Transform.Rotation = Rotation.LookAt(centerDirection);

        CoverContext coverContext = point.Components.Create<CoverContext>();
        coverContext.angle = angle;
        coverContext.wall = wall;

		point.SetParent(GameObject);
	}

    public static (Vector3 centerDirection, float angle, float firstStopAngle) CheckCoverDirections(Vector3 position, Vector3 initialDirection, float Range, float angleIncrement = 5f)
    {
        float currentAngle = angleIncrement;
        Vector3 leftDirection = initialDirection;
        Vector3 rightDirection = initialDirection;
        bool hitLeftFound = true, hitRightFound = true;
        float stopAngle = 0;

        while ((hitLeftFound || hitRightFound) && currentAngle < 180f)
        {
            currentAngle += angleIncrement;
            if(hitLeftFound)
            {
                leftDirection = (Rotation)new Angles(0, -currentAngle, 0) * initialDirection;
                var trace = Game.ActiveScene.Trace.Ray(position, position+leftDirection*Range).WithTag("map").Run();
                hitLeftFound = trace.Hit;
                if(!trace.Hit && stopAngle == 0) stopAngle = currentAngle;
            }
            if(hitRightFound)
            {
                rightDirection = (Rotation)new Angles(0, currentAngle, 0) * initialDirection;
                var trace = Game.ActiveScene.Trace.Ray(position, position+rightDirection*Range).WithTag("map").Run();
                hitRightFound = trace.Hit;
                if(!trace.Hit && stopAngle == 0) stopAngle = currentAngle;
            }
        }

        if (!hitLeftFound && !hitRightFound)
        {
            Vector3 centerDirection = (leftDirection + rightDirection)/2;

            float angle = GetAngleBetweenDirections(leftDirection,rightDirection)/2;

            return (centerDirection, angle,stopAngle);
        }
        else
        {
            return (Vector3.Zero, 40000,stopAngle);
        }
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


