﻿using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Rocks.Tests
{
	[TestFixture]
	public sealed class MakeTests
	{
		[Test]
		public void Make()
		{
			// TODO need to support all method and property scenarios.
			var chunk = Rock.Make<IAmForMaking>();
			var outInt = default(int);

			chunk.TargetAction();
			chunk.TargetAction(44);
			chunk.TargetActionWithOut(out outInt);
			chunk.TargetActionWithRef(ref outInt);
			chunk.TargetActionWithGeneric<int>(44);
			chunk.TargetActionWithGenericAndOut<int>(out outInt);
			chunk.TargetActionWithGenericAndRef<int>(ref outInt);

			chunk.TargetFunc();
			chunk.TargetFunc(44);
			chunk.TargetFuncWithOut(out outInt);
			chunk.TargetFuncWithRef(ref outInt);
			chunk.TargetFuncWithGeneric<int>(44);
			chunk.TargetFuncWithGenericAndOut<int>(out outInt);
			chunk.TargetFuncWithGenericAndRef<int>(ref outInt);

			chunk.TargetProperty = 44;
			var x = chunk.TargetProperty;

			chunk[44] = 44;
			var y = chunk[44];
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
		int TargetFunc();
		int TargetFunc(int a);
		int TargetFuncWithOut(out int a);
		int TargetFuncWithRef(ref int a);
		int TargetFuncWithGeneric<T>(T a);
		int TargetFuncWithGenericAndOut<T>(out T a);
		int TargetFuncWithGenericAndRef<T>(ref T a);
		int TargetProperty { get; set; }
		int this[int a] { get; set; }
	}
}