This page contains a list of frequently asked questions.

### What software do I need to start?

Depending on the platform you wish to develop for the following thing are needed:
 - Android - You need Xamarin.Android: [http://android.xamarin.com/], it works on Windows and Mac and it can be used in combination with either Visual Studio or Xamarin Studio.
 - iOS - You need Xamarin.iOS: [http://ios.xamarin.com/], it works on Windows and Mac and it can be used in combination with either Visual Studio or Xamarin Studio.


### Where do I start? 

The quickest way to start is to download CartBlanche's samples: [https://github.com/CartBlanche/MonoGame-Samples https://github.com/CartBlanche/MonoGame-Samples]

### Where is the IRC channel? 

The official MonoGame IRC channel is #MonoGame on irc.gnome.org.

### How can I take advantage of iOS 4 Multitasking ? 

Your Program class has to extend MonoGameProgram instead of UIApplicationDelegate. Assign MonoGameGame variable to your Game class.

```C#
[Register ("AppDelegate")]
	class  Program : MonoGameProgram
	{		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Fun begins..
			MonoGameGame = new MyAwesomeGame(); 
                        MonoGameGame.Run();
			
			return true;
		}
		
		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
```
### How do I implement iAD into my game? 

Here is a sample class that will work with iOS 4+:

```C#
internal class AdController : UIViewController
	{
	}
    class AdOverlayIAD
    {
		private ADBannerView  m_adBannerView;
		private GameWindow Window;
		private AdController new_controller;
		private bool should_unpause;
		private bool open;
		private ADBannerView adBannerView ;
		private int ad_width;
		private int ad_height;
		
		public AdOverlayIAD (GameWindow Window)
		{
			this.Window=Window;
		}
		
		private void m_adBannerView_AdLoaded(object sender, EventArgs e) {
	     (m_adBannerView as ADBannerView).Hidden = false;
				adBannerView.Hidden=false;
			Debug.WriteLine("SHOW AD");
		 }
		 private void m_adBannerView_FailedToReceiveAd(object sender, AdErrorEventArgs e) {
		     (m_adBannerView as ADBannerView).Hidden = true;
			Debug.WriteLine("HIDE AD"+e.Error.ToString());
	 	}
		 private void m_adBannerView_ActionFinished(object sender, EventArgs e) {
			Debug.WriteLine("DONE AD");
			if (should_unpause) {
				Game.pause=false;
				
			}
			open=false;
			MediaPlayer.Resume();
		        new_controller.View.Frame = new System.Drawing.RectangleF(0, UIScreen.MainScreen.Bounds.Height-ad_height, ad_width, ad_height);
			adBannerView.Hidden=false;
			
	 	}
        public void init()
        {
			bool supported=UIDevice.CurrentDevice.RespondsToSelector(new Selector("isMultitaskingSupported"));
			
			if (supported && UIDevice.CurrentDevice.IsMultitaskingSupported)  {
				m_adBannerView = new ADBannerView(); 
				adBannerView = m_adBannerView as ADBannerView; 
				NSMutableSet nsM = new NSMutableSet();
		        nsM.Add(ADBannerView.SizeIdentifierPortrait);
		        adBannerView.RequiredContentSizeIdentifiers = nsM;
				//adBannerView.ActionFinished
		        adBannerView.AdLoaded += new EventHandler(m_adBannerView_AdLoaded);
		        adBannerView.FailedToReceiveAd += new EventHandler<AdErrorEventArgs>(m_adBannerView_FailedToReceiveAd);
				
                ad_width=320;
                ad_height=50;
                if (Game.width==768) {
                    ad_width=768;
                    ad_height=66;
                }
				adBannerView.ActionShouldBegin=delegate(ADBannerView banner, bool willLeaveApplication) 
				{
					if (!open) {
						open=true;
						MediaPlayer.Pause();
						should_unpause=false;
						if (!Game.pause) {
							should_unpause=true;
							Game.pause=true;
						}
						Debug.WriteLine(willLeaveApplication?"Y":"N");
		        		adBannerView.Hidden=true; 
					}
					return true;
				};
				adBannerView.ActionFinished+=new EventHandler(m_adBannerView_ActionFinished);
		        adBannerView.Frame = new System.Drawing.RectangleF(0, 0, ad_width, ad_height);
				adBannerView.Hidden=true; 
				//UIScreen.MainScreen.Bounds
				new_controller=new AdController();
		        new_controller.View.Frame = new System.Drawing.RectangleF(0,  UIScreen.MainScreen.Bounds.Height-ad_height, ad_width, ad_height);
				new_controller.Add(adBannerView);
				Window.AddSubview(new_controller.View); 
			}	
		}
			
    }
```

### Can I use a physics engine in my game? 

Yes, Box2D and Farseer physics will compile correctly and will run fine at 60 FPS.