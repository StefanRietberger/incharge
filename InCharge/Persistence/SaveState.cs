using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace InCharge.Persistence
{
    [Serializable]
    public class SaveState
    {
        private Dictionary<Type, List<object>> objects = new Dictionary<Type, List<object>>();

        /// <summary>
        /// Puts a persistable object to the save state
        /// </summary>
        /// <param name="persistable"></param>
        public void Put(object persistable)
        {
            var type = persistable.GetType();
            List<object> list;
            objects.TryGetValue(type, out list);
            if (list == null)
            {
                list = new List<object>();
                objects.Add(type, list);
            }
            list.Add(persistable);
        }

        /// <summary>
        /// Gets all persistable objects of a given type from the save state
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Get<T>() where T : ISerializable
        {
            var list = objects[typeof(T)];
            return list as List<T>;
        }

        /// <summary>
        /// Saves the state to disk
        /// </summary>
        public void Save(FileInfo saveFile)
        {
            using (FileStream f = new FileStream(saveFile.FullName, FileMode.Create))
            {
                objects.OrderBy(x => x.Key);
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
