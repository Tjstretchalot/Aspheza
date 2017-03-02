using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Tiles
{
    /// <summary>
    /// This class is mainly used in networking to serialize
    /// tiles.
    /// </summary>
    public class TileIdentifier
    {
        protected static Type[] TileConstructorParamTypes;
        protected static Dictionary<short, Type> IdsToTiles;
        protected static Dictionary<Type, short> TilesToIds;

        static TileIdentifier()
        {
            TileConstructorParamTypes = new Type[] { typeof(NetIncomingMessage) };
            IdsToTiles = new Dictionary<short, Type>();
            TilesToIds = new Dictionary<Type, short>();

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(DirtTile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(GrassTile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(StoneTile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WaterTile).TypeHandle);
        }

        public static void Register(Type tileType, short id)
        {
            IdsToTiles.Add(id, tileType);
            TilesToIds.Add(tileType, id);
        }

        public static short GetIDOfTile(Type tileType)
        {
            return TilesToIds[tileType];
        }

        public static Type GetTypeOfID(short id)
        {
            return IdsToTiles[id];
        }

        public static Tile InitTile(Type type, PointI2D pos, NetIncomingMessage message)
        {
            return (Tile) type.GetConstructor(TileConstructorParamTypes).Invoke(new object[] { pos, PolygonD2D.UnitSquare, message });
        }
    }
}
