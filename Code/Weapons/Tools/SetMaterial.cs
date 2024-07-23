using Softsplit.UI;

namespace Softsplit;

public sealed class SetMaterial : ToolComponent
{
    SetMaterialMenu setMaterialMenu;
	protected override void Start()
	{
        setMaterialMenu = Components.Get<SetMaterialMenu>(true);
        ToolName = "Set Material";
        ToolDes = "Change the material of an object.";        
	}
    protected override void PrimaryAction()
    {
        var hit = Trace();
        if(hit.Hit && !hit.Tags.Contains("map"))
        {
            if(hit.GameObject!=null)
            {
                Recoil(hit.EndPosition);
                SetModelMaterial(hit.GameObject,setMaterialMenu.material);
            }
        }
    }
	protected override void SecondaryAction()
	{

        var hit = Trace();
        if(hit.Hit && !hit.Tags.Contains("map"))
        {
            ModelRenderer modelRenderer = hit.GameObject.Components.Get<ModelRenderer>();
            if(modelRenderer != null && setMaterialMenu.material != null)
            {
                Recoil(hit.EndPosition);
                setMaterialMenu.material = modelRenderer.GetMaterial();
            }
        }
	}

    [Broadcast]
    public static void SetModelMaterial(GameObject gameObject, Material material)
    {
        ModelRenderer modelRenderer = gameObject.Components.Get<ModelRenderer>();
        if(modelRenderer != null && material != null)
        {
            modelRenderer.MaterialOverride = material;
        }
    }
}
