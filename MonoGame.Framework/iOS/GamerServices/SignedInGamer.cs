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

using MonoTouch.Foundation;
using MonoTouch.GameKit;
using MonoTouch.UIKit;

#endregion Statement


namespace Microsoft.Xna.Framework.GamerServices
{
    public class SignedInGamer : Gamer
    {
		private GKLocalPlayer lp;
		
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
			try 				
			{
				var osVersion = UIDevice.CurrentDevice.SystemVersion;
				if(osVersion.Contains("."))
				if(osVersion.IndexOf(".") != osVersion.LastIndexOf("."))
				{
					var parts = osVersion.Split(char.Parse("."));
					osVersion = parts[0] + "." + parts[1];
				}
				
				if (double.Parse(osVersion, System.Globalization.CultureInfo.InvariantCulture) > 4.1)
				{
					
					lp = GKLocalPlayer.LocalPlayer;
			        if (lp != null)
					{
						Guide.IsVisible = true;
						lp.Authenticate( delegate(NSError error) 
						                	{  							              
												try 
												{
													if ( error != null )
													{
#if DEBUG									
														Console.WriteLine(error);
#endif
													}
													else
													{
														
													}
												} 
												finally 
												{
													Guide.IsVisible = false;
												}
											}
						                );
					}
				}
			}
			catch (Exception ex) 
			{
#if DEBUG				
				Console.WriteLine(ex.Message);
#endif
			}
		}
		
		public SignedInGamer()
		{
			
			// Register to receive the GKPlayerAuthenticationDidChangeNotificationName so we are notified when 
			// Authentication changes
			NSNotificationCenter.DefaultCenter.AddObserver( new NSString("GKPlayerAuthenticationDidChangeNotificationName"), (notification) => {   
        													    if (lp !=null && lp.Authenticated)
																{
																	this.Gamertag = lp.Alias;
																	this.DisplayName = lp.PlayerID;	
														        	// Insert code here to handle a successful authentication.
																	Gamer.SignedInGamers.Add(this);
																	// Fire the SignedIn event
																	OnSignedIn(new SignedInEventArgs(this) );
																}
														    	else
																{
														        	// Insert code here to clean up any outstanding Game Center-related classes.
																	Gamer.SignedInGamers.Remove(this);
																	// Fire the SignedOut event
																	OnSignedOut(new SignedOutEventArgs(this) );
																}
	    													});	
			
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
				
				lp.LoadFriends( delegate (string[] FriendsList, NSError error )
				               	{
									foreach(string Friend in FriendsList)
									{
										friendCollection.Add( new FriendGamer(){Gamertag = Friend} );
									}
				});
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
				
				GKAchievementDescription.LoadAchievementDescriptions( delegate(GKAchievementDescription[] achievements, NSError error)
				                                                    {
																		if (achievements != null)
																		{
																			foreach(GKAchievementDescription a in achievements)
																			{
																				gamerAchievements.Add(new Achievement(){Name = a.Title, Key= a.Identifier, Description = a.AchievedDescription, HowToEarn = a.UnachievedDescription, DisplayBeforeEarned = !a.Hidden});
																			}
																		}
																	});
				
				GKAchievement.LoadAchievements( delegate(GKAchievement[] achievements, NSError error)
				                               	{
													if (achievements != null)
													{
														foreach(GKAchievement a in achievements)
														{
															foreach(Achievement ac in gamerAchievements)
															{
																if ( ac.Key == a.Identifier )
																{
																	ac.IsEarned = a.Completed;
																	ac.EarnedDateTime = a.LastReportedDate;
																}
															}															
														}
													}
												} );
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
			GKAchievement a = new GKAchievement(achievementId);
				a.PercentComplete = percentageComplete;
				a.ReportAchievement( delegate(NSError error){
					if (error != null)
					{
						// Retain the achievement object and try again later (not shown).
					}
		
				} );
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
				GKScore score = new GKScore(aCategory);
				score.Value = aScore;
				score.ReportScore(delegate (NSError error)
					{
						if (error != null)
						{
							// Oh oh something went wrong.
						}
				});
			}
		}
		
		public void ResetAchievements()
		{
			if (IsSignedInToLive)
			{
				GKAchievement.ResetAchivements(delegate (NSError error)
					{
						if (error != null)
						{
							// Oh oh something went wrong.
						}
				});
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
				var SignedIn = ( ( lp != null ) && ( lp.Authenticated ) );
				return SignedIn;
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
			 if (SignedIn != null) 
			 {
			    // Invokes the delegates. 
			    SignedIn(this, e);
			 }
		}
		
		protected virtual void OnSignedOut(SignedOutEventArgs e)
		{
			 if (SignedOut != null) 
			 {
			    // Invokes the delegates. 
			    SignedOut(this, e);
			 }
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
