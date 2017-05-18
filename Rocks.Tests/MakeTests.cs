﻿using NUnit.Framework;
using Rocks.Options;
using System.Threading.Tasks;

namespace Rocks.Tests
{
	[TestFixture]
	public sealed class MakeTests
	{
		[Test]
		public void Make()
		{
			var chunk = Rock.Make<IAmForMaking>();

			chunk.TargetAction();
			chunk.TargetAction(44);
			chunk.TargetActionWithOut(out var outInt);
			chunk.TargetActionWithRef(ref outInt);
			chunk.TargetActionWithGeneric<int>(44);
			chunk.TargetActionWithGenericAndOut<int>(out outInt);
			chunk.TargetActionWithGenericAndRef<int>(ref outInt);
			var actionResult = chunk.TargetActionAsync();
			Assert.That(actionResult.IsCompleted, Is.True);

			chunk.TargetFunc();
			chunk.TargetFunc(44);
			chunk.TargetFuncWithOut(out outInt);
			chunk.TargetFuncWithRef(ref outInt);
			chunk.TargetFuncWithGeneric<int>(44);
			chunk.TargetFuncWithGenericAndOut<int>(out outInt);
			chunk.TargetFuncWithGenericAndRef<int>(ref outInt);
			var funcResult = chunk.TargetFuncAsync();
			Assert.That(funcResult.IsCompleted, Is.True);
			Assert.That(funcResult.Result, Is.EqualTo(default(int)));

			chunk.TargetProperty = 44;
			var x = chunk.TargetProperty;

			chunk[44] = 44;
			var y = chunk[44];
		}

		[Test]
		public void EnsureMakeAlwaysUsesCache()
		{
			var chunk1 = Rock.Make<IAmForMaking>(new RockOptions(caching: CachingOptions.GenerateNewVersion));
			var chunk2 = Rock.Make<IAmForMaking>(new RockOptions(caching: CachingOptions.GenerateNewVersion));

			Assert.That(chunk1.GetType(), Is.EqualTo(chunk2.GetType()));
		}
	}

	public interface IAmForMaking
	{
		void TargetAction();
		void TargetAction(int a);
		void TargetActionWithOut(out int a);
		void TargetActionWithRef(ref int a);
		void TargetActionWithGeneric<T>(T a);
		void TargetActionWithGenericAndOut<T>(out T a);
		void TargetActionWithGenericAndRef<T>(ref T a);
		Task TargetActionAsync();
		int TargetFunc();
		int TargetFunc(int a);
		int TargetFuncWithOut(out int a);
		int TargetFuncWithRef(ref int a);
		int TargetFuncWithGeneric<T>(T a);
		int TargetFuncWithGenericAndOut<T>(out T a);
		int TargetFuncWithGenericAndRef<T>(ref T a);
		Task<int> TargetFuncAsync();
		int TargetProperty { get; set; }
		int this[int a] { get; set; }
	}
}