# L2 Requirements — Frontend Code Generation (Angular, React, React Native)

**Parent:** [L1-CodeGenerator.md](L1-CodeGenerator.md) — FR-06, FR-07, FR-08
**Status:** Reverse-engineered from source code
**Date:** 2026-04-03

---

## FR-06: Angular Generation

### FR-06.1: Workspace Generation

The framework shall create Angular workspaces with Jest testing configured.

**Acceptance Criteria:**
- GIVEN a `WorkspaceModel` with name, WHEN the workspace is generated, THEN `ng new` is executed with `--no-create-application`.
- GIVEN a workspace, WHEN generated, THEN Karma is removed and Jest is installed with `jest.config.js` configured.
- GIVEN a workspace, WHEN generated, THEN NGRx component-store is installed.
- GIVEN a workspace with multiple projects, WHEN generated, THEN each project is created within the workspace.

### FR-06.2: Project Generation

The framework shall create Angular application and library projects.

**Acceptance Criteria:**
- GIVEN a `ProjectModel` with type "application", WHEN generated, THEN `ng new` creates an application project.
- GIVEN a `ProjectModel` with type "library", WHEN generated, THEN a library project with `public-api.ts` is created.
- GIVEN any project, WHEN generated, THEN standalone components are enabled by default in `angular.json`.
- GIVEN a library project, WHEN generated, THEN TypeScript compiler options are updated to use Jest types instead of Jasmine.

### FR-06.3: TypeScript Type Generation

The framework shall generate TypeScript type definitions.

**Acceptance Criteria:**
- GIVEN a `TypeScriptTypeModel` with name and properties, WHEN generated, THEN `export type TypeName = { prop1?: type1; prop2: type2; };` is produced.

### FR-06.4: Function Generation

The framework shall generate TypeScript functions with imports.

**Acceptance Criteria:**
- GIVEN a `FunctionModel` with name, body, and imports, WHEN generated, THEN import statements and an `export function` declaration are produced.

### FR-06.5: Barrel Index File Generation

The framework shall generate barrel index files for TypeScript and SCSS.

**Acceptance Criteria:**
- GIVEN a directory with TypeScript modules, WHEN an index file is generated, THEN `export * from './module';` lines are produced for each module.
- GIVEN a directory with SCSS files, WHEN an index file is generated, THEN `@import` lines are produced for each SCSS file.

### FR-06.6: Angular Configuration Utilities

The framework shall provide utilities for modifying Angular JSON configuration.

**Acceptance Criteria:**
- GIVEN an `angular.json` configuration, WHEN `AddBuildConfiguration()` is called, THEN file replacement entries are added.
- GIVEN an `angular.json` configuration, WHEN `EnableDefaultStandalone()` is called, THEN standalone component generation is set as default.
- GIVEN an `angular.json` configuration, WHEN `AddSupportedLocales()` is called, THEN i18n localization settings are configured.

---

## FR-07: React Generation

### FR-07.1: Workspace Generation

The framework shall create React + TypeScript + Vite workspaces.

**Acceptance Criteria:**
- GIVEN a `WorkspaceModel`, WHEN the workspace is generated, THEN `npm create vite@latest` is executed with the `react-ts` template.
- GIVEN a workspace, WHEN generated, THEN `@tanstack/react-query`, `zustand`, `react-router`, `axios`, `tailwindcss`, and `vitest` are installed.

### FR-07.2: Component Generation

The framework shall generate React functional components with TypeScript.

**Acceptance Criteria:**
- GIVEN a `ComponentModel` with name and props, WHEN generated, THEN a functional component with a props interface and `React.forwardRef` wrapping is produced.
- GIVEN a component with `IsClient=true`, WHEN generated, THEN `"use client"` directive appears at the top of the file (for Next.js).
- GIVEN a component with hooks, WHEN generated, THEN hook initialization calls appear in the component body.
- GIVEN a component with children, WHEN generated, THEN child component JSX elements are rendered.

### FR-07.3: Custom Hook Generation

The framework shall generate React custom hooks.

**Acceptance Criteria:**
- GIVEN a `HookModel` with name, parameters, and return type, WHEN generated, THEN `export function useHookName(params): ReturnType { ... }` is produced.
- GIVEN a hook with imports, WHEN generated, THEN import statements appear at the top.

### FR-07.4: API Client Generation

The framework shall generate Axios-based API client functions.

**Acceptance Criteria:**
- GIVEN an `ApiClientModel` with baseUrl and methods, WHEN generated, THEN an axios instance is created with the base URL and exported async functions are produced for each method.
- GIVEN a method with httpMethod GET and route `/users/:id`, WHEN generated, THEN an async function with `id` parameter using `axios.get()` is produced.
- GIVEN a method with httpMethod POST and requestBodyType, WHEN generated, THEN an async function accepting the request body type and using `axios.post()` is produced.
- GIVEN methods with responseType, WHEN generated, THEN return types are `Promise<ResponseType>`.

### FR-07.5: Zustand Store Generation

The framework shall generate Zustand state management stores.

**Acceptance Criteria:**
- GIVEN a `StoreModel` with state properties and actions, WHEN generated, THEN a store interface with state and action types is produced, and a `create<StateType>()` call with default values is produced.
- GIVEN state properties of type string/number/boolean, WHEN generated, THEN defaults are `""`, `0`, `false` respectively.
- GIVEN action implementations, WHEN generated, THEN each action body uses `set()` to update state.

### FR-07.6: TypeScript Interface Generation

The framework shall generate TypeScript interfaces with inheritance and sub-interfaces.

**Acceptance Criteria:**
- GIVEN a `TypeScriptInterfaceModel` with properties, WHEN generated, THEN `export interface Name { prop?: type; }` is produced.
- GIVEN an interface with `extends` list, WHEN generated, THEN `export interface Name extends Base1, Base2 { ... }` is produced.
- GIVEN an interface with sub-interfaces, WHEN generated, THEN nested interface definitions are also produced.
- GIVEN optional properties, WHEN generated, THEN `?` marker is applied.

### FR-07.7: Barrel Index File Generation

The framework shall generate barrel index files for React projects.

**Acceptance Criteria:**
- GIVEN a directory with `.ts` and `.tsx` files, WHEN an index file is generated, THEN re-exports are produced excluding `.spec`, `.test`, and existing index files.

---

## FR-08: React Native Generation

### FR-08.1: Project Generation

The framework shall scaffold React Native projects with TypeScript.

**Acceptance Criteria:**
- GIVEN a `ProjectModel`, WHEN the project is generated, THEN `npx react-native@latest init` is executed with the TypeScript template.
- GIVEN a project, WHEN generated, THEN `@react-navigation/native`, `@react-navigation/stack`, `zustand`, `axios`, `react-native-safe-area-context`, `react-native-screens`, and `react-native-gesture-handler` are installed.

### FR-08.2: Screen Generation

The framework shall generate React Native screen components.

**Acceptance Criteria:**
- GIVEN a `ScreenModel` with name, WHEN generated, THEN a functional component using `SafeAreaView > View > Text` hierarchy is produced.
- GIVEN a screen with props, WHEN generated, THEN a props interface with navigation parameter types (e.g., `ScreenNameParams`) is produced.
- GIVEN a screen with hooks (e.g., `useNavigation`), WHEN generated, THEN hook initialization appears in the component body.
- GIVEN a screen, WHEN generated, THEN `StyleSheet.create()` styles are generated and `testID` attributes are applied to each element using kebab-case naming.

### FR-08.3: Component Generation

The framework shall generate React Native reusable components.

**Acceptance Criteria:**
- GIVEN a `ComponentModel` with name and props, WHEN generated, THEN a `React.FC<Props>` component with `View > Text` structure is produced.
- GIVEN a component with styles, WHEN generated, THEN `StyleSheet.create()` definitions are included.
- GIVEN a component, WHEN generated, THEN `testID` attributes are applied.

### FR-08.4: Navigation Generation

The framework shall generate React Navigation configurations.

**Acceptance Criteria:**
- GIVEN a `NavigationModel` with type "stack", WHEN generated, THEN `createStackNavigator<ParamList>()` with `NavigationContainer` and `Stack.Screen` components is produced.
- GIVEN a `NavigationModel` with type "tab", WHEN generated, THEN `createBottomTabNavigator<ParamList>()` with `Tab.Screen` components is produced.
- GIVEN a `NavigationModel` with type "drawer", WHEN generated, THEN `createDrawerNavigator<ParamList>()` with `Drawer.Screen` components is produced.
- GIVEN a navigation with screens, WHEN generated, THEN a `ParamList` type is exported with screen parameter types.

### FR-08.5: React Native Store Generation

The framework shall generate simplified Zustand stores for React Native.

**Acceptance Criteria:**
- GIVEN a `StoreModel` with state properties and actions, WHEN generated, THEN a store interface and `create<StateType>()` call are produced.
- GIVEN actions, WHEN generated, THEN simple setter actions using `set()` are produced (no complex action implementations).

### FR-08.6: StyleSheet Generation

The framework shall generate React Native StyleSheet objects.

**Acceptance Criteria:**
- GIVEN a `StyleModel` with named style objects and properties, WHEN generated, THEN `StyleSheet.create({ styleName: { property: value } })` is produced.

### FR-08.7: React Native Barrel Files

The framework shall generate barrel index files for React Native projects.

**Acceptance Criteria:**
- GIVEN a directory with `.ts` and `.tsx` files, WHEN an index file is generated, THEN `export * from './module';` lines are produced for each module and subdirectory.
