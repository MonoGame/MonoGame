// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Foundation;
using GameKit;
using UIKit;

namespace Microsoft.Xna.Framework.GamerServices
{
    // iOS Unified API needs explicit conversion of DateTime/NSDate
    // see http://developer.xamarin.com/guides/cross-platform/macios/unified/
    public static class DateTimeNSDateConversions
    {
        public static DateTime NSDateToDateTime(this NSDate date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime( 
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        public static NSDate DateTimeToNSDate(this DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0) );
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }
    }

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

        public void EndAuthentication(IAsyncResult result)
        {
            AuthenticationDelegate ad = (AuthenticationDelegate)result.AsyncState; 
			
            ad.EndInvoke(result);
        }

        private void DoAuthentication()
        {
            try
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(4, 1))
                {
                    UIApplication.SharedApplication.BeginInvokeOnMainThread(delegate
                    {
                        lp = GKLocalPlayer.LocalPlayer;
                    
                        if (lp != null)
                        {
                            if (!UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                            {
                                #pragma warning disable 618
                                // Game Center authentication for iOS 5 and older
                                lp.Authenticate(delegate(NSError error)
                                {
                                    #if DEBUG
                                    if (error != null)
                                        Console.WriteLine(error);
                                    #endif
                                });
                                #pragma warning restore 618
                            }
                            else
                            {
                                // Game Center authentication for iOS 6+
                                lp.AuthenticateHandler = delegate(UIViewController controller, NSError error)
                                {
                                    #if DEBUG
                                    if (error != null)
                                        Console.WriteLine(error);
                                    #endif

                                    if (controller != null)
                                        ((UIViewController)Game.Instance.Services.GetService(typeof(UIViewController))).PresentViewController(controller, true, null);
                                };
                            }
                        }
                    });
                }
            }
            #if DEBUG
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            #else
            catch
            {
            }
            #endif
        }

        public SignedInGamer()
        {
            // Register to receive the GKPlayerAuthenticationDidChangeNotificationName so we are notified when authentication changes
            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("GKPlayerAuthenticationDidChangeNotificationName"), (notification) =>
            {   
                if (lp != null && lp.Authenticated)
                {
                    this.Gamertag = lp.Alias;
                    this.DisplayName = lp.PlayerID;	
                    // Insert code here to handle a successful authentication.
                    Gamer.SignedInGamers.Add(this);
                    // Fire the SignedIn event
                    OnSignedIn(new SignedInEventArgs(this));
                }
                else
                {
                    // Insert code here to clean up any outstanding Game Center-related classes.
                    Gamer.SignedInGamers.Remove(this);
                    // Fire the SignedOut event
                    OnSignedOut(new SignedOutEventArgs(this));
                }
            });
			
            var result = BeginAuthentication(null, null);	
            EndAuthentication(result);
        }

        private void AuthenticationCompletedCallback(IAsyncResult result)
        {
            EndAuthentication(result);	
        }

        #region Methods
        public FriendCollection GetFriends()
        {
            if (IsSignedInToLive)
            {
                if (friendCollection == null)
                {
                    friendCollection = new FriendCollection();
                }
				
                lp.LoadFriends(delegate (string[] FriendsList, NSError error)
                {
                    foreach (string Friend in FriendsList)
                    {
                        friendCollection.Add(new FriendGamer(){ Gamertag = Friend });
                    }
                });
            }
			
            return friendCollection;
        }

        public bool IsFriend(Gamer gamer)
        {
            if (gamer == null)
                throw new ArgumentNullException();
			
            if (gamer.IsDisposed)
                throw new ObjectDisposedException(gamer.ToString());	
			
            bool found = false;
            foreach (FriendGamer f in friendCollection)
            {
                if (f.Gamertag == gamer.Gamertag)
                {
                    found = true;
                }
            }
            return found;
						
        }

        delegate AchievementCollection GetAchievementsDelegate();

        public IAsyncResult BeginGetAchievements(AsyncCallback callback, Object asyncState)
        {
            // Go off and grab achievements
            GetAchievementsDelegate gad = GetAchievements; 
			
            return gad.BeginInvoke(callback, gad);
        }

        private void GetAchievementCompletedCallback(IAsyncResult result)
        {
            // get the delegate that was used to call that method
            GetAchievementsDelegate gad = (GetAchievementsDelegate)result.AsyncState; 

            // get the return value from that method call
            gamerAchievements = gad.EndInvoke(result);
        }

        public AchievementCollection EndGetAchievements(IAsyncResult result)
        {
            GetAchievementsDelegate gad = (GetAchievementsDelegate)result.AsyncState; 
			
            gamerAchievements = gad.EndInvoke(result);
			
            return gamerAchievements;
        }

        public AchievementCollection GetAchievements()
        {
            if (IsSignedInToLive)
            {
                if (gamerAchievements == null)
                {
                    gamerAchievements = new AchievementCollection();
                }
				
                GKAchievementDescription.LoadAchievementDescriptions(delegate(GKAchievementDescription[] achievements, NSError error)
                {
                    if (achievements != null)
                    {
                        foreach (GKAchievementDescription a in achievements)
                        {
                            gamerAchievements.Add(new Achievement() {
                                Name = a.Title,
                                Key = a.Identifier,
                                Description = a.AchievedDescription,
                                HowToEarn = a.UnachievedDescription,
                                DisplayBeforeEarned = !a.Hidden
                            });
                        }
                    }
                });
				
                GKAchievement.LoadAchievements(delegate(GKAchievement[] achievements, NSError error)
                {
                    if (achievements != null)
                    {
                        foreach (GKAchievement a in achievements)
                        {
                            foreach (Achievement ac in gamerAchievements)
                            {
                                if (ac.Key == a.Identifier)
                                {
                                    ac.IsEarned = a.Completed;
                                    ac.EarnedDateTime = a.LastReportedDate.NSDateToDateTime();
                                }
                            }															
                        }
                    }
                });
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

        private void AwardAchievementCompletedCallback(IAsyncResult result)
        {
            EndAwardAchievement(result);	
        }

        public void AwardAchievement(string achievementId)
        {			
            AwardAchievement(achievementId, 100.0f);
        }

        public void DoAwardAchievement(string achievementId, double percentageComplete)
        {
            if (IsSignedInToLive)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(delegate
                {
                    GKAchievement achievement = new GKAchievement(achievementId);
                    achievement.PercentComplete = percentageComplete;

                    if (!UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                    {
                        #pragma warning disable 618
                        // Report achievement for iOS 5 and older
                        achievement.ReportAchievement(delegate(NSError error)
                        {
                            if (error != null)
                            {
                                // Oh oh something went wrong.
                            }
                        });
                        #pragma warning restore 618
                    }
                    else
                    {
                        // Report achievement for iOS 6+
                        GKAchievement.ReportAchievements(new GKAchievement[] { achievement }, delegate (NSError error)
                        {
                            if (error != null)
                            {
                                // Oh oh something went wrong.
                            }
                        });
                    }
                });
            }
        }

        public void AwardAchievement(string achievementId, double percentageComplete)
        {
            if (IsSignedInToLive)
            {
                BeginAwardAchievement(achievementId, percentageComplete, AwardAchievementCompletedCallback, null);
            }
        }

        public void UpdateScore(string aCategory, long aScore)
        {
            if (IsSignedInToLive)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(delegate
                {
                    GKScore score = new GKScore(aCategory);
                    score.Value = aScore;

                    if (!UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                    {
                        #pragma warning disable 618
                        // Report score for iOS 5 and older
                        score.ReportScore(delegate (NSError error)
                        {
                            if (error != null)
                            {
                                // Oh oh something went wrong.
                            }
                        });
                        #pragma warning restore 618
                    }
                    else
                    {
                        // Report score for iOS 6+
                        GKScore.ReportScores(new GKScore[] { score }, delegate (NSError error)
                        {
                            if (error != null)
                            {
                                // Oh oh something went wrong.
                            }
                        });
                    }
                });
            }
        }

        public void ResetAchievements()
        {
            if (IsSignedInToLive)
            {
                UIApplication.SharedApplication.InvokeOnMainThread(delegate
                {
                    GKAchievement.ResetAchivements(delegate(NSError error)
                    {
                        if (error != null)
                        {
                            // Oh oh something went wrong.
                        }
                    });
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
                var SignedIn = ((lp != null) && (lp.Authenticated));
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

        //        LeaderboardWriter _leaderboardWriter = new LeaderboardWriter();
        //        public LeaderboardWriter LeaderboardWriter
        //        {
        //            get
        //            {
        //                return _leaderboardWriter;
        //            }
        //        }
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
        public SignedInEventArgs(SignedInGamer gamer)
        {
			
        }
    }

    public class SignedOutEventArgs : EventArgs
    {
        public SignedOutEventArgs(SignedInGamer gamer)
        {
			
        }
    }
}
