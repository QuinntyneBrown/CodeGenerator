// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CodeGenerator.Core.Errors;

namespace CodeGenerator.IntegrationTests.Helpers;

/// <summary>
/// A fault-injecting file system decorator. Intercepts WriteAllText/WriteAllTextAsync
/// on the File property to simulate I/O failures. All other members delegate to the inner file system.
/// </summary>
public class FaultInjectingFileSystem : FileSystemBase
{
    private readonly IFileSystem _inner;
    private readonly FaultInjectionOptions _options;
    private readonly Random _random;
    private readonly FaultInjectingFile _file;

    public FaultInjectingFileSystem(IFileSystem inner, FaultInjectionOptions options)
    {
        _inner = inner;
        _options = options;
        _random = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : new Random();
        _file = new FaultInjectingFile(this, inner);
    }

    public override IFile File => _file;
    public override IDirectory Directory => _inner.Directory;
    public override IFileInfoFactory FileInfo => _inner.FileInfo;
    public override IFileStreamFactory FileStream => _inner.FileStream;
    public override IPath Path => _inner.Path;
    public override IDirectoryInfoFactory DirectoryInfo => _inner.DirectoryInfo;
    public override IDriveInfoFactory DriveInfo => _inner.DriveInfo;
    public override IFileSystemWatcherFactory FileSystemWatcher => _inner.FileSystemWatcher;
    public override IFileVersionInfoFactory FileVersionInfo => _inner.FileVersionInfo;

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

    private class FaultInjectingFile : FileWrapper
    {
        private readonly FaultInjectingFileSystem _fs;

        public FaultInjectingFile(FaultInjectingFileSystem fs, IFileSystem innerFs) : base(innerFs)
        {
            _fs = fs;
        }

        public override void WriteAllText(string path, string? contents)
        {
            _fs.SimulateLatency();
            if (_fs.ShouldFaultFileWrite(path))
                throw _fs.CreateFileException(path);
            base.WriteAllText(path, contents);
        }

        public override Task WriteAllTextAsync(string path, string? contents, CancellationToken cancellationToken = default)
        {
            _fs.SimulateLatency();
            if (_fs.ShouldFaultFileWrite(path))
                throw _fs.CreateFileException(path);
            return base.WriteAllTextAsync(path, contents, cancellationToken);
        }
    }
}
