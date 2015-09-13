using System;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents an instance of the VBScript engine.
    /// </summary>
    public class VBScriptEngine : WindowsScriptEngine
    {
        #region data
        
        private static readonly Dictionary<int, string> runtimeErrorMap = new Dictionary<int, string>
        {
            // http://msdn.microsoft.com/en-us/library/xe43cc8d(VS.84).aspx
            { 429, "ActiveX component can't create object" },
            { 507, "An exception occurred" },
            { 449, "Argument not optional" },
            { 17, "Can't perform requested operation" },
            { 430, "Class doesn't support Automation" },
            { 506, "Class not defined" },
            { 11, "Division by zero" },
            { 48, "Error in loading DLL" },
            { 5020, "Expected ')' in regular expression" },
            { 5019, "Expected ']' in regular expression" },
            { 432, "File name or class name not found during Automation operation" },
            { 92, "For loop not initialized" },
            { 5008, "Illegal assignment" },
            { 51, "Internal error" },
            { 505, "Invalid or unqualified reference" },
            { 481, "Invalid picture" },
            { 5, "Invalid procedure call or argument" },
            { 5021, "Invalid range in character set" },
            { 94, "Invalid use of Null" },
            { 448, "Named argument not found" },
            { 447, "Object doesn't support current locale setting" },
            { 445, "Object doesn't support this action" },
            { 438, "Object doesn't support this property or method" },
            { 451, "Object not a collection" },
            { 504, "Object not safe for creating" },
            { 503, "Object not safe for initializing" },
            { 502, "Object not safe for scripting" },
            { 424, "Object required" },
            { 91, "Object variable not set" },
            { 7, "Out of Memory" },
            { 28, "Out of stack space" },
            { 14, "Out of string space" },
            { 6, "Overflow" },
            { 35, "Sub or function not defined" },
            { 9, "Subscript out of range" },
            { 5017, "Syntax error in regular expression" },
            { 462, "The remote server machine does not exist or is unavailable" },
            { 10, "This array is fixed or temporarily locked" },
            { 13, "Type mismatch" },
            { 5018, "Unexpected quantifier" },
            { 500, "Variable is undefined" },
            { 458, "Variable uses an Automation type not supported in VBScript" },
            { 450, "Wrong number of arguments or invalid property assignment" }
        };
        
        #endregion
        
        public void AddHostFunction(string name, Delegate d)
        {
            this.AddHostObject(name, d);
        }
        
        public void Add<T>(string name, T obj)
        {
            if (typeof(T) == typeof(Type))
            {
                this.AddHostType(name, obj as Type);
            }
            else if (typeof(T) == typeof(Delegate))
            {
                this.AddHostFunction(name, obj as Delegate);
            }
            else
            {
                this.AddHostObject(name, obj);
            }
        }
        
        #region constructors
        
        /// <summary>
        /// Initializes a new VBScript engine instance.
        /// </summary>
        public VBScriptEngine() : this(null)
        {
        }
        
        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public VBScriptEngine(string name) : this(name, WindowsScriptEngineFlags.None)
        {
        }
        
        /// <summary>
        /// Initializes a new VBScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(WindowsScriptEngineFlags flags) : this(null, flags)
        {
        }
        
        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(string name, WindowsScriptEngineFlags flags) : this("VBScript", name, flags)
        {
        }
        
        /// <summary>
        /// Initializes a new VBScript engine instance with the specified programmatic
        /// identifier, name, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the VBScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected VBScriptEngine(string progID, string name, WindowsScriptEngineFlags flags) : base(progID, name, flags)
        {
            this.Execute(
MiscHelpers.FormatInvariant("{0} [internal]", this.GetType().Name),
                @"
                     class EngineInternalImpl
                         public function getCommandResult(value)
                             if IsObject(value) then
                                 if value is nothing then
                                     getCommandResult = ""[nothing]""
                                 else
                                     dim ValueTypeName
                                     ValueTypeName = TypeName(value)
                                     if (ValueTypeName = ""Object"" or ValueTypeName = ""Unknown"") then
                                         set getCommandResult = value
                                     else
                                         getCommandResult = ""[ScriptObject:"" + ValueTypeName + ""]""
                                     end if
                                 end if
                             elseif IsArray(value) then
                                 getCommandResult = ""[array]""
                             elseif IsNull(value) then
                                 getCommandResult = ""[null]""
                             elseif IsEmpty(value) then
                                 getCommandResult = ""[empty]""
                             else
                                 getCommandResult = CStr(value)
                             end if
                         end function
                         public function invokeConstructor(constructor, args)
                             Err.Raise 445
                         end function
                         public function invokeMethod(target, method, args)
                             Err.Raise 445
                         end function
                     end class
                     set EngineInternal = new EngineInternalImpl
                 ");
        }
        
        #endregion
        
        #region ScriptEngine overrides
        
        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="VBScriptEngine"/> instances return "vbs" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get
            {
                return "vbs";
            }
        }
        
        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// This method is similar to <see cref="ScriptEngine.Evaluate(string)"/> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
        /// <para>
        /// The <see cref="VBScriptEngine"/> version of this method supports both expressions and
        /// statements. If the specified command begins with "eval " (not case-sensitive), the
        /// engine executes the remainder as an expression and attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/0zk841e9(VS.85).aspx">CStr</see>
        /// to convert the result value. Otherwise, it executes the command as a statement and does
        /// not return a value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            var trimmedCommand = command.Trim();
            if (trimmedCommand.StartsWith("eval ", StringComparison.OrdinalIgnoreCase))
            {
                var expression = MiscHelpers.FormatInvariant("EngineInternal.getCommandResult({0})", trimmedCommand.Substring(5));
                return this.GetCommandResultString(this.Evaluate("Expression", true, expression, false));
            }
            
            this.Execute("Command", true, trimmedCommand);
            return null;
        }
        
        internal override IDictionary<int, string> RuntimeErrorMap
        {
            get
            {
                return runtimeErrorMap;
            }
        }
        
        #endregion
    }
}