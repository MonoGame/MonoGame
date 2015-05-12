using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace IgeMacIntegration {

#if MONOMAC
	public class IgeMacMenu {

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_connect_window_key_handler (IntPtr window);

		public static void ConnectWindowKeyHandler (Gtk.Window window)
		{
			ige_mac_menu_connect_window_key_handler (window.Handle);
		}

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_set_global_key_handler_enabled (bool enabled);

		public static bool GlobalKeyHandlerEnabled {
			set { 
				ige_mac_menu_set_global_key_handler_enabled (value);
			}
		}

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_set_menu_bar(IntPtr menu_shell);

		public static Gtk.MenuShell MenuBar { 
			set {
				ige_mac_menu_set_menu_bar(value == null ? IntPtr.Zero : value.Handle);
			}
		}

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_set_quit_menu_item(IntPtr quit_item);

		public static Gtk.MenuItem QuitMenuItem { 
			set {
				ige_mac_menu_set_quit_menu_item(value == null ? IntPtr.Zero : value.Handle);
			}
		}

		[DllImport("libigemacintegration.dylib")]
		static extern IntPtr ige_mac_menu_add_app_menu_group();

		public static IgeMacIntegration.IgeMacMenuGroup AddAppMenuGroup() {
			IntPtr raw_ret = ige_mac_menu_add_app_menu_group();
			IgeMacIntegration.IgeMacMenuGroup ret = raw_ret == IntPtr.Zero ? null : (IgeMacIntegration.IgeMacMenuGroup) GLib.Opaque.GetOpaque (raw_ret, typeof (IgeMacIntegration.IgeMacMenuGroup), false);
			return ret;
		}
	}

	public class IgeMacMenuGroup : GLib.Opaque {

		[DllImport("libigemacintegration.dylib")]
		static extern void ige_mac_menu_add_app_menu_item(IntPtr raw, IntPtr menu_item, IntPtr label);

		public void AddMenuItem(Gtk.MenuItem menu_item, string label) {
			IntPtr native_label = GLib.Marshaller.StringToPtrGStrdup (label);
			ige_mac_menu_add_app_menu_item(Handle, menu_item == null ? IntPtr.Zero : menu_item.Handle, native_label);
			GLib.Marshaller.Free (native_label);
		}

		public IgeMacMenuGroup(IntPtr raw) : base(raw) {}
	}
#endif
}