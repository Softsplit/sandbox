using Sandbox;

public sealed class Beam : Component
{
	VectorLineRenderer LineRenderer1;
	VectorLineRenderer LineRenderer2;
	[Property] public Curve EffectCurve1;
	[Property] public Curve EffectCurve2;
	[Property] public bool enabled;
	[Property] public Vector3 Base;
	[Property] public int pointDistance = 10;
	protected override void OnStart()
	{
		LineRenderer2 = Components.Create<VectorLineRenderer>();
		LineRenderer2.Points = new List<Vector3>{Vector3.Zero,Vector3.Zero};
		LineRenderer2.Color = new Color(0f, 0.5f, 1f);
		LineRenderer2.Width = EffectCurve1;
		LineRenderer2.RunBySelf = false;
		LineRenderer2.Noise = 1f;

		LineRenderer1 = Components.Create<VectorLineRenderer>();
		LineRenderer1.Points = new List<Vector3>{Vector3.Zero,Vector3.Zero};
		LineRenderer1.Color = Color.Cyan;
		LineRenderer1.Width = EffectCurve2;
		LineRenderer1.RunBySelf = false;
		LineRenderer1.Noise = 0.2f;

	}

	protected override void OnFixedUpdate()
	{
		LineRenderer1.Enabled = enabled;
		LineRenderer2.Enabled = enabled;
		if(!enabled) return;
		LineRenderer1.Run();
		LineRenderer2.Run();
	}

	public void CreateEffect(Vector3 Start, Vector3 End)
	{
		LineRenderer1.Points = GetSpacedPoints( Start, End, (int)MathF.Round(Vector3.DistanceBetween(Start,End))/pointDistance);
		LineRenderer2.Points = GetSpacedPoints( Start, End, (int)MathF.Round(Vector3.DistanceBetween(Start,End))/pointDistance*2);
	}

	public static List<Vector3> GetSpacedPoints(Vector3 start, Vector3 end, int numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();

        float step = 1.0f / (numberOfPoints - 1);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i * step;
            Vector3 point = Vector3.Lerp(start, end, t);
            points.Add(point);
        }

        return points;
    }
}
