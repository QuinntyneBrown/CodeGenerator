# L2 Requirements — Testing & CLI (Playwright, Detox, CLI)

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-09, FR-10, FR-15
**Status:** Reverse-engineered from source code
**Date:** 2026-04-03

---

## FR-09: Playwright Test Generation

### FR-09.1: Page Object Generation

The framework shall generate Playwright page object classes with locators, actions, and queries.

**Acceptance Criteria:**
- GIVEN a `PageObjectModel` with name and locators, WHEN generated, THEN a class extending `BasePage` with private locator fields is produced.
- GIVEN locators with strategy `GetByTestId`, WHEN generated, THEN `this.page.getByTestId('selector')` is used.
- GIVEN locators with strategy `GetByRole`, WHEN generated, THEN `this.page.getByRole('role')` is used.
- GIVEN locators with strategy `GetByLabel`, WHEN generated, THEN `this.page.getByLabel('label')` is used.
- GIVEN locators with strategy `Locator`, WHEN generated, THEN `this.page.locator('css-selector')` is used.
- GIVEN a page object with actions, WHEN generated, THEN async action methods with parameters are produced.
- GIVEN a page object with queries, WHEN generated, THEN query methods with configurable return types are produced.

### FR-09.2: Base Page Generation

The framework shall generate an abstract base page class for Playwright tests.

**Acceptance Criteria:**
- GIVEN a base page generation request, WHEN generated, THEN an abstract class with protected `page` and `path` properties is produced.
- GIVEN the base page, WHEN generated, THEN methods `navigate()`, `waitForPageLoad()`, `getPageTitle()`, and `getToastMessage()` are included.
- GIVEN the base page, WHEN generated, THEN locator helper methods `getByTestId()`, `getByRole()`, and `getByLabel()` are included.

### FR-09.3: Test Spec Generation

The framework shall generate Playwright test spec files with describe/test blocks.

**Acceptance Criteria:**
- GIVEN a `TestSpecModel` with test cases, WHEN generated, THEN a `test.describe()` block with `test()` entries is produced.
- GIVEN a test spec, WHEN generated, THEN a `beforeEach` hook with page navigation setup is included.
- GIVEN test cases with setup actions, WHEN generated, THEN each test uses Arrange-Act-Assert pattern with the page object.
- GIVEN a test spec with page object type, WHEN generated, THEN the page object is instantiated in the spec.

### FR-09.4: Custom Fixture Generation

The framework shall generate Playwright custom fixtures.

**Acceptance Criteria:**
- GIVEN a `FixtureModel` with type definitions and setup code, WHEN generated, THEN a fixture file using `base.extend<T>()` is produced.
- GIVEN a fixture, WHEN generated, THEN `expect` is re-exported from `@playwright/test`.

### FR-09.5: Playwright Configuration Generation

The framework shall generate `playwright.config.ts` with multi-browser support.

**Acceptance Criteria:**
- GIVEN a `ConfigModel` with browsers list, WHEN generated, THEN a `playwright.config.ts` with projects for each browser (chromium, firefox, webkit) is produced.
- GIVEN config with baseUrl, timeout, retries, and reporter, WHEN generated, THEN each setting is configured in the exported config object.
- GIVEN CI-specific settings, WHEN generated, THEN parallel execution and CI reporter options are included.

### FR-09.6: Playwright Project Scaffolding

The framework shall scaffold complete Playwright test projects.

**Acceptance Criteria:**
- GIVEN a `ProjectModel`, WHEN generated, THEN `pages/`, `specs/`, `fixtures/`, and `helpers/` directories are created.
- GIVEN a project, WHEN generated, THEN `npm init -y` and `npm install --save-dev @playwright/test` are executed.
- GIVEN a project, WHEN generated, THEN `playwright.config.ts`, `BasePage.ts`, and `tsconfig.json` are created.

---

## FR-10: Detox Test Generation

### FR-10.1: Mobile Page Object Generation

The framework shall generate Detox page object classes with testID-based selectors.

**Acceptance Criteria:**
- GIVEN a `PageObjectModel` with testIds, WHEN generated, THEN a class with private testID string fields is produced.
- GIVEN a page object with visibility checks, WHEN generated, THEN methods like `isScreenVisible()` using `element(by.id())` and `expect().toBeVisible()` are produced.
- GIVEN a page object with interactions (tap, type, scroll, swipe), WHEN generated, THEN methods using `element(by.id()).tap()`, `.typeText()`, `.scroll()`, `.swipe()` are produced.
- GIVEN a page object with combined actions, WHEN generated, THEN multi-step methods executing sequential interactions are produced.
- GIVEN a page object with query helpers, WHEN generated, THEN methods returning element attributes are produced.

### FR-10.2: Detox Base Page Generation

The framework shall generate a base page class with common mobile interactions.

**Acceptance Criteria:**
- GIVEN a base page generation request, WHEN generated, THEN a class with `waitForElement()`, `tapElement()`, `typeInElement()` helper methods is produced.
- GIVEN the base page, WHEN generated, THEN assertion methods `expectVisible()`, `expectNotVisible()`, `expectText()` are included.
- GIVEN the base page, WHEN generated, THEN interaction methods `scrollTo()`, `swipeElement()`, `longPressElement()` are included.

### FR-10.3: Detox Test Spec Generation

The framework shall generate Detox test spec files with Jest describe/it blocks.

**Acceptance Criteria:**
- GIVEN a `TestSpecModel` with test cases, WHEN generated, THEN a `describe()` block with `it()` entries is produced.
- GIVEN a test spec, WHEN generated, THEN `beforeAll()` and `beforeEach()` hooks are included.
- GIVEN `beforeEach`, WHEN generated, THEN `device.reloadReactNative()` is called.
- GIVEN test cases with steps, WHEN generated, THEN each step is executed in sequence within the `it()` block.

### FR-10.4: Detox Configuration Generation

The framework shall generate `.detoxrc.js` configuration for iOS and Android.

**Acceptance Criteria:**
- GIVEN a `DetoxConfigModel` with appName, WHEN generated, THEN a `.detoxrc.js` with test runner (jest), apps (iOS debug, Android debug), devices (iOS simulator, Android emulator), and configurations is produced.
- GIVEN iOS and Android build commands, WHEN generated, THEN platform-specific build commands are configured.

### FR-10.5: Jest Configuration Generation

The framework shall generate Jest configuration for Detox tests.

**Acceptance Criteria:**
- GIVEN a `JestConfigModel`, WHEN generated, THEN a `jest.config.js` with `preset: 'ts-jest'`, Detox test environment, test match pattern `**/*.spec.ts`, Detox reporter, and global setup/teardown is produced.

### FR-10.6: Detox Project Scaffolding

The framework shall scaffold complete Detox test projects.

**Acceptance Criteria:**
- GIVEN a `ProjectModel`, WHEN generated, THEN `pages/` and `specs/` directories are created.
- GIVEN a project, WHEN generated, THEN `npm init -y` and `npm install detox jest @types/jest ts-jest typescript` are executed.
- GIVEN a project, WHEN generated, THEN `.detoxrc.js`, `jest.config.js`, `tsconfig.json`, and `BasePage.ts` are created.

---

## FR-15: CLI Tool

### FR-15.1: Project Scaffolding Command

The CLI tool shall scaffold new code generator projects via the root command.

**Acceptance Criteria:**
- GIVEN `create-code-cli -n MyGenerator -o ./output`, WHEN executed, THEN a solution directory is created with `.sln` file, `src/MyGenerator.Cli/` project, and `eng/scripts/install-cli.bat`.
- GIVEN the `-n` (name) option, WHEN provided, THEN it is used as the solution and project name prefix.
- GIVEN the `-o` (output) option, WHEN provided, THEN the solution is created in the specified directory. Default is the current directory.
- GIVEN the `-f` (framework) option, WHEN provided, THEN the target framework is used (default: `net9.0`).
- GIVEN the `--slnx` flag, WHEN provided, THEN `.slnx` format is used instead of `.sln`.

### FR-15.2: Generated Project Structure

The scaffolded project shall include a functioning CLI with DI configuration.

**Acceptance Criteria:**
- GIVEN a generated project, THEN `Program.cs` contains DI service registration with `AddCoreServices()` and `AddDotNetServices()`.
- GIVEN a generated project, THEN `Commands/AppRootCommand.cs` defines the root command with name and output options.
- GIVEN a generated project, THEN `Commands/HelloWorldCommand.cs` provides a sample command implementation.
- GIVEN a generated project, THEN the `.csproj` includes NuGet tool packaging configuration (`PackAsTool=true`).

### FR-15.3: Install Script Generation

The CLI shall generate a batch script for global tool installation.

**Acceptance Criteria:**
- GIVEN a generated project, THEN `eng/scripts/install-cli.bat` contains commands to `dotnet pack` and `dotnet tool install --global` the CLI.

### FR-15.4: Claude Skill Installation Command

The CLI shall install a Claude skill documentation file.

**Acceptance Criteria:**
- GIVEN `create-code-cli install`, WHEN executed, THEN `.claude/skills/code-generator/SKILL.md` is created in the current directory.
- GIVEN the `-o` option, WHEN provided, THEN the skill file is created in the specified directory.
- GIVEN the skill file, THEN it contains documentation on all CodeGenerator models, factories, and generation patterns covering DotNet, Angular, Python, React, ReactNative, Flask, Playwright, and Detox packages.
