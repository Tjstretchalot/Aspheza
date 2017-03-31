using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.MobileEntities;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Microsoft.Xna.Framework.Content;
using static BaseBuilder.Engine.World.Entities.EntityInventory;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    /// <summary>
    /// A building that is inprogress. Goes through two phases - acquiring
    /// resources and actually building. Building requires worker time.
    /// </summary>
    public class UnbuiltBuilding : ImmobileEntity, Container, Aidable
    {
        /// <summary>
        /// Describes the state / phase we are currently in
        /// </summary>
        protected enum BuildPhase
        {
            /// <summary>
            /// Waiting for workers to give us resources
            /// </summary>
            WaitingResources=0,

            /// <summary>
            /// Waiting for workers to finish building the building.
            /// </summary>
            Building
        }
        
        /// <summary>
        /// The inventory of building supplies
        /// </summary>
        public EntityInventory Inventory { get; protected set; }

        public bool NeedAid
        {
            get
            {
                return Phase == BuildPhase.Building && BuildTimeRemainingMS > 0;
            }
        }

        public int AidTimeToNextMS
        {
            get
            {
                return BuildTimeRemainingMS;
            }
        }

        public override string HoverText
        {
            get
            {
                var result = new StringBuilder(base.HoverText).AppendLine();

                switch(Phase)
                {
                    case BuildPhase.WaitingResources:
                        result.Append("Waiting for resources:");
                        foreach(var resourceRequired in RequiredResources)
                        {
                            var matName = resourceRequired.Item1.Name;
                            var amtWeWant = resourceRequired.Item2;
                            var amtWeHave = Inventory.GetAmountOf(resourceRequired.Item1);

                            result.AppendLine().Append("  ").Append(matName).Append(" ").Append(amtWeHave).Append("/").Append(amtWeWant);
                        }
                        break;
                    case BuildPhase.Building:
                        result.Append("Waiting for workers to build (aid): ").Append(BuildTimeInitialMS - BuildTimeRemainingMS).Append("/").Append(BuildTimeInitialMS);
                        break;
                }
                
                return result.ToString();
            }
        }
        /// <summary>
        /// The renderer that is used.
        /// </summary>
        protected SpriteSheetAnimationRenderer Renderer;

        /// <summary>
        /// The build phase we are in
        /// </summary>
        protected BuildPhase Phase;

        /// <summary>
        /// The resources that are required to build this building
        /// </summary>
        protected List<Tuple<Material, int>> RequiredResources;

        /// <summary>
        /// The entity that corresponds with the built building. To be added
        /// to the shared state when we are finished building.
        /// </summary>
        protected ImmobileEntity BuiltBuildingEntity;

        /// <summary>
       /// The animation to run at different progresses. For example
        /// 
        /// <pre>[ [ 0, UnbuiltGeneric ], [ 0.3, GenericThirty ], [ 0.7, GenericSeventy ] ]</pre>
        /// 
        /// would cause use the unbuilt animation when starting, GenericThirty after 30 percent
        /// of the work has been done (not including acquiring resources - 30% of worker build time),
        /// etc.
        /// </summary>
        protected List<Tuple<double, AnimationType>> ProgressToNewAnimations;

        /// <summary>
        /// How much build time was initially required
        /// </summary>
        protected int BuildTimeInitialMS;

        /// <summary>
        /// How much build time to we have left?
        /// </summary>
        protected int BuildTimeRemainingMS;

        protected double NextAnimationProgress;
        protected AnimationType NextAnimation;
        
        public UnbuiltBuilding() : base()
        {

        }

        public UnbuiltBuilding(PointD2D position, int id, List<Tuple<Material, int>> requiredResources, 
            ImmobileEntity builtEntity, List<Tuple<double, AnimationType>> progToNewAnim, 
            int buildTimeMS) : base(position, builtEntity.CollisionMesh, id)
        {
            RequiredResources = requiredResources;
            BuiltBuildingEntity = builtEntity;
            ProgressToNewAnimations = progToNewAnim;
            BuildTimeInitialMS = buildTimeMS;
            BuildTimeRemainingMS = buildTimeMS;
            Phase = BuildPhase.WaitingResources;

            Inventory = new EntityInventory(requiredResources.Count);
            int minStackSize = 1;
            foreach(var res in requiredResources)
            {
                minStackSize = Math.Max(minStackSize, res.Item2);
            }
            Inventory.SetDefaultStackSize(minStackSize);
            
            InitNonnetworkableParts();
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();

            int numReqRes = message.ReadInt32();
            RequiredResources = new List<Tuple<Material, int>>(numReqRes);
            for(int i = 0; i < numReqRes; i++)
            {
                var mat = Material.GetMaterialByID(message.ReadInt32());
                var amt = message.ReadInt32();
                RequiredResources.Add(Tuple.Create(mat, amt));
            }
            var entTypeID = message.ReadInt16();
            BuiltBuildingEntity = (ImmobileEntity)EntityIdentifier.InitEntity(EntityIdentifier.GetTypeOfID(entTypeID), gameState, message);
            CollisionMesh = BuiltBuildingEntity.CollisionMesh;

            var numProgToAnims = message.ReadInt32();
            ProgressToNewAnimations = new List<Tuple<double, AnimationType>>(numProgToAnims);
            for(int i = 0; i < numProgToAnims; i++)
            {
                var prog = message.ReadDouble();
                var anim = (AnimationType)message.ReadInt32();
                ProgressToNewAnimations.Add(Tuple.Create(prog, anim));
            }

            Inventory = new EntityInventory(message);
            BuildTimeInitialMS = message.ReadInt32();
            BuildTimeRemainingMS = message.ReadInt32();
            Phase = (BuildPhase)message.ReadInt32();

            TasksFromMessage(gameState, message);

            InitNonnetworkableParts();
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);

            message.Write(RequiredResources.Count);
            foreach(var reqRes in RequiredResources)
            {
                message.Write(reqRes.Item1.ID);
                message.Write(reqRes.Item2);
            }

            message.Write(EntityIdentifier.GetIDOfEntity(BuiltBuildingEntity.GetType()));
            BuiltBuildingEntity.Write(message);

            message.Write(ProgressToNewAnimations.Count);
            foreach(var tup in ProgressToNewAnimations)
            {
                message.Write(tup.Item1);
                message.Write((int)tup.Item2);
            }

            Inventory.Write(message);
            message.Write(BuildTimeInitialMS);
            message.Write(BuildTimeRemainingMS);
            message.Write((int)Phase);

            WriteTasks(message);
        }

        protected void InitNonnetworkableParts()
        {
            Inventory.AcceptsMaterialFunc = AcceptsMaterial;
            Inventory.OnMaterialAdded += MaterialAdded;
        }

        private int AcceptsMaterial(Material mat, int amt)
        {
            if (Phase != BuildPhase.WaitingResources)
                return 0;

            var tup = RequiredResources.Find((t) => t.Item1 == mat);
            if (tup == null)
                return 0;

            var amtAlready = Inventory.GetAmountOf(mat);

            if (amtAlready >= tup.Item2)
                return 0;

            return Math.Min(amt, tup.Item2 - amtAlready);
        }

        private void MaterialAdded(object sender, InventoryChangedEventArgs args)
        {
            if (args.ChangedItem == null)
                return;

            if (args.ChangedItem.Item2 == 0)
                return;
            
            foreach(var tup in RequiredResources)
            {
                var mat = tup.Item1;
                var amtWeWant = tup.Item2;
                var amtWeHave = Inventory.GetAmountOf(mat);

                if (amtWeHave != amtWeWant)
                    return;
            }

            Phase = BuildPhase.Building;
        }

        public override void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            base.SimulateTimePassing(sharedState, timeMS);

            if(Phase == BuildPhase.Building && BuildTimeRemainingMS <= 0)
            {
                BuiltBuildingEntity.Position = new PointD2D(Position.X, Position.Y);
                BuiltBuildingEntity.ID = sharedState.EntityIDCounter++;
                sharedState.World.RemoveImmobileEntity(this);
                sharedState.World.AddImmobileEntity(BuiltBuildingEntity);
            }
        }
        
        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if (Renderer == null)
            {
                Renderer = GetRenderer(context.Content);
                double bestChoiceProg = -1;
                double currentProgress = ((double)BuildTimeInitialMS - BuildTimeRemainingMS) / BuildTimeInitialMS;
                AnimationType bestChoice = AnimationType.Idle;

                foreach(var choice in ProgressToNewAnimations)
                {
                    if((bestChoiceProg == -1 || choice.Item1 > bestChoiceProg) && choice.Item1 <= currentProgress)
                    {
                        bestChoiceProg = choice.Item1;
                        bestChoice = choice.Item2;
                    }
                }

                Renderer.StartAnimation(bestChoice, null);
                FindNextBestChoice(currentProgress);
            }

            Renderer.Render(context, overlay, screenTopLeft, 1);
        }

        public void Aid(MobileEntity aider, int aidTimeMS)
        {
            if (Phase != BuildPhase.Building)
                return;

            BuildTimeRemainingMS -= aidTimeMS;

            if (Renderer == null || NextAnimationProgress == -1)
                return;

            double currentProgress = ((double)BuildTimeInitialMS - BuildTimeRemainingMS) / BuildTimeInitialMS;
            if (currentProgress >= NextAnimationProgress)
            {
                Renderer.StartAnimation(NextAnimation, null);
                FindNextBestChoice(currentProgress);
            }
        }

        void FindNextBestChoice(double currentProgress)
        {
            double nextBestChoiceProg = -1;
            AnimationType nextBestChoice = AnimationType.Idle;

            foreach (var choice in ProgressToNewAnimations)
            {
                if ((nextBestChoiceProg == -1 || choice.Item1 < nextBestChoiceProg) && choice.Item1 > currentProgress)
                {
                    nextBestChoiceProg = choice.Item1;
                    nextBestChoice = choice.Item2;
                }
            }

            NextAnimationProgress = nextBestChoiceProg;
            NextAnimation = nextBestChoice;
        }

        protected SpriteSheetAnimationRenderer GetRenderer(ContentManager content)
        {
            // TODO
            return new AnimationRendererBuilder(content)
                .BeginAnimation(null, AnimationType.Idle, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 20, height: 20, topLeftDif: new PointD2D(0, -112))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.Unbuilt, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 20, height: 20, topLeftDif: new PointD2D(0, -112))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltThirty, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 59, height: 59, topLeftDif: new PointD2D(0, -73))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltSixty, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 132 - 99, height: 99, topLeftDif: new PointD2D(0, -33))
                .EndAnimation()
                .BeginAnimation(null, AnimationType.UnbuiltNinety, defaultWidth: 100, defaultSourceTexture: "Temple")
                    .AddFrame(y: 0, height: 132, topLeftDif: new PointD2D(0, 0))
                .EndAnimation()
                .Build();
        }
    }
}
