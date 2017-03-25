using System;
namespace Tmpa {
	public class Error : InvalidOperationException {
		public Error(string message = null, params object[] messageArguments) : base(string.Format(message ?? "", messageArguments)) { } } }