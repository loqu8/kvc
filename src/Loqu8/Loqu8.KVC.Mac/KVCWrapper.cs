using MonoMac.Foundation;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Loqu8.KVC.Mac
{
    public class KVCWrapper : NSObject
    {
        private readonly Object _t;
        private readonly Type _type;
        private readonly bool _isNotifyPropertyChanged;

        public KVCWrapper(Object t)
        {
            _t = t;
            _type = t.GetType();

            var changed = _t as INotifyPropertyChanged;
            if (changed == null) return;

            _isNotifyPropertyChanged = true;
            var npc = changed;
            npc.PropertyChanged += HandlePropertyChanged;
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            WillChangeValue(e.PropertyName);
            DidChangeValue(e.PropertyName);
        }

        public override void SetValueForKeyPath(NSObject nsValue, NSString nsKeyPath)
        {
            var keyPath = nsKeyPath.ToString();
            var keys = keyPath.Split('.');

            Object target = _t;
            PropertyInfo info;
            for (int i = 0; i < keys.Length; i++)
            {
                // Todo: target could be an IDictionary, IEnumerable or array, in which case access could be different, what if we get things like First/Last
                if (target is IDictionary)
                {
                    var dict = (IDictionary)target;
                    if (!dict.Contains(keys[i]))
                        return;

                    target = dict[keys[i]];
                    continue;
                }

                info = target.GetType().GetProperty(keys[i]);
                if (info == null)
                    return;

                if (i == keys.Length - 1)
                {
                    if (i > 0 || !_isNotifyPropertyChanged) WillChangeValue(keys[0]);		// inefficient but oh well... beer first
                    info.SetValue(target, nsValue.ToObject(info.PropertyType), null);
                    if (i > 0 || !_isNotifyPropertyChanged) DidChangeValue(keys[0]);
                }
                else
                {
                    target = info.GetValue(target, null);
                }
            }
        }

		public override void SetValueForKey(NSObject value, NSString nsKey)
        {
			var key = nsKey.ToString ();
			if (key.Contains (".")) {
				SetValueForKeyPath (value, nsKey);
			}
				
			var info = _type.GetProperty(key);
            if (!_isNotifyPropertyChanged) WillChangeValue(nsKey);		// inefficient but oh well... beer first
            info.SetValue(_t, value.ToObject(info.PropertyType), null);
            if (!_isNotifyPropertyChanged) DidChangeValue(nsKey);
        }

        public override NSObject ValueForKeyPath(NSString nsKeyPath)
        {
            var keyPath = nsKeyPath.ToString();
            var keys = keyPath.Split('.');

            Object target = _t;
            foreach (var key in keys)
            {
                if (target is IDictionary)
                {
                    var dict = (IDictionary)target;
                    if (!dict.Contains(key))
                        return null;

                    target = dict[key];
                    continue;
                }

                target = ValueForKey(target, key);
            }
				
			if (!(target is string) && target is IEnumerable) {			
				var items = (IEnumerable)target;
				return items.ToKVCNSArray ();
			}
				
            return target.ToNSObject();
        }


        public override NSObject ValueForKey(NSString nsKey)
        {
			var key = nsKey.ToString ();
			if (key.Contains (".")) {

				return ValueForKeyPath (nsKey);
			}

			var target = ValueForKey(_t, key);		
            return target.ToNSObject();
        }

        protected static Object ValueForKey(Object target, String key)
        {            
			// TODO: target could be an IDictionary, IEnumerable or array, in which case access could be different, what if we get things like First/Last

			if (target is IEnumerable<object> && key == "Count") {
				var tolist = (IEnumerable<object>)target;
				target = tolist.ToList ();
			}

			if (target == null && key == "Count")
				return 0;			// for treeController when we are looking at a null collection

			var type = target.GetType();
			PropertyInfo info = type.GetProperty(key);

			Object value = null;
			if (info != null) {
				value = info.GetValue (target, null);
			}
				
			return value;
        }
    }
}