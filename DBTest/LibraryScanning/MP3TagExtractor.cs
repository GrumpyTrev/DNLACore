using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DBTest
{
	static class MP3TagExtractor
	{
		/// <summary>
		/// Convert encoded Layer value
		/// </summary>
		private static readonly int [] layerLookup = new int[] { 0, 3, 2, 1 };
		private static int Layer( byte rawLayer )
		{
			return layerLookup[ rawLayer ];
		}

		/// <summary>
		/// Determine bit rate from encoded Version, encoded Layer and encoded bit rate
		/// </summary>
		private static readonly int [ , , ] rateLookup = new int [, , ]
		{
			{ // Version 2.5
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Reserved
				{ 0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }, // Layer 3
				{ 0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }, // Layer 2
				{ 0,  32,  48,  56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256, 0 }  // Layer 1
			},
			{ // Reserved
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Invalid
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Invalid
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Invalid
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }  // Invalid
			},
			{ // Version 2
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Reserved
				{ 0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }, // Layer 3
				{ 0,   8,  16,  24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 0 }, // Layer 2
				{ 0,  32,  48,  56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256, 0 }  // Layer 1
			},
			{ // Version 1
				{ 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, 0 }, // Reserved
				{ 0,  32,  40,  48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 0 }, // Layer 3
				{ 0,  32,  48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, 0 }, // Layer 2
				{ 0,  32,  64,  96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0 }, // Layer 1
			}
		};
		private static int BitRate( byte rawVersion, byte rawLayer, byte rawRate )
		{
			return rateLookup[ rawVersion, rawLayer, rawRate ] * 1000;
		}

		/// <summary>
		/// Determine sample rate from encoded Version and encoded sample rate
		/// </summary>
		private static readonly int [ , ] sampleLookup = new int [ , ]
		{
			{ 11025, 12000,  8000, 0 }, // MPEG 2.5
			{     0,     0,     0, 0 }, // Reserved
			{ 22050, 24000, 16000, 0 }, // MPEG 2
			{ 44100, 48000, 32000, 0 }  // MPEG 1
		};
		private static int SampleRate( byte rawVersion, byte rawSampleRate )
		{
			return sampleLookup[ rawVersion, rawSampleRate ];
		}

		/// <summary>
		/// Determine samples per frame from encoded Version and encoded Level
		/// </summary>
		private static readonly int [ , ] samplesPerFrameLookup = new [ , ]
		{
			{      0,  576, 1152,  384 }, // 2.5
			{      0,    0,    0,    0 }, // Reserved
			{      0,  576, 1152,  384 }, // 2
			{      0, 1152, 1152,  384 }  // 1
		};
		private static int SamplesPerFrame( byte rawVersion, byte rawLevel )
		{
			return samplesPerFrameLookup[ rawVersion, rawLevel ];
		}

		/// <summary>
		/// Extract a big endian int from a byte array
		/// </summary>
		/// <param name="source">The byte array</param>
		/// <param name="offset">Offset to get bytes from</param>
		/// <returns></returns>
		private static UInt32 GetBigEndianInt( this byte[] source, int offset )
		{
			byte[] subArray = new byte[ 4 ];

			Array.Copy( source, offset, subArray, 0, 4 );
			Array.Reverse( subArray );

			return BitConverter.ToUInt32( subArray, 0 );
		}

		/// <summary>
		/// Extract MP3 tags and duration from the MP3 stream
		/// </summary>
		/// <param name="fileStream"></param>
		/// <returns></returns>
		public static MP3Tags GetFileTags( Stream fileStream )
		{
			MP3Tags tags = new MP3Tags();

			using ( BinaryReader reader = new BinaryReader( fileStream ) )
			{
				try
				{
					// Make sure the file is at least 10 bytes long to determine the size of the ID3 header
					if ( reader.BaseStream.Length >= 10 )
					{
						// Look at the start of the file for the ID3 tags
						if ( new string( reader.ReadChars( 3 ) ) == "ID3" )
						{
							// Start extracting metadata
							// ID3 version
							byte majorVersion = reader.ReadByte();

							// Skip the minor version
							reader.ReadByte();

							// Skip next 'flags' byte
							reader.ReadByte();

							// The size of the ID3 header
							ulong headerSize = ReadEncodedSize( reader, 4 );
							if ( headerSize > 0 )
							{
								// Get all the required ID3 tags
								int id3EndPosition = ( int )headerSize + 10;
								ReadFrames( reader, majorVersion, id3EndPosition, tags );

								// Step on past the ID3 frames and attempt to read the first MP3 frame
								if ( ( reader.BaseStream.Position < id3EndPosition ) && ( reader.BaseStream.Length >= id3EndPosition ) )
								{
									reader.ReadBytes( id3EndPosition - ( int )reader.BaseStream.Position );

									// The first MP3 frame does not always follow the end of the ID3 header, so go searching for the
									// MP3 synch pattern
									bool mp3HeaderFound = false;
									long readLimit = reader.BaseStream.Position + 0x1000;
									long searchStart = reader.BaseStream.Position;

									byte[] mp3Header = new byte[] { 0, 0, 0 };

									while ( ( mp3HeaderFound == false ) && ( reader.BaseStream.Position < reader.BaseStream.Length ) &&
										( reader.BaseStream.Position < readLimit ) )
									{
										mp3Header[ 1 ] = reader.ReadByte();
										if ( ( mp3Header[ 0 ] == 0xFF ) && ( ( mp3Header[ 1 ] & 0xE0 ) == 0xE0 ) )
										{
											// Synch bytes found
											mp3HeaderFound = true;

											// Get the last byte of the header
											mp3Header[ 2] = reader.ReadByte();

											// Parse the MP3 frame and determine the duration of the file
											tags.Length = ParseMp3Frame( reader, mp3Header );
										}
										else
										{
											mp3Header[ 0 ] = mp3Header[ 1 ];
										}
									}

									if ( mp3HeaderFound == false )
									{
										Logger.Log(	string.Format( "MP3 frame not found between {0} and {1}", searchStart, reader.BaseStream.Position ) );
									}
								}
								else
								{
									Logger.Log( string.Format( "ID3 frame is truncated" ) );
								}
							}
						}
						else
						{
							Logger.Log( string.Format( "Header is not ID3" ) );
						}
					}
					else
					{
						Logger.Log( string.Format( "Could not read header" ) );
					}
				}
				catch ( Exception tagsException )
				{
					Logger.Log( string.Format( "Problem reading file: {0}", tagsException.Message ) );
				}
			}

			return tags;
		}

		/// <summary>
		/// Parse the first MP3 frame and determine the duration of the file
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="header"></param>
		/// <returns></returns>
		private static TimeSpan ParseMp3Frame( BinaryReader reader, byte[] header )
		{
			// Record the current position of the reader so that the correct data length can be used to determine the duration
			long startOfMP3Data = reader.BaseStream.Position;

			// Get the header details
			byte rawVersion = ( byte )( ( header[ 1 ] & 0x18 ) >> 3 );
			byte rawLayer = ( byte )( ( header[ 1 ] & 0x06 ) >> 1 );
			byte rawRate = ( byte )( ( header[ 2 ] & 0xF0 ) >> 4 );
			byte rawSampleRate = ( byte )( ( header[ 2 ] & 0x0C ) >> 2 );
			byte padding = ( byte )( ( header[ 2 ] & 0x02 ) >> 1 );
			int sampleRate = SampleRate( rawVersion, rawSampleRate );

			// Now work out the duration of the file.
			double duration = 0;

			// First need to determine if it is a constant bit rate or variable bit rate file by looking in the data part of the frame
			// Work out the length of the data part of the frame
			int dataLength = 0;

			// Can't determine this if the sample rate is invalid
			if ( sampleRate > 0 )
			{
				if ( Layer( rawLayer ) == 1 )
				{
					dataLength = ( ( ( 12 * BitRate( rawVersion, rawLayer, rawRate ) ) / sampleRate ) + padding ) * 4;
				}
				else
				{
					dataLength = ( ( 144 * BitRate( rawVersion, rawLayer, rawRate ) ) / sampleRate ) + padding;
				}
			}
			else
			{
				Logger.Log(	string.Format( "Invalid sample rate for Version {0} and raw sample rate {1}", rawVersion, rawSampleRate ) );
			}

			string bitRateEncoding = "None";

			if ( dataLength > 0 )
			{
				// Now look for the VBR tags 'Xing', 'Info' and 'VBRI'
				uint noFrames = 0;

				// Only need to look in the first 60 or so bytes
				int bufferSize = Math.Min( dataLength, 60 );
				if ( ( reader.BaseStream.Position + bufferSize ) <= reader.BaseStream.Length )
				{
					byte[] mp3DataBuffer = reader.ReadBytes( bufferSize );

					// If VBR header present Xing, Info or VBRI should be present at offset 33
					string possibleVBR = Encoding.UTF8.GetString( mp3DataBuffer, 33, 4 );
					if ( ( possibleVBR == "Xing" ) || ( possibleVBR == "Info" ) )
					{
						int vbrOffset = 37;

						// Read flags uint
						uint flags = mp3DataBuffer.GetBigEndianInt( vbrOffset );
						vbrOffset += 4;

						if ( ( flags & 0x01 ) == 0x01 )
						{
							// Read number of frames
							noFrames = mp3DataBuffer.GetBigEndianInt( vbrOffset );
						}

						bitRateEncoding = "VBR";
					}
					else if ( possibleVBR == "VBRI" )
					{
						// Read number of frames as UInt32 after skipping 14 bytes
						int offset = 47;

						noFrames = mp3DataBuffer.GetBigEndianInt( offset );
						bitRateEncoding = "VBRI";
					}
				}
				else
				{
					Logger.Log( string.Format( "MP3 frame is truncated" ) );
				}

				if ( noFrames == 0 )
				{
					// CBR
					int bitRate = BitRate( rawVersion, rawLayer, rawRate );
					if ( bitRate > 0 )
					{
						duration = ( ( ( int )reader.BaseStream.Length - startOfMP3Data ) * 8 ) / bitRate;
					}
					else
					{
						Logger.Log( string.Format( "Invalid bit rate for Version {0} Layer {1} and raw bitrate {2}", rawVersion, rawLayer, rawRate ) );
					}

					bitRateEncoding = "CBR";
				}
				else
				{
					// VBR
					if ( sampleRate > 0 )
					{
						duration = ( ( ( ( int )noFrames + 1 ) * SamplesPerFrame( rawVersion, rawLayer ) ) / sampleRate );
					}
				}
			}

			TimeSpan durationInSeconds = TimeSpan.FromSeconds( Math.Ceiling( duration ) );

			Logger.Log( string.Format( "Version {0}, Level {1}, Rate {2}, Sample {3}, Padding {4}, Length {5}. BR encoding {6}, Duration {7}",
				rawVersion, Layer( rawLayer ), BitRate( rawVersion, rawLayer, rawRate ), SampleRate( rawVersion, rawSampleRate ), padding, dataLength,
				bitRateEncoding, durationInSeconds.ToString( @"hh\:mm\:ss" ) ) );

			return durationInSeconds;
		}

		/// <summary>
		/// Read the ID3 meta data extracting required tag values 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="majorVersion"></param>
		/// <param name="maxPosition"></param>
		/// <param name="tags"></param>
		private static void ReadFrames( BinaryReader reader, int majorVersion, int maxPosition, MP3Tags tags )
		{
			// These are the tags that are currently being searched for
			HashSet<string> requiredTags = ( majorVersion == 2 ) ? new HashSet<string>() { "TP1", "TP2", "TT2", "TAL", "TRK" } 
					: new HashSet<string>() { "TPE1", "TPE2", "TIT2", "TALB", "TRCK" };

			int nameSize = ( majorVersion == 2 ) ? 3 : 4;

			// Put the tag contents here
			Dictionary<string, byte[]> frames = new Dictionary<string, byte[]>();

			bool endOfTags = false;
			while ( ( reader.BaseStream.Position < maxPosition ) && ( endOfTags == false ) )
			{
				Logger.Log( string.Format( "Position at start of frame {0}", reader.BaseStream.Position ) );

				// Check for end of the frames
				char frameNameFirstChar = reader.ReadChar();
				if ( frameNameFirstChar == '\0' )
				{
					endOfTags = true;
				}
				else
				{
					// Get the rest of the tag name and its contents
					string frameName = frameNameFirstChar + new string( reader.ReadChars( nameSize - 1 ) );

					ulong frameSize = ReadFrameSize( reader, nameSize, majorVersion );

					Logger.Log( string.Format( "Frame {0}, size {1}", frameName, frameSize ) );

					// Skip a couple of bytes and then read the frame contents
					reader.ReadByte();
					reader.ReadByte();

					// Only store if it is one of the required tags
					if ( requiredTags.Contains( frameName ) == true )
					{
						frames[ frameName ] = ( frameSize > 0 ) ? reader.ReadBytes( ( int )frameSize ) : new byte[ 0 ];
						requiredTags.Remove( frameName );
						endOfTags = ( requiredTags.Count == 0 );
					}
					else
					{
						// Still need to skip over the frame contents
						if ( frameSize > 0 )
						{
							Logger.Log( string.Format( "Position before skipping {0} bytes is {1}", frameSize, reader.BaseStream.Position ) );
							reader.ReadBytes( ( int )frameSize );
							Logger.Log( string.Format( "Position after skipping bytes {0}", reader.BaseStream.Position ) );
						}
					}
				}
			}

			// Now all the required frames have been obtained, extract the contents and assign to the tag instance
			if ( majorVersion == 2 )
			{
				tags.Artist = GetStringTag( frames, "TP1" );
				tags.AlbumArtist = GetStringTag( frames, "TP2" );
				tags.Title = GetStringTag( frames, "TT2" );
				tags.Album = GetStringTag( frames, "TAL" );
				tags.Track = GetStringTag( frames, "TRK" );
			}
			else
			{
				tags.Artist = GetStringTag( frames, "TPE1" );
				tags.AlbumArtist = GetStringTag( frames, "TPE2" );
				tags.Title = GetStringTag( frames, "TIT2" );
				tags.Album = GetStringTag( frames, "TALB" );
				tags.Track = GetStringTag( frames, "TRCK" );
			}

			//			Log.WriteLine( LogPriority.Debug, "MobileApp:ReadFrames", string.Format( "Artist: {0} Title: {1} Album: {2} Track: {3}", tags.Artist, tags.Title, tags.Album,
			//				tags.Track ) );
		}

		/// <summary>
		/// Read a size value encoded as 3 or 4 7-bit bytes
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="noBytes"></param>
		/// <returns></returns>
		private static ulong ReadEncodedSize( BinaryReader reader, int noBytes )
		{
			ulong size = 0;

			byte[] tagSize = reader.ReadBytes( noBytes );
			byte[] bytes = new byte[ noBytes ];

			if ( noBytes == 4 )
			{
				bytes[ 3 ] = ( byte )( tagSize[ 3 ] | ( ( tagSize[ 2 ] & 1 ) << 7 ) );
				bytes[ 2 ] = ( byte )( ( ( tagSize[ 2 ] >> 1 ) & 63 ) | ( ( tagSize[ 1 ] & 3 ) << 6 ) );
				bytes[ 1 ] = ( byte )( ( ( tagSize[ 1 ] >> 2 ) & 31 ) | ( ( tagSize[ 0 ] & 7 ) << 5 ) );
				bytes[ 0 ] = ( byte )( ( tagSize[ 0 ] >> 3 ) & 15 );

				size = ( ulong )( bytes[ 3 ] + ( bytes[ 2 ] << 8 ) + ( bytes[ 1 ] << 16 ) + ( bytes[ 0 ] << 24 ) );
			}
			else
			{
				bytes[ 2 ] = ( byte )( tagSize[ 2 ] | ( ( tagSize[ 1 ] & 1 ) << 7 ) );
				bytes[ 1 ] = ( byte )( ( ( tagSize[ 1 ] >> 1 ) & 63 ) | ( ( tagSize[ 0 ] & 3 ) << 6 ) );
				bytes[ 0 ] = ( byte )( ( tagSize[ 0 ] >> 2 ) & 31 );

				size = ( ulong )( bytes[ 2 ] + ( bytes[ 1 ] << 8 ) + ( bytes[ 0 ] << 16 ) );
			}

			return size;
		}

		/// <summary>
		/// Read either a 3 or 4 byte big endian value, each byte is only 7 bits long
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="noBytes"></param>
		/// <returns></returns>
		private static ulong ReadFrameSize( BinaryReader reader, int noBytes, int majorVersion )
		{
			ulong size = 0;

			byte[] tagSize = reader.ReadBytes( noBytes );

			if ( noBytes == 4 )
			{
				if ( majorVersion == 4 )
				{
					size = ( ulong )( ( tagSize[ 3 ] & 127 ) + ( ( tagSize[ 2 ] & 127 ) << 7 ) + ( ( tagSize[ 1 ] & 127 ) << 14 ) + ( ( tagSize[ 0 ] & 127 ) << 21 ) );
				}
				else
				{
					size = ( ulong )( tagSize[ 3 ] + ( tagSize[ 2 ] << 8 ) + ( tagSize[ 1 ] << 16 ) + ( tagSize[ 0 ] << 24 ) );
				}
			}
			else
			{
				size = ( ulong )( tagSize[ 2 ] + ( tagSize[ 1 ] << 8 ) + ( tagSize[ 0 ] << 16 ) );
			}

			return size;
		}

		/// <summary>
		/// Get the byte array associated with a tag and convert it to a string according to the included encoding
		/// </summary>
		/// <param name="hashTable"></param>
		/// <param name="tagType"></param>
		/// <returns></returns>
		private static string GetStringTag( Dictionary<string, byte[]> hashTable, string tagType )
		{
			string tag = "";

			if ( hashTable.ContainsKey( tagType ) == true )
			{
				byte[] contents = hashTable[ tagType ];
				byte encoding = contents[ 0 ];

				if ( ( encoding == 0 ) || ( encoding == 3 ) )
				{
					// LATIN and UTF-8
					tag = Encoding.GetEncoding( "ISO-8859-1" ).GetString( contents, 1, contents.Length - 1 );
				}
				else if ( encoding == 1 )
				{
					// UCS-2 with 2 byte header
					tag = Encoding.Unicode.GetString( contents, 3, contents.Length - 3 );
				}
				else
				{
					// UTF-16E without 2 byte header
					tag = Encoding.Unicode.GetString( contents, 1, contents.Length - 1 );
				}
			}

			return tag.TrimEnd( '\0' );
		}
	}

	class MP3Tags
	{
		public string Artist { get; set; } = "";
		public string Title { get; set; } = "";
		public string Album { get; set; } = "";
		public string Track { get; set; } = "";
		public string AlbumArtist { get; set; } = "";
		public TimeSpan Length { get; set; } = new TimeSpan();
	}
}