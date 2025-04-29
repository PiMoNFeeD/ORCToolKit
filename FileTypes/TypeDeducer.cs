using System.Diagnostics;
using System.Text;

namespace ORCToolKit.FileTypes
{
	public enum FileType
	{
		Unknown,
		SSGArchive,
		LocalizationContainer,
		SWB2Database
	};

	static public class TypeDeducer
	{
		static public FileType GuessFromFile( FileInfo FileInfoToGuess )
		{
			try
			{
				using ( FileStream Stream = FileInfoToGuess.OpenRead() )
				{
					return GuessFromBuffer( FileInfoToGuess.FullName, Stream );
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return FileType.Unknown;
			}
		}

		static public FileType GuessFromBuffer( string? FileName, Stream FileBuffer )
		{
			FileType Type = FileType.Unknown;

			// first, look at extension
			switch ( Path.GetExtension( FileName ) )
			{
				case ".ssg":
				{
					Type = FileType.SSGArchive;
					break;
				}
				case ".swb2":
				{
					Type = FileType.SWB2Database;
					break;
				}
				case "":
				{
					// extension-less? could be anything, read it and try to make the best guess
					if ( FileBuffer.Length > 8 )
					{
						Int64 OldPosition = FileBuffer.Position;
						FileBuffer.Position = 0;

						using ( BinaryReader Reader = new BinaryReader( FileBuffer, Encoding.UTF8, true ) )
						{
							UInt32 Header1 = Reader.ReadUInt32();
							UInt32 Header2 = Reader.ReadUInt32();

							if ( MathExtensions.IsPowerOfTwo( Header1 ) && Header2 == 0 )
								Type = FileType.LocalizationContainer;
						}

						// make sure we don't screw up chained read calls
						FileBuffer.Position = OldPosition;
					}
					break;
				}
			}
			return Type;
		}

		static public Form? GuessFromFileAndOpen( FileInfo FileInfoToGuess, Form? ParentForm = null, bool Silent = false )
		{
			try
			{
				using ( FileStream Stream = FileInfoToGuess.OpenRead() )
				{
					return GuessFromBufferAndOpen( FileInfoToGuess.FullName, Stream, ParentForm, Silent );
				}
			}
			catch ( Exception ex )
			{
				if ( !Silent )
					MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return null;
			}
		}

		static public Form? GuessFromBufferAndOpen( string? FileName, Stream FileBuffer, Form? ParentForm = null, bool Silent = false )
		{
			switch ( GuessFromBuffer( FileName, FileBuffer ) )
			{
				case FileType.LocalizationContainer:
				{
					FormLocalizationEditor LocalizationEditor = new FormLocalizationEditor();
					LocalizationEditor.OpenFile( FileBuffer );
					if ( ParentForm != null )
						LocalizationEditor.ShowDialog( ParentForm );
					else
						LocalizationEditor.Show();
					return LocalizationEditor;
				}
				case FileType.SSGArchive:
				{
					FormSSGArchiveEditor SSGArchiveEditor = new FormSSGArchiveEditor();
					SSGArchiveEditor.OpenFile( FileBuffer );
					if ( ParentForm != null )
						SSGArchiveEditor.ShowDialog( ParentForm );
					else
						SSGArchiveEditor.Show();
					return SSGArchiveEditor;
				}
				case FileType.SWB2Database:
				default:
				{
					if ( !Silent )
					{
						if ( MessageBox.Show( Resources.ORCToolKit.FileTypeDeducerOpenUnknownText, Resources.ORCToolKit.FileTypeDeducerOpenUnknownTitle,
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning ) == DialogResult.Yes )
						{
							// HACK: pass the file onto explorer to deal with it for us...
							Process FileProcess = new Process();
							FileProcess.StartInfo.FileName = "explorer";
							FileProcess.StartInfo.Arguments = "\"" + FileName + "\"";
							FileProcess.Start();
						}
					}
					break;
				}
			}

			return null;
		}
	}
}
