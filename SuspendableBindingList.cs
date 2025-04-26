using System.ComponentModel;

namespace ORCToolKit
{
	public class SuspendableBindingList<T>: BindingList<T>
	{
		private Int32 _suspendDepth = 0;

		public void Suspend()
		{
			_suspendDepth++;
		}

		public void Resume()
		{
			if ( _suspendDepth < 1 )
				throw new InvalidOperationException();

			_suspendDepth--;
			if ( _suspendDepth == 0 )
				ResetBindings();
		}

		protected override void OnListChanged( ListChangedEventArgs e )
		{
			if ( _suspendDepth < 1 )
				base.OnListChanged( e );
		}
	}
}
