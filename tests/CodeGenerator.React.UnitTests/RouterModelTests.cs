// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;

namespace CodeGenerator.React.UnitTests;

public class RouterModelTests
{
    [Fact]
    public void DefaultConstructor_SetsNameToEmptyString()
    {
        var model = new RouterModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyRoutes()
    {
        var model = new RouterModel();

        Assert.NotNull(model.Routes);
        Assert.Empty(model.Routes);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new RouterModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void DefaultConstructor_UseLayoutWrapper_DefaultsFalse()
    {
        var model = new RouterModel();

        Assert.False(model.UseLayoutWrapper);
    }

    [Fact]
    public void DefaultConstructor_LayoutComponent_DefaultsToEmptyString()
    {
        var model = new RouterModel();

        Assert.Equal(string.Empty, model.LayoutComponent);
    }

    [Fact]
    public void DefaultConstructor_NotFoundComponent_DefaultsToEmptyString()
    {
        var model = new RouterModel();

        Assert.Equal(string.Empty, model.NotFoundComponent);
    }

    [Fact]
    public void NamedConstructor_SetsName()
    {
        var model = new RouterModel("AppRouter");

        Assert.Equal("AppRouter", model.Name);
    }

    [Fact]
    public void NamedConstructor_InitializesEmptyRoutes()
    {
        var model = new RouterModel("AppRouter");

        Assert.NotNull(model.Routes);
        Assert.Empty(model.Routes);
    }

    [Fact]
    public void NamedConstructor_InitializesEmptyImports()
    {
        var model = new RouterModel("AppRouter");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NamedConstructor_UseLayoutWrapper_DefaultsFalse()
    {
        var model = new RouterModel("AppRouter");

        Assert.False(model.UseLayoutWrapper);
    }

    [Fact]
    public void NamedConstructor_LayoutComponent_DefaultsToEmptyString()
    {
        var model = new RouterModel("AppRouter");

        Assert.Equal(string.Empty, model.LayoutComponent);
    }

    [Fact]
    public void NamedConstructor_NotFoundComponent_DefaultsToEmptyString()
    {
        var model = new RouterModel("AppRouter");

        Assert.Equal(string.Empty, model.NotFoundComponent);
    }

    [Fact]
    public void UseLayoutWrapper_CanBeEnabled()
    {
        var model = new RouterModel("AppRouter");
        model.UseLayoutWrapper = true;

        Assert.True(model.UseLayoutWrapper);
    }

    [Fact]
    public void LayoutComponent_CanBeSet()
    {
        var model = new RouterModel("AppRouter");
        model.LayoutComponent = "MainLayout";

        Assert.Equal("MainLayout", model.LayoutComponent);
    }

    [Fact]
    public void NotFoundComponent_CanBeSet()
    {
        var model = new RouterModel("AppRouter");
        model.NotFoundComponent = "NotFoundPage";

        Assert.Equal("NotFoundPage", model.NotFoundComponent);
    }

    [Fact]
    public void Routes_CanAddRoute()
    {
        var model = new RouterModel("AppRouter");
        model.Routes.Add(new RouteDefinitionModel
        {
            Path = "/home",
            Component = "HomePage"
        });

        Assert.Single(model.Routes);
        Assert.Equal("/home", model.Routes[0].Path);
        Assert.Equal("HomePage", model.Routes[0].Component);
    }

    [Fact]
    public void Routes_CanAddMultipleRoutes()
    {
        var model = new RouterModel("AppRouter");
        model.Routes.Add(new RouteDefinitionModel { Path = "/home", Component = "HomePage" });
        model.Routes.Add(new RouteDefinitionModel { Path = "/about", Component = "AboutPage" });
        model.Routes.Add(new RouteDefinitionModel { Path = "/contact", Component = "ContactPage" });

        Assert.Equal(3, model.Routes.Count);
    }

    [Fact]
    public void Imports_CanAddImport()
    {
        var model = new RouterModel("AppRouter");
        model.Imports.Add(new ImportModel("BrowserRouter", "react-router-dom"));

        Assert.Single(model.Imports);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new RouterModel();

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }
}

public class RouteDefinitionModelTests
{
    [Fact]
    public void Constructor_Path_DefaultsToEmptyString()
    {
        var route = new RouteDefinitionModel();

        Assert.Equal(string.Empty, route.Path);
    }

    [Fact]
    public void Constructor_Component_DefaultsToEmptyString()
    {
        var route = new RouteDefinitionModel();

        Assert.Equal(string.Empty, route.Component);
    }

    [Fact]
    public void Constructor_IsIndex_DefaultsFalse()
    {
        var route = new RouteDefinitionModel();

        Assert.False(route.IsIndex);
    }

    [Fact]
    public void Constructor_IsProtected_DefaultsFalse()
    {
        var route = new RouteDefinitionModel();

        Assert.False(route.IsProtected);
    }

    [Fact]
    public void Constructor_InitializesEmptyChildren()
    {
        var route = new RouteDefinitionModel();

        Assert.NotNull(route.Children);
        Assert.Empty(route.Children);
    }

    [Fact]
    public void IsIndex_CanBeSet()
    {
        var route = new RouteDefinitionModel();
        route.IsIndex = true;

        Assert.True(route.IsIndex);
    }

    [Fact]
    public void IsProtected_CanBeSet()
    {
        var route = new RouteDefinitionModel();
        route.IsProtected = true;

        Assert.True(route.IsProtected);
    }

    [Fact]
    public void Children_CanAddNestedRoutes()
    {
        var parentRoute = new RouteDefinitionModel
        {
            Path = "/dashboard",
            Component = "DashboardLayout"
        };

        parentRoute.Children.Add(new RouteDefinitionModel
        {
            Path = "overview",
            Component = "OverviewPage",
            IsIndex = true
        });

        parentRoute.Children.Add(new RouteDefinitionModel
        {
            Path = "settings",
            Component = "SettingsPage"
        });

        Assert.Equal(2, parentRoute.Children.Count);
        Assert.True(parentRoute.Children[0].IsIndex);
        Assert.Equal("settings", parentRoute.Children[1].Path);
    }

    [Fact]
    public void Children_CanNestDeeply()
    {
        var root = new RouteDefinitionModel { Path = "/", Component = "Root" };
        var child = new RouteDefinitionModel { Path = "child", Component = "Child" };
        var grandchild = new RouteDefinitionModel { Path = "grandchild", Component = "Grandchild" };

        child.Children.Add(grandchild);
        root.Children.Add(child);

        Assert.Single(root.Children);
        Assert.Single(root.Children[0].Children);
        Assert.Equal("Grandchild", root.Children[0].Children[0].Component);
    }

    [Fact]
    public void Path_CanBeSet()
    {
        var route = new RouteDefinitionModel();
        route.Path = "/users/:id";

        Assert.Equal("/users/:id", route.Path);
    }

    [Fact]
    public void Component_CanBeSet()
    {
        var route = new RouteDefinitionModel();
        route.Component = "UserDetailPage";

        Assert.Equal("UserDetailPage", route.Component);
    }
}
