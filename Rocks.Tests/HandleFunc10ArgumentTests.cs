﻿using NUnit.Framework;
using System;

namespace Rocks.Tests
{
	[TestFixture]
	public sealed class HandleFunc10ArgumentTests
	{
		[Test]
		public void Make()
		{
			var rock = Rock.Create<IHandleFunc10ArgumentTests>();
			rock.HandleFunc(_ => _.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
			rock.HandleFunc(_ => _.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100));

			var chunk = rock.Make();
			chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100);

			rock.Verify();
		}

		[Test]
		public void MakeAndRaiseEvent()
		{
			var rock = Rock.Create<IHandleFunc10ArgumentTests>();
			var referenceAdornment = rock.HandleFunc(_ => _.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10));
			referenceAdornment.Raises(nameof(IHandleFunc10ArgumentTests.TargetEvent), EventArgs.Empty);
			var valueAdornment = rock.HandleFunc(_ => _.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100));
			valueAdornment.Raises(nameof(IHandleFunc10ArgumentTests.TargetEvent), EventArgs.Empty);

			var eventRaisedCount = 0;
			var chunk = rock.Make();
			chunk.TargetEvent += (s, e) => eventRaisedCount++;
			chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100);

			Assert.AreEqual(2, eventRaisedCount);
			rock.Verify();
		}

		[Test]
		public void MakeWithHandler()
		{
			var argumentA = 0;
			var argumentB = 0;
			var argumentC = 0;
			var argumentD = 0;
			var argumentE = 0;
			var argumentF = 0;
			var argumentG = 0;
			var argumentH = 0;
			var argumentI = 0;
			var argumentJ = 0;
			var stringReturnValue = "a";
			var intReturnValue = 1;

			var rock = Rock.Create<IHandleFunc10ArgumentTests>();
			rock.HandleFunc<int, int, int, int, int, int, int, int, int, int, string>(_ => _.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10),
				(a, b, c, d, e, f, g, h, i, j) => { argumentA = a; argumentB = b; argumentC = c; argumentD = d; argumentE = e; argumentF = f; argumentG = g; argumentH = h; argumentI = i; argumentJ = j; return stringReturnValue; });
			rock.HandleFunc<int, int, int, int, int, int, int, int, int, int, int>(_ => _.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100),
				(a, b, c, d, e, f, g, h, i, j) => { argumentA = a; argumentB = b; argumentC = c; argumentD = d; argumentE = e; argumentF = f; argumentG = g; argumentH = h; argumentI = i; argumentJ = j; return intReturnValue; });
			
			var chunk = rock.Make();
			Assert.AreEqual(stringReturnValue, chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), nameof(chunk.ReferenceTarget));
			Assert.AreEqual(1, argumentA, nameof(argumentA));
			Assert.AreEqual(2, argumentB, nameof(argumentB));
			Assert.AreEqual(3, argumentC, nameof(argumentC));
			Assert.AreEqual(4, argumentD, nameof(argumentD));
			Assert.AreEqual(5, argumentE, nameof(argumentE));
			Assert.AreEqual(6, argumentF, nameof(argumentF));
			Assert.AreEqual(7, argumentG, nameof(argumentG));
			Assert.AreEqual(8, argumentH, nameof(argumentH));
			Assert.AreEqual(9, argumentI, nameof(argumentI));
			Assert.AreEqual(10, argumentJ, nameof(argumentJ));
			argumentA = 0;
			argumentB = 0;
			argumentC = 0;
			argumentD = 0;
			argumentE = 0;
			argumentF = 0;
			argumentG = 0;
			argumentH = 0;
			argumentI = 0;
			argumentJ = 0;
         Assert.AreEqual(intReturnValue, chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100), nameof(chunk.ValueTarget));
			Assert.AreEqual(10, argumentA, nameof(argumentA));
			Assert.AreEqual(20, argumentB, nameof(argumentB));
			Assert.AreEqual(30, argumentC, nameof(argumentC));
			Assert.AreEqual(40, argumentD, nameof(argumentD));
			Assert.AreEqual(50, argumentE, nameof(argumentE));
			Assert.AreEqual(60, argumentF, nameof(argumentF));
			Assert.AreEqual(70, argumentG, nameof(argumentG));
			Assert.AreEqual(80, argumentH, nameof(argumentH));
			Assert.AreEqual(90, argumentI, nameof(argumentI));
			Assert.AreEqual(100, argumentJ, nameof(argumentJ));

			rock.Verify();
		}

		[Test]
		public void MakeWithExpectedCallCount()
		{
			var rock = Rock.Create<IHandleFunc10ArgumentTests>();
			rock.HandleFunc(_ => _.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), 2);
			rock.HandleFunc(_ => _.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100), 2);

			var chunk = rock.Make();
			chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100);
			chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100);

			rock.Verify();
		}

		[Test]
		public void MakeWithHandlerAndExpectedCallCount()
		{
			var argumentA = 0;
			var argumentB = 0;
			var argumentC = 0;
			var argumentD = 0;
			var argumentE = 0;
			var argumentF = 0;
			var argumentG = 0;
			var argumentH = 0;
			var argumentI = 0;
			var argumentJ = 0;
			var stringReturnValue = "a";
			var intReturnValue = 1;

			var rock = Rock.Create<IHandleFunc10ArgumentTests>();
			rock.HandleFunc<int, int, int, int, int, int, int, int, int, int, string>(_ => _.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10),
				(a, b, c, d, e, f, g, h, i, j) => { argumentA = a; argumentB = b; argumentC = c; argumentD = d; argumentE = e; argumentF = f; argumentG = g; argumentH = h; argumentI = i; argumentJ = j; return stringReturnValue; }, 2);
			rock.HandleFunc<int, int, int, int, int, int, int, int, int, int, int>(_ => _.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100),
				(a, b, c, d, e, f, g, h, i, j) => { argumentA = a; argumentB = b; argumentC = c; argumentD = d; argumentE = e; argumentF = f; argumentG = g; argumentH = h; argumentI = i; argumentJ = j; return intReturnValue; }, 2);

			var chunk = rock.Make();
			Assert.AreEqual(stringReturnValue, chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), nameof(chunk.ReferenceTarget));
			Assert.AreEqual(1, argumentA, nameof(argumentA));
			Assert.AreEqual(2, argumentB, nameof(argumentB));
			Assert.AreEqual(3, argumentC, nameof(argumentC));
			Assert.AreEqual(4, argumentD, nameof(argumentD));
			Assert.AreEqual(5, argumentE, nameof(argumentE));
			Assert.AreEqual(6, argumentF, nameof(argumentF));
			Assert.AreEqual(7, argumentG, nameof(argumentG));
			Assert.AreEqual(8, argumentH, nameof(argumentH));
			Assert.AreEqual(9, argumentI, nameof(argumentI));
			Assert.AreEqual(10, argumentJ, nameof(argumentJ));
			argumentA = 0;
			argumentB = 0;
			argumentC = 0;
			argumentD = 0;
			argumentE = 0;
			argumentF = 0;
			argumentG = 0;
			argumentH = 0;
			argumentI = 0;
			argumentJ = 0;
			Assert.AreEqual(stringReturnValue, chunk.ReferenceTarget(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), nameof(chunk.ReferenceTarget));
			Assert.AreEqual(1, argumentA, nameof(argumentA));
			Assert.AreEqual(2, argumentB, nameof(argumentB));
			Assert.AreEqual(3, argumentC, nameof(argumentC));
			Assert.AreEqual(4, argumentD, nameof(argumentD));
			Assert.AreEqual(5, argumentE, nameof(argumentE));
			Assert.AreEqual(6, argumentF, nameof(argumentF));
			Assert.AreEqual(7, argumentG, nameof(argumentG));
			Assert.AreEqual(8, argumentH, nameof(argumentH));
			Assert.AreEqual(9, argumentI, nameof(argumentI));
			Assert.AreEqual(10, argumentJ, nameof(argumentJ));
			argumentA = 0;
			argumentB = 0;
			argumentC = 0;
			argumentD = 0;
			argumentE = 0;
			argumentF = 0;
			argumentG = 0;
			argumentH = 0;
			argumentI = 0;
			argumentJ = 0;
			Assert.AreEqual(intReturnValue, chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100), nameof(chunk.ValueTarget));
			Assert.AreEqual(10, argumentA, nameof(argumentA));
			Assert.AreEqual(20, argumentB, nameof(argumentB));
			Assert.AreEqual(30, argumentC, nameof(argumentC));
			Assert.AreEqual(40, argumentD, nameof(argumentD));
			Assert.AreEqual(50, argumentE, nameof(argumentE));
			Assert.AreEqual(60, argumentF, nameof(argumentF));
			Assert.AreEqual(70, argumentG, nameof(argumentG));
			Assert.AreEqual(80, argumentH, nameof(argumentH));
			Assert.AreEqual(90, argumentI, nameof(argumentI));
			Assert.AreEqual(100, argumentJ, nameof(argumentJ));
			argumentA = 0;
			argumentB = 0;
			argumentC = 0;
			argumentD = 0;
			argumentE = 0;
			argumentF = 0;
			argumentG = 0;
			argumentH = 0;
			argumentI = 0;
			argumentJ = 0;
			Assert.AreEqual(intReturnValue, chunk.ValueTarget(10, 20, 30, 40, 50, 60, 70, 80, 90, 100), nameof(chunk.ValueTarget));
			Assert.AreEqual(10, argumentA, nameof(argumentA));
			Assert.AreEqual(20, argumentB, nameof(argumentB));
			Assert.AreEqual(30, argumentC, nameof(argumentC));
			Assert.AreEqual(40, argumentD, nameof(argumentD));
			Assert.AreEqual(50, argumentE, nameof(argumentE));
			Assert.AreEqual(60, argumentF, nameof(argumentF));
			Assert.AreEqual(70, argumentG, nameof(argumentG));
			Assert.AreEqual(80, argumentH, nameof(argumentH));
			Assert.AreEqual(90, argumentI, nameof(argumentI));
			Assert.AreEqual(100, argumentJ, nameof(argumentJ));

			rock.Verify();
		}
	}

	public interface IHandleFunc10ArgumentTests
	{
		event EventHandler TargetEvent;
		string ReferenceTarget(int a, int b, int c, int d, int e, int f, int g, int h, int i, int j);
		int ValueTarget(int a, int b, int c, int d, int e, int f, int g, int h, int i, int j);
	}
}