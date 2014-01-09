using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace GreenshotPlugin.Core {
	public class BrowserHelper {

		public static IEnumerable<string> GetBrowserUrls() {
			// FireFox
			foreach (WindowDetails window in WindowDetails.GetAllWindows("MozillaWindowClass")) {
				if (window.Text.Length == 0) {
					continue;
				}
				AutomationElement currentElement = AutomationElement.FromHandle(window.Handle);
				Condition conditionCustom = new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom), new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
				for (int i = 5; i > 0 && currentElement != null; i--) {
					currentElement = currentElement.FindFirst(TreeScope.Children, conditionCustom);
				}
				if (currentElement == null) {
					continue;
				}

				Condition conditionDocument = new AndCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document), new PropertyCondition(AutomationElement.IsOffscreenProperty, false));
				AutomationElement docElement = currentElement.FindFirst(TreeScope.Children, conditionDocument);
				if (docElement == null) {
					continue;
				}
				foreach (AutomationPattern pattern in docElement.GetSupportedPatterns()) {
					if (pattern.ProgrammaticName != "ValuePatternIdentifiers.Pattern") {
						continue;
					}
					string url = (docElement.GetCurrentPattern(pattern) as ValuePattern).Current.Value.ToString();
					if (!string.IsNullOrEmpty(url)) {
						yield return url;
						break;
					}
				}
			}

			foreach (WindowDetails window in WindowDetails.GetAllWindows("MozillaWindowClass")) {
				if (window.Text.Length == 0) {
					continue;
				}
				AutomationElement currentElement = AutomationElement.FromHandle(window.Handle);
			}
			foreach (string url in IEHelper.GetIEUrls()) {
				yield return url;
			}

		}
	}
}
