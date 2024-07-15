namespace Softsplit;

/// <summary>
/// Player Prop Stats.
/// </summary>
public partial class PlayerState
{
    [Property]
    public List<GameObject> spawnedPropsList = new List<GameObject>();

    protected async void CheckPropUndo()
    {
        if ( Input.Pressed( "undo" ) )
        {
            if ( spawnedPropsList.Count > 0 )
            {
                DestoryLastSpawnedProp( spawnedPropsList.Last() );
                spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
            }
        }
        else if ( Input.Down( "undo" ) )
        {
            await Task.DelaySeconds( 1 );
            if ( !Input.Down( "undo" ) ) return;
            if ( spawnedPropsList.Count > 0 )
            {
                DestoryLastSpawnedProp( spawnedPropsList.Last() );
                spawnedPropsList.RemoveAt( spawnedPropsList.IndexOf( spawnedPropsList.Last() ) );
            }
        }
    }

    [Broadcast]
    public void DestoryLastSpawnedProp( GameObject propToDestory )
    {
        propToDestory?.Destroy();
    }
}