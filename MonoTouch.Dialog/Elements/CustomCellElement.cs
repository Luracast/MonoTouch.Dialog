using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

#if __UNIFIED__
using UIKit;
using Foundation;
using ObjCRuntime;

using NSAction = global::System.Action;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif

namespace MonoTouch.Dialog
{
	#region Reflection
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class CellAttribute : Attribute
	{
		public CellAttribute(string identifier, params object[] parameters)
		{
			Identifier = new NSString(identifier);
			Parameters = parameters;
		}
		public NSString Identifier;
		public object[] Parameters;
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class OnAddAttribute : Attribute
	{
		public OnAddAttribute(string method)
		{
			Method = method;
		}
		public string Method;
	}
	#endregion

	#region Interfaces

	public interface IUpdate<T>
	{
		void Update(string caption, T value, CustomCellElement<T> element);
	}

	public interface IProvideValue<T>
	{
		T Value { get; set; }
	}

	#endregion

	#region Elements

	public class CustomCellElement<T> : Element, IProvideValue<T>, IElementSizing
	{
		float height = 40;
		float expandedHeight = 40;
		public float Height
		{
			get { return height; }
			set
			{
				if (value > height)
					height = value;
			}
		}
		public float ExpandedHeight
		{
			get { return expandedHeight; }
			set
			{
				if (value > height)
					expandedHeight = value;
			}
		}
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

		private NSString cellIdentifier;

		public string Format = null;

		public event NSAction Tapped;

		public UITextAlignment Alignment = UITextAlignment.Left;


		public CustomCellElement(NSString cellIdentifier, string caption, T value) : base(caption)
		{
			this.cellIdentifier = cellIdentifier;

			Value = value;
		}
		public CustomCellElement(NSString cellIdentifier, string caption, T value, string format) : this(cellIdentifier, caption, value)
		{
			Format = format;
		}


		public override string Summary()
		{
			return String.Format(Format, Value);
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			UITableViewCell cell = (UITableViewCell)tv.DequeueReusableCell(cellIdentifier);
			if (cell == null)
			{
				var arr = NSBundle.MainBundle.LoadNib(cellIdentifier, null, null);
				cell = Runtime.GetNSObject<UITableViewCell>(arr.ValueAt(0));
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Default : UITableViewCellSelectionStyle.None;
			}

			if (cell is IUpdate<T>)
				(cell as IUpdate<T>).Update(Caption, Value, this);

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
			return Height;
		}
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

		public InnerSection(string caption, MethodInfo addMethod, Type elementType, object[] elementConstructorParameters) : base(caption)
		{
			var view = new UITableViewHeaderFooterView();
			view.TextLabel.Text = caption;
			var addButton = new UIButton(UIButtonType.ContactAdd) { TranslatesAutoresizingMaskIntoConstraints = false };
			addButton.SizeToFit();
			addButton.TouchUpInside += delegate
			{
				var result = addMethod.Invoke(null, new object[0]);
				if (result != null)
				{
					if (result is Task<T>)
					{
						Task<T> task = (Task<T>)result;
						task.ContinueWith((Task<T> arg) =>
						{
							T value = arg.Result;
							object[] parameters = new object[elementConstructorParameters.Length + 2];
							elementConstructorParameters.CopyTo(parameters, 0);
							parameters[elementConstructorParameters.Length] = (this.Count + 1).ToString();
							parameters[elementConstructorParameters.Length + 1] = value;
							var element = (Element)Activator.CreateInstance(elementType, parameters);
							UIApplication.SharedApplication.InvokeOnMainThread(
								new NSAction(() =>
								{
									Add(element);
								})
							);
						});
					}
					else
					{
						T value = (T)result;
						object[] parameters = new object[elementConstructorParameters.Length + 2];
						elementConstructorParameters.CopyTo(parameters, 0);
						parameters[elementConstructorParameters.Length] = (this.Count + 1).ToString();
						parameters[elementConstructorParameters.Length + 1] = value;
						var element = (Element)Activator.CreateInstance(elementType, parameters);
						Add(element);
					}
				}
			};
			view.AddSubviews(addButton);
			var top = 0f;
			var gap = 16f;
			var t = NSLayoutConstraint.Create(addButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, view, NSLayoutAttribute.Top, 1.0f, top);
			var r = NSLayoutConstraint.Create(addButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, view, NSLayoutAttribute.Trailing, 1.0f, -gap);
			view.AddConstraints(new NSLayoutConstraint[] { t, r });

			HeaderView = view;
		}

		public InnerSection(string caption) : base(caption) { }

		public InnerSection(string caption, string footer) : base(caption, footer) { }

		public InnerSection(UIView header) : base(header) { }

		public InnerSection(UIView header, UIView footer) : base(header, footer) { }

	}

	#endregion

}
