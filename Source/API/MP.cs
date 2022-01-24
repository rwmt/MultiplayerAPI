﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Verse;

namespace Multiplayer.API
{
    /// <summary>
    /// The primary static class that contains methods used to interface with the multiplayer mod.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MP
    {
        /// <summary>Contains the API version</summary>
        public const string API = "0.3";

        /// <value>
        /// Returns <see langword="true"/> if API is initialized.
        /// </value>
        public static readonly bool enabled = false;

        private static readonly IAPI Sync;

        static MP()
        {
            var mpAssembly = LoadedModManager.RunningMods
                .SelectMany(m => m.assemblies.loadedAssemblies)
                .FirstOrDefault(a => a.GetName().Name == "Multiplayer");

            if (mpAssembly == null)
            {
                Sync = new Dummy();

                return;
            }

            // This can fail in older MP versions halting mod loading
            try {
                Sync = (IAPI) mpAssembly
                .GetType("Multiplayer.Common.MultiplayerAPIBridge")
                .GetField("Instance")
                .GetValue(null);

                enabled = true;
            } catch(Exception e) {
                Log.Error("Multiplayer mod detected but it has no MPAPI Bridge\n\n" + e);
            }
        }

        // Some nice shortcuts ready to be inlined by the JIT compiler :)

        /// <value>
        /// Returns <see langword="true"/> if currently running on a host.
        /// </value>
        public static bool IsHosting => Sync.IsHosting;

        /// <value>
        /// Returns <see langword="true"/> if currently running in a multiplayer session (both on client and host).
        /// </value>
        public static bool IsInMultiplayer => Sync.IsInMultiplayer;

        /// <value>
        /// Returns local player's name.
        /// </value>
        public static string PlayerName => Sync.PlayerName;

        /// <summary>
        /// Starts a new synchronization stack.
        /// </summary>
        /// <remarks>
        /// <para>Has to be called before invoking Watch methods.</para>
        /// <para>See also <see cref="ISyncField.Watch(object, object)"/>.</para>
        /// </remarks>
        public static void WatchBegin() => Sync.WatchBegin();

        /// <summary>
        /// Helper method for <see cref="ISyncField.Watch"/> given a type.
        /// </summary>
        /// <param name="type">An object of type set in the <see cref="ISyncField"/> to watch, for static types</param>
        /// <param name="fieldName"><see cref="ISyncField"/> name of the field to watch for changes</param>
        /// <param name="index">Index in the field path set in <see cref="ISyncField"/></param>
        public static void Watch(Type type, string fieldName, object index = null) => Sync.Watch(type, fieldName, index);

        /// <summary>
        /// Helper method for <see cref="ISyncField.Watch"/> given an instance.
        /// </summary>
        /// <param name="target">An object of type set in the <see cref="ISyncField"/> to watch</param>
        /// <param name="fieldName"><see cref="ISyncField"/> name of the field to watch for changes</param>
        /// <param name="index">Index in the field path set in <see cref="ISyncField"/></param>
        public static void Watch(object target, string fieldName, object index = null) => Sync.Watch(target, fieldName, index);

        /// <summary>
        /// Helper method for <see cref="ISyncField.Watch"/> given an instance.
        /// </summary>
        /// <param name="memberPath"><see cref="ISyncField"/> the memberPath of the ISyncField</param>
        /// <param name="target">An object of type set in the <see cref="ISyncField"/> to watch, null for static</param>
        /// <param name="index">Index in the field path set in <see cref="ISyncField"/></param>
        public static void Watch(string memberPath, object target = null, object index = null) => Sync.Watch(memberPath, target, index);

        /// <summary>
        /// Ends the current synchronization stack and executes it.
        /// </summary>
        /// <remarks>
        /// <para>Has to be called after invoking Watch methods.</para>
        /// <para>See also <see cref="ISyncField.Watch(object, object)"/>.</para>
        /// </remarks>
        public static void WatchEnd() => Sync.WatchEnd();

        /// <summary>
        /// Searches current assembly for MPAPI annotations and registers them
        /// <see cref="SyncMethodAttribute"/>
        /// <see cref="SyncFieldAttribute"/>
        /// </summary>
        public static void RegisterAll() => RegisterAll(new StackTrace().GetFrame(1).GetMethod().ReflectedType.Assembly);

        /// <summary>
        /// Searches the given assembly for MPAPI annotations and registers them
        /// <param name="assembly">The assembly</param>
        /// <see cref="SyncMethodAttribute"/>
        /// <see cref="SyncFieldAttribute"/>
        /// </summary>
        public static void RegisterAll(Assembly assembly) => Sync.RegisterAll(assembly);

        /// <summary>
        /// Registers a field for syncing and returns it's <see cref="ISyncField"/>.
        /// </summary>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncFieldAttribute"/> instead, unless you have to otherwise.</para>
        /// <para>They must be Watched between MP.WatchBegin and MP.WatchEnd with the MP.Watch* methods</para>
        /// </remarks>
        /// <param name="targetType">
        /// <para>Type of the target class that contains the specified member</para>
        /// <para>if null, <paramref name="memberPath"/> will point at field from the global namespace in the "Type/fieldName" format.</para>
        /// </param>
        /// <param name="memberPath">Path to a member. If the member is to be indexed, it has to end with /[] eg. <c>"myArray/[]"</c></param>
        /// <returns>A new registered <see cref="ISyncField"/></returns>
        public static ISyncField RegisterSyncField(Type targetType, string memberPath) => Sync.RegisterSyncField(targetType, memberPath);

        /// <summary>
        /// Registers a field for syncing and returns it's <see cref="ISyncField"/>.
        /// </summary>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncFieldAttribute"/> instead, unless you have to otherwise.</para>
        /// <para>They must be Watched between MP.WatchBegin and MP.WatchEnd with the MP.Watch* methods</para>
        /// </remarks>
        /// <param name="field">FieldInfo of a field to register</param>
        /// <returns>A new registered <see cref="ISyncField"/></returns>
        public static ISyncField RegisterSyncField(FieldInfo field) => Sync.RegisterSyncField(field);

        /// <summary>
        /// Registers a method for syncing and returns its <see cref="ISyncMethod"/>.
        /// </summary>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncMethodAttribute"/> instead, unless you have to otherwise.</para>
        /// </remarks>
        /// <param name="type">Type that contains the method</param>
        /// <param name="methodOrPropertyName">Name of the method</param>
        /// <param name="argTypes">Method's parameter types</param>
        /// <returns>A new registered <see cref="ISyncMethod"/></returns>
        public static ISyncMethod RegisterSyncMethod(Type type, string methodOrPropertyName, SyncType[] argTypes = null) => Sync.RegisterSyncMethod(type, methodOrPropertyName, argTypes);

        /// <summary>
        /// Registers a method for syncing and returns its <see cref="ISyncMethod"/>.
        /// </summary>
        /// <param name="method">MethodInfo of a method to register</param>
        /// <param name="argTypes">Method's parameter types</param>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncMethodAttribute"/> instead, unless you have to otherwise.</para>
        /// </remarks>
        /// <returns>A new registered <see cref="ISyncMethod"/></returns>
        /// <example>
        /// Register a method for syncing using reflection and set it to debug only.
        /// <code>
        ///    RegisterSyncMethod(typeof(MyType).GetMethod(nameof(MyType.MyMethod))).SetDebugOnly();
        /// </code>
        /// </example>
        public static ISyncMethod RegisterSyncMethod(MethodInfo method, SyncType[] argTypes = null) => Sync.RegisterSyncMethod(method, argTypes);

        /// <summary>
        /// Registers the syncDelegate. Handles anonymous nested types, you will have to figure out the name of your target by decompiling.
        /// </summary>
        /// <returns>The sync delegate.</returns>
        /// <param name="type">Type.</param>
        /// <param name="nestedType">Nested type.</param>
        /// <param name="method">Method.</param>
        public static ISyncDelegate RegisterSyncDelegate(Type type, string nestedType, string method) => Sync.RegisterSyncDelegate(type, nestedType, method);

        /// <summary>
        /// Registers the syncDelegate. Handles anonymous nested types, you will have to figure out the name of your target by decompiling.
        /// </summary>
        /// <returns>The sync delegate.</returns>
        /// <param name="inType">In type.</param>
        /// <param name="nestedType">Nested type.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="fields">Fields.</param>
        /// <param name="args">Arguments.</param>
        public static ISyncDelegate RegisterSyncDelegate(Type inType, string nestedType, string methodName, string[] fields, Type[] args = null) => Sync.RegisterSyncDelegate(inType, nestedType, methodName, fields, args);

        /// <summary>
        /// Registers the SyncWorker based on SyncWorkerDelegate.
        /// </summary>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncWorkerAttribute"/> instead, unless you have to otherwise.</para>
        /// </remarks>
        /// <param name="syncWorkerDelegate">Sync worker delegate.</param>
        /// <param name="targetType">Type to handle.</param>
        /// <param name="isImplicit">If set to <c>true</c> the SyncWorker will handle the type and all the derivate Types.</param>
        /// <param name="shouldConstruct">If set to <c>true</c> the SyncWorker will be provided with an instance created with no arguments.</param>
        /// <typeparam name="T">Type to handle.</typeparam>
        public static void RegisterSyncWorker<T>(SyncWorkerDelegate<T> syncWorkerDelegate, Type targetType = null, bool isImplicit = false, bool shouldConstruct = false) => Sync.RegisterSyncWorker(syncWorkerDelegate, targetType, isImplicit: isImplicit, shouldConstruct: shouldConstruct);

        /// <summary>
        /// Registers a method which opens a <see cref="Dialog_NodeTree"/>. The options picked by players will then be synced between all clients.
        /// </summary>
        /// <param name="type">Type that contains the method</param>
        /// <param name="methodOrPropertyName">Name of the method</param>
        /// <param name="argTypes">Method's parameter types</param>
        public static void RegisterSyncDialogNodeTree(Type type, string methodOrPropertyName, SyncType[] argTypes = null) => Sync.RegisterDialogNodeTree(type, methodOrPropertyName, argTypes);

        /// <summary>
        /// Registers a method which opens a <see cref="Dialog_NodeTree"/>. The options picked by players will then be synced between all clients.
        /// </summary>
        /// <param name="method">MethodInfo of a method to register</param>
        /// <param name="argTypes">Method's parameter types</param>
        /// <remarks>
        /// <para>It's recommended to use <see cref="SyncDialogNodeTreeAttribute"/> instead, unless you have to otherwise.</para>
        /// <para>It can be combined with <see cref="RegisterSyncMethod"/> so the call will be replicated by the MPApi on all clients automatically.</para>
        /// </remarks>
        /// <example>
        /// Register a method creating a <see cref="Dialog_NodeTree"/> for syncing using reflection and set it to debug only.
        /// <code>
        ///    RegisterSyncDialogNodeTree(typeof(MyType).GetMethod(nameof(MyType.MyMethod))).SetDebugOnly();
        /// </code>
        /// </example>
        public static void RegisterSyncDialogNodeTree(MethodInfo method) => Sync.RegisterDialogNodeTree(method);

        /// <summary>
        /// Registers a delegate which will be called to check if the game should be paused on specific map.
        /// In case async time is active, only that map will be paused, otherwise all of them will be paused.
        /// If async time is enabled, it'll also be called globally with <see langword="null"/> as the parameter, which will affect all maps. It's skipped if async time is disabled.
        /// </summary>
        /// <param name="pauseLock">Delegate with <see cref="Map"/> as the only parameter returning a <see cref="bool"/> which will be called to see if the game should be paused</param>
        /// <remarks>It's recommended to use <see cref="PauseLockAttribute"/> instead, unless you have to otherwise.</remarks>
        /// <example>
        /// Register a method as a delagate for forced pause locking
        /// <code>
        ///     void Register() => RegisterPauseLock(MyMethod);
        ///     void MyMethod(Map map) => return MyOtherClass.shouldPause;
        /// </code>
        ///
        /// Register a dynamic method as a delegate for forced pause locking
        /// <code>
        ///     RegisterPauseLock(map => MyOtherClass.shouldPause);
        /// </code>
        /// </example>
        public static void RegisterPauseLock(PauseLockDelegate pauseLock) => Sync.RegisterPauseLock(pauseLock);
    }
}
