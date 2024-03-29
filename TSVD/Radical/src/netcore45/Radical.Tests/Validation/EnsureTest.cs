﻿using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Topics.Radical.Validation;

namespace Radical.Tests.Validation
{
	[TestClass()]
	public class ValidatorTest
	{
		[TestMethod]
        [Ignore]
		public void ensure_getFullErrorMessage_should_contain_all_relevant_information()
		{
			var obj = Ensure.That( "" );
			String actual = obj.GetFullErrorMessage( "validator specific message" );

			var containsClassName = actual.Contains( typeof( ValidatorTest ).Name );
			var containsMethodName = actual.Contains( typeof( ValidatorTest ).Name );

			Assert.IsTrue( containsClassName );
			Assert.IsTrue( containsMethodName );
		}

		[TestMethod]
		public void ensure_getFullErrorMessage_should_contain_custom_message()
		{
			var expected = "custom message";

			var obj = Ensure.That( "" ).WithMessage( expected );

			String actual = obj.GetFullErrorMessage( "validator specific message" );

			Assert.IsTrue( actual.Contains( expected ) );
		}

		[TestMethod]
		public void ensure_getFullErrorMessage_using_more_then_one_custom_message_should_contain_custom_message()
		{
			var expected1 = "custom message 1";
			var expected2 = "custom message 2";

			var obj = Ensure.That( "" )
				.WithMessage( expected1 );

			var actual1 = obj.GetFullErrorMessage( "validator specific message" );

			obj.WithMessage( expected2 );

			var actual2 = obj.GetFullErrorMessage( "validator specific message" );

			Assert.IsTrue( actual1.Contains( expected1 ) );
			Assert.IsTrue( actual2.Contains( expected2 ) );
		}

		[TestMethod]
		public void ensure_getFullErrorMessage_should_not_be_null()
		{
			var obj = Ensure.That( "" );
			String actual = obj.GetFullErrorMessage( "validator specific message" );

			Assert.IsNotNull( actual );
		}

		[TestMethod]
		public void ensure_getFullErrorMessage_should_contain_validator_specific_message()
		{
			var expected = "validator specific message";

			var obj = Ensure.That( "" );
			var actual = obj.GetFullErrorMessage( expected );

			Assert.IsTrue( actual.Contains( expected ) );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull()
		{
			IEnsure<string> obj = Ensure.That( "" );
			Assert.IsNotNull( obj );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull_after_if()
		{
			IEnsure<string> obj = Ensure.That( "" );
			IEnsure<string> actual = obj.If( s => true );

			Assert.IsNotNull( actual );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull_after_then_with_false_state()
		{
			var obj = Ensure.That( "" );
			var actual = obj.If( s => false ).Then( ( val ) =>
			{
				//NOP 
			} );

			Assert.IsNotNull( actual );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull_after_then_with_true_state()
		{
			var obj = Ensure.That( "" );
			var actual = obj.If( s => true ).Then( ( val ) =>
			{
				//NOP 
			} );

			Assert.IsNotNull( actual );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull_after_then_value_name_with_false_state()
		{
			var obj = Ensure.That( "" );
			var actual = obj.If( s => false ).Then( ( v, n ) =>
			{
				//NOP 
			} );

			Assert.IsNotNull( actual );
		}

		[TestMethod()]
		public void validator_inspect_isNotNull_after_then_value_name_with_true_state()
		{
			var obj = Ensure.That( "" );
			var actual = obj.If( s => true ).Then( ( v, n ) =>
			{
				//NOP 
			} );

			Assert.IsNotNull( actual );
		}

		[TestMethod()]
		public void validator_instpect_on_nullString_Throw()
		{
            Assert.ThrowsException<ArgumentNullException>( () =>
            {
                string expected = "null";
                Ensure.That( ( string )null ).If( s => s == null )
                    .Then( ( val ) =>
                    {
                        throw new System.ArgumentNullException( expected );
                    } );
            } );
		}

		[TestMethod()]
		public void validator_instpect_on_nullString_exception()
		{
            Assert.ThrowsException<ArgumentNullException>( () =>
            {
                Ensure.That( ( string )null ).IsNotNull();
            } );
		}

		[TestMethod()]
		public void validator_instpect_on_notNullString_valid()
		{
			IEnsure<string> obj = Ensure.That( "Foo" ).IsNotNull();
			Assert.IsNotNull( obj );
		}

		[TestMethod()]
		public void validator_name_using_Named_fluent_interface()
		{
			string expected = "paramName";

			IEnsure<int> obj = Ensure.That( 0 )
				.Named( expected );

			Assert.AreEqual<string>( expected, obj.Name );
		}

		[TestMethod]
		public void validator_getFullErrorMessage_using_valid_name_should_contain_name()
		{
			var expected = "foo.name";
			var actual = Ensure.That( "" ).Named( "foo.name" ).GetFullErrorMessage( "validator.specific.message" );

			Assert.IsTrue( actual.Contains( expected ) );
		}

		[TestMethod]
		public void ensureGeneric_value_normal_should_be_as_expected()
		{
			string expected = "Foo";
			var target = Ensure.That( expected );

			Assert.AreEqual( expected, target.Value );
		}

		[TestMethod]
		public void ensureGeneric_Is_using_invalid_value_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.Is( "not-empty" );
			}
			catch
			{

			}

			Assert.IsTrue( actual );
		}

		[TestMethod]
		public void ensureGeneric_IsNot_using_invalid_value_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.IsNot( String.Empty );
			}
			catch
			{

			}

			Assert.IsTrue( actual );
		}

		[TestMethod]
		public void ensureGeneric_IsTrue_using_invalid_value_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.IsTrue( s => s == "not-empty" );
			}
			catch
			{

			}

			Assert.IsTrue( actual );
		}

		[TestMethod]
		public void ensureGeneric_IsFalse_using_invalid_value_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.IsFalse( s => s == String.Empty );
			}
			catch
			{

			}

			Assert.IsTrue( actual);
		}

		[TestMethod]
		public void ensureGeneric_ThenThrow_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.If( s => s == String.Empty )
					.ThenThrow( v => new Exception() );
			}
			catch
			{

			}

			Assert.IsTrue( actual);
		}

		[TestMethod]
		public void ensureGeneric_Throw_should_invoke_preview_before_failure()
		{
			var actual = false;

			try
			{
				Ensure.That( String.Empty )
					.WithPreview( ( v, e ) => actual = true )
					.Throw( new Exception() );
			}
			catch
			{

			}

			Assert.IsTrue( actual );
		}
	}
}
