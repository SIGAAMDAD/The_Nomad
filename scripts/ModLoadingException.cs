using System;

[Serializable]
public class ModLoadingException : Exception {
	public ModLoadingException()
		: base() { }

	public ModLoadingException( string message )
		: base( message ) { }

	public ModLoadingException( string message, Exception innerException )
		: base( message, innerException ) { }
};