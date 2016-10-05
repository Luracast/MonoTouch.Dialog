//
// Sample showing the core Element-based API to create a dialog
//

#pragma warning disable 414 // The private field 'X' is assigned but its value is never used
#pragma warning disable 169 // The private field 'X' is never used

using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

namespace Sample
{
	// Use the preserve attribute to inform the linker that even if I do not
	// use the fields, to not try to optimize them away.


	[Preserve(AllMembers = true)]
	class OrderForm
	{

		[Section("Order Form")]
		public string RefNo = String.Empty;
		[Date]
		public DateTime DateCreated = DateTime.Now;

		[Date]
		public DateTime? DueDate = null;

		[Entry]
		public string Remarks = "";

	}

	[Preserve(AllMembers = true)]
	class PaymentForm
	{
		[Date]
		public DateTime Date { get; set; } = DateTime.Now;

		public string[] PaymentType = new string[] { "cash", "visa", "master" };

		[Skip]
		public string Type { get; set; } = "Cash";

		[Alignment(UITextAlignment.Right)]
		[Entry(Placeholder = "0.00", KeyboardType = UIKeyboardType.DecimalPad, ClearButtonMode = UITextFieldViewMode.WhileEditing)]
		public string Cash
		{
			get
			{
				return Amount == 0f ? "" : Amount.ToString();
			}
			set
			{
				float.TryParse(value, out Amount);
			}
		}

		[Skip]
		public float Amount = 0f;

		[RadioSelection("PaymentType")]
		public int paymentTypeIndex
		{
			get
			{
				return Array.IndexOf(PaymentType, Type);
			}
			set
			{
				Type = PaymentType[value];
			}
		}

		public void Save()
		{
			throw new NotImplementedException();
		}
	}

	[Preserve(AllMembers = true)]
	class Settings
	{
		[Section]
		public PaymentForm Payment = new PaymentForm();
		public OrderForm order = new OrderForm();
		public bool AccountEnabled;
		[Skip]
		public bool Hidden;

		[Section("Account", "Your credentials")]

		[Entry("Enter your login name")]
		public string Login;

		[Password("Enter your password")]
		public string Password;

		[Section("Autocapitalize, autocorrect and clear button")]

	
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
	}

	public class TimeSettings
	{
		public DateTime Appointment;

		[Date]
		public DateTime Birthday;

		[Time]
		public DateTime Alarm;

		[Date]
		public DateTime? Expiry;
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
			var bc = new Binder(settings, settings, "Settings");

			var dv = new DialogViewController(bc.Root, true);

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
}