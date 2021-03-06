﻿namespace Rocks
{
	public sealed class CreateResult<T>
		where T : class
	{
		internal CreateResult(bool isSuccessful, IRock<T> result)
		{
			this.IsSuccessful = isSuccessful;
			this.Result = result;
		}

		public bool IsSuccessful { get; }
		public IRock<T> Result { get; }
	}
}
