﻿using System;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AlbumsViewModel holds the Album data obtained from the AlbumsController
	/// </summary>
	static class AlbumsViewModel
	{
		/// <summary>
		/// Clear the data held by this model
		/// </summary>
		public static void ClearModel()
		{
			Albums = null;
			UnfilteredAlbums = null;
			FilteredAlbums = null;
			GenreSortedAlbums = null;
			LibraryId = -1;
			FilterSelector.CurrentFilter = null;
			FastScrollSections = null;
			FastScrollSectionLookup = null;
			BaseModel.Clear();
		}

		/// <summary>
		/// The list of albums that is available to be displayed. These will have been filtered and sorted according to the current sort order and 
		/// filter settings. This will be set to either the FilteredAlbums, or the GenreSortedAlbums.
		/// </summary>
		public static List<Album> Albums { get; set; } = null;

		/// <summary>
		/// These are the filtered albums that have not had a Genre sort order applied, see below
		/// </summary>
		public static List<Album> FilteredAlbums { get; set; } = null;

		/// <summary>
		/// These are the filtered albums that have had a Genre sort order applied to them. Any album that has multiple genres will have multiple 
		/// entries in this list
		/// </summary>
		public static List<Album> GenreSortedAlbums { get; set; } = null;

		/// <summary>
		/// List of genres for each album ordered their index in GenreSortedAlbums
		/// </summary>
		public static List< string > AlbumIndexToGenreLookup { get; set; } = null;

		/// <summary>
		/// The list of albums for the current library that has been obtained from the database before any sorting or filtering
		/// </summary>
		public static List<Album> UnfilteredAlbums { get; set; } = null;

		//
		// The following two collections are used for fast scrolling
		// The Adapter requires 3 things
		// 1) A list of all the section names in display order
		// 2) The starting index for each section
		// 3) The section index for each album
		//
		// The section names can be obtained from FastScrollSections
		// The starting index for a section is FastScrollSections[ sectionIndex ]
		// The section index for an album is FastScrollSectionLookup[ albumIndex ]
		//
		// The FastScrollSectionLookup is also used when displaying albums in genre order to obtain the genre (section name) for each album 
		//
		/// <summary>
		/// Lookup table specifying the strings used when fast scrolling, and the index into the Albums collection
		/// </summary>
		public static List<Tuple<string, int>> FastScrollSections = null;

		/// <summary>
		/// Array of the section indexes associated with each album
		/// </summary>
		public static int[] FastScrollSectionLookup = null;

		/// <summary>
		/// The id of the library for which a list of artists have been obtained
		/// </summary>
		public static int LibraryId { get; set; } = -1;

		/// <summary>
		/// The FilterSelection used to select and apply filter to the Albums tab
		/// </summary>
		public static FilterSelection FilterSelector { get; } = new FilterSelection( AlbumsController.SetNewFilter );

		/// <summary>
		/// The common model features are contained in the BaseViewModel
		/// </summary>
		public static BaseViewModel BaseModel { get; } = new BaseViewModel();
 	}
}