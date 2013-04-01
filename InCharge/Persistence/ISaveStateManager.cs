using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynapseGaming.LightingSystem.Core;

namespace InCharge.Persistence
{
    /// <summary>
    /// Defines the behaviour contract for a save state manager
    /// </summary>
    interface ISaveStateManager : IManagerService, IManager, IUnloadable
    {
        List<SaveMeta> GetSaveStateMeta();
        SaveState LoadSaveState(SaveMeta saveMeta);
        void WriteCurrentSaveState();
        void SaveNewState();
        void LoadMostRecentSave();
        bool HasSaves { get; }
        SaveState CurrentSaveState { get; }
        SaveMeta CurrentSaveStateMeta { get; }
    }
}
