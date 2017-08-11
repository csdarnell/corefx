// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace System
{
    public static partial class PlatformDetection
    {
        public static bool IsWindowsIoTCore => false;
        public static bool IsWindows => false;
        public static bool IsWindows7 => false;
        public static bool IsWindows8x => false;
        public static bool IsWindows10Version1607OrGreater => false;
        public static bool IsWindows10Version1703OrGreater => false;
        public static bool IsWindows10InsiderPreviewBuild16215OrGreater => false;
        public static bool IsWindows10Version16251OrGreater => false;
        public static bool IsNotOneCoreUAP =>  true;
        public static bool IsNetfx462OrNewer() { return false; }
        public static bool IsNetfx470OrNewer() { return false; }
        public static bool IsNetfx471OrNewer() { return false; }
        public static bool IsWinRT => false;
        public static int WindowsVersion => -1;

        public static bool IsOpenSUSE => IsDistroAndVersion("opensuse");
        public static bool IsUbuntu => IsDistroAndVersion("ubuntu");
        public static bool IsDebian => IsDistroAndVersion("debian");
        public static bool IsDebian8 => IsDistroAndVersion("debian", "8");
        public static bool IsUbuntu1404 => IsDistroAndVersion("ubuntu", "14.04");
        public static bool IsCentos7 => IsDistroAndVersion("centos", "7");
        public static bool IsTizen => IsDistroAndVersion("tizen");
        public static bool IsNotFedoraOrRedHatOrCentos => !IsDistroAndVersion("fedora") && !IsDistroAndVersion("rhel") && !IsDistroAndVersion("centos");
        public static bool IsFedora => IsDistroAndVersion("fedora");
        public static bool IsWindowsNanoServer => false;
        public static bool IsWindowsAndElevated => false;

        public static Version OSXKernelVersion { get; } = GetOSXKernelVersion();

        public static string GetDistroVersionString()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "";
            }

            DistroInfo v = ParseOsReleaseFile();

            return "Distro=" + v.Id + " VersionId=" + v.VersionId + " Pretty=" + v.PrettyName + " Version=" + v.Version;
        }

        private static DistroInfo ParseOsReleaseFile()
        {
            Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));

            DistroInfo ret = new DistroInfo();
            ret.Id = "";
            ret.VersionId = "";
            ret.Version = "";
            ret.PrettyName = "";

            if (File.Exists("/etc/os-release"))
            {
                foreach (string line in File.ReadLines("/etc/os-release"))
                {
                    if (line.StartsWith("ID=", System.StringComparison.Ordinal))
                    {
                        ret.Id = RemoveQuotes(line.Substring("ID=".Length));
                    }
                    else if (line.StartsWith("VERSION_ID=", System.StringComparison.Ordinal))
                    {
                        ret.VersionId = RemoveQuotes(line.Substring("VERSION_ID=".Length));
                    }
                    else if (line.StartsWith("VERSION=", System.StringComparison.Ordinal))
                    {
                        ret.Version = RemoveQuotes(line.Substring("VERSION=".Length));
                    }
                    else if (line.StartsWith("PRETTY_NAME=", System.StringComparison.Ordinal))
                    {
                        ret.PrettyName = RemoveQuotes(line.Substring("PRETTY_NAME=".Length));
                    }
                }
            }

            return ret;
        }

        private static string RemoveQuotes(string s)
        {
            s = s.Trim();
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
            {
                // Remove quotes.
                s = s.Substring(1, s.Length - 2);
            }

            return s;
        }

        private struct DistroInfo
        {
            public string Id { get; set; }
            public string VersionId { get; set; }
            public string Version { get; set; }
            public string PrettyName { get; set; }
        }

        /// <summary>
        /// Get whether the OS platform matches the given Linux distro and optional version.
        /// </summary>
        /// <param name="distroId">The distribution id.</param>
        /// <param name="versionId">The distro version.  If omitted, compares the distro only.</param>
        /// <returns>Whether the OS platform matches the given Linux distro and optional version.</returns>
        private static bool IsDistroAndVersion(string distroId, string versionId = null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                DistroInfo v = ParseOsReleaseFile();
                if (v.Id == distroId && (versionId == null || v.VersionId == versionId))
                {
                    return true;
                }
            }

            return false;
        }

        private static Version GetOSXKernelVersion()
        {
            if (IsOSX)
            {
                byte[] bytes = new byte[256];
                IntPtr bytesLength = new IntPtr(bytes.Length);
                Assert.Equal(0, sysctlbyname("kern.osrelease", bytes, ref bytesLength, null, IntPtr.Zero));
                string versionString = Encoding.UTF8.GetString(bytes);
                return Version.Parse(versionString);
            }

            return new Version(0, 0, 0);
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int sysctlbyname(string ctlName, byte[] oldp, ref IntPtr oldpLen, byte[] newp, IntPtr newpLen);

        [DllImport("libc", SetLastError = true)]
        internal static extern unsafe uint geteuid();

        public static bool IsSuperUser => geteuid() == 0;
    }
}