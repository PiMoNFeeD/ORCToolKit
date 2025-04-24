using System.Diagnostics;

namespace ORCToolKit
{
	namespace FileTypes
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
			static public FileType Guess( FileInfo FileInfoToGuess )
			{
				FileType Type = FileType.Unknown;

				// first, look at extension
				switch ( FileInfoToGuess.Extension )
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
						using ( FileStream Stream = FileInfoToGuess.OpenRead() )
						{
							using ( BinaryReader Reader = new BinaryReader( Stream ) )
							{
								UInt32 Header1 = Reader.ReadUInt32();
								UInt32 Header2 = Reader.ReadUInt32();

								if ( MathExtensions.IsPowerOfTwo( Header1 ) && Header2 == 0 )
									Type = FileType.LocalizationContainer;
							}
						}
						break;
					}
				}
				return Type;
			}

			static public void GuessAndOpen( FileInfo FileInfoToGuess )
			{
				switch ( Guess( FileInfoToGuess ) )
				{
					case FileType.LocalizationContainer:
					{
						FormLocalizationEditor LocalizationEditor = new FormLocalizationEditor();
						LocalizationEditor.OpenFile( FileInfoToGuess );
						LocalizationEditor.Show();
						break;
					}
					case FileType.SSGArchive:
					case FileType.SWB2Database:
					default:
					{
						if ( MessageBox.Show( Resources.ORCToolKit.FileTypeDeducerOpenUnknownText, Resources.ORCToolKit.FileTypeDeducerOpenUnknownTitle,
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning ) == DialogResult.Yes )
						{
							// HACK: pass the file onto explorer to deal with it for us...
							Process FileProcess = new Process();
							FileProcess.StartInfo.FileName = "explorer";
							FileProcess.StartInfo.Arguments = "\"" + FileInfoToGuess.FullName + "\"";
							FileProcess.Start();
						}
						break;
					}
				}
			}
		}
	}
}
