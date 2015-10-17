using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace XamlScript
{
    [MarkupExtensionReturnType(typeof(object))]
    public class FunctionExtension : MarkupExtension
    {
        public string Name { get; set; }

        public FunctionExtension(string name)
        {
            Name = name;
        }
        public FunctionExtension()
        {

        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var s = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var e = s.TargetProperty as EventInfo;

            return new RoutedEventHandler((se, ea) => JavaScript._engine.Script[Name](se, ea));
        }
    }
}