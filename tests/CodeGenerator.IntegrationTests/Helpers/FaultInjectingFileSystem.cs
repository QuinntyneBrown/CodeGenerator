// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Errors;

namespace CodeGenerator.IntegrationTests.Helpers;

public class FaultInjectingFileSystem : IFileSystem
{
    private readonly IFileSystem _inner;
    private readonly FaultInjectionOptions _options;
    private readonly Random _random;

    public FaultInjectingFileSystem(IFileSystem inner, FaultInjectionOptions options)
    {
        _inner = inner;
        _options = options;
        _random = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : new Random();

        File = new FaultInjectingFile(inner.File, this);
        Directory = inner.Directory;
        DirectoryInfo = inner.DirectoryInfo;
        DriveInfo = inner.DriveInfo;
        FileInfo = inner.FileInfo;
        FileStream = inner.FileStream;
        FileSystemWatcher = inner.FileSystemWatcher;
        Path = inner.Path;
    }

    public IFile File { get; }
    public IDirectory Directory { get; }
    public IFileInfoFactory FileInfo { get; }
    public IFileStreamFactory FileStream { get; }
    public IPath Path { get; }
    public IDirectoryInfoFactory DirectoryInfo { get; }
    public IDriveInfoFactory DriveInfo { get; }
    public IFileSystemWatcherFactory FileSystemWatcher { get; }

    internal bool ShouldFaultFileWrite(string path)
    {
        if (_options.FileWriteFailureRate <= 0.0)
            return false;

        if (_options.TargetPaths.Count > 0 && !_options.TargetPaths.Any(p => path.Contains(p, StringComparison.OrdinalIgnoreCase)))
            return false;

        return _random.NextDouble() < _options.FileWriteFailureRate;
    }

    internal void SimulateLatency()
    {
        if (_options.SimulatedLatency.HasValue)
        {
            Thread.Sleep(_options.SimulatedLatency.Value);
        }
    }

    internal Exception CreateFileException(string path)
    {
        if (_options.SimulateDiskFull)
            return new IOException($"Simulated disk full writing to: {path}");

        if (_options.SimulatePermissionDenied)
            return new UnauthorizedAccessException($"Simulated permission denied for: {path}");

        return new CliIOException($"Simulated file write failure for: {path}");
    }

    private class FaultInjectingFile : IFile
    {
        private readonly IFile _inner;
        private readonly FaultInjectingFileSystem _fs;

        public FaultInjectingFile(IFile inner, FaultInjectingFileSystem fs)
        {
            _inner = inner;
            _fs = fs;
        }

        public IFileSystem FileSystem => _fs;

        public void WriteAllText(string path, string? contents)
        {
            _fs.SimulateLatency();
            if (_fs.ShouldFaultFileWrite(path))
                throw _fs.CreateFileException(path);
            _inner.WriteAllText(path, contents);
        }

        public Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
        {
            _fs.SimulateLatency();
            if (_fs.ShouldFaultFileWrite(path))
                throw _fs.CreateFileException(path);
            return _inner.WriteAllTextAsync(path, contents, cancellationToken);
        }

        // Delegate all other IFile members to inner
        public void AppendAllLines(string path, IEnumerable<string> contents) => _inner.AppendAllLines(path, contents);
        public void AppendAllLines(string path, IEnumerable<string> contents, System.Text.Encoding encoding) => _inner.AppendAllLines(path, contents, encoding);
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => _inner.AppendAllLinesAsync(path, contents, cancellationToken);
        public Task AppendAllLinesAsync(string path, IEnumerable<string> contents, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.AppendAllLinesAsync(path, contents, encoding, cancellationToken);
        public void AppendAllText(string path, string? contents) => _inner.AppendAllText(path, contents);
        public void AppendAllText(string path, string? contents, System.Text.Encoding encoding) => _inner.AppendAllText(path, contents, encoding);
        public Task AppendAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default) => _inner.AppendAllTextAsync(path, contents, cancellationToken);
        public Task AppendAllTextAsync(string path, string? contents, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.AppendAllTextAsync(path, contents, encoding, cancellationToken);
        public StreamWriter AppendText(string path) => _inner.AppendText(path);
        public void Copy(string sourceFileName, string destFileName) => _inner.Copy(sourceFileName, destFileName);
        public void Copy(string sourceFileName, string destFileName, bool overwrite) => _inner.Copy(sourceFileName, destFileName, overwrite);
        public FileSystemStream Create(string path) => _inner.Create(path);
        public FileSystemStream Create(string path, int bufferSize) => _inner.Create(path, bufferSize);
        public FileSystemStream Create(string path, int bufferSize, FileOptions options) => _inner.Create(path, bufferSize, options);
        public StreamWriter CreateText(string path) => _inner.CreateText(path);
        public IFileSystemInfo CreateSymbolicLink(string path, string pathToTarget) => _inner.CreateSymbolicLink(path, pathToTarget);
        public void Decrypt(string path) => _inner.Decrypt(path);
        public void Delete(string path) => _inner.Delete(path);
        public void Encrypt(string path) => _inner.Encrypt(path);
        public bool Exists(string? path) => _inner.Exists(path);
        public FileAttributes GetAttributes(string path) => _inner.GetAttributes(path);
        public DateTime GetCreationTime(string path) => _inner.GetCreationTime(path);
        public DateTime GetCreationTimeUtc(string path) => _inner.GetCreationTimeUtc(path);
        public DateTime GetLastAccessTime(string path) => _inner.GetLastAccessTime(path);
        public DateTime GetLastAccessTimeUtc(string path) => _inner.GetLastAccessTimeUtc(path);
        public DateTime GetLastWriteTime(string path) => _inner.GetLastWriteTime(path);
        public DateTime GetLastWriteTimeUtc(string path) => _inner.GetLastWriteTimeUtc(path);
        public void Move(string sourceFileName, string destFileName) => _inner.Move(sourceFileName, destFileName);
        public void Move(string sourceFileName, string destFileName, bool overwrite) => _inner.Move(sourceFileName, destFileName, overwrite);
        public FileSystemStream Open(string path, FileMode mode) => _inner.Open(path, mode);
        public FileSystemStream Open(string path, FileMode mode, FileAccess access) => _inner.Open(path, mode, access);
        public FileSystemStream Open(string path, FileMode mode, FileAccess access, FileShare share) => _inner.Open(path, mode, access, share);
        public FileSystemStream Open(string path, FileStreamOptions options) => _inner.Open(path, options);
        public FileSystemStream OpenRead(string path) => _inner.OpenRead(path);
        public StreamReader OpenText(string path) => _inner.OpenText(path);
        public FileSystemStream OpenWrite(string path) => _inner.OpenWrite(path);
        public byte[] ReadAllBytes(string path) => _inner.ReadAllBytes(path);
        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) => _inner.ReadAllBytesAsync(path, cancellationToken);
        public string[] ReadAllLines(string path) => _inner.ReadAllLines(path);
        public string[] ReadAllLines(string path, System.Text.Encoding encoding) => _inner.ReadAllLines(path, encoding);
        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default) => _inner.ReadAllLinesAsync(path, cancellationToken);
        public Task<string[]> ReadAllLinesAsync(string path, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.ReadAllLinesAsync(path, encoding, cancellationToken);
        public string ReadAllText(string path) => _inner.ReadAllText(path);
        public string ReadAllText(string path, System.Text.Encoding encoding) => _inner.ReadAllText(path, encoding);
        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) => _inner.ReadAllTextAsync(path, cancellationToken);
        public Task<string> ReadAllTextAsync(string path, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.ReadAllTextAsync(path, encoding, cancellationToken);
        public IEnumerable<string> ReadLines(string path) => _inner.ReadLines(path);
        public IEnumerable<string> ReadLines(string path, System.Text.Encoding encoding) => _inner.ReadLines(path, encoding);
        public IAsyncEnumerable<string> ReadLinesAsync(string path, CancellationToken cancellationToken = default) => _inner.ReadLinesAsync(path, cancellationToken);
        public IAsyncEnumerable<string> ReadLinesAsync(string path, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.ReadLinesAsync(path, encoding, cancellationToken);
        public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName) => _inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName);
        public void Replace(string sourceFileName, string destinationFileName, string? destinationBackupFileName, bool ignoreMetadataErrors) => _inner.Replace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
        public IFileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget) => _inner.ResolveLinkTarget(linkPath, returnFinalTarget);
        public void SetAttributes(string path, FileAttributes fileAttributes) => _inner.SetAttributes(path, fileAttributes);
        public void SetCreationTime(string path, DateTime creationTime) => _inner.SetCreationTime(path, creationTime);
        public void SetCreationTimeUtc(string path, DateTime creationTimeUtc) => _inner.SetCreationTimeUtc(path, creationTimeUtc);
        public void SetLastAccessTime(string path, DateTime lastAccessTime) => _inner.SetLastAccessTime(path, lastAccessTime);
        public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc) => _inner.SetLastAccessTimeUtc(path, lastAccessTimeUtc);
        public void SetLastWriteTime(string path, DateTime lastWriteTime) => _inner.SetLastWriteTime(path, lastWriteTime);
        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc) => _inner.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        public void WriteAllBytes(string path, byte[] bytes) => _inner.WriteAllBytes(path, bytes);
        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) => _inner.WriteAllBytesAsync(path, bytes, cancellationToken);
        public void WriteAllLines(string path, IEnumerable<string> contents) => _inner.WriteAllLines(path, contents);
        public void WriteAllLines(string path, IEnumerable<string> contents, System.Text.Encoding encoding) => _inner.WriteAllLines(path, contents, encoding);
        public void WriteAllLines(string path, string[] contents) => _inner.WriteAllLines(path, contents);
        public void WriteAllLines(string path, string[] contents, System.Text.Encoding encoding) => _inner.WriteAllLines(path, contents, encoding);
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default) => _inner.WriteAllLinesAsync(path, contents, cancellationToken);
        public Task WriteAllLinesAsync(string path, IEnumerable<string> contents, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.WriteAllLinesAsync(path, contents, encoding, cancellationToken);
        public void WriteAllText(string path, string? contents, System.Text.Encoding encoding) => _inner.WriteAllText(path, contents, encoding);
        public Task WriteAllTextAsync(string path, string? contents, System.Text.Encoding encoding, CancellationToken cancellationToken = default) => _inner.WriteAllTextAsync(path, contents, encoding, cancellationToken);
#if FEATURE_UNIX_FILE_MODE
        public UnixFileMode GetUnixFileMode(string path) => _inner.GetUnixFileMode(path);
        public void SetUnixFileMode(string path, UnixFileMode mode) => _inner.SetUnixFileMode(path, mode);
#endif
    }
}
