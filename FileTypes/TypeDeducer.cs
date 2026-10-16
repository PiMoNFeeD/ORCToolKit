using ORCToolKit.Extensions;
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
		static public FileType GuessFromFile( FileInfo fileInfoToGuess )
		{
			try
			{
				using ( FileStream stream = fileInfoToGuess.OpenRead() )
				{
					return GuessFromBuffer( fileInfoToGuess.FullName, stream );
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return FileType.Unknown;
			}
		}

		static public FileType GuessFromBuffer( string? fileName, Stream fileStream )
		{
			FileType type = FileType.Unknown;

			// first, look at extension
			switch ( Path.GetExtension( fileName ) )
			{
				case ".ssg":
				{
					type = FileType.SSGArchive;
					break;
				}
				case ".swb2":
				{
					type = FileType.SWB2Database;
					break;
				}
				case "":
				{
					// extension-less? could be anything, read it and try to make the best guess
					if ( fileStream.Length > 8 )
					{
						Int64 oldPosition = fileStream.Position;
						fileStream.Position = 0;

						using ( BinaryReader reader = new BinaryReader( fileStream, Encoding.UTF8, true ) )
						{
							UInt32 header1 = reader.ReadUInt32();
							UInt32 header2 = reader.ReadUInt32();

							if ( MathExtensions.IsPowerOfTwo( header1 ) && header2 == 0 )
								type = FileType.LocalizationContainer;
						}

						// make sure we don't screw up chained read calls
						fileStream.Position = oldPosition;
					}
					break;
				}
			}
			return type;
		}

		static public Form? GuessFromFileAndOpen( FileInfo fileInfoToGuess, Form? parentForm = null, bool silent = false )
		{
			try
			{
				using ( FileStream stream = fileInfoToGuess.OpenRead() )
				{
					return GuessFromBufferAndOpen( fileInfoToGuess.FullName, stream, parentForm, silent );
				}
			}
			catch ( Exception ex )
			{
				if ( !silent )
					MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				return null;
			}
		}

		static public Form? GuessFromBufferAndOpen( string? fileName, Stream fileStream, Form? parentForm = null, bool silent = false )
		{
			FormEditor? editor = null;
			switch ( GuessFromBuffer( fileName, fileStream ) )
			{
				case FileType.LocalizationContainer:
				{
					editor = new FormLocalizationEditor();
					break;
				}
				case FileType.SSGArchive:
				{
					editor = new FormSSGArchiveEditor();
					break;
				}
				case FileType.SWB2Database:
				{
					editor = new FormSWB2DatabaseEditor();
					break;
				}
				default:
				{
					if ( !silent )
					{
						// HACK: pass the file onto explorer to deal with it for us...
						Process fileProcess = new Process();
						fileProcess.StartInfo.FileName = "explorer";
						fileProcess.StartInfo.Arguments = "\"" + fileName + "\"";
						fileProcess.Start();
					}
					break;
				}
			}

			if ( editor != null )
			{
				editor.OpenFile( fileStream );
				if ( parentForm != null )
					editor.ShowDialog( parentForm );
				else
					editor.Show();
			}
			return editor;
		}
	}
}
