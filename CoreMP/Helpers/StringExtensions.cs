﻿namespace CoreMP

{
	public static class StringExtensions
	{
		/// <summary>
		/// Extension method to trim the start of a string up to and including a target string
		/// </summary>
		/// <param name="stringToTrim"></param>
		/// <param name="stringToFind"></param>
		/// <returns></returns>
		public static string TrimStart( this string stringToTrim, string stringToFind )
		{
			string trimmedString = stringToTrim;

			int index = stringToTrim.ToUpper().IndexOf( stringToFind.ToUpper() );
			if ( index != -1 )
			{
				trimmedString = stringToTrim.Substring( index + stringToFind.Length );
			}

			return trimmedString;
		}


		/// <summary>
		/// Extension method to trim a string after and including a target string
		/// </summary>
		/// <param name="stringToTrim"></param>
		/// <param name="stringToFind"></param>
		/// <returns></returns>
		public static string TrimAfter( this string stringToTrim, string stringToFind )
		{
			string trimmedString = stringToTrim;

			int index = stringToTrim.ToUpper().IndexOf( stringToFind.ToUpper() );
			if ( index != -1 )
			{
				trimmedString = stringToTrim.Substring( 0, index );
			}

			return trimmedString;
		}

		/// <summary>
		/// Remove a leading 'The ' from the string
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public static string RemoveThe( this string subject ) => 
			( subject.ToUpper().StartsWith( "THE " ) == true ) ? subject.Substring( 4 ) : subject;

		/// <summary>
		/// Remove a leading 'Alt. ' from the string
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public static string RemoveAlt( this string subject ) =>
			( subject.ToUpper().StartsWith( "ALT. " ) == true ) ? subject.Substring( 5 ) : subject;
	}
}
