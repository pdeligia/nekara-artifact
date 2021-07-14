using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using Xunit;
using Xunit.Sdk;

namespace FluentAssertions.Specs
{
    [Collection("Equivalency")]

    public class BasicEquivalencySpecs
    {
        public enum LocalOtherType : byte
        {
            Default,
            NonDefault
        }

        public enum LocalType : byte
        {
            Default,
            NonDefault
        }

        #region General

        [Fact]
        public void When_expectation_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo<object>(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected subject to be <null>, but found { }*");
        }

        [Fact]
        public void When_comparing_nested_collection_with_a_null_value_it_should_fail_with_the_correct_message()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new MyClass { Items = new[] { "a" } }
            };

            var expectation = new[]
            {
                new MyClass()
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected*item[0].Items*null*, but found*\"a\"*");
        }

        public class MyClass
        {
            public IEnumerable<string> Items { get; set; }
        }

        [Fact]
        public void When_subject_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SomeDto subject = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(new
            {
            });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
#if NETCOREAPP1_1
                "Expected object to be*, but found <null>*");
#else
                "Expected subject to be*, but found <null>*");
#endif
        }

        [Fact]
        public void When_subject_and_expectation_are_null_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            SomeDto subject = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo<object>(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_equivalence_on_a_value_type_from_system_it_should_not_do_a_structural_comparision()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            // DateTime is used as an example because the current implementation
            // would hit the recursion-depth limit if structural equivalence were attempted.
            var date1 = new
            {
                Property = DateTime.Parse("2011-01-01")
            };

            var date2 = new
            {
                Property = DateTime.Parse("2011-01-01")
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => date1.Should().BeEquivalentTo(date2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_an_object_hides_object_equals_it_should_be_compared_using_its_members()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new VirtualClassOverride
            {
                Property = "Value",
                OtherProperty = "Actual"
            };

            var expected = new VirtualClassOverride
            {
                Property = "Value",
                OtherProperty = "Expected"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>("*OtherProperty*Expected*Actual*");
        }

        public class VirtualClass
        {
            public string Property { get; set; }

            public new virtual bool Equals(object obj)
            {
                return (obj is VirtualClass other) && other.Property.Equals(Property);
            }
        }

        public class VirtualClassOverride : VirtualClass
        {
            public string OtherProperty { get; set; }
        }

        [Fact]
        public void When_treating_a_value_type_in_a_collection_as_a_complex_type_it_should_compare_them_by_members()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new ClassWithValueSemanticsOnSingleProperty
                {
                    Key = "SameKey",
                    NestedProperty = "SomeValue"
                }
            };

            var expected = new[]
            {
                new ClassWithValueSemanticsOnSingleProperty
                {
                    Key = "SameKey",
                    NestedProperty = "OtherValue"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.ComparingByMembers<ClassWithValueSemanticsOnSingleProperty>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*NestedProperty*OtherValue*SomeValue*");
        }

        [Fact]
        public void When_treating_a_value_type_as_a_complex_type_it_should_compare_them_by_members()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "SameKey",
                NestedProperty = "SomeValue"
            };

            var expected = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "SameKey",
                NestedProperty = "OtherValue"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.ComparingByMembers<ClassWithValueSemanticsOnSingleProperty>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*NestedProperty*OtherValue*SomeValue*");
        }

        [Fact]
        public void When_treating_a_type_as_value_type_but_it_was_already_marked_as_reference_type_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "Don't care"
            };

            var expected = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "Don't care"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, options => options
               .ComparingByMembers<ClassWithValueSemanticsOnSingleProperty>()
               .ComparingByValue<ClassWithValueSemanticsOnSingleProperty>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>().WithMessage(
                $"*compare {nameof(ClassWithValueSemanticsOnSingleProperty)}*value*already*members*");
        }

        [Fact]
        public void When_treating_a_type_as_reference_type_but_it_was_already_marked_as_value_type_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "Don't care"
            };

            var expected = new ClassWithValueSemanticsOnSingleProperty
            {
                Key = "Don't care"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, options => options
               .ComparingByValue<ClassWithValueSemanticsOnSingleProperty>()
               .ComparingByMembers<ClassWithValueSemanticsOnSingleProperty>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>().WithMessage(
                $"*compare {nameof(ClassWithValueSemanticsOnSingleProperty)}*members*already*value*");
        }

        [Fact]
        public void When_treating_a_complex_type_in_a_collection_as_a_value_type_it_should_compare_them_by_value()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new[]
            {
                new
                {
                    Address = IPAddress.Parse("1.2.3.4"),
                    Word = "a"
                }
            };

            var expected = new[]
            {
                new
                {
                    Address = IPAddress.Parse("1.2.3.4"),
                    Word = "a"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.ComparingByValue<IPAddress>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_treating_a_complex_type_as_a_value_type_it_should_compare_them_by_value()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Address = IPAddress.Parse("1.2.3.4"),
                Word = "a"
            };

            var expected = new
            {
                Address = IPAddress.Parse("1.2.3.4"),
                Word = "a"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.ComparingByValue<IPAddress>());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_type_originates_from_the_System_namespace_it_should_be_treated_as_a_value_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                UriBuilder = new UriBuilder("http://localhost:9001/api"),
            };

            var expected = new
            {
                UriBuilder = new UriBuilder("https://localhost:9002/bapi"),
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*UriBuilder to be https://localhost:9002/bapi, but found http://localhost:9001/api*");
        }

        [Fact]
        public void When_asserting_equivilence_on_a_string_it_should_use_string_specific_failure_messages()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string s1 = "hello";
            string s2 = "good-bye";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => s1.Should().BeEquivalentTo(s2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*to be*\"good-bye\" with a length of 8, but \"hello\" has a length of 5*");
        }

        [Fact]
        public void When_asserting_equivalence_of_strings_typed_as_objects_it_should_compare_them_as_strings()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------

            // The convoluted construction is so the compiler does not optimize the two objects to be the same.
            object s1 = new string('h', 2);
            object s2 = "hh";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => s1.Should().BeEquivalentTo(s2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_equivalence_of_ints_typed_as_objects_it_should_use_the_runtime_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object s1 = 1;
            object s2 = 1;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => s1.Should().BeEquivalentTo(s2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_all_field_of_the_object_are_equal_equivalency_should_pass()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var object1 = new ClassWithOnlyAField { Value = 1 };
            var object2 = new ClassWithOnlyAField { Value = 1 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => object1.Should().BeEquivalentTo(object2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_nummer_values_are_convertible_it_should_treat_them_as_equivalent()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new Dictionary<string, long>
            {
                ["001"] = 1L,
                ["002"] = 2L
            };

            var expected = new Dictionary<string, int>
            {
                ["001"] = 1,
                ["002"] = 2
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected, x => x.WithTracing());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_all_field_of_the_object_are_not_equal_equivalency_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var object1 = new ClassWithOnlyAField { Value = 1 };
            var object2 = new ClassWithOnlyAField { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => object1.Should().BeEquivalentTo(object2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void When_a_field_on_the_subject_matches_a_property_the_members_should_match_for_equivalence()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var onlyAField = new ClassWithOnlyAField { Value = 1 };
            var onlyAProperty = new ClassWithOnlyAProperty { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => onlyAField.Should().BeEquivalentTo(onlyAProperty);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected member Value to be 101, but found 1.*");
        }

        [Fact]
        public void When_asserting_equivalence_including_only_fields_it_should_not_match_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var onlyAField = new ClassWithOnlyAField { Value = 1 };
            object onlyAProperty = new ClassWithOnlyAProperty { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => onlyAProperty.Should().BeEquivalentTo(onlyAField, opts => opts.ExcludingProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expectation has member Value that the other object does not have.*");
        }

        [Fact]
        public void When_asserting_equivalence_including_only_properties_it_should_not_match_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var onlyAField = new ClassWithOnlyAField { Value = 1 };
            var onlyAProperty = new ClassWithOnlyAProperty { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => onlyAField.Should().BeEquivalentTo(onlyAProperty, opts => opts.IncludingAllDeclaredProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expectation has member Value that the other object does not have.*");
        }

        [Fact]
        public void
            When_asserting_equivalence_of_objects_including_enumerables_it_should_print_the_failure_message_only_once()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var record = new
            {
                Member1 = "",
                Member2 = new[] { "", "" }
            };

            var record2 = new
            {
                Member1 = "different",
                Member2 = new[] { "", "" }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => record.Should().BeEquivalentTo(record2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                @"Expected member Member1 to be*""different"" with a length of 9, but*"""" has a length of 0.*");
        }

        [Fact]
        public void When_asserting_object_equivalence_against_a_null_value_it_should_properly_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => ((object)null).Should().BeEquivalentTo("foo");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*foo*null*");
        }

        [Fact]
        public void When_the_graph_contains_guids_it_should_properly_format_them()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual =
                new[]
                {
                    new { Id = Guid.NewGuid(), Name = "Name" }
                };

            var expected =
                new[]
                {
                    new { Id = Guid.NewGuid(), Name = "Name" }
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected item[0].Id to be *-*, but found *-*");
        }

        #endregion

        #region Selection Rules

        [Fact]
        public void When_specific_properties_have_been_specified_it_should_ignore_the_other_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            var customer = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(customer, options => options
                .Including(d => d.Age)
                .Including(d => d.Birthdate));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_predicate_for_properties_to_include_has_been_specified_it_should_ignore_the_other_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            var customer = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(customer, options => options
                .Including(info => info.SelectedMemberPath.EndsWith("Age"))
                .Including(info => info.SelectedMemberPath.EndsWith("Birthdate")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_non_property_expression_is_provided_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dto = new CustomerDto();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => dto.Should().BeEquivalentTo(dto, options => options.Including(d => d.GetType()));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<ArgumentException>().WithMessage(
                "Expression <d.GetType()> cannot be used to select a member.*");
        }

        [Fact]
        public void When_including_a_property_it_should_exactly_match_the_property()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new
            {
                DeclaredType = LocalOtherType.NonDefault,
                Type = LocalType.NonDefault
            };

            var expectation = new
            {
                DeclaredType = LocalOtherType.NonDefault
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation,
                config => config.Including(o => o.DeclaredType));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        public class CustomType
        {
            public string Name { get; set; }
        }

        public class ClassA
        {
            public List<CustomType> ListOfCustomTypes { get; set; }
        }

        [Fact]
        public void When_including_a_property_using_an_expression_it_should_evaluate_it_from_the_root()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var list1 = new List<CustomType>
            {
                new CustomType { Name = "A" },
                new CustomType { Name = "B" }
            };

            var list2 = new List<CustomType>
            {
                new CustomType { Name = "C" },
                new CustomType { Name = "D" }
            };

            var objectA = new ClassA { ListOfCustomTypes = list1 };
            var objectB = new ClassA { ListOfCustomTypes = list2 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => objectA.Should().BeEquivalentTo(objectB, options => options.Including(x => x.ListOfCustomTypes));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().
                WithMessage("*C*but*A*D*but*B*");
        }

        [Fact]
        public void When_null_is_provided_as_property_expression_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dto = new CustomerDto();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => dto.Should().BeEquivalentTo(dto, options => options.Including((Expression<Func<CustomerDto, object>>)null));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<ArgumentNullException>().WithMessage(
                "Expected an expression, but found <null>.*");
        }

        [Fact]
        public void When_including_fields_it_should_succeed_if_just_the_included_field_match()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            var class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () =>
                    class1.Should().BeEquivalentTo(class2, opts => opts.Including(_ => _.Field1).Including(_ => _.Field2));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow("the only selected fields have the same value");
        }

        [Fact]
        public void When_including_fields_it_should_fail_if_any_included_field_do_not_match()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            var class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () =>
                    class1.Should().BeEquivalentTo(class2,
                        opts => opts.Including(_ => _.Field1).Including(_ => _.Field2).Including(_ => _.Field3));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected member Field3*");
        }

        [Fact]
        public void When_only_the_excluded_property_doesnt_match_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dto = new CustomerDto
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            var customer = new Customer
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-----------------------------------------------------------------------------------------------------------
            dto.Should().BeEquivalentTo(customer, options => options
                .Excluding(d => d.Name)
                .Excluding(d => d.Id));
        }

        [Fact]
        public void When_excluding_members_it_should_pass_if_only_the_excluded_members_are_different()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit"
            };

            var class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () =>
                    class1.Should().BeEquivalentTo(class2,
                        opts => opts.Excluding(_ => _.Field3).Excluding(_ => _.Property1));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow("the non-excluded fields have the same value");
        }

        [Fact]
        public void When_excluding_members_it_should_fail_if_any_non_excluded_members_are_different()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit"
            };

            var class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.Excluding(_ => _.Property1));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected member Field3*");
        }

        [Fact]
        public void When_all_shared_properties_match_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dto = new CustomerDto
            {
                Version = 2,
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            var customer = new Customer
            {
                Id = 1,
                Version = 2,
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => dto.Should().BeEquivalentTo(customer, options => options.ExcludingMissingMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_is_write_only_it_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithWriteOnlyProperty
            {
                WriteOnlyProperty = 123,
                SomeOtherProperty = "whatever"
            };

            var expected = new
            {
                SomeOtherProperty = "whatever"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_is_private_it_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Customer("MyPassword")
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            var other = new Customer("SomeOtherPassword")
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_field_is_private_it_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithAPrivateField(1234) { Value = 1 };

            var other = new ClassWithAPrivateField(54321) { Value = 1 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_is_protected_it_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Customer
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            subject.SetProtected("ActualValue");

            var expected = new Customer
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "John"
            };

            expected.SetProtected("ExpectedValue");

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
        public void When_a_property_is_hidden_in_a_derived_class_it_should_ignore_it()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new SubclassA<string> { Foo = "test" };
            var expectation = new SubclassB<string> { Foo = "test" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_is_an_indexer_it_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new ClassWithIndexer { Foo = "test" };
            var result = new ClassWithIndexer { Foo = "test" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => result.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        public class BaseWithFoo
        {
            public object Foo { get; set; }
        }

        public class SubclassA<T> : BaseWithFoo
        {
            public new T Foo
            {
                get { return (T)base.Foo; }

                set { base.Foo = value; }
            }
        }

        public class D
        {
            public object Foo { get; set; }
        }

        public class SubclassB<T> : D
        {
            public new T Foo
            {
                get { return (T)base.Foo; }

                set { base.Foo = value; }
            }
        }

        public class ClassWithIndexer
        {
            public object Foo { get; set; }

            public string this[int n]
            {
                get
                {
                    return
                        n.ToString(
                            CultureInfo.InvariantCulture);
                }
            }
        }

        [Fact]
        public void When_an_interface_hierarchy_is_used_it_should_include_all_inherited_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            ICar subject = new Car
            {
                VehicleId = 1,
                Wheels = 4
            };

            ICar expected = new Car
            {
                VehicleId = 99999,
                Wheels = 4
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action
                .Should().Throw<XunitException>()
                .WithMessage("Expected member VehicleId*99999*but*1*");
        }

        [Fact]
        public void When_a_reference_to_an_interface_is_provided_it_should_only_include_those_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IVehicle expected = new Car
            {
                VehicleId = 1,
                Wheels = 4
            };

            IVehicle subject = new Car
            {
                VehicleId = 1,
                Wheels = 99999
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_a_reference_to_an_explicit_interface_impl_is_provided_it_should_only_include_those_properties()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            IVehicle expected = new ExplicitCar
            {
                Wheels = 4
            };

            IVehicle subject = new ExplicitCar
            {
                Wheels = 99999
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_a_deeply_nested_property_with_a_value_mismatch_is_excluded_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1",
                    Level = new Level2
                    {
                        Text = "Mismatch"
                    }
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level1",
                    Level = new Level2Dto
                    {
                        Text = "Level2"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.Excluding(r => r.Level.Level.Text));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_with_a_value_mismatch_is_excluded_using_a_predicate_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1",
                    Level = new Level2
                    {
                        Text = "Mismatch"
                    }
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level1",
                    Level = new Level2Dto
                    {
                        Text = "Level2"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, config =>
                config.Excluding(ctx => ctx.SelectedMemberPath == "Level.Level.Text"));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_members_are_excluded_by_the_access_modifier_of_the_getter_using_a_predicate_they_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithAllAccessModifiersForMembers(
                "public",
                "protected",
                "internal",
                "protected-internal",
                "private");

            var expected = new ClassWithAllAccessModifiersForMembers(
                "public",
                "protected",
                "ignored-internal",
                "ignored-protected-internal",
                "private");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, config =>
                config.Excluding(ctx => ctx.WhichGetterHas(CSharpAccessModifier.Internal) ||
                                        ctx.WhichGetterHas(CSharpAccessModifier.ProtectedInternal)));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_members_are_excluded_by_the_access_modifier_of_the_setter_using_a_predicate_they_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithAllAccessModifiersForMembers(
                "public",
                "protected",
                "internal",
                "protected-internal",
                "private");

            var expected = new ClassWithAllAccessModifiersForMembers(
                "public",
                "protected",
                "ignored-internal",
                "ignored-protected-internal",
                "ignored-private");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, config =>
                config.Excluding(ctx => ctx.WhichSetterHas(CSharpAccessModifier.Internal) ||
                                        ctx.WhichSetterHas(CSharpAccessModifier.ProtectedInternal) ||
                                        ctx.WhichSetterHas(CSharpAccessModifier.Private)));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_the_expected_object_has_a_property_not_available_on_the_subject_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
            };

            var other = new
            {
                City = "Rijswijk"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expectation has member City that the other object does not have*");
        }

        [Fact]
        public void When_equally_named_properties_are_type_incompatible_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Type = "A"
            };

            var other = new
            {
                Type = 36
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("Expected member Type to be*36*, but found*\"A\"*");
        }

        [Fact]
        public void When_multiple_properties_mismatch_it_should_report_all_of_them()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Property1 = "A",
                Property2 = "B",
                SubType1 = new
                {
                    SubProperty1 = "C",
                    SubProperty2 = "D"
                }
            };

            var other = new
            {
                Property1 = "1",
                Property2 = "2",
                SubType1 = new
                {
                    SubProperty1 = "3",
                    SubProperty2 = "D"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("*member Property1 to be \"1\", but \"A\" differs near \"A\"*")
                .WithMessage("*member Property2 to be \"2\", but \"B\" differs near \"B\"*")
                .WithMessage("*member SubType1.SubProperty1 to be \"3\", but \"C\" differs near \"C\"*");
        }

        [Fact]
        public void When_excluding_properties_it_should_still_compare_fields()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            var class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum", Field3 = "color" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.ExcludingProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*color*dolor*");
        }

        [Fact]
        public void When_excluding_properties_via_non_array_indexers_it_should_exclude_the_specified_paths()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                List = new[] { new { Foo = 1, Bar = 2 }, new { Foo = 3, Bar = 4 } }.ToList(),
                Dictionary = new Dictionary<string, ClassWithOnlyAProperty>
                {
                    ["Foo"] = new ClassWithOnlyAProperty { Value = 1 },
                    ["Bar"] = new ClassWithOnlyAProperty { Value = 2 }
                }
            };

            var expected = new
            {
                List = new[] { new { Foo = 1, Bar = 2 }, new { Foo = 2, Bar = 4 } }.ToList(),
                Dictionary = new Dictionary<string, ClassWithOnlyAProperty>
                {
                    ["Foo"] = new ClassWithOnlyAProperty { Value = 1 },
                    ["Bar"] = new ClassWithOnlyAProperty { Value = 3 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () =>
                subject.Should().BeEquivalentTo(expected,
                    options => options
                        .Excluding(x => x.List[1].Foo)
                        .Excluding(x => x.Dictionary["Bar"].Value));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_excluding_properties_via_non_array_indexers_it_should_not_exclude_paths_with_different_indexes()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                List = new[] { new { Foo = 1, Bar = 2 }, new { Foo = 3, Bar = 4 } }.ToList(),
                Dictionary = new Dictionary<string, ClassWithOnlyAProperty>
                {
                    ["Foo"] = new ClassWithOnlyAProperty { Value = 1 },
                    ["Bar"] = new ClassWithOnlyAProperty { Value = 2 }
                }
            };

            var expected = new
            {
                List = new[] { new { Foo = 5, Bar = 2 }, new { Foo = 2, Bar = 4 } }.ToList(),
                Dictionary = new Dictionary<string, ClassWithOnlyAProperty>
                {
                    ["Foo"] = new ClassWithOnlyAProperty { Value = 6 },
                    ["Bar"] = new ClassWithOnlyAProperty { Value = 3 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () =>
                subject.Should().BeEquivalentTo(expected,
                    options => options
                        .Excluding(x => x.List[1].Foo)
                        .Excluding(x => x.Dictionary["Bar"].Value));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void
            When_configured_for_runtime_typing_and_properties_are_excluded_the_runtime_type_should_be_used_and_properties_should_be_ignored
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            object class2 = new ClassWithSomeFieldsAndProperties { Field1 = "Lorem", Field2 = "ipsum", Field3 = "dolor" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.ExcludingProperties().RespectingRuntimeTypes());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_using_IncludingAllDeclaredProperties_fields_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            var class2 = new ClassWithSomeFieldsAndProperties
            {
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.IncludingAllDeclaredProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_using_IncludingAllRuntimeProperties_the_runtime_type_should_be_used_and_fields_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            object class2 = new ClassWithSomeFieldsAndProperties
            {
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.IncludingAllRuntimeProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_both_field_and_properties_are_configured_for_inclusion_both_should_be_included()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Property1 = "sit"
            };

            var class2 = new ClassWithSomeFieldsAndProperties();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.IncludingFields().IncludingProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().Which.Message.Should().Contain("Field1").And.Contain("Property1");
        }

        [Fact]
        public void
            When_respecting_the_runtime_type_is_configured_the_runtime_type_should_be_used_and_both_properties_and_fields_included
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Property1 = "sit"
            };

            object class2 = new ClassWithSomeFieldsAndProperties();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.RespectingRuntimeTypes());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().Which.Message.Should().Contain("Field1").And.Contain("Property1");
        }

        #endregion

        #region Matching Rules

        [Fact]
        public void When_using_ExcludingMissingMembers_both_fields_and_properties_should_be_ignored()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var class1 = new ClassWithSomeFieldsAndProperties
            {
                Field1 = "Lorem",
                Field2 = "ipsum",
                Field3 = "dolor",
                Property1 = "sit",
                Property2 = "amet",
                Property3 = "consectetur"
            };

            var class2 = new
            {
                Field1 = "Lorem"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => class1.Should().BeEquivalentTo(class2, opts => opts.ExcludingMissingMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_shared_by_anonymous_types_doesnt_match_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36
            };

            var other = new
            {
                Age = 37
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other, options => options.ExcludingMissingMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>();
        }

        #endregion

        #region DateTime Properties

        [Fact]
        public void When_two_properties_are_datetime_and_both_are_nullable_and_both_are_null_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)null
                };

            var other =
                new
                {
                    Time = (DateTime?)null
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () =>
                subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_two_properties_are_datetime_and_both_are_nullable_and_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            var other =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_properties_are_datetime_and_both_are_nullable_and_expectation_is_null_it_should_throw_and_state_the_difference
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            var other =
                new
                {
                    Time = (DateTime?)null
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Time to be <null>, but found <2013-12-09 15:58:00>.*");
        }

        [Fact]
        public void
            When_two_properties_are_datetime_and_both_are_nullable_and_subject_is_null_it_should_throw_and_state_the_difference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)null
                };

            var other =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Time to be <2013-12-09 15:58:00>, but found <null>.*");
        }

        [Fact]
        public void When_two_properties_are_datetime_and_expectation_is_nullable_and_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = new DateTime(2013, 12, 9, 15, 58, 0)
                };

            var other =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_properties_are_datetime_and_expectation_is_nullable_and_expectation_is_null_it_should_throw_and_state_the_difference
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = new DateTime(2013, 12, 9, 15, 58, 0)
                };

            var other =
                new
                {
                    Time = (DateTime?)null
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Time to be <null>, but found <2013-12-09 15:58:00>.*");
        }

        [Fact]
        public void When_two_properties_are_datetime_and_subject_is_nullable_and_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)new DateTime(2013, 12, 9, 15, 58, 0)
                };

            var other =
                new
                {
                    Time = new DateTime(2013, 12, 9, 15, 58, 0)
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_properties_are_datetime_and_subject_is_nullable_and_subject_is_null_it_should_throw_and_state_the_difference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject =
                new
                {
                    Time = (DateTime?)null
                };

            var other =
                new
                {
                    Time = new DateTime(2013, 12, 9, 15, 58, 0)
                };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Time to be <2013-12-09 15:58:00>, but found <null>.*");
        }

        #endregion

        #region Assertion Rules

        [Fact]
        public void When_two_objects_have_the_same_property_values_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            var other = new
            {
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-----------------------------------------------------------------------------------------------------------
            subject.Should().BeEquivalentTo(other);
        }

        [Fact]
        public void When_two_objects_have_the_same_nullable_property_values_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36,
                Birthdate = (DateTime?)(new DateTime(1973, 9, 20)),
                Name = "Dennis"
            };

            var other = new
            {
                Age = 36,
                Birthdate = (DateTime?)new DateTime(1973, 9, 20),
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-----------------------------------------------------------------------------------------------------------
            subject.Should().BeEquivalentTo(other);
        }

        [Fact]
        public void When_two_objects_have_the_same_properties_but_a_different_value_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 36
            };

            var expectation = new
            {
                Age = 37
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, "because {0} are the same", "they");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Age to be 37 because they are the same, but found 36*");
        }

        [Fact]
        public void When_subject_has_a_valid_property_that_is_compared_with_a_null_property_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Name = "Dennis"
            };

            var other = new
            {
                Name = (string)null
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Name to be <null>, but found \"Dennis\"*");
        }

        [Fact]
        public void When_two_collection_properties_dont_match_it_should_throw_and_specify_the_difference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Values = new[]
                {
                    1, 2, 3
                }
            };

            var other = new
            {
                Values = new[]
                {
                    1, 4, 3
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Values[1] to be 4, but found 2*");
        }

        [Fact]
        public void When_two_string_properties_do_not_match_it_should_throw_and_state_the_difference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Name = "Dennes"
            };

            var other = new
            {
                Name = "Dennis"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other, options => options.Including(d => d.Name));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
                "Expected member Name to be \"Dennis\", but \"Dennes\" differs near \"es\" (index 4)*");
        }

        [Fact]
        public void When_two_properties_are_of_derived_types_but_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Type = new DerivedCustomerType("123")
            };

            var other = new
            {
                Type = new CustomerType("123")
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void
            When_two_properties_have_the_same_declared_type_but_different_runtime_types_and_are_equivilent_according_to_the_declared_type_it_should_succeed
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Type = (CustomerType)new DerivedCustomerType("123")
            };

            var other = new
            {
                Type = new CustomerType("123")
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        #endregion

        #region Member Conversion

        [Fact]
        public void When_two_objects_have_the_same_properties_with_convertable_values_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = "37",
                Birthdate = "1973-09-20"
            };

            var other = new
            {
                Age = 37,
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(other, o => o.WithAutoConversion());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_string_is_declared_equivalent_to_an_int_representing_the_numerals_it_should_pass()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new
            {
                Property = "32"
            };

            var expectation = new
            {
                Property = 32
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation,
                options => options.WithAutoConversion());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_an_int_is_compared_equivalent_to_a_string_representing_the_number_it_should_pass()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new { Property = 32 };
            var expectation = new { Property = "32" };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, options => options.WithAutoConversion());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_only_a_single_property_is_and_can_be_converted_but_the_other_one_doesnt_match_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 32,
                Birthdate = "1973-09-20"
            };

            var expectation = new
            {
                Age = "32",
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation,
                options => options.WithAutoConversionFor(x => x.SelectedMemberPath.Contains("Birthdate")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("*Age*String*Int32*");
        }

        [Fact]
        public void When_only_a_single_property_is_converted_and_the_other_matches_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 32,
                Birthdate = "1973-09-20"
            };

            var expectation = new
            {
                Age = 32,
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, options => options
                .WithAutoConversionFor(x => x.SelectedMemberPath.Contains("Birthdate")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_specific_mismatching_property_is_excluded_from_conversion_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Age = 32,
                Birthdate = "1973-09-20"
            };

            var expectation = new
            {
                Age = 32,
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, options => options
                .WithAutoConversion()
                .WithoutAutoConversionFor(x => x.SelectedMemberPath.Contains("Birthdate")));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage("Expected*<1973-09-20>*\"1973-09-20\"*");
        }

        [Fact]
        public void When_declaring_equivalent_a_convertable_object_that_is_equivalent_once_conveterted_it_should_pass()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string str = "This is a test";
            CustomConvertible obj = new CustomConvertible(str);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => obj.Should().BeEquivalentTo(str, options => options.WithAutoConversion());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        #endregion

        #region Nested Properties

        [Fact]
        public void When_all_the_properties_of_the_nested_objects_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1",
                    Level = new Level2
                    {
                        Text = "Level2"
                    }
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level1",
                    Level = new Level2Dto
                    {
                        Text = "Level2"
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
            act.Should().NotThrow();
        }

        [Fact]
        public void When_the_expectation_contains_a_nested_null_it_should_properly_report_the_difference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1",
                    Level = new Level2()
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level1",
                    Level = null
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
                .WithMessage("*Expected*Level.Level to be <null>, but found*Level2*");
        }

        [Fact]
        public void When_not_all_the_properties_of_the_nested_objects_are_equal_but_nested_objects_are_excluded_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Property = new ClassWithValueSemanticsOnSingleProperty
                {
                    Key = "123",
                    NestedProperty = "Should be ignored"
                }
            };

            var expected = new
            {
                Property = new ClassWithValueSemanticsOnSingleProperty
                {
                    Key = "123",
                    NestedProperty = "Should be ignored as well"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected,
                options => options.ExcludingNestedObjects());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_nested_objects_should_be_excluded_it_should_do_a_simple_equality_check_instead()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var item = new Item
            {
                Child = new Item()
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => item.Should().BeEquivalentTo(new Item(), options => options.ExcludingNestedObjects());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*Item*null*");
        }

        public class Item
        {
            public Item Child { get; set; }
        }

        [Fact]
        public void When_not_all_the_properties_of_the_nested_objects_are_equal_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1"
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () =>
                subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage(
                    "Expected member Level.Text to be \"Level2\", but \"Level1\" differs near \"1\" (index 5)*");
        }

        [Fact]
        public void When_the_actual_nested_object_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level2"
                }
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = null
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("Expected member Level to be <null>*, but found*Level1*Level2*");
        }

        public class StringSubContainer
        {
            public string SubValue { get; set; }
        }

        public class StringContainer
        {
            public StringContainer(string mainValue, string subValue = null)
            {
                MainValue = mainValue;
                SubValues = new[]
                {
                    new StringSubContainer
                    {
                        SubValue = subValue
                    }
                };
            }

            public string MainValue { get; set; }
            public IList<StringSubContainer> SubValues { get; set; }
        }

        public class MyClass2
        {
            public StringContainer One { get; set; }
            public StringContainer Two { get; set; }
        }

        [Fact]
        public void When_deeply_nested_strings_dont_match_it_should_properly_report_the_mismatches()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var expected = new[]
            {
                new MyClass2
                {
                    One = new StringContainer("EXPECTED", "EXPECTED"),
                    Two = new StringContainer("CORRECT")
                },
                new MyClass2()
            };

            var actual = new[]
            {
                new MyClass2
                {
                    One = new StringContainer("INCORRECT", "INCORRECT"),
                    Two = new StringContainer("CORRECT")
                },
                new MyClass2()
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("*EXPECTED*INCORRECT*EXPECTED*INCORRECT*");
        }

        [Fact]
        public void When_the_nested_object_property_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new Root
            {
                Text = "Root",
                Level = null
            };

            var expected = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () =>
                subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("Expected member Level to be*Level1Dto*Level2*, but found <null>*");
        }

        [Fact]
        public void When_not_all_the_properties_of_the_nested_object_exist_on_the_expected_object_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Level = new
                {
                    Text = "Level1",
                }
            };

            var expected = new
            {
                Level = new
                {
                    Text = "Level1",
                    OtherProperty = "OtherProperty"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("Expectation has member Level.OtherProperty that the other object does not have*");
        }

        [Fact]
        public void When_all_the_shared_properties_of_the_nested_objects_are_equal_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Level = new
                {
                    Text = "Level1",
                    Property = "Property"
                }
            };

            var expected = new
            {
                Level = new
                {
                    Text = "Level1",
                    OtherProperty = "OtherProperty"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expected, options => options.ExcludingMissingMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_deeply_nested_properties_do_not_have_all_equal_values_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var root = new Root
            {
                Text = "Root",
                Level = new Level1
                {
                    Text = "Level1",
                    Level = new Level2
                    {
                        Text = "Level2"
                    }
                }
            };

            var rootDto = new RootDto
            {
                Text = "Root",
                Level = new Level1Dto
                {
                    Text = "Level1",
                    Level = new Level2Dto
                    {
                        Text = "A wrong text value"
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => root.Should().BeEquivalentTo(rootDto);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage(
                    "Expected member Level.Level.Text to be *A wrong text value*but*\"Level2\"*length*");
        }

        [Fact]
        public void When_two_objects_have_the_same_nested_objects_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var c1 = new ClassOne();
            var c2 = new ClassOne();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => c1.Should().BeEquivalentTo(c2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_property_of_a_nested_object_doesnt_match_it_should_clearly_indicate_the_path()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var c1 = new ClassOne();
            var c2 = new ClassOne();
            c2.RefOne.ValTwo = 2;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => c1.Should().BeEquivalentTo(c2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected member RefOne.ValTwo to be 2, but found 3*");
        }

        #endregion

        #region Cyclic References

        [Fact]
        public void When_validating_nested_properties_that_have_cyclic_references_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var cyclicRoot = new CyclicRoot
            {
                Text = "Root"
            };

            cyclicRoot.Level = new CyclicLevel1
            {
                Text = "Level1",
                Root = cyclicRoot
            };

            var cyclicRootDto = new CyclicRootDto
            {
                Text = "Root"
            };

            cyclicRootDto.Level = new CyclicLevel1Dto
            {
                Text = "Level1",
                Root = cyclicRootDto
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => cyclicRoot.Should().BeEquivalentTo(cyclicRootDto);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage("Expected member Level.Root to be*but it contains a cyclic reference*");
        }

        [Fact]
        public void When_validating_nested_properties_and_ignoring_cyclic_references_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var cyclicRoot = new CyclicRoot
            {
                Text = "Root"
            };
            cyclicRoot.Level = new CyclicLevel1
            {
                Text = "Level1",
                Root = cyclicRoot
            };

            var cyclicRootDto = new CyclicRootDto
            {
                Text = "Root"
            };
            cyclicRootDto.Level = new CyclicLevel1Dto
            {
                Text = "Level1",
                Root = cyclicRootDto
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => cyclicRoot.Should().BeEquivalentTo(cyclicRootDto, options => options.IgnoringCyclicReferences());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_two_cyclic_graphs_are_equivalent_when_ignoring_cycle_references_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new Parent();
            actual.Child1 = new Child(actual, 1);
            actual.Child2 = new Child(actual);

            var expected = new Parent();
            expected.Child1 = new Child(expected);
            expected.Child2 = new Child(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected, x => x
                .Excluding(y => y.Child1)
                .IgnoringCyclicReferences());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        public class Parent
        {
            public Child Child1 { get; set; }
            public Child Child2 { get; set; }
        }

        public class Child
        {
            public Child(Parent parent, int stuff = 0)
            {
                Parent = parent;
                Stuff = stuff;
            }

            public Parent Parent { get; set; }
            public int Stuff { get; set; }
        }

        [Fact]
        public void When_validating_nested_properties_that_are_null_it_should_not_throw_on_cyclic_references()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new CyclicRoot
            {
                Text = null
            };

            actual.Level = new CyclicLevel1
            {
                Text = null,
                Root = null
            };

            var expectation = new CyclicRootDto
            {
                Text = null
            };

            expectation.Level = new CyclicLevel1Dto
            {
                Text = null,
                Root = null
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_the_graph_contains_the_same_value_object_it_should_not_be_treated_as_a_cyclic_reference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new CyclicRootWithValueObject
            {
                Object = new ValueObject("MyValue")
            };

            actual.Level = new CyclicLevelWithValueObject
            {
                Object = new ValueObject("MyValue"),
                Root = null
            };

            var expectation = new CyclicRootWithValueObject
            {
                Object = new ValueObject("MyValue")
            };

            expectation.Level = new CyclicLevelWithValueObject
            {
                Object = new ValueObject("MyValue"),
                Root = null
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

#if NET45 || NET47 || NETCOREAPP2_0
        [Fact]
        public void When_asserting_types_with_infinite_oject_graphs_are_equivilent_it_should_not_overflow_the_stack()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var recursiveClass1 = new ClassWithInfinitelyRecursiveProperty();
            var recursiveClass2 = new ClassWithInfinitelyRecursiveProperty();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => recursiveClass1.Should().BeEquivalentTo(recursiveClass2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow<StackOverflowException>();
        }

#endif
        [Fact]
        public void
            When_asserting_equivilence_on_objects_needing_high_recursion_depth_and_disabling_recursion_depth_limit_it_should_recurse_to_completion
            ()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var recursiveClass1 = new ClassWithFiniteRecursiveProperty(15);
            var recursiveClass2 = new ClassWithFiniteRecursiveProperty(15);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act =
                () => recursiveClass1.Should().BeEquivalentTo(recursiveClass2,
                    options => options.AllowingInfiniteRecursion());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        internal class LogbookEntryProjection
        {
            public virtual LogbookCode Logbook { get; set; }
            public virtual ICollection<LogbookRelation> LogbookRelations { get; set; }
        }

        internal class LogbookRelation
        {
            public virtual LogbookCode Logbook { get; set; }
        }

        internal class LogbookCode
        {
            public LogbookCode(string key)
            {
                Key = key;
            }

            public string Key { get; protected set; }
        }

        [Fact]
        public void When_the_root_object_is_referenced_from_a_nested_object_it_should_treat_it_as_a_cyclic_reference()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var company1 = new MyCompany { Name = "Company" };
            var user1 = new MyUser { Name = "User", Company = company1 };
            var logo1 = new MyCompanyLogo { Url = "blank", Company = company1, CreatedBy = user1 };
            company1.Logo = logo1;

            var company2 = new MyCompany { Name = "Company" };
            var user2 = new MyUser { Name = "User", Company = company2 };
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

        #endregion

        #region Tuples

        [Fact]
        public void When_a_nested_member_is_a_tuple_it_should_compare_its_property_for_equivalence()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new
            {
                Tuple = (new[] { "string1" }, new[] { "string2" })
            };

            var expected = new
            {
                Tuple = (new[] { "string1" }, new[] { "string2" })
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        #endregion

        #region Enums

        [Fact]
        public void When_asserting_the_same_enum_member_is_equivalent_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange / Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => EnumOne.One.Should().BeEquivalentTo(EnumOne.One);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_the_actual_enum_value_is_null_it_should_report_that_properly()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                NullableEnum = (DayOfWeek?)null
            };

            var expectation = new
            {
                NullableEnum = DayOfWeek.Friday
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*5*null*");
        }

        [Fact]
        public void When_the_actual_enum_name_is_null_it_should_report_that_properly()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new
            {
                NullableEnum = (DayOfWeek?)null
            };

            var expectation = new
            {
                NullableEnum = DayOfWeek.Friday
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, o => o.ComparingEnumsByValue());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
                .WithMessage("Expected*5*null*");
        }

        [Fact]
        public void When_asserting_different_enum_members_are_equivalent_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange / Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => EnumOne.One.Should().BeEquivalentTo(EnumOne.Two);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected value to be*3*, but found*0*");
#else
                .WithMessage("Expected EnumOne.One to be*3*, but found*0*");
#endif
        }

        [Fact]
        public void When_asserting_members_from_different_enum_types_are_equivalent_it_should_compare_by_value_by_default()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithEnumOne();
            var expectation = new ClassWithEnumTwo();

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
        public void When_asserting_members_from_different_enum_types_are_equivalent_by_value_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithEnumOne { Enum = EnumOne.One };
            var expectation = new ClassWithEnumThree { Enum = EnumeThree.ValueZero };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, config => config.ComparingEnumsByValue());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_members_from_different_enum_types_are_equivalent_by_stringvalue_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithEnumOne { Enum = EnumOne.Two };
            var expectation = new ClassWithEnumThree { Enum = EnumeThree.Two };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, config => config.ComparingEnumsByName());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_members_from_different_char_enum_types_are_equivalent_by_value_it_should_succeed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new ClassWithEnumCharOne { Enum = EnumCharOne.B };
            var expectation = new ClassWithEnumCharTwo { Enum = EnumCharTwo.ValueB };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeEquivalentTo(expectation, config => config.ComparingEnumsByValue());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_enums_typed_as_object_are_equivalent_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object e1 = EnumOne.One;
            object e2 = EnumOne.One;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => e1.Should().BeEquivalentTo(e2);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        [Fact]
        public void When_a_numeric_member_is_compared_with_an_enum_it_should_respect_the_enum_options()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var actual = new
            {
                Property = 1
            };

            var expected = new
            {
                Property = TestEnum.First
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expected, options => options.ComparingEnumsByValue());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        public enum TestEnum
        {
            First = 1
        }

        #endregion

        #region Memberless Objects

        [Fact]
        public void When_asserting_instances_of_an_anonymous_type_having_no_members_are_equivalent_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange / Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => new { }.Should().BeEquivalentTo(new { });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void When_asserting_instances_of_a_class_having_no_members_are_equivalent_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange / Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => new ClassWithNoMembers().Should().BeEquivalentTo(new ClassWithNoMembers());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void When_asserting_instances_of_Object_are_equivalent_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange / Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => new object().Should().BeEquivalentTo(new object());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void When_an_type_only_exposes_fields_but_fields_are_ignored_in_the_equivalence_comparision_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var object1 = new ClassWithOnlyAField { Value = 1 };
            var object2 = new ClassWithOnlyAField { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => object1.Should().BeEquivalentTo(object2, opts => opts.IncludingAllDeclaredProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>("the objects have no members to compare.");
        }

        [Fact]
        public void When_an_type_only_exposes_properties_but_properties_are_ignored_in_the_equivalence_comparision_it_should_fail()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var object1 = new ClassWithOnlyAProperty { Value = 1 };
            var object2 = new ClassWithOnlyAProperty { Value = 101 };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => object1.Should().BeEquivalentTo(object2, opts => opts.ExcludingProperties());

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<InvalidOperationException>("the objects have no members to compare.");
        }

        [Fact]
        public void When_asserting_instances_of_arrays_of_types_in_System_are_equivalent_it_should_respect_the_runtime_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object actual = new int[0];
            object expectation = new int[0];

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => actual.Should().BeEquivalentTo(expectation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().NotThrow();
        }

        #endregion
    }

    #region Test Classes

    public class ClassOne
    {
        public ClassTwo RefOne { get; set; } = new ClassTwo();

        public int ValOne { get; set; } = 1;
    }

    public class ClassTwo
    {
        public int ValTwo { get; set; } = 3;
    }

    public class ClassWithWriteOnlyProperty
    {
        private int writeOnlyPropertyValue;

        public int WriteOnlyProperty
        {
            set { writeOnlyPropertyValue = value; }
        }

        public string SomeOtherProperty { get; set; }
    }

    internal enum EnumOne
    {
        One = 0,
        Two = 3
    }

    internal enum EnumCharOne
    {
        A = 'A',
        B = 'B'
    }

    internal enum EnumCharTwo
    {
        A = 'Z',
        ValueB = 'B'
    }

    internal enum EnumTwo
    {
        One = 0,
        Two = 3
    }

    internal enum EnumeThree
    {
        ValueZero = 0,
        Two = 3
    }

    internal class ClassWithEnumCharOne
    {
        public EnumCharOne Enum { get; set; }
    }

    internal class ClassWithEnumCharTwo
    {
        public EnumCharTwo Enum { get; set; }
    }

    internal class ClassWithEnumOne
    {
        public EnumOne Enum { get; set; }
    }

    internal class ClassWithEnumTwo
    {
        public EnumTwo Enum { get; set; }
    }

    internal class ClassWithEnumThree
    {
        public EnumeThree Enum { get; set; }
    }

    internal class ClassWithNoMembers
    {
    }

    internal class ClassWithOnlyAField
    {
        public int Value;
    }

    internal class ClassWithAPrivateField : ClassWithOnlyAField
    {
        private int value;

        public ClassWithAPrivateField(int value)
        {
            this.value = value;
        }
    }

    internal class ClassWithOnlyAProperty
    {
        public int Value { get; set; }
    }

    internal struct StructWithNoMembers
    {
    }

    internal class ClassWithInfinitelyRecursiveProperty
    {
        public ClassWithInfinitelyRecursiveProperty Self
        {
            get { return new ClassWithInfinitelyRecursiveProperty(); }
        }
    }

    internal class ClassWithFiniteRecursiveProperty
    {
        private readonly int depth;

        public ClassWithFiniteRecursiveProperty(int recursiveDepth)
        {
            depth = recursiveDepth;
        }

        public ClassWithFiniteRecursiveProperty Self
        {
            get
            {
                return depth > 0
                    ? new ClassWithFiniteRecursiveProperty(depth - 1)
                    : null;
            }
        }
    }

    internal class ClassWithSomeFieldsAndProperties
    {
        public string Field1;

        public string Field2;

        public string Field3;

        public string Property1 { get; set; }

        public string Property2 { get; set; }

        public string Property3 { get; set; }
    }

    internal class ClassWithCctor
    {
        static ClassWithCctor() { }
    }

    internal class ClassWithCctorAndNonDefaultConstructor
    {
        static ClassWithCctorAndNonDefaultConstructor() { }
        public ClassWithCctorAndNonDefaultConstructor(int i) { }
    }

    internal class MyCompanyLogo
    {
        public string Url { get; set; }
        public MyCompany Company { get; set; }
        public MyUser CreatedBy { get; set; }
    }

    internal class MyUser
    {
        public string Name { get; set; }
        public MyCompany Company { get; set; }
    }

    internal class MyCompany
    {
        public string Name { get; set; }
        public MyCompanyLogo Logo { get; set; }
        public List<MyUser> Users { get; set; }
    }

    public class Customer : Entity
    {
        private string PrivateProperty { get; set; }

        protected string ProtectedProperty { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate { get; set; }
        public long Id { get; set; }

        public void SetProtected(string value)
        {
            ProtectedProperty = value;
        }

        public Customer()
        {
        }

        public Customer(string privateProperty)
        {
            PrivateProperty = privateProperty;
        }
    }

    public class Entity
    {
        internal long Version { get; set; }
    }

    public class CustomerDto
    {
        public long Version { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate { get; set; }
    }

    public class CustomerType
    {
        public CustomerType(string code)
        {
            Code = code;
        }

        public string Code { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as CustomerType;
            return (other != null) && (Code.Equals(other.Code));
        }

        public override int GetHashCode()
        {
            return Code?.GetHashCode() ?? 0;
        }

        public static bool operator ==(CustomerType a, CustomerType b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Code.Equals(b.Code);
        }

        public static bool operator !=(CustomerType a, CustomerType b)
        {
            return !(a == b);
        }
    }

    public class DerivedCustomerType : CustomerType
    {
        public string DerivedInfo { get; set; }

        public DerivedCustomerType(string code)
            : base(code)
        {
        }
    }

    public class CustomConvertible : IConvertible
    {
        private readonly string convertedStringValue;

        public CustomConvertible(string convertedStringValue)
        {
            this.convertedStringValue = convertedStringValue;
        }

        public TypeCode GetTypeCode()
        {
            throw new InvalidCastException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public string ToString(IFormatProvider provider)
        {
            return convertedStringValue;
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException();
        }
    }

    #endregion

    #region Nested classes for comparison

    public class ClassWithAllAccessModifiersForMembers
    {
        public string PublicField;
        protected string ProtectedField;
        internal string InternalField;
        protected internal string ProtectedInternalField;
        private string PrivateField;

        public string PublicProperty { get; set; }
        public string ReadOnlyProperty { get; private set; }
        public string WriteOnlyProperty { private get; set; }
        protected string ProtectedProperty { get; set; }
        internal string InternalProperty { get; set; }
        protected internal string ProtectedInternalProperty { get; set; }
        private string PrivateProperty { get; set; }

        public ClassWithAllAccessModifiersForMembers(string publicValue, string protectedValue, string internalValue,
            string protectedInternalValue, string privateValue)
        {
            PublicField = publicValue;
            ProtectedField = protectedValue;
            InternalField = internalValue;
            ProtectedInternalField = protectedInternalValue;
            PrivateField = privateValue;

            PublicProperty = publicValue;
            ReadOnlyProperty = privateValue;
            WriteOnlyProperty = privateValue;
            ProtectedProperty = protectedValue;
            InternalProperty = internalValue;
            ProtectedInternalProperty = protectedInternalValue;
            PrivateProperty = privateValue;
        }
    }

    public class ClassWithValueSemanticsOnSingleProperty
    {
        public string Key { get; set; }
        public string NestedProperty { get; set; }

        protected bool Equals(ClassWithValueSemanticsOnSingleProperty other)
        {
            return string.Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ClassWithValueSemanticsOnSingleProperty)obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }

    public class Root
    {
        public string Text { get; set; }
        public Level1 Level { get; set; }
    }

    public class Level1
    {
        public string Text { get; set; }
        public Level2 Level { get; set; }
    }

    public class Level2
    {
        public string Text { get; set; }
    }

    public class RootDto
    {
        public string Text { get; set; }
        public Level1Dto Level { get; set; }
    }

    public class Level1Dto
    {
        public string Text { get; set; }
        public Level2Dto Level { get; set; }
    }

    public class Level2Dto
    {
        public string Text { get; set; }
    }

    public class CyclicRoot
    {
        public string Text { get; set; }
        public CyclicLevel1 Level { get; set; }
    }

    public class CyclicRootWithValueObject
    {
        public ValueObject Object { get; set; }
        public CyclicLevelWithValueObject Level { get; set; }
    }

    public class ValueObject
    {
        public ValueObject(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override bool Equals(object obj)
        {
            return ((ValueObject)obj).Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class CyclicLevel1
    {
        public string Text { get; set; }
        public CyclicRoot Root { get; set; }
    }

    public class CyclicLevelWithValueObject
    {
        public ValueObject Object { get; set; }
        public CyclicRootWithValueObject Root { get; set; }
    }

    public class CyclicRootDto
    {
        public string Text { get; set; }
        public CyclicLevel1Dto Level { get; set; }
    }

    public class CyclicLevel1Dto
    {
        public string Text { get; set; }
        public CyclicRootDto Root { get; set; }
    }

    #endregion

    #region Interfaces for verifying inheritance of properties

    public class Car : Vehicle, ICar
    {
        public int Wheels { get; set; }
    }

    public class ExplicitCar : ExplicitVehicle, ICar
    {
        public int Wheels { get; set; }
    }

    public class Vehicle : IVehicle
    {
        public int VehicleId { get; set; }
    }

    public class ExplicitVehicle : IVehicle
    {
        int IVehicle.VehicleId { get; set; }
    }

    public interface ICar : IVehicle
    {
        int Wheels { get; set; }
    }

    public interface IVehicle
    {
        int VehicleId { get; set; }
    }

    #endregion
}
