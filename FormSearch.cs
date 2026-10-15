using ORCToolKit.Search;
using System.Resources;

namespace ORCToolKit
{
	public partial class FormSearch: LocalizedForm
	{
		public event SearchEventHandler? OnSearch;

		public FormSearch()
		{
			InitializeComponent();
			LocalizeComponent();
		}

		public override LocalizedControl[] LocalizedControls
		{
			get
			{
				return [
					new LocalizedControl { Object = this, ObjectName = this.Name, FieldName = "Text" },
					new LocalizedControl { Object = SearchLabel, ObjectName = SearchLabel.Name, FieldName = "Text" },
					new LocalizedControl { Object = SearchBackwardButton, ObjectName = SearchBackwardButton.Name, FieldName = "Text" },
					new LocalizedControl { Object = SearchForwardButton, ObjectName = SearchForwardButton.Name, FieldName = "Text" },
					new LocalizedControl { Object = WholeWordCheckBox, ObjectName = WholeWordCheckBox.Name, FieldName = "Text" },
					new LocalizedControl { Object = CaseSensitiveCheckBox, ObjectName = CaseSensitiveCheckBox.Name, FieldName = "Text" },
				];
			}
		}

		private void InvokeSearch( SearchDirection Direction )
		{
			if ( string.IsNullOrEmpty( SearchTextBox.Text ) )
				return;

			if ( OnSearch != null )
				OnSearch( this, new SearchEventArgs( Direction, SearchTextBox.Text, WholeWordCheckBox.Checked, CaseSensitiveCheckBox.Checked ) );
		}

		private void FormSearch_FormClosing( object sender, FormClosingEventArgs e )
		{
			// don't remove ourselves!
			Hide();
			e.Cancel = true;
		}

		private void FormSearch_VisibleChanged( object sender, EventArgs e )
		{
			if ( Visible )
			{
				BeginInvoke( (MethodInvoker)delegate
				{
					SearchTextBox.Focus();
					SearchTextBox.SelectAll();
				} );
			}
		}

		private void SearchBackwardButton_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Backward );
		}

		private void SearchForwardButton_Click( object sender, EventArgs e )
		{
			InvokeSearch( SearchDirection.Forward );
		}

		private void SearchTextBox_KeyPress( object sender, KeyPressEventArgs e )
		{
			if ( e.KeyChar == (char)Keys.Enter )
			{
				if ( Control.ModifierKeys == Keys.Shift )
					InvokeSearch( SearchDirection.Backward );
				else
					InvokeSearch( SearchDirection.Forward );
				e.Handled = true;
			}
		}
	}
}
