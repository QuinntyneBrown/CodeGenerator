// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Detox.Syntax;

namespace CodeGenerator.Detox.Builders;

public class PageObjectBuilder : BuilderBase<PageObjectModel, PageObjectBuilder>
{
    private PageObjectBuilder(PageObjectModel model)
        : base(model)
    {
    }

    public static PageObjectBuilder For(string name)
    {
        return new PageObjectBuilder(new PageObjectModel(name));
    }

    public PageObjectBuilder WithElement(string name, string testId)
    {
        _model.TestIds.Add(new PropertyModel(name, testId));
        return Self;
    }

    public PageObjectBuilder WithInteraction(string name, string body)
    {
        _model.Interactions.Add(new InteractionModel(name, string.Empty, body));
        return Self;
    }

    public PageObjectBuilder WithInteraction(string name, string @params, string body)
    {
        _model.Interactions.Add(new InteractionModel(name, @params, body));
        return Self;
    }

    public PageObjectBuilder WithCombinedAction(string name, List<string> steps)
    {
        _model.CombinedActions.Add(new CombinedActionModel(name, string.Empty, steps));
        return Self;
    }

    public PageObjectBuilder WithCombinedAction(string name, string @params, List<string> steps)
    {
        _model.CombinedActions.Add(new CombinedActionModel(name, @params, steps));
        return Self;
    }

    public PageObjectBuilder WithQueryHelper(string name, string body)
    {
        _model.QueryHelpers.Add(new QueryHelperModel(name, string.Empty, body));
        return Self;
    }

    public PageObjectBuilder WithVisibilityCheck(string check)
    {
        _model.VisibilityChecks.Add(check);
        return Self;
    }

    public PageObjectBuilder WithImport(string type, string module)
    {
        _model.Imports.Add(new ImportModel(type, module));
        return Self;
    }
}
