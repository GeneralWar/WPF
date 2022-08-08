using System;
using System.IO;

namespace General.WPF
{
    public class HelperCache
    {
        public virtual string CachePath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.GetType().Name);

        protected virtual byte[]? readFromCache()
        {
            try
            {
                string filename = this.CachePath;
                Tracer.Log($"Try to load {this.GetType().Name} from {filename}");
                return File.Exists(filename) ? File.ReadAllBytes(filename) : null;
            }
            catch (Exception e)
            {
                Tracer.Exception(e);
                return null;
            }
        }

        protected virtual void writeToCache(byte[] data)
        {
            try
            {
                string filename = this.CachePath;
                Tracer.Log($"Try to save {this.GetType().Name} to {filename}");

                string? directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(filename, data);
            }
            catch (Exception e)
            {
                Tracer.Exception(e);
            }
        }
    }
}
