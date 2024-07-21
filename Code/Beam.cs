using Sandbox;

public sealed class Beam : Component
{
	VectorLineRenderer LineRenderer1;
	VectorLineRenderer LineRenderer2;
	[Property] public bool RunBySelf {get;set;}
	[Property] public float Noise {get;set;} = 1f;
	[Property] public Curve EffectCurve1{get;set;}
	[Property] public Curve EffectCurve2{get;set;}
	[Property] public bool enabled{get;set;}
	[Property] public Vector3 Base{get;set;}
	[Property] public int pointDistance {get;set;}= 10;
	[Property] public GameObject ObjectStart {get;set;} 
	[Property] public GameObject ObjectEnd {get;set;} 
	protected override void OnStart()
	{
		LineRenderer2 = Components.Create<VectorLineRenderer>();
		LineRenderer2.Points = new List<Vector3>{Vector3.Zero,Vector3.Zero};
		LineRenderer2.Color = new Color(0f, 0.5f, 1f);
		LineRenderer2.Width = EffectCurve1;
		LineRenderer2.RunBySelf = false;
		LineRenderer2.Noise = Noise;

		LineRenderer1 = Components.Create<VectorLineRenderer>();
		LineRenderer1.Points = new List<Vector3>{Vector3.Zero,Vector3.Zero};
		LineRenderer1.Color = Color.Cyan;
		LineRenderer1.Width = EffectCurve2;
		LineRenderer1.RunBySelf = false;
		LineRenderer1.Noise = Noise*0.2f;

	}
	protected override void OnPreRender()
	{
		if(RunBySelf)
		{
			CreateEffect(ObjectStart.Transform.Position,ObjectEnd.Transform.Position,ObjectStart.Transform.World.Forward);
			LineRenderer1.Run();
			LineRenderer2.Run();
		}
	}
	protected override void OnFixedUpdate()
	{
		LineRenderer1.Enabled = enabled;
		LineRenderer2.Enabled = enabled;
		if(!enabled && !RunBySelf) return;
		LineRenderer1.Run();
		LineRenderer2.Run();
	}

	public void CreateEffect(Vector3 Start, Vector3 End, Vector3 dir)
	{
		LineRenderer1.Points = GetCurvedPoints( Start, dir, End, (int)MathF.Round(Vector3.DistanceBetween(Start,End))/pointDistance);
		LineRenderer2.Points = GetCurvedPoints( Start, dir, End, (int)MathF.Round(Vector3.DistanceBetween(Start,End))/pointDistance*2);
	}

	public static List<Vector3> GetCurvedPoints(Vector3 start, Vector3 direction, Vector3 end, int numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 control = start + direction*10;

        float step = 1.0f / (numberOfPoints - 1);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i * step;
            Vector3 point = CalculateBezierPoint(t, start, control, end);
            points.Add(point);
        }

        return points;
    }

    private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2 * (1-t) * t * p1
        p += tt * p2;        // t^2 * p2

        return p;
    }
}
