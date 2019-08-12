using System;
using System.Collections;
using System.Text;
using System.IO;

namespace mp3info
{
	/// <summary>
	/// Summary description for ID3v2.
	/// </summary>
	public class ID3v2
	{
		public string filename;

		// id3v2 header
		public int MajorVersion;
		public int MinorVersion;
		
		public bool FA_Unsynchronisation;
		public bool FB_ExtendedHeader;
		public bool FC_ExperimentalIndicator;
		public bool FD_Footer;

		// ext header 
		public ulong extHeaderSize;
		public int extNumFlagBytes;

		public bool EB_Update;
		public bool EC_CRC;
		public bool ED_Restrictions;

		public ulong extC_CRC;
		public byte extD_Restrictions;



		private BinaryReader br;
		private byte[] ba;
		public bool hasTag;

		public ulong headerSize;
		
		public Hashtable framesHash;
		public ArrayList frames;

		private bool fileOpen;

		//public bool header;

		// song info
		public string Title;
		public string Artist;
		public string Album;
		public string Year;
		public string Comment;
		public string Genre;
		public string Track;
		public string totalTracks;
		



		private void Initialize_Components()
		{
			this.filename = "";

		
			this.MajorVersion = 0;
			this.MinorVersion = 0;

			this.FA_Unsynchronisation = false;
			this.FB_ExtendedHeader = false;
			this.FC_ExperimentalIndicator = false;

			this.fileOpen = false;

			this.frames = new ArrayList();
			this.framesHash = new Hashtable();

			this.Album = "";
			this.Artist = "";
			this.Comment = "";
			this.extC_CRC = 0;
			this.EB_Update = false;
			this.EC_CRC = false;
			this.ED_Restrictions = false;
			this.extD_Restrictions = 0;
			this.extHeaderSize = 0;
			this.extNumFlagBytes = 0;
			this.fileOpen = false;
			this.hasTag = false;
			this.headerSize = 0;
			this.Title = "";
			this.totalTracks = "";
			this.Track = "";
			this.Year = "";
			
		}


		public ID3v2()
		{
			Initialize_Components();
		}
		 
		public ID3v2( string fileName)
		{
			Initialize_Components();
			this.filename = fileName;
		}



	
		private void CloseFile()
		{
			br.Close();
			
		}



		private void ReadHeader ( )
		{


			// bring in the first three bytes.  it must be ID3 or we have no tag
			// TODO add logic to check the end of the file for "3D1" and other
			// possible starting spots
			string id3start = new string (br.ReadChars(3));

			// check for a tag
			if  (!id3start.Equals("ID3"))
			{
				// TODO we are fucked.
				//throw id3v2ReaderException;
				this.hasTag = false;
				return;

			}
			else
			{
				this.hasTag = true;

				// read id3 version.  2 bytes:
				// The first byte of ID3v2 version is it's major version,
				// while the second byte is its revision number
				this.MajorVersion = System.Convert.ToInt32(br.ReadByte());
				this.MinorVersion = System.Convert.ToInt32(br.ReadByte());
		
				//read next byte for flags
				bool [] boolar = BitReader.ToBitBool(br.ReadByte());
				// set the flags
				this.FA_Unsynchronisation= boolar[0];
				this.FB_ExtendedHeader = boolar[1];
				this.FC_ExperimentalIndicator = boolar[2];

				// read teh size
				// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
				//Dan Code 
				char[] tagSize = br.ReadChars(4);    // I use this to read the bytes in from the file
				int[] bytes = new int[4];      // for bit shifting
				ulong newSize = 0;    // for the final number
				// The ID3v2 tag size is encoded with four bytes
				// where the most significant bit (bit 7)
				// is set to zero in every byte,
				// making a total of 28 bits.
				// The zeroed bits are ignored
				//
				// Some bit grinding is necessary.  Hang on.
			

				bytes[3] =  tagSize[3]             | ((tagSize[2] & 1) << 7) ;
				bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6) ;
				bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5) ;
				bytes[0] = ((tagSize[0] >> 3) & 15) ;

				newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
					((UInt64)bytes[2] << 8)  |
					((UInt64)bytes[1] << 16) |
					((UInt64)bytes[0] << 24)) ;
				//End Dan Code
		
				this.headerSize = newSize;
			}
		}

		private void ReadFrames ()
		{
			id3v2Frame f = new id3v2Frame();
			do 
			{
				f = f.ReadFrame(br, this.MajorVersion);
				
				// check if we have hit the padding.
				if (f.padding == true)
				{
					//we hit padding.  lets advance to end and stop reading.
					br.BaseStream.Position = System.Convert.ToInt64(headerSize);
					break;
				}
				this.frames.Add(f);
				this.framesHash.Add(f.frameName, f);
				#region frameprocessing

				/*
				else 
				{
					// figure out which type it is
					if (f.frameName.StartsWith("T"))
					{
						if (f.frameName.Equals("TXXX"))
						{
							ProcessTXXX(f);
						}
						else 
						{
							ProcessTTYPE(f);
						}
					}
					else
					{
						if (f.frameName.StartsWith("W"))
						{
							if (f.frameName.Equals("WXXX"))
							{
								ProcessWXXX(f);
							}
							else 
							{
								ProcessWTYPE(f);
							}
						}
						else
						{
							// if it isn't  a muliple reader case (above) then throw it into the switch to process
							switch (f.frameName)
							{
							
								case "IPLS":
									ProcessIPLS(f);
									break;
								case "MCDI":
									ProcessMCDI(f);
									break;
								case "UFID":
									ProcessUFID(f);
									break;
								case "COMM":
									ProcessCOMM(f);
									break;
									
								default:
									frames.Add(f.frameName, f.frameContents);
									AddItemToList(f.frameName, "non text");
									break;
							}
				}
			
		}


			}*/
				#endregion
			} while (br.BaseStream.Position  <= System.Convert.ToInt64(this.headerSize));
		}		
		
		public void Read()
		{
						
			FileStream fs = new FileStream(this.filename, FileMode.Open, FileAccess.Read);
			br = new BinaryReader (fs);

			ReadHeader();

			if (this.hasTag)
			{
				if (this.FB_ExtendedHeader)
				{
					ReadExtendedHeader();
				}

			
				
			
				ReadFrames();
		
				if (this.FD_Footer)
				{
					ReadFooter();
				}
				ParseCommonHeaders();
			}

			fs.Close();
			br.Close();

		


			#region tag reader

			/*if (!fileOpen)
					{
						if (!this.filename == "")
						{
							OpenFile();
						}
						else
						{
							// we are fucked.  throw an exception or something
						}
					}
					// bring in the first three bytes.  it must be ID3 or we have no tag
					// TODO add logic to check the end of the file for "3D1" and other
					// possible starting spots
					string id3start = new string (br.ReadChars(3));

					// check for a tag
					if  (!id3start.Equals("ID3"))
					{
						// TODO we are fucked.  not really we just don't ahve a tag
						// and we need to bail out gracefully.
						throw id3v23ReaderException;
					}
					else 
					{
						// we have a tag
						this.header = true;
					}

					// read id3 version.  2 bytes:
					// The first byte of ID3v2 version is it's major version,
					// while the second byte is its revision number
					this.MajorVersion = System.Convert.ToInt32(br.ReadByte());
					this.MinorVersion = System.Convert.ToInt32(br.ReadByte());

					// here is where we get fancy.  I am useing silisoft's php code as 
					// a reference here.  we are going to try and parse for 2.2, 2.3 and 2.4
					// in one pass.  hold on!!

					if ((this.header) && (this.MajorVersion <= 4)) // probably won't work on higher versions
					{
						// (%ab000000 in v2.2, %abc00000 in v2.3, %abcd0000 in v2.4.x)
						//read next byte for flags
						bool [] boolar = BitReader.ToBitBool(br.ReadByte());
						// set the flags
						if (this.MajorVersion == 2)
						{
							this.FA_Unsyncronisation = boolar[0];
							this.FB_ExtendedHeader = boolar[1];
						}
						else if ( this.MajorVersion == 3 )
						{
							// set the flags
							this.FA_Unsyncronisation = boolar[0];
							this.FB_ExtendedHeader = boolar[1];
							this.FC_ExperimentalIndicator = boolar[2];
						}
						else if (this.MajorVersion == 4)
						{
							// set the flags
							this.FA_Unsyncronisation = boolar[0];
							this.FB_ExtendedHeader = boolar[1];
							this.FC_ExperimentalIndicator = boolar[2];
							this.FD_Footer = boolar[3];
						}

						// read teh size
						// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
						//Dan Code 
						char[] tagSize = br.ReadChars(4);    // I use this to read the bytes in from the file
						int[] bytes = new int[4];      // for bit shifting
						ulong newSize = 0;    // for the final number
						// The ID3v2 tag size is encoded with four bytes
						// where the most significant bit (bit 7)
						// is set to zero in every byte,
						// making a total of 28 bits.
						// The zeroed bits are ignored
						//
						// Some bit grinding is necessary.  Hang on.
				
			

						bytes[3] =  tagSize[3]             | ((tagSize[2] & 1) << 7) ;
						bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6) ;
						bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5) ;
						bytes[0] = ((tagSize[0] >> 3) & 15) ;

						newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
							((UInt64)bytes[2] << 8)  |
							((UInt64)bytes[1] << 16) |
							((UInt64)bytes[0] << 24)) ;
						//End Dan Code
		
						this.id3v2HeaderSize = newSize;
			

					}
					*/
				#endregion
		}

		private void ParseCommonHeaders()
		{
			if (this.MajorVersion == 2)
			{
				if(this.framesHash.Contains("TT2"))
				{
					
					byte [] bytes = ((id3v2Frame)this.framesHash["TT2"]).frameContents;
					StringBuilder sb = new StringBuilder();
					byte textEncoding;
					

					for (int i = 1; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						
						//this.Title = myString.Substring(1);
						this.Title = sb.ToString();
					}
				}
			
		

				if(this.framesHash.Contains("TP1"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TP1"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Artist = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TAL"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TAL"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Album = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TYE"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TYE"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Year = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TRK"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TRK"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Track = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TCO"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TCO"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Genre = sb.ToString();
					}
				}
				if(this.framesHash.Contains("COM"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["COM"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Comment = sb.ToString();
					}
				}
			}
			else 
			{ // $id3info["majorversion"] > 2
				if(this.framesHash.Contains("TIT2"))
				{
					
					byte [] bytes = ((id3v2Frame)this.framesHash["TIT2"]).frameContents;
					StringBuilder sb = new StringBuilder();
					byte textEncoding;
					

					for (int i = 1; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						
						//this.Title = myString.Substring(1);
						this.Title = sb.ToString();
					}
				}
			
		

				if(this.framesHash.Contains("TPE1"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TPE1"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Artist = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TALB"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TALB"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Album = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TYER"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TYER"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Year = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TRCK"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TRCK"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Track = sb.ToString();
					}
				}
				if(this.framesHash.Contains("TCON"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["TCON"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Genre = sb.ToString();
					}
				}
				if(this.framesHash.Contains("COMM"))
				{
					StringBuilder sb = new StringBuilder();
					byte [] bytes = ((id3v2Frame)this.framesHash["COMM"]).frameContents;
					byte textEncoding;

					for (int i = 0; i < bytes.Length; i++)
					{
						if (i == 0)
						{
							//read the text encoding.
							textEncoding = bytes[i];
						}
						else
						{
							sb.Append(System.Convert.ToChar(bytes[i]));
						}
						this.Comment = sb.ToString();
					}
				}
			}
	
			string [] trackHolder = this.Track.Split('/');
			this.Track = trackHolder[0];
			if (trackHolder.Length > 1)
				this.totalTracks = trackHolder[1];
				
		}

		private void ReadFooter()
		{
			
			// bring in the first three bytes.  it must be ID3 or we have no tag
			// TODO add logic to check the end of the file for "3D1" and other
			// possible starting spots
			string id3start = new string (br.ReadChars(3));

			// check for a tag
			if  (!id3start.Equals("3DI"))
			{
				// TODO we are fucked.  not really we just don't ahve a tag
				// and we need to bail out gracefully.
				//throw id3v23ReaderException;
			}
			else 
			{
				// we have a tag
				this.hasTag = true;
			}

			// read id3 version.  2 bytes:
			// The first byte of ID3v2 version is it's major version,
			// while the second byte is its revision number
			this.MajorVersion = System.Convert.ToInt32(br.ReadByte());
			this.MinorVersion = System.Convert.ToInt32(br.ReadByte());

			// here is where we get fancy.  I am useing silisoft's php code as 
			// a reference here.  we are going to try and parse for 2.2, 2.3 and 2.4
			// in one pass.  hold on!!

			if ((this.hasTag) && (this.MajorVersion <= 4)) // probably won't work on higher versions
			{
				// (%ab000000 in v2.2, %abc00000 in v2.3, %abcd0000 in v2.4.x)
				//read next byte for flags
				bool [] boolar = BitReader.ToBitBool(br.ReadByte());
				// set the flags
				if (this.MajorVersion == 2)
				{
					this.FA_Unsynchronisation = boolar[0];
					this.FB_ExtendedHeader = boolar[1];
				}
				else if ( this.MajorVersion == 3 )
				{
					// set the flags
					this.FA_Unsynchronisation = boolar[0];
					this.FB_ExtendedHeader = boolar[1];
					this.FC_ExperimentalIndicator = boolar[2];
				}
				else if (this.MajorVersion == 4)
				{
					// set the flags
					this.FA_Unsynchronisation = boolar[0];
					this.FB_ExtendedHeader = boolar[1];
					this.FC_ExperimentalIndicator = boolar[2];
					this.FD_Footer = boolar[3];
				}

				// read teh size
				// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
				//Dan Code 
				char[] tagSize = br.ReadChars(4);    // I use this to read the bytes in from the file
				int[] bytes = new int[4];      // for bit shifting
				ulong newSize = 0;    // for the final number
				// The ID3v2 tag size is encoded with four bytes
				// where the most significant bit (bit 7)
				// is set to zero in every byte,
				// making a total of 28 bits.
				// The zeroed bits are ignored
				//
				// Some bit grinding is necessary.  Hang on.
				
			

				bytes[3] =  tagSize[3]             | ((tagSize[2] & 1) << 7) ;
				bytes[2] = ((tagSize[2] >> 1) & 63) | ((tagSize[1] & 3) << 6) ;
				bytes[1] = ((tagSize[1] >> 2) & 31) | ((tagSize[0] & 7) << 5) ;
				bytes[0] = ((tagSize[0] >> 3) & 15) ;

				newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
					((UInt64)bytes[2] << 8)  |
					((UInt64)bytes[1] << 16) |
					((UInt64)bytes[0] << 24)) ;
				//End Dan Code
		
				this.headerSize = newSize;
			

			}
		}

		private void ReadExtendedHeader()
		{

			//			Extended header size   4 * %0xxxxxxx
			//			Number of flag bytes       $01
			//			Extended Flags             $xx
			//			Where the 'Extended header size' is the size of the whole extended header, stored as a 32 bit synchsafe integer.
			// read teh size
			// this code is courtesy of Daniel E. White w/ minor modifications by me  Thanx Dan
			//Dan Code 
			char[] extHeaderSize = br.ReadChars(4);    // I use this to read the bytes in from the file
			int[] bytes = new int[4];      // for bit shifting
			ulong newSize = 0;    // for the final number
			// The ID3v2 tag size is encoded with four bytes
			// where the most significant bit (bit 7)
			// is set to zero in every byte,
			// making a total of 28 bits.
			// The zeroed bits are ignored
			//
			// Some bit grinding is necessary.  Hang on.
			

			bytes[3] =  extHeaderSize[3]              | ((extHeaderSize[2] & 1) << 7) ;
			bytes[2] = ((extHeaderSize[2] >> 1) & 63) | ((extHeaderSize[1] & 3) << 6) ;
			bytes[1] = ((extHeaderSize[1] >> 2) & 31) | ((extHeaderSize[0] & 7) << 5) ;
			bytes[0] = ((extHeaderSize[0] >> 3) & 15) ;

			newSize  = ((UInt64)10 +	(UInt64)bytes[3] |
				((UInt64)bytes[2] << 8)  |
				((UInt64)bytes[1] << 16) |
				((UInt64)bytes[0] << 24)) ;
			//End Dan Code

			this.extHeaderSize = newSize;
			// next we read the number of flag bytes

			this.extNumFlagBytes = System.Convert.ToInt32(br.ReadByte());

			// read the flag byte(s) and set the flags
			bool[] extFlags = BitReader.ToBitBools(br.ReadBytes(this.extNumFlagBytes));

			this.EB_Update = extFlags[1];
			this.EC_CRC = extFlags[2];
			this.ED_Restrictions = extFlags[3];

			// check for flags
			if (this.EB_Update)
			{
				// this tag has no data but will have a null byte so we need to read it in
				//Flag data length      $00

				br.ReadByte();
			}

			if (this.EC_CRC)
			{
				//        Flag data length       $05
				//       Total frame CRC    5 * %0xxxxxxx
				// read the first byte and check to make sure it is 5.  if not the header is corrupt
				// we will still try to process but we may be funked.

				int extC_FlagDataLength = System.Convert.ToInt32(br.ReadByte());
				if (extC_FlagDataLength == 5)
				{


					extHeaderSize = br.ReadChars(5);    // I use this to read the bytes in from the file
					bytes = new int[4];      // for bit shifting
					newSize = 0;    // for the final number
					// The ID3v2 tag size is encoded with four bytes
					// where the most significant bit (bit 7)
					// is set to zero in every byte,
					// making a total of 28 bits.
					// The zeroed bits are ignored
					//
					// Some bit grinding is necessary.  Hang on.
			

					bytes[4] = extHeaderSize[4]		|  ((extHeaderSize[3] & 1) << 7 ) ;
					bytes[3] = ((extHeaderSize[3] >> 1) & 63) | ((extHeaderSize[2] & 3) << 6) ;
					bytes[2] = ((extHeaderSize[2] >> 2) & 31) | ((extHeaderSize[1] & 7) << 5) ;
					bytes[1] = ((extHeaderSize[1] >> 3) & 15) | ((extHeaderSize[0] & 15) << 4) ;
					bytes[0] = ((extHeaderSize[0] >> 4) & 7);

					newSize  = ((UInt64)10 +	(UInt64)bytes[4] |
						((UInt64)bytes[3] << 8)  |
						((UInt64)bytes[2] << 16) |
						((UInt64)bytes[1] << 24) |
						((UInt64)bytes[0] << 32)) ;
				
					this.extHeaderSize = newSize;
				}
				else
				{
					// we are fucked.  do something
				}
			}

			if (this.ED_Restrictions)
			{
				// Flag data length       $01
				//Restrictions           %ppqrrstt
				
				// advance past flag data lenght byte
				br.ReadByte();

				this.extD_Restrictions = br.ReadByte();

			}

		}

	}

}
