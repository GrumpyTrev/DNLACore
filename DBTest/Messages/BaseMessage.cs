namespace DBTest
{
	/// <summary>
	/// All other messages are based on this message
	/// </summary>
	class BaseMessage
	{
		/// <summary>
		/// Send this message to all registered receivers
		/// </summary>
		public void Send()
		{
			Mediator.SendMessage( this );
		}
	}
}