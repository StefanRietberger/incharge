using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynapseGaming.LightingSystem.Core;
using System.IO;

namespace InCharge.Persistence
{
    /// <summary>
    /// Manages save states
    /// </summary>
    public class SaveStateManager : ISaveStateManager
    {
        private int processOrder;
        private IManagerServiceProvider sceneInterface;
        private DirectoryInfo saveDir;

        private SaveState currentSaveState;

        public SaveState CurrentSaveState
        {
            get { return currentSaveState; }
        }
        private SaveMeta currentSaveStateMeta;

        public SaveMeta CurrentSaveStateMeta
        {
            get { return currentSaveStateMeta; }           
        }

        public SaveStateManager(IManagerServiceProvider sceneInterface)
        {
            this.sceneInterface = sceneInterface;
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InCharge/SaveGames/");
            this.saveDir = new DirectoryInfo(path);

            if (!this.saveDir.Exists)
            {
                this.saveDir.Create();
            }
        }

        /// <summary>
        /// Returns a list of save state meta data
        /// </summary>
        /// <returns></returns>
        public List<SaveMeta> GetSaveStateMeta()
        {
            List<SaveMeta> states = new List<SaveMeta>();
            var saveFiles = this.saveDir.EnumerateFiles("*.sav");

            foreach (var saveFile in saveFiles)
            {
                var meta = new SaveMeta();
                meta.SaveFile = saveFile;
                states.Add(meta);
            }

            return states;
        }

        /// <summary>
        /// Loads a save state described by the passed meta data
        /// </summary>
        /// <param name="saveMeta"></param>
        /// <returns></returns>
        public SaveState LoadSaveState(SaveMeta saveMeta)
        {
            var state = saveMeta.LoadSaveState();
            this.currentSaveState = state;
            this.currentSaveStateMeta = saveMeta;
            return state;
        }

        #region IManagerService Members

        int IManagerService.ManagerProcessOrder
        {
            get
            {
                return this.processOrder;
            }
            set
            {
                this.processOrder = value;
            }
        }

        Type IManagerService.ManagerType
        {
            get { return typeof(ISaveStateManager); }
        }

        #endregion

        #region IManager Members

        void IManager.ApplyPreferences(ISystemPreferences preferences)
        {
            // TODO: might want to let users set save dir path here
        }

        void IManager.Clear()
        {
            // return to default settings
        }

        IManagerServiceProvider IManager.OwnerSceneInterface
        {
            get { return this.sceneInterface; }
        }

        #endregion

        #region IUnloadable Members

        void IUnloadable.Unload()
        {
            // clean up any file references or similar
        }

        #endregion


        /// <summary>
        /// Loads the most recent save state into the managers current state storage
        /// </summary>
        public void LoadMostRecentSave()
        {
            var metaList = this.GetSaveStateMeta();
            var mostRecent = metaList.OrderByDescending(m => m.SaveDate).FirstOrDefault();
            if (mostRecent != null)
            {
                this.currentSaveState = this.LoadSaveState(mostRecent);
            }
        }

        /// <summary>
        /// Checks if there are any save games at all
        /// </summary>
        /// <returns></returns>
        public bool HasSaves
        {
            get
            {
                return this.GetSaveStateMeta().Count > 0;
            }
        }


        public void SaveNewState()
        {
            var saveID = Guid.NewGuid();
            // Meta data file name
            var newMetaFile = new FileInfo(string.Format("{0}/{1}.sav", this.saveDir, saveID.ToString("N")));

            var meta = new SaveMeta();
            // save game file name
            meta.SaveFile = new FileInfo(string.Format("{0}/{1}.dat", this.saveDir, saveID.ToString("N")));
            meta.Save(newMetaFile);

            this.currentSaveState = new SaveState();
            this.currentSaveStateMeta = meta;
        }


        public void WriteCurrentSaveState()
        {
            this.currentSaveStateMeta.WriteSaveState(this.currentSaveState);
        }

    }
}
