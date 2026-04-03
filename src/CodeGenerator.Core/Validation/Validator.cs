// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace CodeGenerator.Core.Validation;

public class Validator<T>
{
    private readonly List<Func<T, ValidationResult>> _rules = [];

    public Validator<T> RuleFor<TProp>(
        Expression<Func<T, TProp>> expression,
        Func<TProp, bool> predicate,
        string errorMessage)
    {
        var propertyName = GetPropertyName(expression);

        _rules.Add(instance =>
        {
            var result = new ValidationResult();
            var getter = expression.Compile();
            var value = getter(instance);

            if (!predicate(value))
            {
                result.AddError(propertyName, errorMessage);
            }

            return result;
        });

        return this;
    }

    public Validator<T> When(bool condition, Action<Validator<T>> ruleBuilder)
    {
        if (condition)
        {
            ruleBuilder(this);
        }

        return this;
    }

    public Validator<T> Must(Func<T, bool> predicate, string errorMessage)
    {
        _rules.Add(instance =>
        {
            var result = new ValidationResult();

            if (!predicate(instance))
            {
                result.AddError(string.Empty, errorMessage);
            }

            return result;
        });

        return this;
    }

    public ValidationResult Validate(T instance)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            result.Merge(rule(instance));
        }

        return result;
    }

    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression operandMember)
        {
            return operandMember.Member.Name;
        }

        return expression.Body.ToString();
    }
}
