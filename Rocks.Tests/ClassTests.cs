﻿using NUnit.Framework;
using Rocks.Options;
using System;

namespace Rocks.Tests
{
	[TestFixture]
	public sealed class ClassTests
	{
		[Test]
		public void Make()
		{
			var rock = Rock.Create<ClassTestsTarget>(
				new RockOptions(
					codeFile: CodeFileOptions.Create, 
					codeFileDirectory: TestContext.CurrentContext.TestDirectory,
					level: OptimizationSetting.Debug));
			rock.Handle(_ => _.TargetMethod());

			var chunk = rock.Make();
			chunk.TargetMethod();

			rock.Verify();
		}

		[Test]
		public void MakeWithCustomConstructor()
		{
			var rock = Rock.Create<ClassTestsTarget>();
			rock.Handle(_ => _.TargetMethod());

			var chunk = rock.Make(new object[] { 2 });
			chunk.TargetMethod();

			Assert.That(chunk.Value, Is.EqualTo(2), nameof(chunk.Value));

			rock.Verify();
		}
	}

	public class ClassTestsTarget
	{
		public ClassTestsTarget() { }

		public ClassTestsTarget(int value) => this.Value = value;

		public virtual void TargetMethod() => this.TargetEvent(this, EventArgs.Empty);

		public virtual int TargetProperty { get; set; }

		public int Value { get; private set; }

		public virtual event EventHandler TargetEvent;
	}
}
