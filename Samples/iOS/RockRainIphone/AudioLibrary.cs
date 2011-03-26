using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace RockRainIphone
{
    public class AudioLibrary
    {
        private SoundEffect explosion;
        private SoundEffect newMeteor;
        private SoundEffect menuBack;
        private SoundEffect menuSelect;
        private SoundEffect powerGet;
        private SoundEffect powerShow;
        private Song backMusic;
        private Song startMusic;

		ContentManager Content;
		
        public SoundEffect Explosion
        {
            get 
			{ 	
				if (explosion == null)
					explosion = Content.Load<SoundEffect>("explosion");
				return explosion; 
			}
        }

        public SoundEffect NewMeteor
        {
            get 
			{
				if (newMeteor == null)
					newMeteor = Content.Load<SoundEffect>("newmeteor");        
				return newMeteor; 
			}
        }

        public SoundEffect MenuBack
        {
            get 
			{
				if (menuBack == null)
					menuBack = Content.Load<SoundEffect>("menu_back");
				return menuBack; 
			}
        }

        public SoundEffect MenuSelect
        {
            get 
			{
				if (menuSelect == null)
					menuSelect = Content.Load<SoundEffect>("menu_select3");
				return menuSelect; 
			}
        }

        public SoundEffect PowerGet
        {
            get 
			{ 
				if (powerGet == null)
					powerGet = Content.Load<SoundEffect>("powerget");
				return powerGet; 
			}
        }

        public SoundEffect PowerShow
        {
            get 
			{ 
				if (powerShow == null) 
					powerShow = Content.Load<SoundEffect>("powershow");
				return powerShow; 
			}
        }

        public Song BackMusic
        {
            get 
			{
				if (backMusic == null)
					backMusic = Content.Load<Song>("backmusic.mp3");
				return backMusic; 
			}
        }

        public Song StartMusic
        {
            get 
			{ 
				if (startMusic == null)
				{
					 startMusic = Content.Load<Song>("startmusic.mp3");
				}
				return startMusic; 
			}
        }

        public AudioLibrary(ContentManager Content)
        {
			this.Content = Content;         
        }

    }
}