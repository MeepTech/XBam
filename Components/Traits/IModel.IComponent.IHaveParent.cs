using Newtonsoft.Json;

namespace Meep.Tech.XBam {
  public partial interface IModel {
    public partial interface IComponent {

      /// <summary>
      /// Signifies that this component type knows it's parent model
      /// </summary>
      public interface IKnowMyParentModel {

        /// <summary>
        /// Get the storage currently containing this component.
        /// </summary>
        [JsonIgnore]
        public IReadableComponentStorage Container {
          get;
          internal protected set;
        }

        /// <summary>
        /// Get the storage currently containing this component.
        /// </summary>
        [JsonIgnore]
        public IReadableComponentStorage Storage
          => Container;

        /// <summary>
        /// Get the parent model of this component.
        /// </summary>
        [JsonIgnore]
        public IModel Parent 
          => (IModel)Storage;
      }
    }
  }

  public static class IHaveParentExtensions {

    /// <summary>
    /// Get the parent model of this component.
    /// </summary>
    public static IModel GetParent(this IModel.IComponent.IKnowMyParentModel child)
      => child.Parent;

    /// <summary>
    /// Get the storage currently containing this component.
    /// </summary>
    public static IReadableComponentStorage GetContainer(this IModel.IComponent.IKnowMyParentModel child)
      => child.Storage;
  }
}