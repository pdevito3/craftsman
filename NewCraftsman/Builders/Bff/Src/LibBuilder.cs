namespace NewCraftsman.Builders.Bff.Src;

using System.IO.Abstractions;
using Helpers;
using Services;

public class LibBuilder
{
	private readonly ICraftsmanUtilities _utilities;

	public LibBuilder(ICraftsmanUtilities utilities)
	{
		_utilities = utilities;
	}

    public void CreateAxios(string spaDirectory)
    {
        var classPath = ClassPathHelper.BffSpaSrcLibClassPath(spaDirectory, "axios.tsx");
        var fileText = GetAxiosText();
        _utilities.CreateFile(classPath, fileText);
    }
    
    public static string GetAxiosText()
    {
            return @$"import Axios from 'axios';

export const api = Axios.create({{
	withCredentials: true,
	headers: {{
		'X-CSRF': '1',
	}},
}});

api.defaults.timeout = 30_000; // If you want to increase this, do it for a specific call, not the global app API.
api.interceptors.response.use(
	(response) => response,
	async (error) => {{
		if (error.response) {{
			// The request was made and the server responded with a status code
			// that falls out of the range of 2xx
			console.error(error.response.status, error.response.data, error.response.headers);
		}} else if (error.request) {{
			// The request was made but no response was received
			// `error.request` is an instance of XMLHttpRequest in the browser and an instance of
			// http.ClientRequest in node.js
			console.error(error.request);
		}}

		// if (error && error.response && error.response.status === 401) {{
		//   window.location.assign(logoutUrl);
		// }}
		// console.log(error && error.toJSON && error.toJSON() || undefined);

		return Promise.reject(error);
	}}
);
";
    }
}