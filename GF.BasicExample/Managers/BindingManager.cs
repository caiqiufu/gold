using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace GF.BasicExample.Managers
{
    internal static class BindingManager
    {
        public static void BindTo<T>(this ListControl control, IList<T> dataSource)
        {
            BindTo(control, ToBindingList(dataSource));
        }

        public static void BindTo(this Control control, IBindingList dataSource)
        {
            var bindingProperty = GetAttribute<DefaultBindingPropertyAttribute>(control);
            var propertyName = bindingProperty?.Name ?? nameof(Control.Text);
            var binding = new Binding(propertyName, dataSource, null, true, DataSourceUpdateMode.OnPropertyChanged);

            control.DataBindings.Clear();
            control.DataBindings.Add(binding);
        }

        public static void BindTo(this ListControl control, IBindingList dataSource)
        {
            control.DataSource = dataSource;
        }

        private static T GetAttribute<T>(IDisposable source)
            where T : Attribute
        {
            return Attribute.GetCustomAttribute(source.GetType(), typeof(T)) as T;
        }

        private static IBindingList ToBindingList<T>(IList<T> source)
        {
            return new BindingList<T>(source);
        }
    }
}
