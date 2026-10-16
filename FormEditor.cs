using ORCToolKit.Search;

namespace ORCToolKit
{
	public class EntriesUpdatedEventArgs: EventArgs
	{
		public readonly bool HasUnsavedEdits;

		public EntriesUpdatedEventArgs( bool hasUnsavedEdits ) { HasUnsavedEdits = hasUnsavedEdits; }
	}

	public delegate void EntriesUpdatedEventHandler( object? sender, EntriesUpdatedEventArgs args );

	public class FormEditor: LocalizedForm
	{
		private bool _hasUnsavedEdits = false;

		protected FormSearch FormSearch = new FormSearch();
		protected string LastSearchString = string.Empty;
		protected bool LastSearchWholeWord = false;
		protected bool LastSearchCaseSensitive = false;

		virtual protected FileTypes.IFileType? CurrentFile { get { return null; } } // can't throw an exception because VS designer sucks ass...
		virtual protected string FileDialogFilter { get { throw new NotImplementedException(); } }

		protected event EventHandler? OnNewFile;
		protected event EventHandler? OnOpenFile;
		protected event EventHandler? OnSaveFile;
		protected event EntriesUpdatedEventHandler? OnEntriesUpdated;
		virtual protected bool TrySearch( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive ) { throw new NotImplementedException(); }
		virtual protected void SaveChild( FileTypes.IFileType? file ) { _hasUnsavedEdits = false; }

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

		public void OpenFile( Stream fileBuffer )
		{
			CurrentFile?.LoadFromBuffer( fileBuffer );

			if ( OnOpenFile != null )
				OnOpenFile( this, new EventArgs() );

			EntriesUpdated( false );
		}

		public void SaveFile( Stream fileBuffer )
		{
			_hasUnsavedEdits = false;
			CurrentFile?.SaveToBuffer( fileBuffer );

			if ( OnSaveFile != null )
				OnSaveFile( this, new EventArgs() );
		}

		public void SaveFile( FormEditor editor )
		{
			_hasUnsavedEdits = false;
			editor.SaveChild( CurrentFile );

			if ( OnSaveFile != null )
				OnSaveFile( this, new EventArgs() );
		}

		public void EntriesUpdated( bool hasUnsavedEdits = true )
		{
			_hasUnsavedEdits = hasUnsavedEdits;
			if ( OnEntriesUpdated != null )
				OnEntriesUpdated( this, new EntriesUpdatedEventArgs( _hasUnsavedEdits ) );
		}

		protected bool InvokeOpenDialog()
		{
			if ( OwnedByAnotherEditor )
				return false;

			if ( CheckUnsavedChanges() )
			{
				using ( OpenFileDialog fileDialog = new OpenFileDialog() )
				{
					fileDialog.Filter = FileDialogFilter;
					fileDialog.RestoreDirectory = true;
					fileDialog.Multiselect = false;
					if ( fileDialog.ShowDialog() == DialogResult.OK )
					{
						using ( FileStream file = new FileStream( fileDialog.FileName, FileMode.Open, FileAccess.Read ) )
						{
							OpenFile( file );
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
				FormEditor ownerEditor = (FormEditor)Owner;
				ownerEditor.SaveChild( CurrentFile );
				return true; // save successful
			}

			using ( SaveFileDialog fileDialog = new SaveFileDialog() )
			{
				fileDialog.Filter = FileDialogFilter;
				fileDialog.RestoreDirectory = true;
				if ( fileDialog.ShowDialog() == DialogResult.OK )
				{
					using ( FileStream file = new FileStream( fileDialog.FileName, FileMode.Create, FileAccess.Write ) )
					{
						SaveFile( file );
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

		protected void InvokeSearch( SearchDirection direction, string searchString, bool wholeWord, bool caseSensitive )
		{
			LastSearchString = searchString;

			if ( !TrySearch( direction, searchString, wholeWord, caseSensitive ) )
			{
				MessageBox.Show( string.Format( Resources.ORCToolKit.SearchFailedText, searchString ), Resources.ORCToolKit.SearchFailedTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		protected bool CheckUnsavedChanges()
		{
			if ( !_hasUnsavedEdits )
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
			_hasUnsavedEdits = false;
		}

		private void FormSearch_OnSearch( object sender, SearchEventArgs e )
		{
			InvokeSearch( e.Direction, e.SearchString, e.WholeWord, e.CaseSensitive );
		}
	}
}
