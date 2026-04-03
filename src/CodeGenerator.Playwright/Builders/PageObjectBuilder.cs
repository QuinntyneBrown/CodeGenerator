// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Playwright.Syntax;

namespace CodeGenerator.Playwright.Builders;

public class PageObjectBuilder : BuilderBase<PageObjectModel, PageObjectBuilder>
{
    private PageObjectBuilder(PageObjectModel model)
        : base(model)
    {
    }

    public static PageObjectBuilder For(string name)
    {
        return new PageObjectBuilder(new PageObjectModel(name, string.Empty));
    }

    public PageObjectBuilder WithUrl(string url)
    {
        _model.Path = url;
        return Self;
    }

    public PageObjectBuilder WithLocator(string name, LocatorStrategy strategy, string selector)
    {
        _model.Locators.Add(new LocatorModel(name, strategy, selector));
        return Self;
    }

    public PageObjectBuilder WithAction(string name, string body)
    {
        _model.Actions.Add(new PageActionModel(name, string.Empty, body));
        return Self;
    }

    public PageObjectBuilder WithAction(string name, string @params, string body)
    {
        _model.Actions.Add(new PageActionModel(name, @params, body));
        return Self;
    }

    public PageObjectBuilder WithQuery(string name, string returnExpr)
    {
        _model.Queries.Add(new PageQueryModel(name, "string", returnExpr));
        return Self;
    }

    public PageObjectBuilder WithQuery(string name, string returnType, string returnExpr)
    {
        _model.Queries.Add(new PageQueryModel(name, returnType, returnExpr));
        return Self;
    }

    public PageObjectBuilder WithImport(string type, string module)
    {
        _model.Imports.Add(new ImportModel(type, module));
        return Self;
    }
}
