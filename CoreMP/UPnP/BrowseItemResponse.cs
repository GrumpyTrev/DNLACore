using System.IO;
using System.Xml.Serialization;

namespace CoreMP
{
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	[System.Xml.Serialization.XmlRootAttribute( "DIDL-Lite", Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/", IsNullable = false )]
	public partial class BrowseItemResponse
	{
		public static BrowseItemResponse DeserialiseResponse( string response )
		{
			BrowseItemResponse result = null;

			// Remove all namespace prefixes
			string anonResponse = response.Replace( "dc:", "" ).Replace( "pv:", "" ).Replace( "upnp:", "" ).Replace( "dlna:", "" );

			using ( TextReader reader = new StringReader( anonResponse ) )
			{
				result = ( BrowseItemResponse )new XmlSerializer( typeof( BrowseItemResponse ) ).Deserialize( reader );
			}

			return result;
		}

		private BrowseItem[] itemsField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "item" )]
		public BrowseItem[] Items
		{
			get
			{
				return this.itemsField;
			}
			set
			{
				this.itemsField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class BrowseItem
	{
		private string titleField;

		private string dateField;

		private string genreField;

		private string albumField;

		private string originalTrackNumberField;

		private string creatorField;

		private string extensionField;

		private string albumArtistField;

		private string modificationTimeField;

		private string addedTimeField;

		private string lastUpdatedField;

		private string album_crosslinkField;

		private string artist_crosslinkField;

		private string genre_crosslinkField;

		private string bookmarkField;

		private string classField;

		private DIDLLiteItemAlbumArtURI albumArtURIField;

		private DIDLLiteItemArtist[] artistField;

		private DIDLLiteItemAuthor[] authorField;

		private DIDLLiteItemRes resField;

		private string idField;

		private string parentIDField;

		private string restrictedField;

		/// <remarks/>
		public string title
		{
			get
			{
				return this.titleField;
			}
			set
			{
				this.titleField = value;
			}
		}

		/// <remarks/>
		public string date
		{
			get
			{
				return this.dateField;
			}
			set
			{
				this.dateField = value;
			}
		}

		/// <remarks/>
		public string genre
		{
			get
			{
				return this.genreField;
			}
			set
			{
				this.genreField = value;
			}
		}

		/// <remarks/>
		public string album
		{
			get
			{
				return this.albumField;
			}
			set
			{
				this.albumField = value;
			}
		}

		/// <remarks/>
		public string originalTrackNumber
		{
			get
			{
				return this.originalTrackNumberField;
			}
			set
			{
				this.originalTrackNumberField = value;
			}
		}

		/// <remarks/>
		public string creator
		{
			get
			{
				return this.creatorField;
			}
			set
			{
				this.creatorField = value;
			}
		}

		/// <remarks/>
		public string extension
		{
			get
			{
				return this.extensionField;
			}
			set
			{
				this.extensionField = value;
			}
		}

		/// <remarks/>
		public string albumArtist
		{
			get
			{
				return this.albumArtistField;
			}
			set
			{
				this.albumArtistField = value;
			}
		}

		/// <remarks/>
		public string modificationTime
		{
			get
			{
				return this.modificationTimeField;
			}
			set
			{
				this.modificationTimeField = value;
			}
		}

		/// <remarks/>
		public string addedTime
		{
			get
			{
				return this.addedTimeField;
			}
			set
			{
				this.addedTimeField = value;
			}
		}

		/// <remarks/>
		public string lastUpdated
		{
			get
			{
				return this.lastUpdatedField;
			}
			set
			{
				this.lastUpdatedField = value;
			}
		}

		/// <remarks/>
		public string album_crosslink
		{
			get
			{
				return this.album_crosslinkField;
			}
			set
			{
				this.album_crosslinkField = value;
			}
		}

		/// <remarks/>
		public string artist_crosslink
		{
			get
			{
				return this.artist_crosslinkField;
			}
			set
			{
				this.artist_crosslinkField = value;
			}
		}

		/// <remarks/>
		public string genre_crosslink
		{
			get
			{
				return this.genre_crosslinkField;
			}
			set
			{
				this.genre_crosslinkField = value;
			}
		}

		/// <remarks/>
		public string bookmark
		{
			get
			{
				return this.bookmarkField;
			}
			set
			{
				this.bookmarkField = value;
			}
		}

		/// <remarks/>
		public string @class
		{
			get
			{
				return this.classField;
			}
			set
			{
				this.classField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "albumArtURI", IsNullable = true )]
		public DIDLLiteItemAlbumArtURI albumArtURI
		{
			get
			{
				return this.albumArtURIField;
			}
			set
			{
				this.albumArtURIField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "artist", IsNullable = true )]
		public DIDLLiteItemArtist[] artist
		{
			get
			{
				return this.artistField;
			}
			set
			{
				this.artistField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "author", IsNullable = true )]
		public DIDLLiteItemAuthor[] author
		{
			get
			{
				return this.authorField;
			}
			set
			{
				this.authorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "res", IsNullable = true )]
		public DIDLLiteItemRes res
		{
			get
			{
				return this.resField;
			}
			set
			{
				this.resField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string id
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string parentID
		{
			get
			{
				return this.parentIDField;
			}
			set
			{
				this.parentIDField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string restricted
		{
			get
			{
				return this.restrictedField;
			}
			set
			{
				this.restrictedField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class DIDLLiteItemAlbumArtURI
	{

		private string profileIDField;

		private string valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string profileID
		{
			get
			{
				return this.profileIDField;
			}
			set
			{
				this.profileIDField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class DIDLLiteItemArtist
	{

		private string roleField;

		private string valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string role
		{
			get
			{
				return this.roleField;
			}
			set
			{
				this.roleField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class DIDLLiteItemAuthor
	{

		private string roleField;

		private string valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string role
		{
			get
			{
				return this.roleField;
			}
			set
			{
				this.roleField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class DIDLLiteItemRes
	{

		private string durationField;

		private string sizeField;

		private string bitrateField;

		private string protocolInfoField;

		private string valueField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string duration
		{
			get
			{
				return this.durationField;
			}
			set
			{
				this.durationField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string size
		{
			get
			{
				return this.sizeField;
			}
			set
			{
				this.sizeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string bitrate
		{
			get
			{
				return this.bitrateField;
			}
			set
			{
				this.bitrateField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string protocolInfo
		{
			get
			{
				return this.protocolInfoField;
			}
			set
			{
				this.protocolInfoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}

	}
}
