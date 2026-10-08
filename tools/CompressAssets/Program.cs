using System.IO.Compression;

string libPath = args.Length > 0
    ? args[0]
    : Path.Combine("..", "..", "ECommerce.Web", "wwwroot", "lib");

libPath = Path.GetFullPath(libPath);

if (!Directory.Exists(libPath))
{
    Console.WriteLine($"Path not found: {libPath}");
    return;
}

var extensions = new[] { ".css", ".js" };

var files = Directory.EnumerateFiles(libPath, "*.*", SearchOption.AllDirectories)
    .Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
    .Where(f => !f.EndsWith(".gz") && !f.EndsWith(".br"))
    .ToList();

Console.WriteLine($"Found {files.Count} files under {libPath}");

foreach (var source in files)
{
    var bytes = File.ReadAllBytes(source);
    var sourceWriteTime = File.GetLastWriteTimeUtc(source);

    var gzPath = source + ".gz";
    if (!File.Exists(gzPath) || File.GetLastWriteTimeUtc(gzPath) < sourceWriteTime)
    {
        using var fs = File.Create(gzPath);
        using var gz = new GZipStream(fs, CompressionLevel.SmallestSize);
        gz.Write(bytes, 0, bytes.Length);
        Console.WriteLine($"gzip:   {source}");
    }

    var brPath = source + ".br";
    if (!File.Exists(brPath) || File.GetLastWriteTimeUtc(brPath) < sourceWriteTime)
    {
        using var fs = File.Create(brPath);
        using var br = new BrotliStream(fs, CompressionLevel.SmallestSize);
        br.Write(bytes, 0, bytes.Length);
        Console.WriteLine($"brotli: {source}");
    }
}

Console.WriteLine("Done.");