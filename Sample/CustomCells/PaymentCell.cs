using System;
using MonoTouch.Dialog;
using Foundation;
using UIKit;

namespace Canary
{
	public partial class PaymentCell : UITableViewCell, IUpdate<float>
	{
		public static readonly NSString Key = new NSString("PaymentCell");
		public static readonly UINib Nib;

		static PaymentCell()
		{
			Nib = UINib.FromName("PaymentCell", NSBundle.MainBundle);
		}

		protected PaymentCell(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public void Update(string caption, float value, CustomCellElement<float> element)
		{
			Title.Text = caption;
			SubTitle.Text = value.ToString("C");
		}
	}
}
