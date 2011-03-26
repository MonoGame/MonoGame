
using System;
using Microsoft.Xna.Framework.Storage;
using System.Xml.Serialization;
using System.IO;

namespace Microsoft.Xna.Samples.Storage
{
	[Serializable]
    public struct SaveGame
    {
        public string Name;
        public int HiScore;
        public DateTime Date;
		
		[NonSerialized]
        public int DontKeep;
	}
	
	public class SaveGameStorage
    {
		public void Save(SaveGame sg)
		{
			StorageDevice device = StorageDevice.ShowStorageDeviceGuide();
			
			 // Open a storage container
            StorageContainer container = device.OpenContainer("TestStorage");
			
			// Get the path of the save game
            string filename = Path.Combine(container.Path, "savegame.xml");

            // Open the file, creating it if necessary
            FileStream stream = File.Open(filename, FileMode.OpenOrCreate);
			
			// Convert the object to XML data and put it in the stream
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGame));
            serializer.Serialize(stream, sg);

            // Close the file
            stream.Close();
			
			 // Dispose the container, to commit changes
            container.Dispose();
		}
		
		public SaveGame Load()
        {
            SaveGame ret = new SaveGame();
			
			StorageDevice device = StorageDevice.ShowStorageDeviceGuide();
			
           // Open a storage container
            StorageContainer container = device.OpenContainer("TestStorage");

            // Get the path of the save game
            string filename = Path.Combine(container.Path, "savegame.xml");

            // Check to see if the save exists
            if (!File.Exists(filename))
                // Notify the user there is no save           
                return ret;

            // Open the file
            FileStream stream = File.Open(filename, FileMode.OpenOrCreate,
                FileAccess.Read);
			
			// Read the data from the file
            XmlSerializer serializer = new XmlSerializer(typeof(SaveGame));
            ret = (SaveGame)serializer.Deserialize(stream);

            // Close the file
            stream.Close();

            // Dispose the container
            container.Dispose();

            return ret;
        }
    }
}
