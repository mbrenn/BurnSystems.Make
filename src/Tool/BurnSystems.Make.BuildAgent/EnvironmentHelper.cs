using System.Runtime.InteropServices;

namespace BurnSystems.Make.BuildAgent;

/// <summary>
/// Supports the determination of the right operating system.
/// Code taken and modified by
/// https://github.com/cake-build/cake/blob/eee7ee5d2a18ec34c3cb3ad63313d8649dbffe2b/src/Cake.Core/Polyfill/EnvironmentHelper.cs
/// </summary>
public static class EnvironmentHelper
{
    public static bool Is64BitOperativeSystem()
    {
        return RuntimeInformation.OSArchitecture == Architecture.X64
               || RuntimeInformation.OSArchitecture == Architecture.Arm64;
    }

    public static OSPlatform GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OSPlatform.OSX;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OSPlatform.Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OSPlatform.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return OSPlatform.FreeBSD;
        }

        throw new InvalidOperationException("Unknown Platform");
    }

    public static bool IsWindows(OSPlatform family)
    {
        return family == OSPlatform.Windows;
    }

    public static bool IsUnix()
    {
        return IsUnix(GetOSPlatform());
    }

    public static bool IsWindows()
    {
        return IsWindows(GetOSPlatform());
    }

    public static bool IsUnix(OSPlatform family)
    {
        return family == OSPlatform.Linux
               || family == OSPlatform.OSX
               || family == OSPlatform.FreeBSD;
    }

    public static bool IsOSX(OSPlatform family)
    {
        return family == OSPlatform.OSX;
    }

    public static bool IsLinux(OSPlatform family)
    {
        return family == OSPlatform.Linux;
    }

    public static bool IsFreeBSD(OSPlatform family)
    {
        return family == OSPlatform.FreeBSD;
    }
}