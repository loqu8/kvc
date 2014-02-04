using MonoMac.Foundation;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

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

        public override void SetValueForKey(NSObject value, NSString key)
        {
            // should never be called
            var info = _type.GetProperty(key.ToString());
            if (!_isNotifyPropertyChanged) WillChangeValue(key);		// inefficient but oh well... beer first
            info.SetValue(_t, value.ToObject(info.PropertyType), null);
            if (!_isNotifyPropertyChanged) DidChangeValue(key);
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
            return target.ToNSObject();
        }

        public override NSObject ValueForKey(NSString nsKey)
        {
            var target = ValueForKey(_t, nsKey.ToString());
            return target.ToNSObject();
        }

        protected static Object ValueForKey(Object target, String key)
        {
            // Todo: target could be an IDictionary, IEnumerable or array, in which case access could be different, what if we get things like First/Last
            var type = target.GetType();
            PropertyInfo info = type.GetProperty(key);
            return info.GetValue(target, null);
        }
    }
}