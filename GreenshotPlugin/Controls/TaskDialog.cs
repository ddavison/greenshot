/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2013  Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GreenshotPlugin.UnmanagedHelpers;
using GreenshotPlugin.Core;

namespace GreenshotPlugin.Controls {

	/// <summary>
	/// This is a static wrapper which makes sure we don't open the TaskDialog on Windows XP
	/// </summary>
	public static class GreenshotDialog {
		private static bool isVistaOrLater = Environment.OSVersion.Version.Major >= 6;

		/// <summary>
		/// Show a message
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="title"></param>
		/// <param name="text"></param>
		public static DialogResult Show(IWin32Window owner, string title, string text) {
			if (!isVistaOrLater) {
				return MessageBox.Show(owner, text, title);
			} else {
				TaskDialog taskDialog = new TaskDialog();
				taskDialog.WindowTitle = title;
				taskDialog.MainInstruction = text;
				taskDialog.CustomMainIcon = GreenshotResources.getGreenshotIcon();
				taskDialog.CommonButtons = TaskDialogCommonButtons.Ok;

				return (DialogResult)taskDialog.Show(owner);
			}
		}

		/// <summary>
		/// Ask with Yes, No, Cancel
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="title"></param>
		/// <param name="text"></param>
		public static DialogResult AskYesNoCancel(IWin32Window owner, string title, string text) {
			if (!isVistaOrLater) {
				return MessageBox.Show(owner, text, title, MessageBoxButtons.YesNoCancel);
			} else {
				TaskDialog taskDialog = new TaskDialog();
				taskDialog.WindowTitle = title;
				taskDialog.MainInstruction = text;
				taskDialog.CustomMainIcon = GreenshotResources.getGreenshotIcon();
				taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No | TaskDialogCommonButtons.Cancel;
				taskDialog.UseCommandLinks = true;
				TaskDialogButton button1 = new TaskDialogButton(100, "Testing 1 2 3\r\nBlub bla.");
				TaskDialogButton button2 = new TaskDialogButton(200, "Testing 456\r\nBlub bla.");
				taskDialog.Buttons = new TaskDialogButton[] { button1, button2 };

				return (DialogResult)taskDialog.Show(owner);
			}
		}

		/// <summary>
		/// Ask with Yes, No, Cancel
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="title"></param>
		/// <param name="text"></param>
		public static DialogResult AskYesNo(IWin32Window owner, string title, string text) {
			if (!isVistaOrLater) {
				return MessageBox.Show(owner, text, title, MessageBoxButtons.YesNo);
			} else {
				TaskDialog taskDialog = new TaskDialog();
				taskDialog.WindowTitle = title;
				taskDialog.MainInstruction = text;
				taskDialog.CustomMainIcon = GreenshotResources.getGreenshotIcon();
				taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
				taskDialog.UseCommandLinks = true;


				return (DialogResult)taskDialog.Show(owner);
			}
		}
	}

	/// <summary>
	/// This class is a managed wrapper for the TaskDialog which was added to Vista
	/// </summary>
	public class TaskDialog {
		/// <summary>
		/// TaskDialogIndirect taken from commctl.h
		/// </summary>
		/// <param name="pTaskConfig">All the parameters about the Task Dialog to Show.</param>
		/// <param name="pnButton">The push button pressed.</param>
		/// <param name="pnRadioButton">The radio button that was selected.</param>
		/// <param name="pfVerificationFlagChecked">The state of the verification checkbox on dismiss of the Task Dialog.</param>
		[DllImport("ComCtl32", CharSet = CharSet.Unicode, PreserveSig = false)]
		private static extern void TaskDialogIndirect([In] ref TASKDIALOGCONFIG pTaskConfig, [Out] out int pnButton, [Out] out int pnRadioButton, [Out] out bool pfVerificationFlagChecked);

		/// <summary>
		/// The signature of the callback that receives messages from the Task Dialog when various events occur.
		/// </summary>
		/// <param name="hwnd">The window handle of the </param>
		/// <param name="msg">The message being passed.</param>
		/// <param name="wParam">wParam which is interpreted differently depending on the message.</param>
		/// <param name="lParam">wParam which is interpreted differently depending on the message.</param>
		/// <param name="refData">The refrence data that was set to TaskDialog.CallbackData.</param>
		/// <returns>A HRESULT value. The return value is specific to the message being processed. </returns>
		private delegate int InteropTaskDialogCallback([In] IntPtr hwnd, [In] uint msg, [In] UIntPtr wParam, [In] IntPtr lParam, [In] IntPtr refData);

		#region TaskDialog Interop structures

		/// <summary>
		/// TASKDIALOGCONFIG taken from commctl.h.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
		private struct TASKDIALOGCONFIG {
			/// <summary>
			/// Size of the structure in bytes.
			/// </summary>
			public uint cbSize;

			/// <summary>
			/// Parent window handle.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr hwndParent;

			/// <summary>
			/// Module instance handle for resources.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr hInstance;

			/// <summary>
			/// Flags.
			/// </summary>
			public TASKDIALOG_FLAGS dwFlags;            // TASKDIALOG_FLAGS (TDF_XXX) flags

			/// <summary>
			/// Bit flags for commonly used buttons.
			/// </summary>
			public TaskDialogCommonButtons dwCommonButtons;    // TASKDIALOG_COMMON_BUTTON (TDCBF_XXX) flags

			/// <summary>
			/// Window title.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszWindowTitle;                         // string or MAKEINTRESOURCE()

			/// <summary>
			/// The Main icon. Overloaded member. Can be string, a handle, a special value or a resource ID.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr MainIcon;

			/// <summary>
			/// Main Instruction.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszMainInstruction;

			/// <summary>
			/// Content.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszContent;

			/// <summary>
			/// Count of custom Buttons.
			/// </summary>
			public uint cButtons;

			/// <summary>
			/// Array of custom buttons.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr pButtons;

			/// <summary>
			/// ID of default button.
			/// </summary>
			public int nDefaultButton;

			/// <summary>
			/// Count of radio Buttons.
			/// </summary>
			public uint cRadioButtons;

			/// <summary>
			/// Array of radio buttons.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr pRadioButtons;

			/// <summary>
			/// ID of default radio button.
			/// </summary>
			public int nDefaultRadioButton;

			/// <summary>
			/// Text for verification check box. often "Don't ask be again".
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszVerificationText;

			/// <summary>
			/// Expanded Information.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszExpandedInformation;

			/// <summary>
			/// Text for expanded control.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszExpandedControlText;

			/// <summary>
			/// Text for expanded control.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszCollapsedControlText;

			/// <summary>
			/// Icon for the footer. An overloaded member link MainIcon.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr FooterIcon;

			/// <summary>
			/// Footer Text.
			/// </summary>
			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszFooter;

			/// <summary>
			/// Function pointer for callback.
			/// </summary>
			public InteropTaskDialogCallback pfCallback;

			/// <summary>
			/// Data that will be passed to the call back.
			/// </summary>
			[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // Managed code owns actual resource. Passed to native in syncronous call. No lifetime issues.
			public IntPtr lpCallbackData;

			/// <summary>
			/// Width of the Task Dialog's area in DLU's.
			/// </summary>
			public uint cxWidth;                                // width of the Task Dialog's client area in DLU's. If 0, Task Dialog will calculate the ideal width.
		}

		#endregion

		#region Interop Enums
		/// <summary>
		/// TASKDIALOG_FLAGS taken from CommCtrl.h.
		/// </summary>
		[Flags]
		private enum TASKDIALOG_FLAGS {
			/// <summary>
			/// Enable hyperlinks.
			/// </summary>
			TDF_ENABLE_HYPERLINKS = 0x0001,

			/// <summary>
			/// Use icon handle for main icon.
			/// </summary>
			TDF_USE_HICON_MAIN = 0x0002,

			/// <summary>
			/// Use icon handle for footer icon.
			/// </summary>
			TDF_USE_HICON_FOOTER = 0x0004,

			/// <summary>
			/// Allow dialog to be cancelled, even if there is no cancel button.
			/// </summary>
			TDF_ALLOW_DIALOG_CANCELLATION = 0x0008,

			/// <summary>
			/// Use command links rather than buttons.
			/// </summary>
			TDF_USE_COMMAND_LINKS = 0x0010,

			/// <summary>
			/// Use command links with no icons rather than buttons.
			/// </summary>
			TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020,

			/// <summary>
			/// Show expanded info in the footer area.
			/// </summary>
			TDF_EXPAND_FOOTER_AREA = 0x0040,

			/// <summary>
			/// Expand by default.
			/// </summary>
			TDF_EXPANDED_BY_DEFAULT = 0x0080,

			/// <summary>
			/// Start with verification flag already checked.
			/// </summary>
			TDF_VERIFICATION_FLAG_CHECKED = 0x0100,

			/// <summary>
			/// Show a progress bar.
			/// </summary>
			TDF_SHOW_PROGRESS_BAR = 0x0200,

			/// <summary>
			/// Show a marquee progress bar.
			/// </summary>
			TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400,

			/// <summary>
			/// Callback every 200 milliseconds.
			/// </summary>
			TDF_CALLBACK_TIMER = 0x0800,

			/// <summary>
			/// Center the dialog on the owner window rather than the monitor.
			/// </summary>
			TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000,

			/// <summary>
			/// Right to Left Layout.
			/// </summary>
			TDF_RTL_LAYOUT = 0x2000,

			/// <summary>
			/// No default radio button.
			/// </summary>
			TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000,

			/// <summary>
			/// Task Dialog can be minimized.
			/// </summary>
			TDF_CAN_BE_MINIMIZED = 0x8000
		}
		#endregion
		
		/// <summary>
		/// The string to be used for the dialog box title. If this parameter is NULL, the filename of the executable program is used.
		/// </summary>
		private string windowTitle;

		/// <summary>
		/// The string to be used for the main instruction.
		/// </summary>
		private string mainInstruction;

		/// <summary>
		/// The string to be used for the dialog’s primary content. If the EnableHyperlinks member is true,
		/// then this string may contain hyperlinks in the form: <A HREF="executablestring">Hyperlink Text</A>. 
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		private string content;

		/// <summary>
		/// Specifies the push buttons displayed in the dialog box.  This parameter may be a combination of flags.
		/// If no common buttons are specified and no custom buttons are specified using the Buttons member, the
		/// dialog box will contain the OK button by default.
		/// </summary>
		private TaskDialogCommonButtons commonButtons;

		/// <summary>
		/// Specifies a built in icon for the main icon in the dialog. If this is set to none
		/// and the CustomMainIcon is null then no main icon will be displayed.
		/// </summary>
		private TaskDialogIcon mainIcon;

		/// <summary>
		/// Specifies a custom in icon for the main icon in the dialog. If this is set to none
		/// and the CustomMainIcon member is null then no main icon will be displayed.
		/// </summary>
		private Icon customMainIcon;

		/// <summary>
		/// Specifies a built in icon for the icon to be displayed in the footer area of the
		/// dialog box. If this is set to none and the CustomFooterIcon member is null then no
		/// footer icon will be displayed.
		/// </summary>
		private TaskDialogIcon footerIcon;

		/// <summary>
		/// Specifies a custom icon for the icon to be displayed in the footer area of the
		/// dialog box. If this is set to none and the CustomFooterIcon member is null then no
		/// footer icon will be displayed.
		/// </summary>
		private Icon customFooterIcon;

		/// <summary>
		/// Specifies the custom push buttons to display in the dialog. Use CommonButtons member for
		/// common buttons; OK, Yes, No, Retry and Cancel, and Buttons when you want different text
		/// on the push buttons.
		/// </summary>
		private TaskDialogButton[] buttons;

		/// <summary>
		/// Specifies the radio buttons to display in the dialog.
		/// </summary>
		private TaskDialogButton[] radioButtons;

		/// <summary>
		/// The flags passed to TaskDialogIndirect.
		/// </summary>
		private TASKDIALOG_FLAGS flags;

		/// <summary>
		/// Indicates the default button for the dialog. This may be any of the values specified
		/// in ButtonId members of one of the TaskDialogButton structures in the Buttons array,
		/// or one a DialogResult value that corresponds to a buttons specified in the CommonButtons Member.
		/// If this member is zero or its value does not correspond to any button ID in the dialog,
		/// then the first button in the dialog will be the default. 
		/// </summary>
		private int defaultButton;

		/// <summary>
		/// Indicates the default radio button for the dialog. This may be any of the values specified
		/// in ButtonId members of one of the TaskDialogButton structures in the RadioButtons array.
		/// If this member is zero or its value does not correspond to any radio button ID in the dialog,
		/// then the first button in RadioButtons will be the default.
		/// The property NoDefaultRadioButton can be set to have no default.
		/// </summary>
		private int defaultRadioButton;

		/// <summary>
		/// The string to be used to label the verification checkbox. If this member is null, the
		/// verification checkbox is not displayed in the dialog box.
		/// </summary>
		private string verificationText;

		/// <summary>
		/// The string to be used for displaying additional information. The additional information is
		/// displayed either immediately below the content or below the footer text depending on whether
		/// the ExpandFooterArea member is true. If the EnableHyperlinks member is true, then this string
		/// may contain hyperlinks in the form: <A HREF="executablestring">Hyperlink Text</A>.
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		private string expandedInformation;

		/// <summary>
		/// The string to be used to label the button for collapsing the expanded information. This
		/// member is ignored when the ExpandedInformation member is null. If this member is null
		/// and the CollapsedControlText is specified, then the CollapsedControlText value will be
		/// used for this member as well.
		/// </summary>
		private string expandedControlText;

		/// <summary>
		/// The string to be used to label the button for expanding the expanded information. This
		/// member is ignored when the ExpandedInformation member is null.  If this member is null
		/// and the ExpandedControlText is specified, then the ExpandedControlText value will be
		/// used for this member as well.
		/// </summary>
		private string collapsedControlText;

		/// <summary>
		/// The string to be used in the footer area of the dialog box. If the EnableHyperlinks member
		/// is true, then this string may contain hyperlinks in the form: <A HREF="executablestring">
		/// Hyperlink Text</A>.
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		private string footer;

		/// <summary>
		/// The callback that receives messages from the Task Dialog when various events occur.
		/// </summary>
		private TaskDialogCallback callback;

		/// <summary>
		/// Reference that is passed to the callback.
		/// </summary>
		private object callbackData;

		/// <summary>
		/// Specifies the width of the Task Dialog’s client area in DLU’s. If 0, Task Dialog will calculate the ideal width.
		/// </summary>
		private uint width;

		/// <summary>
		/// Creates a default Task Dialog.
		/// </summary>
		public TaskDialog() {
			this.Reset();
		}

		/// <summary>
		/// Returns true if the current operating system supports TaskDialog. If false TaskDialog.Show should not
		/// be called as the results are undefined but often results in a crash.
		/// </summary>
		public static bool IsAvailableOnThisOS {
			get {
				OperatingSystem os = Environment.OSVersion;
				if (os.Platform != PlatformID.Win32NT) {
					return false;
				}

				return (os.Version.CompareTo(TaskDialog.RequiredOSVersion) >= 0);
			}
		}

		/// <summary>
		/// The minimum Windows version needed to support TaskDialog.
		/// </summary>
		public static Version RequiredOSVersion {
			get {
				return new Version(6, 0, 5243);
			}
		}

		/// <summary>
		/// The string to be used for the dialog box title. If this parameter is NULL, the filename of the executable program is used.
		/// </summary>
		public string WindowTitle {
			get {
				return this.windowTitle;
			}
			set {
				this.windowTitle = value;
			}
		}

		/// <summary>
		/// The string to be used for the main instruction.
		/// </summary>
		public string MainInstruction {
			get {
				return this.mainInstruction;
			}
			set {
				this.mainInstruction = value;
			}
		}

		/// <summary>
		/// The string to be used for the dialog’s primary content. If the EnableHyperlinks member is true,
		/// then this string may contain hyperlinks in the form: <A HREF="executablestring">Hyperlink Text</A>. 
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		public string Content {
			get {
				return this.content;
			}
			set {
				this.content = value;
			}
		}

		/// <summary>
		/// Specifies the push buttons displayed in the dialog box. This parameter may be a combination of flags.
		/// If no common buttons are specified and no custom buttons are specified using the Buttons member, the
		/// dialog box will contain the OK button by default.
		/// </summary>
		public TaskDialogCommonButtons CommonButtons {
			get {
				return this.commonButtons;
			}
			set {
				this.commonButtons = value;
			}
		}

		/// <summary>
		/// Specifies a built in icon for the main icon in the dialog. If this is set to none
		/// and the CustomMainIcon is null then no main icon will be displayed.
		/// </summary>
		public TaskDialogIcon MainIcon {
			get {
				return this.mainIcon;
			}
			set {
				this.mainIcon = value;
			}
		}

		/// <summary>
		/// Specifies a custom in icon for the main icon in the dialog. If this is set to none
		/// and the CustomMainIcon member is null then no main icon will be displayed.
		/// </summary>
		public Icon CustomMainIcon {
			get {
				return this.customMainIcon;
			}
			set {
				this.customMainIcon = value;
			}
		}

		/// <summary>
		/// Specifies a built in icon for the icon to be displayed in the footer area of the
		/// dialog box. If this is set to none and the CustomFooterIcon member is null then no
		/// footer icon will be displayed.
		/// </summary>
		public TaskDialogIcon FooterIcon {
			get {
				return this.footerIcon;
			}
			set {
				this.footerIcon = value;
			}
		}

		/// <summary>
		/// Specifies a custom icon for the icon to be displayed in the footer area of the
		/// dialog box. If this is set to none and the CustomFooterIcon member is null then no
		/// footer icon will be displayed.
		/// </summary>
		public Icon CustomFooterIcon {
			get {
				return this.customFooterIcon;
			}
			set {
				this.customFooterIcon = value;
			}
		}

		/// <summary>
		/// Specifies the custom push buttons to display in the dialog. Use CommonButtons member for
		/// common buttons; OK, Yes, No, Retry and Cancel, and Buttons when you want different text
		/// on the push buttons.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // Style of use is like single value. Array is of value types.
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // Returns a reference, not a copy.
		public TaskDialogButton[] Buttons {
			get {
				return this.buttons;
			}

			set {
				if (value == null) {
					throw new ArgumentNullException("value");
				}

				this.buttons = value;
			}
		}

		/// <summary>
		/// Specifies the radio buttons to display in the dialog.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // Style of use is like single value. Array is of value types.
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")] // Returns a reference, not a copy.
		public TaskDialogButton[] RadioButtons {
			get {
				return this.radioButtons;
			}

			set {
				if (value == null) {
					throw new ArgumentNullException("value");
				}

				this.radioButtons = value;
			}
		}

		/// <summary>
		/// Enables hyperlink processing for the strings specified in the Content, ExpandedInformation
		/// and FooterText members. When enabled, these members may be strings that contain hyperlinks
		/// in the form: <A HREF="executablestring">Hyperlink Text</A>. 
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// Note: Task Dialog will not actually execute any hyperlinks. Hyperlink execution must be handled
		/// in the callback function specified by Callback member.
		/// </summary>
		public bool EnableHyperlinks {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_ENABLE_HYPERLINKS, value);
			}
		}

		/// <summary>
		/// Indicates that the dialog should be able to be closed using Alt-F4, Escape and the title bar’s
		/// close button even if no cancel button is specified in either the CommonButtons or Buttons members.
		/// </summary>
		public bool AllowDialogCancellation {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_ALLOW_DIALOG_CANCELLATION, value);
			}
		}

		/// <summary>
		/// Indicates that the buttons specified in the Buttons member should be displayed as command links
		/// (using a standard task dialog glyph) instead of push buttons.  When using command links, all
		/// characters up to the first new line character in the ButtonText member (of the TaskDialogButton
		/// structure) will be treated as the command link’s main text, and the remainder will be treated
		/// as the command link’s note. This flag is ignored if the Buttons member has no entires.
		/// </summary>
		public bool UseCommandLinks {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS, value);
			}
		}

		/// <summary>
		/// Indicates that the buttons specified in the Buttons member should be displayed as command links
		/// (without a glyph) instead of push buttons. When using command links, all characters up to the
		/// first new line character in the ButtonText member (of the TaskDialogButton structure) will be
		/// treated as the command link’s main text, and the remainder will be treated as the command link’s
		/// note. This flag is ignored if the Buttons member has no entires.
		/// </summary>
		public bool UseCommandLinksNoIcon {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON, value);
			}
		}

		/// <summary>
		/// Indicates that the string specified by the ExpandedInformation member should be displayed at the
		/// bottom of the dialog’s footer area instead of immediately after the dialog’s content. This flag
		/// is ignored if the ExpandedInformation member is null.
		/// </summary>
		public bool ExpandFooterArea {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_EXPAND_FOOTER_AREA, value);
			}
		}

		/// <summary>
		/// Indicates that the string specified by the ExpandedInformation member should be displayed
		/// when the dialog is initially displayed. This flag is ignored if the ExpandedInformation member
		/// is null.
		/// </summary>
		public bool ExpandedByDefault {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_EXPANDED_BY_DEFAULT, value);
			}
		}

		/// <summary>
		/// Indicates that the verification checkbox in the dialog should be checked when the dialog is
		/// initially displayed. This flag is ignored if the VerificationText parameter is null.
		/// </summary>
		public bool VerificationFlagChecked {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED, value);
			}
		}

		/// <summary>
		/// Indicates that a Progress Bar should be displayed.
		/// </summary>
		public bool ShowProgressBar {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR, value);
			}
		}

		/// <summary>
		/// Indicates that an Marquee Progress Bar should be displayed.
		/// </summary>
		public bool ShowMarqueeProgressBar {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR, value);
			}
		}

		/// <summary>
		/// Indicates that the TaskDialog’s callback should be called approximately every 200 milliseconds.
		/// </summary>
		public bool CallbackTimer {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER, value);
			}
		}

		/// <summary>
		/// Indicates that the TaskDialog should be positioned (centered) relative to the owner window
		/// passed when calling Show. If not set (or no owner window is passed), the TaskDialog is
		/// positioned (centered) relative to the monitor.
		/// </summary>
		public bool PositionRelativeToWindow {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_POSITION_RELATIVE_TO_WINDOW, value);
			}
		}

		/// <summary>
		/// Indicates that the TaskDialog should have right to left layout.
		/// </summary>
		public bool RightToLeftLayout {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_RTL_LAYOUT) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_RTL_LAYOUT, value);
			}
		}

		/// <summary>
		/// Indicates that the TaskDialog should have no default radio button.
		/// </summary>
		public bool NoDefaultRadioButton {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON, value);
			}
		}

		/// <summary>
		/// Indicates that the TaskDialog can be minimised. Works only if there if parent window is null. Will enable cancellation also.
		/// </summary>
		public bool CanBeMinimized {
			get {
				return (this.flags & TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED) != 0;
			}
			set {
				this.SetFlag(TASKDIALOG_FLAGS.TDF_CAN_BE_MINIMIZED, value);
			}
		}

		/// <summary>
		/// Indicates the default button for the dialog. This may be any of the values specified
		/// in ButtonId members of one of the TaskDialogButton structures in the Buttons array,
		/// or one a DialogResult value that corresponds to a buttons specified in the CommonButtons Member.
		/// If this member is zero or its value does not correspond to any button ID in the dialog,
		/// then the first button in the dialog will be the default. 
		/// </summary>
		public int DefaultButton {
			get {
				return this.defaultButton;
			}
			set {
				this.defaultButton = value;
			}
		}

		/// <summary>
		/// Indicates the default radio button for the dialog. This may be any of the values specified
		/// in ButtonId members of one of the TaskDialogButton structures in the RadioButtons array.
		/// If this member is zero or its value does not correspond to any radio button ID in the dialog,
		/// then the first button in RadioButtons will be the default.
		/// The property NoDefaultRadioButton can be set to have no default.
		/// </summary>
		public int DefaultRadioButton {
			get {
				return this.defaultRadioButton;
			}
			set {
				this.defaultRadioButton = value;
			}
		}

		/// <summary>
		/// The string to be used to label the verification checkbox. If this member is null, the
		/// verification checkbox is not displayed in the dialog box.
		/// </summary>
		public string VerificationText {
			get {
				return this.verificationText;
			}
			set {
				this.verificationText = value;
			}
		}

		/// <summary>
		/// The string to be used for displaying additional information. The additional information is
		/// displayed either immediately below the content or below the footer text depending on whether
		/// the ExpandFooterArea member is true. If the EnameHyperlinks member is true, then this string
		/// may contain hyperlinks in the form: <A HREF="executablestring">Hyperlink Text</A>.
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		public string ExpandedInformation {
			get {
				return this.expandedInformation;
			}
			set {
				this.expandedInformation = value;
			}
		}

		/// <summary>
		/// The string to be used to label the button for collapsing the expanded information. This
		/// member is ignored when the ExpandedInformation member is null. If this member is null
		/// and the CollapsedControlText is specified, then the CollapsedControlText value will be
		/// used for this member as well.
		/// </summary>
		public string ExpandedControlText {
			get {
				return this.expandedControlText;
			}
			set {
				this.expandedControlText = value;
			}
		}

		/// <summary>
		/// The string to be used to label the button for expanding the expanded information. This
		/// member is ignored when the ExpandedInformation member is null.  If this member is null
		/// and the ExpandedControlText is specified, then the ExpandedControlText value will be
		/// used for this member as well.
		/// </summary>
		public string CollapsedControlText {
			get {
				return this.collapsedControlText;
			}
			set {
				this.collapsedControlText = value;
			}
		}

		/// <summary>
		/// The string to be used in the footer area of the dialog box. If the EnableHyperlinks member
		/// is true, then this string may contain hyperlinks in the form: <A HREF="executablestring">
		/// Hyperlink Text</A>.
		/// WARNING: Enabling hyperlinks when using content from an unsafe source may cause security vulnerabilities.
		/// </summary>
		public string Footer {
			get {
				return this.footer;
			}
			set {
				this.footer = value;
			}
		}

		/// <summary>
		/// width of the Task Dialog's client area in DLU's. If 0, Task Dialog will calculate the ideal width.
		/// </summary>
		public uint Width {
			get {
				return this.width;
			}
			set {
				this.width = value;
			}
		}

		/// <summary>
		/// The callback that receives messages from the Task Dialog when various events occur.
		/// </summary>
		public TaskDialogCallback Callback {
			get {
				return this.callback;
			}
			set {
				this.callback = value;
			}
		}

		/// <summary>
		/// Reference that is passed to the callback.
		/// </summary>
		public object CallbackData {
			get {
				return this.callbackData;
			}
			set {
				this.callbackData = value;
			}
		}

		/// <summary>
		/// Resets the Task Dialog to the state when first constructed, all properties set to their default value.
		/// </summary>
		public void Reset() {
			this.windowTitle = null;
			this.mainInstruction = null;
			this.content = null;
			this.commonButtons = 0;
			this.mainIcon = TaskDialogIcon.None;
			this.customMainIcon = null;
			this.footerIcon = TaskDialogIcon.None;
			this.customFooterIcon = null;
			this.buttons = new TaskDialogButton[0];
			this.radioButtons = new TaskDialogButton[0];
			this.flags = 0;
			this.defaultButton = 0;
			this.defaultRadioButton = 0;
			this.verificationText = null;
			this.expandedInformation = null;
			this.expandedControlText = null;
			this.collapsedControlText = null;
			this.footer = null;
			this.callback = null;
			this.callbackData = null;
			this.width = 0;
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show() {
			bool verificationFlagChecked;
			int radioButtonResult;
			return this.Show(IntPtr.Zero, out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="owner">Owner window the task Dialog will modal to.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IWin32Window owner) {
			bool verificationFlagChecked;
			int radioButtonResult;
			return this.Show((owner == null ? IntPtr.Zero : owner.Handle), out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="hwndOwner">Owner window the task Dialog will modal to.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IntPtr hwndOwner) {
			bool verificationFlagChecked;
			int radioButtonResult;
			return this.Show(hwndOwner, out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="owner">Owner window the task Dialog will modal to.</param>
		/// <param name="verificationFlagChecked">Returns true if the verification checkbox was checked when the dialog
		/// was dismissed.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IWin32Window owner, out bool verificationFlagChecked) {
			int radioButtonResult;
			return this.Show((owner == null ? IntPtr.Zero : owner.Handle), out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="hwndOwner">Owner window the task Dialog will modal to.</param>
		/// <param name="verificationFlagChecked">Returns true if the verification checkbox was checked when the dialog
		/// was dismissed.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IntPtr hwndOwner, out bool verificationFlagChecked) {
			// We have to call a private version or PreSharp gets upset about a unsafe
			// block in a public method. (PreSharp error 56505)
			int radioButtonResult;
			return this.PrivateShow(hwndOwner, out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="owner">Owner window the task Dialog will modal to.</param>
		/// <param name="verificationFlagChecked">Returns true if the verification checkbox was checked when the dialog
		/// was dismissed.</param>
		/// <param name="radioButtonResult">The radio botton selected by the user.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IWin32Window owner, out bool verificationFlagChecked, out int radioButtonResult) {
			return this.Show((owner == null ? IntPtr.Zero : owner.Handle), out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="hwndOwner">Owner window the task Dialog will modal to.</param>
		/// <param name="verificationFlagChecked">Returns true if the verification checkbox was checked when the dialog
		/// was dismissed.</param>
		/// <param name="radioButtonResult">The radio botton selected by the user.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		public int Show(IntPtr hwndOwner, out bool verificationFlagChecked, out int radioButtonResult) {
			// We have to call a private version or PreSharp gets upset about a unsafe
			// block in a public method. (PreSharp error 56505)
			return this.PrivateShow(hwndOwner, out verificationFlagChecked, out radioButtonResult);
		}

		/// <summary>
		/// Creates, displays, and operates a task dialog. The task dialog contains application-defined messages, title,
		/// verification check box, command links and push buttons, plus any combination of predefined icons and push buttons
		/// as specified on the other members of the class before calling Show.
		/// </summary>
		/// <param name="hwndOwner">Owner window the task Dialog will modal to.</param>
		/// <param name="verificationFlagChecked">Returns true if the verification checkbox was checked when the dialog
		/// was dismissed.</param>
		/// <param name="radioButtonResult">The radio botton selected by the user.</param>
		/// <returns>The result of the dialog, either a DialogResult value for common push buttons set in the CommonButtons
		/// member or the ButtonID from a TaskDialogButton structure set on the Buttons member.</returns>
		private int PrivateShow(IntPtr hwndOwner, out bool verificationFlagChecked, out int radioButtonResult) {
			verificationFlagChecked = false;
			radioButtonResult = 0;
			int result = 0;
			TASKDIALOGCONFIG config = new TASKDIALOGCONFIG();

			try {
				config.cbSize = (uint)Marshal.SizeOf(typeof(TASKDIALOGCONFIG));
				config.hwndParent = hwndOwner;
				config.dwFlags = this.flags;
				config.dwCommonButtons = this.commonButtons;

				if (!string.IsNullOrEmpty(this.windowTitle)) {
					config.pszWindowTitle = this.windowTitle;
				}

				config.MainIcon = (IntPtr)this.mainIcon;
				if (this.customMainIcon != null) {
					config.dwFlags |= TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN;
					config.MainIcon = this.customMainIcon.Handle;
				}

				if (!string.IsNullOrEmpty(this.mainInstruction)) {
					config.pszMainInstruction = this.mainInstruction;
				}

				if (!string.IsNullOrEmpty(this.content)) {
					config.pszContent = this.content;
				}

				TaskDialogButton[] customButtons = this.buttons;
				if (customButtons.Length > 0) {
					// Hand marshal the buttons array.
					int elementSize = Marshal.SizeOf(typeof(TaskDialogButton));
					config.pButtons = Marshal.AllocHGlobal(elementSize * (int)customButtons.Length);
					for (int i = 0; i < customButtons.Length; i++) {
						unsafe // Unsafe because of pointer arithmatic.
						{
							byte* p = (byte*)config.pButtons;
							Marshal.StructureToPtr(customButtons[i], (IntPtr)(p + (elementSize * i)), false);
						}

						config.cButtons++;
					}
				}

				TaskDialogButton[] customRadioButtons = this.radioButtons;
				if (customRadioButtons.Length > 0) {
					// Hand marshal the buttons array.
					int elementSize = Marshal.SizeOf(typeof(TaskDialogButton));
					config.pRadioButtons = Marshal.AllocHGlobal(elementSize * (int)customRadioButtons.Length);
					for (int i = 0; i < customRadioButtons.Length; i++) {
						unsafe // Unsafe because of pointer arithmatic.
						{
							byte* p = (byte*)config.pRadioButtons;
							Marshal.StructureToPtr(customRadioButtons[i], (IntPtr)(p + (elementSize * i)), false);
						}

						config.cRadioButtons++;
					}
				}

				config.nDefaultButton = this.defaultButton;
				config.nDefaultRadioButton = this.defaultRadioButton;

				if (!string.IsNullOrEmpty(this.verificationText)) {
					config.pszVerificationText = this.verificationText;
				}

				if (!string.IsNullOrEmpty(this.expandedInformation)) {
					config.pszExpandedInformation = this.expandedInformation;
				}

				if (!string.IsNullOrEmpty(this.expandedControlText)) {
					config.pszExpandedControlText = this.expandedControlText;
				}

				if (!string.IsNullOrEmpty(this.collapsedControlText)) {
					config.pszCollapsedControlText = this.CollapsedControlText;
				}

				config.FooterIcon = (IntPtr)this.footerIcon;
				if (this.customFooterIcon != null) {
					config.dwFlags |= TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER;
					config.FooterIcon = this.customFooterIcon.Handle;
				}

				if (!string.IsNullOrEmpty(this.footer)) {
					config.pszFooter = this.footer;
				}

				// If our user has asked for a callback then we need to ask for one to
				// translate to the friendly version.
				if (this.callback != null) {
					config.pfCallback = new InteropTaskDialogCallback(this.PrivateCallback);
				}

				////config.lpCallbackData = this.callbackData; // How do you do this? Need to pin the ref?
				config.cxWidth = this.width;

				// The call all this mucking about is here for.
				TaskDialogIndirect(ref config, out result, out radioButtonResult, out verificationFlagChecked);
			} finally {
				// Free the unmanged memory needed for the button arrays.
				// There is the possiblity of leaking memory if the app-domain is destroyed in a non clean way
				// and the hosting OS process is kept alive but fixing this would require using hardening techniques
				// that are not required for the users of this class.
				if (config.pButtons != IntPtr.Zero) {
					int elementSize = Marshal.SizeOf(typeof(TaskDialogButton));
					for (int i = 0; i < config.cButtons; i++) {
						unsafe {
							byte* p = (byte*)config.pButtons;
							Marshal.DestroyStructure((IntPtr)(p + (elementSize * i)), typeof(TaskDialogButton));
						}
					}

					Marshal.FreeHGlobal(config.pButtons);
				}

				if (config.pRadioButtons != IntPtr.Zero) {
					int elementSize = Marshal.SizeOf(typeof(TaskDialogButton));
					for (int i = 0; i < config.cRadioButtons; i++) {
						unsafe {
							byte* p = (byte*)config.pRadioButtons;
							Marshal.DestroyStructure((IntPtr)(p + (elementSize * i)), typeof(TaskDialogButton));
						}
					}

					Marshal.FreeHGlobal(config.pRadioButtons);
				}
			}

			return result;
		}

		/// <summary>
		/// The callback from the native Task Dialog. This prepares the friendlier arguments and calls the simplier callback.
		/// </summary>
		/// <param name="hwnd">The window handle of the Task Dialog that is active.</param>
		/// <param name="msg">The notification. A TaskDialogNotification value.</param>
		/// <param name="wparam">Specifies additional noitification information.  The contents of this parameter depends on the value of the msg parameter.</param>
		/// <param name="lparam">Specifies additional noitification information.  The contents of this parameter depends on the value of the msg parameter.</param>
		/// <param name="refData">Specifies the application-defined value given in the call to TaskDialogIndirect.</param>
		/// <returns>A HRESULT. It's not clear in the spec what a failed result will do.</returns>
		private int PrivateCallback([In] IntPtr hwnd, [In] uint msg, [In] UIntPtr wparam, [In] IntPtr lparam, [In] IntPtr refData) {
			TaskDialogCallback callback = this.callback;
			if (callback != null) {
				// Prepare arguments for the callback to the user we are insulating from Interop casting sillyness.

				// Future: Consider reusing a single ActiveTaskDialog object and mark it as destroyed on the destry notification.
				ActiveTaskDialog activeDialog = new ActiveTaskDialog(hwnd);
				TaskDialogNotificationArgs args = new TaskDialogNotificationArgs();
				args.Notification = (TaskDialogNotification)msg;
				switch (args.Notification) {
					case TaskDialogNotification.ButtonClicked:
					case TaskDialogNotification.RadioButtonClicked:
						args.ButtonId = (int)wparam;
						break;
					case TaskDialogNotification.HyperlinkClicked:
						args.Hyperlink = Marshal.PtrToStringUni(lparam);
						break;
					case TaskDialogNotification.Timer:
						args.TimerTickCount = (uint)wparam;
						break;
					case TaskDialogNotification.VerificationClicked:
						args.VerificationFlagChecked = (wparam != UIntPtr.Zero);
						break;
					case TaskDialogNotification.ExpandoButtonClicked:
						args.Expanded = (wparam != UIntPtr.Zero);
						break;
				}

				return (callback(activeDialog, args, this.callbackData) ? 1 : 0);
			}

			return 0; // false;
		}

		/// <summary>
		/// Helper function to set or clear a bit in the flags field.
		/// </summary>
		/// <param name="flag">The Flag bit to set or clear.</param>
		/// <param name="value">True to set, false to clear the bit in the flags field.</param>
		private void SetFlag(TASKDIALOG_FLAGS flag, bool value) {
			if (value) {
				this.flags |= flag;
			} else {
				this.flags &= ~flag;
			}
		}
	}

	/// <summary>
	/// The signature of the callback that recieves notificaitons from the Task Dialog.
	/// </summary>
	/// <param name="taskDialog">The active task dialog which has methods that can be performed on an active Task Dialog.</param>
	/// <param name="args">The notification arguments including the type of notification and information for the notification.</param>
	/// <param name="callbackData">The value set on TaskDialog.CallbackData</param>
	/// <returns>Return value meaning varies depending on the Notification member of args.</returns>
	public delegate bool TaskDialogCallback(ActiveTaskDialog taskDialog, TaskDialogNotificationArgs args, object callbackData);

	/// <summary>
	/// The TaskDialog common button flags used to specify the builtin bottons to show in the TaskDialog.
	/// </summary>
	[Flags]
	public enum TaskDialogCommonButtons {
		/// <summary>
		/// No common buttons.
		/// </summary>
		None = 0,

		/// <summary>
		/// OK common button. If selected Task Dialog will return DialogResult.OK.
		/// </summary>
		Ok = 0x0001,

		/// <summary>
		/// Yes common button. If selected Task Dialog will return DialogResult.Yes.
		/// </summary>
		Yes = 0x0002,

		/// <summary>
		/// No common button. If selected Task Dialog will return DialogResult.No.
		/// </summary>
		No = 0x0004,

		/// <summary>
		/// Cancel common button. If selected Task Dialog will return DialogResult.Cancel.
		/// If this button is specified, the dialog box will respond to typical cancel actions (Alt-F4 and Escape).
		/// </summary>
		Cancel = 0x0008,

		/// <summary>
		/// Retry common button. If selected Task Dialog will return DialogResult.Retry.
		/// </summary>
		Retry = 0x0010,

		/// <summary>
		/// Close common button. If selected Task Dialog will return this value.
		/// </summary>
		Close = 0x0020,
	}

	/// <summary>
	/// The System icons the TaskDialog supports.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")] // Type comes from CommCtrl.h
	public enum TaskDialogIcon : uint {
		/// <summary>
		/// No Icon.
		/// </summary>
		None = 0,

		/// <summary>
		/// System warning icon.
		/// </summary>
		Warning = 0xFFFF, // MAKEINTRESOURCEW(-1)

		/// <summary>
		/// System Error icon.
		/// </summary>
		Error = 0xFFFE, // MAKEINTRESOURCEW(-2)

		/// <summary>
		/// System Information icon.
		/// </summary>
		Information = 0xFFFD, // MAKEINTRESOURCEW(-3)

		/// <summary>
		/// Shield icon.
		/// </summary>
		Shield = 0xFFFC, // MAKEINTRESOURCEW(-4)
	}

	/// <summary>
	/// Task Dialog callback notifications. 
	/// </summary>
	public enum TaskDialogNotification {
		/// <summary>
		/// Sent by the Task Dialog once the dialog has been created and before it is displayed.
		/// The value returned by the callback is ignored.
		/// </summary>
		Created = 0,

		//// Spec is not clear what this is so not supporting it.
		///// <summary>
		///// Sent by the Task Dialog when a navigation has occurred.
		///// The value returned by the callback is ignored.
		///// </summary>   
		// Navigated = 1,

		/// <summary>
		/// Sent by the Task Dialog when the user selects a button or command link in the task dialog.
		/// The button ID corresponding to the button selected will be available in the
		/// TaskDialogNotificationArgs. To prevent the Task Dialog from closing, the application must
		/// return true, otherwise the Task Dialog will be closed and the button ID returned to via
		/// the original application call.
		/// </summary>
		ButtonClicked = 2,            // wParam = Button ID

		/// <summary>
		/// Sent by the Task Dialog when the user clicks on a hyperlink in the Task Dialog’s content.
		/// The string containing the HREF of the hyperlink will be available in the
		/// TaskDialogNotificationArgs. To prevent the TaskDialog from shell executing the hyperlink,
		/// the application must return TRUE, otherwise ShellExecute will be called.
		/// </summary>
		HyperlinkClicked = 3,            // lParam = (LPCWSTR)pszHREF

		/// <summary>
		/// Sent by the Task Dialog approximately every 200 milliseconds when TaskDialog.CallbackTimer
		/// has been set to true. The number of milliseconds since the dialog was created or the
		/// notification returned true is available on the TaskDialogNotificationArgs. To reset
		/// the tickcount, the application must return true, otherwise the tickcount will continue to
		/// increment.
		/// </summary>
		Timer = 4,            // wParam = Milliseconds since dialog created or timer reset

		/// <summary>
		/// Sent by the Task Dialog when it is destroyed and its window handle no longer valid.
		/// The value returned by the callback is ignored.
		/// </summary>
		Destroyed = 5,

		/// <summary>
		/// Sent by the Task Dialog when the user selects a radio button in the task dialog.
		/// The button ID corresponding to the button selected will be available in the
		/// TaskDialogNotificationArgs.
		/// The value returned by the callback is ignored.
		/// </summary>
		RadioButtonClicked = 6,            // wParam = Radio Button ID

		/// <summary>
		/// Sent by the Task Dialog once the dialog has been constructed and before it is displayed.
		/// The value returned by the callback is ignored.
		/// </summary>
		DialogConstructed = 7,

		/// <summary>
		/// Sent by the Task Dialog when the user checks or unchecks the verification checkbox.
		/// The verificationFlagChecked value is available on the TaskDialogNotificationArgs.
		/// The value returned by the callback is ignored.
		/// </summary>
		VerificationClicked = 8,             // wParam = 1 if checkbox checked, 0 if not, lParam is unused and always 0

		/// <summary>
		/// Sent by the Task Dialog when the user presses F1 on the keyboard while the dialog has focus.
		/// The value returned by the callback is ignored.
		/// </summary>
		Help = 9,

		/// <summary>
		/// Sent by the task dialog when the user clicks on the dialog's expando button.
		/// The expanded value is available on the TaskDialogNotificationArgs.
		/// The value returned by the callback is ignored.
		/// </summary>
		ExpandoButtonClicked = 10            // wParam = 0 (dialog is now collapsed), wParam != 0 (dialog is now expanded)
	}

	/// <summary>
	/// Progress bar state.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")] // Comes from CommCtrl.h PBST_* values which don't have a zero.
	public enum ProgressBarState {
		/// <summary>
		/// Normal.
		/// </summary>
		Normal = 1,

		/// <summary>
		/// Error state.
		/// </summary>
		Error = 2,

		/// <summary>
		/// Paused state.
		/// </summary>
		Paused = 3
	}


	/// <summary>
	/// A custom button for the TaskDialog.
	/// </summary>
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // Would be unused code as not required for usage.
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct TaskDialogButton {
		/// <summary>
		/// The ID of the button. This value is returned by TaskDialog.Show when the button is clicked.
		/// </summary>
		private int buttonId;

		/// <summary>
		/// The string that appears on the button.
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		private string buttonText;

		/// <summary>
		/// Initialize the custom button.
		/// </summary>
		/// <param name="id">The ID of the button. This value is returned by TaskDialog.Show when
		/// the button is clicked. Typically this will be a value in the DialogResult enum.</param>
		/// <param name="text">The string that appears on the button.</param>
		public TaskDialogButton(int id, string text) {
			this.buttonId = id;
			this.buttonText = text;
		}

		/// <summary>
		/// The ID of the button. This value is returned by TaskDialog.Show when the button is clicked.
		/// </summary>
		public int ButtonId {
			get {
				return this.buttonId;
			}
			set {
				this.buttonId = value;
			}
		}

		/// <summary>
		/// The string that appears on the button.
		/// </summary>
		public string ButtonText {
			get {
				return this.buttonText;
			}
			set {
				this.buttonText = value;
			}
		}
	}

	/// <summary>
	/// Arguments passed to the TaskDialog callback.
	/// </summary>
	public class TaskDialogNotificationArgs {
		/// <summary>
		/// What the TaskDialog callback is a notification of.
		/// </summary>
		private TaskDialogNotification notification;

		/// <summary>
		/// The button ID if the notification is about a button. This a DialogResult
		/// value or the ButtonID member of a TaskDialogButton set in the
		/// TaskDialog.Buttons or TaskDialog.RadioButtons members.
		/// </summary>
		private int buttonId;

		/// <summary>
		/// The HREF string of the hyperlink the notification is about.
		/// </summary>
		private string hyperlink;

		/// <summary>
		/// The number of milliseconds since the dialog was opened or the last time the
		/// callback for a timer notification reset the value by returning true.
		/// </summary>
		private uint timerTickCount;

		/// <summary>
		/// The state of the verification flag when the notification is about the verification flag.
		/// </summary>
		private bool verificationFlagChecked;

		/// <summary>
		/// The state of the dialog expando when the notification is about the expando.
		/// </summary>
		private bool expanded;

		/// <summary>
		/// What the TaskDialog callback is a notification of.
		/// </summary>
		public TaskDialogNotification Notification {
			get {
				return this.notification;
			}
			set {
				this.notification = value;
			}
		}

		/// <summary>
		/// The button ID if the notification is about a button. This a DialogResult
		/// value or the ButtonID member of a TaskDialogButton set in the
		/// TaskDialog.Buttons member.
		/// </summary>
		public int ButtonId {
			get {
				return this.buttonId;
			}
			set {
				this.buttonId = value;
			}
		}

		/// <summary>
		/// The HREF string of the hyperlink the notification is about.
		/// </summary>
		public string Hyperlink {
			get {
				return this.hyperlink;
			}
			set {
				this.hyperlink = value;
			}
		}

		/// <summary>
		/// The number of milliseconds since the dialog was opened or the last time the
		/// callback for a timer notification reset the value by returning true.
		/// </summary>
		public uint TimerTickCount {
			get {
				return this.timerTickCount;
			}
			set {
				this.timerTickCount = value;
			}
		}

		/// <summary>
		/// The state of the verification flag when the notification is about the verification flag.
		/// </summary>
		public bool VerificationFlagChecked {
			get {
				return this.verificationFlagChecked;
			}
			set {
				this.verificationFlagChecked = value;
			}
		}

		/// <summary>
		/// The state of the dialog expando when the notification is about the expando.
		/// </summary>
		public bool Expanded {
			get {
				return this.expanded;
			}
			set {
				this.expanded = value;
			}
		}
	}

	/// <summary>
	/// The active Task Dialog window. Provides several methods for acting on the active TaskDialog.
	/// You should not use this object after the TaskDialog Destroy notification callback. Doing so
	/// will result in undefined behavior and likely crash.
	/// </summary>
	public class ActiveTaskDialog : IWin32Window {

		#region Enums
		/// <summary>
		/// TASKDIALOG_ELEMENTS taken from CommCtrl.h
		/// </summary>
		private enum TASKDIALOG_ELEMENTS {
			/// <summary>
			/// The content element.
			/// </summary>
			TDE_CONTENT,

			/// <summary>
			/// Expanded Information.
			/// </summary>
			TDE_EXPANDED_INFORMATION,

			/// <summary>
			/// Footer.
			/// </summary>
			TDE_FOOTER,

			/// <summary>
			/// Main Instructions
			/// </summary>
			TDE_MAIN_INSTRUCTION
		}

		/// <summary>
		/// TASKDIALOG_ICON_ELEMENTS taken from CommCtrl.h
		/// </summary>
		private enum TASKDIALOG_ICON_ELEMENTS {
			/// <summary>
			/// Main instruction icon.
			/// </summary>
			TDIE_ICON_MAIN,

			/// <summary>
			/// Footer icon.
			/// </summary>
			TDIE_ICON_FOOTER
		}

		/// <summary>
		/// TASKDIALOG_MESSAGES taken from CommCtrl.h.
		/// </summary>
		private enum TASKDIALOG_MESSAGES : uint {
			// Spec is not clear on what this is for.
			///// <summary>
			///// Navigate page.
			///// </summary>
			////TDM_NAVIGATE_PAGE = WM_USER + 101,

			/// <summary>
			/// Click button.
			/// </summary>
			TDM_CLICK_BUTTON = WindowsMessages.WM_USER + 102, // wParam = Button ID

			/// <summary>
			/// Set Progress bar to be marquee mode.
			/// </summary>
			TDM_SET_MARQUEE_PROGRESS_BAR = WindowsMessages.WM_USER + 103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)

			/// <summary>
			/// Set Progress bar state.
			/// </summary>
			TDM_SET_PROGRESS_BAR_STATE = WindowsMessages.WM_USER + 104, // wParam = new progress state

			/// <summary>
			/// Set progress bar range.
			/// </summary>
			TDM_SET_PROGRESS_BAR_RANGE = WindowsMessages.WM_USER + 105, // lParam = MAKELPARAM(nMinRange, nMaxRange)

			/// <summary>
			/// Set progress bar position.
			/// </summary>
			TDM_SET_PROGRESS_BAR_POS = WindowsMessages.WM_USER + 106, // wParam = new position

			/// <summary>
			/// Set progress bar marquee (animation).
			/// </summary>
			TDM_SET_PROGRESS_BAR_MARQUEE = WindowsMessages.WM_USER + 107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)

			/// <summary>
			/// Set a text element of the Task Dialog.
			/// </summary>
			TDM_SET_ELEMENT_TEXT = WindowsMessages.WM_USER + 108, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)

			/// <summary>
			/// Click a radio button.
			/// </summary>
			TDM_CLICK_RADIO_BUTTON = WindowsMessages.WM_USER + 110, // wParam = Radio Button ID

			/// <summary>
			/// Enable or disable a button.
			/// </summary>
			TDM_ENABLE_BUTTON = WindowsMessages.WM_USER + 111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID

			/// <summary>
			/// Enable or disable a radio button.
			/// </summary>
			TDM_ENABLE_RADIO_BUTTON = WindowsMessages.WM_USER + 112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID

			/// <summary>
			/// Check or uncheck the verfication checkbox.
			/// </summary>
			TDM_CLICK_VERIFICATION = WindowsMessages.WM_USER + 113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)

			/// <summary>
			/// Update the text of an element (no effect if origially set as null).
			/// </summary>
			TDM_UPDATE_ELEMENT_TEXT = WindowsMessages.WM_USER + 114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)

			/// <summary>
			/// Designate whether a given Task Dialog button or command link should have a User Account Control (UAC) shield icon.
			/// </summary>
			TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = WindowsMessages.WM_USER + 115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)

			/// <summary>
			/// Refreshes the icon of the task dialog.
			/// </summary>
			TDM_UPDATE_ICON = WindowsMessages.WM_USER + 116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
		}
		#endregion

		/// <summary>
		/// The Task Dialog's window handle.
		/// </summary>
		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")] // We don't own the window.
		private IntPtr handle;

		/// <summary>
		/// Creates a ActiveTaskDialog.
		/// </summary>
		/// <param name="handle">The Task Dialog's window handle.</param>
		internal ActiveTaskDialog(IntPtr handle) {
			if (handle == IntPtr.Zero) {
				throw new ArgumentNullException("handle");
			}

			this.handle = handle;
		}

		/// <summary>
		/// The Task Dialog's window handle.
		/// </summary>
		public IntPtr Handle {
			get {
				return this.handle;
			}
		}

		//// Not supported. Task Dialog Spec does not indicate what this is for.
		////public void NavigatePage()
		////{
		////    // TDM_NAVIGATE_PAGE                   = WM_USER+101,
		////    UnsafeNativeMethods.SendMessage(
		////        this.windowHandle,
		////        (uint)UnsafeNativeMethods.TASKDIALOG_MESSAGES.TDM_NAVIGATE_PAGE,
		////        IntPtr.Zero,
		////        //a UnsafeNativeMethods.TASKDIALOGCONFIG value);
		////}

		/// <summary>
		/// Simulate the action of a button click in the TaskDialog. This can be a DialogResult value 
		/// or the ButtonID set on a TasDialogButton set on TaskDialog.Buttons.
		/// </summary>
		/// <param name="buttonId">Indicates the button ID to be selected.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool ClickButton(int buttonId) {
			// TDM_CLICK_BUTTON                    = WM_USER+102, // wParam = Button ID
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_CLICK_BUTTON,
				(IntPtr)buttonId,
				IntPtr.Zero) != IntPtr.Zero;
		}

		/// <summary>
		/// Used to indicate whether the hosted progress bar should be displayed in marquee mode or not.
		/// </summary>
		/// <param name="marquee">Specifies whether the progress bar sbould be shown in Marquee mode.
		/// A value of true turns on Marquee mode.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetMarqueeProgressBar(bool marquee) {
			// TDM_SET_MARQUEE_PROGRESS_BAR        = WM_USER+103, // wParam = 0 (nonMarque) wParam != 0 (Marquee)
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_MARQUEE_PROGRESS_BAR,
				(marquee ? (IntPtr)1 : IntPtr.Zero),
				IntPtr.Zero) != IntPtr.Zero;

			// Future: get more detailed error from and throw.
		}

		/// <summary>
		/// Sets the state of the progress bar.
		/// </summary>
		/// <param name="newState">The state to set the progress bar.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetProgressBarState(ProgressBarState newState) {
			// TDM_SET_PROGRESS_BAR_STATE          = WM_USER+104, // wParam = new progress state
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_STATE,
				(IntPtr)newState,
				IntPtr.Zero) != IntPtr.Zero;

			// Future: get more detailed error from and throw.
		}

		/// <summary>
		/// Set the minimum and maximum values for the hosted progress bar.
		/// </summary>
		/// <param name="minRange">Minimum range value. By default, the minimum value is zero.</param>
		/// <param name="maxRange">Maximum range value.  By default, the maximum value is 100.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetProgressBarRange(Int16 minRange, Int16 maxRange) {
			// TDM_SET_PROGRESS_BAR_RANGE          = WM_USER+105, // lParam = MAKELPARAM(nMinRange, nMaxRange)
			// #define MAKELPARAM(l, h)      ((LPARAM)(DWORD)MAKELONG(l, h))
			// #define MAKELONG(a, b)      ((LONG)(((WORD)(((DWORD_PTR)(a)) & 0xffff)) | ((DWORD)((WORD)(((DWORD_PTR)(b)) & 0xffff))) << 16))
			IntPtr lparam = (IntPtr)((((Int32)minRange) & 0xffff) | ((((Int32)maxRange) & 0xffff) << 16));
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_RANGE,
				IntPtr.Zero,
				lparam) != IntPtr.Zero;

			// Return value is actually prior range.
		}

		/// <summary>
		/// Set the current position for a progress bar.
		/// </summary>
		/// <param name="newPosition">The new position.</param>
		/// <returns>Returns the previous value if successful, or zero otherwise.</returns>
		public int SetProgressBarPosition(int newPosition) {
			// TDM_SET_PROGRESS_BAR_POS            = WM_USER+106, // wParam = new position
			return (int)User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_POS,
				(IntPtr)newPosition,
				IntPtr.Zero);
		}

		/// <summary>
		/// Sets the animation state of the Marquee Progress Bar.
		/// </summary>
		/// <param name="startMarquee">true starts the marquee animation and false stops it.</param>
		/// <param name="speed">The time in milliseconds between refreshes.</param>
		public void SetProgressBarMarquee(bool startMarquee, uint speed) {
			// TDM_SET_PROGRESS_BAR_MARQUEE        = WM_USER+107, // wParam = 0 (stop marquee), wParam != 0 (start marquee), lparam = speed (milliseconds between repaints)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_MARQUEE,
				(startMarquee ? new IntPtr(1) : IntPtr.Zero),
				(IntPtr)speed);
		}

		/// <summary>
		/// Updates the content text.
		/// </summary>
		/// <param name="content">The new value.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetContent(string content) {
			// TDE_CONTENT,
			// TDM_SET_ELEMENT_TEXT                = WM_USER+108  // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_CONTENT,
				content) != IntPtr.Zero;
		}

		/// <summary>
		/// Updates the Expanded Information text.
		/// </summary>
		/// <param name="expandedInformation">The new value.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetExpandedInformation(string expandedInformation) {
			// TDE_EXPANDED_INFORMATION,
			// TDM_SET_ELEMENT_TEXT                = WM_USER+108  // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_EXPANDED_INFORMATION,
				expandedInformation) != IntPtr.Zero;
		}

		/// <summary>
		/// Updates the Footer text.
		/// </summary>
		/// <param name="footer">The new value.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetFooter(string footer) {
			// TDE_FOOTER,
			// TDM_SET_ELEMENT_TEXT                = WM_USER+108  // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_FOOTER,
				footer) != IntPtr.Zero;
		}

		/// <summary>
		/// Updates the Main Instruction.
		/// </summary>
		/// <param name="mainInstruction">The new value.</param>
		/// <returns>If the function succeeds the return value is true.</returns>
		public bool SetMainInstruction(string mainInstruction) {
			// TDE_MAIN_INSTRUCTION
			// TDM_SET_ELEMENT_TEXT                = WM_USER+108  // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			return User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_MAIN_INSTRUCTION,
				mainInstruction) != IntPtr.Zero;
		}

		/// <summary>
		/// Simulate the action of a radio button click in the TaskDialog. 
		/// The passed buttonID is the ButtonID set on a TaskDialogButton set on TaskDialog.RadioButtons.
		/// </summary>
		/// <param name="buttonId">Indicates the button ID to be selected.</param>
		public void ClickRadioButton(int buttonId) {
			// TDM_CLICK_RADIO_BUTTON = WM_USER+110, // wParam = Radio Button ID
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_CLICK_RADIO_BUTTON,
				(IntPtr)buttonId,
				IntPtr.Zero);
		}

		/// <summary>
		/// Enable or disable a button in the TaskDialog. 
		/// The passed buttonID is the ButtonID set on a TaskDialogButton set on TaskDialog.Buttons
		/// or a common button ID.
		/// </summary>
		/// <param name="buttonId">Indicates the button ID to be enabled or diabled.</param>
		/// <param name="enable">Enambe the button if true. Disable the button if false.</param>
		public void EnableButton(int buttonId, bool enable) {
			// TDM_ENABLE_BUTTON = WM_USER+111, // lParam = 0 (disable), lParam != 0 (enable), wParam = Button ID
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_ENABLE_BUTTON,
				(IntPtr)buttonId,
				(IntPtr)(enable ? 0 : 1));
		}

		/// <summary>
		/// Enable or disable a radio button in the TaskDialog. 
		/// The passed buttonID is the ButtonID set on a TaskDialogButton set on TaskDialog.RadioButtons.
		/// </summary>
		/// <param name="buttonId">Indicates the button ID to be enabled or diabled.</param>
		/// <param name="enable">Enambe the button if true. Disable the button if false.</param>
		public void EnableRadioButton(int buttonId, bool enable) {
			// TDM_ENABLE_RADIO_BUTTON = WM_USER+112, // lParam = 0 (disable), lParam != 0 (enable), wParam = Radio Button ID
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_ENABLE_RADIO_BUTTON,
				(IntPtr)buttonId,
				(IntPtr)(enable ? 0 : 1));
		}

		/// <summary>
		/// Check or uncheck the verification checkbox in the TaskDialog. 
		/// </summary>
		/// <param name="checkedState">The checked state to set the verification checkbox.</param>
		/// <param name="setKeyboardFocusToCheckBox">True to set the keyboard focus to the checkbox, and fasle otherwise.</param>
		public void ClickVerification(bool checkedState, bool setKeyboardFocusToCheckBox) {
			// TDM_CLICK_VERIFICATION = WM_USER+113, // wParam = 0 (unchecked), 1 (checked), lParam = 1 (set key focus)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_CLICK_VERIFICATION,
				(checkedState ? new IntPtr(1) : IntPtr.Zero),
				(setKeyboardFocusToCheckBox ? new IntPtr(1) : IntPtr.Zero));
		}

		/// <summary>
		/// Updates the content text.
		/// </summary>
		/// <param name="content">The new value.</param>
		public void UpdateContent(string content) {
			// TDE_CONTENT,
			// TDM_UPDATE_ELEMENT_TEXT             = WM_USER+114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_CONTENT,
				content);
		}

		/// <summary>
		/// Updates the Expanded Information text. No effect if it was previously set to null.
		/// </summary>
		/// <param name="expandedInformation">The new value.</param>
		public void UpdateExpandedInformation(string expandedInformation) {
			// TDE_EXPANDED_INFORMATION,
			// TDM_UPDATE_ELEMENT_TEXT             = WM_USER+114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_EXPANDED_INFORMATION,
				expandedInformation);
		}

		/// <summary>
		/// Updates the Footer text. No Effect if it was perviously set to null.
		/// </summary>
		/// <param name="footer">The new value.</param>
		public void UpdateFooter(string footer) {
			// TDE_FOOTER,
			// TDM_UPDATE_ELEMENT_TEXT             = WM_USER+114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_FOOTER,
				footer);
		}

		/// <summary>
		/// Updates the Main Instruction.
		/// </summary>
		/// <param name="mainInstruction">The new value.</param>
		public void UpdateMainInstruction(string mainInstruction) {
			// TDE_MAIN_INSTRUCTION
			// TDM_UPDATE_ELEMENT_TEXT             = WM_USER+114, // wParam = element (TASKDIALOG_ELEMENTS), lParam = new element text (LPCWSTR)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ELEMENT_TEXT,
				(IntPtr)TASKDIALOG_ELEMENTS.TDE_MAIN_INSTRUCTION,
				mainInstruction);
		}

		/// <summary>
		/// Designate whether a given Task Dialog button or command link should have a User Account Control (UAC) shield icon.
		/// </summary>
		/// <param name="buttonId">ID of the push button or command link to be updated.</param>
		/// <param name="elevationRequired">False to designate that the action invoked by the button does not require elevation;
		/// true to designate that the action does require elevation.</param>
		public void SetButtonElevationRequiredState(int buttonId, bool elevationRequired) {
			// TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE = WM_USER+115, // wParam = Button ID, lParam = 0 (elevation not required), lParam != 0 (elevation required)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE,
				(IntPtr)buttonId,
				(IntPtr)(elevationRequired ? new IntPtr(1) : IntPtr.Zero));
		}

		/// <summary>
		/// Updates the main instruction icon. Note the type (standard via enum or
		/// custom via Icon type) must be used when upating the icon.
		/// </summary>
		/// <param name="icon">Task Dialog standard icon.</param>
		public void UpdateMainIcon(TaskDialogIcon icon) {
			// TDM_UPDATE_ICON = WM_USER+116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
				(IntPtr)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_MAIN,
				(IntPtr)icon);
		}

		/// <summary>
		/// Updates the main instruction icon. Note the type (standard via enum or
		/// custom via Icon type) must be used when upating the icon.
		/// </summary>
		/// <param name="icon">The icon to set.</param>
		public void UpdateMainIcon(Icon icon) {
			// TDM_UPDATE_ICON = WM_USER+116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
				(IntPtr)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_MAIN,
				(icon == null ? IntPtr.Zero : icon.Handle));
		}

		/// <summary>
		/// Updates the footer icon. Note the type (standard via enum or
		/// custom via Icon type) must be used when upating the icon.
		/// </summary>
		/// <param name="icon">Task Dialog standard icon.</param>
		public void UpdateFooterIcon(TaskDialogIcon icon) {
			// TDM_UPDATE_ICON = WM_USER+116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
				(IntPtr)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_FOOTER,
				(IntPtr)icon);
		}

		/// <summary>
		/// Updates the footer icon. Note the type (standard via enum or
		/// custom via Icon type) must be used when upating the icon.
		/// </summary>
		/// <param name="icon">The icon to set.</param>
		public void UpdateFooterIcon(Icon icon) {
			// TDM_UPDATE_ICON = WM_USER+116  // wParam = icon element (TASKDIALOG_ICON_ELEMENTS), lParam = new icon (hIcon if TDF_USE_HICON_* was set, PCWSTR otherwise)
			User32.SendMessage(
				this.handle,
				(uint)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON,
				(IntPtr)TASKDIALOG_ICON_ELEMENTS.TDIE_ICON_FOOTER,
				(icon == null ? IntPtr.Zero : icon.Handle));
		}
	}

}
