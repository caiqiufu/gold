using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GF.BasicExample.Converters;

namespace GF.BasicExample.Managers
{
    internal sealed class DescriptionManager : IDisposable
    {
        private readonly ToolTip _toolTip;
        private readonly Dictionary<ListBox, IDescriptionProvider> _controls;

        public DescriptionManager(ToolTip toolTip)
        {
            _toolTip = toolTip;
            _controls = new Dictionary<ListBox, IDescriptionProvider>();
        }

        public static string ToDescription<T>(object item, Action<T, StringBuilder> details)
        {
            var sb = new StringBuilder();

            sb.AppendLine(TypeConverterBase.ToString(item));

            if (item is T typedItem)
                details(typedItem, sb);

            return sb.ToString();
        }

        public void Register(ListBox control, IDescriptionProvider provider)
        {
            if (!_controls.ContainsKey(control))
                control.MouseMove += OnMouseMove;

            _controls[control] = provider;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var listBox = (ListBox)sender;
            var index = listBox.IndexFromPoint(e.Location);

            var item = index >= 0 && index < listBox.Items.Count
                ? listBox.Items[index]
                : null;

            SetToolTip(listBox, item);
        }

        private string SetToolTip(ListBox control, object item)
        {
            if (item != null && _controls.TryGetValue(control, out var provider))
            {
                var toolTipText = provider.GetItemDescription(item);

                if (_toolTip.GetToolTip(control) != toolTipText)
                    _toolTip.SetToolTip(control, toolTipText);

                return toolTipText;
            }

            _toolTip.Hide(control);
            return null;
        }

        public void Dispose()
        {
            foreach (var control in _controls.Keys)
                control.MouseMove -= OnMouseMove;

            _controls.Clear();
        }
    }
}
