using System;
using System.Collections.Generic;

namespace CoreMP
{
	public class ArtistAlbum
	{
		[Obsolete( "Do not create model instances directly", false )]
		public ArtistAlbum() { }

		public virtual int Id { get; set; }

		public string Name { get; set; }

		public int AlbumId { get; set; }

		public virtual Album Album { get; set; }

		public int ArtistId { get; set; }

		public virtual List<Song> Songs { get; set; }

		public virtual Artist Artist { get; set; }
	}
}
