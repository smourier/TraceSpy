using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TraceSpy
{
    // all properties and methods start with DictionaryObject and are protected so they won't interfere with super type
    public abstract class DictionaryObject : INotifyPropertyChanged, INotifyPropertyChanging, IDataErrorInfo, INotifyDataErrorInfo
    {
        private readonly ConcurrentDictionary<string, DictionaryObjectProperty> _properties = new ConcurrentDictionary<string, DictionaryObjectProperty>();

        protected DictionaryObject()
        {
            DictionaryObjectRaiseOnPropertyChanging = true;
            DictionaryObjectRaiseOnPropertyChanged = true;
            DictionaryObjectRaiseOnErrorsChanged = true;
        }

        public virtual bool IsValid => !DictionaryObjectHasErrors;

        protected virtual ConcurrentDictionary<string, DictionaryObjectProperty> DictionaryObjectProperties => _properties;

        // these PropertyChangxxx are public and don't start with BaseObject because used by everyone
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event EventHandler<DictionaryObjectPropertyRollbackEventArgs> PropertyRollback;

        protected virtual bool DictionaryObjectRaiseOnPropertyChanging { get; set; }
        protected virtual bool DictionaryObjectRaiseOnPropertyChanged { get; set; }
        protected virtual bool DictionaryObjectRaiseOnErrorsChanged { get; set; }

        protected string DictionaryObjectError => DictionaryObjectGetError(null);
        protected bool DictionaryObjectHasErrors => (DictionaryObjectGetErrors(null)?.Cast<object>().Any()).GetValueOrDefault();

        public virtual void CopyTo(DictionaryObject other, DictionaryObjectPropertySetOptions options = DictionaryObjectPropertySetOptions.None)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (other == this)
                return;

            foreach (var kv in DictionaryObjectProperties)
            {
                if (kv.Value == null)
                    continue;

                other.DictionaryObjectSetPropertyValue(kv.Value.Value, options, kv.Key);
            }
        }

        protected virtual string DictionaryObjectGetError(string propertyName)
        {
            var errors = DictionaryObjectGetErrors(propertyName);
            if (errors == null)
                return null;

            string error = string.Join(Environment.NewLine, errors.Cast<object>().Select(e => string.Format("{0}", e)));
            return !string.IsNullOrEmpty(error) ? error : null;
        }

        protected virtual IEnumerable DictionaryObjectGetErrors(string propertyName) => null;

        protected void OnErrorsChanged(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            OnErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected virtual void OnErrorsChanged(object sender, DataErrorsChangedEventArgs e) => ErrorsChanged?.Invoke(sender, e);

        protected virtual void OnPropertyRollback(object sender, DictionaryObjectPropertyRollbackEventArgs e) => PropertyRollback?.Invoke(sender, e);

        protected void OnPropertyChanging(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            OnPropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanging(object sender, PropertyChangingEventArgs e) => PropertyChanging?.Invoke(sender, e);

        protected void OnPropertyChanged(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
            if (e?.PropertyName != nameof(IsValid))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }

        protected T DictionaryObjectGetPropertyValue<T>([CallerMemberName] string name = null) => DictionaryObjectGetPropertyValue(default(T), name);
        protected virtual T DictionaryObjectGetPropertyValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            DictionaryObjectProperties.TryGetValue(name, out DictionaryObjectProperty property);
            if (property == null)
                return defaultValue;

            if (!DictionaryObjectConversions.TryChangeType(property.Value, out T value))
                return defaultValue;

            return value;
        }

        protected virtual bool DictionaryObjectAreValuesEqual(object value1, object value2)
        {
            if (value1 == null)
                return value2 == null;

            if (value2 == null)
                return false;

            return value1.Equals(value2);
        }

        private class ObjectComparer : IEqualityComparer<object>
        {
            private readonly DictionaryObject _dob;

            public ObjectComparer(DictionaryObject dob)
            {
                _dob = dob;
            }

            public new bool Equals(object x, object y) => _dob.DictionaryObjectAreValuesEqual(x, y);
            public int GetHashCode(object obj) => (obj?.GetHashCode()).GetValueOrDefault();
        }

        protected virtual bool DictionaryObjectAreErrorsEqual(IEnumerable errors1, IEnumerable errors2)
        {
            if (errors1 == null && errors2 == null)
                return true;

            var dic = new Dictionary<object, int>(new ObjectComparer(this));
            IEnumerable<object> left = errors1 != null ? errors1.Cast<object>() : Enumerable.Empty<object>();
            foreach (var obj in left)
            {
                if (dic.ContainsKey(obj))
                {
                    dic[obj]++;
                }
                else
                {
                    dic.Add(obj, 1);
                }
            }

            if (errors2 == null)
                return dic.Count == 0;

            foreach (var obj in errors2)
            {
                if (dic.ContainsKey(obj))
                {
                    dic[obj]--;
                }
                else
                    return false;
            }
            return dic.Values.All(c => c == 0);
        }

        protected virtual DictionaryObjectProperty DictionaryObjectUpdatingProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty oldProperty, DictionaryObjectProperty newProperty) => null;
        protected virtual DictionaryObjectProperty DictionaryObjectUpdatedProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty oldProperty, DictionaryObjectProperty newProperty) => null;
        protected virtual DictionaryObjectProperty DictionaryObjectRollbackProperty(DictionaryObjectPropertySetOptions options, string name, DictionaryObjectProperty oldProperty, DictionaryObjectProperty newProperty) => null;
        protected virtual DictionaryObjectProperty DictionaryObjectCreateProperty() => new DictionaryObjectProperty();

        protected bool DictionaryObjectSetPropertyValue(object value, [CallerMemberName] string name = null) => DictionaryObjectSetPropertyValue(value, DictionaryObjectPropertySetOptions.None, name);
        protected virtual bool DictionaryObjectSetPropertyValue(object value, DictionaryObjectPropertySetOptions options, [CallerMemberName] string name = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            IEnumerable oldErrors = null;
            var rollbackOnError = (options & DictionaryObjectPropertySetOptions.RollbackChangeOnError) == DictionaryObjectPropertySetOptions.RollbackChangeOnError;
            var onErrorsChanged = (options & DictionaryObjectPropertySetOptions.DontRaiseOnErrorsChanged) != DictionaryObjectPropertySetOptions.DontRaiseOnErrorsChanged;
            if (!DictionaryObjectRaiseOnErrorsChanged)
            {
                onErrorsChanged = false;
            }

            if (onErrorsChanged || rollbackOnError)
            {
                oldErrors = DictionaryObjectGetErrors(name);
            }

            var forceChanged = (options & DictionaryObjectPropertySetOptions.ForceRaiseOnPropertyChanged) == DictionaryObjectPropertySetOptions.ForceRaiseOnPropertyChanged;
            var onChanged = (options & DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanged) != DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanged;
            if (!DictionaryObjectRaiseOnPropertyChanged)
            {
                onChanged = false;
                forceChanged = false;
            }

            var newProp = DictionaryObjectCreateProperty();
            newProp.Value = value;
            DictionaryObjectProperty oldProp = null;
            var finalProp = DictionaryObjectProperties.AddOrUpdate(name, newProp, (k, o) =>
            {
                oldProp = o;
                var updating = DictionaryObjectUpdatingProperty(options, k, o, newProp);
                if (updating != null)
                    return updating;

                var testEquality = (options & DictionaryObjectPropertySetOptions.DontTestValuesForEquality) != DictionaryObjectPropertySetOptions.DontTestValuesForEquality;
                if (testEquality && o != null && DictionaryObjectAreValuesEqual(value, o.Value))
                    return o;

                var onChanging = (options & DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanging) != DictionaryObjectPropertySetOptions.DontRaiseOnPropertyChanging;
                if (!DictionaryObjectRaiseOnPropertyChanging)
                {
                    onChanging = false;
                }

                if (onChanging)
                {
                    var e = new DictionaryObjectPropertyChangingEventArgs(name, oldProp, newProp);
                    OnPropertyChanging(this, e);
                    if (e.Cancel)
                        return o;
                }

                var updated = DictionaryObjectUpdatedProperty(options, k, o, newProp);
                if (updated != null)
                    return updated;

                return newProp;
            });

            if (forceChanged || (onChanged && ReferenceEquals(finalProp, newProp)))
            {
                var rollbacked = false;
                if (rollbackOnError)
                {
                    if ((DictionaryObjectGetErrors(name)?.Cast<object>().Any()).GetValueOrDefault())
                    {
                        var rolled = DictionaryObjectRollbackProperty(options, name, oldProp, newProp);
                        if (rolled == null)
                        {
                            rolled = oldProp;
                        }

                        if (rolled == null)
                        {
                            DictionaryObjectProperties.TryRemove(name, out DictionaryObjectProperty dop);
                        }
                        else
                        {
                            DictionaryObjectProperties.AddOrUpdate(name, rolled, (k, o) => rolled);
                        }

                        var e = new DictionaryObjectPropertyRollbackEventArgs(name, rolled, value);
                        OnPropertyRollback(this, e);
                        rollbacked = true;
                    }
                }

                if (!rollbacked)
                {
                    var e = new DictionaryObjectPropertyChangedEventArgs(name, oldProp, newProp);
                    OnPropertyChanged(this, e);

                    if (onErrorsChanged)
                    {
                        var newErrors = DictionaryObjectGetErrors(name);
                        if (!DictionaryObjectAreErrorsEqual(oldErrors, newErrors))
                        {
                            OnErrorsChanged(name);
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        string IDataErrorInfo.Error => DictionaryObjectError;
        string IDataErrorInfo.this[string columnName] => DictionaryObjectGetError(columnName);
        bool INotifyDataErrorInfo.HasErrors => DictionaryObjectHasErrors;
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName) => DictionaryObjectGetErrors(propertyName);
    }

    public class DictionaryObjectProperty
    {
        public object Value { get; set; }

        public override string ToString()
        {
            var value = Value;
            if (value == null)
                return null;

            if (value is string svalue)
                return svalue;

            return string.Format("{0}", value);
        }
    }

    [Flags]
    public enum DictionaryObjectPropertySetOptions
    {
        None = 0x0,
        DontRaiseOnPropertyChanging = 0x1,
        DontRaiseOnPropertyChanged = 0x2,
        DontTestValuesForEquality = 0x4,
        DontRaiseOnErrorsChanged = 0x8,
        ForceRaiseOnPropertyChanged = 0x10,
        TrackChanges = 0x20,
        RollbackChangeOnError = 0x40,
    }

    public class DictionaryObjectPropertyChangingEventArgs : PropertyChangingEventArgs
    {
        public DictionaryObjectPropertyChangingEventArgs(string propertyName, DictionaryObjectProperty existingProperty, DictionaryObjectProperty newProperty)
            : base(propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (newProperty == null)
                throw new ArgumentNullException(nameof(newProperty));

            // existingProperty may be null

            ExistingProperty = existingProperty;
            NewProperty = newProperty;
        }

        public DictionaryObjectProperty ExistingProperty { get; }
        public DictionaryObjectProperty NewProperty { get; }
        public bool Cancel { get; set; }
    }

    public class DictionaryObjectPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public DictionaryObjectPropertyChangedEventArgs(string propertyName, DictionaryObjectProperty existingProperty, DictionaryObjectProperty newProperty)
            : base(propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (newProperty == null)
                throw new ArgumentNullException(nameof(newProperty));

            // existingProperty may be null

            ExistingProperty = existingProperty;
            NewProperty = newProperty;
        }

        public DictionaryObjectProperty ExistingProperty { get; }
        public DictionaryObjectProperty NewProperty { get; }
    }

    public class DictionaryObjectPropertyRollbackEventArgs : EventArgs
    {
        public DictionaryObjectPropertyRollbackEventArgs(string propertyName, DictionaryObjectProperty existingProperty, object invalidValue)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            // existingProperty may be null
            PropertyName = propertyName;
            ExistingProperty = existingProperty;
            InvalidValue = invalidValue;
        }

        public string PropertyName { get; }
        public DictionaryObjectProperty ExistingProperty { get; }
        public object InvalidValue { get; }
    }

    internal static class DictionaryObjectConversions
    {
        public static object ChangeType(object input, Type conversionType) => ChangeType(input, conversionType, null, null);
        public static object ChangeType(object input, Type conversionType, object defaultValue) => ChangeType(input, conversionType, defaultValue, null);
        public static object ChangeType(object input, Type conversionType, object defaultValue, IFormatProvider provider)
        {
            if (!TryChangeType(input, conversionType, provider, out object value))
                return defaultValue;

            return value;
        }

        public static T ChangeType<T>(object input) => ChangeType(input, default(T));
        public static T ChangeType<T>(object input, T defaultValue) => ChangeType(input, defaultValue, null);
        public static T ChangeType<T>(object input, T defaultValue, IFormatProvider provider)
        {
            if (!TryChangeType(input, provider, out T value))
                return defaultValue;

            return value;
        }

        public static bool TryChangeType<T>(object input, out T value) => TryChangeType(input, null, out value);
        public static bool TryChangeType<T>(object input, IFormatProvider provider, out T value)
        {
            if (!TryChangeType(input, typeof(T), provider, out object tvalue))
            {
                value = default;
                return false;
            }

            value = (T)tvalue;
            return true;
        }

        public static bool TryChangeType(object input, Type conversionType, out object value) => TryChangeType(input, conversionType, null, out value);
        public static bool TryChangeType(object input, Type conversionType, IFormatProvider provider, out object value)
        {
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));

            if (conversionType == typeof(object))
            {
                value = input;
                return true;
            }

            if (conversionType.IsNullable())
            {
                Type nullableType = conversionType.GenericTypeArguments[0];
                if (input == null)
                {
                    value = null;
                    return true;
                }

                return TryChangeType(input, nullableType, provider, out value);
            }

            value = conversionType.IsValueType ? Activator.CreateInstance(conversionType) : null;
            if (input == null)
                return !conversionType.IsValueType;

            var inputType = input.GetType();
            if (inputType.IsAssignableFrom(conversionType))
            {
                value = input;
                return true;
            }

            if (conversionType.IsEnum)
                return EnumTryParse(conversionType, input, out value);

            if (conversionType == typeof(Guid))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 16)
                        return false;

                    value = new Guid(bytes);
                    return true;
                }

                var svalue = string.Format(provider, "{0}", input).Nullify();
                if (svalue != null && Guid.TryParse(svalue, out Guid guid))
                {
                    value = guid;
                    return true;
                }
                return false;
            }

            if (conversionType == typeof(IntPtr))
            {
                if (IntPtr.Size == 8)
                {
                    if (TryChangeType(input, provider, out long l))
                    {
                        value = new IntPtr(l);
                        return true;
                    }
                }
                else if (TryChangeType(input, provider, out int i))
                {
                    value = new IntPtr(i);
                    return true;
                }
                return false;
            }

            if (conversionType == typeof(bool))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 1)
                        return false;

                    value = BitConverter.ToBoolean(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(int))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((int)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((int)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((int)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((int)(byte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 4)
                        return false;

                    value = BitConverter.ToInt32(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(long))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((long)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((long)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((long)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((long)(byte)input);
                    return true;
                }

                if (inputType == typeof(DateTime))
                {
                    value = ((DateTime)input).Ticks;
                    return true;
                }

                if (inputType == typeof(TimeSpan))
                {
                    value = ((TimeSpan)input).Ticks;
                    return true;
                }

                if (inputType == typeof(DateTimeOffset))
                {
                    value = ((DateTimeOffset)input).Ticks;
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 8)
                        return false;

                    value = BitConverter.ToInt64(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(short))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((short)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((short)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((short)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((short)(byte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 2)
                        return false;

                    value = BitConverter.ToInt16(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(sbyte))
            {
                if (inputType == typeof(uint))
                {
                    value = unchecked((sbyte)(uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = unchecked((sbyte)(ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = unchecked((sbyte)(ushort)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = unchecked((sbyte)(byte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 1)
                        return false;

                    value = unchecked((sbyte)bytes[0]);
                    return true;
                }
            }

            if (conversionType == typeof(uint))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((uint)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((uint)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((uint)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((uint)(sbyte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 4)
                        return false;

                    value = BitConverter.ToUInt32(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(ulong))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((ulong)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((ulong)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((ulong)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((ulong)(sbyte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 8)
                        return false;

                    value = BitConverter.ToUInt64(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(ushort))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((ushort)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((ushort)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((ushort)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((ushort)(sbyte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 2)
                        return false;

                    value = BitConverter.ToUInt16(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(byte))
            {
                if (inputType == typeof(int))
                {
                    value = unchecked((byte)(int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = unchecked((byte)(long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = unchecked((byte)(short)input);
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = unchecked((byte)(sbyte)input);
                    return true;
                }

                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 1)
                        return false;

                    value = bytes[0];
                    return true;
                }
            }

            if (conversionType == typeof(decimal))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 16)
                        return false;

                    value = ToDecimal(bytes);
                    return true;
                }
            }

            if (conversionType == typeof(DateTime))
            {
                if (inputType == typeof(long))
                {
                    value = new DateTime((long)input);
                    return true;
                }

                if (inputType == typeof(DateTimeOffset))
                {
                    value = ((DateTimeOffset)input).DateTime;
                    return true;
                }
            }

            if (conversionType == typeof(TimeSpan))
            {
                if (inputType == typeof(long))
                {
                    value = new TimeSpan((long)input);
                    return true;
                }
            }

            if (conversionType == typeof(char))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 2)
                        return false;

                    value = BitConverter.ToChar(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(float))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 4)
                        return false;

                    value = BitConverter.ToSingle(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(double))
            {
                if (inputType == typeof(byte[]))
                {
                    var bytes = (byte[])input;
                    if (bytes.Length != 8)
                        return false;

                    value = BitConverter.ToDouble(bytes, 0);
                    return true;
                }
            }

            if (conversionType == typeof(DateTimeOffset))
            {
                if (inputType == typeof(DateTime))
                {
                    value = new DateTimeOffset((DateTime)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = new DateTimeOffset(new DateTime((long)input));
                    return true;
                }
            }

            if (conversionType == typeof(byte[]))
            {
                if (inputType == typeof(int))
                {
                    value = BitConverter.GetBytes((int)input);
                    return true;
                }

                if (inputType == typeof(long))
                {
                    value = BitConverter.GetBytes((long)input);
                    return true;
                }

                if (inputType == typeof(short))
                {
                    value = BitConverter.GetBytes((short)input);
                    return true;
                }

                if (inputType == typeof(uint))
                {
                    value = BitConverter.GetBytes((uint)input);
                    return true;
                }

                if (inputType == typeof(ulong))
                {
                    value = BitConverter.GetBytes((ulong)input);
                    return true;
                }

                if (inputType == typeof(ushort))
                {
                    value = BitConverter.GetBytes((ushort)input);
                    return true;
                }

                if (inputType == typeof(bool))
                {
                    value = BitConverter.GetBytes((bool)input);
                    return true;
                }

                if (inputType == typeof(char))
                {
                    value = BitConverter.GetBytes((char)input);
                    return true;
                }

                if (inputType == typeof(float))
                {
                    value = BitConverter.GetBytes((float)input);
                    return true;
                }

                if (inputType == typeof(double))
                {
                    value = BitConverter.GetBytes((double)input);
                    return true;
                }

                if (inputType == typeof(byte))
                {
                    value = new byte[] { (byte)input };
                    return true;
                }

                if (inputType == typeof(sbyte))
                {
                    value = new byte[] { unchecked((byte)(sbyte)input) };
                    return true;
                }

                if (inputType == typeof(decimal))
                {
                    value = ((decimal)value).ToBytes();
                    return true;
                }

                if (inputType == typeof(Guid))
                {
                    value = ((Guid)input).ToByteArray();
                    return true;
                }
            }

            var tc = TypeDescriptor.GetConverter(conversionType);
            if (tc != null && tc.CanConvertFrom(inputType))
            {
                try
                {
                    value = tc.ConvertFrom(null, provider as CultureInfo, input);
                    return true;
                }
                catch
                {
                    // continue;
                }
            }

            tc = TypeDescriptor.GetConverter(inputType);
            if (tc != null && tc.CanConvertTo(conversionType))
            {
                try
                {
                    value = tc.ConvertTo(null, provider as CultureInfo, input, conversionType);
                    return true;
                }
                catch
                {
                    // continue;
                }
            }

            if (input is IConvertible convertible)
            {
                try
                {
                    value = convertible.ToType(conversionType, provider);
                    return true;
                }
                catch
                {
                    // continue
                }
            }

            if (conversionType == typeof(string))
            {
                value = string.Format(provider, "{0}", input);
                return true;
            }

            return false;
        }

        public static decimal ToDecimal(this byte[] bytes)
        {
            if (bytes == null || bytes.Length != 16)
                throw new ArgumentException(null, nameof(bytes));

            var ints = new int[4];
            Buffer.BlockCopy(bytes, 0, ints, 0, 16);
            return new decimal(ints);
        }

        public static byte[] ToBytes(this decimal dec)
        {
            var bytes = new byte[16];
            Buffer.BlockCopy(decimal.GetBits(dec), 0, bytes, 0, 16);
            return bytes;
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static string Nullify(this string text)
        {
            if (text == null)
                return null;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            var t = text.Trim();
            return t.Length == 0 ? null : t;
        }

        private static readonly Lazy<MethodInfo> _enumTryParse = new Lazy<MethodInfo>(() => typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == nameof(Enum.TryParse) && m.GetParameters().Length == 3));

        public static bool EnumTryParse(Type enumType, object value, out object enumValue) => EnumTryParse(enumType, value, true, out enumValue);
        public static bool EnumTryParse(Type enumType, object value, bool ignoreCase, out object enumValue)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            if (!enumType.IsEnum)
                throw new ArgumentException(null, nameof(enumType));

            var svalue = value as string;
            if (svalue == null && value != null)
            {
                svalue = string.Format("{0}", value);
            }

            var tryParse = _enumTryParse.Value.MakeGenericMethod(enumType);
            var args = new object[] { svalue, ignoreCase, Enum.ToObject(enumType, 0) };
            var result = (bool)tryParse.Invoke(null, args);
            enumValue = args[2];
            return result;
        }
    }
}
