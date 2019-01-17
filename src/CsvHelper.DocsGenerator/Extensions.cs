﻿using CsvHelper.DocsGenerator.Formatters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CsvHelper.DocsGenerator
{
    public static class Extensions
    {
		// Assembly

		public static string GetHtmlName(Assembly assembly)
		{
			throw new NotImplementedException();
		}

		// Type

		public static string GetTypeName(this Type type)
		{
			if (type.IsEnum)
			{
				return "Enum";
			}

			if (type.IsInterface)
			{
				return "Interface";
			}

			if (type.IsClass)
			{
				return  "Class";
			}

			throw new InvalidOperationException($"No type name found for type '{type.GetFullName()}'.");
		}

		public static string GetHtmlName(this Type type)
		{
			return HtmlFormat(type);
		}

		public static string GetCodeName(this Type type)
		{
			return HtmlFormat(type, isCodeBlock: true);
		}

		public static string GetFullName(this Type type)
		{
			return $"{type.Namespace}.{type.Name}";
		}

		public static string GetFullHtmlName(this Type type)
		{
			return $"{type.Namespace}.{type.GetHtmlName()}";
		}

		public static string GetFullCodeName(this Type type)
		{
			return $"{type.Namespace}.{type.GetCodeName()}";
		}

		public static string GetSummary(this Type type)
		{
			return GetSummary($"T:{type.GetFullName()}");
		}

		// Property

		public static string GetHtmlName(this PropertyInfo property)
		{
			return property.Name;
		}

		public static string GetCodeName(this PropertyInfo property)
		{
			throw new NotImplementedException();
		}

		public static string GetFullName(this PropertyInfo property)
		{
			return $"{property.DeclaringType.FullName}.{property.Name}";
		}

		public static string GetFullHtmlName(this PropertyInfo property)
		{
			throw new NotImplementedException();
		}

		public static string GetFullCodeName(this PropertyInfo property)
		{
			throw new NotImplementedException();
		}

		public static string GetSummary(this PropertyInfo property)
		{
			var parameters = property.GetIndexParameters().ToList();
			var parametersText = string.Empty;
			if (parameters.Count > 0)
			{
				parametersText = $"({string.Join(",", parameters.Select(p => p.ParameterType.FullName))})";
			}

			return GetSummary($"P:{property.GetFullName()}{parametersText}");
		}

		// Field

		public static string GetHtmlName(this FieldInfo field)
		{
			return field.Name;
		}

		public static string GetCodeName(this FieldInfo field)
		{
			return field.Name;
		}

		public static string GetFullName(this FieldInfo field)
		{
			return $"{field.DeclaringType.FullName}.{field.Name}";
		}

		public static string GetFullHtmlName(this FieldInfo field)
		{
			throw new NotImplementedException();
		}

		public static string GetFullCodeName(this FieldInfo field)
		{
			throw new NotImplementedException();
		}

		public static string GetSummary(this FieldInfo field)
		{
			return GetSummary($"F:{field.GetFullName()}");
		}
		
		// Constructor

		public static string GetHtmlName(this ConstructorInfo constructor)
		{
			return HtmlFormat(constructor);
		}

		public static string GetCodeName(this ConstructorInfo constructor)
		{
			throw new NotImplementedException();
		}

		public static string GetFullName(this ConstructorInfo constructor)
		{
			throw new NotImplementedException();
		}

		public static string GetFullHtmlName(this ConstructorInfo constructor)
		{
			throw new NotImplementedException();
		}

		public static string GetFullCodeName(this ConstructorInfo constructor)
		{
			throw new NotImplementedException();
		}

		public static string GetSummary(this ConstructorInfo constructor)
		{
			return GetSummary($"M:{XmlDocFormat(constructor)}");
		}

		// Method

		public static string GetHtmlName(this MethodInfo method)
		{
			return HtmlFormat(method);
		}

		public static string GetCodeName(this MethodInfo method)
		{
			throw new NotImplementedException();
		}

		public static string GetFullName(this MethodInfo method)
		{
			throw new NotImplementedException();
		}

		public static string GetFullHtmlName(this MethodInfo method)
		{
			throw new NotImplementedException();
		}

		public static string GetFullCodeName(this MethodInfo method)
		{
			throw new NotImplementedException();
		}

		public static string GetSummary(this MethodInfo method)
		{
			return GetSummary($"M:{XmlDocFormat(method)}");
		}
			   
		// Private

		private static Type GetType(string typeName)
		{
			Type type = null;
			if (typeName.StartsWith("CsvHelper"))
			{
				var assembly = Assembly.GetAssembly(typeof(CsvHelperException));
				type = assembly.GetType(typeName);
			}
			else
			{
				type = Type.GetType(typeName);
			}

			return type;
		}

		private static string GetSummary(string memberName)
		{
			var members = XmlDocs.XElement.Descendants("member");
			var member = members?.SingleOrDefault(m => m.Attribute("name")?.Value == memberName);
			var summary = member?.Element("summary");
			if (summary != null)
			{
				var summaryText = new List<string>();
				foreach (var node in summary.Nodes())
				{
					string text;
					if (node.NodeType == XmlNodeType.Element)
					{
						var el = (XElement)node;
						switch (el.Name.ToString())
						{
							case "paramref":
								text = el.Attribute("name").Value;
								break;
							case "see":
								var typeName = el.Attribute("cref").Value.Substring(2);
								text = typeName;
								//var type = GetType(typeName);
								//text = type == null ? typeName : type.GetFullHtmlName();
								break;
							case "c":
								text = el.Value;
								break;
							default:
								throw new InvalidOperationException($"Unhandled element '{el.Name}'.");
						}

						text = $"``{text.Trim()}``";
					}
					else if (node.NodeType == XmlNodeType.Text)
					{
						text = node.ToString();
					}
					else
					{
						throw new InvalidOperationException($"Unhandled node type '{node.NodeType}'.");
					}

					text = Regex.Replace(text, @"\s{2,}", " ").Trim();

					summaryText.Add(text);
				}

				return string.Join(" ", summaryText);
			}

			if (memberName.Substring(2).StartsWith("CsvHelper"))
			{
				Console.WriteLine($"No summary found for '{memberName}'.");
			}

			return null;

		}

		private static string HtmlFormat(Type type, bool generateLinks = false, bool isCodeBlock = false)
		{
			var symbols = isCodeBlock ? Symbols.Code : Symbols.Html;

			var @namespace = type.Namespace;
			var name = type.Name;

			if (type.IsByRef)
			{
				name = name.TrimEnd('&');
			}

			if (generateLinks)
			{
				if (@namespace.StartsWith("CsvHelper"))
				{
					name = $"[{name}](/api/{@namespace}/{name})";
				}
				else
				{
					name = $"[{name}](https://docs.microsoft.com/en-us/dotnet/api/{@namespace.ToLower()}.{name.ToLower()})";
				}
			}

			var genericArgumentsText = string.Empty;
			var genericArguments = type.GetGenericArguments().ToList();
			if (genericArguments.Count > 0)
			{
				name = name.Substring(0, name.IndexOf('`'));
				genericArgumentsText = $"{symbols["<"]}{string.Join(", ", genericArguments.Select(a => HtmlFormat(a)))}{symbols[">"]}";
			}

			return $"{name}{genericArgumentsText}";
		}

		private static string HtmlFormat(MethodBase methodInfo, bool generateLinks = false, bool isCodeBlock = false)
		{
			var symbols = isCodeBlock ? Symbols.Code : Symbols.Html;

			var @namespace = methodInfo.DeclaringType.Namespace;
			var typeName = methodInfo.DeclaringType.Name;
			var methodName = methodInfo.Name;

			var name = methodName;
			if (methodInfo.IsConstructor)
			{
				name = methodInfo.DeclaringType.IsGenericType
					? typeName.Substring(0, typeName.IndexOf('`'))
					: typeName;
			}

			var genericArgumentsText = string.Empty;
			var genericArguments = new List<Type>();
			if (!methodInfo.IsConstructor)
			{
				genericArguments = methodInfo.GetGenericArguments().ToList();
				if (genericArguments.Count > 0)
				{
					genericArgumentsText = $"{symbols["<"]}{string.Join(", ", genericArguments.Select(a => HtmlFormat(a)))}{symbols[">"]}";
				}
			}

			var parametersText = string.Empty;
			var parameters = methodInfo.GetParameters().ToList();
			if (parameters.Count > 0)
			{
				var typeGenericArguments = methodInfo.DeclaringType.GetGenericArguments();

				parametersText = string.Join(", ", parameters.Select(p =>
				{
					// Don't generate links if the type is a generic parameter.
					var shouldGenerateLinks = generateLinks &&
					!(
						typeGenericArguments.Any(a => $"{a.Namespace}.{a.Name}" == $"{p.ParameterType.Namespace}.{p.ParameterType.Name}") ||
						genericArguments.Any(a => $"{a.Namespace}.{a.Name}" == $"{p.ParameterType.Namespace}.{p.ParameterType.Name}")
					);

					var outText = p.IsOut ? "out " : string.Empty;
					return $"{outText}{HtmlFormat(p.ParameterType, shouldGenerateLinks)}";
				}));
			}

			return $"{name}{genericArgumentsText}({parametersText})";
		}

		private static string XmlDocFormat(Type type)
		{
			var @namespace = type.Namespace;
			var name = type.Name;

			return $"{@namespace}.{name}";
		}

		private static string XmlDocFormat(MethodBase methodInfo)
		{
			var typeText = XmlDocFormat(methodInfo.DeclaringType);

			var methodName = methodInfo.Name.Replace('.', '#');

			var typeGenericArguments = methodInfo.DeclaringType.GetGenericArguments().ToList();

			var methodGenericArguments = new List<Type>();
			if (!methodInfo.IsConstructor)
			{
				methodGenericArguments = methodInfo.GetGenericArguments().ToList();
				if (methodGenericArguments.Count > 0)
				{
					methodName = $"{methodName}``{methodGenericArguments.Count}";
				}
			}

			var parametersText = string.Empty;
			var parameters = methodInfo.GetParameters().ToList();
			if (parameters.Count > 0)
			{
				parametersText = $"({string.Join(",", parameters.Select(p => XmlDocFormat(p.ParameterType, typeGenericArguments, methodGenericArguments)))})";
			}

			return $"{typeText}.{methodName}{parametersText}";
		}

		private static string XmlDocFormat(Type parameterType, List<Type> typeGenericParameters, List<Type> methodGenericParameters)
		{
			var @namespace = parameterType.Namespace;
			var name = parameterType.Name;

			if (parameterType.IsByRef)
			{
				name = name.TrimEnd('&');
			}

			var typeName = $"{@namespace}.{name}";

			// Check if the parameter is a generic argument of a type.
			var index = typeGenericParameters.FindIndex(t => $"{t.Namespace}.{t.Name}" == typeName);
			if (index >= 0)
			{
				var refText = parameterType.IsByRef ? "@" : string.Empty;
				return $"`{index}{refText}";
			}

			// Check if the parameter is a generic argument of a method.
			index = methodGenericParameters.FindIndex(t => $"{t.Namespace}.{t.Name}" == typeName);
			if (index >= 0)
			{
				var refText = parameterType.IsByRef ? "@" : string.Empty;
				return $"``{index}{refText}";
			}

			var genericArgumentsText = string.Empty;
			var genericArguments = parameterType.GetGenericArguments().ToList();
			if (genericArguments.Count > 0)
			{
				name = name.Substring(0, name.IndexOf('`'));
				genericArgumentsText = $"{{{string.Join(",", genericArguments.Select(a => XmlDocFormat(a, typeGenericParameters, methodGenericParameters)))}}}";
			}

			if (parameterType.IsByRef)
			{
				name += "@";
			}

			return $"{@namespace}.{name}{genericArgumentsText}";
		}
	}
}

//method.Name
//".ctor"
//method.DeclaringType.Name
//"Factory"
//method.DeclaringType.Namespace
//"CsvHelper"
//method.DeclaringType.FullName
//"CsvHelper.Factory"
