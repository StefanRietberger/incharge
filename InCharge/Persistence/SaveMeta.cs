using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace InCharge.Persistence
{
    /// <summary>
    /// Stores and provides meta data to save states
    /// </summary>
    [Serializable]
    public class SaveMeta
    {
        /// <summary>
        /// Described save file location
        /// </summary>
        private FileInfo saveFile;

        public FileInfo SaveFile
        {
            get { return saveFile; }
            set { saveFile = value; }
        }

        /// <summary>
        /// Date of last save
        /// </summary>
        public DateTime SaveDate
        {
            get { return saveFile.LastWriteTime; }
        }

        public SaveMeta()
        {
        }

        public SaveState LoadSaveState()
        {
            return SaveState.Load(this.saveFile);
        }

        public void WriteSaveState(SaveState state)
        {
            state.Save(this.saveFile);
        }

        /// <summary>
        /// Saves the state to disk
        /// </summary>
        public void Save(FileInfo saveFile)
        {
            using (FileStream f = new FileStream(saveFile.FullName, FileMode.Create))
            {               
                var formatter = new BinaryFormatter();
                formatter.Serialize(f, this);
            }
        }

        /// <summary>
        /// Loads a state from disk
        /// </summary>
        public static SaveState Load(FileInfo saveFile)
        {
            using (FileStream f = new FileStream(saveFile.FullName, FileMode.Open))
            {
                SaveState loadedState = new SaveState();

                var formatter = new BinaryFormatter();

                loadedState = formatter.Deserialize(f) as SaveState;

                return loadedState;
            }
        }
    }
}
