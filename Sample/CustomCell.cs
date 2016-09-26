using System;

using Foundation;
using MonoTouch.Dialog;
using UIKit;

namespace Sample
{
	public partial class CustomCell : UITableViewCell, IUpdate<float>
	{
		public static readonly NSString Key = new NSString("CustomCell");
		public static readonly UINib Nib;

		static CustomCell()
		{
			Nib = UINib.FromName("CustomCell", NSBundle.MainBundle);
		}

		protected CustomCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public CustomCell() : base()
		{
			
		}

		public void Update(string title, string subtitle, string id = "", DateTime? date = null)
		{
			Title.Text = title;
			SubTitle.Text = subtitle;
			ID.Text = id;
			Date.Text = date == null ? "" : date.Value.ToString("d");
		}

		public void Update(string caption, float value, CustomCellElement<float> element)
		{
			Title.Text = "";
			SubTitle.Text = value.ToString("C")+ " SGD";
			ID.Text = caption;
			Date.Text = "";
			element.Height = 60;
			element.ExpandedHeight = 60;
		}
	}
}
