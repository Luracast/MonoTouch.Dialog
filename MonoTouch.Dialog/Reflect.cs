//
// Reflect.cs: Creates Element classes from an instance
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Linq;

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
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class ElementAttribute : Attribute
	{
		public ElementAttribute(Type customType, params object[] parameters)
		{
			CustomType = customType;
			Parameters = parameters;
		}
		public Type CustomType;
		public object[] Parameters;
	}

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

	public interface IUpdate<T>
	{
		void Update(string caption, T value, CustomCellElement<T> element);
	}

	public class CustomCellElement<T> : Element, IProvideValue<T>, IElementSizing
	{
		float height = 40;
		float expandedHeight = 40;
		public float Height 
		{ 
			get { return height; }
			set {
				if (value > height)
					height = value;
			}
		}
		public float ExpandedHeight 
		{ 
			get{ return expandedHeight; }
			set{
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

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class EntryAttribute : Attribute {
		public EntryAttribute () : this (null) { }

		public EntryAttribute (string placeholder)
		{
			Placeholder = placeholder;
		}

		public string Placeholder;
		public UIKeyboardType KeyboardType;
		public UITextAutocorrectionType AutocorrectionType;
		public UITextAutocapitalizationType AutocapitalizationType;
		public UITextFieldViewMode ClearButtonMode;
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class DateAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class TimeAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class CheckboxAttribute : Attribute { }

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class MultilineAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class HtmlAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class SkipAttribute : Attribute {}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class PasswordAttribute : EntryAttribute {
		public PasswordAttribute (string placeholder) : base (placeholder) {}
	}
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class AlignmentAttribute : Attribute {
		public AlignmentAttribute (UITextAlignment alignment) {
			Alignment = alignment;
		}
		public UITextAlignment Alignment;
	}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class RadioSelectionAttribute : Attribute {
		public string Target;
		public RadioSelectionAttribute (string target) 
		{
			Target = target;
		}
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class OnTapAttribute : Attribute {
		public OnTapAttribute (string method)
		{
			Method = method;
		}
		public string Method;
	}
	
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class CaptionAttribute : Attribute {
		public CaptionAttribute (string caption)
		{
			Caption = caption;
		}
		public string Caption;
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
	public class SectionAttribute : Attribute {
		public SectionAttribute () {}
		
		public SectionAttribute (string caption)
		{
			Caption = caption;
		}
			
		public SectionAttribute (string caption, string footer)
		{
			Caption = caption;
			Footer = footer;
		}
		public string Caption, Footer;
	}

	public class RangeAttribute : Attribute {
		public RangeAttribute (float low, float high)
		{
			Low = low;
			High = high;
		}
		public float Low, High;
		public bool ShowCaption;
	}

	public class BindingContext : IDisposable {
		public RootElement Root;
		Dictionary<Element,MemberAndInstance> mappings;
			
		class MemberAndInstance {
			public MemberAndInstance (MemberInfo mi, object o)
			{
				Member = mi;
				Obj = o;
			}
			public MemberInfo Member;
			public object Obj;
		}
		
		static object GetValue (MemberInfo mi, object o)
		{
			var fi = mi as FieldInfo;
			if (fi != null)
				return fi.GetValue (o);
			var pi = mi as PropertyInfo;
			
			var getMethod = pi.GetGetMethod ();
			return getMethod.Invoke (o, new object [0]);
		}

		static void SetValue (MemberInfo mi, object o, object val)
		{
			var fi = mi as FieldInfo;
			if (fi != null){
				fi.SetValue (o, val);
				return;
			}
			var pi = mi as PropertyInfo;
			var setMethod = pi.GetSetMethod ();
			setMethod.Invoke (o, new object [] { val });
		}
			
		static string MakeCaption (string name)
		{
			var sb = new StringBuilder (name.Length);
			bool nextUp = true;
			
			foreach (char c in name){
				if (nextUp){
					sb.Append (Char.ToUpper (c));
					nextUp = false;
				} else {
					if (c == '_'){
						sb.Append (' ');
						continue;
					}
					if (Char.IsUpper (c))
						sb.Append (' ');
					sb.Append (c);
				}
			}
			return sb.ToString ();
		}

		// Returns the type for fields and properties and null for everything else
		static Type GetTypeForMember (MemberInfo mi)
		{				
			if (mi is FieldInfo)
				return ((FieldInfo) mi).FieldType;
			else if (mi is PropertyInfo)
				return ((PropertyInfo) mi).PropertyType;
			return null;
		}
		
		public BindingContext (object callbacks, object o, string title)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			
			mappings = new Dictionary<Element,MemberAndInstance> ();
			
			Root = new RootElement (title);
			Populate (callbacks, o, Root);
		}
		
		void Populate (object callbacks, object o, RootElement root)
		{
			MemberInfo last_radio_index = null;
			var members = o.GetType ().GetMembers (BindingFlags.DeclaredOnly | BindingFlags.Public |
							       BindingFlags.NonPublic | BindingFlags.Instance);

			Section section = null;
			
			foreach (var mi in members){
				Type mType = GetTypeForMember (mi);

				if (mType == null)
					continue;
				Element element = null;
				string caption = null;
				object [] attrs = mi.GetCustomAttributes (false);
				bool skip = false;
				foreach (var attr in attrs){
					if (attr is SkipAttribute || attr is System.Runtime.CompilerServices.CompilerGeneratedAttribute)
						skip = true;
					else if (attr is CaptionAttribute)
						caption = ((CaptionAttribute)attr).Caption;
					else if (attr is SectionAttribute)
					{
						if (section != null)
							root.Add(section);
						var sa = attr as SectionAttribute;
						section = new Section(sa.Caption, sa.Footer);
					}
					else if (attr is CellAttribute)
					{
						skip = true;
						object[] customParams = ((CellAttribute)attr).Parameters;
						caption = caption ?? MakeCaption(mi.Name);
						var cellIdentier = new NSString(((CellAttribute)attr).Identifier);

						if (mType.IsArray)
						{
							Type CustomType = typeof(CustomCellElement<>).MakeGenericType(mType.GetElementType());
							int counter = 1;

							Type ist = typeof(InnerSection<>).MakeGenericType(mType.GetElementType());
							var subsection = (Section)Activator.CreateInstance(ist, caption);

							foreach (var v in (IEnumerable)GetValue(mi, o))
							{
								List<object> parameters = new List<object>() {cellIdentier, counter.ToString(), v };
								if (customParams != null)
								{
									parameters.AddRange(customParams);
								}
								element = (Element)Activator.CreateInstance(CustomType, parameters.ToArray());
								if (element != null)
								{
									subsection.Add(element);
								}
								counter++;
							}
							mappings[subsection] = new MemberAndInstance(mi, o);
							if (section != null)
							{
								root.Add(section);
								section = null;
							}
							root.Add(subsection);
						}
						else
						{
							Type CustomType = typeof(CustomCellElement<>).MakeGenericType(mType);
							var parameters = new List<object>() { cellIdentier, caption, GetValue(mi, o) };
							if (customParams != null)
							{
								parameters.AddRange(customParams);
							}
							element = (Element)Activator.CreateInstance(CustomType, parameters.ToArray());
							if (element != null)
							{
								if (section == null)
									section = new Section();
								section.Add(element);
								mappings[element] = new MemberAndInstance(mi, o);
							}
						}
					}
					else if (attr is ElementAttribute)
					{
						skip = true;
						object[] customParams = ((ElementAttribute)attr).Parameters;
						caption = caption ?? MakeCaption(mi.Name);
						Type CustomType = ((ElementAttribute)attr).CustomType;

						if (mType.IsArray)
						{
							int counter = 1;

							Type ist = typeof(InnerSection<>).MakeGenericType(mType.GetElementType());
							var subsection = (Section) Activator.CreateInstance(ist, caption);

							foreach (var v in (IEnumerable) GetValue(mi, o))
							{
								List<object> parameters = new List<object>() { counter.ToString(), v };
								if (customParams != null)
								{
									parameters.AddRange(customParams);
								}
								element = (Element)Activator.CreateInstance(CustomType, parameters.ToArray());
								if (element != null)
								{
									subsection.Add(element);
								}
								counter++;
							}
							mappings[subsection] = new MemberAndInstance(mi, o);
							if (section != null)
							{
								root.Add(section);
								section = null;
							}
							root.Add(subsection);
						}
						else
						{
							var parameters = new List<object>() {caption,  GetValue(mi, o) };
							if (customParams != null)
							{
								parameters.AddRange(customParams);
							}
							element = (Element)Activator.CreateInstance(CustomType, parameters.ToArray());
							if (element != null)
							{
								if (section == null)
									section = new Section();
								section.Add(element);
								mappings[element] = new MemberAndInstance(mi, o);
							}
						}
					}
				}
				if (skip)
					continue;
				
				if (caption == null)
					caption = MakeCaption (mi.Name);
				
				if (section == null)
					section = new Section ();
				

				if (mType == typeof (string)){
					PasswordAttribute pa = null;
					AlignmentAttribute align = null;
					EntryAttribute ea = null;
					object html = null;
					NSAction invoke = null;
					bool multi = false;
					
					foreach (object attr in attrs){
						if (attr is PasswordAttribute)
							pa = attr as PasswordAttribute;
						else if (attr is EntryAttribute)
							ea = attr as EntryAttribute;
						else if (attr is MultilineAttribute)
							multi = true;
						else if (attr is HtmlAttribute)
							html = attr;
						else if (attr is AlignmentAttribute)
							align = attr as AlignmentAttribute;
						
						if (attr is OnTapAttribute){
							string mname = ((OnTapAttribute) attr).Method;
							
							if (callbacks == null){
								throw new Exception ("Your class contains [OnTap] attributes, but you passed a null object for `context' in the constructor");
							}
							
							var method = callbacks.GetType ().GetMethod (mname);
							if (method == null)
								throw new Exception ("Did not find method " + mname);
							invoke = delegate {
								method.Invoke (method.IsStatic ? null : callbacks, new object [0]);
							};
						}
					}
					
					string value = (string) GetValue (mi, o);
					if (pa != null)
						element = new EntryElement (caption, pa.Placeholder, value, true);
					else if (ea != null)
						element = new EntryElement (caption, ea.Placeholder, value) { KeyboardType = ea.KeyboardType, AutocapitalizationType = ea.AutocapitalizationType, AutocorrectionType = ea.AutocorrectionType, ClearButtonMode = ea.ClearButtonMode };
					else if (multi)
						element = new MultilineElement (caption, value);
					else if (html != null)
						element = new HtmlElement (caption, value);
					else {
						var selement = new StringElement (caption, value);
						element = selement;
						
						if (align != null)
							selement.Alignment = align.Alignment;
					}
					
					if (invoke != null)
						((StringElement) element).Tapped += invoke;
				} else if (mType == typeof (float)){
					var floatElement = new FloatElement (null, null, (float) GetValue (mi, o));
					floatElement.Caption = caption;
					element = floatElement;
					
					foreach (object attr in attrs){
						if (attr is RangeAttribute){
							var ra = attr as RangeAttribute;
							floatElement.MinValue = ra.Low;
							floatElement.MaxValue = ra.High;
							floatElement.ShowCaption = ra.ShowCaption;
						}
					}
				} else if (mType == typeof (bool)){
					bool checkbox = false;
					foreach (object attr in attrs){
						if (attr is CheckboxAttribute)
							checkbox = true;
					}
					
					if (checkbox)
						element = new CheckboxElement (caption, (bool) GetValue (mi, o));
					else
						element = new BooleanElement (caption, (bool) GetValue (mi, o));
				} else if (mType == typeof (DateTime)){
					var dateTime = (DateTime) GetValue (mi, o);
					bool asDate = false, asTime = false;
					
					foreach (object attr in attrs){
						if (attr is DateAttribute)
							asDate = true;
						else if (attr is TimeAttribute)
							asTime = true;
					}
					
					if (asDate)
						element = new DateElement (caption, dateTime);
					else if (asTime)
						element = new TimeElement (caption, dateTime);
					else
						 element = new DateTimeElement (caption, dateTime);
				} else if (mType.IsEnum){
					var csection = new Section ();
					ulong evalue = Convert.ToUInt64 (GetValue (mi, o), null);
					int idx = 0;
					int selected = 0;
					
					foreach (var fi in mType.GetFields (BindingFlags.Public | BindingFlags.Static)){
						ulong v = Convert.ToUInt64 (GetValue (fi, null));
						
						if (v == evalue)
							selected = idx;
						
						CaptionAttribute ca = Attribute.GetCustomAttribute(fi, typeof(CaptionAttribute)) as CaptionAttribute;
						csection.Add (new RadioElement (ca != null ? ca.Caption : MakeCaption (fi.Name)));
						idx++;
					}
					
					element = new RootElement (caption, new RadioGroup (null, selected)) { csection };
				} else if (mType == typeof (UIImage)){
					element = new ImageElement ((UIImage) GetValue (mi, o));
				} else if (typeof (System.Collections.IEnumerable).IsAssignableFrom (mType)){
					var csection = new Section ();
					int count = 0;
					
					if (last_radio_index == null)
						throw new Exception ("IEnumerable found, but no previous int found");
					foreach (var e in (IEnumerable) GetValue (mi, o)){
						csection.Add (new RadioElement (e.ToString ()));
						count++;
					}
					int selected = (int) GetValue (last_radio_index, o);
					if (selected >= count || selected < 0)
						selected = 0;
					element = new RootElement (caption, new MemberRadioGroup (null, selected, last_radio_index)) { csection };
					last_radio_index = null;
				} else if (typeof (int) == mType){
					foreach (object attr in attrs){
						if (attr is RadioSelectionAttribute){
							last_radio_index = mi;
							break;
						}
					}
				} else {
					var nested = GetValue (mi, o);
					if (nested != null){
						var newRoot = new RootElement (caption);
						Populate (callbacks, nested, newRoot);
						element = newRoot;
					}
				}
				
				if (element == null)
					continue;
				section.Add (element);
				mappings [element] = new MemberAndInstance (mi, o);
			}
			root.Add (section);
		}
		
		class MemberRadioGroup : RadioGroup {
			public MemberInfo mi;
			
			public MemberRadioGroup (string key, int selected, MemberInfo mi) : base (key, selected)
			{
				this.mi = mi;
			}
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				foreach (var element in mappings.Keys){
					element.Dispose ();
				}
				mappings = null;
			}
		}
		
		public void Fetch ()
		{
			foreach (var dk in mappings){
				Element element = dk.Key;
				MemberInfo mi = dk.Value.Member;
				object obj = dk.Value.Obj;
				if (element is DateTimeElement)
					SetValue (mi, obj, ((DateTimeElement) element).DateValue);
				else if (element is FloatElement)
					SetValue (mi, obj, ((FloatElement) element).Value);
				else if (element is BooleanElement)
					SetValue (mi, obj, ((BooleanElement) element).Value);
				else if (element is CheckboxElement)
					SetValue (mi, obj, ((CheckboxElement) element).Value);
				else if (element is EntryElement){
					var entry = (EntryElement) element;
					entry.FetchValue ();
					SetValue (mi, obj, entry.Value);
				} 
				else if (element is ImageElement)
					SetValue (mi, obj, ((ImageElement) element).Value);
				else if (element is RootElement){
					var re = element as RootElement;
					if (re.group as MemberRadioGroup != null){
						var group = re.group as MemberRadioGroup;
						SetValue (group.mi, obj, re.RadioSelected);
					} else if (re.group as RadioGroup != null){
						var mType = GetTypeForMember (mi);
						var fi = mType.GetFields (BindingFlags.Public | BindingFlags.Static) [re.RadioSelected];
						
						SetValue (mi, obj, fi.GetValue (null));
					}
				} 
				else if (element.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IProvideValue<>)))
				{
					SetValue(mi, obj, element.GetType().GetProperty("Value").GetValue(element, null));
				}
			}
		}
	}
}
