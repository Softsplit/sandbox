namespace Softsplit;

/// <summary>
/// Player Stats.
/// </summary>
public partial class PlayerStats : Component
{
    [Property]
    public List<GameObject> spawnedPropsList = new List<GameObject>();

    protected override void OnUpdate()
    {
        if ( Input.Pressed( "prop_undo" ) )
        {
            if ( spawnedPropsList.Count > 0 )
            {
                spawnedPropsList.Last().Destroy();
                spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
            }
        }
    }
}