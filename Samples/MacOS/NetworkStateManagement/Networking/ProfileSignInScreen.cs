#region File Description
//-----------------------------------------------------------------------------
// ProfileSignInScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace NetworkStateManagement
{
    /// <summary>
    /// In order to play a networked game, you must have a player profile signed in.
    /// If you want to play on Live, that has to be a Live profile. Rather than just
    /// failing with an error message, it is nice if we can automatically bring up the
    /// Guide screen when we detect that no suitable profiles are currently signed in,
    /// so the user can easily correct the problem. This screen checks the sign in
    /// state, and brings up the Guide user interface if there is a problem with it.
    /// It then raises an event as soon as a valid profile has been signed in.
    /// 
    /// There are two scenarios for how this can work. If no good profile is signed in:
    /// 
    ///     - MainMenuScreen activates the ProfileSignInScreen
    ///     - ProfileSignInScreen activates the Guide user interface
    ///     - User signs in a profile
    ///     - ProfileSignInScreen raises the ProfileSignedIn event
    ///     - This advances to the CreateOrFindSessionScreen
    /// 
    /// Alternatively, there might already be a valid profile signed in. In this case:
    /// 
    ///     - MainMenuScreen activates the ProfileSignInScreen
    ///     - ProfileSignInScreen notices everything is already good
    ///     - ProfileSignInScreen raises the ProfileSignedIn event
    ///     - This advances to the CreateOrFindSessionScreen
    /// 
    /// In this second case, the ProfileSignInScreen is only active for a single
    /// Update, so the user just sees a transition directly from the MainMenuScreen
    /// to the CreateOrFindSessionScreen.
    /// </summary>
    class ProfileSignInScreen : GameScreen
    {
        #region Fields

        NetworkSessionType sessionType;
        bool haveShownGuide;
        bool haveShownMarketplace;

        #endregion

        #region Events

        public event EventHandler<EventArgs> ProfileSignedIn;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new profile sign in screen.
        /// </summary>
        public ProfileSignInScreen(NetworkSessionType sessionType)
        {
            this.sessionType = sessionType;

            IsPopup = true;
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the profile sign in screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (ValidProfileSignedIn())
            {
                // As soon as we detect a suitable profile is signed in,
                // we raise the profile signed in event, then go away.
                if (ProfileSignedIn != null)
                    ProfileSignedIn(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (IsActive && !Guide.IsVisible)
            {
                // If we are in trial mode, and they want to play online, and a profile
                // is signed in, take them to marketplace so they can purchase the game.
                if ((Guide.IsTrialMode) &&
                    (NetworkSessionComponent.IsOnlineSessionType(sessionType)) &&
                    (Gamer.SignedInGamers[ControllingPlayer.Value] != null) &&
                    (!haveShownMarketplace))
                {
                    ShowMarketplace();

                    haveShownMarketplace = true;
                }
                else if (!haveShownGuide && !haveShownMarketplace)
                {
                    // No suitable profile is signed in, and we haven't already shown
                    // the Guide. Let's show it now, so they can sign in a profile.
                    Guide.ShowSignIn(1,
                            NetworkSessionComponent.IsOnlineSessionType(sessionType));

                    haveShownGuide = true;
                }
                else
                {
                    // Hmm. No suitable profile is signed in, but we already showed
                    // the Guide, and the Guide isn't still visible. There is only
                    // one thing that can explain this: they must have cancelled the
                    // Guide without signing in a profile. We'd better just exit,
                    // which will leave us on the same menu as before.
                    ExitScreen();
                }
            }
        }


        /// <summary>
        /// Helper checks whether a valid player profile is signed in.
        /// </summary>
        bool ValidProfileSignedIn()
        {
            // If there is no profile signed in, that is never good.
            SignedInGamer gamer = Gamer.SignedInGamers[ControllingPlayer.Value];

            if (gamer == null)
                return false;

            // If we want to play in a Live session, also make sure the profile is
            // signed in to Live, and that it has the privilege for online gameplay.
            if (NetworkSessionComponent.IsOnlineSessionType(sessionType))
            {
                if (!gamer.IsSignedInToLive)
                    return false;

                if (!gamer.Privileges.AllowOnlineSessions)
                    return false;
            }

            // Okeydokey, this looks good.
            return true;
        }


        /// <summary>
        /// LIVE networking is not supported in trial mode. Rather than just giving
        /// the user an error, this function asks if they want to purchase the full
        /// game, then takes them to Marketplace where they can do that. Once the
        /// Guide is active, the user can either make the purchase, or cancel it.
        /// When the Guide closes, ProfileSignInScreen.Update will notice that
        /// Guide.IsVisible has gone back to false, at which point it will check if
        /// the game is still in trial mode, and either exit the screen or proceed
        /// forward accordingly.
        /// </summary>
        void ShowMarketplace()
        {
            MessageBoxScreen confirmMarketplaceMessageBox =
                                    new MessageBoxScreen(Resources.ConfirmMarketplace);

            confirmMarketplaceMessageBox.Accepted += delegate
            {
                Guide.ShowMarketplace(ControllingPlayer.Value);
            };

            ScreenManager.AddScreen(confirmMarketplaceMessageBox, ControllingPlayer);
        }


        #endregion
    }
}
