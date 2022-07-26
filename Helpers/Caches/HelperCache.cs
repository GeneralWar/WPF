using System;
using System.IO;

namespace General.WPF
{
    public class HelperCache
    {
        public virtual string CachePath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.GetType().Name);

        protected virtual byte[]? readFromCache() => File.Exists(this.CachePath) ? File.ReadAllBytes(this.CachePath) : null;

        protected virtual void writeToCache(byte[] data)
        {
            string filename = this.CachePath;
            string? directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filename, data);
        }
    }
}
