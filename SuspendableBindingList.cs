using System.ComponentModel;

namespace ORCToolKit
{
	public class SuspendableBindingList<T>: BindingList<T>
	{
		private bool _suspended = false;

		public void Suspend()
		{
			_suspended = true;
		}

		public void Resume()
		{
			_suspended = false;
			ResetBindings();
		}

		protected override void OnListChanged( ListChangedEventArgs e )
		{
			if ( !_suspended )
				base.OnListChanged( e );
		}
	}
}
