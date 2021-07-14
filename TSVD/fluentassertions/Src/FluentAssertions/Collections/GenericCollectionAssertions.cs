﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;

namespace FluentAssertions.Collections
{
    [DebuggerNonUserCode]
    public class GenericCollectionAssertions<T> :
        SelfReferencingCollectionAssertions<T, GenericCollectionAssertions<T>>
    {
        public GenericCollectionAssertions(IEnumerable<T> actualValue)
            : base(actualValue)
        {
        }

        /// <summary>
        /// Asserts that the collection does not contain any <c>null</c> items.
        /// </summary>
        /// <param name="predicate">The predicate when evaluated should not be null.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> NotContainNulls<TKey>(Expression<Func<T, TKey>> predicate, string because = "", params object[] becauseArgs)
            where TKey : class
        {
            if (Subject is null)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} not to contain <null>s{reason}, but collection is <null>.");
            }

            Func<T, TKey> compiledPredicate = predicate.Compile();

            var values = Subject
                .Where(e => compiledPredicate(e) is null)
                .ToArray();

            if (values.Length > 0)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} not to contain <null>s on {0}{reason}, but found {1}.",
                        predicate.Body,
                        values);
            }

            return new AndConstraint<GenericCollectionAssertions<T>>(this);
        }

        /// <summary>
        /// Asserts that the collection does not contain any duplicate items.
        /// </summary>
        /// <param name="predicate">The predicate to group the items by.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> OnlyHaveUniqueItems<TKey>(Expression<Func<T, TKey>> predicate, string because = "", params object[] becauseArgs)
        {
            if (Subject is null)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} to only have unique items{reason}, but found {0}.", Subject);
            }

            Func<T, TKey> compiledPredicate = predicate.Compile();

            IGrouping<TKey, T>[] groupWithMultipleItems = Subject
                .GroupBy(compiledPredicate)
                .Where(g => g.Count() > 1)
                .ToArray();

            if (groupWithMultipleItems.Length > 0)
            {
                if (groupWithMultipleItems.Length > 1)
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith("Expected {context:collection} to only have unique items on {0}{reason}, but items {1} are not unique.",
                            predicate.Body,
                            groupWithMultipleItems.SelectMany(g => g));
                }
                else
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith("Expected {context:collection} to only have unique items on {0}{reason}, but item {1} is not unique.",
                            predicate.Body,
                            groupWithMultipleItems[0].First());
                }
            }

            return new AndConstraint<GenericCollectionAssertions<T>>(this);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] args)
        {
            return BeInAscendingOrder(propertyExpression, Comparer<TSelector>.Default, because, args);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInAscendingOrder(
            IComparer<T> comparer, string because = "", params object[] args)
        {
            return BeInAscendingOrder(item => item, comparer, because, args);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] args)
        {
            return BeOrderedBy(propertyExpression, comparer, SortOrder.Ascending, because, args);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] args)
        {
            return BeInDescendingOrder(propertyExpression, Comparer<TSelector>.Default, because, args);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInDescendingOrder(
            IComparer<T> comparer, string because = "", params object[] args)
        {
            return BeInDescendingOrder(item => item, comparer, because, args);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="args">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<GenericCollectionAssertions<T>> BeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] args)
        {
            return BeOrderedBy(propertyExpression, comparer, SortOrder.Descending, because, args);
        }

        private AndConstraint<GenericCollectionAssertions<T>> BeOrderedBy<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, SortOrder direction, string because, object[] args)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer),
                    "Cannot assert collection ordering without specifying a comparer.");
            }

            if (IsValidProperty(propertyExpression, because, args))
            {
                ICollection<T> unordered = Subject.ConvertOrCastToCollection();

                Func<T, TSelector> keySelector = propertyExpression.Compile();

                IOrderedEnumerable<T> expectation = (direction == SortOrder.Ascending)
                    ? unordered.OrderBy(keySelector, comparer)
                    : unordered.OrderByDescending(keySelector, comparer);

                var orderString = propertyExpression.GetMemberPath();
                orderString = orderString == "\"\"" ? string.Empty : " by " + orderString;

                Execute.Assertion
                    .ForCondition(unordered.SequenceEqual(expectation))
                    .BecauseOf(because, args)
                    .FailWith("Expected {context:collection} {0} to be ordered{1}{reason} and result in {2}.",
                        Subject, orderString, expectation);
            }

            return new AndConstraint<GenericCollectionAssertions<T>>(this);
        }

        private bool IsValidProperty<TSelector>(Expression<Func<T, TSelector>> propertyExpression, string because, object[] args)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression),
                    "Cannot assert collection ordering without specifying a property.");
            }

            return Execute.Assertion
                .ForCondition(!(Subject is null))
                .BecauseOf(because, args)
                .FailWith("Expected {context:collection} to be ordered by {0}{reason} but found <null>.",
                    propertyExpression.GetMemberPath());
        }

        /// <summary>
        /// Asserts that all elements in a collection of objects are equivalent to a given object.
        /// </summary>
        /// <remarks>
        /// Objects within the collection are equivalent to given object when both object graphs have equally named properties with the same
        /// value, irrespective of the type of those objects. Two properties are also equal if one type can be converted to another
        /// and the result is equal.
        /// The type of a collection property is ignored as long as the collection implements <see cref="IEnumerable"/> and all
        /// items in the collection are structurally equal.
        /// Notice that actual behavior is determined by the global defaults managed by <see cref="AssertionOptions"/>.
        /// </remarks>
        /// <param name="because">
        /// An optional formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the
        /// assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public void AllBeEquivalentTo<TExpectation>(TExpectation expectation,
            string because = "", params object[] becauseArgs)
        {
            AllBeEquivalentTo(expectation, options => options, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that all elements in a collection of objects are equivalent to a given object.
        /// </summary>
        /// <remarks>
        /// Objects within the collection are equivalent to given object when both object graphs have equally named properties with the same
        /// value, irrespective of the type of those objects. Two properties are also equal if one type can be converted to another
        /// and the result is equal.
        /// The type of a collection property is ignored as long as the collection implements <see cref="IEnumerable"/> and all
        /// items in the collection are structurally equal.
        /// Notice that actual behavior is determined by the global defaults managed by <see cref="AssertionOptions"/>.
        /// </remarks>
        /// <param name="config">
        /// A reference to the <see cref="EquivalencyAssertionOptions{TExpectation}"/> configuration object that can be used
        /// to influence the way the object graphs are compared. You can also provide an alternative instance of the
        /// <see cref="EquivalencyAssertionOptions{TSubject}"/> class. The global defaults are determined by the
        /// <see cref="AssertionOptions"/> class.
        /// </param>
        /// <param name="because">
        /// An optional formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the
        /// assertion is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public void AllBeEquivalentTo<TExpectation>(TExpectation expectation,
            Func<EquivalencyAssertionOptions<TExpectation>, EquivalencyAssertionOptions<TExpectation>> config, string because = "",
            params object[] becauseArgs)
        {
            TExpectation[] repeatedExpectation = RepeatAsManyAs(expectation, Subject).ToArray();

            BeEquivalentTo(repeatedExpectation, config, because, becauseArgs);
        }

        private static IEnumerable<TExpectation> RepeatAsManyAs<TExpectation>(TExpectation value, IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return Enumerable.Empty<TExpectation>();
            }

            return RepeatAsManyAsIterator(value, enumerable);
        }

        private static IEnumerable<TExpectation> RepeatAsManyAsIterator<TExpectation>(TExpectation value, IEnumerable<T> enumerable)
        {
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return value;
                }
            }
        }
    }
}
