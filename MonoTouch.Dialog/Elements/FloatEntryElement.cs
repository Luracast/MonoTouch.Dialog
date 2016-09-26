
#pragma warning disable 414 // The private field 'X' is assigned but its value is never used
#pragma warning disable 169 // The private field 'X' is never used

using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Globalization;
using System.Drawing;
using System.Linq;

#if __UNIFIED__
using UIKit;
using CoreGraphics;
using Foundation;
using CoreAnimation;

using NSAction = global::System.Action;
#else
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
#endif

#if !__UNIFIED__
using nint = global::System.Int32;
using nuint = global::System.UInt32;
using nfloat = global::System.Single;

using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;
using CGRect = global::System.Drawing.RectangleF;
#endif

using MonoTouch.Dialog.Utilities;

namespace MonoTouch.Dialog
{

	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class FloatEntryElement : Element
	{
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		public float Value
		{
			get
			{
				if (entry == null)
					return val;
				var newValue = float.Parse(entry.Text, CultureInfo.InvariantCulture);
				if (newValue.Equals(val))
					return val;
				val = newValue;

				if (Changed != null)
					Changed(this, EventArgs.Empty);
				return val;
			}
			set
			{
				val = value;
				if (entry != null)
					entry.Text = value.ToString();
			}
		}
		protected float val;

		/// <summary>
		/// The key used for reusable UITableViewCells.
		/// </summary>
		static NSString entryKey = new NSString("FloatEntryElement");
		protected virtual NSString EntryKey
		{
			get
			{
				return entryKey;
			}
		}

		/// <summary>
		/// The type of keyboard used for input, you can change
		/// this to use this for numeric input, email addressed,
		/// urls, phones.
		/// </summary>
		public UIKeyboardType KeyboardType
		{
			get
			{
				return keyboardType;
			}
			set
			{
				keyboardType = value;
				if (entry != null)
					entry.KeyboardType = value;
			}
		}

		/// <summary>
		/// The type of Return Key that is displayed on the
		/// keyboard, you can change this to use this for
		/// Done, Return, Save, etc. keys on the keyboard
		/// </summary>
		public UIReturnKeyType? ReturnKeyType
		{
			get
			{
				return returnKeyType;
			}
			set
			{
				returnKeyType = value;
				if (entry != null && returnKeyType.HasValue)
					entry.ReturnKeyType = returnKeyType.Value;
			}
		}

		/// <summary>
		/// The default value for this property is <c>false</c>. If you set it to <c>true</c>, the keyboard disables the return key when the text entry area contains no text. As soon as the user enters any text, the return key is automatically enabled.
		/// </summary>
		public bool EnablesReturnKeyAutomatically
		{
			get
			{
				return enablesReturnKeyAutomatically;
			}
			set
			{
				enablesReturnKeyAutomatically = value;
				if (entry != null)
					entry.EnablesReturnKeyAutomatically = value;
			}
		}

		public UITextAutocapitalizationType AutocapitalizationType
		{
			get
			{
				return autocapitalizationType;
			}
			set
			{
				autocapitalizationType = value;
				if (entry != null)
					entry.AutocapitalizationType = value;
			}
		}

		public UITextAutocorrectionType AutocorrectionType
		{
			get
			{
				return autocorrectionType;
			}
			set
			{
				autocorrectionType = value;
				if (entry != null)
					this.autocorrectionType = value;
			}
		}

		public UITextFieldViewMode ClearButtonMode
		{
			get
			{
				return clearButtonMode;
			}
			set
			{
				clearButtonMode = value;
				if (entry != null)
					entry.ClearButtonMode = value;
			}
		}

		public UITextAlignment TextAlignment
		{
			get
			{
				return textalignment;
			}
			set
			{
				textalignment = value;
				if (entry != null)
				{
					entry.TextAlignment = textalignment;
				}
			}
		}

		public bool AlignEntryWithAllSections { get; set; }

		public bool NotifyChangedOnKeyStroke { get; set; }

		UITextAlignment textalignment = UITextAlignment.Left;
		UIKeyboardType keyboardType = UIKeyboardType.Default;
		UIReturnKeyType? returnKeyType = null;
		bool enablesReturnKeyAutomatically = false;
		UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.Sentences;
		UITextAutocorrectionType autocorrectionType = UITextAutocorrectionType.Default;
		UITextFieldViewMode clearButtonMode = UITextFieldViewMode.Never;
		bool isPassword, becomeResponder;
		UITextField entry;
		string placeholder;
		static UIFont font = UIFont.BoldSystemFontOfSize(17);

		public event EventHandler Changed;
		public event Func<bool> ShouldReturn;
		public EventHandler EntryStarted { get; set; }
		public EventHandler EntryEnded { get; set; }
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public FloatEntryElement(string caption, float value) : base(caption)
		{
			Value = value;
			this.placeholder = "";
			KeyboardType = UIKeyboardType.DecimalPad;
			TextAlignment = UITextAlignment.Right;
		}

		public FloatEntryElement(string caption, float value, string placeholder) : base(caption)
		{
			Value = value;
			this.placeholder = placeholder;
			KeyboardType = UIKeyboardType.DecimalPad;
			TextAlignment = UITextAlignment.Right;
		}

		/// <summary>
		/// Constructs an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use.
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		/// <param name="isPassword">
		/// True if this should be used to enter a password.
		/// </param>
		public FloatEntryElement(string caption, float value, string placeholder, bool isPassword) : base(caption)
		{
			Value = value;
			this.isPassword = isPassword;
			this.placeholder = placeholder;
		}

		public override string Summary()
		{
			return Value.ToString();
		}

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		CGSize ComputeEntryPosition(UITableView tv, UITableViewCell cell)
		{
			nfloat maxWidth = -15; // If all EntryElements have a null Caption, align UITextField with the Caption offset of normal cells (at 10px).
			nfloat maxHeight = font.LineHeight;

			// Determine if we should calculate accross all sections or just the current section.
			var sections = AlignEntryWithAllSections ? (Parent.Parent as RootElement).Sections : (new[] { Parent as Section }).AsEnumerable();

			foreach (Section s in sections)
			{

				foreach (var e in s.Elements)
				{

					var ee = e as EntryElement;

					if (ee != null
						&& !String.IsNullOrEmpty(ee.Caption))
					{

						var size = ee.Caption.StringSize(font);

						maxWidth = (nfloat)Math.Max(size.Width, maxWidth);
						maxHeight = (nfloat)Math.Max(size.Height, maxHeight);
					}
				}
			}

			return new CGSize(25 + (nfloat)Math.Min(maxWidth, 160), maxHeight);
		}

		protected virtual UITextField CreateTextField(CGRect frame)
		{
			return new UITextField(frame)
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
				Placeholder = placeholder ?? "",
				SecureTextEntry = isPassword,
				Text = Value > 0 ? Value.ToString() : "0.00",
				Tag = 1,
				TextAlignment = textalignment,
				ClearButtonMode = ClearButtonMode
			};
		}

		static readonly NSString passwordKey = new NSString("FloatEntryElement+Password");
		static readonly NSString cellkey = new NSString("FLoatEntryElement");

		protected override NSString CellKey
		{
			get
			{
				return isPassword ? passwordKey : cellkey;
			}
		}

		UITableViewCell cell;
		public override UITableViewCell GetCell(UITableView tv)
		{
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.TextLabel.Font = font;

			}
			cell.TextLabel.Text = Caption;

			var offset = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 20 : 90;
			cell.Frame = new CGRect(cell.Frame.X, cell.Frame.Y, tv.Frame.Width - offset, cell.Frame.Height);
			CGSize size = ComputeEntryPosition(tv, cell);
			nfloat yOffset = (cell.ContentView.Bounds.Height - size.Height) / 2 - 1;
			nfloat width = cell.ContentView.Bounds.Width - size.Width;
			if (textalignment == UITextAlignment.Right)
			{
				// Add padding if right aligned
				width -= 10;
			}
#if __TVOS__
			var entryFrame = new CGRect (size.Width, yOffset, width, size.Height + 20 /* FIXME: figure out something better than adding a magic number */);
#else
			var entryFrame = new CGRect(size.Width, yOffset, width, size.Height);
#endif

			if (entry == null)
			{
				entry = CreateTextField(entryFrame);
				entry.EditingChanged += delegate
				{
					if (NotifyChangedOnKeyStroke)
					{
						FetchValue();
					}
				};
				entry.ValueChanged += delegate
				{
					FetchValue();
				};
				entry.Ended += delegate
				{
					FetchValue();
					if (EntryEnded != null)
					{
						EntryEnded(this, null);
					}
				};
				entry.ShouldReturn += delegate
				{

					if (ShouldReturn != null)
						return ShouldReturn();

					RootElement root = GetImmediateRootElement();
					FloatEntryElement focus = null;

					if (root == null)
						return true;

					foreach (var s in root.Sections)
					{
						foreach (var e in s.Elements)
						{
							if (e == this)
							{
								focus = this;
							}
							else if (focus != null && e is EntryElement)
							{
								focus = e as FloatEntryElement;
								break;
							}
						}

						if (focus != null && focus != this)
							break;
					}

					if (focus != this)
						focus.BecomeFirstResponder(true);
					else
						focus.ResignFirstResponder(true);

					return true;
				};
				entry.Started += delegate
				{
					FloatEntryElement self = null;

					if (EntryStarted != null)
					{
						EntryStarted(this, null);
					}

					if (!returnKeyType.HasValue)
					{
						var returnType = UIReturnKeyType.Default;

						foreach (var e in (Parent as Section).Elements)
						{
							if (e == this)
								self = this;
							else if (self != null && e is FloatEntryElement)
								returnType = UIReturnKeyType.Next;
						}
						entry.ReturnKeyType = returnType;
					}
					else
						entry.ReturnKeyType = returnKeyType.Value;

					tv.ScrollToRow(IndexPath, UITableViewScrollPosition.Middle, true);
				};
				cell.ContentView.AddSubview(entry);
			}
			else
				entry.Frame = entryFrame;

			if (becomeResponder)
			{
				entry.BecomeFirstResponder();
				becomeResponder = false;
			}
			entry.KeyboardType = KeyboardType;
			entry.EnablesReturnKeyAutomatically = EnablesReturnKeyAutomatically;
			entry.AutocapitalizationType = AutocapitalizationType;
			entry.AutocorrectionType = AutocorrectionType;

			return cell;
		}

		/// <summary>
		///  Copies the value from the UITextField in the EntryElement to the
		//   Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue()
		{
			if (entry == null)
				return;

			var newValue = float.Parse(entry.Text, CultureInfo.InvariantCulture);
			if (newValue.Equals(Value))
				return;

			Value = newValue;

			if (Changed != null)
				Changed(this, EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (entry != null)
				{
					entry.Dispose();
					entry = null;
				}
			}
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
			tableView.DeselectRow(indexPath, true);
		}

		public override bool Matches(string text)
		{
			return Value.ToString().IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1 || base.Matches(text);
		}

		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public virtual void BecomeFirstResponder(bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView();
			if (tv == null)
				return;
			tv.ScrollToRow(IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
			{
				entry.BecomeFirstResponder();
				becomeResponder = false;
			}
		}

		public virtual void ResignFirstResponder(bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView();
			if (tv == null)
				return;
			tv.ScrollToRow(IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
				entry.ResignFirstResponder();
		}
	}

	public interface IProvideValue<T>
	{
		T Value { get; set; }
	}

	public class InnerSection<T> : Section, IProvideValue<T[]>
	{
		public T[] Value
		{
			get
			{
				var arr = new T[Elements.Count];
				var i = 0;
				Console.WriteLine(this.Count);
				foreach (var e in Elements)
				{
					if (e is IProvideValue<T>)
					{
						var p = e as IProvideValue<T>;
						arr[i] = p.Value;
					}
					i++;
				}
				return arr;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public InnerSection(string caption) : base(caption) 
		{
			FooterView = new UIView();
			FooterView.Bounds = new CGRect(0, 0, 320, 60);
			var button = UIButton.FromType(UIButtonType.RoundedRect);
			button.TintColor = UIColor.White;
			button.Layer.CornerRadius = (nfloat).3;
			button.BackgroundColor = UIColor.Green;
			button.TouchUpInside += delegate {
				Add(new GenericElement<float>((Count + 1).ToString(), 22f));
			};
			button.SetTitle("Add", UIControlState.Normal);
			button.Frame = new CGRect(12, 4, 100, 35);
			FooterView.Add(button);
		}

		public InnerSection(string caption, string footer) : base(caption, footer) { }

		public InnerSection(UIView header) : base(header) { }

		public InnerSection(UIView header, UIView footer) : base(header, footer) { }

	}


	public class GenericElement<T> : Element, IProvideValue<T>
		where T : struct
	{
		public readonly NSString CellIdentifier;

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


		public GenericElement(string caption, T value) : base(caption)
		{
			/* increment 1 for testing purpose
			if (value is float)
			{
				float i = (float)(object)value;
				i++;
				value = (T)(object)i;
			}
			*/
			Value = value;
			CellIdentifier = new NSString(this.GetType().Name + value.GetType().Name);
		}
		public GenericElement(string caption, T value, string format) : this(caption, value) 
		{
			Format = format;
		}


		public override string Summary()
		{
			return String.Format(Format, Value);
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(CellIdentifier);
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, CellIdentifier);
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;

			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = ValueString;

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
	}

	public class TableSource : DialogViewController.Source
	{
		#region Constructors
		public TableSource(DialogViewController dvc) : base(dvc) { }

		#endregion

		#region Override Methods
		/*
		public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
		{
			Console.WriteLine(indexPath.Section);

			UITableViewRowAction hiButton = UITableViewRowAction.Create(
			UITableViewRowActionStyle.Normal,
			"Edit",
			delegate
			{
				Console.WriteLine("Edit World!");
			});
			UITableViewRowAction niButton = UITableViewRowAction.Create(
				UITableViewRowActionStyle.Destructive,
				"Delete",
				delegate
				{
					Console.WriteLine("Delete World!");
				});
			return new UITableViewRowAction[] { hiButton, niButton };
		}
		*/

		public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
		{
			//var s = Container.Root.Sections[(int)indexPath.Section];
			//return s is InnerSection<>;
			return indexPath.Section == 1;
		}

		public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			var s = Container.Root.Sections[(int)indexPath.Section];
			s.Remove(indexPath.Row);
		}
		#endregion
	}
}