using Microsoft.Xna.Framework;

using MonoTouch.MessageUI;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework.GamerServices
{
    public class EmailComposeTask
    {
        #region Fields
        private static Game game;
        private MFMailComposeViewController mail;
        #endregion

        #region Properties
        public string To
        {
            set
            {
                if (this.mail != null && !string.IsNullOrEmpty(value))
                    mail.SetToRecipients(new string[]{value});
            }
        }
        public string Cc
        {
            set
            {
                if (this.mail != null && !string.IsNullOrEmpty(value))
                    mail.SetCcRecipients(new string[]{value});
            }
        }
        public string Bcc
        {
            set
            {
                if (this.mail != null && !string.IsNullOrEmpty(value))
                    mail.SetBccRecipients(new string[]{value});
            }
        }
        public string Subject
        {
            set
            {
                if (this.mail != null && this.mail != null && !string.IsNullOrEmpty(value))
                    mail.SetSubject(value);
            }
        }
        public string Body
        {
            set
            {
                if (this.mail != null && !string.IsNullOrEmpty(value))
                    mail.SetMessageBody(value, false);
            }
        }
        #endregion

        #region Constructors
        public EmailComposeTask()
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                this.mail = new MFMailComposeViewController ();
                this.mail.Finished += Mail_Dismiss;
            }
        }
        #endregion

        #region Methods
        public static void Initialise(Game game)
        {
            EmailComposeTask.game = game;
        }

        public void Show()
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                (EmailComposeTask.game.Platform as iOSGamePlatform).ViewController.PresentModalViewController(this.mail, true);
            }
        }

        private void Mail_Dismiss(object sender, MFComposeResultEventArgs args)
        {
            args.Controller.DismissModalViewControllerAnimated(true);
        }
        #endregion
    }
}