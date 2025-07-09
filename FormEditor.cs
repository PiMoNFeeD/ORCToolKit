using ORCToolKit.Search;
using System.Text.RegularExpressions;

namespace ORCToolKit
{
	public class FormEditor: Form
	{
		protected bool HasUnsavedEdits = false;

		private FormSearch FormSearch = new FormSearch();
		protected string LastSearchString = string.Empty;
		protected bool LastSearchWholeWord = false;
		protected bool LastSearchCaseSensitive = false;

		virtual protected FileTypes.IFileType? CurrentFile { get { return null; } } // can't throw an exception because VS designer sucks ass...
		virtual protected string FileDialogFilter { get { throw new NotImplementedException(); } }

		protected event EventHandler? OnNewFile;
		protected event EventHandler? OnOpenFile;
		protected event EventHandler? OnSaveFile;
		protected event EventHandler? OnEntriesUpdated;
		virtual protected bool TrySearch( SearchDirection Direction, string SearchString, bool WholeWord, bool CaseSensitive ) { throw new NotImplementedException(); }
		virtual protected void SaveChild( FileTypes.IFileType? File ) { throw new NotImplementedException(); }

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
				HasUnsavedEdits = false;
				CurrentFile?.Reset();
				EntriesUpdated();
			}
		}

		public void OpenFile( Stream FileBuffer )
		{
			HasUnsavedEdits = false;
			CurrentFile?.LoadFromBuffer( FileBuffer );

			if ( OnOpenFile != null )
				OnOpenFile( this, new EventArgs() );

			EntriesUpdated();
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

		public void EntriesUpdated()
		{
			if ( OnEntriesUpdated != null )
				OnEntriesUpdated( this, new EventArgs() );
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

		virtual protected bool SearchCompareString( string CompareTo, string SearchString, bool WholeWord, bool CaseSensitive )
		{
			if ( WholeWord )
			{
				// use regex for whole word matching
				RegexOptions Options = RegexOptions.Compiled;
				if ( !CaseSensitive )
					Options |= RegexOptions.IgnoreCase;
				return Regex.IsMatch( CompareTo, $@"\b{SearchString}\b", Options );
			}
			else
			{
				return CompareTo.Contains( SearchString, CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase );
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
		}

		private void FormSearch_OnSearch( object sender, SearchEventArgs e )
		{
			InvokeSearch( e.Direction, e.SearchString, e.WholeWord, e.CaseSensitive );
		}
	}
}
