// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Builders;
using CodeGenerator.Flask.Syntax;

namespace CodeGenerator.Flask.Builders;

public class ControllerBuilder : BuilderBase<ControllerModel, ControllerBuilder>
{
    public ControllerBuilder() : base(new ControllerModel()) { }

    public static ControllerBuilder For(string name)
    {
        var builder = new ControllerBuilder();
        builder._model.Name = name;
        return builder;
    }

    public ControllerBuilder WithCrud(string modelName)
    {
        var lowerName = modelName.ToLowerInvariant();

        WithRoute("/", "GET", $"get_{lowerName}s", $"return {lowerName}_service.get_all()");
        WithRoute("/<int:id>", "GET", $"get_{lowerName}_by_id", $"return {lowerName}_service.get_by_id(id)");
        WithRoute("/", "POST", $"create_{lowerName}", $"return {lowerName}_service.create(request.get_json())");
        WithRoute("/<int:id>", "PUT", $"update_{lowerName}", $"return {lowerName}_service.update(id, request.get_json())");
        WithRoute("/<int:id>", "DELETE", $"delete_{lowerName}", $"return {lowerName}_service.delete(id)");

        return Self;
    }

    public ControllerBuilder WithRoute(string path, string method, string handlerName, string body)
    {
        _model.Routes.Add(new ControllerRouteModel
        {
            Path = path,
            Methods = [method],
            HandlerName = handlerName,
            Body = body
        });
        return Self;
    }

    public ControllerBuilder WithUrlPrefix(string prefix)
    {
        _model.UrlPrefix = prefix;
        return Self;
    }

    public ControllerBuilder WithService(string varName, string className)
    {
        _model.ServiceInstances.Add(new ControllerInstanceModel
        {
            VariableName = varName,
            ClassName = className
        });
        return Self;
    }

    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            throw new InvalidOperationException("ControllerModel requires a non-empty Name.");
        }

        base.Validate();
    }
}
