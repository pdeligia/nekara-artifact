using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using FluentAssertions.Extensions;
using Xunit;
using Xunit.Sdk;

namespace FluentAssertions.Specs
{
    [Collection("Equivalency")]
    public class CollectionEquivalencySpecs
    {
        public class SubDummy
        {
            public SubDummy(int id)
            {
                Id = id;
            }

            public int Id { get; }

            public override bool Equals(object obj)
            {
                if (!(obj is SubDummy))
                {
                    return false;
                }

                return Id == ((SubDummy)obj).Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        public class TestDummy
        {
            public TestDummy(SubDummy sd)
            {
                Sd = sd;
            }

            public SubDummy Sd { get; }
        }

        private class NonGenericCollection : ICollection
        {
            private readonly IList<object> inner;

            public NonGenericCollection(IList<object> inner)
            {
                this.inner = inner;
            }

            public IEnumerator GetEnumerator()
            {
                foreach (var @object in inner)
                {
                    yield return @object;
                }
            }

            public void CopyTo(Array array, int index)
            {
                ((ICollection)inner).CopyTo(array, index);
            }

            public int Count => inner.Count;

            public object SyncRoot => ((ICollection)inner).SyncRoot;

            public bool IsSynchronized => ((ICollection)inner).IsSynchronized;
        }

        private class MultiEnumerable : IEnumerable<int>, IEnumerable<long>
        {
            private readonly List<int> ints = new List<int>();
            private readonly List<long> longs = new List<long>();

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                return ints.GetEnumerator();
            }

            public IEnumerator GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<long> IEnumerable<long>.GetEnumerator()
            {
                return longs.GetEnumerator();
            }
        }

        private class EnumerableOfStringAndObject : IEnumerable<object>, IEnumerable<string>
        {
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<string> GetEnumerator()
            {
                yield return string.Empty;
            }
        }

        public class MyObject
        {
            public string MyString { get; set; }
            public MyChildObject Child { get; set; }
        }

        public class MyChildObject
        {
            public int Id { get; set; }

            public string MyChildString { get; set; }

            public override bool Equals(object obj)
            {
                return obj is MyChildObject && ((MyChildObject)obj).Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        private class SelectPropertiesSelectionRule : IMemberSelectionRule
        {
            public bool OverridesStandardIncludeRules => throw new NotImplementedException();

            public IEnumerable<SelectedMemberInfo> SelectMembers(IEnumerable<SelectedMemberInfo> selectedMembers,
                IMemberInfo context, IEquivalencyAssertionOptions config)
            {
                return context.CompileTimeType.GetNonPrivateProperties().Select(SelectedMemberInfo.Create);
            }

            bool IMemberSelectionRule.IncludesMembers => OverridesStandardIncludeRules;
        }

        private class SelectNoMembersSelectionRule : IMemberSelectionRule
        {
            public bool OverridesStandardIncludeRules => true;

            public IEnumerable<SelectedMemberInfo> SelectMembers(IEnumerable<SelectedMemberInfo> selectedMembers,
                IMemberInfo context, IEquivalencyAssertionOptions config)
            {
                return Enumerable.Empty<SelectedMemberInfo>();
            }

            bool IMemberSelectionRule.IncludesMembers => OverridesStandardIncludeRules;
        }

        public class UserRolesLookupElement
        {
            private readonly Dictionary<Guid, List<string>> innerRoles = new Dictionary<Guid, List<string>>();

            public virtual Dictionary<Guid, IEnumerable<string>> Roles
            {
                get { return innerRoles.ToDictionary(x => x.Key, y => y.Value.Select(z => z)); }
            }

            public void Add(Guid userId, params string[] roles)
            {
                innerRoles[userId] = roles.ToList();
            }
        }

        [Fact]
        public void When_a_byte_array_does_not_match_strictly_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new byte[] { 1, 2, 3, 4, 5, 6 };

            var expectation = new byte[] { 6, 5, 4, 3, 2, 1 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected*item[0]*6*1*");
        }

        [Fact]
        public void When_a_collection_does_not_match_it_should_include_items_in_message()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new int[] { 1, 2 };

            var expectation = new int[] { 3, 2, 1 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected*but*{1, 2}*1 item(s) less than*{3, 2, 1}*");
        }

        [Fact]
        public void When_a_nullable_collection_does_not_match_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Values = (ImmutableArray<int>?)ImmutableArray.Create<int>(1, 2, 3)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(new
            {
                Values = (ImmutableArray<int>?)ImmutableArray.Create<int>(1, 2, 4)
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected member Values[2] to be 4, but found 3*");
        }

        [Fact]
        public void
            When_a_collection_contains_a_reference_to_an_object_that_is_also_in_its_parent_it_should_not_be_treated_as_a_cyclic_reference
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var logbook = new BasicEquivalencySpecs.LogbookCode("SomeKey");

            var logbookEntry = new BasicEquivalencySpecs.LogbookEntryProjection
            {
                Logbook = logbook,
                LogbookRelations = new[]
                {
                    new BasicEquivalencySpecs.LogbookRelation
                    {
                        Logbook = logbook
                    }
                }
            };

            var equivalentLogbookEntry = new BasicEquivalencySpecs.LogbookEntryProjection
            {
                Logbook = logbook,
                LogbookRelations = new[]
                {
                    new BasicEquivalencySpecs.LogbookRelation
                    {
                        Logbook = logbook
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () =>
                    logbookEntry.Should().BeEquivalentTo(equivalentLogbookEntry);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_a_collection_contains_less_items_than_expected_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new[]
                {
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    },
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "Jane"
                    }
                }
            };

            var subject = new
            {
                Customers = new[]
                {
                    new CustomerDto
                    {
                        Age = 24,
                        Birthdate = 21.September(1973),
                        Name = "John"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage(
                    "*member Customers to be a collection with 2 item(s), but*contains 1 item(s) less than*");
        }

        [Fact]
        public void
            When_a_collection_contains_more_items_than_expected_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new[]
                {
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    }
                }
            };

            var subject = new
            {
                Customers = new[]
                {
                    new CustomerDto
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "Jane"
                    },
                    new CustomerDto
                    {
                        Age = 24,
                        Birthdate = 21.September(1973),
                        Name = "John"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage(
                    "*member Customers to be a collection with 1 item(s), but*contains 1 item(s) more than*");
        }

        [Fact]
        public void
            When_a_collection_property_contains_objects_with_matching_properties_in_any_order_it_should_not_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new[]
                {
                    new Customer
                    {
                        Age = 32,
                        Birthdate = 31.July(1978),
                        Name = "Jane"
                    },
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    }
                }
            };

            var subject = new
            {
                Customers = new[]
                {
                    new CustomerDto
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    },
                    new CustomerDto
                    {
                        Age = 32,
                        Birthdate = 31.July(1978),
                        Name = "Jane"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, o => o.ExcludingMissingMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_a_collection_property_contains_objects_with_mismatching_properties_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new[]
                {
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    }
                }
            };

            var subject = new
            {
                Customers = new[]
                {
                    new CustomerDto
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "Jane"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*Customers[0].Name*John*Jane*");
        }

        [Fact]
        public void
            When_a_collection_property_was_expected_but_the_property_is_not_a_collection_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Customers = "Jane, John"
            };

            var expected = new
            {
                Customers = new[]
                {
                    new Customer
                    {
                        Age = 38,
                        Birthdate = 20.September(1973),
                        Name = "John"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage(
                    "*member Customers to be {*Customer*{*Age*38*, but*Jane, John*");
        }

        [Fact]
        public void
            When_a_complex_object_graph_with_collections_matches_expectations_it_should_not_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Bytes = new byte[]
                {
                    1, 2, 3, 4
                },
                Object = new
                {
                    A = 1,
                    B = 2
                }
            };

            var expected = new
            {
                Bytes = new byte[]
                {
                    1, 2, 3, 4
                },
                Object = new
                {
                    A = 1,
                    B = 2
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_deeply_nested_property_of_a_collection_with_an_invalid_value_is_excluded_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Text = "Root",
                Level = new
                {
                    Text = "Level1",
                    Level = new
                    {
                        Text = "Level2"
                    },
                    Collection = new[]
                    {
                        new { Number = 1, Text = "Text" },
                        new { Number = 2, Text = "Actual" }
                    }
                }
            };

            var expected = new
            {
                Text = "Root",
                Level = new
                {
                    Text = "Level1",
                    Level = new
                    {
                        Text = "Level2"
                    },
                    Collection = new[]
                    {
                        new { Number = 1, Text = "Text" },
                        new { Number = 2, Text = "Expected" }
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.Excluding(x => x.Level.Collection[1].Number).Excluding(x => x.Level.Collection[1].Text));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_a_dictionary_property_is_detected_it_should_ignore_the_order_of_the_pairs
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new Dictionary<string, string>
                {
                    ["Key2"] = "Value2",
                    ["Key1"] = "Value1"
                }
            };

            var subject = new
            {
                Customers = new Dictionary<string, string>
                {
                    ["Key1"] = "Value1",
                    ["Key2"] = "Value2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_a_object_implements_multiple_IEnumerable_interfaces_but_the_declared_type_is_assignable_to_only_one_and_runtime_checking_is_configured_it_should_fail
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<string> collection1 = new EnumerableOfStringAndObject();
            IEnumerable<string> collection2 = new EnumerableOfStringAndObject();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => collection1.Should().BeEquivalentTo(collection2, opts => opts.RespectingRuntimeTypes());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>("the runtime type is assignable to two IEnumerable interfaces")
                .WithMessage("*cannot determine which one*");
        }

        [Fact]
        public void When_a_specific_property_is_included_it_should_ignore_the_rest_of_the_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var result = new[]
            {
                new
                {
                    A = "aaa",
                    B = "bbb"
                }
            };

            var expected = new
            {
                A = "aaa",
                B = "ccc"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => result.Should().BeEquivalentTo(new[] { expected }, options => options.Including(x => x.A));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_a_strongly_typed_collection_is_declared_as_an_untyped_collection_and_runtime_checking_is_configured_is_should_use_the_runtime_type
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ICollection collection1 = new List<Car> { new Car() };
            ICollection collection2 = new List<Customer> { new Customer() };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => collection1.Should().BeEquivalentTo(collection2, opts => opts.RespectingRuntimeTypes());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>("the items have different runtime types");
        }

        [Fact]
        public void When_all_subject_items_are_equivalent_to_expectation_object_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<SomeDto>
            {
                new SomeDto { Name = "someDto", Age = 1 },
                new SomeDto { Name = "someDto", Age = 1 },
                new SomeDto { Name = "someDto", Age = 1 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().AllBeEquivalentTo(new
            {
                Name = "someDto",
                Age = 1,
                Birthdate = new DateTime()
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_all_subject_items_are_not_equivalent_to_expectation_object_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[] { 1, 2, 3 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().AllBeEquivalentTo(1);

            ////-----------------------------------------------------------------------------------------------------------
            //// Assert
            ////-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>().WithMessage(
                "Expected item[1] to be 1, but found 2.*Expected item[2] to be 1, but found 3*");
        }

        [Fact]
        public void
            When_an_object_implements_multiple_IEnumerable_interfaces_but_the_declared_type_is_assignable_to_only_one_it_should_respect_the_declared_type
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<string> collection1 = new EnumerableOfStringAndObject();
            IEnumerable<string> collection2 = new EnumerableOfStringAndObject();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => collection1.Should().BeEquivalentTo(collection2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow("the declared type is assignable to only one IEnumerable interface");
        }

        [Fact]
        public void When_an_unordered_collection_must_be_strict_using_a_predicate_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 1, 2, 3, 4, 5 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            var expectation = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 5, 4, 3, 2, 1 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation, options =>
                options.WithStrictOrderingFor(s => s.SelectedMemberPath.Contains("UnorderedCollection")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("*Expected item[0].UnorderedCollection*5 item(s)*empty collection*");
        }

        [Fact]
        public void When_an_unordered_collection_must_be_strict_using_an_expression_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 1, 2, 3, 4, 5 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            var expectation = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 5, 4, 3, 2, 1 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () =>
                    subject.Should().BeEquivalentTo(expectation,
                        options => options
                            .WithStrictOrderingFor(
                                s => s.UnorderedCollection));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "*Expected item[0].UnorderedCollection*5 item(s)*empty collection*");
        }

        [Fact]
        public void When_an_unordered_collection_must_not_be_strict_using_a_predicate_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 1, 2, 3, 4, 5 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            var expectation = new[]
            {
                new
                {
                    Name = "John",
                    UnorderedCollection = new[] { 5, 4, 3, 2, 1 }
                },
                new
                {
                    Name = "Jane",
                    UnorderedCollection = new int[0]
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation, options => options
                .WithStrictOrdering()
                .WithoutStrictOrderingFor(s => s.SelectedMemberPath.Contains("UnorderedCollection")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void
            When_asserting_equivalence_of_collections_and_configured_to_use_runtime_properties_it_should_respect_the_runtime_type
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ICollection collection1 = new NonGenericCollection(new[] { new Customer() });
            ICollection collection2 = new NonGenericCollection(new[] { new Car() });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () =>
                    collection1.Should().BeEquivalentTo(collection2,
                        opts => opts.RespectingRuntimeTypes());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>("the types have different properties");
        }

        [Fact]
        public void When_asserting_equivalence_of_generic_collections_it_should_respect_the_declared_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var collection1 = new Collection<CustomerType> { new DerivedCustomerType("123") };
            var collection2 = new Collection<CustomerType> { new CustomerType("123") };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => collection1.Should().BeEquivalentTo(collection2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow("the objects are equivalent according to the members on the declared type");
        }

        [Fact]
        public void When_asserting_equivalence_of_non_generic_collections_it_should_respect_the_runtime_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ICollection subject = new NonGenericCollection(new[] { new Customer() });
            ICollection expectation = new NonGenericCollection(new[] { new Car() });

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*Wheels*not have*VehicleId*not have*");
        }

        [Fact]
        public void When_custom_assertion_rules_are_utilized_the_rules_should_be_respected()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new
                {
                    Value = new Customer
                    {
                        Name = "John",
                        Age = 31,
                        Id = 1
                    }
                },
                new
                {
                    Value =new Customer
                    {
                        Name = "Jane",
                        Age = 24,
                        Id = 2
                    }
                }
            };

            var expectation = new[]
            {
                new
                {
                    Value = new CustomerDto
                    {
                        Name = "John",
                        Age = 30
                    }
                },
                new
                {
                    Value = new CustomerDto
                    {
                        Name = "Jane",
                        Age = 24
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, opts => opts
                .Using<int>(ctx => ((int)ctx.Subject).Should().BeInRange(ctx.Expectation - 1, ctx.Expectation + 1))
                .WhenTypeIs<int>()
            );

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_expectation_is_null_enumerable_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = Enumerable.Empty<object>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected subject to be <null>, but found*");
        }

        [Fact]
        public void When_nested_objects_are_excluded_from_collections_it_should_use_simple_equality_semantics()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new MyObject
            {
                MyString = "identical string",
                Child = new MyChildObject
                {
                    Id = 1,
                    MyChildString = "identical string"
                }
            };

            var expectation = new MyObject
            {
                MyString = "identical string",
                Child = new MyChildObject
                {
                    Id = 1,
                    MyChildString = "DIFFERENT STRING"
                }
            };

            IList<MyObject> actualList = new List<MyObject> { actual };
            IList<MyObject> expectationList = new List<MyObject> { expectation };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actualList.Should().BeEquivalentTo(expectationList, opt => opt.ExcludingNestedObjects());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_no_collection_item_matches_it_should_report_the_closest_match
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 30,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "Jane",
                    Age = 30,
                    Id = 2
                },
                new Customer
                {
                    Name = "John",
                    Age = 28,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected*item[1].Age*28*27*");
        }

        [Fact]
        public void When_only_a_deeply_nested_property_is_included_it_should_exclude_the_other_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actualObjects = new[]
            {
                new
                {
                    SubObject = new
                    {
                        Property1 = "John",
                        Property2 = "John"
                    }
                },
                new
                {
                    SubObject = new
                    {
                        Property1 = "John",
                        Property2 = "John"
                    }
                }
            };

            var expectedObjects = new[]
            {
                new
                {
                    SubObject = new
                    {
                        Property1 = "John",
                        Property2 = "John"
                    }
                },
                new
                {
                    SubObject = new
                    {
                        Property1 = "John",
                        Property2 = "Jane"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actualObjects.Should().BeEquivalentTo(expectedObjects, options =>
                options.Including(order => order.SubObject.Property1));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_selection_rules_are_configured_they_should_be_evaluated_from_right_to_left()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var list1 = new[] { new { Value = 3 } };
            var list2 = new[] { new { Value = 2 } };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => list1.Should().BeEquivalentTo(list2, config =>
            {
                config.WithoutSelectionRules();
                config.Using(new SelectNoMembersSelectionRule());
                config.Using(new SelectPropertiesSelectionRule());
                return config;
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*to be 2, but found 3*");
        }

        [Fact]
        public void When_subject_and_expectation_are_null_enumerable_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<object> subject = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_subject_is_empty_and_expectation_is_object_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<char>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().AllBeEquivalentTo('g');

            ////-----------------------------------------------------------------------------------------------------------
            //// Assert
            ////-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_subject_is_null_and_expectation_is_enumerable_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IEnumerable<object> subject = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(Enumerable.Empty<object>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected subject not to be <null>*");
        }

        [Fact]
        public void When_the_expectation_is_not_a_multi_dimensional_array_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo("not-a-multi-dimensional-array");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*29*but*contains 23 item(s) less than*");
        }

        [Fact]
        public void When_the_expectation_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo<object>(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected subject to be <null>*{1, 2, 3, 4, 5, 6}*");
#else
                .WithMessage("Expected actual to be <null>*{1, 2, 3, 4, 5, 6}*");
#endif
        }

        [Fact]
        public void When_the_length_of_the_2nd_dimension_differs_between_the_arrays_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            var expectation = new[,]
            {
                { 1, 2, 3 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected dimension 0 to contain 1 item(s), but found 2*");
        }

        [Fact]
        public void When_the_length_of_the_first_dimension_differs_between_the_arrays_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            var expectation = new[,]
            {
                { 1, 2 },
                { 4, 5 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected dimension 1 to contain 2 item(s), but found 3*");
        }

        [Fact]
        public void When_the_number_of_dimensions_of_the_arrays_are_not_the_same_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            var expectation = new[]
            {
                1, 2
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected subject*2 item(s)*but*contains 4 item(s) more than*");
#else
                .WithMessage("Expected actual*2 item(s)*but*contains 4 item(s) more than*");
#endif
        }

        [Fact]
        public void When_the_other_dictionary_does_not_contain_enough_items_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = new Dictionary<string, string>
                {
                    ["Key1"] = "Value1",
                    ["Key2"] = "Value2"
                }
            };

            var subject = new
            {
                Customers = new Dictionary<string, string>
                {
                    ["Key1"] = "Value1"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected*Customers*dictionary*2 item(s)*but*misses*Key2*");
        }

        [Fact]
        public void When_the_other_property_is_not_a_dictionary_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new
            {
                Customers = "I am a string"
            };

            var subject = new
            {
                Customers = new Dictionary<string, string>
                {
                    ["Key2"] = "Value2",
                    ["Key1"] = "Value1"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*member*Customers*String*found*Dictionary*");
        }

        [Fact]
        public void
            When_the_root_object_is_referenced_from_an_object_in_a_nested_collection_it_should_treat_it_as_a_cyclic_reference
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var company1 = new MyCompany { Name = "Company" };
            var user1 = new MyUser { Name = "User", Company = company1 };
            company1.Users = new List<MyUser> { user1 };
            var logo1 = new MyCompanyLogo { Url = "blank", Company = company1, CreatedBy = user1 };
            company1.Logo = logo1;

            var company2 = new MyCompany { Name = "Company" };
            var user2 = new MyUser { Name = "User", Company = company2 };
            company2.Users = new List<MyUser> { user2 };
            var logo2 = new MyCompanyLogo { Url = "blank", Company = company2, CreatedBy = user2 };
            company2.Logo = logo2;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => company1.Should().BeEquivalentTo(company2, o => o.IgnoringCyclicReferences());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void
            When_the_subject_contains_less_items_than_expected_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "*subject to be a collection with 2 item(s), but*contains 1 item(s) less than*");
        }

        [Fact]
        public void
            When_the_subject_contains_more_items_than_expected_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "Expected subject to be a collection with 1 item(s), but*contains 1 item(s) more than*");
        }

        [Fact]
        public void
            When_the_subject_contains_same_number_of_items_and_both_contain_duplicates_it_should_succeed
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void
            When_the_subject_contains_same_number_of_items_but_expectation_contains_duplicates_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "Expected item[1].Name to be \"John\", but \"Jane\" differs near*");
        }

        [Fact]
        public void
            When_the_subject_contains_same_number_of_items_but_subject_contains_duplicates_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "Expected item[1].Name to be \"Jane\", but \"John\" differs near*");
        }

        [Fact]
        public void
            When_two_collections_have_nested_members_of_the_contained_equivalent_but_not_equal_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var list1 = new[]
            {
                new
                {
                    Nested = new ClassWithOnlyAProperty
                    {
                        Value = 1
                    }
                }
            };

            var list2 = new[]
            {
                new
                {
                    Nested = new
                    {
                        Value = 1
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => list1.Should().BeEquivalentTo(list2, opts => opts);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_collections_have_properties_of_the_contained_items_excluded_but_still_differ_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var list1 = new[] { new KeyValuePair<int, int>(1, 123) };
            var list2 = new[] { new KeyValuePair<int, int>(2, 321) };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => list1.Should().BeEquivalentTo(list2, config => config
                .Excluding(ctx => ctx.Key)
                .ComparingByMembers<KeyValuePair<int, int>>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected item[0].Value to be 321, but found 123.*");
        }

        [Fact]
        public void
            When_two_equivalent_dictionaries_are_compared_directly_as_if_it_is_a_collection_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var result = new Dictionary<string, int?>
            {
                ["C"] = null,
                ["B"] = 0,
                ["A"] = 0
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => result.Should().BeEquivalentTo(new Dictionary<string, int?>
            {
                ["A"] = 0,
                ["B"] = 0,
                ["C"] = null
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_two_equivalent_dictionaries_are_compared_directly_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var result = new Dictionary<string, int>
            {
                ["C"] = 0,
                ["B"] = 0,
                ["A"] = 0
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => result.Should().BeEquivalentTo(new Dictionary<string, int>
            {
                ["A"] = 0,
                ["B"] = 0,
                ["C"] = 0
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_lists_dont_contain_the_same_structural_equal_objects_it_should_throw
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 30,
                    Id = 2
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected*item[1].Age*30*24*");
        }

        [Fact]
        public void When_two_lists_only_differ_in_excluded_properties_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<CustomerDto>
            {
                new CustomerDto
                {
                    Name = "John",
                    Age = 27
                },
                new CustomerDto
                {
                    Name = "Jane",
                    Age = 30
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () =>
                    subject.Should().BeEquivalentTo(expectation,
                        options => options
                            .ExcludingMissingMembers()
                            .Excluding(c => c.Age));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_two_multi_dimensional_arrays_are_equivalent_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            var expectation = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_two_multi_dimensional_arrays_are_not_equivalent_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 }
            };

            var expectation = new[,]
            {
                { 1,  2, 4 },
                { 4, -5, 6 }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*item[0,2]*4*3*item[1,1]*-5*5*");
        }

        [Fact]
        public void When_two_nested_dictionaries_contain_null_values_it_should_not_crash()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var projection = new
            {
                ReferencedEquipment = new Dictionary<int, string>
                {
                    [1] = null
                }
            };

            var persistedProjection = new
            {
                ReferencedEquipment = new Dictionary<int, string>
                {
                    [1] = null
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => persistedProjection.Should().BeEquivalentTo(projection);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_two_nested_dictionaries_contain_null_values_it_should_not_crash2()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var userId = Guid.NewGuid();

            var actual = new UserRolesLookupElement();
            actual.Add(userId, "Admin", "Special");

            var expected = new UserRolesLookupElement();
            expected.Add(userId, "Admin", "Other");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*Roles[*][1]*Other*Special*");
        }

        [Fact]
        public void When_two_nested_dictionaries_do_not_match_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var projection = new
            {
                ReferencedEquipment = new Dictionary<int, string>
                {
                    [1] = "Bla1"
                }
            };

            var persistedProjection = new
            {
                ReferencedEquipment = new Dictionary<int, string>
                {
                    [1] = "Bla2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => persistedProjection.Should().BeEquivalentTo(projection);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected*ReferencedEquipment[1]*Bla1*Bla2*2*index 3*");
        }

        [Fact]
        public void
            When_two_ordered_lists_are_structurally_equivalent_it_should_succeed
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new List<Customer>
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_two_unordered_lists_are_structurally_equivalent_and_order_is_strict_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new Collection<Customer>
            {
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation, options => options.WithStrictOrdering());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage(
                    "Expected item[0].Name*Jane*John*item[1].Name*John*Jane*");
        }

        [Fact]
        public void
            When_two_unordered_lists_are_structurally_equivalent_it_should_succeed
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                },
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                }
            };

            var expectation = new Collection<Customer>
            {
                new Customer
                {
                    Name = "Jane",
                    Age = 24,
                    Id = 2
                },
                new Customer
                {
                    Name = "John",
                    Age = 27,
                    Id = 1
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action =
                () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }
    }
}
