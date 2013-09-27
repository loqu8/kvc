using System;
using System.Globalization;
using MonoMac.Foundation;
using System.Drawing;

namespace Loqu8.KVC
{
	public static class NSObjectExtensions
	{
		public static NSObject ToNSObject (this Object o)
		{
			if (o is DateTime) {
				var date = (DateTime)o;
				NSDate nsDate = DateTime.SpecifyKind (date, DateTimeKind.Utc);
				return nsDate;
			} else {
				return NSObject.FromObject (o);
			}
		}

		public static Object ToObject (this NSObject nsO)
		{
			return nsO.ToObject (null);
		}

//		public enum TypeCode
//		{
//			Empty,
//			Object,
//			DBNull,
//			Boolean,
//			Char,
//			SByte,
//			Byte,
//			Int16,
//			UInt16,
//			Int32,
//			UInt32,
//			Int64,
//			UInt64,
//			Single,
//			Double,
//			Decimal,
//			DateTime,
//			String = 18
//		}

		public static Object ToObject (this NSObject nsO, Type targetType)
		{
			if (nsO is NSString) {
				var s = (NSString)nsO;
				return s.ToString ();
			}

			if (nsO == null && Type.GetTypeCode (targetType) == TypeCode.String) {
				return string.Empty;
			}

			if (nsO is NSDate) {
				var nsDate = (NSDate)nsO;
				return DateTime.SpecifyKind (nsDate, DateTimeKind.Unspecified);
			}

			if (nsO is NSDecimalNumber) {
				return decimal.Parse (nsO.ToString (), CultureInfo.InvariantCulture);
			}
			
			if (nsO is NSNumber) {
				var x = (NSNumber)nsO;
			
				switch (Type.GetTypeCode (targetType)) {
				case TypeCode.Boolean:
					return x.BoolValue;
				case TypeCode.Char:
					return Convert.ToChar (x.ByteValue);
				case TypeCode.SByte:
					return x.SByteValue;
				case TypeCode.Byte:
					return x.ByteValue;
				case TypeCode.Int16:
					return x.Int16Value;
				case TypeCode.UInt16:
					return x.UInt16Value;
				case TypeCode.Int32:
					return x.Int32Value;
				case TypeCode.UInt32:
					return x.UInt32Value;
				case TypeCode.Int64:
					return x.Int64Value;
				case TypeCode.UInt64:
					return x.UInt64Value;
				case TypeCode.Single:
					return x.FloatValue;
				case TypeCode.Double:
					return x.DoubleValue;
				}
			}

			if (nsO is NSValue) {
				var v = (NSValue)nsO;

				if (targetType == typeof(IntPtr)) {
					return v.PointerValue;
				}

				if (targetType == typeof(SizeF)) {
					return v.SizeFValue;
				}

				if (targetType == typeof(RectangleF)) {
					return v.RectangleFValue;
				}

				if (targetType == typeof(PointF)) {
					return v.PointFValue;
				}			
			}

			return nsO;
		}
	}
}

