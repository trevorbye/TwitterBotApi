using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace TwitterBot.Helpers
{
    // https://madskristensen.net/blog/cache-busting-in-aspnet
    public static class AppendVersionTagHelper
    {
        public static string AppendVersion(this string rootRelativePath)
        {
            try
            {
                if (HttpRuntime.Cache[rootRelativePath] is null)
                {
                    var path = rootRelativePath?.StartsWith("/") ?? false
                        ? rootRelativePath
                        : $"/{rootRelativePath}";

                    var absolute = HostingEnvironment.MapPath(path);
                    var result = $"{rootRelativePath}?v-{ComputeFileHash(absolute)}";
                    HttpRuntime.Cache.Insert(rootRelativePath, result, new CacheDependency(absolute));
                }

                return HttpRuntime.Cache[rootRelativePath] as string;
            }
            catch
            {
                return rootRelativePath;
            }            
        }

        static string ComputeFileHash(string path)
        {
            using (var algorithm = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                return BytesToString(algorithm.ComputeHash(stream));
            }
        }

        static string BytesToString(byte[] array)
        {
            var result = new StringBuilder();
            foreach (var b in array)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }
    }
}