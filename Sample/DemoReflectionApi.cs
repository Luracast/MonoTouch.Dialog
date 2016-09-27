//
// Sample showing the core Element-based API to create a dialog
//

#pragma warning disable 414 // The private field 'X' is assigned but its value is never used
#pragma warning disable 169 // The private field 'X' is never used

using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using ObjCRuntime;
using System.Threading.Tasks;
#if __UNIFIED__
using UIKit;
using Foundation;
using NSAction = global::System.Action;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Sample
{
	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.

	[Preserve(AllMembers = true)]
	class Settings
	{

		[Cell("CustomCell")]
		public float Cost = 22f;
		[Element(typeof(CustomElement<float>), "{0:C} SGD")]
		public float TotalCost = 322f;

		[OnAdd("OnAdd")]
		[Cell("CustomCell")]
		public float[] Prices = new float[] { 343f, 34212f };

		[Section]
		public bool AccountEnabled;
		[Skip]
		public bool Hidden;

		[Section("Account", "Your credentials")]

		[Entry("Enter your login name")]
		public string Login;

		[Password("Enter your password")]
		public string Password;

		[Section("Autocapitalize, autocorrect and clear button")]

		[Entry(Placeholder = "Enter your name", AutocorrectionType = UITextAutocorrectionType.Yes, AutocapitalizationType = UITextAutocapitalizationType.Words, ClearButtonMode = UITextFieldViewMode.WhileEditing)]
		public string Name;

		[Section("Time Editing")]

		public TimeSettings TimeSamples;

		[Section("Enumerations")]

		[Caption("Favorite CLR type")]
		public TypeCode FavoriteType;

		[Section("Checkboxes")]
		[Checkbox]
		bool English = true;

		[Checkbox]
		bool Spanish;

		[Section("Image Selection")]
		public UIImage Top;
		public UIImage Middle;
		public UIImage Bottom;

		[Section("Multiline")]
		[Caption("This is a\nmultiline string\nall you need is the\n[Multiline] attribute")]
		[Multiline]
		public string multi;

		[Section("IEnumerable")]
		[RadioSelection("ListOfString")]
		public int selected = 1;
		public IList<string> ListOfString;

		public static Task<float> OnAdd()
		{
			return GetFloat();
		}

		public static Task<float> GetFloat()
		{
			var tcs = new TaskCompletionSource<float>();

			UIApplication.SharedApplication.InvokeOnMainThread(
				new NSAction(() =>
				{
					UIAlertView alert = new UIAlertView(
						"Add the new price",
						"",
						null,
						"Cancel",
						"OK"
					);
					alert.Clicked += (sender, buttonArgs) =>
					{
						if (buttonArgs.ButtonIndex != alert.CancelButtonIndex)
							tcs.SetResult(28372f);
						else
							tcs.SetCanceled();
					};
					alert.Show();
				})
			);

			return tcs.Task;
		}
	}

	public class TimeSettings
	{
		public DateTime Appointment;

		[Date]
		public DateTime Birthday;

		[Time]
		public DateTime Alarm;
	}

	public partial class AppDelegate
	{
		Settings settings;

		public void DemoReflectionApi()
		{
			if (settings == null)
			{
				var image = UIImage.FromFile("monodevelop-32.png");

				settings = new Settings()
				{
					AccountEnabled = true,
					Login = "postmater@localhost.com",
					TimeSamples = new TimeSettings()
					{
						Appointment = DateTime.Now,
						Birthday = new DateTime(1980, 6, 24),
						Alarm = new DateTime(2000, 1, 1, 7, 30, 0, 0)
					},
					FavoriteType = TypeCode.Int32,
					Top = image,
					Middle = image,
					Bottom = image,
					ListOfString = new List<string>() { "One", "Two", "Three" }
				};
			}
			var bc = new BindingContext(settings, settings, "Settings");

			var dv = new DialogViewController(bc.Root, true);

			//dv.Editing = true;
			//dv.SetEditing(true, true);

			dv.TableView.Source = new TableSource(dv);


			// When the view goes out of screen, we fetch the data.
			dv.ViewDisappearing += delegate
			{
				// This reflects the data back to the object instance
				bc.Fetch();

				// Manly way of dumping the data.
				Console.WriteLine("Current status:");
				Console.WriteLine(
					"AccountEnabled:  {0}\n" +
					"Login:           {1}\n" +
					"Password:        {2}\n" +
					"Name:      	  {3}\n" +
					"Appointment:     {4}\n" +
					"Birthday:        {5}\n" +
					"Alarm:           {6}\n" +
					"Favorite Type:   {7}\n" +
					"IEnumerable idx: {8}",
					settings.AccountEnabled, settings.Login, settings.Password, settings.Name,
					settings.TimeSamples.Appointment, settings.TimeSamples.Birthday,
					settings.TimeSamples.Alarm, settings.FavoriteType,
					settings.selected);
			};
			navigation.PushViewController(dv, true);
		}
	}

	public class CustomElement<T> : Element, IProvideValue<T>, IElementSizing
		where T : struct
	{

		public T Value
		{
			get;
			set;

		}

		public string ValueString
		{
			get
			{
				return String.IsNullOrEmpty(Format)
							? Value.ToString()
							: String.Format(Format, Value);
			}

		}

		public string Format = null;

		public event NSAction Tapped;

		public UITextAlignment Alignment = UITextAlignment.Left;


		public CustomElement(string caption, T value) : base(caption)
		{
			Value = value;
		}
		public CustomElement(string caption, T value, string format) : this(caption, value)
		{
			Format = format;
		}


		public override string Summary()
		{
			return String.Format(Format, Value);
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			CustomCell cell = (CustomCell)tv.DequeueReusableCell(CustomCell.Key);
			if (cell == null)
			{
				var arr = NSBundle.MainBundle.LoadNib(CustomCell.Key, null, null);
				cell = Runtime.GetNSObject<CustomCell>(arr.ValueAt(0));
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
			}

			cell.Update(Caption, ValueString);

			return cell;
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped();
			tableView.DeselectRow(indexPath, true);
		}

		public override bool Matches(string text)
		{
			return ValueString.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1;
		}

		public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
		{
			//var cell = tableView.CellAt(indexPath);
			return 60;
		}
	}
}