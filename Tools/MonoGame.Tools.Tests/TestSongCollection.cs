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
        public void testSongConstructor()
        {
            SongCollection collection = new SongCollection();
            void loadContent(ContentManager content)
            {
                Song songs = content.Load<Song>("bark_mono.wav");
                collection.Add(songs);
                Assert.AreEqual(1, collection.Count);
                collection.Clone();
                Assert.AreEqual(2, collection.Count);
            }
        }
    }
}
