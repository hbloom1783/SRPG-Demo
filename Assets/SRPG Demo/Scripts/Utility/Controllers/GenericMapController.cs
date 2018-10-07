using Gamelogic.Grids;

namespace SRPGDemo.Utility.Controllers
{
    public class GenericMapController<TCell, TPoint> : GridBehaviour<TPoint> where TPoint : IGridPoint<TPoint> where TCell : TileCell
    {
    }
}
