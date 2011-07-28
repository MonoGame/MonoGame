using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGame.Framework.MonoLive;

namespace MonoGame.Framework.GamerServices
{
    internal class MonoLiveClient
    {

        private static MonoLiveClient instance = null;

        public static MonoLiveClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MonoLiveClient();
                }
                return instance;
            }
        }
        
        internal MonoLiveClient()
        {
        }

        public delegate void SignInCompletedEventHandler(object sender, SignInCompletedEventArgs e);

        public event SignInCompletedEventHandler SignInCompleted;

        public void SignInAsync(string username, string password)
        {
            MonoLive.MonoLive client = new MonoLive.MonoLive();
            client.SignInCompleted += new MonoLive.SignInCompletedEventHandler(client_SignInCompleted);
            client.SignInAsync(username, password, client);                            
        }

        private void client_SignInCompleted(object sender, MonoLive.SignInCompletedEventArgs e)
        {
            if (SignInCompleted != null && e.Error != null)
            {
                ((IDisposable)e.UserState).Dispose();
                SignInCompleted(this, new SignInCompletedEventArgs(new Microsoft.Xna.Framework.GamerServices.SignedInGamer()
                {
                    Gamertag = e.Result.Gamer.GamerTag,
                    DisplayName = e.Result.Gamer.GamerTag
                }));
                return;
            }
            SignInCompleted(this, null);
            
        }

        public Microsoft.Xna.Framework.GamerServices.Gamer SignIn(string username, string password)
        {
            using (MonoLive.MonoLive client = new MonoLive.MonoLive())
            {
                MonoLive.Result result = client.SignIn(username, password);
                if (result.ok)
                {
                    return new Microsoft.Xna.Framework.GamerServices.SignedInGamer()
                    {
                        Gamertag = result.Gamer.GamerTag,
                        DisplayName = result.Gamer.GamerTag
                    };                    
                }
            }
            return null;
        }

    }

    public partial class SignInCompletedEventArgs : EventArgs
    {
        private Microsoft.Xna.Framework.GamerServices.Gamer gamer;

        internal SignInCompletedEventArgs(Microsoft.Xna.Framework.GamerServices.Gamer gamer) 
        {
            this.gamer = gamer;
        }

        /// <remarks/>
        public Microsoft.Xna.Framework.GamerServices.Gamer Gamer
        {
            get
            {
                return this.gamer;
            }
        }
    }
}
