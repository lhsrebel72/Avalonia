﻿using System;
using System.Reflection;

namespace Avalonia.Diagnostics.ViewModels
{
    internal class ClrPropertyViewModel : PropertyViewModel
    {
        private readonly object _target;
        private Type _assignedType;
        private bool _isPinned;
        private object? _value;
        private readonly Type _propertyType;

#nullable disable
        // Remove "nullable disable" after MemberNotNull will work on our CI.
        public ClrPropertyViewModel(object o, PropertyInfo property)
#nullable restore
        {
            _target = o;
            Property = property;
            _isPinned = false;

            if (property.DeclaringType == null || !property.DeclaringType.IsInterface)
            {
                Name = property.Name;
            }
            else
            {
                Name = property.DeclaringType.Name + '.' + property.Name;
            }

            DeclaringType = property.DeclaringType;
            _propertyType = property.PropertyType;

            Update();
        }

        public PropertyInfo Property { get; }
        public override object Key => Name;
        public override string Name { get; }
        public override string Group => "CLR Properties";

        public override Type AssignedType => _assignedType;
        public override Type PropertyType => _propertyType;
        public override bool IsReadonly => !Property.CanWrite;

        //determines if property is pinned to top in dev tools
        public override bool IsPinned
        {
            get => _isPinned;
            set
            {
                try
                {
                    _isPinned = value;
                    OnIsPinnedChanged();
                }
                catch { }
            }
        }

        public override event EventHandler IsPinnedChanged;

        protected virtual void OnIsPinnedChanged()
        {
            IsPinnedChanged?.Invoke(this, EventArgs.Empty);
        }

        public override object? Value
        {
            get => _value;
            set
            {
                try
                {
                    Property.SetValue(_target, value);
                    Update();
                }
                catch { }
            }
        }

        public override string Priority => string.Empty;

        public override bool? IsAttached => default;

        public override Type? DeclaringType { get; }

        // [MemberNotNull(nameof(_type))]
        public override void Update()
        {
            object? value;
            Type? valueType = null;

            try
            {
                value = Property.GetValue(_target);
                valueType = value?.GetType();
            }
            catch (Exception e)
            {
                value = e.GetBaseException();
            }

            RaiseAndSetIfChanged(ref _value, value, nameof(Value));
            RaiseAndSetIfChanged(ref _assignedType, valueType ?? Property.PropertyType, nameof(AssignedType));
            RaisePropertyChanged(nameof(Type));
        }
    }
}
