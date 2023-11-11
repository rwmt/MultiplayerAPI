using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace Multiplayer.API
{
    /// <summary>
    /// SyncField interface.
    /// </summary>
    /// <example>
    /// <para>Creates and registers a SyncField that points to <c>myField</c> in object of type <c>MyType</c> and enables its change buffer.</para>
    /// <code>
    /// MPApi.SyncField(typeof(MyType), "myField").SetBufferChanges();
    /// </code>
    /// <para>Creates and registers a SyncField that points to <c>myField</c> which resides in <c>MyStaticClass</c>.</para>
    /// <code>
    /// MPApi.SyncField(null, "MyAssemblyNamespace.MyStaticClass.myField");
    /// </code>
    /// <para>Creates and registers a SyncField that points to <c>myField</c> that resides in an object stored by myEnumberable defined in an object of type <c>MyType</c>.</para>
    /// <para>To watch this one you have to supply an index in <see cref="Watch(object, object)"/>.</para>
    /// <code>
    /// MPApi.SyncField(typeof(MyType), "myEnumerable/[]/myField");
    /// </code>
    /// </example>
    public interface ISyncField
    {
        /// <summary>
        /// Instructs SyncField to cancel synchronization if the value of the member it's pointing at is null.
        /// </summary>
        /// <returns>self</returns>
        ISyncField CancelIfValueNull();

        /// <summary>
        /// Instructs SyncField to sync in game loop.
        /// </summary>
        /// <returns>self</returns>
        ISyncField InGameLoop();

        /// <summary>
        /// Adds an Action that runs after a field is synchronized.
        /// </summary>
        /// <param name="action">An action ran after a field is synchronized. Called with target and value.</param>
        /// <returns>self</returns>
        ISyncField PostApply(Action<object, object> action);

        /// <summary>
        /// Adds an Action that runs before a field is synchronized.
        /// </summary>
        /// <param name="action">An action ran before a field is synchronized. Called with target and value.</param>
        /// <returns>self</returns>
        ISyncField PreApply(Action<object, object> action);

        /// <summary>
        /// Instructs SyncField to use a buffer instead of syncing instantly (when <see cref="MP.WatchEnd"/> is called).
        /// </summary>
        /// <returns>self</returns>
        ISyncField SetBufferChanges();

        /// <summary>
        /// Instructs SyncField to synchronize only in debug mode.
        /// </summary>
        /// <returns>self</returns>
        ISyncField SetDebugOnly();

        /// <summary>
        /// Instructs SyncField to synchronize only if it's invoked by the host.
        /// </summary>
        /// <returns>self</returns>
        ISyncField SetHostOnly();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>self</returns>
        ISyncField SetVersion(int version);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">An object of type set in the <see cref="ISyncField"/>. Set to null if you're watching a static field.</param>
        /// <param name="index">Index in the field path set in <see cref="ISyncField"/>.</param>
        /// <returns>self</returns>
        void Watch(object target = null, object index = null);

        /// <summary>
        /// Manually syncs a field.
        /// </summary>
        /// <param name="target">An object of type set in the <see cref="ISyncField"/>. Set to null if you're watching a static field.</param>
        /// <param name="value">Value to apply to the synced field.</param>
        /// <param name="index">Index in the field path set in <see cref="ISyncField"/></param>
        /// <returns><see langword="true"/> if the change should be canceled.</returns>
        bool DoSync(object target, object value, object index = null);

        string ToString();
    }

    /// <summary>
    /// ISyncCall interface.
    /// </summary>
    /// <remarks>Used internally</remarks>
    public interface ISyncCall
    {
        /// <summary>
        /// Manually calls the synced method.
        /// </summary>
        /// <param name="target">Object currently bound to that method. Null if the method is static.</param>
        /// <param name="args">Parameters to call the method with.</param>
        /// <returns><see langword="true"/> if the original call should be canceled.</returns>
        bool DoSync(object target, params object[] args);
    }

    /// <summary>
    /// SyncMethod interface.
    /// </summary>
    /// <remarks>See <see cref="SyncMethodAttribute"/>, <see cref="MP.RegisterSyncMethod(MethodInfo, SyncType[])"/> and <see cref="MP.RegisterSyncMethod(Type, string, SyncType[])"/> to see how to use it.</remarks>
    public interface ISyncMethod : ISyncCall
    {
        /// <summary>
        /// Instructs SyncMethod to cancel synchronization if any arg is null.
        /// </summary>
        /// <returns>self</returns>
        ISyncMethod CancelIfAnyArgNull();

        /// <summary>
        /// Instructs SyncMethod to cancel synchronization if no map objects were selected during call replication.
        /// </summary>
        /// <returns>self</returns>
        ISyncMethod CancelIfNoSelectedMapObjects();

        /// <summary>
        /// Instructs SyncMethod to cancel synchronization if no world objects were selected during call replication.
        /// </summary>
        /// <returns>self</returns>
        ISyncMethod CancelIfNoSelectedWorldObjects();

        /// <summary>
        /// Use parameter's type's IExposable interface to transfer its data to other clients.
        /// </summary>
        /// <remarks>IExposable is the interface used for saving data to the save which means it utilizes IExposable.ExposeData() method.</remarks>
        /// <param name="index">Index at which parameter is to be marked to expose</param>
        /// <returns>self</returns>
        ISyncMethod ExposeParameter(int index);

        /// <summary>
        /// Currently unused in the Multiplayer mod.
        /// </summary>
        /// <param name="time">Milliseconds between resends</param>
        /// <returns>self</returns>
        ISyncMethod MinTime(int time);

        /// <summary>
        /// Instructs method to send context along with the call.
        /// </summary>
        /// <remarks>Context is restored after method is called.</remarks>
        /// <param name="context">One or more context flags</param>
        /// <returns>self</returns>
        ISyncMethod SetContext(SyncContext context);

        /// <summary>
        /// Instructs SyncMethod to synchronize only in debug mode.
        /// </summary>
        /// <returns>self</returns>
        ISyncMethod SetDebugOnly();

        /// <summary>
        /// Adds an Action that runs before a call is replicated on client.
        /// </summary>
        /// <param name="action">An action ran before a call is replicated on client. Called with target and value.</param>
        /// <returns>self</returns>
        ISyncMethod SetPreInvoke(Action<object, object[]> action);

        /// <summary>
        /// Adds an Action that runs after a call is replicated on client.
        /// </summary>
        /// <param name="action">An action ran after a call is replicated on client. Called with target and value.</param>
        /// <returns>self</returns>
        ISyncMethod SetPostInvoke(Action<object, object[]> action);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="version">Handler version</param>
        /// <returns>self</returns>
        ISyncMethod SetVersion(int version);

        string ToString();
    }

    // Todo: Document
    /// <summary>
    /// Sync delegate.
    /// </summary>
    /// <remarks>See <see cref="MP.RegisterSyncDelegate(Type, string, string)"/> and <see cref="MP.RegisterSyncDelegate(Type, string, string, string[], Type[])"/> to see how to use it.</remarks>
    public interface ISyncDelegate : ISyncCall
    {
        /// <summary>
        /// Instructs ISyncDelegate to cancel synchronization except for <param name="blacklist">
        /// </summary>
        /// <returns>self</returns>
        /// <param name="blacklist">field names to be excluded</param>
        ISyncDelegate CancelIfAnyFieldNull(params string[] blacklist);

        /// <summary>
        /// Instructs ISyncDelegate to cancel synchronization except for <param name="whitelist">
        /// </summary>
        /// <returns>self</returns>
        /// <param name="whitelist">Whitelist.</param>
        ISyncDelegate CancelIfFieldsNull(params string[] whitelist);

        /// <summary>
        /// Cancels if no selected objects.
        /// </summary>
        /// <returns>self</returns>
        ISyncDelegate CancelIfNoSelectedObjects();

        /// <summary>
        /// Removes the nulls from lists.
        /// </summary>
        /// <returns>self</returns>
        /// <param name="listFields">List fields.</param>
        ISyncDelegate RemoveNullsFromLists(params string[] listFields);

        /// <summary>
        /// Sets the context.
        /// </summary>
        /// <returns>self</returns>
        /// <param name="context">Context.</param>
        ISyncDelegate SetContext(SyncContext context);

        /// <summary>
        /// Sets the debug only.
        /// </summary>
        /// <returns>self</returns>
        ISyncDelegate SetDebugOnly();

        string ToString();
    }


    /// <summary>
    /// An attribute that marks a method as a SyncWorker for a type specified in its second parameter.
    /// </summary>
    /// <remarks>
    /// Method with this attribute has to be static.
    /// </remarks>
    /// <example>
    /// <para>An implementation that manually constructs an object.</para>
    /// <code>
    ///    [SyncWorkerAttribute]
    ///    public static void MySyncWorker(SyncWorker sync, ref MyClass inst)
    ///    {
    ///        if(!sync.isWriting)
    ///            inst = new MyClass("hello");
    ///        
    ///        sync.bind(ref inst.myField);
    ///    }
    /// </code>
    /// <para>An implementation that instead of creating a new object, references its existing one which resides in MyThingComp that inherits ThingComp class.</para>
    /// <para>Subclasses of ThingComp are sent as a reference by the multiplayer mod itself.</para>
    /// <code>
    ///    [SyncWorkerAttribute]
    ///    public static void MySyncWorker(SyncWorker sync, ref MyClass inst)
    ///    {
    ///        if(!sync.isWriting)
    ///            MyThingComp parent = null;
    ///            sync.Bind(ref parent);    // Receive its parent
    ///            inst = new MyClass(parent);
    ///        else
    ///            sync.Bind(ref inst.parent);    // Send its parent
    ///        
    ///        sync.bind(ref inst.myField);
    ///    }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public class SyncWorkerAttribute : Attribute
    {
        /// <summary>Decides if the type specified in the second parameter should also be used as a syncer for all of its subclasses.</summary>
        public bool isImplicit = false;

        /// <summary>Decides if the method should get an already constructed object in case of reading data.</summary>
        public bool shouldConstruct = false;
    }

    /// <summary>
    /// SyncWorker signature for adding new Types.
    /// </summary>
    /// <param name="obj">Target Type</param>
    /// <remarks><see cref="SyncWorkerAttribute"/> for usage examples.</remarks>
    public delegate void SyncWorkerDelegate<T>(SyncWorker sync, ref T obj);

    /// <summary>
    /// An abstract class that can be both a reader and a writer depending on implementation.
    /// </summary>
    /// <remarks>See <see cref="ISynchronizable"/> and <see cref="SyncWorkerAttribute"/> for usage examples.</remarks>
    public abstract class SyncWorker
    {
        /// <summary><see langword="true"/> if is currently writing.</summary>
        public readonly bool isWriting;

        protected SyncWorker(bool isWriting)
        {
            this.isWriting = isWriting;
        }

        public void Write<T>(T obj, SyncType type)
        {
            if (isWriting)
            {
                Bind(ref obj, type);
            }
        }

        /// <summary>
        /// Write the specified obj, only active during writing.
        /// </summary>
        /// <param name="obj">Object to write.</param>
        /// <typeparam name="T">Type to write.</typeparam>
        public void Write<T>(T obj) {
            if (isWriting) {
                Bind(ref obj);
            }
        }

        public T Read<T>(SyncType type)
        {
            T obj = default(T);

            if (isWriting)
            {
                return obj;
            }

            Bind(ref obj, type);

            return obj;
        }

        /// <summary>
        /// Read the specified Type from the memory stream, only active during reading.
        /// </summary>
        /// <returns>The requested Type object. Null if writing.</returns>
        /// <typeparam name="T">The Type to read.</typeparam>
        public T Read<T>() {
            T obj = default(T);

            if (isWriting) {
                return obj;
            }

            Bind(ref obj);

            return obj;
        }

        public abstract void Bind<T>(ref T obj, SyncType type);

        /// <summary>Reads or writes a <see cref="Type"/> referenced by <paramref name="type"/>.</summary>
        /// <typeparam name="T">Base type that <paramref name="type"/> derives from.</typeparam>
        /// <param name="type">type to bind</param>
        public abstract void BindType<T>(ref Type type);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref byte obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref sbyte obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref short obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref ushort obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref int obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref uint obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref long obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref ulong obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref float obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref double obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref string obj);

        /// <summary>Reads or writes an object referenced by <paramref name="obj"/>.</summary>
        /// <param name="obj">object to bind</param>
        public abstract void Bind(ref bool obj);

        /// <summary>
        /// Reads or writes an object referenced by <paramref name="obj"/>
        /// </summary>
        /// <remarks>Can read/write types using user defined syncers, <see cref="ISynchronizable"/>s and readers/writers implemented by the multiplayer mod.</remarks>
        /// <typeparam name="T">type of the object to bind</typeparam>
        /// <param name="obj">object to bind</param>
        public abstract void Bind<T>(ref T obj);

        /// <summary>
        /// Uses reflection to bind a field or property
        /// </summary>
        /// <param name="obj">
        /// <para>object where the field or property can be found</para>
        /// <para>if null, <paramref name="name"/> will point at field from the global namespace</para>
        /// </param>
        /// <param name="name">path to the field or property</param>
        public abstract void Bind(object obj, string name);

        /// <summary>
        /// Reads or writes an object inheriting <see cref="ISynchronizable"/> interface. 
        /// </summary>
        /// <remarks>Does not create a new object.</remarks>
        /// <param name="obj">object to bind</param>
        public void Bind(ref ISynchronizable obj)
        {
            obj.Sync(this);
        }
    }

    /// <summary>
    /// An interface that allows syncing objects that inherit it.
    /// </summary>
    public interface ISynchronizable
    {
        /// <summary>
        /// An entry point that is used when object is to be read/written.
        /// </summary>
        /// <remarks>
        /// <para>Requires a default constructor that takes no parameters.</para>
        /// <para>Check <see cref="SyncWorkerAttribute"/> to see how to make a syncer that allows for a manual object construction.</para>
        /// </remarks>
        /// <param name="sync">A SyncWorker that will read/write data bound with Bind methods.</param>
        /// <example>
        /// <para>A simple implementation that binds object's fields x, y, z for reading/writing.</para>
        /// <code>
        /// public void Sync(SyncWorker sync)
        ///    {
        ///        sync.Bind(ref this.x);
        ///        sync.Bind(ref this.y);
        ///        sync.Bind(ref this.z);
        ///    }
        /// </code>
        /// 
        /// <para>An implementation that sends field a, but saves it back into field b when it's received.</para>
        /// <code>
        /// public void Sync(SyncWorker sync)
        ///    {
        ///        if(sync.isWriting)
        ///            sync.Bind(ref this.a);
        ///        else
        ///            sync.Bind(ref this.b);
        ///    }
        /// </code>
        /// </example>
        void Sync(SyncWorker sync);
    }

    /// <summary>
    /// An attribute that marks a method for pause lock checking It needs a <see cref="bool"/> return type and a single <see cref="Map"/> parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PauseLockAttribute : Attribute
    { }

    /// <summary>
    /// Signature for adding new local pause locking methods
    /// </summary>
    /// <param name="map">Current map to check if it should be paused</param>
    /// <returns><see langword="true"/> if time should be paused on the specific map</returns>
    public delegate bool PauseLockDelegate(Map map);

    /// <summary>
    /// Objects implementing this marker interface sync their exact type and all declared fields.
    /// This is useful when syncing type hierarchies by value.
    /// <remarks>The synced object is created uninitialized using reflection (no constructor is called).</remarks>
    /// </summary>
    public interface ISyncSimple { }

    /// <summary>
    /// A ThingFilter context provides information for syncing ThingFilter interactions.
    /// Inheriting objects should store the ThingFilter's owner in a record property.
    /// The type exists because vanilla ThingFilters don't store references to their owners.
    /// </summary>
    public abstract record ThingFilterContext : ISyncSimple
    {
        public abstract ThingFilter Filter { get; }
        public abstract ThingFilter ParentFilter { get; }
        public virtual IEnumerable<SpecialThingFilterDef> HiddenFilters => null;
    }

    public interface IAPI
    {
        bool IsHosting { get; }
        bool IsInMultiplayer { get; }
        string PlayerName { get; }
        bool IsExecutingSyncCommand { get; }
        bool IsExecutingSyncCommandIssuedBySelf { get; }

        void SetThingFilterContext(ThingFilterContext context);

        void WatchBegin();
        void Watch(Type targetType, string fieldName, object target = null, object index = null);
        void Watch(object target, string fieldName, object index = null);
        void Watch(string memberPath, object target = null, object index = null);
        void WatchEnd();

        void RegisterAll(Assembly assembly);

        ISyncField RegisterSyncField(Type targetType, string memberPath);
        ISyncField RegisterSyncField(FieldInfo field);

        ISyncMethod RegisterSyncMethod(Type type, string methodOrPropertyName, SyncType[] argTypes = null);
        ISyncMethod RegisterSyncMethod(MethodInfo method, SyncType[] argTypes);

        ISyncDelegate RegisterSyncDelegate(Type type, string nestedType, string method);
        ISyncDelegate RegisterSyncDelegate(Type inType, string nestedType, string methodName, string[] fields, Type[] args = null);

        void RegisterSyncWorker<T>(SyncWorkerDelegate<T> syncWorkerDelegate, Type targetType = null, bool isImplicit = false, bool shouldConstruct = false);

        void RegisterDialogNodeTree(Type type, string methodOrPropertyName, SyncType[] argTypes = null);

        void RegisterDialogNodeTree(MethodInfo method);

        [Obsolete($"Use {nameof(Session)} instead.")]
        void RegisterPauseLock(PauseLockDelegate pauseLock);

        Thing GetThingById(int id);
        bool TryGetThingById(int id, out Thing value);

        IReadOnlyList<IPlayerInfo> GetPlayers();
        IPlayerInfo GetPlayerById(int id);

        ISessionManager GetGlobalSessionManager();
        ISessionManager GetLocalSessionManager(Map map);
        void SetCurrentSessionWithTransferables(ISessionWithTransferables session);
    }

    /// <summary>
    /// An interface for the class holding player data
    /// </summary>
    public interface IPlayerInfo
    {
        /// <summary>
        /// ID of the current player
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Username of the current player
        /// </summary>
        string Username { get; }
        /// <summary>
        /// <see langword="true"/> if the current player is Arbiter instance, <see langword="false"/> in every other case
        /// </summary>
        bool IsArbiter { get; }

        /// <summary>
        /// <see cref="Map.Index"/> of the map the player is on
        /// </summary>
        int CurrentMapIndex { get; }
        /// <summary>
        /// The map the current player is on
        /// </summary>
        Map CurrentMap { get; }

        /// <summary>
        /// List of all the things the player has selected, as numeric IDs
        /// </summary>
        /// <remarks>Generally use <see cref="SelectedThings"/>, unless you're able to access the IDs directly.</remarks>
        IReadOnlyList<int> SelectedThingsByIds { get; }
        /// <summary>
        /// List of all the things the player has selected
        /// </summary>
        IReadOnlyList<Thing> SelectedThings { get; }
    }

    public interface ISessionManager
    {
        /// <summary>
        /// Returns the list of all currently active sessions for this specific <see cref="ISessionManager"/>.
        /// </summary>
        IReadOnlyList<Session> AllSessions { get; }
        /// <summary>
        /// Returns the list of all currently active exposable sessions for this specific <see cref="ISessionManager"/>.
        /// </summary>
        IReadOnlyList<ExposableSession> ExposableSessions { get; }
        /// <summary>
        /// Returns the list of all currently active semi-persistent sessions for this specific <see cref="ISessionManager"/>.
        /// </summary>
        IReadOnlyList<SemiPersistentSession> SemiPersistentSessions { get; }
        /// <summary>
        /// Returns the list of all currently active ticking sessions for this specific <see cref="ISessionManager"/>.
        /// </summary>
        IReadOnlyList<ITickingSession> TickingSessions { get; }
        /// <summary>
        /// A convenience property for checking if any of the sessions is active.
        /// </summary>
        bool AnySessionActive { get; }

        /// <summary>
        /// Adds a new session to the list of active sessions.
        /// </summary>
        /// <param name="session">The session to try to add to active sessions.</param>
        /// <returns><see langword="true"/> if the session was added to active ones, <see langword="false"/> if there was a conflict between sessions.</returns>
        bool AddSession(Session session);

        /// <summary>
        /// Tries to get a conflicting session (through the use of <see cref="ISessionWithCreationRestrictions"/>) or, if there was none, returns the input <paramref name="session"/>.
        /// </summary>
        /// <param name="session">The session to try to add to active sessions.</param>
        /// <returns>A session that was conflicting with the input one, or the input itself if there were no conflicts. It may be of a different type than the input.</returns>
        Session GetOrAddSessionAnyConflict(Session session);

        /// <summary>
        /// Tries to get a conflicting session (through the use of <see cref="ISessionWithCreationRestrictions"/>) or, if there was none, returns the input <paramref name="session"/>.
        /// </summary>
        /// <param name="session">The session to try to add to active sessions.</param>
        /// <returns>A session that was conflicting with the input one if it's the same type (<c>other is T</c>), null if it's a different type, or the input itself if there were no conflicts.</returns>
        T GetOrAddSession<T>(T session) where T : Session;

        /// <summary>
        /// Tries to remove a session from active ones.
        /// </summary>
        /// <param name="session">The session to try to remove from the active sessions.</param>
        /// <returns><see langword="true"/> if successfully removed from <see cref="AllSessions"/>. Doesn't correspond to if it was successfully removed from other lists of sessions.</returns>
        bool RemoveSession(Session session);

        /// <summary>
        /// Returns the first active session of specific type.
        /// </summary>
        /// <typeparam name="T">Type of the session to retrieve.</typeparam>
        /// <returns>The first session of specified type, or <see langword="null"/> if there are none.</returns>
        T GetFirstOfType<T>() where T : Session;

        /// <summary>
        /// Returns the session with specific ID of specific type.
        /// </summary>
        /// <param name="id">The ID of the session to search for.</param>
        /// <typeparam name="T">Type of the session to retrieve.</typeparam>
        /// <returns>The session with provided ID and of specified type, or <see langword="null"/> if there are none.</returns>
        T GetFirstWithId<T>(int id) where T : Session;

        /// <summary>
        /// Returns the session with specific ID.
        /// </summary>
        /// <param name="id">The ID of the session to search for.</param>
        /// <returns>The session with provided ID, or <see langword="null"/> if there are none.</returns>
        Session GetFirstWithId(int id);

        /// <summary>
        /// Checks if any of active sessions is currently pausing the game.
        /// </summary>
        /// <param name="map">The map at which the sessions would check if the game is paused. Global session manager accepts <see langword="null"/> for global pausing.</param>
        /// <returns><see langword="true"/> if any session is active, <see langword="false"/> otherwise.</returns>
        /// <remarks>Local session managers expect the <paramref name="map"/> to be the same as the map it's attached to.</remarks>
        bool IsAnySessionCurrentlyPausing(Map map); // Is it necessary for the API?
    }

    /// <summary>
    /// <para>Required by sessions dealing with transferables, like trading or caravan forming. By implementing this interface, Multiplayer will handle majority of syncing of changes in transferables.</para>
    /// <para>When drawing the dialog tied to this session, you'll have to set <see cref="MP.SetCurrentSessionWithTransferables"/> to the proper session, and set it to null once done.</para>
    /// </summary>
    /// <remarks>For safety, make sure to set <see cref="MP.SetCurrentSessionWithTransferables"/> in <see langword="try"/> and unset in <see langword="finally"/>.</remarks>
    public interface ISessionWithTransferables
    {
        /// <summary>
        /// Used when syncing data across players, specifically to retrieve <see cref="Transferable"/> based on the <see cref="Thing"/> it has.
        /// </summary>
        /// <param name="thingId"><see cref="Thing.thingIDNumber"/> of the <see cref="Thing"/>.</param>
        /// <returns><see cref="Transferable"/> which corresponds to a <see cref="Thing"/> with specific <see cref="Thing.thingIDNumber"/>.</returns>
        Transferable GetTransferableByThingId(int thingId);

        /// <summary>
        /// Called when the count in a specific <see cref="Transferable"/> was changed.
        /// </summary>
        /// <param name="tr">Transferable whose count was changed.</param>
        void Notify_CountChanged(Transferable tr);
    }

    /// <summary>
    /// Interface used by sessions that have restrictions based on other existing sessions, for example limiting them to only 1 session of specific type.
    /// </summary>
    public interface ISessionWithCreationRestrictions
    {
        /// <summary>
        /// <para>Method used to check if the current session can be created by checking other <see cref="ISession"/>.</para>
        /// <para>Only sessions in the current context are checked (local map sessions or global sessions).</para>
        /// </summary>
        /// <param name="other">The other session the current one is checked against. Can be of different type.</param>
        /// <remarks>Currently only the current class checks against the existing ones - the existing classed don't check against this one.</remarks>
        /// <returns><see langword="true"/> if the current session should be created, <see langword="false"/> otherwise</returns>
        bool CanExistWith(Session other);
    }

    /// <summary>
    /// Used by sessions that are are required to tick together with the map/world.
    /// </summary>
    public interface ITickingSession
    {
        /// <summary>
        /// Called once per session when the map (for local sessions) or the world (for global sessions) is ticking.
        /// </summary>
        /// <remarks>The sessions are iterated over backwards using a for loop, so it's safe for them to remove themselves from the session manager.</remarks>
        void Tick();
    }
}
