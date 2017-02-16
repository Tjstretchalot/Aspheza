using BaseBuilder.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.State
{
    [Obsolete(message: "Now in SharedGameState and LocalGameState")]
    public class GameState
    {
        public TileWorld World;
        public Camera Camera;
    }
}
