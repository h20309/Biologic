using Biologic.Native;

namespace Biologic;

internal sealed class ECLabPathResolver
{
  private readonly string configurationRoot;

  public ECLabPathResolver(string? configurationRoot = null)
  {
    string root = string.IsNullOrWhiteSpace(configurationRoot)
      ? Directory.GetCurrentDirectory()
      : configurationRoot;

    this.configurationRoot = Path.GetFullPath(root);
  }

  public string ConfigurationRoot => this.configurationRoot;

  public string ResolveDirectory(string? configuredPath, string fallbackRelativePath = "lib")
  {
    string pathValue = string.IsNullOrWhiteSpace(configuredPath)
      ? fallbackRelativePath
      : configuredPath;

    if (Path.IsPathRooted(pathValue))
    {
      return Path.GetFullPath(pathValue);
    }

    return Path.GetFullPath(Path.Combine(this.configurationRoot, pathValue));
  }

  public string ToNativePath(string path)
  {
    string absolutePath = Path.GetFullPath(path);
    string shortPath = ECLibNative.GetShortPath(absolutePath);

    if (string.IsNullOrWhiteSpace(shortPath))
    {
      return absolutePath;
    }

    return Path.GetFullPath(shortPath);
  }
}