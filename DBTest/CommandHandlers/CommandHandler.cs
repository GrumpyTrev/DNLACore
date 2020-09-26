namespace DBTest
{
	abstract class CommandHandler
	{
		public abstract void HandleCommand( int commandIdentity );

		public virtual void BindToRouter()
		{
			CommandRouter.BindHandler( CommandIdentity, this );
		}

		protected abstract int CommandIdentity { get; }
	}
}