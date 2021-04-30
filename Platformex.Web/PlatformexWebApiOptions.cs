using System;

namespace Platformex.Web
{
    public sealed class PlatformexWebApiOptions
    {
        public PlatformexWebApiOptions(String basePath)
        {
            BasePath = basePath;
        }

        public String BasePath { get; set; }
    }
}