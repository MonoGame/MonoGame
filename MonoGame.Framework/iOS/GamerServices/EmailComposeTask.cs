using Microsoft.Xna.Framework;

using MonoTouch.MessageUI;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework.GamerServices
{
    public static class EmailComposeTask
    {
        private static Game game;

        public static void Initialise(Game game)
        {
            EmailComposeTask.game = game;
        }

        public static void Show(string to, string cc, string bcc, string subject, string body)
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                MFMailComposeViewController mail = new MFMailComposeViewController();
                if (!string.IsNullOrEmpty(to))
                    mail.SetToRecipients(new string[]{to});
                if (!string.IsNullOrEmpty(cc))
                    mail.SetCcRecipients(new string[]{cc});
                if (!string.IsNullOrEmpty(bcc))
                    mail.SetBccRecipients(new string[]{bcc});
                if (!string.IsNullOrEmpty(subject))
                    mail.SetSubject(subject);
                if (!string.IsNullOrEmpty(body))
                    mail.SetMessageBody(body, false);
                mail.Finished += (object sender, MFComposeResultEventArgs args) => { args.Controller.DismissModalViewControllerAnimated (true); };
                (EmailComposeTask.game.Platform as iOSGamePlatform).ViewController.PresentModalViewController(mail, true);
            }
        }
    }
}