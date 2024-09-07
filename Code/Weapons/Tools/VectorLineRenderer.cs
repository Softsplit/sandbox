#region Assembly Sandbox.Game, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// location unknown
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.Internal;

namespace Sandbox;


[Title("Vector Line Renderer")]
[Category("Rendering")]
[Icon("show_chart")]
public sealed class VectorLineRenderer : Component, Component.ExecuteInEditor
{

    private SceneLineObject _so;
    [Property]
    public bool RunBySelf {get;set;} = true;

    [Group("Points")]
    [Property]
    public List<Vector3> Points { get; set; }

    [Group("Appearance")]
    [Property]
    public float Noise { get; set; } = 1f;

    [Group("Appearance")]
    [Property]
    public Gradient Color { get; set; } = global::Color.Cyan;


    [Group("Appearance")]
    [Property]
    public Curve Width { get; set; } = 5f;


    [Group("Spline")]
    [Property]
    [Range(1f, 32f, 0.01f, true, true)]
    public int SplineInterpolation { get; set; }

    [Group("Spline")]
    [Property]
    [Range(-1f, 1f, 0.01f, true, true)]
    public float SplineTension { get; set; }

    [Group("Spline")]
    [Property]
    [Range(-1f, 1f, 0.01f, true, true)]
    public float SplineContinuity { get; set; }

    [Group("Spline")]
    [Property]
    [Range(-1f, 1f, 0.01f, true, true)]
    public float SplineBias { get; set; }

    [Group("End Caps")]
    [Property]
    public SceneLineObject.CapStyle StartCap { get; set; }

    [Group("End Caps")]
    [Property]
    public SceneLineObject.CapStyle EndCap { get; set; }

    [Group("Rendering")]
    [Property]
    public bool Wireframe { get; set; }

    [Group("Rendering")]
    [Property]
    public bool Opaque { get; set; } = true;


    protected override void OnEnabled()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so = new SceneLineObject(base.Scene.SceneWorld);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.Transform = base.Transform.World;
    }

    protected override void OnDisabled()
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so?.Delete();
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so = null;
    }

	protected override void OnPreRender()
	{
		if(RunBySelf) Run();
	}

	public void Run()
    {
        if (_so == null)
        {
            return;
        }

        if (Points == null)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            _so.RenderingEnabled = false;
            return;
        }

        IEnumerable<Vector3> enumerable = Points.Select((x, index) =>
        {
            if (index == 0 || index == Points.Count() - 1)
            {
                return x;
            }
            else
            {
                return x + (Vector3.Random * Noise);
            }
        });
        int num = enumerable.Count();
        if (num <= 1)
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            _so.RenderingEnabled = false;
            return;
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.StartCap = StartCap;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.EndCap = EndCap;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.Wireframe = Wireframe;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.Opaque = Opaque;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.RenderingEnabled = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.Transform = base.Transform.World;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.Flags.CastShadows = true;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        RenderAttributes attributes = _so.Attributes;
        StringToken k = StringToken.Literal("BaseTexture", 388050857u);
        Texture value = Texture.White;
        int mip = -1;
        attributes.Set(in k, in value, in mip);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        RenderAttributes attributes2 = _so.Attributes;
        k = StringToken.Literal("D_BLEND", 348860154u);
        mip = ((!Opaque) ? 1 : 0);
        attributes2.SetCombo(in k, in mip);
        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.StartLine();
        if (num == 2 || SplineInterpolation == 1)
        {
            int num2 = 0;
            foreach (Vector3 item in enumerable)
            {
                Vector3 pos = item;
                float time = (float)num2 / (float)num;
                RuntimeHelpers.EnsureSufficientExecutionStack();
                _so.AddLinePoint(in pos, Color.Evaluate(time), Width.Evaluate(time));
                RuntimeHelpers.EnsureSufficientExecutionStack();
                num2++;
            }
        }
        else
        {
            int num3 = 0;
            int num4 = SplineInterpolation.Clamp(1, 100);
            int num5 = (num - 1) * num4;
            foreach (Vector3 item2 in enumerable.TcbSpline(num4, SplineTension, SplineContinuity, SplineBias))
            {
                Vector3 pos2 = item2;
                float time2 = (float)num3 / (float)num5;
                RuntimeHelpers.EnsureSufficientExecutionStack();
                _so.AddLinePoint(in pos2, Color.Evaluate(time2), Width.Evaluate(time2));
                RuntimeHelpers.EnsureSufficientExecutionStack();
                num3++;
            }
        }

        RuntimeHelpers.EnsureSufficientExecutionStack();
        _so.EndLine();
    }
}

