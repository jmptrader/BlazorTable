﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorTable
{
    internal static class Utillities
    {
        public static Type GetNonNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));

                        if (memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }

        public static Expression<Func<T, bool>> CallMethodType<T>(Expression<Func<T, object>> expression, Type type, string method, Type parameter, object value)
        {
            return CallMethodType(expression, type, method, new[] { parameter }, new[] { value });
        }

        public static Expression<Func<T, bool>> CallMethodType<T>(Expression<Func<T, object>> expression, Type type, string method, Type[] parameters, object[] values)
        {
            MethodInfo methodInfo = type.GetMethod(method, parameters);

            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    expression.Body,
                    methodInfo,
                    values.Select(x => Expression.Constant(x))),
                expression.Parameters);
        }

        public static Expression<Func<T, bool>> CallMethodTypeStaticSelf<T>(Expression<Func<T, object>> expression, Type type, string method, Type parameter)
        {
            MethodInfo methodInfo = type.GetMethod(method, new[] { parameter });

            var call = Expression.Call(methodInfo, expression.Body);

            return Expression.Lambda<Func<T, bool>>(call, expression.Parameters);
        }

        public static Type GetMemberUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", nameof(member));
            }
        }

        public static MemberInfo GetPropertyMemberInfo<T>(this Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (!(expression.Body is MemberExpression body))
            {
                UnaryExpression ubody = (UnaryExpression)expression.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body?.Member;
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
        }
    }
}
