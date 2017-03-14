using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.WorldObject.Entities;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors
{
    /// <summary>
    /// A material restriction will either restrict all materials
    /// except a specific one, or restrict a specific material.
    /// </summary>
    public class MaterialRestriction : ITransferRestrictor
    {
        /// <summary>
        /// If we are restricting everything except a specific material.
        /// </summary>
        public bool AllExcept;

        /// <summary>
        /// The material that we are restricting all except
        /// </summary>
        public Material KeyMaterial;

        public MaterialRestriction(Material material, bool allExcept)
        {
            AllExcept = allExcept;
            KeyMaterial = material;
        }

        public MaterialRestriction(NetIncomingMessage message)
        {
            AllExcept = message.ReadBoolean();

            if (message.ReadBoolean())
            {
                var matID = message.ReadInt32();
                KeyMaterial = Material.GetMaterialByID(matID);
            }
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(AllExcept);

            if(KeyMaterial == null)
            {
                message.Write(false);
            }else
            {
                message.Write(true);
                message.Write(KeyMaterial.ID);
            }
        }

        public void Restrict(SharedGameState sharedGameState, Container from, Container to, Dictionary<Material, int> maxes)
        {
            if (AllExcept)
            {
                var keysCopied = maxes.Keys.ToArray();

                foreach (var key in keysCopied)
                {
                    if (key != KeyMaterial)
                        maxes.Remove(key);
                }
            }else
            {
                if(maxes.ContainsKey(KeyMaterial))
                    maxes.Remove(KeyMaterial);
            }
        }


        public void Reset()
        {
        }

        public bool IsValid()
        {
            return KeyMaterial != null;
        }
    }
}
