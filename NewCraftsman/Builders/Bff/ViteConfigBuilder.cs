namespace NewCraftsman.Builders.Bff;

using System.IO.Abstractions;
using Helpers;
using Services;

public class ViteConfigBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public ViteConfigBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateViteConfig(string spaDirectory, int? proxyPort)
    {
        var classPath = ClassPathHelper.BffSpaRootClassPath(spaDirectory, $"vite.config.ts");
        var fileText = GetViteConfigText(proxyPort);
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetViteConfigText(int? proxyPort)
    {
            return @$"import reactRefresh from '@vitejs/plugin-react-refresh';
import {{ readFileSync }} from 'fs';
import {{ join }} from 'path';
import {{ defineConfig }} from 'vite';

const baseFolder =
	process.env.APPDATA !== undefined && process.env.APPDATA !== ''
		? `${{process.env.APPDATA}}/ASP.NET/https`
		: `${{process.env.HOME}}/.aspnet/https`;

const certificateName = process.env.npm_package_name;

const certFilePath = join(baseFolder, `${{certificateName}}.pem`);
const keyFilePath = join(baseFolder, `${{certificateName}}.key`);

const path = require('path');
const {{ env }} = require('process');

const target = env.ASPNETCORE_HTTPS_PORT
	? `https://localhost:${{env.ASPNETCORE_HTTPS_PORT}}`
	: env.ASPNETCORE_URLS
		? env.ASPNETCORE_URLS.split(';')[0]
		: 'http://localhost:18082';

const baseProxy = {{
    target,
    secure: false,
}};

// https://vitejs.dev/config/
export default defineConfig({{
    plugins: [reactRefresh()],
	resolve: {{
        alias: {{
            '@': path.resolve(__dirname, '/src'),
        }},
    }},
    server: {{
        https: {{
            key: readFileSync(keyFilePath),
            cert: readFileSync(certFilePath),
        }},
        port: {proxyPort},
        strictPort: true,

        // these are the proxy routes that will be forwarded to your **BFF**
        proxy: {{
            '/bff': baseProxy,
            '/signin-oidc': baseProxy,
            '/signout-callback-oidc': baseProxy,
            '/api': baseProxy,
        }},
    }},
}});
";
    }
}
