namespace Carcassonne.State
{
    /// <summary>
    /// Describes different phases of gameplay.
    /// </summary>
    public enum Phase
    {
        NewTurn,
        TileDrawn,
        TileDown,
        MeepleDrawn,
        MeepleDown,
        GameOver
    }
}