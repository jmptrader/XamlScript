using System.ComponentModel;
using System.Windows.Markup;

namespace XamlScript
{
    [ContentProperty("Source")]
    public class Script
    {
        public string Source { get; set; }

        [DefaultValue(ScriptType.JavaScript)]
        public ScriptType Type { get; set; }
    }
    public enum ScriptType { JavaScript, VBScript }
}