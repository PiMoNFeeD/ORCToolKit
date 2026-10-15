using ORCToolKit.Search;
using System.Text.RegularExpressions;

namespace ORCToolKit
{
	public class EntriesUpdatedEventArgs: EventArgs
	{
		public readonly bool HasUnsavedEdits;

		public EntriesUpdatedEventArgs( bool HasUnsavedEdits ) { this.HasUnsavedEdits = HasUnsavedEdits; }
	}

	public delegate void EntriesUpdatedEventHandler( Object? sender, EntriesUpdatedEventArgs args );

	public class FormEditor: Form
	{
		private bool HasUnsavedEdits = false;

		private FormSearch FormSearch = new FormSearch();
		protected string LastSearchString = string.Empty;
		protected bool LastSearchWholeWord = false;
		protected bool LastSearchCaseSensitive = false;

		virtual protected FileTypes.IFileType? CurrentFile { get { return null; } } // can't throw an exception because VS designer sucks ass...
		virtual protected string FileDialogFilter { get { throw new NotImplementedException(); } }

		protected event EventHandler? OnNewFile;
		protected event EventHandler? OnOpenFile;
		protected event EventHandler? OnSaveFile;
		protected event EntriesUpdatedEventHandler? OnEntriesUpdated;
		virtual protected bool TrySearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive ) { throw new NotImplementedException(); }
		virtual protected void SaveChild( FileTypes.IFileType? File ) { HasUnsavedEdits = false; }

		protected bool OwnedByAnotherEditor = false;

		public FormEditor()
		{
			NewFile();

			FormClosing += FormEditor_FormClosing;
			Shown += FormEditor_Shown;
			FormSearch.OnSearch += FormSearch_OnSearch;
		}

		public void NewFile()
		{
			if ( CheckUnsavedChanges() )
			{
				CurrentFile?.Reset();

				if ( OnNewFile != null )
					OnNewFile( this, new EventArgs() );

				EntriesUpdated( false );
			}
		}

		public void OpenFile( Stream FileBuffer )
		{
			CurrentFile?.LoadFromBuffer( FileBuffer );

			if ( OnOpenFile != null )
				OnOpenFile( this, new EventArgs() );

			EntriesUpdated( false );
		}

		public void SaveFile( Stream FileBuffer )
		{
			HasUnsavedEdits = false;
			CurrentFile?.SaveToBuffer( FileBuffer );

			if ( OnSaveFile != null )
				OnSaveFile( this, new EventArgs() );
		}

		public void SaveFile( FormEditor Editor )
		{
			HasUnsavedEdits = false;
			Editor.SaveChild( CurrentFile );

			if ( OnSaveFile != null )
				OnSaveFile( this, new EventArgs() );
		}

		public void EntriesUpdated( bool UnsavedEdits = true )
		{
			HasUnsavedEdits = UnsavedEdits;
			if ( OnEntriesUpdated != null )
				OnEntriesUpdated( this, new EntriesUpdatedEventArgs( HasUnsavedEdits ) );
		}

		protected bool InvokeOpenDialog()
		{
			if ( OwnedByAnotherEditor )
				return false;

			if ( CheckUnsavedChanges() )
			{
				using ( OpenFileDialog FileDialog = new OpenFileDialog() )
				{
					FileDialog.Filter = FileDialogFilter;
					FileDialog.RestoreDirectory = true;
					FileDialog.Multiselect = false;
					if ( FileDialog.ShowDialog() == DialogResult.OK )
					{
						using ( FileStream File = new FileStream( FileDialog.FileName, FileMode.Open, FileAccess.Read ) )
						{
							OpenFile( File );
						}
					}
				}
			}

			return false;
		}

		protected bool InvokeSaveDialog()
		{
			if ( OwnedByAnotherEditor )
			{
				// if we're owned by another editor, we should save into that one instead
				FormEditor OwnerEditor = (FormEditor)Owner;
				OwnerEditor.SaveChild( CurrentFile );
				return true; // save successful
			}

			using ( SaveFileDialog FileDialog = new SaveFileDialog() )
			{
				FileDialog.Filter = FileDialogFilter;
				FileDialog.RestoreDirectory = true;
				if ( FileDialog.ShowDialog() == DialogResult.OK )
				{
					using ( FileStream File = new FileStream( FileDialog.FileName, FileMode.Create, FileAccess.Write ) )
					{
						SaveFile( File );
					}
					return true;
				}
			}

			return false;
		}

		protected void InvokeSearchDialog()
		{
			FormSearch.Show();
		}

		protected void InvokeSearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			LastSearchString = SearchString;

			if ( !TrySearch( Direction, SearchString, WholeWord, CaseSensitive ) )
			{
				MessageBox.Show( string.Format( Resources.ORCToolKit.SearchFailedText, SearchString ), Resources.ORCToolKit.SearchFailedTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		protected bool CheckUnsavedChanges()
		{
			if ( !HasUnsavedEdits )
				return true;

			switch ( MessageBox.Show( Resources.ORCToolKit.UnsavedChangesText, Resources.ORCToolKit.UnsavedChangesTitle,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information ) )
			{
				case DialogResult.Yes:
					return InvokeSaveDialog();
				case DialogResult.No:
					return true;
				case DialogResult.Cancel:
					return false;
			}

			return false;
		}

		private void FormEditor_FormClosing( object? sender, FormClosingEventArgs e )
		{
			if ( !CheckUnsavedChanges() )
				e.Cancel = true;
		}

		private void FormEditor_Shown( object? sender, EventArgs e )
		{
			OwnedByAnotherEditor = Owner is FormEditor;
			HasUnsavedEdits = false;
		}

		private void FormSearch_OnSearch( object sender, SearchEventArgs e )
		{
			InvokeSearch( e.Direction, e.SearchString, e.WholeWord, e.CaseSensitive );
		}
	}
}
