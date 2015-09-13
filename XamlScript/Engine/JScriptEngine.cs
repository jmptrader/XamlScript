using System;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents an instance of the JScript engine.
    /// </summary>
    public class JScriptEngine : WindowsScriptEngine
    {
        #region data
        
        private static readonly Dictionary<int, string> runtimeErrorMap = new Dictionary<int, string>
        {
            // http://msdn.microsoft.com/en-us/library/1dk3k160(VS.84).aspx
            { 5029, "Array length must be a finite positive integer" },
            { 5030, "Array length must be assigned a finite positive number" },
            { 5028, "Array or arguments object expected" },
            { 5010, "Boolean expected" },
            { 5003, "Cannot assign to a function result" },
            { 5000, "Cannot assign to 'this'" },
            { 5034, "Circular reference in value argument not supported" },
            { 5006, "Date object expected" },
            { 5015, "Enumerator object expected" },
            { 5022, "Exception thrown and not caught" },
            { 5020, "Expected ')' in regular expression" },
            { 5019, "Expected ']' in regular expression" },
            { 5023, "Function does not have a valid prototype object" },
            { 5002, "Function expected" },
            { 5008, "Illegal assignment" },
            { 5021, "Invalid range in character set" },
            { 5035, "Invalid replacer argument" },
            { 5014, "JScript object expected" },
            { 5001, "Number expected" },
            { 5007, "Object expected" },
            { 5012, "Object member expected" },
            { 5016, "Regular Expression object expected" },
            { 5005, "String expected" },
            { 5017, "Syntax error in regular expression" },
            { 5026, "The number of fractional digits is out of range" },
            { 5027, "The precision is out of range" },
            { 5025, "The URI to be decoded is not a valid encoding" },
            { 5024, "The URI to be encoded contains an invalid character" },
            { 5009, "Undefined identifier" },
            { 5018, "Unexpected quantifier" },
            { 5013, "VBArray expected" }
        };
        
        #endregion
        
        #region constructors
        
        /// <summary>
        /// Initializes a new JScript engine instance.
        /// </summary>
        public JScriptEngine() : this(null)
        {
        }
        
        /// <summary>
        /// Initializes a new JScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public JScriptEngine(string name) : this(name, WindowsScriptEngineFlags.None)
        {
        }
        
        /// <summary>
        /// Initializes a new JScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(WindowsScriptEngineFlags flags) : this(null, flags)
        {
        }
        
        /// <summary>
        /// Initializes a new JScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(string name, WindowsScriptEngineFlags flags) : this("JScript", name, flags)
        {
        }
        
        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected JScriptEngine(string progID, string name, WindowsScriptEngineFlags flags) : base(progID, name, flags)
        {
            this.Execute(
MiscHelpers.FormatInvariant("{0} [internal]", this.GetType().Name),
                @"
                     EngineInternal = (function () {

                         function convertArgs(args) {
                             var result = [];
                             var count = args.Length;
                             for (var i = 0; i < count; i++) {
                                 result.push(args[i]);
                             }
                             return result;
                         }

                         function construct(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) {
                             return new this(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                         }

                         return {

                             getCommandResult: function (value) {
                                 if (value != null) {
                                     if ((typeof(value) == 'object') || (typeof(value) == 'function')) {
                                         if (typeof(value.toString) == 'function') {
                                             return value.toString();
                                         }
                                     }
                                 }
                                 return value;
                             },

                             invokeConstructor: function (constructor, args) {
                                 if (typeof(constructor) != 'function') {
                                     throw new Error('Function expected');
                                 }
                                 return construct.apply(constructor, convertArgs(args));
                             },

                             invokeMethod: function (target, method, args) {
                                 if (typeof(method) != 'function') {
                                     throw new Error('Function expected');
                                 }
                                 return method.apply(target, convertArgs(args));
                             }
                         };
                     })();
                 ");
        }
        
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
        
        #region ScriptEngine overrides
        
        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="JScriptEngine"/> instances return "js" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get
            {
                return "js";
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
        /// The <see cref="JScriptEngine"/> version of this method attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/k6xhc6yc(VS.85).aspx">toString</see>
        /// to convert the return value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            this.Script.EngineInternal.command = command;
            return base.ExecuteCommand("EngineInternal.getCommandResult(eval(EngineInternal.command))");
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