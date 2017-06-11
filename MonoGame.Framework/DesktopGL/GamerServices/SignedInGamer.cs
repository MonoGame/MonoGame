#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Statement
﻿using System;

#endregion Statement


namespace Microsoft.Xna.Framework.GamerServices
{
    public class SignedInGamer : Gamer
    {		
		private AchievementCollection gamerAchievements;
		private FriendCollection friendCollection;
		
		delegate void AuthenticationDelegate();
		
		public IAsyncResult BeginAuthentication(AsyncCallback callback, Object asyncState)
		{
			// Go off authenticate
			AuthenticationDelegate ad = DoAuthentication; 
			
			return ad.BeginInvoke(callback, ad);
		}
		
		public void EndAuthentication( IAsyncResult result )
		{
			AuthenticationDelegate ad = (AuthenticationDelegate)result.AsyncState; 
			
			ad.EndInvoke(result);
		}
		
		private void DoAuthentication()
		{
				
		}
		
		public SignedInGamer()
		{
			
			var result = BeginAuthentication(null, null);	
			EndAuthentication( result );
		}
		
		private void AuthenticationCompletedCallback( IAsyncResult result )
		{
			EndAuthentication(result);	
		}
		
		#region Methods
		public FriendCollection GetFriends()
		{
			if(IsSignedInToLive)
			{
				if ( friendCollection == null )
				{
					friendCollection = new FriendCollection();
				}
								
			}
			
			return friendCollection;
		}
		
		public bool IsFriend (Gamer gamer)
		{
			if ( gamer == null ) 
				throw new ArgumentNullException();
			
			if ( gamer.IsDisposed )
				throw new ObjectDisposedException(gamer.ToString());	
			
			bool found = false;
			foreach(FriendGamer f in friendCollection)
			{
				if ( f.Gamertag == gamer.Gamertag )
				{
					found = true;
				}
			}
			return found;
						
		}
		
		delegate AchievementCollection GetAchievementsDelegate();
		
		public IAsyncResult BeginGetAchievements( AsyncCallback callback, Object asyncState)
		{
			// Go off and grab achievements
			GetAchievementsDelegate gad = GetAchievements; 
			
			return gad.BeginInvoke(callback, gad);
		}
		
		private void GetAchievementCompletedCallback( IAsyncResult result )
		{
			// get the delegate that was used to call that method
			GetAchievementsDelegate gad = (GetAchievementsDelegate)result.AsyncState; 

			// get the return value from that method call
			gamerAchievements = gad.EndInvoke(result);
		}
		
		public AchievementCollection EndGetAchievements( IAsyncResult result )
		{
			GetAchievementsDelegate gad = (GetAchievementsDelegate)result.AsyncState; 
			
			gamerAchievements = gad.EndInvoke(result);
			
			return gamerAchievements;
		}
		
		public AchievementCollection GetAchievements()
		{
			if ( IsSignedInToLive )
			{
				if (gamerAchievements == null)
				{
					gamerAchievements = new AchievementCollection();
				}
								
			}
			return gamerAchievements;
		}
		
		delegate void AwardAchievementDelegate(string achievementId, double percentageComplete);

        public IAsyncResult BeginAwardAchievement(string achievementId, AsyncCallback callback, Object state)
        {
            return BeginAwardAchievement(achievementId, 100.0, callback, state);
        }

        public IAsyncResult BeginAwardAchievement(
         string achievementId,
		 double percentageComplete,
         AsyncCallback callback,
         Object state
		)
		{	
			// Go off and award the achievement
			AwardAchievementDelegate aad = DoAwardAchievement; 
				
			return aad.BeginInvoke(achievementId, percentageComplete, callback, aad);
		}
		
		public void EndAwardAchievement(IAsyncResult result)
		{
			AwardAchievementDelegate aad = (AwardAchievementDelegate)result.AsyncState; 
			
			aad.EndInvoke(result);
		}
		
		private void AwardAchievementCompletedCallback( IAsyncResult result )
		{
			EndAwardAchievement(result);	
		}
		
		public void AwardAchievement( string achievementId )
		{			
			AwardAchievement(achievementId, 100.0f);
		}
		
		public void DoAwardAchievement( string achievementId, double percentageComplete )
		{			
		}
		
		public void AwardAchievement( string achievementId, double percentageComplete )
		{
			if (IsSignedInToLive)
			{
				BeginAwardAchievement( achievementId, percentageComplete, AwardAchievementCompletedCallback, null );
			}
		}
		
		public void UpdateScore( string aCategory, long aScore )
		{
			if (IsSignedInToLive)
			{				
			}
		}
		
		public void ResetAchievements()
		{
			if (IsSignedInToLive)
			{				
			}
		}

		#endregion
			
		#region Properties
		public GameDefaults GameDefaults 
		{ 
			get
			{
				throw new NotSupportedException();
			}
		}
		
		public bool IsGuest 
		{ 
			get
			{
				throw new NotSupportedException();
			}
		}
		
		public bool IsSignedInToLive 
		{ 
			get
			{
				return false;
			}
		}
		
		public int PartySize 
		{ 
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		
        public PlayerIndex PlayerIndex
        {
            get
            {
                return PlayerIndex.One;
            }
        }
		
		public GamerPresence Presence 
		{ 
			get
			{
				throw new NotSupportedException();
			}
		}

        GamerPrivileges _privileges = new GamerPrivileges();
        public GamerPrivileges Privileges
        {
            get
            {
                return _privileges;
            }
        }
		#endregion
		
		
		protected virtual void OnSignedIn(SignedInEventArgs e)
		{
            EventHelpers.Raise(this, SignedIn, e);
        }
		
		protected virtual void OnSignedOut(SignedOutEventArgs e)
		{
            EventHelpers.Raise(this, SignedOut, e);
		}

		
		#region Events
		public static event EventHandler<SignedInEventArgs> SignedIn;
		
		public static event EventHandler<SignedOutEventArgs> SignedOut;
		#endregion
    }
	
	public class SignedInEventArgs : EventArgs
	{
		public SignedInEventArgs ( SignedInGamer gamer )
		{
			
		}
	}
	
	public class SignedOutEventArgs : EventArgs
	{
		public SignedOutEventArgs (SignedInGamer gamer )
		{
			
		}
	}
}

