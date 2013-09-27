using System;
using MonoMac.Foundation;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace Loqu8.KVC
{
	public class KVCWrapper : NSObject
	{
		private readonly Object _t;
		private readonly Type _type;

		public KVCWrapper (Object t)
		{
			_t = t;
			_type = t.GetType ();
		}
	
		public override void SetValueForKeyPath (NSObject nsValue, NSString nsKeyPath)
		{
			var keyPath = nsKeyPath.ToString ();
			var keys = keyPath.Split ('.');

			Object target = _t;
			PropertyInfo info;
			for (int i = 0; i < keys.Length; i++) {
				// Todo: target could be an IDictionary, IEnumerable or array, in which case access could be different, what if we get things like First/Last
				if (target is IDictionary) {
					var dict = (IDictionary)target;
					if (!dict.Contains (keys [i]))
						return;				

					target = dict [keys [i]];
					continue;
				}

				info = target.GetType ().GetProperty (keys[i]);
				if (info == null)
					return;

				if (i == keys.Length - 1) {
					WillChangeValue (keys[0]);
					info.SetValue (target, nsValue.ToObject (info.PropertyType), null);
					DidChangeValue (keys[0]);
				} else {
					target = info.GetValue (target, null);
				}
			}
		}

		public override void SetValueForKey (NSObject value, NSString key)
		{
			// should not be called
			var info = _type.GetProperty (key.ToString ());
			info.SetValue (_t, value.ToObject(info.PropertyType), null);
		}

		public override NSObject ValueForKeyPath (NSString nsKeyPath)
		{
			var keyPath = nsKeyPath.ToString ();
			var keys = keyPath.Split ('.');

			Object target = _t;
			foreach (var key in keys) {
				if (target is IDictionary) {
					var dict = (IDictionary)target;
					if (!dict.Contains (key))
						return;				

					target = dict [key];
					continue;
				}

				target = ValueForKey (target, key);
			}
			return target.ToNSObject ();
		}

		public override NSObject ValueForKey (NSString nsKey)
		{
			var target = ValueForKey (_t, nsKey.ToString());
			return target.ToNSObject ();
		}

		protected static Object ValueForKey (Object target, String key)
		{
			// Todo: target could be an IDictionary, IEnumerable or array, in which case access could be different, what if we get things like First/Last
			var type = target.GetType ();
			PropertyInfo info = type.GetProperty (key);
			return info.GetValue (target, null);
		}
	}
}

