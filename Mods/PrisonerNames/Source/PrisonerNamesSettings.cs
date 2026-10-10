using System;
using System.Collections.Generic;
using Oxygendsl.Core;
using Oxygendsl.Mod;
using Verse;

namespace Odf.PrisonerNames
{
    sealed class PrisonerNamesSettingsValues : IOxygenSettingsValues
    {
        public bool GameAvailable
        {
            get { return GameComponent_PrisonerNames.Current != null; }
        }

        public object Get(string key, string scope, object defaultValue)
        {
            if (!string.Equals(scope, "game", StringComparison.Ordinal) || !string.Equals(key, "enabled", StringComparison.Ordinal))
                return defaultValue;
            GameComponent_PrisonerNames runtime = GameComponent_PrisonerNames.Current;
            return runtime == null ? false : runtime.Enabled;
        }

        public void Set(string key, string scope, object value)
        {
            if (!string.Equals(scope, "game", StringComparison.Ordinal) || !string.Equals(key, "enabled", StringComparison.Ordinal))
                return;
            GameComponent_PrisonerNames runtime = GameComponent_PrisonerNames.Current;
            if (runtime == null) return;
            if (!(value is bool enabled)) throw new ArgumentException("enabled must be bool.", nameof(value));
            runtime.SetEnabled(enabled);
        }

    }

    sealed class PrisonerNamesModule : IOxygenHostValue
    {
        public bool TryGetMember(string name, out object value)
        {
            switch (name)
            {
                case "open_editor":
                    value = Actions.OpenEditor;
                    return true;
                case "game_available":
                    value = Predicates.GameAvailable;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }


        static class Actions
        {
            public static readonly IOxygenHostCallable OpenEditor = new ActionCallable(PrisonerNamesApi.OpenWindow);
        }

        static class Predicates
        {
            public static readonly IOxygenHostCallable GameAvailable = new BoolCallable(delegate
            {
                return GameComponent_PrisonerNames.Current != null;
            });
        }
    }


    sealed class ActionCallable : IOxygenHostCallable
    {
        readonly Action action;

        public ActionCallable(Action action) { this.action = action; }

        public object Invoke(IList<object> positional, IDictionary<string, object> named)
        {
            if (action != null) action();
            return null;
        }
    }

    sealed class BoolCallable : IOxygenHostCallable
    {
        readonly Func<bool> predicate;

        public BoolCallable(Func<bool> predicate) { this.predicate = predicate; }

        public object Invoke(IList<object> positional, IDictionary<string, object> named)
        {
            return predicate != null && predicate();
        }
    }

    static class PrisonerNamesHost
    {
        internal const string PackageId = "odf.nanaloveyuki.prisonernames";
        internal const string ModulePath = "prisonernames";
        internal static void Register(Mod owner)
        {
            OxygenSettingsHost.Modules.Register(PackageId, ModulePath, new PrisonerNamesModule());
            OxygenSettingsHost.Register(owner, new PrisonerNamesSettingsValues());
            OxygenEditorHost.RegisterDocuments(PackageId, PrisonerEditorDocuments.Current);
        }

        internal static void Subscribe()
        {
            if (subscription != null) return;
            subscription = OxygenEvents.Subscribe(PackageId, message =>
            {
                GameComponent_PrisonerNames runtime = GameComponent_PrisonerNames.Current;
                if (runtime != null) runtime.Handle(message);
            });
        }

        static IDisposable subscription;
    }
}
