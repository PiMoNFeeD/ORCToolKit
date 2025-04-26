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

	static public class Deducer
	{
		static public FileType GuessFromFile( FileInfo FileInfoToGuess )
		{
			using ( FileStream Stream = FileInfoToGuess.OpenRead() )
			{
				return GuessFromBuffer( FileInfoToGuess.FullName, Stream );
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
					using ( BinaryReader Reader = new BinaryReader( FileBuffer, Encoding.UTF8, true ) )
					{
						UInt32 Header1 = Reader.ReadUInt32();
						UInt32 Header2 = Reader.ReadUInt32();

						if ( MathExtensions.IsPowerOfTwo( Header1 ) && Header2 == 0 )
							Type = FileType.LocalizationContainer;
					}
					break;
				}
			}
			return Type;
		}

		static public void GuessFromFileAndOpen( FileInfo FileInfoToGuess, Form? ParentForm = null )
		{
			using ( FileStream Stream = FileInfoToGuess.OpenRead() )
			{
				GuessFromBufferAndOpen( FileInfoToGuess.FullName, Stream );
			}
		}

		static public void GuessFromBufferAndOpen( string? FileName, Stream FileBuffer, Form? ParentForm = null )
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
					break;
				}
				case FileType.SSGArchive:
				{
					FormSSGArchiveEditor SSGArchiveEditor = new FormSSGArchiveEditor();
					SSGArchiveEditor.OpenFile( FileBuffer );
					if ( ParentForm != null )
						SSGArchiveEditor.ShowDialog( ParentForm );
					else
						SSGArchiveEditor.Show();
					break;
				}
				case FileType.SWB2Database:
				default:
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
					break;
				}
			}
		}
	}
}
