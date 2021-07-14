﻿using System;

using FluentAssertions.Common;
using FluentAssertions.Extensions;
using FluentAssertions.Formatting;
using Xunit;
using Xunit.Sdk;

namespace FluentAssertions.Specs
{
    public class ReferenceTypeAssertionsSpecs
    {
        [Fact]
        public void When_the_same_objects_are_expected_to_be_the_same_it_should_not_fail()
        {
            //-------------------------------------------------------------------------------------------------------------------
            // Arrange
            //-------------------------------------------------------------------------------------------------------------------
            var subject = new ClassWithCustomEqualMethod(1);
            var referenceToSubject = subject;

            //-------------------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-------------------------------------------------------------------------------------------------------------------
            subject.Should().BeSameAs(referenceToSubject);
        }

        [Fact]
        public void When_two_different_objects_are_expected_to_be_the_same_it_should_fail_with_a_clear_explanation()
        {
            //-------------------------------------------------------------------------------------------------------------------
            // Arrange
            //-------------------------------------------------------------------------------------------------------------------
            var subject = new
            {
                Name = "John Doe"
            };

            var otherObject = new
            {
                UserName = "JohnDoe"
            };

            //-------------------------------------------------------------------------------------------------------------------
            // Act
            //-------------------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().BeSameAs(otherObject, "they are {0} {1}", "the", "same");

            //-------------------------------------------------------------------------------------------------------------------
            // Assert
            //-------------------------------------------------------------------------------------------------------------------
            act
                .Should().Throw<XunitException>()
                .WithMessage(
#if NETCOREAPP1_1
                    "Expected object to refer to*{ UserName = JohnDoe } because " +
                    "they are the same, but found*{ Name = John Doe }.");
#else
                    "Expected subject to refer to*{ UserName = JohnDoe } because " +
                    "they are the same, but found*{ Name = John Doe }.");
#endif
        }

        [Fact]
        public void When_two_different_objects_are_expected_not_to_be_the_same_it_should_not_fail()
        {
            //-------------------------------------------------------------------------------------------------------------------
            // Arrange
            //-------------------------------------------------------------------------------------------------------------------
            var someObject = new ClassWithCustomEqualMethod(1);
            var notSameObject = new ClassWithCustomEqualMethod(1);

            //-------------------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-------------------------------------------------------------------------------------------------------------------
            someObject.Should().NotBeSameAs(notSameObject);
        }

        [Fact]
        public void When_two_equal_object_are_expected_not_to_be_the_same_it_should_fail()
        {
            //-------------------------------------------------------------------------------------------------------------------
            // Arrange
            //-------------------------------------------------------------------------------------------------------------------
            var someObject = new ClassWithCustomEqualMethod(1);
            ClassWithCustomEqualMethod sameObject = someObject;

            //-------------------------------------------------------------------------------------------------------------------
            // Act
            //-------------------------------------------------------------------------------------------------------------------
            Action act = () => someObject.Should().NotBeSameAs(sameObject, "they are {0} {1}", "the", "same");

            //-------------------------------------------------------------------------------------------------------------------
            // Assert
            //-------------------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Did not expect object to refer to*ClassWithCustomEqualMethod(1) because they are the same.");
#else
                .WithMessage("Did not expect someObject to refer to*ClassWithCustomEqualMethod(1) because they are the same.");
#endif
        }

        [Fact]
        public void When_object_is_of_the_expected_type_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = "blah";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().BeOfType(typeof(string));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_object_is_an_open_generic_of_the_expected_type_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var openGeneric = new System.Collections.Generic.List<string>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => openGeneric.Should().BeOfType(typeof(System.Collections.Generic.List<>));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_object_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().BeOfType(typeof(string));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected type to be System.String, but found <null>.");
#else
                .WithMessage("Expected aString to be System.String, but found <null>.");
#endif
        }

        [Fact]
        public void When_object_is_not_of_the_expected_type_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = "blah";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().BeOfType(typeof(Int32));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected type to be System.Int32, but found System.String.");
        }

        [Fact]
        public void When_object_is_of_the_unexpected_type_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = "blah";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().NotBeOfType(typeof(string));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected type not to be [" + typeof(string).AssemblyQualifiedName + "], but it is.");
        }

        [Fact]
        public void When_object_is_of_the_unexpected_generic_type_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = "blah";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().NotBeOfType<string>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
                .WithMessage("Expected type not to be [" + typeof(string).AssemblyQualifiedName + "], but it is.");
        }

        [Fact]
        public void When_object_is_not_of_the_expected_type_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = "blah";

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().NotBeOfType(typeof(Int32));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().NotThrow();
        }

        [Fact]
        public void When_asserting_object_is_not_of_type_and_it_is_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            string aString = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action action = () => aString.Should().NotBeOfType(typeof(string));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            action.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected type not to be System.String, but found <null>.");
#else
                .WithMessage("Expected aString not to be System.String, but found <null>.");
#endif
        }

        [Fact]
        public void When_object_satisfies_predicate_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var someObject = new object();

            //-----------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-----------------------------------------------------------------------------------------------------------
            someObject.Should().Match(o => (o != null));
        }

        [Fact]
        public void When_typed_object_satisfies_predicate_it_should_not_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var someObject = new SomeDto
            {
                Name = "Dennis Doomen",
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act / Assert
            //-----------------------------------------------------------------------------------------------------------
            someObject.Should().Match<SomeDto>(o => o.Age > 0);
        }

        [Fact]
        public void When_object_does_not_match_the_predicate_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var someObject = new object();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => someObject.Should().Match(o => o == null, "it is not initialized yet");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>()
#if NETCOREAPP1_1
                .WithMessage("Expected object to match (o == null) because it is not initialized yet*");
#else
                .WithMessage("Expected someObject to match (o == null) because it is not initialized yet*");
#endif
        }

        [Fact]
        public void When_a_typed_object_does_not_match_the_predicate_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var someObject = new SomeDto
            {
                Name = "Dennis Doomen",
                Age = 36,
                Birthdate = new DateTime(1973, 9, 20)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => someObject.Should().Match((SomeDto d) => d.Name.Length == 0, "it is not initialized yet");

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
#if NETCOREAPP1_1
                "Expected object to match (d.Name.Length == 0) because it is not initialized yet*");
#else
                "Expected someObject to match (d.Name.Length == 0) because it is not initialized yet*");
#endif
        }

        [Fact]
        public void When_object_is_matched_against_a_null_it_should_throw()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var someObject = new object();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => someObject.Should().Match(null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<ArgumentNullException>().WithMessage(
                "Cannot match an object against a <null> predicate.*");
        }

        #region Structure Reporting

        [Fact]
        public void When_an_assertion_on_two_objects_fails_it_should_show_the_properties_of_the_class()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new SomeDto
            {
                Age = 37,
                Birthdate = 20.September(1973),
                Name = "Dennis"
            };

            var other = new SomeDto
            {
                Age = 2,
                Birthdate = 22.February(2009),
                Name = "Teddie"
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().Be(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
#if NETCOREAPP1_1
                "Expected object to be*FluentAssertions.Specs.SomeDto*{*Age = 2*Birthdate = <2009-02-22>*" +
                "   Name = \"Teddie\"*}, but found*FluentAssertions.Specs.SomeDto*{*Age = 37*" +
                "   Birthdate = <1973-09-20>*Name = \"Dennis\"*}.");
#else
                "Expected subject to be*FluentAssertions.Specs.SomeDto*{*Age = 2*Birthdate = <2009-02-22>*" +
                    "   Name = \"Teddie\"*}, but found*FluentAssertions.Specs.SomeDto*{*Age = 37*" +
                        "   Birthdate = <1973-09-20>*Name = \"Dennis\"*}.");
#endif
        }

        [Fact]
        public void When_an_assertion_on_two_objects_fails_and_they_implement_tostring_it_should_show_their_string_representation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            object subject = 3;
            object other = 4;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().Be(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(
#if NETCOREAPP1_1
                "Expected object to be 4, but found 3.");
#else
                "Expected subject to be 4, but found 3.");
#endif
        }

        [Fact]
        public void When_an_assertion_on_two_unknown_objects_fails_it_should_report_the_type_name()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var subject = new object();
            var other = new object();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Action act = () => subject.Should().Be(other);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            act.Should().Throw<XunitException>().WithMessage(string.Format(
#if NETCOREAPP1_1
                "Expected object to be System.Object (HashCode={0}), but found System.Object (HashCode={1}).",
#else
                "Expected subject to be System.Object (HashCode={0}), but found System.Object (HashCode={1}).",
#endif
                other.GetHashCode(), subject.GetHashCode()));
        }

        #endregion
    }

    public class SomeDto
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public DateTime Birthdate { get; set; }
    }

    internal class ClassWithCustomEqualMethod
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "T:System.Object" /> class.
        /// </summary>
        public ClassWithCustomEqualMethod(int key)
        {
            Key = key;
        }

        private int Key { get; set; }

        private bool Equals(ClassWithCustomEqualMethod other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.Key == Key;
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
            if (obj.GetType() != typeof(ClassWithCustomEqualMethod))
            {
                return false;
            }
            return Equals((ClassWithCustomEqualMethod)obj);
        }

        public override int GetHashCode()
        {
            return Key;
        }

        public static bool operator ==(ClassWithCustomEqualMethod left, ClassWithCustomEqualMethod right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClassWithCustomEqualMethod left, ClassWithCustomEqualMethod right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        ///   Returns a <see cref = "T:System.String" /> that represents the current <see cref = "T:System.Object" />.
        /// </summary>
        /// <returns>
        ///   A <see cref = "T:System.String" /> that represents the current <see cref = "T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("ClassWithCustomEqualMethod({0})", Key);
        }
    }
}
