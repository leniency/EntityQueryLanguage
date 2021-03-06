using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EntityQueryLanguage.DataApi.Parsing;

namespace EntityQueryLanguage.DataApi.Util
{
    public class DataApiExpressionUtil
    {
        public static Expression SelectDynamic(ParameterExpression currentContextParam, Expression baseExp, IEnumerable<DataApiNode> fieldExpressions, ISchemaProvider schemaProvider)
        {
            Type dynamicType;
            var memberInit = CreateNewExpression(currentContextParam, fieldExpressions, schemaProvider, out dynamicType);
            var selector = Expression.Lambda(memberInit, currentContextParam);
            return Expression.Call(typeof(Enumerable), "Select", new Type[2] { currentContextParam.Type, dynamicType }, baseExp, selector);
        }

        public static Expression CreateNewExpression(Expression currentContext, IEnumerable<DataApiNode> fieldExpressions, ISchemaProvider schemaProvider)
        {
            Type dynamicType;
            var memberInit = CreateNewExpression(currentContext, fieldExpressions, schemaProvider, out dynamicType);
            return memberInit;
        }
        public static Expression CreateNewExpression(Expression currentContext, IEnumerable<DataApiNode> fieldExpressions, ISchemaProvider schemaProvider, out Type dynamicType)
        {
            var fieldExpressionsByName = fieldExpressions.ToDictionary(f => f.Name, f => f.Expression);
            dynamicType = LinqRuntimeTypeBuilder.GetDynamicType(fieldExpressions.ToDictionary(f => f.Name, f => f.Expression.Type));

            var bindings = dynamicType.GetFields().Select(p => Expression.Bind(p, fieldExpressionsByName[p.Name])).OfType<MemberBinding>();
            var newExp = Expression.New(dynamicType.GetConstructor(Type.EmptyTypes));
            var mi = Expression.MemberInit(newExp, bindings);
            return mi;
        }
    }
}