﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Rocks.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static Rocks.Extensions.MethodBaseExtensions;
using static Rocks.Extensions.MethodInfoExtensions;
using static Rocks.Extensions.PropertyInfoExtensions;
using static Rocks.Extensions.TypeExtensions;

namespace Rocks.Construction
{
	internal abstract class Builder
	{
		internal Builder(Type baseType,
			ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>> handlers,
			SortedSet<string> namespaces, Options options)
		{
			this.BaseType = baseType;
			this.IsUnsafe = this.BaseType.IsUnsafe();
			this.Handlers = handlers;
			this.Namespaces = namespaces;
			this.Options = options;
		}

		internal virtual void Build()
		{
			this.Tree = this.MakeTree();
		}

		private List<string> GetGeneratedConstructors()
		{
			var generatedConstructors = new List<string>();
			var constructorName = this.GetTypeNameWithNoGenerics();

			foreach (var constructor in this.BaseType.GetConstructors(ReflectionValues.PublicInstance))
			{
				var parameters = constructor.GetParameters();

				if (parameters.Length > 0)
				{
					generatedConstructors.Add(CodeTemplates.GetConstructorTemplate(
						constructorName, constructor.GetArgumentNameList(), constructor.GetParameters(this.Namespaces)));
				}
			}

			return generatedConstructors;
		}

		private List<string> GetGeneratedMethods()
		{
			var generatedMethods = new List<string>();

			foreach (var baseMethod in this.BaseType.GetMethods(ReflectionValues.PublicNonPublicInstance)
				.Where(_ => !_.IsSpecialName && _.IsVirtual && !_.IsFinal))
			{
				var methodInformation = this.GetMethodInformation(baseMethod);
				var argumentNameList = baseMethod.GetArgumentNameList();
				var outInitializers = !methodInformation.ContainsRefAndOrOutParameters ? string.Empty : baseMethod.GetOutInitializers();

				if (baseMethod.IsPublic)
				{
					// Either the base method contains no refs/outs, or the user specified a delegate
					// to use to handle that method (remember, types with methods with refs/outs are gen'd
					// each time, and that's the only reason the handlers are passed in.
					if (!methodInformation.ContainsRefAndOrOutParameters || !string.IsNullOrWhiteSpace(methodInformation.DelegateCast))
					{
						if (!methodInformation.ContainsRefAndOrOutParameters && baseMethod.GetParameters().Length > 0)
						{
							generatedMethods.Add(this.GenerateMethodWithNoRefOutParameters(
								baseMethod, methodInformation.DelegateCast, argumentNameList, outInitializers, methodInformation.DescriptionWithOverride));
						}
						else
						{
							generatedMethods.Add(this.GenerateMethodWithRefOutOrNoParameters(
								baseMethod, methodInformation.DelegateCast, argumentNameList, outInitializers, methodInformation.DescriptionWithOverride));

							if (methodInformation.ContainsRefAndOrOutParameters)
							{
								this.HandleRefOutMethod(baseMethod, methodInformation);
							}
						}
					}
					else
					{
						generatedMethods.Add(CodeTemplates.GetRefOutNotImplementedMethodTemplate(methodInformation.DescriptionWithOverride));
					}
				}
				else if (!baseMethod.IsPrivate && baseMethod.IsAbstract)
				{
					var visibility = baseMethod.IsFamily ? CodeTemplates.Protected : CodeTemplates.Internal;

					generatedMethods.Add(baseMethod.ReturnType != typeof(void) ?
						CodeTemplates.GetNonPublicFunctionImplementationTemplate(visibility, methodInformation.Description,
							outInitializers, $"{baseMethod.ReturnType.GetSafeName()}{baseMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}") :
						CodeTemplates.GetNonPublicActionImplementationTemplate(visibility, methodInformation.Description,
							outInitializers));
				}
			}

			return generatedMethods;
		}

		protected class MethodInformation
		{
			public bool ContainsRefAndOrOutParameters { get; set; }
			public string DelegateCast { get; set; }
			public string Description { get; set; }
			public string DescriptionWithOverride { get; set; }
		}

		protected abstract MethodInformation GetMethodInformation(MethodInfo baseMethod);

		private string GenerateMethodWithNoRefOutParameters(MethodInfo baseMethod, string delegateCast, string argumentNameList, string outInitializers, string methodDescriptionWithOverride)
		{
			var expectationChecks = baseMethod.GetExpectationChecks();
			var expectationExceptionMessage = baseMethod.GetExpectationExceptionMessage();

			if (baseMethod.ReturnType != typeof(void))
			{
				return baseMethod.ReturnType.IsValueType ||
					(baseMethod.ReturnType.IsGenericParameter && (baseMethod.ReturnType.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0) ?
						CodeTemplates.GetFunctionWithValueTypeReturnValueMethodTemplate(
							baseMethod.MetadataToken, argumentNameList, $"{baseMethod.ReturnType.GetSafeName()}{baseMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", 
							expectationChecks, delegateCast, outInitializers, expectationExceptionMessage, methodDescriptionWithOverride) :
						CodeTemplates.GetFunctionWithReferenceTypeReturnValueMethodTemplate(
							baseMethod.MetadataToken, argumentNameList, $"{baseMethod.ReturnType.GetSafeName()}{baseMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", 
							expectationChecks, delegateCast, outInitializers, expectationExceptionMessage, methodDescriptionWithOverride);
			}
			else
			{
				return CodeTemplates.GetActionMethodTemplate(
					baseMethod.MetadataToken, argumentNameList, expectationChecks, delegateCast, outInitializers, expectationExceptionMessage, methodDescriptionWithOverride);
			}
		}

		protected virtual void HandleRefOutMethod(MethodInfo baseMethod, MethodInformation methodDescription) { }

		private string GenerateMethodWithRefOutOrNoParameters(MethodInfo baseMethod, string delegateCast, string argumentNameList, string outInitializers, string methodDescriptionWithOverride)
		{
			if (baseMethod.ReturnType != typeof(void))
			{
				return baseMethod.ReturnType.IsValueType ||
					(baseMethod.ReturnType.IsGenericParameter && (baseMethod.ReturnType.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0) ?
						CodeTemplates.GetFunctionWithValueTypeReturnValueAndNoArgumentsMethodTemplate(
							baseMethod.MetadataToken, argumentNameList, $"{baseMethod.ReturnType.GetSafeName()}{baseMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}",
							delegateCast, outInitializers, methodDescriptionWithOverride) :
						CodeTemplates.GetFunctionWithReferenceTypeReturnValueAndNoArgumentsMethodTemplate(
							baseMethod.MetadataToken, argumentNameList, $"{baseMethod.ReturnType.GetSafeName()}{baseMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}",
							delegateCast, outInitializers, methodDescriptionWithOverride);
			}
			else
			{
				return CodeTemplates.GetActionMethodWithNoArgumentsTemplate(
					baseMethod.MetadataToken, argumentNameList, delegateCast, outInitializers, methodDescriptionWithOverride);
			}
		}

		private List<string> GetGeneratedEvents()
		{
			var generatedEvents = new List<string>();

			foreach (var @event in this.BaseType.GetEvents(ReflectionValues.PublicNonPublicInstance))
			{
				var eventHandlerType = @event.EventHandlerType;
				this.Namespaces.Add(eventHandlerType.Namespace);
				var eventMethod = @event.AddMethod;
				var methodInformation = this.GetMethodInformation(eventMethod);
				var @override = methodInformation.DescriptionWithOverride.Contains("override") ? "override " : string.Empty;

				if (eventMethod.IsPublic)
				{
					if (eventHandlerType.IsGenericType)
					{
						var eventGenericType = eventHandlerType.GetGenericArguments()[0];
						generatedEvents.Add(CodeTemplates.GetEventTemplate(@override, 
                     $"EventHandler<{eventGenericType.GetSafeName()}>", @event.Name));
						this.Namespaces.Add(eventGenericType.Namespace);
					}
					else
					{
						generatedEvents.Add(CodeTemplates.GetEventTemplate(@override, 
                     eventHandlerType.GetSafeName(), @event.Name));
					}
				}
				else if (!eventMethod.IsPrivate && eventMethod.IsAbstract)
				{
					var visibility = eventMethod.IsFamily ? CodeTemplates.Protected : CodeTemplates.Internal;

					if (eventHandlerType.IsGenericType)
					{
						var eventGenericType = eventHandlerType.GetGenericArguments()[0];
						generatedEvents.Add(CodeTemplates.GetNonPublicEventTemplate(visibility,
							$"EventHandler<{eventGenericType.GetSafeName()}>", @event.Name));
						this.Namespaces.Add(eventGenericType.Namespace);
					}
					else
					{
						generatedEvents.Add(CodeTemplates.GetNonPublicEventTemplate(visibility,
							eventHandlerType.GetSafeName(), @event.Name));
					}
				}
			}

			return generatedEvents;
		}

		private List<string> GetGeneratedProperties()
		{
			var generatedProperties = new List<string>();

			foreach (var baseProperty in this.BaseType.GetProperties(ReflectionValues.PublicNonPublicInstance)
				.Where(_ => _.GetDefaultMethod().IsVirtual && !_.GetDefaultMethod().IsFinal))
			{
				this.Namespaces.Add(baseProperty.PropertyType.Namespace);
				var indexers = baseProperty.GetIndexParameters();
				var propertyMethod = (baseProperty.CanRead ? baseProperty.GetMethod : baseProperty.SetMethod);
				var methodInformation = this.GetMethodInformation(propertyMethod);
				var @override = methodInformation.DescriptionWithOverride.Contains("override") ? "override " : string.Empty;

				if (propertyMethod.IsPublic)
				{
					var propertyImplementations = new List<string>();

					if (baseProperty.CanRead)
					{
						var getMethod = baseProperty.GetMethod;
						var getArgumentNameList = getMethod.GetArgumentNameList();
						var getDelegateCast = getMethod.GetDelegateCast();

						if (getMethod.GetParameters().Length > 0)
						{
							var getExpectationChecks = getMethod.GetExpectationChecks();
							var getExpectationExceptionMessage = getMethod.GetExpectationExceptionMessage();
							propertyImplementations.Add(getMethod.ReturnType.IsValueType ?
								CodeTemplates.GetPropertyGetWithValueTypeReturnValueTemplate(
									getMethod.MetadataToken, getArgumentNameList, $"{getMethod.ReturnType.GetSafeName()}{getMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", 
									getExpectationChecks, getDelegateCast, getExpectationExceptionMessage) :
								CodeTemplates.GetPropertyGetWithReferenceTypeReturnValueTemplate(
									getMethod.MetadataToken, getArgumentNameList, $"{getMethod.ReturnType.GetSafeName()}{getMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", 
									getExpectationChecks, getDelegateCast, getExpectationExceptionMessage));
						}
						else
						{
							propertyImplementations.Add(getMethod.ReturnType.IsValueType ?
								CodeTemplates.GetPropertyGetWithValueTypeReturnValueAndNoIndexersTemplate(
									getMethod.MetadataToken, getArgumentNameList,
									$"{getMethod.ReturnType.GetSafeName()}{getMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", getDelegateCast) :
								CodeTemplates.GetPropertyGetWithReferenceTypeReturnValueAndNoIndexersTemplate(
									getMethod.MetadataToken, getArgumentNameList,
									$"{getMethod.ReturnType.GetSafeName()}{getMethod.ReturnType.GetGenericArguments(this.Namespaces).Arguments}", getDelegateCast));
						}
					}

					if (baseProperty.CanWrite)
					{
						var setMethod = baseProperty.SetMethod;
						var setArgumentNameList = setMethod.GetArgumentNameList();
						var setDelegateCast = setMethod.GetDelegateCast();

						if (setMethod.GetParameters().Length > 0)
						{
							var setExpectationChecks = setMethod.GetExpectationChecks();
							var setExpectationExceptionMessage = setMethod.GetExpectationExceptionMessage();
							propertyImplementations.Add(CodeTemplates.GetPropertySetTemplate(
								setMethod.MetadataToken, setArgumentNameList, setExpectationChecks, setDelegateCast, setExpectationExceptionMessage));
						}
						else
						{
							propertyImplementations.Add(CodeTemplates.GetPropertySetAndNoIndexersTemplate(
								setMethod.MetadataToken, setArgumentNameList, setDelegateCast));
						}
					}

					if (indexers.Length > 0)
					{
						var parameters = string.Join(", ",
							from indexer in indexers
							let _ = this.Namespaces.Add(indexer.ParameterType.Namespace)
							select $"{indexer.ParameterType.Name} {indexer.Name}");

						// Indexer
						generatedProperties.Add(CodeTemplates.GetPropertyIndexerTemplate(
							$"{@override}{baseProperty.PropertyType.GetSafeName()}{baseProperty.PropertyType.GetGenericArguments(this.Namespaces).Arguments}", parameters, 
							string.Join(Environment.NewLine, propertyImplementations)));
					}
					else
					{
						// Normal
						generatedProperties.Add(CodeTemplates.GetPropertyTemplate(
							$"{@override}{baseProperty.PropertyType.GetSafeName()}{baseProperty.PropertyType.GetGenericArguments(this.Namespaces).Arguments}", baseProperty.Name,
                     string.Join(Environment.NewLine, propertyImplementations)));
					}
				}
				else if (!propertyMethod.IsPrivate && propertyMethod.IsAbstract)
				{
					var propertyImplementations = new List<string>();
					var visibility = propertyMethod.IsFamily ? CodeTemplates.Protected : CodeTemplates.Internal;

					if (baseProperty.CanRead)
					{
						propertyImplementations.Add(CodeTemplates.GetNonPublicPropertyGetTemplate());
					}

					if(baseProperty.CanWrite)
					{
						propertyImplementations.Add(CodeTemplates.GetNonPublicPropertySetTemplate());
					}

					if (indexers.Length > 0)
					{
						var parameters = string.Join(", ",
							from indexer in indexers
							let _ = this.Namespaces.Add(indexer.ParameterType.Namespace)
							select $"{indexer.ParameterType.Name} {indexer.Name}");

						// Indexer
						generatedProperties.Add(CodeTemplates.GetNonPublicPropertyIndexerTemplate(visibility,
							$"{baseProperty.PropertyType.GetSafeName()}{baseProperty.PropertyType.GetGenericArguments(this.Namespaces).Arguments}", parameters, 
                     string.Join(Environment.NewLine, propertyImplementations)));
					}
					else
					{
						// Normal
						generatedProperties.Add(CodeTemplates.GetNonPublicPropertyTemplate(visibility,
							$"{baseProperty.PropertyType.GetSafeName()}{baseProperty.PropertyType.GetGenericArguments(this.Namespaces).Arguments}", baseProperty.Name,
                     string.Join(Environment.NewLine, propertyImplementations)));
					}
				}
			}

			return generatedProperties;
		}

		protected string GetTypeNameWithNoGenerics() => this.TypeName.Split('<').First();

		private string MakeCode()
		{
			var methods = this.GetGeneratedMethods();
			var constructors = this.GetGeneratedConstructors();
			var properties = this.GetGeneratedProperties();
			var events = this.GetGeneratedEvents();

			this.Namespaces.Add(this.BaseType.Namespace);
			this.Namespaces.Add(typeof(ExpectationException).Namespace);
			this.Namespaces.Add(typeof(IMock).Namespace);
			this.Namespaces.Add(typeof(HandlerInformation).Namespace);
			this.Namespaces.Add(typeof(string).Namespace);
			this.Namespaces.Add(typeof(ReadOnlyDictionary<,>).Namespace);
			this.Namespaces.Add(typeof(BindingFlags).Namespace);

			return CodeTemplates.GetClassTemplate(
				string.Join(Environment.NewLine,
					(from @namespace in this.Namespaces
					 select $"using {@namespace};")),
				this.TypeName, $"{this.BaseType.GetSafeName()}{this.BaseType.GetGenericArguments(this.Namespaces).Arguments}",
				string.Join(Environment.NewLine, methods),
				string.Join(Environment.NewLine, properties),
				string.Join(Environment.NewLine, events),
				string.Join(Environment.NewLine, constructors),
				this.BaseType.Namespace,
				this.Options.Serialization == SerializationOptions.Supported ?
					"[Serializable]" : string.Empty,
				this.Options.Serialization == SerializationOptions.Supported ?
					CodeTemplates.GetConstructorNoArgumentsTemplate(this.GetTypeNameWithNoGenerics()) : string.Empty,
				this.GetTypeNameWithNoGenerics(), this.GetAdditionNamespaceCode());
		}

		private SyntaxTree MakeTree()
		{
			var @class = this.MakeCode();
			SyntaxTree tree = null;

			if (this.Options.CodeFile == CodeFileOptions.Create)
			{
				Directory.CreateDirectory(this.GetDirectoryForFile());
				var fileName = Path.Combine(this.GetDirectoryForFile(),
					$"{this.TypeName.Replace("<", string.Empty).Replace(">", string.Empty).Replace(", ", string.Empty)}.cs");
				tree = SyntaxFactory.SyntaxTree(
					SyntaxFactory.ParseSyntaxTree(@class)
						.GetCompilationUnitRoot().NormalizeWhitespace(),
					path: fileName, encoding: new UTF8Encoding(false, true));
				File.WriteAllText(fileName, tree.GetText().ToString());
			}
			else
			{
				tree = SyntaxFactory.ParseSyntaxTree(@class);
			}

			return tree;
		}

		protected abstract string GetDirectoryForFile();
		protected virtual string GetAdditionNamespaceCode() => string.Empty;

		internal Options Options { get; }
		internal SyntaxTree Tree { get; private set; }
		internal Type BaseType { get; }
		internal bool IsUnsafe { get; private set; }
		internal ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>> Handlers { get; }
		internal SortedSet<string> Namespaces { get; }
		internal string TypeName { get; set; }
	}
}
