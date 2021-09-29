using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.Tools.Tests
{
    class TestSongCollection
    {

        [Test]
        public void TestSongConstructor()
        {
            SongCollection collection = new SongCollection();
            System.Uri firstUri = new System.Uri("C:/Users/sujee/openSourceProject/MonoGame/Tests/Assets/Audio/rock_loop_stereo.ogg", System.UriKind.Relative);
            Song firstSong = Song.FromUri("mySecondSong", firstUri);
            collection.Add(firstSong);
            Assert.AreEqual(firstSong, collection[0]);
            Assert.AreEqual(1, collection.Count);
        }
    }
}
