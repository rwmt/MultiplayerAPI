using System;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Multiplayer.API
{
    /// <summary>
    /// Context flags which are sent along with a command
    /// </summary>
    [Flags]
    public enum SyncContext
    {
        /// <summary>Default  value. (no context)</summary>
        None = 0,
        /// <summary>Send mouse cell context (emulates mouse position)</summary>
        MapMouseCell = 1,
        /// <summary>Send map selected context (object selected on the map)</summary>
        MapSelected = 2,
        /// <summary>Send world selected context (object selected on the world map)</summary>
        WorldSelected = 4,
        /// <summary>Send order queue context (emulates pressing KeyBindingDefOf.QueueOrder)</summary>
        QueueOrder_Down = 8,
        /// <summary>Send current map context</summary>
        CurrentMap = 16,
    }

    /// <summary>
    /// An attribute that is used to mark methods for syncing.
    /// The call will be replicated by the MPApi on all clients automatically.
    /// </summary>
    /// <example>
    /// <para>An example showing how to mark a method for syncing.</para>
    /// <code>
    /// [SyncMethod]
    /// public void MyMethod(...)
    /// {
    ///     ...
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public class SyncMethodAttribute : Attribute
    {
        public SyncContext context;

        /// <summary>Instructs SyncMethod to cancel synchronization if any arg is null (see <see cref="ISyncMethod.CancelIfAnyArgNull"/>).</summary>
        public bool cancelIfAnyArgNull = false;

        /// <summary>Instructs SyncMethod to cancel synchronization if no map objects were selected during the call (see <see cref="ISyncMethod.CancelIfNoSelectedMapObjects"/>).</summary>
        public bool cancelIfNoSelectedMapObjects = false;

        /// <summary>Instructs SyncMethod to cancel synchronization if no world objects were selected during call replication(see <see cref="ISyncMethod.CancelIfNoSelectedWorldObjects"/>).</summary>
        public bool cancelIfNoSelectedWorldObjects = false;

        /// <summary>Instructs SyncMethod to synchronize only in debug mode (see <see cref="ISyncMethod.SetDebugOnly"/>).</summary>
        public bool debugOnly = false;

        /// <summary>A list of types to expose (see <see cref="ISyncMethod.ExposeParameter"/>)</summary>
        public int[] exposeParameters;

        /// <param name="context">Context</param>
        public SyncMethodAttribute(SyncContext context = SyncContext.None)
        {
            this.context = context;
        }
    }

    /// <summary>
    /// An attribute that is used to mark fields for syncing.
    /// It will be Watched for changes by the MPApi when instructed.
    /// </summary>
    /// <example>
    /// <para>An example showing how to mark a field for syncing.</para>
    /// <code>
    /// [SyncField]
    /// public class MyClass
    /// {
    ///     [SyncField]
    ///     bool myField;
    /// 
    ///     ...
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncFieldAttribute : Attribute
    {
        public SyncContext context;

        /// <summary>Instructs SyncField to cancel synchronization if the value of the member it's pointing at is null.</summary>
        public bool cancelIfValueNull = false;

        /// <summary>Instructs SyncField to sync in game loop.</summary>
        public bool inGameLoop = false;

        /// <summary>Instructs SyncField to use a buffer instead of syncing instantly (when <see cref="MP.WatchEnd"/> is called).</summary>
        public bool bufferChanges = true;

        /// <summary>Instructs SyncField to synchronize only in debug mode.</summary>
        public bool debugOnly = false;

        /// <summary>Instructs SyncField to synchronize only if it's invoked by the host.</summary>
        public bool hostOnly = false;

        /// <summary></summary>
        public int version;

        /// <param name="context">Context</param>
        public SyncFieldAttribute(SyncContext context = SyncContext.None)
        {
            this.context = context;
        }
    }

    /// <summary>
    /// An attribute that is used to mark methods which create <see cref="Verse.Dialog_NodeTree"/> for syncing.
    /// Any option picked by a player will be synced between all clients automatically.
    /// It can be combined with <see cref="SyncDialogNodeTreeAttribute"/> so the call will be replicated by the MPApi on all clients automatically.
    /// </summary>
    /// <example>
    /// <para>An example showing how to mark a method for syncing.</para>
    /// <code>
    /// [SyncDialogNodeTree]
    /// public void MyMethod(...)
    /// {
    ///     ...
    ///     Find.WindowStack.Add(new Dialog_NodeTree(diaNode, faction));
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public class SyncDialogNodeTreeAttribute : Attribute
    { }

    public struct SyncType
    {
        public readonly Type type;
        public bool expose;
        public bool contextMap;

        public SyncType(Type type)
        {
            this.type = type;
            this.expose = false;
            contextMap = false;
        }

        public static implicit operator SyncType(ParameterInfo param)
        {
            return new SyncType(param.ParameterType) { /*expose = param.HasAttribute<SyncExpose>(), contextMap = param.HasAttribute<SyncContextMap>()*/ };
        }

        public static implicit operator SyncType(Type type)
        {
            return new SyncType(type);
        }
    }

    /// <summary>Specifies the type of method. Those values are identical to Harmony's MethodType enum, and exist here to prevent reliance of this API on Harmony.</summary>
    public enum ParentMethodType
    {
        /// <summary>This is a normal method</summary>
        Normal,
        /// <summary>This is a getter</summary>
        Getter,
        /// <summary>This is a setter</summary>
        Setter,
        /// <summary>This is a constructor</summary>
        Constructor,
        /// <summary>This is a static constructor</summary>
        StaticConstructor,
        /// <summary>This targets the MoveNext method of the enumerator result</summary>
        Enumerator,
    }

    /// <summary>
    /// <para>Used by Multiplayer's session manager to allow for creation of blocking dialogs, while (in case of async time) only pausing specific maps.</para>
    /// <para>Sessions will be reset/reloaded during reloading - to prevent it, implement <see cref="IExposableSession"/> or <see cref="ISemiPersistentSession"/>.</para>
    /// <para>You should avoid implementing this interface directly, instead opting into inheriting <see cref="Session"/> for greater compatibility.</para>
    /// </summary>
    public abstract class Session
    {
        // Use internal to prevent mods from easily modifying it?
        protected int sessionId;
        // Should it be virtual?
        /// <summary>
        /// <para>Used for syncing session across players by assigning them IDs, similarly to how every <see cref="Thing"/> receives an ID.</para>
        /// <para>Automatically applied by the session manager</para>
        /// <para>If inheriting <see cref="Session"/> you don't have to worry about this property.</para>
        /// </summary>
        public int SessionId
        {
            get => sessionId;
            set => sessionId = value;
        }

        /// <summary>
        /// Used by the session manager while joining the game - if it returns <see langword="false"/> it'll get removed.
        /// </summary>
        public virtual bool IsSessionValid => true;

        /// <summary>
        /// Mandatory constructor for any subclass of <see cref="Session"/>.
        /// </summary>
        /// <param name="map">The map this session belongs to. It will be provided by session manager when syncing.</param>
        protected Session(Map map) { }

        /// <summary>
        /// Called once the sessions has been added to the list of active sessions. Can be used for initialization.
        /// </summary>
        /// <remarks>In case of <see cref="ISessionWithCreationRestrictions"/>, this will only be called if successfully added.</remarks>
        public virtual void PostAddSession()
        {
        }

        /// <summary>
        /// Called once the sessions has been removed to the list of active sessions. Can be used for cleanup.
        /// </summary>
        public virtual void PostRemoveSession()
        {
        }

        /// <summary>
        /// A convenience method to switch to a specific map or world. Intended to be used from <see cref="GetBlockingWindowOptions"/> when opening menu.
        /// </summary>
        /// <param name="map">Map to switch to or <see langword="null"/> to switch to world view.</param>
        protected static void SwitchToMapOrWorld(Map map)
        {
            if (map == null)
            {
                Find.World.renderer.wantedMode = WorldRenderMode.Planet;
            }
            else
            {
                if (WorldRendererUtility.WorldRenderedNow) CameraJumper.TryHideWorld();
                Current.Game.CurrentMap = map;
            }
        }

        /// <summary>
        /// The map this session is used by or <see langword="null"/> in case of global sessions.
        /// </summary>
        public abstract Map Map { get; }

        /// <summary>
        /// <para>Called when checking ticking and if any session returns <see langword="true"/> - it'll force pause the map/game.</para>
        /// <para>In case of local (map) sessions, it'll only be called by the current map. In case of global (world) sessions, it'll be called by the world and each map.</para>
        /// </summary>
        /// <param name="map">Current map (when checked from local session manager) or <see langword="null"/> (when checked from local session manager).</param>
        /// <remarks>If there are multiple sessions active, this method is not guaranteed to run if a session before this one returned <see langword="true"/>.</remarks>
        /// <returns><see langword="true"/> if the session should pause the map/game, <see langword="false"/> otherwise.</returns>
        public abstract bool IsCurrentlyPausing(Map map);

        /// <summary>
        /// Called when a session is active, and if any session returns a non-null value, a button will be displayed which will display all options.
        /// </summary>
        /// <param name="entry">Currently processed colonist bar entry. Will be called once per <see cref="ColonistBar.Entry.group"/>.</param>
        /// <returns>Menu option that will be displayed when the session is active. Can be <see langword="null"/>.</returns>
        public abstract FloatMenuOption GetBlockingWindowOptions(ColonistBar.Entry entry);
    }

    /// <summary>
    /// <para>Sessions inheriting from this class contain persistent data.</para>
    /// <para>When inheriting from this class, remember to call <c>base.ExposeData()</c> to let it handle <see cref="Session.SessionId"/></para>
    /// <para>Persistent data:</para>
    /// <list type="bullet">
    ///     <item>Serialized into XML using RimWorld's Scribe system</item>
    ///     <item>Save-bound: survives a server restart</item>
    /// </list>
    /// </summary>
    public abstract class ExposableSession : Session, IExposable
    {
        /// <inheritdoc cref="Session(Map)"/>
        protected ExposableSession(Map map) : base(map) { }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref sessionId, "sessionId");
        }
    }

    /// <summary>
    /// <para>Sessions implementing this interface consist of semi-persistent data.</para>
    /// <para>Semi-persistent data:</para>
    /// <list type="bullet">
    ///     <item>Serialized into binary using the Sync system</item>
    ///     <item>Session-bound: survives a reload, lost when the server is closed</item>
    /// </list>
    /// </summary>
    public abstract class SemiPersistentSession : Session
    {
        /// <inheritdoc cref="Session(Map)"/>
        protected SemiPersistentSession(Map map) : base(map) { }

        /// <summary>
        /// Writes/reads the data used by this session.
        /// </summary>
        /// <param name="sync">Sync worker used for writing/reading the data.</param>
        public abstract void Sync(SyncWorker sync);
    }
}