using Microsoft.ClearScript;
using System;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.furesoft.tk/xaml/script/2015", "XamlScript")] 
[assembly: XmlnsPrefix("http://schemas.furesoft.tk/xaml/script/2015", "xs")]

namespace XamlScript
{
    public class Javascript : FrameworkElement
    {
        public static readonly DependencyProperty ScriptProperty =
            DependencyProperty.RegisterAttached("Script",
            typeof(Script), typeof(Javascript), new PropertyMetadata
            (new PropertyChangedCallback(ScriptChanged)));

        public static JScriptEngine _engine = new JScriptEngine();

        static Javascript()
        {
            _engine.Add("host", new ExtendedHostFunctions());

            _engine.Add("alert", new Action<string>((msg) => MessageBox.Show(msg)));
        }

        private static void ScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _engine.Execute(((Script)e.NewValue).Source);
        }

        public static Script GetScript(DependencyObject obj)
        {
            return (Script)obj.GetValue(ScriptProperty);
        }

        public static void SetScript(DependencyObject obj, Script value)
        {
            obj.SetValue(ScriptProperty, value);
        }
    }
}