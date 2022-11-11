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
	public partial class BrowseFolderResponse
	{
		public static BrowseFolderResponse DeserialiseResponse( string response )
		{
			BrowseFolderResponse result = null;

			// Remove all namespace prefixes
			string anonResponse = response.Replace( "dc:", "" ).Replace( "pv:", "" ).Replace( "upnp:", "" ).Replace( "dlna:", "" );

			using ( TextReader reader = new StringReader( anonResponse ) )
			{
				result = ( BrowseFolderResponse )new XmlSerializer( typeof( BrowseFolderResponse ) ).Deserialize( reader );
			}

			return result;
		}

		private BrowseFolderItem[] itemsField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute( "container" )]
		public BrowseFolderItem[] Items
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

	[System.CodeDom.Compiler.GeneratedCodeAttribute( "xsd", "4.8.3928.0" )]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute( "code" )]
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true, Namespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" )]
	public partial class BrowseFolderItem
	{

		private string titleField;

		private string childCountContainerField;

		private string modificationTimeField;

		private string lastUpdatedField;

		private string containerContentField;

		private string classField;

		private string idField;

		private string parentIDField;

		private string restrictedField;

		private string childCountField;

		private string searchableField;

		private string persistentIDField;

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
		public string childCountContainer
		{
			get
			{
				return this.childCountContainerField;
			}
			set
			{
				this.childCountContainerField = value;
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
		public string containerContent
		{
			get
			{
				return this.containerContentField;
			}
			set
			{
				this.containerContentField = value;
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

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string childCount
		{
			get
			{
				return this.childCountField;
			}
			set
			{
				this.childCountField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string searchable
		{
			get
			{
				return this.searchableField;
			}
			set
			{
				this.searchableField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string persistentID
		{
			get
			{
				return this.persistentIDField;
			}
			set
			{
				this.persistentIDField = value;
			}
		}
	}
}
