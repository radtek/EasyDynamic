using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;

namespace EasyDynamicObject
{

    public class TypeClassMapping
    {
        public Type Class;
        public MethodInfo Method;
    }

    public class EasyObject : DynamicObject
    {
        public Dictionary<string, object> Properties = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        public static Dictionary<string, TypeClassMapping> Methods = new Dictionary<string, TypeClassMapping>(StringComparer.CurrentCultureIgnoreCase);

        public static void LoadCustomFunctions()
        {
            ExtractMethodsFromFile(Path.Combine(Environment.CurrentDirectory, "Plugins"));
            ExtractMethodsFromFile(Path.Combine(Environment.CurrentDirectory,"Extensions"));
        }

        private static void ExtractMethodsFromFile(string dllPath)
        {
            if (!Directory.Exists(dllPath)) return;
            foreach (string dllFileName in Directory.GetFiles(dllPath, "*.dll"))
            {
                Assembly asm = Assembly.LoadFile(dllFileName);
                foreach (Type type in asm.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        foreach (MethodInfo mi in type.GetMethods())
                        {
                            if (!mi.IsAbstract && !mi.IsConstructor && mi.IsPublic && (mi.DeclaringType == type))
                            {
                                if (!Methods.ContainsKey(mi.Name))
                                {
                                    Methods.Add(mi.Name, new TypeClassMapping() { Class = type, Method = mi });
                                }
                                Methods.Add(type.Name + "_" + mi.Name, new TypeClassMapping() { Class = type, Method = mi });
                            }
                        }
                    }
                }

            }
        }


        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string keyName = binder.Name;
            if (Properties.ContainsKey(keyName))
            {
                result = Properties[keyName];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string keyName = binder.Name;
            if (Properties.ContainsKey(binder.Name)) Properties.Remove(keyName);
            Properties.Add(keyName, value);
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (Properties.ContainsKey(binder.Name)) Properties.Remove(binder.Name);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            string operationName = binder.Name;
            result = null;
            switch (operationName.ToUpper())
            {
                case "LOADCUSTOMFUNCTIONS":
                    LoadCustomFunctions();
                    break;
                default:
                    result = ExecuteMethod(operationName, args);
                    break;
            }
            return true;
        }

        public static object ExecuteMethod(string methodName, params object[] args)
        {
            object result = null;
            if (Methods.ContainsKey(methodName))
            {
                for (int i=0; i< args.Length; i++)
                {
                    args[i] = EvaluateString(args[i].ToString());
                }

                TypeClassMapping tcm = Methods[methodName];
                int methodParamLength = tcm.Method.GetParameters().Length;
                if ((methodParamLength < args.Length) && (tcm.Method.GetParameters()[methodParamLength - 1].ParameterType.IsArray))
                {
                    int paramLength = args.Length - methodParamLength + 1;
                    Type argType = Type.GetType(tcm.Method.GetParameters()[methodParamLength - 1].ParameterType.FullName).GetElementType();
                    dynamic paramObject = Array.CreateInstance(argType,paramLength);

                    for (int i = 0; i < paramLength; i++)
                    {
                        paramObject[i] =  Convert.ChangeType(args[methodParamLength - 1 + i], argType);
                    }
                        //Array.Copy(args, tcm.Method.GetParameters().Length - 1, paramObject, 0, paramLength);
                    Array.Resize(ref args, tcm.Method.GetParameters().Length);
                    args[args.Length -1] = paramObject;
                }
                if (args.Length != tcm.Method.GetParameters().Length)
                {
                    return false;
                }
                if (tcm.Method.IsStatic)
                {
                    result = tcm.Method.Invoke(null, args);
                }
                else
                {
                    result = tcm.Method.Invoke(Activator.CreateInstance(tcm.Class, null), args);
                }
            }
            return result;
        }


        public static object ExecuteString(string command)
        {
            if (String.IsNullOrWhiteSpace(command)) return false;
            string commandPattern = @"@(?'methodname' [a-zA-Z]\w*)
                                      \( 
                                        (?'params'
                                          (?> [^()]* | \( (?<DEPTH>) | \) (?<-DEPTH>) )*
                                          (?(DEPTH)(?!))
                                        ) 
                                      \)";
            object commandResult = String.Empty;

            while (Regex.IsMatch(command, commandPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                Match commandMatch = Regex.Match(command, commandPattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                string methodName = commandMatch.Groups["methodname"].Value;
                object[] methodParams = new object[] { };
                if (!String.IsNullOrWhiteSpace(commandMatch.Groups["params"].Value))
                {
                    methodParams = commandMatch.Groups["params"].Value.Split(',');
                }
                commandResult = ExecuteMethod(methodName, methodParams);
                command = command.Replace(commandMatch.Value, commandResult.ToString());
            }
            return EvaluateString(command);
        }

        [STAThread]
        public static string EvaluateString(string strToEvaluate, string providerName = "CSharp")
        {
            try
            {
                CodeDomProvider codeProvider = CodeDomProvider.CreateProvider(providerName);
                CompilerParameters cp = new CompilerParameters();

                string className = "Class" + System.Guid.NewGuid().ToString().Replace("-", "");

                cp.ReferencedAssemblies.Add("system.dll");
                cp.CompilerOptions = "/t:library";
                cp.GenerateInMemory = true;

                StringBuilder sb = new StringBuilder("");
                sb.Append("using System;\n");

                sb.Append("namespace EasyObject.Evaluator { \n");
                sb.Append("public class " + className + "{ \n");
                sb.Append("public object Evaluate(){\n");
                sb.Append("return " + strToEvaluate + "; \n");
                sb.Append("} \n");
                sb.Append("} \n");
                sb.Append("}\n");

                CompilerResults cr = codeProvider.CompileAssemblyFromSource(cp, sb.ToString());
                if (cr.Errors.Count > 0)
                {
                    return strToEvaluate;
                }

                System.Reflection.Assembly a = cr.CompiledAssembly;
                object o = a.CreateInstance("EasyObject.Evaluator." + className);
                Type t = o.GetType();
                MethodInfo mi = t.GetMethod("Evaluate");

                object s = mi.Invoke(o, null);
                return s.ToString();
            }
            catch
            {
                return strToEvaluate;
            }
        }
    }
}
