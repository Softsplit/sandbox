namespace Softsplit;

/// <summary>
/// Player Prop Stats.
/// </summary>
public partial class PlayerState
{
    [Property]
    public List<GameObject> spawnedPropsList = new List<GameObject>();
    public float undoPropHeldTimer { get; private set; } = 0;

    protected void CheckPropUndo()
    {
        if ( Input.Pressed( "undo" ) )
        {
            undoPropHeldTimer = 0;
            if ( spawnedPropsList.Count > 0 )
            {
                DestoryLastSpawnedProp( spawnedPropsList.Last() );
                spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
            }
        }
        else if ( Input.Down( "undo" ) )
        {
            undoPropHeldTimer += 0.1f;
            if ( undoPropHeldTimer > 1 )
                if ( spawnedPropsList.Count > 0 )
                {
                    DestoryLastSpawnedProp( spawnedPropsList.Last() );
                    spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
                    undoPropHeldTimer = 0;
                }
        }
    }

    [Broadcast]
    public void DestoryLastSpawnedProp( GameObject propToDestory )
    {
        propToDestory?.Destroy();
    }
}