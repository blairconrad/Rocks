﻿#pragma warning disable CS0618
using NUnit.Framework;
using Rocks.Construction.InMemory;
using Rocks.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rocks.Tests.Construction.InMemory
{
	[TestFixture]
	public sealed class InMemoryBuilderTests
	{
		[Test]
		public void Build()
		{
			var baseType = typeof(IBuilderTest);
			var handlers = new ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>>(
				new Dictionary<int, ReadOnlyCollection<HandlerInformation>>());
			var namespaces = new SortedSet<string> { baseType.Namespace };
			var options = new RockOptions();

			var builder = new InMemoryBuilder(baseType, handlers, namespaces, options, false);
			builder.Build();

			Assert.That(builder.BaseType, Is.SameAs(baseType), nameof(builder.BaseType));
			Assert.That(builder.Handlers, Is.SameAs(handlers), nameof(builder.Handlers));
			Assert.That(builder.Namespaces, Is.SameAs(namespaces), nameof(builder.Namespaces));
			Assert.That(namespaces.Count, Is.EqualTo(0), nameof(namespaces.Count));

			Assert.That(builder.Options, Is.SameAs(options), nameof(builder.Options));
			Assert.That(builder.Tree, Is.Not.Null, nameof(builder.Tree));
			Assert.That(!string.IsNullOrWhiteSpace(builder.TypeName), Is.True, nameof(builder.TypeName));

			var tree = builder.Tree.ToString();

			Assert.That(tree.StartsWith("#pragma warning disable CS0618"), Is.False);
			Assert.That(tree.Contains("#pragma warning disable CS0672"), Is.False);
			Assert.That(tree.Contains("#pragma warning restore CS0672"), Is.False);
			Assert.That(tree.EndsWith("#pragma warning restore CS0618"), Is.False);
		}

		[Test]
		public void BuildWhenTypeIsObsolete()
		{
			var baseType = typeof(IAmObsolete);
			var handlers = new ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>>(
				new Dictionary<int, ReadOnlyCollection<HandlerInformation>>());
			var namespaces = new SortedSet<string> { baseType.Namespace };
			var options = new RockOptions();

			var builder = new InMemoryBuilder(baseType, handlers, namespaces, options, false);
			builder.Build();

			var tree = builder.Tree.ToString();

			Assert.That(tree.StartsWith("#pragma warning disable CS0618"), Is.True);
			Assert.That(tree.Contains("#pragma warning disable CS0672"), Is.True);
			Assert.That(tree.Contains("#pragma warning restore CS0672"), Is.True);
			Assert.That(tree.EndsWith("#pragma warning restore CS0618"), Is.True);
		}

		[Test]
		public void BuildWhenMethodIsObsolete()
		{
			var baseType = typeof(IHaveAnObsoleteMethod);
			var handlers = new ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>>(
				new Dictionary<int, ReadOnlyCollection<HandlerInformation>>());
			var namespaces = new SortedSet<string> { baseType.Namespace };
			var options = new RockOptions();

			var builder = new InMemoryBuilder(baseType, handlers, namespaces, options, false);
			builder.Build();

			var tree = builder.Tree.ToString();

			Assert.That(tree.StartsWith("#pragma warning disable CS0618"), Is.True);
			Assert.That(tree.Contains("#pragma warning disable CS0672"), Is.True);
			Assert.That(tree.Contains("#pragma warning restore CS0672"), Is.True);
			Assert.That(tree.EndsWith("#pragma warning restore CS0618"), Is.True);
		}

		[Test]
		public void BuildWhenPropertyIsObsolete()
		{
			var baseType = typeof(IHaveAnObsoleteProperty);
			var handlers = new ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>>(
				new Dictionary<int, ReadOnlyCollection<HandlerInformation>>());
			var namespaces = new SortedSet<string> { baseType.Namespace };
			var options = new RockOptions();

			var builder = new InMemoryBuilder(baseType, handlers, namespaces, options, false);
			builder.Build();

			var tree = builder.Tree.ToString();

			Assert.That(tree.StartsWith("#pragma warning disable CS0618"), Is.True);
			Assert.That(tree.Contains("#pragma warning disable CS0672"), Is.True);
			Assert.That(tree.Contains("#pragma warning restore CS0672"), Is.True);
			Assert.That(tree.EndsWith("#pragma warning restore CS0618"), Is.True);
		}

		[Test]
		public void BuildWhenEventIsObsolete()
		{
			var baseType = typeof(IHaveAnObsoleteEvent);
			var handlers = new ReadOnlyDictionary<int, ReadOnlyCollection<HandlerInformation>>(
				new Dictionary<int, ReadOnlyCollection<HandlerInformation>>());
			var namespaces = new SortedSet<string> { baseType.Namespace };
			var options = new RockOptions();

			var builder = new InMemoryBuilder(baseType, handlers, namespaces, options, false);
			builder.Build();

			var tree = builder.Tree.ToString();

			Assert.That(tree.StartsWith("#pragma warning disable CS0618"), Is.True);
			Assert.That(tree.Contains("#pragma warning disable CS0672"), Is.True);
			Assert.That(tree.Contains("#pragma warning restore CS0672"), Is.True);
			Assert.That(tree.EndsWith("#pragma warning restore CS0618"), Is.True);
		}
	}

	public interface IBuilderTest { }

	[Obsolete("", false)]
	public interface IAmObsolete { }

	public interface IHaveAnObsoleteMethod
	{
		[Obsolete]
		void TargetMethod();
		int TargetProperty { get; set; }
		event EventHandler TargetEvent;
	}

	public interface IHaveAnObsoleteProperty
	{
		void TargetMethod();
		[Obsolete]
		int TargetProperty { get; set; }
		event EventHandler TargetEvent;
	}

	public interface IHaveAnObsoleteEvent
	{
		void TargetMethod();
		int TargetProperty { get; set; }
		[Obsolete]
		event EventHandler TargetEvent;
	}
}
#pragma warning restore CS0618