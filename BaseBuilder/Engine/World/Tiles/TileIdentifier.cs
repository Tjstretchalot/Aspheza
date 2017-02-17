using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
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

        protected static RectangleD2D TileCollisionMesh;

        static TileIdentifier()
        {
            TileConstructorParamTypes = new Type[] { typeof(PointI2D), typeof(RectangleD2D) };
            IdsToTiles = new Dictionary<short, Type>();
            TilesToIds = new Dictionary<Type, short>();

            TileCollisionMesh = new RectangleD2D(1, 1);

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(DirtTile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(GrassTile).TypeHandle);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(StoneTile).TypeHandle);
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

        public static Tile InitTile(Type type, PointI2D position)
        {
            return (Tile) type.GetConstructor(TileConstructorParamTypes).Invoke(new object[] { position, TileCollisionMesh });
        }
    }
}
