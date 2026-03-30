// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts.Abstractions;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.Detox.Artifacts;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
    private readonly ICommandService commandService;
    private readonly ILogger<ProjectGenerationStrategy> logger;

    public ProjectGenerationStrategy(
        ICommandService commandService,
        ILogger<ProjectGenerationStrategy> logger)
    {
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Priority => 1;

    public bool CanHandle(ProjectModel model) => model is ProjectModel;

    public async Task GenerateAsync(ProjectModel model)
    {
        logger.LogInformation("Create Detox Test Project. {name}", model.Name);

        System.IO.Directory.CreateDirectory(model.Directory);

        var pagesDir = $"{model.Directory}{Path.DirectorySeparatorChar}pages";
        var specsDir = $"{model.Directory}{Path.DirectorySeparatorChar}specs";

        System.IO.Directory.CreateDirectory(pagesDir);
        System.IO.Directory.CreateDirectory(specsDir);

        logger.LogInformation("Initializing npm project for {name}", model.Name);

        commandService.Start("npm init -y", model.Directory);

        logger.LogInformation("Installing Detox dependencies for {name}", model.Name);

        commandService.Start("npm install --save-dev detox jest @types/jest ts-jest typescript", model.Directory);

        logger.LogInformation("Generating configuration files for {name}", model.Name);

        GenerateDetoxConfig(model);
        GenerateJestConfig(model);
        GenerateTsConfig(model);
        GenerateBasePage(model);
    }

    private void GenerateDetoxConfig(ProjectModel model)
    {
        var content = $@"/** @type {{import('detox').DetoxConfig}} */
module.exports = {{
  testRunner: {{
    args: {{
      $0: 'jest',
      config: 'jest.config.js',
    }},
    jest: {{
      setupTimeout: 120000,
    }},
  }},
  apps: {{
    'ios.debug': {{
      type: 'ios.app',
      binaryPath: 'ios/build/Build/Products/Debug-iphonesimulator/{model.AppName}.app',
      build: 'xcodebuild -workspace ios/{model.AppName}.xcworkspace -scheme {model.AppName} -configuration Debug -sdk iphonesimulator -derivedDataPath ios/build',
    }},
    'android.debug': {{
      type: 'android.apk',
      binaryPath: 'android/app/build/outputs/apk/debug/app-debug.apk',
      build: 'cd android && ./gradlew assembleDebug assembleAndroidTest -DtestBuildType=debug',
      reversePorts: [8081],
    }},
  }},
  devices: {{
    simulator: {{
      type: 'ios.simulator',
      device: {{
        type: 'iPhone 15',
      }},
    }},
    emulator: {{
      type: 'android.emulator',
      device: {{
        avdName: 'Pixel_5_API_34',
      }},
    }},
  }},
  configurations: {{
    'ios.sim.debug': {{
      device: 'simulator',
      app: 'ios.debug',
    }},
    'android.emu.debug': {{
      device: 'emulator',
      app: 'android.debug',
    }},
  }},
}};
";

        File.WriteAllText($"{model.Directory}{Path.DirectorySeparatorChar}.detoxrc.js", content);
    }

    private void GenerateJestConfig(ProjectModel model)
    {
        var content = @"/** @type {import('ts-jest').JestConfigWithTsJest} */
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'node',
  testRunner: 'jest-circus/runner',
  testTimeout: 120000,
  testMatch: ['<rootDir>/specs/**/*.spec.ts'],
  transform: {
    '^.+\\.tsx?$': 'ts-jest',
  },
  reporters: ['detox/runners/jest/reporter'],
  globalSetup: 'detox/runners/jest/globalSetup',
  globalTeardown: 'detox/runners/jest/globalTeardown',
  testEnvironment: 'detox/runners/jest/testEnvironment',
  verbose: true,
};
";

        File.WriteAllText($"{model.Directory}{Path.DirectorySeparatorChar}jest.config.js", content);
    }

    private void GenerateTsConfig(ProjectModel model)
    {
        var content = @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""module"": ""commonjs"",
    ""lib"": [""ES2020""],
    ""strict"": true,
    ""esModuleInterop"": true,
    ""skipLibCheck"": true,
    ""forceConsistentCasingInFileNames"": true,
    ""resolveJsonModule"": true,
    ""declaration"": true,
    ""declarationMap"": true,
    ""sourceMap"": true,
    ""outDir"": ""./dist"",
    ""rootDir"": ""./""
  },
  ""include"": [""pages/**/*.ts"", ""specs/**/*.ts""],
  ""exclude"": [""node_modules"", ""dist""]
}
";

        File.WriteAllText($"{model.Directory}{Path.DirectorySeparatorChar}tsconfig.json", content);
    }

    private void GenerateBasePage(ProjectModel model)
    {
        var content = @"import { expect, element, by, waitFor } from 'detox';

export class BasePage {
  async waitForElement(testId: string, timeout: number = 5000): Promise<void> {
    await waitFor(element(by.id(testId)))
      .toBeVisible()
      .withTimeout(timeout);
  }

  async tapElement(testId: string): Promise<void> {
    await element(by.id(testId)).tap();
  }

  async typeInElement(testId: string, text: string): Promise<void> {
    await element(by.id(testId)).clearText();
    await element(by.id(testId)).typeText(text);
  }

  async expectVisible(testId: string): Promise<void> {
    await expect(element(by.id(testId))).toBeVisible();
  }

  async expectNotVisible(testId: string): Promise<void> {
    await expect(element(by.id(testId))).not.toBeVisible();
  }

  async expectText(testId: string, text: string): Promise<void> {
    await expect(element(by.id(testId))).toHaveText(text);
  }

  async scrollTo(scrollViewId: string, testId: string, direction: string = 'down'): Promise<void> {
    await waitFor(element(by.id(testId)))
      .toBeVisible()
      .whileElement(by.id(scrollViewId))
      .scroll(200, direction as any);
  }

  async swipeElement(testId: string, direction: 'left' | 'right' | 'up' | 'down'): Promise<void> {
    await element(by.id(testId)).swipe(direction);
  }

  async longPressElement(testId: string): Promise<void> {
    await element(by.id(testId)).longPress();
  }
}
";

        var pagesDir = $"{model.Directory}{Path.DirectorySeparatorChar}pages";

        File.WriteAllText($"{pagesDir}{Path.DirectorySeparatorChar}BasePage.ts", content);
    }
}
