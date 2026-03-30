// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Syntax;

public class DetoxConfigSyntaxGenerationStrategy : ISyntaxGenerationStrategy<DetoxConfigModel>
{
    private readonly ILogger<DetoxConfigSyntaxGenerationStrategy> logger;

    public DetoxConfigSyntaxGenerationStrategy(ILogger<DetoxConfigSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(DetoxConfigModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating Detox config syntax.");

        var builder = StringBuilderCache.Acquire();

        var iosBuild = string.IsNullOrEmpty(model.IosBuild)
            ? $"xcodebuild -workspace ios/{model.AppName}.xcworkspace -scheme {model.AppName} -configuration Debug -sdk iphonesimulator -derivedDataPath ios/build"
            : model.IosBuild;

        var androidBuild = string.IsNullOrEmpty(model.AndroidBuild)
            ? "cd android && ./gradlew assembleDebug assembleAndroidTest -DtestBuildType=debug"
            : model.AndroidBuild;

        builder.AppendLine("/** @type {import('detox').DetoxConfig} */");
        builder.AppendLine("module.exports = {");

        builder.AppendLine("testRunner: {".Indent(1, 2));
        builder.AppendLine("args: {".Indent(2, 2));
        builder.AppendLine($"$0: '{model.TestRunner}',".Indent(3, 2));
        builder.AppendLine("config: 'jest.config.js',".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("jest: {".Indent(2, 2));
        builder.AppendLine("setupTimeout: 120000,".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));

        builder.AppendLine("apps: {".Indent(1, 2));
        builder.AppendLine("'ios.debug': {".Indent(2, 2));
        builder.AppendLine("type: 'ios.app',".Indent(3, 2));
        builder.AppendLine($"binaryPath: 'ios/build/Build/Products/Debug-iphonesimulator/{model.AppName}.app',".Indent(3, 2));
        builder.AppendLine($"build: '{iosBuild}',".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("'android.debug': {".Indent(2, 2));
        builder.AppendLine("type: 'android.apk',".Indent(3, 2));
        builder.AppendLine("binaryPath: 'android/app/build/outputs/apk/debug/app-debug.apk',".Indent(3, 2));
        builder.AppendLine($"build: '{androidBuild}',".Indent(3, 2));
        builder.AppendLine("reversePorts: [8081],".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));

        builder.AppendLine("devices: {".Indent(1, 2));
        builder.AppendLine("simulator: {".Indent(2, 2));
        builder.AppendLine("type: 'ios.simulator',".Indent(3, 2));
        builder.AppendLine("device: {".Indent(3, 2));
        builder.AppendLine("type: 'iPhone 15',".Indent(4, 2));
        builder.AppendLine("},".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("emulator: {".Indent(2, 2));
        builder.AppendLine("type: 'android.emulator',".Indent(3, 2));
        builder.AppendLine("device: {".Indent(3, 2));
        builder.AppendLine("avdName: 'Pixel_5_API_34',".Indent(4, 2));
        builder.AppendLine("},".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));

        builder.AppendLine("configurations: {".Indent(1, 2));
        builder.AppendLine("'ios.sim.debug': {".Indent(2, 2));
        builder.AppendLine("device: 'simulator',".Indent(3, 2));
        builder.AppendLine("app: 'ios.debug',".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("'android.emu.debug': {".Indent(2, 2));
        builder.AppendLine("device: 'emulator',".Indent(3, 2));
        builder.AppendLine("app: 'android.debug',".Indent(3, 2));
        builder.AppendLine("},".Indent(2, 2));
        builder.AppendLine("},".Indent(1, 2));

        builder.AppendLine("};");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
