//  
//  Copyright (C) 2009 Amr Hassan
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Http;

namespace Lastfm.Scrobbling
{
	internal class Request
	{
		RequestParameters Parameters;
		Uri URI {get; set;}
		
		internal Request(Uri uri, RequestParameters parameters)
		{
			URI = uri;
			Parameters = parameters;
		}
		
		internal string execute()
		{
			var client = new HttpClient ();
			client.DefaultRequestHeaders.AcceptCharset.ParseAdd ("utf-8");

			HttpResponseMessage webresponse;
			string output = "FAILED";
			try
			{
				webresponse = client.PostAsync(URI,new StringContent(Parameters.ToString(),Encoding.UTF8,"application/x-www-form-urlencoded")).Result;
				output = webresponse.Content.ReadAsStringAsync().Result;
			}catch (Exception e){
				Console.WriteLine (e);
			}

			checkForErrors(output);
			
			return output;
		}
		
		private void checkForErrors(string output)
		{
			string line = output.Split('\n')[0];
			if(line.StartsWith("BANNED"))
				throw new BannedClientException();
			else if(line.StartsWith("BADAUTH"))
				throw new AuthenticationFailureException();
			else if(line.StartsWith("BADTIME"))
				throw new WrongTimeException();
			else if(line.StartsWith("FAILED") || output.Contains("lfm status=\"failed\""))
				throw new ScrobblingException(output.Substring(output.IndexOf(' ') + 1));
		}
	}
}
