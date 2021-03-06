﻿using NUnit.Framework;
using Rocks.Exceptions;
using System.IO;
using Rocks.Options;

namespace Rocks.Tests
{
	[TestFixture]
	public sealed class RockTests
	{
		[Test]
		public void Create() => 
			Assert.That(Rock.Create<IRockTests>(), Is.Not.Null, nameof(Rock.Create));

		[Test]
		public void CreateWhenTypeIsSealed() =>
			Assert.That(() => Rock.Create<string>(), Throws.TypeOf<ValidationException>());

		[Test]
		public void TryCreate()
		{
			var result = Rock.TryCreate<IRockTests>();
			Assert.That(result.IsSuccessful, nameof(result.IsSuccessful), Is.True);
			Assert.That(result.Result, Is.Not.Null, nameof(result.Result));
		}

		[Test]
		public void TryCreateWhenTypeIsSealed()
		{
			var result = Rock.TryCreate<string>();
			Assert.That(result.IsSuccessful, Is.False);
			Assert.That(result.Result, Is.Null, nameof(result.Result));
		}

		[Test]
		public void Make()
		{
			var rock = Rock.Create<IRockTests>();
			rock.Handle(_ => _.Member());

			var chunk = rock.Make();
			var chunkType = chunk.GetType();
			Assert.That(chunkType.Namespace, Is.EqualTo(typeof(IRockTests).Namespace), nameof(chunkType.Namespace));

			var chunkAsRock = chunk as IMock;
			Assert.That(chunkAsRock.Handlers.Count, Is.EqualTo(1), nameof(chunkAsRock.Handlers.Count));
		}

		[Test]
		public void MakeWithFile()
		{
			var testDirectory = TestContext.CurrentContext.TestDirectory;
			var rock = Rock.Create<IFileTests>(
				new RockOptions(
					level: OptimizationSetting.Debug,
					codeFile: CodeFileOptions.Create,
					codeFileDirectory: testDirectory));
			rock.Handle(_ => _.Member("a", 44));

			var chunk = rock.Make();
			var chunkType = chunk.GetType();
			Assert.That(chunkType.Namespace, Is.EqualTo(typeof(IFileTests).Namespace), nameof(chunkType.Namespace));
			Assert.That(File.Exists(Path.Combine(testDirectory, $"{chunkType.Name}.cs")), Is.True, nameof(File.Exists));

			var chunkAsRock = chunk as IMock;
			Assert.That(chunkAsRock.Handlers.Count, Is.EqualTo(1), nameof(chunkAsRock.Handlers.Count));

			chunk.Member("a", 44);
			rock.Verify();
		}

		[Test]
		public void MakeWhenTypeNameExistsInRocksAssembly()
		{
			var rock = Rock.Create<SomeNamespaceOtherThanRocks.IMock>();
			var chunk = rock.Make();
		}

		[Test]
		public void Remake()
		{
			var rock = Rock.Create<IRockTests>();
			rock.Handle(_ => _.Member());

			var chunk = rock.Make();
			chunk.Member();

			rock.Verify();

			var secondRock = Rock.Create<IRockTests>();
			secondRock.Handle(_ => _.SecondMember());

			var secondChunk = secondRock.Make();
			secondChunk.SecondMember();

			secondRock.Verify();
		}

		[Test]
		public void RemakeWithSameOptions()
		{
			var rock = Rock.Create<ISameRemake>(new RockOptions(level: OptimizationSetting.Release));
			var chunk = rock.Make();

			var secondRock = Rock.Create<ISameRemake>(new RockOptions(level: OptimizationSetting.Release));
			var secondChunk = secondRock.Make();

			Assert.That(secondChunk.GetType(), Is.EqualTo(chunk.GetType()));
		}

		[Test]
		public void RemakeWithDifferentOptions()
		{
			var rock = Rock.Create<IDifferentRemake>(new RockOptions(level: OptimizationSetting.Debug));
			var chunk = rock.Make();

			var secondRock = Rock.Create<IDifferentRemake>(new RockOptions(level: OptimizationSetting.Release));
			var secondChunk = secondRock.Make();

			Assert.That(secondChunk.GetType(), Is.Not.EqualTo(chunk.GetType()));
		}
	}

	public interface ISameRemake
	{
		void Target();
	}

	public interface IDifferentRemake
	{
		void Target();
	}

	public interface IRockTests
	{
		void Member();
		void SecondMember();
	}

	public interface IFileTests
	{
		void Member(string a, int b);
	}

	namespace SomeNamespaceOtherThanRocks
	{
		public abstract class ArgumentExpectation { }
		public interface IMock
		{
			void Target(ArgumentExpectation a);
		}
	}
}
