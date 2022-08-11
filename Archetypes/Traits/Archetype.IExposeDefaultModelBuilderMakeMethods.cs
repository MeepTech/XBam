

using Meep.Tech.XBam.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.XBam {

  public partial class Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// Exposes the base set of builder Make functions publicly for ease of access. 
    /// </summary>
    public interface IExposeDefaultModelBuilderMakeMethods {

      /// <summary>
      /// Exposes the base set of builder Make functions that use a builder as their parameter publicly for ease of access. 
      /// </summary>
      public interface ViaParamList : ITrait<ViaParamList> {

        string ITrait<ViaParamList>.TraitName
          => "Exposes Default Param Based Model Builder Methods";

        string ITrait<ViaParamList>.TraitDescription
          => $"Publicaly Exposes Model Builder Methods with Param based Parameters that are attached to this Archetype which are normally Protected";

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<KeyValuePair<string, object>> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(IEnumerable<(string key, object value)> @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params);

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TModelBase Make(params (string key, object value)[] @params)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(@params.AsEnumerable());

        /// <summary>
        /// Make a model from this archetype using a set of params to populate the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(@params.AsEnumerable());
      }

      /// <summary>
      /// Exposes the base set of builder Make functions that use a list of Params as their parameters publicly for ease of access. 
      /// </summary>
      public interface WithBuilder : ITrait<WithBuilder> {

        string ITrait<WithBuilder>.TraitName
          => "Exposes Default Builder Based Model Builder Methods";

        string ITrait<WithBuilder>.TraitDescription
          => $"Publicaly Exposes Model Builder Methods with Builder based Parameters that are attached to this Archetype which are normally Protected";

        /// <summary>
        /// The builder for the base model type of this archetype.
        /// </summary>
        IBuilder<TModelBase> Build(IEnumerable<KeyValuePair<string, object>> initialParams = null)
          => (this as Archetype<TModelBase, TArchetypeBase>).Build(initialParams);

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        TDesiredModel Make<TDesiredModel>()
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IBuilder, IBuilder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TModelBase Make(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(configureBuilder);

        /// <summary>
        /// Make a model by and configuring the default builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(configureBuilder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TModelBase Make(IModel<TModelBase>.Builder builder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IModel<TModelBase>.Builder builder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TModelBase Make(IBuilder builder)
          => (TModelBase)(this as Archetype<TModelBase, TArchetypeBase>).Make(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IBuilder builder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TModelBase Make(IBuilder<TModelBase> builder)
          => (this as Archetype<TModelBase, TArchetypeBase>).Make(builder);

        /// <summary>
        /// Make a model from this archetype using a fully qualified builder.
        /// </summary>
        TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>(builder);
      }

      /// <summary>
      /// Exposes the entire base set of Make functions publicly for ease of access. 
      /// </summary>
      public interface Fully : WithBuilder, ViaParamList {

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
       new TModelBase Make()
          => (this as Archetype<TModelBase, TArchetypeBase>).Make();

        /// <summary>
        /// Make a default model from this Archetype
        /// </summary>
        new TDesiredModel Make<TDesiredModel>() 
          where TDesiredModel : TModelBase
            => (this as Archetype<TModelBase, TArchetypeBase>).Make<TDesiredModel>();
      }
    }
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.Fully
  /// </summary>
  public static class ArchetypePublicFullMakers {

    /// <summary>
    /// Exposes the interface for any public model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully GetDefaultBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.Fully @this, out TDesiredModel model)
      where TDesiredModel : TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>();
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithBuilderParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicBuilderMakers {

    /// <summary>
    /// Gets the exposed model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder GetDefaultBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    public static IBuilder<TModelBase> Builder<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IEnumerable<KeyValuePair<string, object>> @params = null)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Build(@params);

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make();

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    /*public static TModelBase Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>();*/

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model)
      where TDesiredModel : class, TModelBase
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>();

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
      => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);*/

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model, Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);*/

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model, Func<IBuilder, IBuilder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    /*public static TDesiredModel Make<TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(configureBuilder);*/

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TModelBase, TArchetypeBase, TDesiredModel>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model, Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(configureBuilder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);*/

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model, IModel<TModelBase>.Builder builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a  builder.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IBuilder builder)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(builder);*/

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// This returns the same value passed to the out parameter, this is mainly just here to help you avoid entering all 3 generic types when you want to use Make in a simple way.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithBuilder @this, out TDesiredModel model, IBuilder<TModelBase> builder)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(builder);
  }

  /// <summary>
  /// Public make extensions for archeytpes that implement Archetype.IExposePublicMakers.WithParamListParameters or ..Fully
  /// </summary>
  public static class ArchetypePublicParameterListMakers {

    /// <summary>
    /// Gets the exposed model builder Make functions for this archetype.
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList GetDefaultBuilders<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this;

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// This does by default for models.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(
      this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this,
      IEnumerable<KeyValuePair<string, object>> @params
    ) where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(
      this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this,
      IEnumerable<KeyValuePair<string, object>> @params
    ) where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, out TDesiredModel model, IEnumerable<KeyValuePair<string, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, out TDesiredModel model, IEnumerable<(string key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params.AsEnumerable());*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, out TDesiredModel model, params (string key, object value)[] @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, IEnumerable<(IModel.IBuilder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<(IModel.IBuilder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, out TDesiredModel model, IEnumerable<(IModel.IBuilder.Param key, object value)> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TModelBase Make<TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, IEnumerable<KeyValuePair<IModel.IBuilder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.WithParamListParameters @this, IEnumerable<KeyValuePair<IModel.IBuilder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => @this.Make<TDesiredModel>(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel, TModelBase, TArchetypeBase>(this Archetype<TModelBase, TArchetypeBase>.IExposeDefaultModelBuilderMakeMethods.ViaParamList @this, out TDesiredModel model, IEnumerable<KeyValuePair<IModel.IBuilder.Param, object>> @params)
      where TModelBase : IModel<TModelBase>
      where TDesiredModel : TModelBase
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        => model = @this.Make<TDesiredModel>(@params);*/

  }
}