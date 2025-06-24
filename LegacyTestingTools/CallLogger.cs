using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace LegacyTestingTools
{
    public static class CallLogFormatterContext
    {
        private static readonly ThreadLocal<CallLogger?> _currentCallLogger = new(() => null);
        private static readonly ThreadLocal<string?> _currentMethodName = new(() => null);
        private static readonly ThreadLocal<string[]?> _constructorArgNames = new(() => null);
        
        public static void SetCurrentLogger(CallLogger logger)
        {
            _currentCallLogger.Value = logger;
        }
        
        public static void SetCurrentMethodName(string methodName)
        {
            _currentMethodName.Value = methodName;
        }
        
        public static void ClearCurrentLogger()
        {
            _currentCallLogger.Value = null;
            _currentMethodName.Value = null;
            _constructorArgNames.Value = null;
        }
        
        public static void AddNote(string note)
        {
            _currentCallLogger.Value?.withNote(note);
        }
        
        public static void SetConstructorArguments(params string[] argumentNames)
        {
            _constructorArgNames.Value = argumentNames;
        }
        
        internal static string[]? GetConstructorArguments()
        {
            return _constructorArgNames.Value;
        }
        
        public static void IgnoreCall()
        {
            var methodName = _currentMethodName.Value;
            if (methodName != null && _currentCallLogger.Value != null)
            {
                _currentCallLogger.Value._ignoredCalls.Add(methodName);
            }
        }
        
        public static void IgnoreArgument(int argumentIndex)
        {
            var methodName = _currentMethodName.Value;
            if (methodName != null && _currentCallLogger.Value != null)
            {
                if (!_currentCallLogger.Value._ignoredArguments.ContainsKey(methodName))
                    _currentCallLogger.Value._ignoredArguments[methodName] = new HashSet<int>();
                _currentCallLogger.Value._ignoredArguments[methodName].Add(argumentIndex);
            }
        }
        
        public static void IgnoreAllArguments()
        {
            var methodName = _currentMethodName.Value;
            if (methodName != null && _currentCallLogger.Value != null)
            {
                _currentCallLogger.Value._ignoredAllArguments.Add(methodName);
            }
        }
        
        public static void IgnoreReturnValue()
        {
            var methodName = _currentMethodName.Value;
            if (methodName != null && _currentCallLogger.Value != null)
            {
                _currentCallLogger.Value._ignoredReturnValues.Add(methodName);
            }
        }
    }

    public abstract class CallLogFormatter
    {
        private readonly Dictionary<string, HashSet<int>> _ignoredArguments = new();
        private readonly HashSet<string> _ignoredCalls = new();
        private readonly HashSet<string> _ignoredAllArguments = new();
        private readonly HashSet<string> _ignoredReturnValues = new();
        private readonly Dictionary<string, string> _notes = new();

        protected void IgnoreCall(string methodName)
        {
            _ignoredCalls.Add(methodName);
        }

        protected void IgnoreArgument(string methodName, int argumentIndex)
        {
            if (!_ignoredArguments.ContainsKey(methodName))
                _ignoredArguments[methodName] = new HashSet<int>();
            _ignoredArguments[methodName].Add(argumentIndex);
        }

        protected void IgnoreAllArguments(string methodName)
        {
            _ignoredAllArguments.Add(methodName);
        }

        protected void IgnoreReturnValue(string methodName)
        {
            _ignoredReturnValues.Add(methodName);
        }

        protected void AddNote(string methodName, string note)
        {
            _notes[methodName] = note;
        }

        internal bool ShouldIgnoreCall(string methodName) => _ignoredCalls.Contains(methodName);
        internal bool ShouldIgnoreArgument(string methodName, int index) => _ignoredArguments.ContainsKey(methodName) && _ignoredArguments[methodName].Contains(index);
        internal bool ShouldIgnoreAllArguments(string methodName) => _ignoredAllArguments.Contains(methodName);
        internal bool ShouldIgnoreReturnValue(string methodName) => _ignoredReturnValues.Contains(methodName);
        internal string? GetNote(string methodName) => _notes.TryGetValue(methodName, out var note) ? note : null;
    }

    public class CallLoggerProxy<T> : DispatchProxy, IConstructorCalledWith where T : class
    {
        private T _target = null!;
        private CallLogger _logger = null!;
        private string _emoji = "";
        private CallLogFormatter? _formatter;
        private string? _interfaceName;

        public void ConstructorCalledWith(params object[] args)
        {
            // Log constructor call directly with proper interface name
            var interfaceName = _interfaceName ?? typeof(T).Name;
            if (interfaceName.StartsWith("I") && interfaceName.Length > 1)
            {
                // It's an interface name, use it as-is
            }
            else
            {
                // Find the first interface the target implements
                var interfaces = _target?.GetType().GetInterfaces() ?? typeof(T).GetInterfaces();
                var mainInterface = interfaces.FirstOrDefault(i => i.Name.StartsWith("I") && i != typeof(IConstructorCalledWith));
                interfaceName = mainInterface?.Name ?? typeof(T).Name;
            }

            var callLogger = new CallLogger(_logger._storybook, _emoji);
            callLogger.forInterface(interfaceName);
            
            // Add individual arguments - use names from context if available
            if (args != null)
            {
                var constructorArgNames = CallLogFormatterContext.GetConstructorArguments();
                for (int i = 0; i < args.Length; i++)
                {
                    var argName = (constructorArgNames != null && i < constructorArgNames.Length) 
                        ? constructorArgNames[i] 
                        : $"Arg{i}";
                    callLogger.withArgument(args[i], argName);
                }
            }
            
            callLogger.log("ConstructorCalledWith");
        }


        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null) return null;

            var methodName = targetMethod.Name;
            
            // Check if call should be ignored
            if (_formatter?.ShouldIgnoreCall(methodName) == true)
            {
                return targetMethod.Invoke(_target, args);
            }


            // Create a new logger instance for this call
            var sb = new StringBuilder();
            var callLogger = new CallLogger(sb, _emoji);
            
            // Set current logger context for stubs to access
            CallLogFormatterContext.SetCurrentLogger(callLogger);
            CallLogFormatterContext.SetCurrentMethodName(methodName);
            
            // Add arguments if not ignored
            if (args != null && _formatter?.ShouldIgnoreAllArguments(methodName) != true)
            {
                var parameters = targetMethod.GetParameters();
                for (int i = 0; i < args.Length && i < parameters.Length; i++)
                {
                    if (_formatter?.ShouldIgnoreArgument(methodName, i) != true)
                    {
                        if (parameters[i].IsOut)
                        {
                            callLogger.withArgument("out", parameters[i].Name);
                        }
                        else if (parameters[i].ParameterType.IsByRef)
                        {
                            callLogger.withArgument($"ref {args[i]}", parameters[i].Name);
                        }
                        else
                        {
                            callLogger.withArgument(args[i], parameters[i].Name);
                        }
                    }
                }
            }

            // Add custom note if available
            var note = _formatter?.GetNote(methodName);
            if (note != null)
            {
                callLogger.withNote(note);
            }

            // Invoke the actual method
            object? result;
            try
            {
                result = targetMethod.Invoke(_target, args);
            }
            catch (Exception ex)
            {
                callLogger.withNote($"Exception: {ex.Message}");
                callLogger.log(methodName);
                _logger._storybook.Append(sb.ToString());
                
                // Clear current logger context even on exception
                CallLogFormatterContext.ClearCurrentLogger();
                
                throw;
            }

            // Log return value if not ignored
            if (result != null && _formatter?.ShouldIgnoreReturnValue(methodName) != true)
            {
                callLogger.withReturn(result);
            }

            // Log out parameters
            if (args != null)
            {
                var parameters = targetMethod.GetParameters();
                for (int i = 0; i < args.Length && i < parameters.Length; i++)
                {
                    if (parameters[i].IsOut || parameters[i].ParameterType.IsByRef)
                    {
                        callLogger.withOut(args[i], parameters[i].Name);
                    }
                }
            }

            callLogger.log(methodName);
            _logger._storybook.Append(sb.ToString());
            
            // Clear current logger context
            CallLogFormatterContext.ClearCurrentLogger();
            
            return result;
        }

        public static T Create(T target, CallLogger logger, string emoji)
        {
            var proxy = Create<T, CallLoggerProxy<T>>() as CallLoggerProxy<T>;
            proxy!._target = target;
            proxy._logger = logger;
            proxy._emoji = emoji;
            proxy._formatter = target as CallLogFormatter;
            return (proxy as T)!;
        }
    }

    public class CallLogger
    {
        internal readonly StringBuilder _storybook;
        private readonly string _emoji;
        private object? _returnValue;
        private string? _note;
        private readonly List<(string name, object? value, string emoji)> _parameters = new();
        private string? _methodName;
        private string? _forcedInterfaceName;
        
        // Internal fields for ignored calls/arguments/returns (used by CallLogFormatterContext)
        internal readonly Dictionary<string, HashSet<int>> _ignoredArguments = new();
        internal readonly HashSet<string> _ignoredCalls = new();
        internal readonly HashSet<string> _ignoredAllArguments = new();
        internal readonly HashSet<string> _ignoredReturnValues = new();

        public CallLogger(StringBuilder storybook, string emoji = "")
        {
            _storybook = storybook;
            _emoji = emoji;
        }

        public T Wrap<T>(T target, string emoji = "🔧") where T : class
        {
            return CallLoggerProxy<T>.Create(target, this, emoji);
        }

        public T WrapClass<T>(T target, string emoji = "🔧") where T : class
        {
            return CallLoggerProxy<T>.Create(target, this, emoji);
        }

        public CallLogger withReturn(object? returnValue, string? description = null)
        {
            _returnValue = returnValue;
            return this;
        }

        public CallLogger withNote(string note)
        {
            _note = note;
            return this;
        }

        public CallLogger withArgument(object? value, string? name = null)
        {
            var paramName = name ?? $"Arg{_parameters.Count}";
            _parameters.Add((paramName, value, "🔸"));
            return this;
        }

        public CallLogger withOut(object? value, string? name = null)
        {
            var paramName = name ?? $"Out{_parameters.Count}";
            _parameters.Add((paramName, value, "♦️"));
            return this;
        }

        public CallLogger forInterface(string interfaceName)
        {
            _forcedInterfaceName = interfaceName;
            return this;
        }

        public void log([CallerMemberName] string? methodName = null)
        {
            _methodName = methodName;
            
            if (_methodName == "ConstructorCalledWith")
            {
                LogConstructorCall();
            }
            else
            {
                LogMethodCall();
            }
            
            // Clear parameters for next call
            _parameters.Clear();
            _returnValue = null;
            _note = null;
            _forcedInterfaceName = null;
        }

        private void LogConstructorCall()
        {
            var interfaceName = _forcedInterfaceName ?? GetInterfaceName();
            _storybook.AppendLine($"{_emoji} {interfaceName} constructor called with:");
            
            foreach (var (name, value, emoji) in _parameters)
            {
                _storybook.AppendLine($"  {emoji} {name}: {value}");
            }
            
            _storybook.AppendLine();
        }

        private void LogMethodCall()
        {
            // Use detailed format for all calls
            _storybook.AppendLine($"{_emoji} {_methodName}:");
            
            foreach (var (name, value, emoji) in _parameters)
            {
                var formattedValue = FormatValue(value);
                _storybook.AppendLine($"  {emoji} {name}: {formattedValue}");
            }

            if (!string.IsNullOrEmpty(_note))
            {
                _storybook.AppendLine($"  🗒️ {_note}");
            }

            if (_returnValue != null)
            {
                var formattedReturn = FormatValue(_returnValue);
                _storybook.AppendLine($"  🔹 Returns: {formattedReturn}");
            }

            _storybook.AppendLine();
        }

        private void CaptureMethodParameters(object?[] args)
        {
            var frame = new System.Diagnostics.StackFrame(3, false);
            var method = frame.GetMethod();
            
            if (method != null)
            {
                var parameters = method.GetParameters();
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i < args.Length)
                    {
                        _parameters.Add((parameters[i].Name ?? $"param{i}", args[i], "🔸"));
                    }
                    else
                    {
                        _parameters.Add((parameters[i].Name ?? $"param{i}", GetDefaultValue(parameters[i].ParameterType), "🔸"));
                    }
                }
            }
        }

        private string GetInterfaceName()
        {
            var frame = new System.Diagnostics.StackFrame(3, false);
            var method = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            
            if (declaringType != null)
            {
                var interfaces = declaringType.GetInterfaces();
                var mainInterface = interfaces.FirstOrDefault(i => i.Name.StartsWith("I") && i != typeof(IConstructorCalledWith));
                return mainInterface?.Name ?? declaringType.Name;
            }
            
            return "Unknown";
        }


        private object?[] GetMethodArgumentValues(MethodBase method)
        {
            // This is also simplified - real implementation would need stack inspection
            // or other techniques to get actual parameter values
            var parameters = method.GetParameters();
            return parameters.Select(p => GetDefaultValue(p.ParameterType)).ToArray();
        }

        private object? GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private string FormatValue(object? value)
        {
            if (value == null) return "null";
            
            // Handle collections (Lists, Arrays, etc.)
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                {
                    items.Add(FormatValue(item));
                }
                return string.Join(",", items);
            }
            
            // Handle decimals with invariant culture
            if (value is decimal dec)
            {
                return dec.ToString(CultureInfo.InvariantCulture);
            }
            
            // Handle dates with format matching verified.txt
            if (value is DateTime dt)
            {
                return dt.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            
            // Handle other numeric types with invariant culture
            if (value is double d)
            {
                return d.ToString(CultureInfo.InvariantCulture);
            }
            
            if (value is float f)
            {
                return f.ToString(CultureInfo.InvariantCulture);
            }
            
            return value.ToString() ?? "null";
        }
    }
}