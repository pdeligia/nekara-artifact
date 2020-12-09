﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Topics.Radical.Validation;
using Topics.Radical.ComponentModel;
using Topics.Radical.Linq;

namespace Topics.Radical
{
	/// <summary>
	/// The Puzzle inversion of control container.
	/// </summary>
	public class PuzzleContainer : IPuzzleContainer
	{
		#region IDisposable Members

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="PuzzleContainer"/> is reclaimed by garbage collection.
		/// </summary>
		~PuzzleContainer()
		{
			this.Dispose( false );
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose( Boolean disposing )
		{
			if( disposing )
			{
				/*
				 * Se disposing è 'true' significa che dispose
				 * è stato invocato direttamentente dall'utente
				 * è quindi lecito accedere ai 'field' e ad 
				 * eventuali reference perchè sicuramente Finalize
				 * non è ancora stato chiamato su questi oggetti
				 */
				this.allEntries.Clear();
				this.releasedInstances.Clear();

				this.facilities.ForEach( f => f.Teardown( this ) );
				this.facilities.Clear();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose( true );
			GC.SuppressFinalize( this );
		}

		#endregion

		/// <summary>
		/// Occurs when a component is registered in this container.
		/// </summary>
		public event EventHandler<ComponentRegisteredEventArgs> ComponentRegistered;

		/// <summary>
		/// Raises the <see cref="E:ComponentRegistered"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Topics.Radical.ComponentModel.ComponentRegisteredEventArgs"/> instance containing the event data.</param>
		protected virtual void OnComponentRegistered( ComponentRegisteredEventArgs e )
		{
			var h = this.ComponentRegistered;
			if( h != null )
			{
				h( this, e );
			}
		}

		readonly IList<IContainerEntry> allEntries = new List<IContainerEntry>();
		readonly IDictionary<IContainerEntry, Object> releasedInstances = new Dictionary<IContainerEntry, Object>();
		readonly IList<IPuzzleContainerFacility> facilities = new List<IPuzzleContainerFacility>();

		/// <summary>
		/// Initializes a new instance of the <see cref="PuzzleContainer"/> class.
		/// </summary>
		public PuzzleContainer()
		{
			this.Register
			(
				EntryBuilder.For<IServiceProvider>().UsingInstance( this )
			);
		}

		/// <summary>
		/// Registers the specified entry in this container.
		/// </summary>
		/// <param name="entry">The entry to register.</param>
		/// <returns>This container instance.</returns>
		public IPuzzleContainer Register( IContainerEntry entry )
		{
			Ensure.That( entry ).Named( "entry" ).IsNotNull();

			allEntries.Add( entry );

			this.OnComponentRegistered( new ComponentRegisteredEventArgs( entry ) );

			return this;
		}

		/// <summary>
		/// Registers all the specified entries.
		/// </summary>
		/// <param name="entries">The entries to register.</param>
		/// <returns>This container instance.</returns>
		public IPuzzleContainer Register( IEnumerable<IContainerEntry> entries )
		{
			Ensure.That( entries )
				.Named( "entries" )
				.IsNotNull();

			foreach( var entry in entries )
			{
				this.Register( entry );
			}

			return this;
		}

		/// <summary>
		/// Resolves the specified service type.
		/// </summary>
		/// <typeparam name="TService">The type of the service.</typeparam>
		/// <returns>The resolved service instance.</returns>
		public TService Resolve<TService>()
		{
			return ( TService )this.Resolve( typeof( TService ) );
		}

		/// <summary>
		/// Resolves the specified service type.
		/// </summary>
		/// <param name="serviceType">The Type of the service.</param>
		/// <returns>The resolved service instance.</returns>
		public object Resolve( Type serviceType )
		{
			Ensure.That( serviceType )
				.Named( "serviceType" )
				.IsNotNull()
				.IsTrue( t => this.IsRegistered( t ) );

			var entry = this.GetEntryFor( serviceType );
			return this.ResolveEntry( entry );
		}

		/// <summary>
		/// Determines whether the given service type is registered.
		/// </summary>
		/// <typeparam name="TService">The type of the service.</typeparam>
		/// <returns>
		/// 	<c>true</c> if the given service type is registered; otherwise, <c>false</c>.
		/// </returns>
		public Boolean IsRegistered<TService>()
		{
			return this.IsRegistered( typeof( TService ) );
		}

		/// <summary>
		/// Determines whether the specified type is registered.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// 	<c>true</c> if the specified type is registered; otherwise, <c>false</c>.
		/// </returns>
		public Boolean IsRegistered( Type type )
		{
			return type != null && this.allEntries
				.Any( x =>
				{
					return x.Component == type ||
					(
						x.Service == type &&
						(
							x.Component != null ||
							x.Factory != null
						)
					);
				} );
		}

		IContainerEntry GetEntryFor( Type type )
		{
			return this.allEntries.FirstOrDefault( x =>
			{
				return x.Component == type ||
				(
					x.Service == type &&
					(
						x.Component != null ||
						x.Factory != null
					)
				);
			} );
		}

		Object ResolveEntry( IContainerEntry entry )
		{
			if( entry.Lifestyle == Lifestyle.Singleton && this.releasedInstances.ContainsKey( entry ) )
			{
				return this.releasedInstances[ entry ];
			}

			Object instance = null;

			if( entry.Factory != null )
			{
				instance = entry.Factory.DynamicInvoke();
			}
			else
			{
				var ctor = entry.Component
					.GetConstructors()
					.FirstOrDefault( x =>
					{
						return x.GetParameters()
							.All( y =>
							{
								return
								(
									entry.Parameters.ContainsKey( y.Name ) &&
									y.ParameterType.IsAssignableFrom( entry.Parameters[ y.Name ].GetType() )
								) || this.IsRegistered( y.ParameterType );
							} );
					} );

				Ensure.That( ctor )
					.Named( "ctor" )
					.WithMessage( "Cannot find any valid constructor fot type {0}.", entry.Component.FullName )
					.IsNotNull();

				var ctorParams = ctor.GetParameters();
				var pars = new Object[ ctorParams.Length ];
				for( int i = 0; i < pars.Length; i++ )
				{
					var p = ctorParams[ i ];
					if( entry.Parameters.ContainsKey( p.Name ) )
					{
						pars[ i ] = entry.Parameters[ p.Name ];
					}
					else
					{
						pars[ i ] = this.Resolve( p.ParameterType );
					}
				}

				instance = ctor.Invoke( pars );
			}

			if( instance != null )
			{
				foreach( var x in entry.Parameters )
				{
					var prop = entry.Component.GetProperty( x.Key );
					if( prop != null && prop.CanWrite && prop.PropertyType.IsAssignableFrom( x.Value.GetType() ) )
					{
						prop.SetValue( instance, x.Value, null );
					}
				}

				//qui si potrebbe ipotizzare un qualcosa di simile a castle
				//che cerca di risolvere le proprietà pubbliche esposte dal tipo
				//appena risolto.

				if( entry.Lifestyle == Lifestyle.Singleton )
				{
					this.releasedInstances.Add( entry, instance );
				}
			}

			return instance;
		}

		/// <summary>
		/// Gets the service object of the specified type.
		/// </summary>
		/// <param name="serviceType">An object that specifies the type of service object to get.</param>
		/// <returns>
		/// A service object of type <paramref name="serviceType"/>.-or- null if there is no service object of type <paramref name="serviceType"/>.
		/// </returns>
		public object GetService( Type serviceType )
		{
			if( this.IsRegistered( serviceType ) )
			{
				var entry = this.GetEntryFor( serviceType );
				return ResolveEntry( entry );
			}
			return null;
		}

		/// <summary>
		/// Adds the given facility instance to this container.
		/// </summary>
		/// <param name="facility">The facility.</param>
		/// <returns>This container instance.</returns>
		public IPuzzleContainer AddFacility( IPuzzleContainerFacility facility )
		{
			Ensure.That( facility ).Named( () => facility ).IsNotNull();

			facility.Initialize( this );
			this.facilities.Add( facility );

			return this;
		}

		/// <summary>
		/// Adds a new facility.
		/// </summary>
		/// <typeparam name="TFacility">The type of the facility.</typeparam>
		/// <returns>This container instance.</returns>
		public IPuzzleContainer AddFacility<TFacility>() where TFacility : IPuzzleContainerFacility
		{
			/*
			 * si potrebbe ipotizzare di "registrare" TFacility e poi risolverlo
			 * in modo da permettere alla facility a sua volta di avere dipendenze
			 * l'inghippo è che tipicamente una facility viene aggiunta all'inizio
			 */
			Ensure.That( typeof( TFacility ) )
				.WithMessage( "Cannot register an interface." )
				.IsFalse( t => t.IsInterface )
				.WithMessage( "Cannot register an abstract class." )
				.IsFalse( t => t.IsAbstract );

			var facility = ( IPuzzleContainerFacility )Activator.CreateInstance<TFacility>();

			return this.AddFacility( facility );
		}

		/// <summary>
		/// Gets all the installed facilities.
		/// </summary>
		/// <returns>A readonly list of all the installed facilities.</returns>
		public IEnumerable<IPuzzleContainerFacility> GetFacilities()
		{
			return this.facilities.AsReadOnly();
		}
	}

}
